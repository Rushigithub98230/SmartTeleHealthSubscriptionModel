using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Utilities;
using SmartTelehealth.Application.Constants;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service responsible for payment processing operations.
/// This service handles all payment-related operations including processing,
/// refunds, retries, payment method management, and payment analytics.
/// It focuses solely on payment execution and management.
/// 
/// Key Features:
/// - Payment processing and retry mechanisms
/// - Refund processing and management
/// - Payment method management
/// - Payment validation and status checking
/// - Payment history and analytics
/// - Special payment types (upfront, bundle)
/// - Payment reporting and exports
/// - Integration with StripeBillingService for Stripe operations
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IStripeBillingService _stripeBillingService;
    private readonly IBillingRepository _billingRepository;
    private readonly IStripeService _stripeService;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;
    private readonly ISubscriptionPaymentRepository _subscriptionPaymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFailedRefundRepository _failedRefundRepository;

    /// <summary>
    /// Initializes a new instance of the PaymentService with all required dependencies
    /// </summary>
    /// <param name="stripeBillingService">Service for Stripe-specific billing operations</param>
    /// <param name="billingRepository">Repository for billing record data access operations</param>
    /// <param name="stripeService">Service for core Stripe API operations</param>
    /// <param name="mapper">AutoMapper instance for entity-DTO mapping</param>
    /// <param name="logger">Logger instance for logging operations and errors</param>
    /// <param name="subscriptionPaymentRepository">Repository for subscription payment data access operations</param>
    /// <param name="subscriptionRepository">Repository for subscription data access operations</param>
    /// <param name="unitOfWork">Unit of work for transaction management</param>
    /// <param name="failedRefundRepository">Repository for tracking failed compensating refunds</param>
    public PaymentService(
        IStripeBillingService stripeBillingService,
        IBillingRepository billingRepository,
        IStripeService stripeService,
        IMapper mapper,
        ILogger<PaymentService> logger,
        ISubscriptionPaymentRepository subscriptionPaymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
        IFailedRefundRepository failedRefundRepository)
    {
        _stripeBillingService = stripeBillingService ?? throw new ArgumentNullException(nameof(stripeBillingService));
        _billingRepository = billingRepository ?? throw new ArgumentNullException(nameof(billingRepository));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscriptionPaymentRepository = subscriptionPaymentRepository ?? throw new ArgumentNullException(nameof(subscriptionPaymentRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _failedRefundRepository = failedRefundRepository ?? throw new ArgumentNullException(nameof(failedRefundRepository));
    }

    #region Core Payment Processing

    /// <summary>
    /// Processes a payment for a specific billing record
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to process payment for</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing payment processing results and status</returns>
    public async Task<JsonModel> ProcessPaymentAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing payment for billing record {BillingRecordId}", billingRecordId);
            
            // Validate billing record exists
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            SubscriptionPayment subscriptionPayment = null;
            
            // Create or get existing SubscriptionPayment for subscription-related billing
            // Includes: Subscription, Overage, Recurring (all subscription-related charges)
            if ((billingRecord.Type == BillingRecord.BillingType.Subscription || 
                 billingRecord.Type == BillingRecord.BillingType.Overage ||
                 billingRecord.Type == BillingRecord.BillingType.Recurring) && 
                billingRecord.SubscriptionId.HasValue)
            {
                subscriptionPayment = await GetOrCreateSubscriptionPaymentAsync(billingRecord, tokenModel);
            }
            
            // Process payment through Stripe
            var stripeResult = await _stripeBillingService.ProcessStripePaymentAsync(billingRecordId, tokenModel);
            
            // Update payment records with transaction safety
            await UpdatePaymentRecordsAsync(billingRecord, subscriptionPayment, stripeResult, tokenModel);
            
            if (stripeResult.StatusCode == 200)
            {
                _logger.LogInformation("Payment processed successfully for billing record {BillingRecordId}", billingRecordId);
            }
            else
            {
                _logger.LogWarning("Payment processing failed for billing record {BillingRecordId}: {Message}", 
                    billingRecordId, stripeResult.Message);
            }
            
            return stripeResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error processing payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Records an externally processed payment (e.g., from Stripe webhook).
    /// Creates SubscriptionPayment, updates subscription billing dates, and resets privileges.
    /// Use this when payment was already processed by external system (Stripe auto-charge).
    /// 
    /// CRITICAL: This method MUST be called from webhooks to ensure:
    /// 1. SubscriptionPayment record is created
    /// 2. LastBillingDate is updated
    /// 3. NextBillingDate is recalculated
    /// 4. Privileges are reset for new billing period
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record that was externally paid</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing payment recording results and status</returns>
    public async Task<JsonModel> RecordExternalPaymentAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Recording external payment for billing record {BillingRecordId}", billingRecordId);
            
            // Validate billing record exists
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            // Validate billing record is already marked as Paid (external payment already processed)
            if (billingRecord.Status != BillingRecord.BillingStatus.Paid)
            {
                _logger.LogWarning("Billing record {BillingRecordId} is not marked as Paid, status is {Status}", 
                    billingRecordId, billingRecord.Status);
                return new JsonModel { data = new object(), Message = "Billing record is not in Paid status", StatusCode = 400 };
            }

            SubscriptionPayment subscriptionPayment = null;
            
            // Create or get existing SubscriptionPayment for subscription-related billing
            if ((billingRecord.Type == BillingRecord.BillingType.Subscription || 
                 billingRecord.Type == BillingRecord.BillingType.Overage ||
                 billingRecord.Type == BillingRecord.BillingType.Recurring) && 
                billingRecord.SubscriptionId.HasValue)
            {
                subscriptionPayment = await GetOrCreateSubscriptionPaymentAsync(billingRecord, tokenModel);
            }
            
            // Update payment records WITHOUT processing through Stripe (already paid externally)
            await UpdatePaymentRecordsForExternalPaymentAsync(billingRecord, subscriptionPayment, tokenModel);
            
            _logger.LogInformation("External payment recorded successfully for billing record {BillingRecordId}", billingRecordId);
            
            return new JsonModel 
            { 
                data = new { billingRecordId, subscriptionPaymentId = subscriptionPayment?.Id }, 
                Message = "External payment recorded successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording external payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error recording external payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retries a failed payment for a specific billing record
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to retry payment for</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing payment retry results and status</returns>
    public async Task<JsonModel> RetryPaymentAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Retrying payment for billing record {BillingRecordId}", billingRecordId);
            
            // Delegate to StripeBillingService for Stripe-specific payment retry
            var retryResult = await _stripeBillingService.RetryStripePaymentAsync(billingRecordId, tokenModel);
            
            if (retryResult.StatusCode == 200)
            {
                _logger.LogInformation("Payment retry successful for billing record {BillingRecordId}", billingRecordId);
            }
            else
            {
                _logger.LogWarning("Payment retry failed for billing record {BillingRecordId}: {Message}", 
                    billingRecordId, retryResult.Message);
            }
            
            return retryResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error retrying payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retries a failed payment for a specific billing record
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to retry payment for</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing payment retry results and status</returns>
    public async Task<JsonModel> RetryFailedPaymentAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Retrying failed payment for billing record {BillingRecordId}", billingRecordId);
            
            // Validate billing record exists
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            // Check if payment is actually failed
            if (billingRecord.Status != BillingRecord.BillingStatus.Failed)
            {
                return new JsonModel { data = new object(), Message = "Payment is not in failed status", StatusCode = 400 };
            }

            // Delegate to StripeBillingService for Stripe-specific payment retry
            var retryResult = await _stripeBillingService.RetryStripePaymentAsync(billingRecordId, tokenModel);
            
            if (retryResult.StatusCode == 200)
            {
                _logger.LogInformation("Failed payment retry successful for billing record {BillingRecordId}", billingRecordId);
            }
            else
            {
                _logger.LogWarning("Failed payment retry failed for billing record {BillingRecordId}: {Message}", 
                    billingRecordId, retryResult.Message);
            }
            
            return retryResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error retrying failed payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes a partial payment for a specific billing record
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to process partial payment for</param>
    /// <param name="amount">The partial payment amount</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing partial payment processing results and status</returns>
    public async Task<JsonModel> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing partial payment for billing record {BillingRecordId}, amount: {Amount}", billingRecordId, amount);
            
            // Delegate to StripeBillingService for Stripe-specific partial payment processing
            var paymentResult = await _stripeBillingService.ProcessStripePartialPaymentAsync(billingRecordId, amount, tokenModel);
            
            if (paymentResult.StatusCode == 200)
            {
                _logger.LogInformation("Partial payment processed successfully for billing record {BillingRecordId}", billingRecordId);
            }
            else
            {
                _logger.LogWarning("Failed to process partial payment for billing record {BillingRecordId}: {Message}", 
                    billingRecordId, paymentResult.Message);
            }
            
            return paymentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing partial payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error processing partial payment", StatusCode = 500 };
        }
    }

    #endregion

    #region Refund Operations

    /// <summary>
    /// Processes a refund for a specific billing record
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to process refund for</param>
    /// <param name="amount">The amount to refund</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing refund processing results and status</returns>
    public async Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing refund for billing record {BillingRecordId}, amount: {Amount}", billingRecordId, amount);
            
            // Delegate to StripeBillingService for Stripe-specific refund processing
            var refundResult = await _stripeBillingService.ProcessStripeRefundAsync(billingRecordId, amount, tokenModel);
            
            if (refundResult.StatusCode == 200)
            {
                _logger.LogInformation("Refund processed successfully for billing record {BillingRecordId}", billingRecordId);
            }
            else
            {
                _logger.LogWarning("Failed to process refund for billing record {BillingRecordId}: {Message}", 
                    billingRecordId, refundResult.Message);
            }
            
            return refundResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error processing refund", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes a refund for a specific billing record with reason
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to process refund for</param>
    /// <param name="amount">The amount to refund</param>
    /// <param name="reason">The reason for the refund</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing refund processing results and status</returns>
    public async Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing refund for billing record {BillingRecordId}, amount: {Amount}, reason: {Reason}", 
                billingRecordId, amount, reason);
            
            // Delegate to StripeBillingService for Stripe-specific refund processing
            var refundResult = await _stripeBillingService.ProcessStripeRefundAsync(billingRecordId, amount, tokenModel);
            
            if (refundResult.StatusCode == 200)
            {
                _logger.LogInformation("Refund processed successfully for billing record {BillingRecordId}", billingRecordId);
            }
            else
            {
                _logger.LogWarning("Failed to process refund for billing record {BillingRecordId}: {Message}", 
                    billingRecordId, refundResult.Message);
            }
            
            return refundResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error processing refund", StatusCode = 500 };
        }
    }

    #endregion

    #region Payment Method Management

    /// <summary>
    /// Updates the payment method for a specific billing record
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to update payment method for</param>
    /// <param name="paymentMethodId">The new payment method ID</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing payment method update results and status</returns>
    public async Task<JsonModel> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Updating payment method for billing record {BillingRecordId}", billingRecordId);
            
            // Delegate to StripeBillingService for Stripe-specific payment method update
            var updateResult = await _stripeBillingService.UpdateStripePaymentMethodAsync(billingRecordId, paymentMethodId, tokenModel);
            
            if (updateResult.StatusCode == 200)
            {
                _logger.LogInformation("Payment method updated successfully for billing record {BillingRecordId}", billingRecordId);
            }
            else
            {
                _logger.LogWarning("Failed to update payment method for billing record {BillingRecordId}: {Message}", 
                    billingRecordId, updateResult.Message);
            }
            
            return updateResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error updating payment method", StatusCode = 500 };
        }
    }

    #endregion

    #region Special Payment Types

    /// <summary>
    /// Creates an upfront payment
    /// </summary>
    /// <param name="createDto">DTO containing upfront payment creation details</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing upfront payment creation results and status</returns>
    public async Task<JsonModel> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Creating upfront payment for user {UserId}", createDto.UserId);
            
            // Delegate to StripeBillingService for Stripe-specific upfront payment creation
            var paymentResult = await _stripeBillingService.CreateStripeUpfrontPaymentAsync(createDto, tokenModel);
            
            if (paymentResult.StatusCode == 200)
            {
                _logger.LogInformation("Upfront payment created successfully for user {UserId}", createDto.UserId);
            }
            else
            {
                _logger.LogWarning("Failed to create upfront payment for user {UserId}: {Message}", 
                    createDto.UserId, paymentResult.Message);
            }
            
            return paymentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating upfront payment for user {UserId}", createDto.UserId);
            return new JsonModel { data = new object(), Message = "Error creating upfront payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes a bundle payment for multiple services or subscriptions
    /// </summary>
    /// <param name="createDto">DTO containing bundle payment processing details</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing bundle payment processing results and status</returns>
    public async Task<JsonModel> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing bundle payment for user {UserId}", createDto.UserId);
            
            // Delegate to StripeBillingService for Stripe-specific bundle payment processing
            var paymentResult = await _stripeBillingService.ProcessStripeBundlePaymentAsync(createDto, tokenModel);
            
            if (paymentResult.StatusCode == 200)
            {
                _logger.LogInformation("Bundle payment processed successfully for user {UserId}", createDto.UserId);
            }
            else
            {
                _logger.LogWarning("Failed to process bundle payment for user {UserId}: {Message}", 
                    createDto.UserId, paymentResult.Message);
            }
            
            return paymentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bundle payment for user {UserId}", createDto.UserId);
            return new JsonModel { data = new object(), Message = "Error processing bundle payment", StatusCode = 500 };
        }
    }

    #endregion

    #region Payment Status & Validation

    /// <summary>
    /// Gets all pending payments
    /// </summary>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing pending payments list and status</returns>
    public async Task<JsonModel> GetPendingPaymentsAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting pending payments for user {UserId}", tokenModel.UserID);
            
            // Get pending billing records
            var pendingRecords = await _billingRepository.GetPendingBillingRecordsAsync();
            var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(pendingRecords);
            
            return new JsonModel { data = billingRecordDtos, Message = "Pending payments retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments for user {UserId}", tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "Error getting pending payments", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Checks if a payment is overdue for a specific billing record
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to check</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing overdue status and information</returns>
    public async Task<JsonModel> IsPaymentOverdueAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Checking if payment is overdue for billing record {BillingRecordId}", billingRecordId);
            
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            var isOverdue = billingRecord.DueDate.HasValue && 
                           billingRecord.DueDate.Value < DateTime.UtcNow && 
                           billingRecord.Status == BillingRecord.BillingStatus.Pending;

            return new JsonModel { data = isOverdue, Message = "Payment overdue status checked successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if payment is overdue for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error checking payment overdue status", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Validates a payment for a specific billing record
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to validate</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing validation results and status</returns>
    public async Task<JsonModel> ValidatePaymentAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Validating payment for billing record {BillingRecordId}", billingRecordId);
            
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            // Validate payment method if exists
            if (!string.IsNullOrEmpty(billingRecord.PaymentMethod))
            {
                var isValid = await _stripeService.ValidatePaymentMethodAsync(billingRecord.PaymentMethod, tokenModel);
                return new JsonModel { data = isValid, Message = "Payment validation completed", StatusCode = 200 };
            }

            return new JsonModel { data = false, Message = "No payment method found", StatusCode = 400 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error validating payment", StatusCode = 500 };
        }
    }

    #endregion

    #region Payment History & Analytics

    /// <summary>
    /// Gets payment history for a specific user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="startDate">Optional start date for filtering</param>
    /// <param name="endDate">Optional end date for filtering</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing payment history and status</returns>
    public async Task<JsonModel> GetPaymentHistoryAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting payment history for user {UserId}", userId);
            
            var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
            
            if (startDate.HasValue || endDate.HasValue)
            {
                billingRecords = billingRecords.Where(br => 
                    (!startDate.HasValue || br.CreatedDate >= startDate.Value) &&
                    (!endDate.HasValue || br.CreatedDate <= endDate.Value));
            }

            var paymentHistory = billingRecords.Select(br => new PaymentHistoryDto
            {
                Id = br.Id,
                UserId = br.UserId,
                Amount = br.Amount,
                Status = br.Status.ToString(),
                PaymentDate = br.PaidAt.HasValue ? br.PaidAt.Value : (br.CreatedDate ?? DateTime.UtcNow),
                Description = br.Description ?? string.Empty
            }).ToList();

            return new JsonModel { data = paymentHistory, Message = "Payment history retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error getting payment history", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Gets payment history for a specific user (string userId overload)
    /// </summary>
    /// <param name="userId">The user ID as string</param>
    /// <param name="startDate">Optional start date for filtering</param>
    /// <param name="endDate">Optional end date for filtering</param>
    /// <returns>IEnumerable of PaymentHistoryDto</returns>
    public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            if (!int.TryParse(userId, out var userIdInt))
            {
                return Enumerable.Empty<PaymentHistoryDto>();
            }

            var result = await GetPaymentHistoryAsync(userIdInt, startDate, endDate, new TokenModel());
            
            if (result.data is IEnumerable<PaymentHistoryDto> history)
            {
                return history;
            }

            return Enumerable.Empty<PaymentHistoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
            return Enumerable.Empty<PaymentHistoryDto>();
        }
    }

    /// <summary>
    /// Gets payment analytics for a date range
    /// </summary>
    /// <param name="startDate">Optional start date for analytics</param>
    /// <param name="endDate">Optional end date for analytics</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing payment analytics and status</returns>
    public async Task<JsonModel> GetPaymentAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting payment analytics for date range {StartDate} to {EndDate}", startDate, endDate);
            
            var billingRecords = await _billingRepository.GetAllAsync();
            
            if (startDate.HasValue || endDate.HasValue)
            {
                billingRecords = billingRecords.Where(br => 
                    (!startDate.HasValue || br.CreatedDate >= startDate.Value) &&
                    (!endDate.HasValue || br.CreatedDate <= endDate.Value));
            }

            var analytics = new PaymentAnalyticsDto
            {
                TotalPayments = billingRecords.Count(),
                TotalSpent = billingRecords.Sum(br => br.Amount),
                SuccessfulPayments = billingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                FailedPayments = billingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Failed),
                TotalTransactions = billingRecords.Count(),
                SuccessfulTransactions = billingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                FailedTransactions = billingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Failed)
            };

            return new JsonModel { data = analytics, Message = "Payment analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment analytics");
            return new JsonModel { data = new object(), Message = "Error getting payment analytics", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Gets payment analytics for a specific user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="startDate">Optional start date for analytics</param>
    /// <param name="endDate">Optional end date for analytics</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing user payment analytics and status</returns>
    public async Task<JsonModel> GetPaymentAnalyticsAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting payment analytics for user {UserId}", userId);
            
            var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
            
            if (startDate.HasValue || endDate.HasValue)
            {
                billingRecords = billingRecords.Where(br => 
                    (!startDate.HasValue || br.CreatedDate >= startDate.Value) &&
                    (!endDate.HasValue || br.CreatedDate <= endDate.Value));
            }

            var analytics = new PaymentAnalyticsDto
            {
                TotalPayments = billingRecords.Count(),
                TotalSpent = billingRecords.Sum(br => br.Amount),
                SuccessfulPayments = billingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                FailedPayments = billingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Failed),
                TotalTransactions = billingRecords.Count(),
                SuccessfulTransactions = billingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                FailedTransactions = billingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Failed)
            };

            return new JsonModel { data = analytics, Message = "User payment analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment analytics for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error getting user payment analytics", StatusCode = 500 };
        }
    }

    #endregion

    #region Payment Reports

    /// <summary>
    /// Exports payment history for a specific user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="startDate">Optional start date for filtering</param>
    /// <param name="endDate">Optional end date for filtering</param>
    /// <param name="format">Export format (PDF, CSV, Excel)</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing export results and status</returns>
    public async Task<JsonModel> ExportPaymentHistoryAsync(int userId, DateTime? startDate, DateTime? endDate, string format, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Exporting payment history for user {UserId} in format {Format}", userId, format);
            
            var result = await GetPaymentHistoryAsync(userId, startDate, endDate, tokenModel);
            
            if (result.StatusCode != 200)
            {
                return result;
            }

            // TODO: Implement actual export logic based on format
            var exportData = new { Message = $"Payment history exported in {format} format", Data = result.data };
            
            return new JsonModel { data = exportData, Message = "Payment history exported successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting payment history for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error exporting payment history", StatusCode = 500 };
        }
    }

    #endregion

    #region Invoice Management

    /// <summary>
    /// Creates an invoice
    /// </summary>
    /// <param name="createDto">DTO containing invoice creation details</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing invoice creation results and status</returns>
    public async Task<JsonModel> CreateInvoiceAsync(CreateInvoiceDto createDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Creating invoice for user {UserId}", createDto.UserId);
            
            // Delegate to StripeBillingService for Stripe-specific invoice creation
            var invoiceResult = await _stripeBillingService.CreateStripeInvoiceAsync(createDto, tokenModel);
            
            if (invoiceResult.StatusCode == 200)
            {
                _logger.LogInformation("Invoice created successfully for user {UserId}", createDto.UserId);
            }
            else
            {
                _logger.LogWarning("Failed to create invoice for user {UserId}: {Message}", 
                    createDto.UserId, invoiceResult.Message);
            }
            
            return invoiceResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for user {UserId}", createDto.UserId);
            return new JsonModel { data = new object(), Message = "Error creating invoice", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Generates a PDF invoice for a specific billing record
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record to generate PDF invoice for</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing PDF generation results and status</returns>
    public async Task<JsonModel> GenerateInvoicePdfAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Generating invoice PDF for billing record {BillingRecordId}", billingRecordId);
            
            // Delegate to StripeBillingService for Stripe-specific PDF generation
            var pdfResult = await _stripeBillingService.GenerateStripeInvoicePdfAsync(billingRecordId, tokenModel);
            
            if (pdfResult.StatusCode == 200)
            {
                _logger.LogInformation("Invoice PDF generated successfully for billing record {BillingRecordId}", billingRecordId);
            }
            else
            {
                _logger.LogWarning("Failed to generate invoice PDF for billing record {BillingRecordId}: {Message}", 
                    billingRecordId, pdfResult.Message);
            }
            
            return pdfResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice PDF for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error generating invoice PDF", StatusCode = 500 };
        }
    }

    #endregion

    #region Recurring Billing

    /// <summary>
    /// Creates recurring billing
    /// </summary>
    /// <param name="createDto">DTO containing recurring billing creation details</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing recurring billing creation results and status</returns>
    public async Task<JsonModel> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Creating recurring billing for user {UserId}", createDto.UserId);
            
            // Delegate to StripeBillingService for Stripe-specific recurring billing creation
            var recurringResult = await _stripeBillingService.CreateStripeRecurringBillingAsync(createDto, tokenModel);
            
            if (recurringResult.StatusCode == 200)
            {
                _logger.LogInformation("Recurring billing created successfully for user {UserId}", createDto.UserId);
            }
            else
            {
                _logger.LogWarning("Failed to create recurring billing for user {UserId}: {Message}", 
                    createDto.UserId, recurringResult.Message);
            }
            
            return recurringResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recurring billing for user {UserId}", createDto.UserId);
            return new JsonModel { data = new object(), Message = "Error creating recurring billing", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes recurring payment for a specific subscription
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to process recurring payment for</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing recurring payment processing results and status</returns>
    public async Task<JsonModel> ProcessRecurringPaymentAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing recurring payment for subscription {SubscriptionId}", subscriptionId);
            
            // Delegate to StripeBillingService for Stripe-specific recurring payment processing
            var paymentResult = await _stripeBillingService.ProcessStripeRecurringPaymentAsync(subscriptionId, tokenModel);
            
            if (paymentResult.StatusCode == 200)
            {
                _logger.LogInformation("Recurring payment processed successfully for subscription {SubscriptionId}", subscriptionId);
            }
            else
            {
                _logger.LogWarning("Failed to process recurring payment for subscription {SubscriptionId}: {Message}", 
                    subscriptionId, paymentResult.Message);
            }
            
            return paymentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing recurring payment for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Error processing recurring payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Cancels recurring billing for a specific subscription
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to cancel recurring billing for</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing recurring billing cancellation results and status</returns>
    public async Task<JsonModel> CancelRecurringBillingAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Cancelling recurring billing for subscription {SubscriptionId}", subscriptionId);
            
            // Delegate to StripeBillingService for Stripe-specific recurring billing cancellation
            var cancellationResult = await _stripeBillingService.CancelStripeRecurringBillingAsync(subscriptionId, tokenModel);
            
            if (cancellationResult.StatusCode == 200)
            {
                _logger.LogInformation("Recurring billing cancelled successfully for subscription {SubscriptionId}", subscriptionId);
            }
            else
            {
                _logger.LogWarning("Failed to cancel recurring billing for subscription {SubscriptionId}: {Message}", 
                    subscriptionId, cancellationResult.Message);
            }
            
            return cancellationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling recurring billing for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Error cancelling recurring billing", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Gets payment schedule for a specific subscription
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to get payment schedule for</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing payment schedule information</returns>
    public async Task<JsonModel> GetPaymentScheduleAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting payment schedule for subscription {SubscriptionId}", subscriptionId);
            
            // Get subscription details from billing repository
            var billingRecords = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
            var subscription = billingRecords.FirstOrDefault()?.Subscription;
            
            if (subscription == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            }

            var paymentSchedule = new
            {
                SubscriptionId = subscriptionId,
                NextPaymentDate = subscription.NextBillingDate,
                BillingCycle = subscription.BillingCycle?.Name ?? string.Empty,
                Amount = subscription.Amount,
                Currency = subscription.Currency,
                Status = subscription.Status
            };
            
            _logger.LogInformation("Payment schedule retrieved successfully for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = paymentSchedule, Message = "Payment schedule retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment schedule for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Error retrieving payment schedule", StatusCode = 500 };
        }
    }

    #endregion

    #region Payment Method Management (SRP Refactoring - Moved from SubscriptionService)

    /// <summary>
    /// Retrieves all payment methods for a specific user
    /// SRP Refactoring: Moved from SubscriptionService to PaymentService where it belongs
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose payment methods to retrieve</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the user's payment methods or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admins can retrieve any user's payment methods
    /// - Users can only retrieve their own payment methods
    /// </remarks>
    public async Task<JsonModel> GetPaymentMethodsAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - user can only access their own payment methods unless admin
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.UserID != userId)
            {
                _logger.LogWarning("Access denied: User {RequestingUserId} attempted to access payment methods for user {TargetUserId}", 
                    tokenModel.UserID, userId);
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            _logger.LogInformation("Retrieving payment methods for user {UserId}", userId);

            // Retrieve payment methods from Stripe service
            var methods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString(), tokenModel);
            
            _logger.LogInformation("Successfully retrieved payment methods for user {UserId}", userId);
            return new JsonModel { data = methods, Message = "Payment methods retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment methods for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error retrieving payment methods", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Adds a payment method to a user's account
    /// SRP Refactoring: Moved from SubscriptionService to PaymentService where it belongs
    /// </summary>
    /// <param name="userId">The unique identifier of the user to add the payment method to</param>
    /// <param name="paymentMethodId">The Stripe payment method ID to add</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the added payment method or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admins can add payment methods to any user's account
    /// - Users can only add payment methods to their own account
    /// </remarks>
    public async Task<JsonModel> AddPaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - user can only add payment methods to their own account unless admin
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.UserID != userId)
            {
                _logger.LogWarning("Access denied: User {RequestingUserId} attempted to add payment method for user {TargetUserId}", 
                    tokenModel.UserID, userId);
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            _logger.LogInformation("Adding payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);

            // Add payment method to user's Stripe customer account
            var methodId = await _stripeService.AddPaymentMethodAsync(userId.ToString(), paymentMethodId, tokenModel);
            var method = new PaymentMethodDto { Id = methodId };
            
            _logger.LogInformation("Successfully added payment method for user {UserId}", userId);
            return new JsonModel { data = method, Message = "Payment method added successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);
            return new JsonModel { data = new object(), Message = "Error adding payment method", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Adds a payment method using DTO (for webhook processing)
    /// </summary>
    /// <param name="dto">Payment method data</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the result</returns>
    public async Task<JsonModel> AddPaymentMethodAsync(AddPaymentMethodDto dto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Adding payment method {PaymentMethodId} for user {UserId}", dto.PaymentMethodId, dto.UserId);

            // Add payment method to user's Stripe customer account
            var methodId = await _stripeService.AddPaymentMethodAsync(dto.UserId.ToString(), dto.PaymentMethodId, tokenModel);
            var method = new PaymentMethodDto { Id = methodId };
            
            _logger.LogInformation("Successfully added payment method for user {UserId}", dto.UserId);
            return new JsonModel { data = method, Message = "Payment method added successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment method {PaymentMethodId} for user {UserId}", dto.PaymentMethodId, dto.UserId);
            return new JsonModel { data = new object(), Message = "Error adding payment method", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Gets user payment methods (alias for GetPaymentMethodsAsync)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing payment methods</returns>
    public async Task<JsonModel> GetUserPaymentMethodsAsync(int userId, TokenModel tokenModel)
    {
        return await GetPaymentMethodsAsync(userId, tokenModel);
    }

    /// <summary>
    /// Sets a payment method as default
    /// </summary>
    /// <param name="paymentMethodId">Payment method ID</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the result</returns>
    public async Task<JsonModel> SetDefaultPaymentMethodAsync(string paymentMethodId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Setting payment method {PaymentMethodId} as default for user {UserId}", paymentMethodId, tokenModel.UserID);

            // Set payment method as default in Stripe
            await _stripeService.SetDefaultPaymentMethodAsync(tokenModel.UserID.ToString(), paymentMethodId, tokenModel);
            
            _logger.LogInformation("Successfully set payment method {PaymentMethodId} as default for user {UserId}", paymentMethodId, tokenModel.UserID);
            return new JsonModel { data = true, Message = "Payment method set as default successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting payment method {PaymentMethodId} as default for user {UserId}", paymentMethodId, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "Error setting payment method as default", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Gets a payment method by Stripe ID
    /// </summary>
    /// <param name="stripePaymentMethodId">Stripe payment method ID</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the payment method</returns>
    public async Task<JsonModel> GetPaymentMethodByStripeIdAsync(string stripePaymentMethodId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting payment method {PaymentMethodId} for user {UserId}", stripePaymentMethodId, tokenModel.UserID);

            // First get the user's Stripe customer ID from subscription
            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(tokenModel.UserID);
            var subscription = subscriptions?.FirstOrDefault();
            if (subscription?.User?.StripeCustomerId == null)
            {
                return new JsonModel { data = new object(), Message = "User not found or no Stripe customer ID", StatusCode = 404 };
            }

            // Get payment method from Stripe using available method
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(subscription.User.StripeCustomerId, tokenModel);
            var paymentMethod = paymentMethods?.FirstOrDefault(pm => pm.Id == stripePaymentMethodId);
            
            if (paymentMethod == null)
            {
                return new JsonModel { data = new object(), Message = "Payment method not found", StatusCode = 404 };
            }

            _logger.LogInformation("Successfully retrieved payment method {PaymentMethodId} for user {UserId}", stripePaymentMethodId, tokenModel.UserID);
            return new JsonModel { data = paymentMethod, Message = "Payment method retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment method {PaymentMethodId} for user {UserId}", stripePaymentMethodId, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "Error retrieving payment method", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Deletes a payment method from user's account
    /// CRITICAL: Prevents deletion if payment method is used in active subscriptions
    /// </summary>
    /// <param name="paymentMethodId">The Stripe payment method ID to delete</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing deletion result and status</returns>
    public async Task<JsonModel> DeletePaymentMethodAsync(string paymentMethodId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Deleting payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);

            // First get the user's Stripe customer ID
            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(tokenModel.UserID);
            var subscription = subscriptions?.FirstOrDefault();
            if (subscription?.User?.StripeCustomerId == null)
            {
                return new JsonModel { data = new object(), Message = "User not found or no Stripe customer ID", StatusCode = 404 };
            }

            // Check if payment method is used in any active subscriptions
            var activeSubscriptions = subscriptions?.Where(s => s.Status == Subscription.SubscriptionStatuses.Active && 
                                                               s.PaymentMethodId == paymentMethodId).ToList();
            
            if (activeSubscriptions?.Any() == true)
            {
                _logger.LogWarning("Cannot delete payment method {PaymentMethodId} - used in {Count} active subscriptions", 
                    paymentMethodId, activeSubscriptions.Count);
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = $"Cannot delete payment method. It is currently used in {activeSubscriptions.Count} active subscription(s). Please update those subscriptions first.", 
                    StatusCode = 400 
                };
            }

            // Check if this is the user's only payment method
            var allPaymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(subscription.User.StripeCustomerId, tokenModel);
            if (allPaymentMethods?.Count() <= 1)
            {
                _logger.LogWarning("Cannot delete payment method {PaymentMethodId} - it is the only payment method for user {UserId}", 
                    paymentMethodId, tokenModel.UserID);
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "Cannot delete payment method. It is your only payment method. Please add another payment method first.", 
                    StatusCode = 400 
                };
            }

            // Remove payment method from Stripe
            var removeResult = await _stripeService.RemovePaymentMethodAsync(subscription.User.StripeCustomerId, paymentMethodId, tokenModel);
            
            if (removeResult)
            {
                _logger.LogInformation("Successfully deleted payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);
                return new JsonModel { data = true, Message = "Payment method deleted successfully", StatusCode = 200 };
            }
            else
            {
                _logger.LogWarning("Failed to delete payment method {PaymentMethodId} from Stripe for user {UserId}", paymentMethodId, tokenModel.UserID);
                return new JsonModel { data = new object(), Message = "Failed to delete payment method from Stripe", StatusCode = 500 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "Error deleting payment method", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Updates payment method details (expiry, billing address, etc.)
    /// </summary>
    /// <param name="paymentMethodId">The Stripe payment method ID to update</param>
    /// <param name="dto">Updated payment method details</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing update result and status</returns>
    public async Task<JsonModel> UpdatePaymentMethodDetailsAsync(string paymentMethodId, UpdatePaymentMethodDto dto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Updating payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);

            // Validate input
            if (string.IsNullOrEmpty(paymentMethodId))
            {
                return new JsonModel { data = new object(), Message = "Payment method ID is required", StatusCode = 400 };
            }

            if (dto == null)
            {
                return new JsonModel { data = new object(), Message = "Update data is required", StatusCode = 400 };
            }

            // First get the user's Stripe customer ID
            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(tokenModel.UserID);
            var subscription = subscriptions?.FirstOrDefault();
            if (subscription?.User?.StripeCustomerId == null)
            {
                return new JsonModel { data = new object(), Message = "User not found or no Stripe customer ID", StatusCode = 404 };
            }

            // Verify payment method belongs to user
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(subscription.User.StripeCustomerId, tokenModel);
            var existingMethod = paymentMethods?.FirstOrDefault(pm => pm.Id == paymentMethodId);
            
            if (existingMethod == null)
            {
                return new JsonModel { data = new object(), Message = "Payment method not found or does not belong to user", StatusCode = 404 };
            }

            // Update payment method in Stripe
            var updateResult = await _stripeService.UpdatePaymentMethodAsync(subscription.User.StripeCustomerId, paymentMethodId, tokenModel);
            
            if (updateResult)
            {
                _logger.LogInformation("Successfully updated payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);
                return new JsonModel { data = true, Message = "Payment method updated successfully", StatusCode = 200 };
            }
            else
            {
                _logger.LogWarning("Failed to update payment method {PaymentMethodId} in Stripe for user {UserId}", paymentMethodId, tokenModel.UserID);
                return new JsonModel { data = new object(), Message = "Failed to update payment method in Stripe", StatusCode = 500 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "Error updating payment method", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Validates a payment method before use
    /// </summary>
    /// <param name="paymentMethodId">The Stripe payment method ID to validate</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing validation result and status</returns>
    public async Task<JsonModel> ValidatePaymentMethodAsync(string paymentMethodId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Validating payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);

            // Validate input
            if (string.IsNullOrEmpty(paymentMethodId))
            {
                return new JsonModel { data = new object(), Message = "Payment method ID is required", StatusCode = 400 };
            }

            // First get the user's Stripe customer ID
            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(tokenModel.UserID);
            var subscription = subscriptions?.FirstOrDefault();
            if (subscription?.User?.StripeCustomerId == null)
            {
                return new JsonModel { data = new object(), Message = "User not found or no Stripe customer ID", StatusCode = 404 };
            }

            // Validate payment method in Stripe
            var validationResult = await _stripeService.ValidatePaymentMethodAsync(paymentMethodId, tokenModel);
            
            if (validationResult)
            {
                // Verify payment method belongs to user
                var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(subscription.User.StripeCustomerId, tokenModel);
                var userMethod = paymentMethods?.FirstOrDefault(pm => pm.Id == paymentMethodId);
                
                if (userMethod != null)
                {
                    _logger.LogInformation("Payment method {PaymentMethodId} is valid for user {UserId}", paymentMethodId, tokenModel.UserID);
                    return new JsonModel { data = new { isValid = true, paymentMethod = userMethod }, Message = "Payment method is valid", StatusCode = 200 };
                }
                else
                {
                    _logger.LogWarning("Payment method {PaymentMethodId} does not belong to user {UserId}", paymentMethodId, tokenModel.UserID);
                    return new JsonModel { data = new { isValid = false }, Message = "Payment method does not belong to user", StatusCode = 403 };
                }
            }
            else
            {
                _logger.LogWarning("Payment method {PaymentMethodId} validation failed for user {UserId}", paymentMethodId, tokenModel.UserID);
                return new JsonModel { data = new { isValid = false }, Message = "Payment method validation failed", StatusCode = 400 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "Error validating payment method", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Gets detailed information about a specific payment method
    /// </summary>
    /// <param name="paymentMethodId">The Stripe payment method ID</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing detailed payment method information</returns>
    public async Task<JsonModel> GetPaymentMethodDetailsAsync(string paymentMethodId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting detailed information for payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);

            // Validate input
            if (string.IsNullOrEmpty(paymentMethodId))
            {
                return new JsonModel { data = new object(), Message = "Payment method ID is required", StatusCode = 400 };
            }

            // First get the user's Stripe customer ID
            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(tokenModel.UserID);
            var subscription = subscriptions?.FirstOrDefault();
            if (subscription?.User?.StripeCustomerId == null)
            {
                return new JsonModel { data = new object(), Message = "User not found or no Stripe customer ID", StatusCode = 404 };
            }

            // Get payment method details from Stripe
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(subscription.User.StripeCustomerId, tokenModel);
            var paymentMethod = paymentMethods?.FirstOrDefault(pm => pm.Id == paymentMethodId);
            
            if (paymentMethod == null)
            {
                return new JsonModel { data = new object(), Message = "Payment method not found", StatusCode = 404 };
            }

            // Get usage statistics
            var usageStats = new
            {
                IsUsedInActiveSubscriptions = subscriptions?.Any(s => s.Status == Subscription.SubscriptionStatuses.Active && s.PaymentMethodId == paymentMethodId) ?? false,
                ActiveSubscriptionCount = subscriptions?.Count(s => s.Status == Subscription.SubscriptionStatuses.Active && s.PaymentMethodId == paymentMethodId) ?? 0,
                TotalSubscriptionCount = subscriptions?.Count(s => s.PaymentMethodId == paymentMethodId) ?? 0,
                IsDefault = subscription?.PaymentMethodId == paymentMethodId
            };

            var detailedInfo = new
            {
                PaymentMethod = paymentMethod,
                UsageStatistics = usageStats,
                LastUsed = subscriptions?.Where(s => s.PaymentMethodId == paymentMethodId)
                                         .OrderByDescending(s => s.UpdatedDate)
                                         .FirstOrDefault()?.UpdatedDate
            };

            _logger.LogInformation("Successfully retrieved detailed information for payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);
            return new JsonModel { data = detailedInfo, Message = "Payment method details retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detailed information for payment method {PaymentMethodId} for user {UserId}", paymentMethodId, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "Error retrieving payment method details", StatusCode = 500 };
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets or creates a SubscriptionPayment for the given billing record to prevent duplicates
    /// </summary>
    private async Task<SubscriptionPayment> GetOrCreateSubscriptionPaymentAsync(BillingRecord billingRecord, TokenModel tokenModel)
    {
        // Check if SubscriptionPayment already exists for this billing record
        var existingPayment = await _subscriptionPaymentRepository.GetByBillingRecordIdAsync(billingRecord.Id);
        if (existingPayment != null)
        {
            _logger.LogInformation("Found existing SubscriptionPayment {PaymentId} for billing record {BillingRecordId}", 
                existingPayment.Id, billingRecord.Id);
            return existingPayment;
        }

        // Get subscription details
        var subscription = await _subscriptionRepository.GetByIdAsync(billingRecord.SubscriptionId.Value);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {billingRecord.SubscriptionId} not found for billing record {billingRecord.Id}");
        }

        // Calculate billing period
        var (billingPeriodStart, billingPeriodEnd) = CalculateBillingPeriod(subscription, billingRecord);

        // Determine payment type based on billing record type
        var paymentType = MapBillingTypeToPaymentType(billingRecord.Type);
        
        // Create description based on billing type
        var description = billingRecord.Type switch
        {
            BillingRecord.BillingType.Overage => $"Overage charges for {subscription.SubscriptionPlan?.Name ?? "subscription"}",
            BillingRecord.BillingType.Recurring => $"Recurring payment for {subscription.SubscriptionPlan?.Name ?? "subscription"}",
            _ => $"Subscription payment for {subscription.SubscriptionPlan?.Name ?? "Unknown Plan"}"
        };

        // Create new SubscriptionPayment
        var subscriptionPayment = new SubscriptionPayment
        {
            Id = Guid.NewGuid(),
            SubscriptionId = billingRecord.SubscriptionId.Value,
            BillingRecordId = billingRecord.Id,
            CurrencyId = billingRecord.CurrencyId,
            Amount = billingRecord.Amount,
            TaxAmount = billingRecord.TaxAmount,
            NetAmount = billingRecord.TotalAmount,
            Description = description,
            Status = SubscriptionPayment.PaymentStatus.Pending,
            Type = paymentType,
            DueDate = billingRecord.DueDate ?? DateTime.UtcNow.AddDays(30),
            BillingPeriodStart = billingPeriodStart,
            BillingPeriodEnd = billingPeriodEnd,
            AttemptCount = 0,
            CreatedBy = tokenModel.UserID,
            CreatedDate = DateTime.UtcNow
        };

        var createdPayment = await _subscriptionPaymentRepository.CreateAsync(subscriptionPayment);
        _logger.LogInformation("Created new SubscriptionPayment {PaymentId} for billing record {BillingRecordId}", 
            createdPayment.Id, billingRecord.Id);

        return createdPayment;
    }

    /// <summary>
    /// Calculates billing period for a subscription payment.
    /// REFACTORED (PHASE 1): Now delegates to centralized BillingCycleCalculator.
    /// FIXED: Now correctly handles all billing cycles (monthly, quarterly, annual).
    /// </summary>
    private (DateTime start, DateTime end) CalculateBillingPeriod(Subscription subscription, BillingRecord billingRecord)
    {
        try
        {
            // Determine if this is first payment (no LastBillingDate)
            bool isFirstPayment = !subscription.LastBillingDate.HasValue;
            
            // Delegate to centralized calculator
            var (start, end) = BillingCycleCalculator.CalculateBillingPeriod(subscription, isFirstPayment);
            
            _logger.LogDebug(
                "{PaymentType} billing period for subscription {SubscriptionId}: {Start:yyyy-MM-dd} to {End:yyyy-MM-dd}",
                isFirstPayment ? "First" : "Renewal",
                subscription.Id,
                start,
                end);
            
            return (start, end);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating billing period for subscription {SubscriptionId}", subscription.Id);
            // Safe fallback
            var start = subscription.LastBillingDate ?? subscription.StartDate;
            var end = start.AddMonths(1).AddDays(-1);
            return (start, end);
        }
    }
    
    /// <summary>
    /// Calculates the end date of a billing period based on start date and billing cycle.
    /// REFACTORED: Now uses centralized BillingCycleCalculator for consistency.
    /// </summary>
    private DateTime CalculateEndDateForCycle(DateTime startDate, MasterBillingCycle billingCycle)
    {
        try
        {
            // Use centralized calculator
            return BillingCycleCalculator.CalculateEndDateForCycle(startDate, billingCycle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating end date for billing cycle {CycleName}", billingCycle?.Name);
            return startDate.AddMonths(1).AddDays(-1); // Safe fallback
        }
    }

    /// <summary>
    /// Updates payment records with transaction safety
    /// </summary>
    private async Task UpdatePaymentRecordsAsync(BillingRecord billingRecord, SubscriptionPayment subscriptionPayment, 
        JsonModel stripeResult, TokenModel tokenModel)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var isSuccess = stripeResult.StatusCode == 200;
            
            if (subscriptionPayment != null)
            {
                // Update SubscriptionPayment
                subscriptionPayment.AttemptCount++;
                subscriptionPayment.UpdatedBy = tokenModel.UserID;
                subscriptionPayment.UpdatedDate = DateTime.UtcNow;

                if (isSuccess)
                {
                    subscriptionPayment.Status = SubscriptionPayment.PaymentStatus.Succeeded;
                    subscriptionPayment.PaidAt = DateTime.UtcNow;
                    subscriptionPayment.StripePaymentIntentId = billingRecord.StripePaymentIntentId;
                    subscriptionPayment.StripeInvoiceId = billingRecord.StripeInvoiceId;
                    // ReceiptUrl property doesn't exist in BillingRecord - removed
                }
                else
                {
                    subscriptionPayment.Status = SubscriptionPayment.PaymentStatus.Failed;
                    subscriptionPayment.FailedAt = DateTime.UtcNow;
                    subscriptionPayment.FailureReason = stripeResult.Message;
                    subscriptionPayment.NextRetryAt = CalculateNextRetry(subscriptionPayment.AttemptCount);
                }

                await _subscriptionPaymentRepository.UpdateAsync(subscriptionPayment);
            }

            // Update BillingRecord status
            billingRecord.Status = isSuccess ? BillingRecord.BillingStatus.Paid : BillingRecord.BillingStatus.Failed;
            billingRecord.UpdatedBy = tokenModel.UserID;
            billingRecord.UpdatedDate = DateTime.UtcNow;

            if (isSuccess)
            {
                billingRecord.PaidAt = DateTime.UtcNow;
            }

            await _billingRepository.UpdateAsync(billingRecord);

            // Update subscription LastBillingDate if payment succeeded
            if (isSuccess && subscriptionPayment != null)
            {
                var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionPayment.SubscriptionId);
                if (subscription != null)
                {
                    // FIXED: LastBillingDate should be the START of the billing period, not the END
                    subscription.LastBillingDate = subscriptionPayment.BillingPeriodStart;
                    
                    // FIXED: Calculate next billing date using proper billing cycle logic
                    subscription.NextBillingDate = CalculateNextBillingDate(subscription);
                    
                    // Update last payment date for tracking
                    subscription.LastPaymentDate = DateTime.UtcNow;
                    subscription.FailedPaymentAttempts = 0; // Reset failed attempts on successful payment
                    
                    subscription.UpdatedBy = tokenModel.UserID;
                    subscription.UpdatedDate = DateTime.UtcNow;
                    await _subscriptionRepository.UpdateAsync(subscription);
                    
                    // Reset privilege usage for new billing period
                    await ResetPrivilegesForNewBillingPeriodAsync(subscription, tokenModel);
                }
            }

            await _unitOfWork.CommitTransactionAsync();
            _logger.LogInformation("Successfully updated payment records for billing record {BillingRecordId}", billingRecord.Id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating payment records for billing record {BillingRecordId}", billingRecord.Id);
            
            // CRITICAL FIX (Issue #10): If Stripe payment succeeded but database update failed,
            // issue compensating refund to maintain Stripe-Database consistency
            // This prevents users from being charged without a database record
            if (stripeResult.StatusCode == 200 && !string.IsNullOrEmpty(billingRecord.StripePaymentIntentId))
            {
                await IssueCompensatingRefundAsync(billingRecord, tokenModel);
            }
            
            throw;
        }
    }

    /// <summary>
    /// Updates payment records for externally processed payments (e.g., Stripe webhooks).
    /// Similar to UpdatePaymentRecordsAsync but skips Stripe processing.
    /// </summary>
    private async Task UpdatePaymentRecordsForExternalPaymentAsync(BillingRecord billingRecord, 
        SubscriptionPayment subscriptionPayment, TokenModel tokenModel)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (subscriptionPayment != null)
            {
                // Update SubscriptionPayment - mark as succeeded since external payment was already processed
                subscriptionPayment.Status = SubscriptionPayment.PaymentStatus.Succeeded;
                subscriptionPayment.PaidAt = billingRecord.PaidAt ?? DateTime.UtcNow;
                subscriptionPayment.StripePaymentIntentId = billingRecord.StripePaymentIntentId;
                subscriptionPayment.StripeInvoiceId = billingRecord.StripeInvoiceId;
                subscriptionPayment.UpdatedBy = tokenModel.UserID;
                subscriptionPayment.UpdatedDate = DateTime.UtcNow;

                await _subscriptionPaymentRepository.UpdateAsync(subscriptionPayment);
            }

            // BillingRecord is already marked as Paid by webhook, just ensure consistency
            billingRecord.UpdatedBy = tokenModel.UserID;
            billingRecord.UpdatedDate = DateTime.UtcNow;
            await _billingRepository.UpdateAsync(billingRecord);

            // Update subscription LastBillingDate and reset privileges
            if (subscriptionPayment != null)
            {
                var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionPayment.SubscriptionId);
                if (subscription != null)
                {
                    // Update LastBillingDate to the START of the billing period
                    subscription.LastBillingDate = subscriptionPayment.BillingPeriodStart;
                    
                    // Calculate next billing date using proper billing cycle logic
                    subscription.NextBillingDate = CalculateNextBillingDate(subscription);
                    
                    // Update last payment date for tracking
                    subscription.LastPaymentDate = DateTime.UtcNow;
                    subscription.FailedPaymentAttempts = 0; // Reset failed attempts on successful payment
                    
                    subscription.UpdatedBy = tokenModel.UserID;
                    subscription.UpdatedDate = DateTime.UtcNow;
                    await _subscriptionRepository.UpdateAsync(subscription);
                    
                    // CRITICAL: Reset privilege usage for new billing period
                    await ResetPrivilegesForNewBillingPeriodAsync(subscription, tokenModel);
                }
            }

            await _unitOfWork.CommitTransactionAsync();
            _logger.LogInformation("Successfully updated payment records for external payment - billing record {BillingRecordId}", 
                billingRecord.Id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating payment records for external payment - billing record {BillingRecordId}", 
                billingRecord.Id);
            
            // CRITICAL FIX (Issue #10): If external payment was already processed in Stripe but database update failed,
            // issue compensating refund to maintain consistency
            if (billingRecord.Status == BillingRecord.BillingStatus.Paid && 
                !string.IsNullOrEmpty(billingRecord.StripePaymentIntentId))
            {
                await IssueCompensatingRefundAsync(billingRecord, tokenModel);
            }
            
            throw;
        }
    }

    /// <summary>
    /// Issues a compensating refund when Stripe payment succeeds but database update fails.
    /// This maintains consistency between Stripe and the database by ensuring users are not charged
    /// when the database transaction fails. Critical for preventing customer disputes and data integrity issues.
    /// If the refund fails, it is automatically added to the FailedRefunds table for retry and manual review.
    /// 
    /// CRITICAL SAFEGUARD: Prevents double refunds by checking if a refund already exists for this billing record.
    /// </summary>
    /// <param name="billingRecord">The billing record with Stripe payment information</param>
    /// <param name="tokenModel">Token for audit and authorization</param>
    private async Task IssueCompensatingRefundAsync(BillingRecord billingRecord, TokenModel tokenModel)
    {
        // CRITICAL SAFEGUARD #1: Check if refund already exists to prevent double refunds
        // This can happen if:
        // 1. Webhook retries and calls this method multiple times
        // 2. Background service is processing the same billing record
        // 3. Manual admin intervention triggers refund while automatic process is running
        var existingFailedRefund = await _failedRefundRepository.GetByBillingRecordIdAsync(billingRecord.Id);
        if (existingFailedRefund != null)
        {
            _logger.LogWarning(
                "⚠️ DUPLICATE REFUND PREVENTED: A refund already exists for billing record {BillingRecordId}. " +
                "FailedRefundId: {FailedRefundId}, Status: {Status}, RetryCount: {RetryCount}/{MaxRetries}. " +
                "Skipping duplicate refund attempt to prevent double refunding the customer.",
                billingRecord.Id, existingFailedRefund.Id, existingFailedRefund.Status, 
                existingFailedRefund.RetryCount, existingFailedRefund.MaxRetries);
            return;
        }
        
        string errorMessage = null;
        bool refundSucceeded = false;
        
        try
        {
            _logger.LogWarning(
                "CRITICAL: Stripe payment succeeded but database update failed for billing record {BillingRecordId}. " +
                "Issuing compensating refund to prevent charging user without database record. " +
                "PaymentIntentId: {PaymentIntentId}, Amount: ${Amount}",
                billingRecord.Id, billingRecord.StripePaymentIntentId, billingRecord.TotalAmount);
            
            var refundResult = await _stripeService.ProcessRefundAsync(
                billingRecord.StripePaymentIntentId,
                billingRecord.TotalAmount,
                tokenModel);
            
            if (refundResult)
            {
                refundSucceeded = true;
                _logger.LogInformation(
                    "✅ Successfully issued compensating refund for Stripe payment {PaymentIntentId}. " +
                    "User will not be charged due to database failure. Amount refunded: ${Amount}",
                    billingRecord.StripePaymentIntentId, billingRecord.TotalAmount);
            }
            else
            {
                errorMessage = "Stripe refund API returned false (refund failed)";
                _logger.LogError(
                    "❌ CRITICAL ALERT: Failed to issue compensating refund for Stripe payment {PaymentIntentId}. " +
                    "User was charged ${Amount} but database update failed. " +
                    "ADDING TO FAILED REFUNDS QUEUE FOR RETRY. BillingRecordId: {BillingRecordId}",
                    billingRecord.StripePaymentIntentId, billingRecord.TotalAmount, billingRecord.Id);
            }
        }
        catch (Exception refundEx)
        {
            errorMessage = $"Exception during refund: {refundEx.Message}";
            _logger.LogError(refundEx, 
                "❌ CRITICAL ALERT: Exception occurred while attempting compensating refund for Stripe payment {PaymentIntentId}. " +
                "User was charged ${Amount} but database update failed. " +
                "ADDING TO FAILED REFUNDS QUEUE FOR RETRY. BillingRecordId: {BillingRecordId}",
                billingRecord.StripePaymentIntentId, billingRecord.TotalAmount, billingRecord.Id);
        }
        
        // If refund failed, add to failed refunds table for automatic retry and manual review
        if (!refundSucceeded && !string.IsNullOrEmpty(errorMessage))
        {
            await RecordFailedRefundAsync(billingRecord, errorMessage, tokenModel);
        }
    }
    
    /// <summary>
    /// Records a failed compensating refund to the FailedRefunds table for automated retry and manual review.
    /// This ensures financial discrepancies don't go unnoticed and are automatically retried.
    /// </summary>
    /// <param name="billingRecord">The billing record for which refund failed</param>
    /// <param name="errorMessage">Error message from the failed refund attempt</param>
    /// <param name="tokenModel">Token for audit</param>
    private async Task RecordFailedRefundAsync(BillingRecord billingRecord, string errorMessage, TokenModel tokenModel)
    {
        try
        {
            var failedRefund = new FailedRefund
            {
                Id = Guid.NewGuid(),
                BillingRecordId = billingRecord.Id,
                StripePaymentIntentId = billingRecord.StripePaymentIntentId,
                StripeInvoiceId = billingRecord.StripeInvoiceId,
                Amount = billingRecord.TotalAmount,
                UserId = billingRecord.UserId,
                ChargedAt = DateTime.UtcNow,
                DatabaseFailedAt = DateTime.UtcNow,
                FirstAttemptAt = DateTime.UtcNow,
                LastAttemptAt = DateTime.UtcNow,
                RetryCount = 0,
                MaxRetries = 5,
                Status = FailedRefundStatus.Pending,
                LastErrorMessage = errorMessage,
                DatabaseFailureReason = "Database transaction failed after Stripe payment succeeded",
                Priority = "Critical",
                AdminNotified = false,
                CreatedBy = tokenModel?.UserID ?? 0,
                CreatedDate = DateTime.UtcNow
            };
            
            await _failedRefundRepository.CreateAsync(failedRefund);
            
            _logger.LogWarning(
                "✅ Failed refund recorded to database for automatic retry. " +
                "FailedRefundId: {FailedRefundId}, BillingRecordId: {BillingRecordId}, Amount: ${Amount}. " +
                "Background service will retry up to 5 times. Admin will be notified if all retries fail.",
                failedRefund.Id, billingRecord.Id, billingRecord.TotalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ CATASTROPHIC: Failed to record failed refund to database. " +
                "BillingRecordId: {BillingRecordId}, PaymentIntentId: {PaymentIntentId}, Amount: ${Amount}. " +
                "THIS REQUIRES IMMEDIATE MANUAL INTERVENTION - User charged but no refund and no retry record!",
                billingRecord.Id, billingRecord.StripePaymentIntentId, billingRecord.TotalAmount);
            
            // Don't throw - we've already logged the critical alert
            // At this point, only manual database check and intervention can resolve this
        }
    }

    /// <summary>
    /// Resets all privileges for a subscription at the start of a new billing period.
    /// REFACTORED: Now delegates to centralized PrivilegeResetHelper for consistency.
    /// This ensures all privilege resets use the same logic across all services.
    /// Uses admin-set Value directly (no calculation) - the SINGLE SOURCE OF TRUTH.
    /// </summary>
    private async Task ResetPrivilegesForNewBillingPeriodAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            // Get all privilege usage records for this subscription
            var usageRecords = await _subscriptionRepository.GetSubscriptionPrivilegeUsagesAsync(subscription.Id);
            
            // Delegate to centralized helper for consistent reset logic
            // This helper is the SINGLE SOURCE OF TRUTH for privilege resets
            await PrivilegeResetHelper.ResetPrivilegesForBillingPeriodAsync(
                subscription,
                usageRecords,
                async (usage) => await _subscriptionRepository.UpdatePrivilegeUsageAsync(usage),
                tokenModel.UserID,
                _logger
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting privileges for subscription {SubscriptionId}", subscription.Id);
            // Don't throw - privilege reset failure shouldn't fail the payment
        }
    }
    
    /// <summary>
    /// Calculates next retry time using smart retry scheduling
    /// </summary>
    private DateTime CalculateNextRetry(int attemptCount)
    {
        return attemptCount switch
        {
            1 => DateTime.UtcNow.AddHours(1),    // 1 hour
            2 => DateTime.UtcNow.AddDays(1),     // 1 day
            3 => DateTime.UtcNow.AddDays(3),     // 3 days
            _ => DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS)      // Consistent grace period for any additional attempts
        };
    }

    /// <summary>
    /// Calculates next billing date for subscription based on billing cycle.
    /// REFACTORED: Now uses centralized BillingCycleCalculator for consistency.
    /// </summary>
    private DateTime CalculateNextBillingDate(Subscription subscription)
    {
        // CONSISTENT FIX: Use centralized billing cycle calculator instead of duplicate implementation
        return BillingCycleCalculator.CalculateNextBillingDate(DateTime.UtcNow, subscription.BillingCycle);
    }

    /// <summary>
    /// Maps BillingRecord.BillingType to SubscriptionPayment.PaymentType
    /// </summary>
    private SubscriptionPayment.PaymentType MapBillingTypeToPaymentType(BillingRecord.BillingType billingType)
    {
        return billingType switch
        {
            BillingRecord.BillingType.Subscription => SubscriptionPayment.PaymentType.Subscription,
            BillingRecord.BillingType.Overage => SubscriptionPayment.PaymentType.Overage,
            BillingRecord.BillingType.Recurring => SubscriptionPayment.PaymentType.Recurring,
            BillingRecord.BillingType.Upfront => SubscriptionPayment.PaymentType.Upfront,
            BillingRecord.BillingType.Refund => SubscriptionPayment.PaymentType.Refund,
            _ => SubscriptionPayment.PaymentType.Subscription // Default to Subscription for unknown types
        };
    }

    #endregion
}

using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
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
    public PaymentService(
        IStripeBillingService stripeBillingService,
        IBillingRepository billingRepository,
        IStripeService stripeService,
        IMapper mapper,
        ILogger<PaymentService> logger,
        ISubscriptionPaymentRepository subscriptionPaymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _stripeBillingService = stripeBillingService ?? throw new ArgumentNullException(nameof(stripeBillingService));
        _billingRepository = billingRepository ?? throw new ArgumentNullException(nameof(billingRepository));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscriptionPaymentRepository = subscriptionPaymentRepository ?? throw new ArgumentNullException(nameof(subscriptionPaymentRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
    /// Calculates billing period for subscription payment using LastBillingDate logic
    /// </summary>
    private (DateTime start, DateTime end) CalculateBillingPeriod(Subscription subscription, BillingRecord billingRecord)
    {
        var now = DateTime.UtcNow;
        
        // For first payment (no LastBillingDate), use subscription start date
        if (!subscription.LastBillingDate.HasValue)
        {
            var start = subscription.StartDate;
            var end = start.AddMonths(1).AddDays(-1); // End of first month
            return (start, end);
        }

        // For renewal payments, use LastBillingDate + 1 day as start
        var periodStart = subscription.LastBillingDate.Value.AddDays(1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1); // End of billing period

        return (periodStart, periodEnd);
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
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionPayment.SubscriptionId);
                if (subscription != null)
                {
                    subscription.LastBillingDate = subscriptionPayment.BillingPeriodEnd;
                    subscription.NextBillingDate = CalculateNextBillingDate(subscription);
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
            throw;
        }
    }

    /// <summary>
    /// Resets privilege usage for new billing period (scales to billing cycle)
    /// </summary>
    private async Task ResetPrivilegesForNewBillingPeriodAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            var usageRecords = await _subscriptionRepository.GetSubscriptionPrivilegeUsagesAsync(subscription.Id);
            var billingCycleDays = subscription.BillingCycle.DurationInDays;
            var monthsInCycle = billingCycleDays / 30.0m;
            
            foreach (var usage in usageRecords)
            {
                var planPrivilege = subscription.SubscriptionPlan.PlanPrivileges
                    .FirstOrDefault(p => p.Id == usage.SubscriptionPlanPrivilegeId);
                
                if (planPrivilege != null)
                {
                    var monthlyLimit = planPrivilege.MonthlyLimit ?? planPrivilege.Value;
                    var allowedForCycle = monthlyLimit == -1 ? -1 : (int)Math.Ceiling(monthlyLimit * monthsInCycle);
                    
                    usage.UsedValue = 0;
                    usage.AllowedValue = allowedForCycle;
                    usage.UsagePeriodStart = subscription.LastBillingDate.Value.AddDays(1);
                    usage.UsagePeriodEnd = subscription.NextBillingDate;
                    usage.UpdatedBy = tokenModel.UserID;
                    usage.UpdatedDate = DateTime.UtcNow;
                    
                    await _subscriptionRepository.UpdatePrivilegeUsageAsync(usage);
                    
                    _logger.LogInformation("Reset privilege {PrivilegeId} for subscription {SubscriptionId}: AllowedValue={AllowedValue}, Period={Start} to {End}",
                        planPrivilege.PrivilegeId, subscription.Id, allowedForCycle, usage.UsagePeriodStart, usage.UsagePeriodEnd);
                }
            }
            
            _logger.LogInformation("Reset {Count} privilege usages for subscription {SubscriptionId}", 
                usageRecords.Count(), subscription.Id);
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
            _ => DateTime.UtcNow.AddDays(7)      // 7 days for any additional attempts
        };
    }

    /// <summary>
    /// Calculates next billing date for subscription
    /// </summary>
    private DateTime CalculateNextBillingDate(Subscription subscription)
    {
        if (!subscription.LastBillingDate.HasValue)
        {
            return subscription.StartDate.AddMonths(1);
        }

        return subscription.LastBillingDate.Value.AddMonths(1);
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

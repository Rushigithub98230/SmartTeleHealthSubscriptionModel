using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
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

    /// <summary>
    /// Initializes a new instance of the PaymentService with all required dependencies
    /// </summary>
    /// <param name="stripeBillingService">Service for Stripe-specific billing operations</param>
    /// <param name="billingRepository">Repository for billing record data access operations</param>
    /// <param name="stripeService">Service for core Stripe API operations</param>
    /// <param name="mapper">AutoMapper instance for entity-DTO mapping</param>
    /// <param name="logger">Logger instance for logging operations and errors</param>
    public PaymentService(
        IStripeBillingService stripeBillingService,
        IBillingRepository billingRepository,
        IStripeService stripeService,
        IMapper mapper,
        ILogger<PaymentService> logger)
    {
        _stripeBillingService = stripeBillingService ?? throw new ArgumentNullException(nameof(stripeBillingService));
        _billingRepository = billingRepository ?? throw new ArgumentNullException(nameof(billingRepository));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            // Delegate to StripeBillingService for Stripe-specific payment processing
            var paymentResult = await _stripeBillingService.ProcessStripePaymentAsync(billingRecordId, tokenModel);
            
            if (paymentResult.StatusCode == 200)
            {
                _logger.LogInformation("Payment processed successfully for billing record {BillingRecordId}", billingRecordId);
            }
            else
            {
                _logger.LogWarning("Payment processing failed for billing record {BillingRecordId}: {Message}", 
                    billingRecordId, paymentResult.Message);
            }
            
            return paymentResult;
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
            var pendingRecords = await _billingRepository.GetPendingRecordsAsync();
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
}

using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Interface for payment processing operations.
/// This service handles all payment-related operations including processing,
/// refunds, retries, payment method management, and payment analytics.
/// It focuses solely on payment execution and management.
/// </summary>
public interface IPaymentService
{
    // Core Payment Processing
    Task<JsonModel> ProcessPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    
    /// <summary>
    /// Records an externally processed payment (e.g., from Stripe webhook).
    /// Creates SubscriptionPayment, updates subscription billing dates, and resets privileges.
    /// Use this when payment was already processed by external system (Stripe auto-charge).
    /// </summary>
    Task<JsonModel> RecordExternalPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    
    Task<JsonModel> RetryPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> RetryFailedPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);
    
    // Refund Operations
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason, TokenModel tokenModel);
    
    // Payment Method Management
    Task<JsonModel> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId, TokenModel tokenModel);
    Task<JsonModel> GetPaymentMethodsAsync(int userId, TokenModel tokenModel); // SRP Refactoring: Moved from ISubscriptionService
    Task<JsonModel> AddPaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel); // SRP Refactoring: Moved from ISubscriptionService
    Task<JsonModel> AddPaymentMethodAsync(AddPaymentMethodDto dto, TokenModel tokenModel);
    Task<JsonModel> GetUserPaymentMethodsAsync(int userId, TokenModel tokenModel);
    Task<JsonModel> SetDefaultPaymentMethodAsync(string paymentMethodId, TokenModel tokenModel);
    Task<JsonModel> GetPaymentMethodByStripeIdAsync(string stripePaymentMethodId, TokenModel tokenModel);
    
    // NEW: Enhanced Payment Method Management Methods
    Task<JsonModel> DeletePaymentMethodAsync(string paymentMethodId, TokenModel tokenModel);
    Task<JsonModel> UpdatePaymentMethodDetailsAsync(string paymentMethodId, UpdatePaymentMethodDto dto, TokenModel tokenModel);
    Task<JsonModel> ValidatePaymentMethodAsync(string paymentMethodId, TokenModel tokenModel);
    Task<JsonModel> GetPaymentMethodDetailsAsync(string paymentMethodId, TokenModel tokenModel);
    
    // Special Payment Types
    Task<JsonModel> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto, TokenModel tokenModel);
    
    // Payment Status & Validation
    Task<JsonModel> GetPendingPaymentsAsync(TokenModel tokenModel);
    Task<JsonModel> IsPaymentOverdueAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ValidatePaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> GetPaymentScheduleAsync(Guid subscriptionId, TokenModel tokenModel);
    
    // Payment History & Analytics
    Task<JsonModel> GetPaymentHistoryAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetPaymentAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetPaymentAnalyticsAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    
    // Payment Reports
    Task<JsonModel> ExportPaymentHistoryAsync(int userId, DateTime? startDate, DateTime? endDate, string format, TokenModel tokenModel);
    
    // Invoice Management
    Task<JsonModel> CreateInvoiceAsync(CreateInvoiceDto createDto, TokenModel tokenModel);
    Task<JsonModel> GenerateInvoicePdfAsync(Guid billingRecordId, TokenModel tokenModel);
    
    // Recurring Billing
    Task<JsonModel> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessRecurringPaymentAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> CancelRecurringBillingAsync(Guid subscriptionId, TokenModel tokenModel);
}

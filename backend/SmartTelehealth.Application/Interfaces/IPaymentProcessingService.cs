using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Interface for payment processing service operations.
/// This interface defines the contract for payment processing, billing management,
/// and payment lifecycle operations within the application.
/// </summary>
public interface IPaymentProcessingService
{
    // Billing Record Management
    Task<JsonModel> CreateBillingRecordAsync(CreateBillingRecordDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetBillingRecordAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> GetAllBillingRecordsAsync(int page, int pageSize, string? searchTerm, string[]? status, string[]? type, string[]? userId, string[]? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel);
    Task<JsonModel> GetUserBillingHistoryAsync(int userId, TokenModel tokenModel);
    Task<JsonModel> GetSubscriptionBillingHistoryAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> GetOverdueBillingRecordsAsync(TokenModel tokenModel);
    Task<JsonModel> GetPendingPaymentsAsync(TokenModel tokenModel);

    // Payment Processing
    Task<JsonModel> ProcessPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> RetryPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> RetryFailedPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);
    Task<JsonModel> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId, TokenModel tokenModel);

    // Refund Processing
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason, TokenModel tokenModel);
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);

    // Recurring Billing
    Task<JsonModel> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessRecurringPaymentAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> CancelRecurringBillingAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> GetPaymentScheduleAsync(Guid subscriptionId, TokenModel tokenModel);

    // Payment Types
    Task<JsonModel> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto, TokenModel tokenModel);

    // Billing Adjustments
    Task<JsonModel> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto, TokenModel tokenModel);
    Task<JsonModel> GetBillingAdjustmentsAsync(Guid billingRecordId, TokenModel tokenModel);

    // Invoice Management
    Task<JsonModel> CreateInvoiceAsync(CreateInvoiceDto createDto, TokenModel tokenModel);
    Task<JsonModel> GenerateInvoiceAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> GenerateInvoicePdfAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> GetInvoiceAsync(string invoiceNumber, TokenModel tokenModel);
    Task<JsonModel> UpdateInvoiceStatusAsync(string invoiceNumber, string newStatus, TokenModel tokenModel);

    // Billing Cycles
    Task<JsonModel> CreateBillingCycleAsync(CreateBillingCycleDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetBillingCycleRecordsAsync(Guid billingCycleId, TokenModel tokenModel);
    Task<JsonModel> ProcessBillingCycleAsync(Guid billingCycleId, TokenModel tokenModel);

    // Analytics and Reporting
    Task<JsonModel> GetPaymentAnalyticsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null);
    Task<JsonModel> GetPaymentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null);
    Task<JsonModel> GetRevenueSummaryAsync(DateTime? from = null, DateTime? to = null, string? planId = null, TokenModel tokenModel = null);
    Task<JsonModel> GetBillingSummaryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null);
    Task<JsonModel> GetBillingAnalyticsAsync(TokenModel tokenModel);
    Task<JsonModel> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf", TokenModel tokenModel = null);
    Task<JsonModel> ExportRevenueAsync(DateTime? from = null, DateTime? to = null, string? planId = null, string format = "csv", TokenModel tokenModel = null);
    Task<JsonModel> ExportBillingRecordsAsync(TokenModel tokenModel, int page, int pageSize, string? searchTerm, string[]? status, string[]? type, string[]? userId, string[]? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, string format);

    // Payment History
    Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetPaymentHistoryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null);

    // Calculations
    Task<JsonModel> CalculateTotalAmountAsync(decimal subtotal, decimal tax, decimal shipping, TokenModel tokenModel);
    Task<JsonModel> CalculateTaxAmountAsync(decimal amount, string taxRate, TokenModel tokenModel);
    Task<JsonModel> CalculateShippingAmountAsync(string shippingMethod, bool isExpress, TokenModel tokenModel);
    Task<JsonModel> CalculateDueDateAsync(DateTime startDate, int daysToAdd, TokenModel tokenModel);

    // Payment Status
    Task<JsonModel> IsPaymentOverdueAsync(Guid billingRecordId, TokenModel tokenModel);
}



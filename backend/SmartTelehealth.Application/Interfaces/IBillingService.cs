using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

public interface IBillingService
{
    // Existing Methods
    Task<JsonModel> CreateBillingRecordAsync(CreateBillingRecordDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetBillingRecordAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetUserBillingHistoryAsync(int userId, TokenModel tokenModel);
    Task<JsonModel> GetSubscriptionBillingHistoryAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> GetAllBillingRecordsAsync(int page, int pageSize, string? searchTerm, string[]? status, string[]? type, string[]? userId, string[]? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel);
    Task<JsonModel> ProcessPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason, TokenModel tokenModel);
    Task<JsonModel> GetOverdueBillingRecordsAsync(TokenModel tokenModel);
    Task<JsonModel> GetPendingPaymentsAsync(TokenModel tokenModel);
    Task<JsonModel> CalculateTotalAmountAsync(decimal baseAmount, decimal taxAmount, decimal shippingAmount, TokenModel tokenModel);
    Task<JsonModel> CalculateTaxAmountAsync(decimal baseAmount, string state, TokenModel tokenModel);
    Task<JsonModel> CalculateShippingAmountAsync(string deliveryAddress, bool isExpress, TokenModel tokenModel);
    Task<JsonModel> IsPaymentOverdueAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> CalculateDueDateAsync(DateTime billingDate, int gracePeriodDays, TokenModel tokenModel);
    Task<JsonModel> GetBillingAnalyticsAsync(TokenModel tokenModel);
    
    // Payment History Methods
    Task<JsonModel> GetPaymentHistoryAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetPaymentAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetPaymentAnalyticsAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    
    // Phase 2: Enhanced Billing Features
    Task<JsonModel> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessRecurringPaymentAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> CancelRecurringBillingAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto, TokenModel tokenModel);
    Task<JsonModel> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto, TokenModel tokenModel);
    Task<JsonModel> GetBillingAdjustmentsAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> RetryFailedPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> RetryPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);
    Task<JsonModel> CreateInvoiceAsync(CreateInvoiceDto createDto, TokenModel tokenModel);
    Task<JsonModel> GenerateInvoicePdfAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format, TokenModel tokenModel);
    Task<JsonModel> GetBillingSummaryAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetPaymentScheduleAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId, TokenModel tokenModel);
    Task<JsonModel> CreateBillingCycleAsync(CreateBillingCycleDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessBillingCycleAsync(Guid billingCycleId, TokenModel tokenModel);
    Task<JsonModel> GetBillingCycleRecordsAsync(Guid billingCycleId, TokenModel tokenModel);
    Task<JsonModel> GetRevenueSummaryAsync(DateTime? from, DateTime? to, string? planId, TokenModel tokenModel);
    Task<JsonModel> ExportRevenueAsync(DateTime? from, DateTime? to, string? planId, string format, TokenModel tokenModel);
    
    // Export functionality
    Task<JsonModel> ExportBillingRecordsAsync(TokenModel tokenModel, int page, int pageSize, string? searchTerm, string[]? status, string[]? type, string[]? userId, string[]? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, string format);
    
    // NEW: Enhanced invoice management methods
    Task<JsonModel> GenerateInvoiceAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> GetInvoiceAsync(string invoiceNumber, TokenModel tokenModel);
    Task<JsonModel> UpdateInvoiceStatusAsync(string invoiceNumber, string newStatus, TokenModel tokenModel);
} 
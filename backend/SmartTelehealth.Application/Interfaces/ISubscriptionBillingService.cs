using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Comprehensive Subscription Billing Service Interface
/// Combines all functionality from IBillingService and IPrivilegeBasedBillingService
/// Aligned with client's subscription management billing workflow
/// </summary>
public interface ISubscriptionBillingService
{
    // ===== PRIVILEGE-BASED BILLING (Client Workflow) =====
    
    /// <summary>
    /// Calculates the base price for a subscription plan based on privileges and their unit costs
    /// Client Workflow Step 1: Admin Creates a Subscription Plan
    /// </summary>
    Task<JsonModel> CalculatePlanBasePriceAsync(CalculatePlanPriceDto calculateDto, TokenModel tokenModel);

    /// <summary>
    /// Processes privilege usage and calculates extra charges if limits are exceeded
    /// Client Workflow Steps 3-4: Privilege Usage Tracking & Extra Usage Calculation
    /// </summary>
    Task<JsonModel> ProcessPrivilegeUsageAsync(ProcessPrivilegeUsageDto usageDto, TokenModel tokenModel);

    /// <summary>
    /// Processes subscription renewal and resets privilege usage
    /// Client Workflow Step 6: Renewal or Expiry
    /// </summary>
    Task<JsonModel> ProcessSubscriptionRenewalAsync(Guid subscriptionId, TokenModel tokenModel);

    /// <summary>
    /// Resets subscription dates and privilege usage for a new billing period.
    /// ⚠️ WARNING: This method ONLY updates dates and resets privileges.
    /// It does NOT create billing records or process payments.
    /// Phase 4 Refactor: Renamed from ProcessSubscriptionRenewalAsync to clarify limited scope.
    /// For complete renewal with billing, use AutomatedBillingService.ProcessSubscriptionRenewalAsync
    /// </summary>
    Task<JsonModel> ResetSubscriptionForNewBillingPeriodAsync(Guid subscriptionId, TokenModel tokenModel);

    /// <summary>
    /// Gets privilege usage summary for a user
    /// </summary>
    Task<JsonModel> GetPrivilegeUsageSummaryAsync(int userId, TokenModel tokenModel);
    
    // ===== CORE BILLING RECORD MANAGEMENT =====
    
    Task<JsonModel> CreateBillingRecordAsync(CreateBillingRecordDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetBillingRecordAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetUserBillingHistoryAsync(int userId, TokenModel tokenModel);
    Task<JsonModel> GetSubscriptionBillingHistoryAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> GetBillingRecordsWithFilteringAsync(BillingFilterDto filter, TokenModel? tokenModel = null, bool adminOnly = false);
    Task<JsonModel> GetAllBillingRecordsAsync(int page, int pageSize, string? searchTerm, string[]? status, string[]? type, string[]? userId, string[]? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel);
    
    // ===== PAYMENT PROCESSING =====
    
    Task<JsonModel> ProcessPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason, TokenModel tokenModel);
    Task<JsonModel> RetryFailedPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> RetryPaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);
    Task<JsonModel> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId, TokenModel tokenModel);
    
    // ===== BILLING QUERIES =====
    
    Task<JsonModel> GetOverdueBillingRecordsAsync(TokenModel tokenModel);
    Task<JsonModel> GetPendingPaymentsAsync(TokenModel tokenModel);
    
    // ===== CALCULATIONS =====
    
    Task<JsonModel> CalculateTotalAmountAsync(decimal baseAmount, decimal taxAmount, decimal shippingAmount, TokenModel tokenModel);
    Task<JsonModel> CalculateTaxAmountAsync(decimal baseAmount, string state, TokenModel tokenModel);
    Task<JsonModel> CalculateShippingAmountAsync(string deliveryAddress, bool isExpress, TokenModel tokenModel);
    Task<JsonModel> IsPaymentOverdueAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> CalculateDueDateAsync(DateTime billingDate, int gracePeriodDays, TokenModel tokenModel);
    
    // ===== PAYMENT HISTORY & ANALYTICS =====
    
    Task<JsonModel> GetPaymentHistoryAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetPaymentAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetPaymentAnalyticsAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetBillingAnalyticsAsync(TokenModel tokenModel);
    Task<JsonModel> GetBillingSummaryAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetRevenueSummaryAsync(DateTime? from, DateTime? to, string? planId, TokenModel tokenModel);
    
    // ===== ENHANCED BILLING FEATURES =====
    
    Task<JsonModel> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessRecurringPaymentAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> CancelRecurringBillingAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto, TokenModel tokenModel);
    
    // ===== BILLING ADJUSTMENTS =====
    
    Task<JsonModel> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto, TokenModel tokenModel);
    Task<JsonModel> GetBillingAdjustmentsAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ReverseBillingAdjustmentAsync(Guid adjustmentId, TokenModel tokenModel);
    Task<decimal> GetTotalAdjustmentAmountAsync(Guid billingRecordId);
    
    // ===== INVOICING =====
    
    Task<JsonModel> CreateInvoiceAsync(CreateInvoiceDto createDto, TokenModel tokenModel);
    Task<JsonModel> GenerateInvoiceAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> GenerateInvoicePdfAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> GetInvoiceAsync(string invoiceNumber, TokenModel tokenModel);
    Task<JsonModel> UpdateInvoiceStatusAsync(string invoiceNumber, string newStatus, TokenModel tokenModel);
    
    // ===== REPORTING & EXPORT =====
    
    Task<JsonModel> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format, TokenModel tokenModel);
    Task<JsonModel> ExportBillingRecordsAsync(TokenModel tokenModel, int page, int pageSize, string? searchTerm, string[]? status, string[]? type, string[]? userId, string[]? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, string format);
    Task<JsonModel> ExportRevenueAsync(DateTime? from, DateTime? to, string? planId, string format, TokenModel tokenModel);
    
    // ===== BILLING CYCLE MANAGEMENT =====
    
    Task<JsonModel> CreateBillingCycleAsync(CreateBillingCycleDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessBillingCycleAsync(Guid billingCycleId, TokenModel tokenModel);
    Task<JsonModel> GetBillingCycleRecordsAsync(Guid billingCycleId, TokenModel tokenModel);
    Task<JsonModel> GetPaymentScheduleAsync(Guid subscriptionId, TokenModel tokenModel);
    
    // ===== SRP REFACTORING: BILLING RECORD FACTORY METHODS =====
    
    Task<JsonModel> CreateSubscriptionBillingAsync(Subscription subscription, decimal amount, string description, DateTime? dueDate = null, TokenModel tokenModel = null);
    Task<JsonModel> CreateOverageBillingAsync(Subscription subscription, string privilegeName, decimal amount, TokenModel tokenModel);
    
    /// <summary>
    /// Creates healthcare-compliant overage billing (uses latest plan version pricing).
    /// Healthcare Rule: Overage charges use LATEST plan pricing to prevent abuse.
    /// </summary>
    Task<JsonModel> CreateHealthcareOverageBillingAsync(Guid subscriptionId, Guid privilegeId, int quantity, TokenModel tokenModel);
    
    Task<JsonModel> CreateConsultationBillingAsync(int userId, Guid consultationId, decimal amount, string? description = null, TokenModel tokenModel = null);
    Task<JsonModel> CreateMedicationBillingAsync(Subscription subscription, decimal amount, string? description = null, TokenModel tokenModel = null);
    
    // ===== SRP REFACTORING: BILLING DATE CALCULATION =====
    
    DateTime CalculateNextBillingDate(DateTime currentDate, MasterBillingCycle billingCycle);
    Task<DateTime> CalculateNextBillingDateForSubscriptionAsync(Guid subscriptionId, TokenModel tokenModel);
    
    // ===== BILLING CYCLES (For User Purchase Flow) =====
    
    /// <summary>
    /// Gets all active billing cycles for user subscription purchase flow
    /// </summary>
    Task<IEnumerable<MasterBillingCycle>> GetAllBillingCyclesAsync();
    
    // ===== PHASE 2: BILLING MANAGEMENT =====
    
    /// <summary>
    /// Get aggregate billing summary for admin dashboard
    /// Phase 2: Returns overall statistics without user filtering
    /// </summary>
    Task<JsonModel> GetAdminBillingSummaryAsync(TokenModel tokenModel);
    
    /// <summary>
    /// Manually mark a billing record as paid (admin override)
    /// Phase 2: For manual payment processing
    /// </summary>
    Task<JsonModel> MarkBillingAsPaidAsync(Guid billingRecordId, MarkAsPaidRequestDto request, TokenModel tokenModel);
    
    // ===== PHASE 3: FAILED PAYMENT MANAGEMENT =====
    
    /// <summary>
    /// Get all failed payments with details and retry status
    /// Phase 3: Returns comprehensive failed payment information
    /// </summary>
    Task<JsonModel> GetFailedPaymentsAsync(TokenModel tokenModel);
    
    /// <summary>
    /// Send payment reminder email to customer
    /// Phase 3: Customer communication for failed payments
    /// </summary>
    Task<JsonModel> SendPaymentReminderAsync(Guid billingRecordId, SendReminderRequestDto request, TokenModel tokenModel);
    
    /// <summary>
    /// Bulk retry multiple failed payments
    /// Phase 3: Batch processing for failed payments
    /// </summary>
    Task<JsonModel> BulkRetryPaymentsAsync(BulkRetryRequestDto request, TokenModel tokenModel);
}


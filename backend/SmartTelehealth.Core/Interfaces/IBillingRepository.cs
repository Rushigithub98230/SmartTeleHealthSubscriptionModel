using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Core.Interfaces;

public interface IBillingRepository : IRepositoryBase<BillingRecord>
{
    // Basic CRUD methods are inherited from IRepositoryBase<BillingRecord>
    
    Task<IEnumerable<BillingRecord>> GetByUserIdAsync(int userId);
    Task<IEnumerable<BillingRecord>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<BillingRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<BillingRecord>> GetByStatusAsync(BillingRecord.BillingStatus status);
    
    // New methods for InvoiceService
    Task<BillingRecord?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<BillingRecord>> GetInvoicesByUserIdAsync(int userId, int page, int pageSize);
    Task<int> GetInvoiceCountByUserIdAsync(int userId);
    Task<IEnumerable<BillingRecord>> GetBillingRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // Additional methods needed by BillingService
    Task<IEnumerable<BillingAdjustment>> GetAdjustmentsByBillingRecordIdAsync(Guid billingRecordId);
    Task<BillingAdjustment?> GetAdjustmentByIdAsync(Guid adjustmentId);
    Task<BillingAdjustment> CreateAdjustmentAsync(BillingAdjustment adjustment);
    Task<BillingAdjustment> UpdateAdjustmentAsync(BillingAdjustment adjustment);
    Task<IEnumerable<BillingRecord>> GetByBillingCycleIdAsync(Guid billingCycleId);
    Task<IEnumerable<BillingRecord>> GetOverdueRecordsAsync();
    Task<IEnumerable<BillingRecord>> GetPendingRecordsAsync();
    
    // Webhook support methods
    Task<BillingRecord?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId);
    Task<BillingRecord?> GetByStripeInvoiceIdAsync(string stripeInvoiceId);
    Task<BillingRecord?> GetByTransactionIdAsync(string transactionId);
    
    // Comprehensive filtering method
    Task<(IEnumerable<BillingRecord> BillingRecords, int TotalCount)> GetBillingRecordsWithAdvancedFilteringAsync(BillingFilterDto filter);
} 
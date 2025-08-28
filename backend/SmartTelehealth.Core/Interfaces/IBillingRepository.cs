using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IBillingRepository : IRepositoryBase<BillingRecord>
{
    Task<BillingRecord?> GetByIdAsync(Guid id);
    Task<IEnumerable<BillingRecord>> GetByUserIdAsync(int userId);
    Task<IEnumerable<BillingRecord>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<BillingRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<BillingRecord>> GetByStatusAsync(BillingRecord.BillingStatus status);
    Task<BillingRecord> CreateAsync(BillingRecord billingRecord);
    Task<BillingRecord> UpdateAsync(BillingRecord billingRecord);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    
    // New methods for InvoiceService
    Task<BillingRecord?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<BillingRecord>> GetInvoicesByUserIdAsync(int userId, int page, int pageSize);
    Task<int> GetInvoiceCountByUserIdAsync(int userId);
    Task<IEnumerable<BillingRecord>> GetBillingRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // Additional methods needed by BillingService
    Task<IEnumerable<BillingAdjustment>> GetAdjustmentsByBillingRecordIdAsync(Guid billingRecordId);
    Task<IEnumerable<BillingRecord>> GetByBillingCycleIdAsync(Guid billingCycleId);
    Task<IEnumerable<BillingRecord>> GetOverdueRecordsAsync();
    Task<IEnumerable<BillingRecord>> GetPendingRecordsAsync();
} 
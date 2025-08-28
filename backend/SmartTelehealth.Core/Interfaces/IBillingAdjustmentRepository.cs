using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IBillingAdjustmentRepository
{
    Task<BillingAdjustment> GetByIdAsync(Guid id);
    Task<IEnumerable<BillingAdjustment>> GetByBillingRecordIdAsync(Guid billingRecordId);
    Task<BillingAdjustment> CreateAsync(BillingAdjustment billingAdjustment);
    Task<BillingAdjustment> UpdateAsync(BillingAdjustment billingAdjustment);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<BillingAdjustment>> GetAllAsync();
} 
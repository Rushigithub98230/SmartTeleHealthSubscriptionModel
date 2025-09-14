using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IBillingAdjustmentRepository : IRepositoryBase<BillingAdjustment>
{
    // Basic CRUD methods are inherited from IRepositoryBase<BillingAdjustment>
    
    Task<IEnumerable<BillingAdjustment>> GetByBillingRecordIdAsync(Guid billingRecordId);
} 
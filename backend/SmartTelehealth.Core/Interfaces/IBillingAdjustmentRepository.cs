using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IBillingAdjustmentRepository : IRepositoryBase<BillingAdjustment>
{
    // Basic CRUD methods are inherited from IRepositoryBase<BillingAdjustment>
    
    Task<IEnumerable<BillingAdjustment>> GetByBillingRecordIdAsync(Guid billingRecordId);
    
    // Database-level filtering, pagination, and sorting
    Task<(IEnumerable<BillingAdjustment> Adjustments, int TotalCount)> GetAdjustmentsWithFilteringAsync(
        int page, int pageSize, Guid? billingRecordId = null, string? type = null, 
        string? search = null, DateTime? startDate = null, DateTime? endDate = null, 
        string? sortBy = "CreatedDate", string? sortOrder = "desc");
} 
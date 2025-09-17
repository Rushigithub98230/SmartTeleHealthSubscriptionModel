using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionPlanPrivilegeRepository : IRepositoryBase<SubscriptionPlanPrivilege>
{
    // Basic CRUD methods are inherited from IRepositoryBase<SubscriptionPlanPrivilege>
    
    Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPlanIdAsync(Guid planId);
    Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPrivilegeIdAsync(Guid privilegeId);
    Task AddAsync(SubscriptionPlanPrivilege planPrivilege); // Legacy method

    // Database-level filtering, pagination, and sorting
    Task<(IEnumerable<SubscriptionPlanPrivilege> Privileges, int TotalCount)> GetPlanPrivilegesWithFilteringAsync(
        int page, int pageSize, Guid? planId = null, Guid? privilegeId = null, 
        string? search = null, bool? isActive = null, string? sortBy = "CreatedDate", string? sortOrder = "desc");
} 
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionPlanPrivilegeRepository : IRepositoryBase<SubscriptionPlanPrivilege>
{
    // Basic CRUD methods are inherited from IRepositoryBase<SubscriptionPlanPrivilege>
    
    Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPlanIdAsync(Guid planId);
    Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPrivilegeIdAsync(Guid privilegeId);
    Task AddAsync(SubscriptionPlanPrivilege planPrivilege); // Legacy method
} 
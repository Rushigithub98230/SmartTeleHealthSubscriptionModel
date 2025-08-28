using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionPlanPrivilegeRepository
{
    Task<SubscriptionPlanPrivilege?> GetByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPlanIdAsync(Guid planId);
    Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPrivilegeIdAsync(Guid privilegeId);
    Task AddAsync(SubscriptionPlanPrivilege planPrivilege);
    Task UpdateAsync(SubscriptionPlanPrivilege planPrivilege);
    Task DeleteAsync(Guid id);
} 
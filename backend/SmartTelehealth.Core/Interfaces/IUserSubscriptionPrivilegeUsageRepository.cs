using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IUserSubscriptionPrivilegeUsageRepository : IRepositoryBase<UserSubscriptionPrivilegeUsage>
{
    // Basic CRUD methods are inherited from IRepositoryBase<UserSubscriptionPrivilegeUsage>
    
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionPlanPrivilegeIdAsync(Guid subscriptionPlanPrivilegeId);
    Task AddAsync(UserSubscriptionPrivilegeUsage usage); // Legacy method
} 
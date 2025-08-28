using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IUserSubscriptionPrivilegeUsageRepository
{
    Task<UserSubscriptionPrivilegeUsage?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionPlanPrivilegeIdAsync(Guid subscriptionPlanPrivilegeId);
    Task AddAsync(UserSubscriptionPrivilegeUsage usage);
    Task UpdateAsync(UserSubscriptionPrivilegeUsage usage);
    Task DeleteAsync(Guid id);
} 
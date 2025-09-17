using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IUserSubscriptionPrivilegeUsageRepository : IRepositoryBase<UserSubscriptionPrivilegeUsage>
{
    // Basic CRUD methods are inherited from IRepositoryBase<UserSubscriptionPrivilegeUsage>
    
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionPlanPrivilegeIdAsync(Guid subscriptionPlanPrivilegeId);
    Task AddAsync(UserSubscriptionPrivilegeUsage usage); // Legacy method

    // Database-level filtering, pagination, and sorting
    Task<(IEnumerable<UserSubscriptionPrivilegeUsage> Usages, int TotalCount)> GetUsagesWithFilteringAsync(
        int page, int pageSize, Guid? subscriptionId = null, Guid? privilegeId = null, 
        int? userId = null, string? search = null, bool? isActive = null, 
        DateTime? startDate = null, DateTime? endDate = null, string? sortBy = "LastUsedAt", string? sortOrder = "desc");
} 
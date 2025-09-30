using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IUserSubscriptionPrivilegeUsageRepository : IRepositoryBase<UserSubscriptionPrivilegeUsage>
{
    
    // Custom methods with different names to avoid overriding base methods
    Task<UserSubscriptionPrivilegeUsage?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetAllWithDetailsAsync();
    Task<UserSubscriptionPrivilegeUsage> CreateUsageAsync(UserSubscriptionPrivilegeUsage usage);
    Task<UserSubscriptionPrivilegeUsage> UpdateUsageAsync(UserSubscriptionPrivilegeUsage usage);
    Task<bool> DeleteUsageAsync(Guid id);
    Task<bool> ExistsUsageAsync(Guid id);
    
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionPlanPrivilegeIdAsync(Guid subscriptionPlanPrivilegeId);
    Task AddAsync(UserSubscriptionPrivilegeUsage usage); // Legacy method

    // Database-level filtering, pagination, and sorting
    Task<(IEnumerable<UserSubscriptionPrivilegeUsage> Usages, int TotalCount)> GetUsagesWithFilteringAsync(
        int page, int pageSize, Guid? subscriptionId = null, Guid? privilegeId = null, 
        int? userId = null, string? search = null, bool? isActive = null, 
        DateTime? startDate = null, DateTime? endDate = null, string? sortBy = "LastUsedAt", string? sortOrder = "desc");
    
    /// <summary>
    /// Gets privilege usage record for a specific user and privilege
    /// </summary>
    Task<UserSubscriptionPrivilegeUsage?> GetByUserAndPrivilegeAsync(int userId, Guid privilegeId);
    
    /// <summary>
    /// Gets all privilege usage records for a specific user
    /// </summary>
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetByUserIdAsync(int userId);
    
    /// <summary>
    /// Updates a privilege usage record
    /// </summary>
    Task<UserSubscriptionPrivilegeUsage> UpdatePrivilegeUsageAsync(UserSubscriptionPrivilegeUsage usage);
    
    /// <summary>
    /// Creates a new privilege usage record
    /// </summary>
    Task<UserSubscriptionPrivilegeUsage> CreatePrivilegeUsageAsync(UserSubscriptionPrivilegeUsage usage);
} 
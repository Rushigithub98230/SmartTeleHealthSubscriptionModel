using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IPrivilegeUsageHistoryRepository : IRepositoryBase<PrivilegeUsageHistory>
{
    // Basic CRUD methods are inherited from IRepositoryBase<PrivilegeUsageHistory>
    
    // Custom methods with different names to avoid overriding base methods
    Task<PrivilegeUsageHistory?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<PrivilegeUsageHistory>> GetAllWithDetailsAsync();
    Task<PrivilegeUsageHistory> CreateUsageHistoryAsync(PrivilegeUsageHistory history);
    Task<PrivilegeUsageHistory> UpdateUsageHistoryAsync(PrivilegeUsageHistory history);
    Task<bool> DeleteUsageHistoryAsync(Guid id);
    Task<bool> ExistsUsageHistoryAsync(Guid id);
    
    Task<IEnumerable<PrivilegeUsageHistory>> GetByUserSubscriptionPrivilegeUsageIdAsync(Guid userSubscriptionPrivilegeUsageId);
    Task<IEnumerable<PrivilegeUsageHistory>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<PrivilegeUsageHistory>> GetByDateRangeAsync(Guid subscriptionId, DateTime startDate, DateTime endDate);
    Task<int> GetDailyUsageAsync(Guid subscriptionId, Guid privilegeId, DateTime date);
    Task<int> GetWeeklyUsageAsync(Guid subscriptionId, Guid privilegeId, DateTime weekStart);
    Task<int> GetMonthlyUsageAsync(Guid subscriptionId, Guid privilegeId, DateTime monthStart);
    Task AddAsync(PrivilegeUsageHistory usageHistory); // Legacy method
    
    // Database-level filtering, pagination, and sorting
    Task<(IEnumerable<PrivilegeUsageHistory> History, int TotalCount)> GetUsageHistoryWithFilteringAsync(
        int page, int pageSize, string? privilegeId, string? userId, string? subscriptionId, 
        DateTime? startDate, DateTime? endDate, string? sortBy = "UsedAt", string? sortOrder = "desc");
    
    // Database-level aggregation
    Task<object> GetUsageSummaryAsync(string? privilegeId, string? userId, string? subscriptionId, 
        DateTime? startDate, DateTime? endDate);
}

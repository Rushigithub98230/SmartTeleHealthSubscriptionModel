using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IPrivilegeUsageHistoryRepository : IRepositoryBase<PrivilegeUsageHistory>
{
    // Basic CRUD methods are inherited from IRepositoryBase<PrivilegeUsageHistory>
    
    Task<IEnumerable<PrivilegeUsageHistory>> GetByUserSubscriptionPrivilegeUsageIdAsync(Guid userSubscriptionPrivilegeUsageId);
    Task<IEnumerable<PrivilegeUsageHistory>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<PrivilegeUsageHistory>> GetByDateRangeAsync(Guid subscriptionId, DateTime startDate, DateTime endDate);
    Task<int> GetDailyUsageAsync(Guid subscriptionId, Guid privilegeId, DateTime date);
    Task<int> GetWeeklyUsageAsync(Guid subscriptionId, Guid privilegeId, DateTime weekStart);
    Task<int> GetMonthlyUsageAsync(Guid subscriptionId, Guid privilegeId, DateTime monthStart);
    Task AddAsync(PrivilegeUsageHistory usageHistory); // Legacy method
}

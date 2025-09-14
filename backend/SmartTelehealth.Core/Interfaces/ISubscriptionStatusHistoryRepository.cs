using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionStatusHistoryRepository : IRepositoryBase<SubscriptionStatusHistory>
{
    // Basic CRUD methods are inherited from IRepositoryBase<SubscriptionStatusHistory>
    
    Task<IEnumerable<SubscriptionStatusHistory>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<SubscriptionStatusHistory>> GetByStatusAsync(string status);
    Task<IEnumerable<SubscriptionStatusHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetCountBySubscriptionIdAsync(Guid subscriptionId);
    Task<SubscriptionStatusHistory?> GetLatestBySubscriptionIdAsync(Guid subscriptionId);
} 
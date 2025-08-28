using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionStatusHistoryRepository
{
    Task<SubscriptionStatusHistory?> GetByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionStatusHistory>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<SubscriptionStatusHistory>> GetByStatusAsync(string status);
    Task<IEnumerable<SubscriptionStatusHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<SubscriptionStatusHistory> CreateAsync(SubscriptionStatusHistory history);
    Task<SubscriptionStatusHistory> UpdateAsync(SubscriptionStatusHistory history);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetCountBySubscriptionIdAsync(Guid subscriptionId);
    Task<SubscriptionStatusHistory?> GetLatestBySubscriptionIdAsync(Guid subscriptionId);
} 
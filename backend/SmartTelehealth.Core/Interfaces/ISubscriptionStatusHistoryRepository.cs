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
    
    // Database-level filtering, pagination, and sorting
    Task<(IEnumerable<SubscriptionStatusHistory> History, int TotalCount)> GetHistoryWithFilteringAsync(
        int page, int pageSize, Guid? subscriptionId = null, string? status = null, 
        string? search = null, DateTime? startDate = null, DateTime? endDate = null, 
        string? sortBy = "ChangedAt", string? sortOrder = "desc");
} 
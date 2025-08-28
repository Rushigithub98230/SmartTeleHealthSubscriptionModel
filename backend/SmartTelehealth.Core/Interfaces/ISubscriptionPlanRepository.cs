using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionPlan>> GetAllAsync();
    Task AddAsync(SubscriptionPlan plan);
    Task UpdateAsync(SubscriptionPlan plan);
    Task DeleteAsync(Guid id);
} 
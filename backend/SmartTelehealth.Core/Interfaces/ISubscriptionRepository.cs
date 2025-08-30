using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionRepository : IRepositoryBase<Subscription>
{
    // Basic CRUD methods (inherited from IRepositoryBase<T>)
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync are inherited
    
    // Subscription-specific methods
    Task<IEnumerable<Subscription>> GetByUserIdAsync(int userId);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, TokenModel tokenModel);
    Task<IEnumerable<Subscription>> GetByStatusAsync(string status);
    Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync();
    Task<IEnumerable<Subscription>> GetSubscriptionsDueForBillingAsync(DateTime billingDate);
    Task<IEnumerable<Subscription>> GetSubscriptionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // Additional methods for analytics and automation
    Task<int> GetActiveSubscriptionCountAsync();
    Task<decimal> GetTotalMonthlyRevenueAsync();
    Task<IEnumerable<Subscription>> GetSubscriptionsExpiringSoonAsync(int daysAhead);
    Task<IEnumerable<Subscription>> GetSubscriptionsWithFailedPaymentsAsync();
    
    // Methods for getting subscriptions in date range and suspended subscriptions
    Task<IEnumerable<Subscription>> GetSubscriptionsInDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Subscription>> GetSuspendedSubscriptionsAsync();
    
    // Subscription Plan methods
    Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionPlan>> GetAllSubscriptionPlansAsync();
    Task<SubscriptionPlan> CreateSubscriptionPlanAsync(SubscriptionPlan plan);
    Task<SubscriptionPlan> UpdateSubscriptionPlanAsync(SubscriptionPlan plan);
    Task<bool> DeleteSubscriptionPlanAsync(Guid id);
    
    // Additional methods needed by services
    Task<IEnumerable<Subscription>> GetByPlanIdAsync(Guid planId);
    Task<IEnumerable<Subscription>> GetAllSubscriptionsAsync();
    Task<IEnumerable<SubscriptionPlan>> GetActiveSubscriptionPlansAsync();
    Task<Category?> GetCategoryByNameAsync(string categoryName);
    Task<IEnumerable<SubscriptionPlan>> GetSubscriptionPlansByCategoryAsync(Guid categoryId);
    Task AddStatusHistoryAsync(SubscriptionStatusHistory statusHistory);
    Task<int> GetCountAsync();
    Task<IEnumerable<Subscription>> GetByCategoryIdAsync(Guid categoryId);
    
    // Analytics methods
    Task<IEnumerable<Subscription>> GetSubscriptionsCreatedInRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Subscription>> GetPausedSubscriptionsAsync();
    Task<IEnumerable<Subscription>> GetCancelledSubscriptionsInRangeAsync(DateTime startDate, DateTime endDate);
    
    // Additional methods needed by services
    Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId);
    Task<int> GetActiveSubscriptionsCountAsync();
    Task<int> GetCancelledSubscriptionsCountAsync();
    
    // Usage tracking methods
    Task<IEnumerable<Subscription>> GetSubscriptionsWithResetUsageAsync();
    Task ResetUsageCountersAsync();
    
    // Billing cycle methods
    Task<MasterBillingCycle?> GetBillingCycleByIdAsync(Guid billingCycleId);
} 
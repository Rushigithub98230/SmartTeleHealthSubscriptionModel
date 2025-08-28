using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionRepository : RepositoryBase<Subscription>, ISubscriptionRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Subscription>> GetAllAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetByUserIdAsync(int userId)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, TokenModel tokenModel)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
    }

    public async Task<IEnumerable<Subscription>> GetByStatusAsync(string status)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .Where(s => s.Status == "Active" || s.Status == "TrialActive")
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsDueForBillingAsync(DateTime billingDate)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .Where(s => s.Status == "Active" && s.NextBillingDate <= billingDate)
            .OrderBy(s => s.NextBillingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<Subscription> CreateAsync(Subscription subscription)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null)
            return false;

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Subscriptions.AnyAsync(s => s.Id == id);
    }

    public async Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(Guid id)
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.PlanPrivileges)
            .FirstOrDefaultAsync(sp => sp.Id == id);
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetAllSubscriptionPlansAsync()
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.PlanPrivileges)
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.DisplayOrder)
            .ToListAsync();
    }

    public async Task<SubscriptionPlan> CreateSubscriptionPlanAsync(SubscriptionPlan plan)
    {
        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<SubscriptionPlan> UpdateSubscriptionPlanAsync(SubscriptionPlan plan)
    {
        _context.SubscriptionPlans.Update(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<bool> DeleteSubscriptionPlanAsync(Guid id)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null)
            return false;

        _context.SubscriptionPlans.Remove(plan);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetActiveSubscriptionCountAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.Status == "Active" || s.Status == "TrialActive")
            .CountAsync();
    }

    public async Task<decimal> GetTotalMonthlyRevenueAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.Status == "Active" && s.CurrentPrice > 0)
            .SumAsync(s => s.CurrentPrice);
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsExpiringSoonAsync(int daysAhead)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "Active" && s.NextBillingDate <= expiryDate)
            .OrderBy(s => s.NextBillingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsWithFailedPaymentsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "PaymentFailed")
            .OrderByDescending(s => s.LastPaymentFailedDate)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Subscriptions.CountAsync();
    }

    public async Task<IEnumerable<Subscription>> GetByCategoryIdAsync(Guid categoryId)
    {
        // Since SubscriptionPlan doesn't have Category property, return all active subscriptions
        // This method can be enhanced later if categories are added to subscription plans
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "Active")
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetByPlanIdAsync(Guid planId)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.SubscriptionPlanId == planId)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetAllSubscriptionsAsync()
    {
        return await GetAllAsync();
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetActiveSubscriptionPlansAsync()
    {
        return await _context.SubscriptionPlans
            .Where(sp => sp.IsActive)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByNameAsync(string categoryName)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == categoryName);
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetSubscriptionPlansByCategoryAsync(Guid categoryId)
    {
        // Since SubscriptionPlan doesn't have Category property, return all active plans
        // This method can be enhanced later if categories are added
        return await _context.SubscriptionPlans
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.PlanPrivileges)
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.DisplayOrder)
            .ToListAsync();
    }

    public async Task AddStatusHistoryAsync(SubscriptionStatusHistory statusHistory)
    {
        await _context.SubscriptionStatusHistories.AddAsync(statusHistory);
        await _context.SaveChangesAsync();
    }

    // Analytics methods
    public async Task<IEnumerable<Subscription>> GetSubscriptionsCreatedInRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetPausedSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "Paused")
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetCancelledSubscriptionsInRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "Cancelled" && s.CancelledDate >= startDate && s.CancelledDate <= endDate)
            .OrderByDescending(s => s.CancelledDate)
            .ToListAsync();
    }

    // Additional methods needed by services
    public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .Where(s => s.UserId == userId && (s.Status == "Active" || s.Status == "TrialActive"))
            .OrderByDescending(s => s.CreatedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetActiveSubscriptionsCountAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.Status == "Active" || s.Status == "TrialActive")
            .CountAsync();
    }

    public async Task<int> GetCancelledSubscriptionsCountAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.Status == "Cancelled")
            .CountAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSuspendedSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "Suspended")
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    // Usage tracking methods
    public async Task<IEnumerable<Subscription>> GetSubscriptionsWithResetUsageAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "Active" && s.LastUsedDate.HasValue && 
                       s.LastUsedDate.Value.AddDays(30) <= DateTime.UtcNow)
            .OrderBy(s => s.LastUsedDate)
            .ToListAsync();
    }

    public async Task ResetUsageCountersAsync()
    {
        var subscriptions = await _context.Subscriptions
            .Where(s => s.Status == "Active" && s.LastUsedDate.HasValue && 
                       s.LastUsedDate.Value.AddDays(30) <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var subscription in subscriptions)
        {
            subscription.TotalUsageCount = 0;
            subscription.LastUsedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
} 
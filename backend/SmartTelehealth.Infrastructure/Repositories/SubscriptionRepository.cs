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

    // Custom methods with different names to avoid overriding base methods
    public async Task<Subscription?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Subscription>> GetAllWithDetailsAsync()
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

    public async Task<(IEnumerable<Subscription> subscriptions, int totalCount)> GetUserSubscriptionsWithFilteringAsync(int userId, SubscriptionFilterDto filter)
    {
        var query = _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Where(s => s.UserId == userId);

        // Apply search term filter
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(s => 
                s.SubscriptionPlan.Name.Contains(filter.SearchTerm) ||
                s.SubscriptionPlan.Description.Contains(filter.SearchTerm) ||
                s.Status.Contains(filter.SearchTerm));
        }

        // Apply status filter
        if (filter.Statuses != null && filter.Statuses.Any())
        {
            query = query.Where(s => filter.Statuses.Contains(s.Status));
        }

        // Apply date range filter
        if (filter.CreatedDateFrom.HasValue)
        {
            query = query.Where(s => s.CreatedDate >= filter.CreatedDateFrom.Value);
        }
        if (filter.CreatedDateTo.HasValue)
        {
            query = query.Where(s => s.CreatedDate <= filter.CreatedDateTo.Value);
        }

        // Apply plan filter
        if (filter.PlanIds != null && filter.PlanIds.Any())
        {
            query = query.Where(s => filter.PlanIds.Contains(s.SubscriptionPlanId));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, filter.SortColumn, filter.SortOrder);

        // Apply pagination
        var subscriptions = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (subscriptions, totalCount);
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

    public async Task<(IEnumerable<Subscription> subscriptions, int totalCount)> GetActiveSubscriptionsWithFilteringAsync(SubscriptionFilterDto filter)
    {
        var query = _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .Where(s => s.Status == "Active" || s.Status == "TrialActive");

        // Apply search term filter
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(s => 
                s.User.FirstName.Contains(filter.SearchTerm) ||
                s.User.LastName.Contains(filter.SearchTerm) ||
                s.SubscriptionPlan.Name.Contains(filter.SearchTerm) ||
                s.SubscriptionPlan.Description.Contains(filter.SearchTerm));
        }

        // Apply date range filter
        if (filter.CreatedDateFrom.HasValue)
        {
            query = query.Where(s => s.CreatedDate >= filter.CreatedDateFrom.Value);
        }
        if (filter.CreatedDateTo.HasValue)
        {
            query = query.Where(s => s.CreatedDate <= filter.CreatedDateTo.Value);
        }

        // Apply plan filter
        if (filter.PlanIds != null && filter.PlanIds.Any())
        {
            query = query.Where(s => filter.PlanIds.Contains(s.SubscriptionPlanId));
        }

        // Apply user filter
        if (filter.UserIds != null && filter.UserIds.Any())
        {
            query = query.Where(s => filter.UserIds.Contains(s.UserId));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, filter.SortColumn, filter.SortOrder);

        // Apply pagination
        var subscriptions = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (subscriptions, totalCount);
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

    // Note: CreateAsync, UpdateAsync, DeleteAsync are inherited from RepositoryBase<Subscription>
    // These methods handle audit properties automatically when called from the service layer

    public async Task<bool> ExistsSubscriptionAsync(Guid id)
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
            .Include(sp => sp.Currency)
            .Include(sp => sp.Category)
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

    public async Task<IEnumerable<Subscription>> GetSubscriptionsDueForRenewalAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "Active" && 
                       s.EndDate.HasValue && 
                       s.EndDate.Value <= DateTime.UtcNow.AddDays(7))
            .OrderBy(s => s.EndDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsWithExpiredTrialsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "TrialActive" && 
                       s.TrialEndDate.HasValue && 
                       s.TrialEndDate.Value <= DateTime.UtcNow)
            .OrderBy(s => s.TrialEndDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsWithUpcomingRenewalsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.User)
            .Where(s => s.Status == "Active" && 
                       s.NextBillingDate <= DateTime.UtcNow.AddDays(3))
            .OrderBy(s => s.NextBillingDate)
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
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.Currency)
            .Include(sp => sp.Category)
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByNameAsync(string categoryName)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == categoryName);
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetSubscriptionPlansByCategoryAsync(Guid categoryId)
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.Currency)
            .Include(sp => sp.Category)
            .Include(sp => sp.PlanPrivileges)
            .Where(sp => sp.IsActive && sp.CategoryId == categoryId)
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

    public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsByPlanIdAsync(Guid planId)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Where(s => s.SubscriptionPlanId == planId && 
                       s.Status == Subscription.SubscriptionStatuses.Active)
            .ToListAsync();
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

    // Billing cycle methods
    public async Task<MasterBillingCycle?> GetBillingCycleByIdAsync(Guid billingCycleId)
    {
        return await _context.MasterBillingCycles
            .FirstOrDefaultAsync(bc => bc.Id == billingCycleId);
    }

    public async Task<IEnumerable<MasterBillingCycle>> GetAllBillingCyclesAsync()
    {
        return await _context.MasterBillingCycles
            .Where(bc => bc.IsActive)
            .OrderBy(bc => bc.DurationInDays)
            .ToListAsync();
    }

    // Currency methods
    public async Task<MasterCurrency?> GetCurrencyByIdAsync(Guid currencyId)
    {
        return await _context.MasterCurrencies
            .FirstOrDefaultAsync(c => c.Id == currencyId);
    }

    /// <summary>
    /// Retrieves subscriptions with comprehensive filtering using filter DTO
    /// </summary>
    public async Task<(IEnumerable<Subscription> Subscriptions, int TotalCount)> GetSubscriptionsWithAdvancedFilteringAsync(SubscriptionFilterDto filter)
    {
        var query = _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
                .ThenInclude(sp => sp.Category)
            .Include(s => s.SubscriptionPlan)
                .ThenInclude(sp => sp.Currency)
            .Include(s => s.BillingCycle)
            .Include(s => s.User)
            .Include(s => s.StatusHistory)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(s => 
                s.SubscriptionPlan.Name.ToLower().Contains(term) ||
                s.User.Email.ToLower().Contains(term) ||
                s.Status.ToLower().Contains(term) ||
                (s.StripeSubscriptionId != null && s.StripeSubscriptionId.ToLower().Contains(term)));
        }

        // Apply ID filters
        if (filter.SubscriptionId.HasValue)
        {
            query = query.Where(s => s.Id == filter.SubscriptionId.Value);
        }

        if (filter.PlanId.HasValue)
        {
            query = query.Where(s => s.SubscriptionPlanId == filter.PlanId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.PlanName))
        {
            var planName = filter.PlanName.ToLower();
            query = query.Where(s => s.SubscriptionPlan.Name.ToLower().Contains(planName));
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(s => s.UserId == filter.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.UserEmail))
        {
            var email = filter.UserEmail.ToLower();
            query = query.Where(s => s.User.Email.ToLower().Contains(email));
        }

        // Apply status filters
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(s => s.Status == filter.Status);
        }

        if (filter.Statuses != null && filter.Statuses.Any())
        {
            query = query.Where(s => filter.Statuses.Contains(s.Status));
        }

        // Apply boolean status filters
        if (filter.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == filter.IsActive.Value);
        }

        if (filter.IsTrial.HasValue)
        {
            query = query.Where(s => s.IsTrialSubscription == filter.IsTrial.Value);
        }

        if (filter.IsPaused.HasValue)
        {
            query = query.Where(s => s.IsPaused == filter.IsPaused.Value);
        }

        if (filter.IsCancelled.HasValue)
        {
            query = query.Where(s => s.IsCancelled == filter.IsCancelled.Value);
        }

        if (filter.IsExpired.HasValue)
        {
            query = query.Where(s => s.IsExpired == filter.IsExpired.Value);
        }

        // Apply amount filters
        if (filter.MinAmount.HasValue)
        {
            query = query.Where(s => s.Amount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(s => s.Amount <= filter.MaxAmount.Value);
        }

        if (filter.ExactAmount.HasValue)
        {
            query = query.Where(s => s.Amount == filter.ExactAmount.Value);
        }

        if (filter.CurrencyId.HasValue)
        {
            query = query.Where(s => s.SubscriptionPlan.CurrencyId == filter.CurrencyId.Value);
        }

        // Apply billing cycle filters
        if (filter.BillingCycleId.HasValue)
        {
            query = query.Where(s => s.BillingCycleId == filter.BillingCycleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.BillingCycleName))
        {
            var cycleName = filter.BillingCycleName.ToLower();
            query = query.Where(s => s.BillingCycle.Name.ToLower().Contains(cycleName));
        }

        // Apply date range filters
        if (filter.CreatedDateFrom.HasValue)
        {
            query = query.Where(s => s.CreatedDate >= filter.CreatedDateFrom.Value);
        }

        if (filter.CreatedDateTo.HasValue)
        {
            query = query.Where(s => s.CreatedDate <= filter.CreatedDateTo.Value);
        }

        if (filter.UpdatedDateFrom.HasValue)
        {
            query = query.Where(s => s.UpdatedDate >= filter.UpdatedDateFrom.Value);
        }

        if (filter.UpdatedDateTo.HasValue)
        {
            query = query.Where(s => s.UpdatedDate <= filter.UpdatedDateTo.Value);
        }

        if (filter.StartDateFrom.HasValue)
        {
            query = query.Where(s => s.StartDate >= filter.StartDateFrom.Value);
        }

        if (filter.StartDateTo.HasValue)
        {
            query = query.Where(s => s.StartDate <= filter.StartDateTo.Value);
        }

        if (filter.EndDateFrom.HasValue)
        {
            query = query.Where(s => s.EndDate >= filter.EndDateFrom.Value);
        }

        if (filter.EndDateTo.HasValue)
        {
            query = query.Where(s => s.EndDate <= filter.EndDateTo.Value);
        }

        if (filter.NextBillingDateFrom.HasValue)
        {
            query = query.Where(s => s.NextBillingDate >= filter.NextBillingDateFrom.Value);
        }

        if (filter.NextBillingDateTo.HasValue)
        {
            query = query.Where(s => s.NextBillingDate <= filter.NextBillingDateTo.Value);
        }

        if (filter.LastBillingDateFrom.HasValue)
        {
            query = query.Where(s => s.LastBillingDate >= filter.LastBillingDateFrom.Value);
        }

        if (filter.LastBillingDateTo.HasValue)
        {
            query = query.Where(s => s.LastBillingDate <= filter.LastBillingDateTo.Value);
        }

        // Apply trial duration filters
        if (filter.MinTrialDays.HasValue)
        {
            query = query.Where(s => s.TrialDurationInDays >= filter.MinTrialDays.Value);
        }

        if (filter.MaxTrialDays.HasValue)
        {
            query = query.Where(s => s.TrialDurationInDays <= filter.MaxTrialDays.Value);
        }

        // Apply billing interval filters - Note: BillingInterval property doesn't exist in Subscription entity
        // These filters are commented out as they're not applicable to the current entity structure
        // if (filter.MinBillingInterval.HasValue)
        // {
        //     query = query.Where(s => s.BillingInterval >= filter.MinBillingInterval.Value);
        // }

        // if (filter.MaxBillingInterval.HasValue)
        // {
        //     query = query.Where(s => s.BillingInterval <= filter.MaxBillingInterval.Value);
        // }

        // Apply Stripe integration filters
        if (!string.IsNullOrWhiteSpace(filter.StripeSubscriptionId))
        {
            query = query.Where(s => s.StripeSubscriptionId == filter.StripeSubscriptionId);
        }

        if (!string.IsNullOrWhiteSpace(filter.StripeCustomerId))
        {
            query = query.Where(s => s.StripeCustomerId == filter.StripeCustomerId);
        }

        if (filter.HasStripeIntegration.HasValue)
        {
            if (filter.HasStripeIntegration.Value)
            {
                query = query.Where(s => !string.IsNullOrEmpty(s.StripeSubscriptionId));
            }
            else
            {
                query = query.Where(s => string.IsNullOrEmpty(s.StripeSubscriptionId));
            }
        }

        // Apply list filters
        if (filter.SubscriptionIds != null && filter.SubscriptionIds.Any())
        {
            query = query.Where(s => filter.SubscriptionIds.Contains(s.Id));
        }

        if (filter.ExcludeSubscriptionIds != null && filter.ExcludeSubscriptionIds.Any())
        {
            query = query.Where(s => !filter.ExcludeSubscriptionIds.Contains(s.Id));
        }

        if (filter.PlanIds != null && filter.PlanIds.Any())
        {
            query = query.Where(s => filter.PlanIds.Contains(s.SubscriptionPlanId));
        }

        if (filter.UserIds != null && filter.UserIds.Any())
        {
            query = query.Where(s => filter.UserIds.Contains(s.UserId));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply dynamic sorting
        query = ApplySorting(query, filter.SortColumn, filter.SortOrder);

        // Apply pagination
        var skip = (filter.Page - 1) * filter.PageSize;
        var subscriptions = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .ToListAsync();

        return (subscriptions, totalCount);
    }

    private static IQueryable<Subscription> ApplySorting(IQueryable<Subscription> query, string? sortColumn, string? sortOrder)
    {
        // Default sorting if parameters are null or empty
        if (string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortOrder))
        {
            return query.OrderByDescending(s => s.CreatedDate);
        }

        return sortColumn.ToLower() switch
        {
            "createddate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.CreatedDate)
                : query.OrderBy(s => s.CreatedDate),
            "updateddate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.UpdatedDate)
                : query.OrderBy(s => s.UpdatedDate),
            "startdate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.StartDate)
                : query.OrderBy(s => s.StartDate),
            "enddate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.EndDate)
                : query.OrderBy(s => s.EndDate),
            "nextbillingdate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.NextBillingDate)
                : query.OrderBy(s => s.NextBillingDate),
            "lastbillingdate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.LastBillingDate)
                : query.OrderBy(s => s.LastBillingDate),
            "amount" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.Amount)
                : query.OrderBy(s => s.Amount),
            "status" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.Status)
                : query.OrderBy(s => s.Status),
            "planname" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.SubscriptionPlan.Name)
                : query.OrderBy(s => s.SubscriptionPlan.Name),
            "useremail" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.User.Email)
                : query.OrderBy(s => s.User.Email),
            "trialdays" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(s => s.TrialDurationInDays)
                : query.OrderBy(s => s.TrialDurationInDays),
            // "billinginterval" => sortOrder.ToLower() == "desc" 
            //     ? query.OrderByDescending(s => s.BillingInterval)
            //     : query.OrderBy(s => s.BillingInterval),
            _ => query.OrderByDescending(s => s.CreatedDate)
        };
    }
    
    /// <summary>
    /// Gets all privilege usage records for a subscription
    /// </summary>
    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetSubscriptionPrivilegeUsagesAsync(Guid subscriptionId)
    {
        return await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
                .ThenInclude(p => p.Privilege)
            .Where(u => u.SubscriptionId == subscriptionId)
            .ToListAsync();
    }
    
    /// <summary>
    /// Updates a privilege usage record
    /// </summary>
    public async Task UpdatePrivilegeUsageAsync(UserSubscriptionPrivilegeUsage usage)
    {
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Get all subscriptions for a user (for analytics)
    /// </summary>
    public async Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(int userId)
    {
        return await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Include(s => s.Currency)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }
    
    /// <summary>
    /// Get all billing records for a user (for analytics)
    /// </summary>
    public async Task<IEnumerable<BillingRecord>> GetBillingRecordsByUserIdAsync(int userId)
    {
        return await _context.BillingRecords
            .Include(b => b.Subscription)
            .Include(b => b.Currency)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BillingDate)
            .ToListAsync();
    }
    
    /// <summary>
    /// Get all payments for a user (for analytics)
    /// </summary>
    public async Task<IEnumerable<SubscriptionPayment>> GetPaymentsByUserIdAsync(int userId)
    {
        return await _context.SubscriptionPayments
            .Include(p => p.Subscription)
            .Include(p => p.BillingRecord)
            .Include(p => p.Currency)
            .Where(p => p.Subscription != null && p.Subscription.UserId == userId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }
    
    /// <summary>
    /// Get user subscription privilege usages for a subscription (for analytics)
    /// </summary>
    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetUserSubscriptionPrivilegeUsagesAsync(Guid subscriptionId)
    {
        return await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.Subscription)
            .Include(u => u.Privilege)
            .Include(u => u.SubscriptionPlanPrivilege)
                .ThenInclude(p => p.Privilege)
            .Where(u => u.SubscriptionId == subscriptionId)
            .ToListAsync();
    }

    // Additional analytics methods
    public async Task<int> GetNewSubscriptionsCountAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
            .CountAsync();
    }

    public async Task<int> GetTrialsEndingCountAsync(DateTime endDate)
    {
        return await _context.Subscriptions
            .Where(s => s.IsInTrial && s.TrialEndDate.HasValue && s.TrialEndDate.Value <= endDate)
            .CountAsync();
    }
} 
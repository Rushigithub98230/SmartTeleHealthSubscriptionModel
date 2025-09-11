using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for subscription plan data access operations.
/// This repository handles all database operations related to subscription plans,
/// extending the base repository functionality with plan-specific operations.
/// </summary>
public class SubscriptionPlanRepository : RepositoryBase<SubscriptionPlan>, ISubscriptionPlanRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the SubscriptionPlanRepository
    /// </summary>
    /// <param name="context">The database context for data access</param>
    public SubscriptionPlanRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Basic CRUD Operations

    /// <summary>
    /// Retrieves a subscription plan by its unique identifier with related entities
    /// </summary>
    public override async Task<SubscriptionPlan?> GetByIdAsync(object id)
    {
        if (id is not Guid planId)
            return null;

        return await GetByIdAsync(planId);
    }

    /// <summary>
    /// Retrieves a subscription plan by its unique identifier with related entities
    /// </summary>
    public async Task<SubscriptionPlan?> GetByIdAsync(Guid id)
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.PlanPrivileges)
                .ThenInclude(spp => spp.Privilege)
            .Include(sp => sp.Subscriptions)
            .FirstOrDefaultAsync(sp => sp.Id == id);
    }

    /// <summary>
    /// Retrieves all subscription plans with related entities
    /// </summary>
    public override async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.PlanPrivileges)
                .ThenInclude(spp => spp.Privilege)
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes a subscription plan by its unique identifier
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null)
                return false;

            _context.SubscriptionPlans.Remove(plan);
        await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Status Management Operations

    /// <summary>
    /// Activates a subscription plan
    /// </summary>
    public async Task<bool> ActivateAsync(Guid id)
    {
        try
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null)
                return false;

            plan.IsActive = true;
        plan.UpdatedDate = DateTime.UtcNow;
            
            _context.Entry(plan).State = EntityState.Modified;
        await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deactivates a subscription plan
    /// </summary>
    public async Task<bool> DeactivateAsync(Guid id)
    {
        try
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null)
                return false;

            plan.IsActive = false;
            plan.UpdatedDate = DateTime.UtcNow;
            
            _context.Entry(plan).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Query Operations

    /// <summary>
    /// Retrieves all active subscription plans
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync()
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves subscription plans by category
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlan>> GetPlansByCategoryAsync(Guid categoryId)
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Where(sp => sp.CategoryId == categoryId)
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Searches subscription plans by name or description
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlan>> SearchPlansAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var term = searchTerm.ToLower();
        return await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Where(sp => sp.Name.ToLower().Contains(term) || 
                        (sp.Description != null && sp.Description.ToLower().Contains(term)))
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves subscription plans with comprehensive filtering, pagination, and sorting
    /// </summary>
    public async Task<(IEnumerable<SubscriptionPlan> Plans, int TotalCount)> GetPlansWithPaginationAsync(
        int page = 1, 
        int pageSize = 50, 
        string? searchTerm = null, 
        string? categoryId = null, 
        bool? isActive = null,
        string? sortColumn = "DisplayOrder",
        string? sortOrder = "asc")
    {
        var query = _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.PlanPrivileges)
                .ThenInclude(spp => spp.Privilege)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(sp => 
                sp.Name.ToLower().Contains(term) || 
                (sp.Description != null && sp.Description.ToLower().Contains(term)) ||
                (sp.ShortDescription != null && sp.ShortDescription.ToLower().Contains(term)));
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(categoryId) && Guid.TryParse(categoryId, out var categoryGuid))
        {
            query = query.Where(sp => sp.CategoryId == categoryGuid);
        }

        // Apply active status filter
        if (isActive.HasValue)
        {
            query = query.Where(sp => sp.IsActive == isActive.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply dynamic sorting
        query = ApplySorting(query, sortColumn, sortOrder);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var plans = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (plans, totalCount);
    }

    /// <summary>
    /// Applies dynamic sorting to the query
    /// </summary>
    private static IQueryable<SubscriptionPlan> ApplySorting(IQueryable<SubscriptionPlan> query, string sortColumn, string sortOrder)
    {
        return sortColumn.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.Name)
                : query.OrderBy(sp => sp.Name),
            "price" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.Price)
                : query.OrderBy(sp => sp.Price),
            "createddate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.CreatedDate)
                : query.OrderBy(sp => sp.CreatedDate),
            "updateddate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.UpdatedDate)
                : query.OrderBy(sp => sp.UpdatedDate),
            "isactive" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.IsActive)
                : query.OrderBy(sp => sp.IsActive),
            "displayorder" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.DisplayOrder)
                : query.OrderBy(sp => sp.DisplayOrder),
            _ => query.OrderBy(sp => sp.DisplayOrder).ThenBy(sp => sp.Name)
        };
    }

    #endregion

    #region Analytics and Reporting Operations

    /// <summary>
    /// Retrieves subscription plan statistics
    /// </summary>
    public async Task<object> GetPlanStatisticsAsync()
    {
        var totalPlans = await _context.SubscriptionPlans.CountAsync();
        var activePlans = await _context.SubscriptionPlans.CountAsync(sp => sp.IsActive);
        var inactivePlans = totalPlans - activePlans;
        var plansWithTrials = await _context.SubscriptionPlans.CountAsync(sp => sp.IsTrialAllowed);
        
        var averagePrice = await _context.SubscriptionPlans
            .Where(sp => sp.IsActive)
            .AverageAsync(sp => sp.Price);

        var plansByCategory = await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .GroupBy(sp => sp.Category != null ? sp.Category.Name : "Uncategorized")
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        return new
        {
            TotalPlans = totalPlans,
            ActivePlans = activePlans,
            InactivePlans = inactivePlans,
            PlansWithTrials = plansWithTrials,
            AveragePrice = averagePrice,
            PlansByCategory = plansByCategory
        };
    }

    /// <summary>
    /// Retrieves subscription plans created within a date range
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlan>> GetPlansByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Where(sp => sp.CreatedDate >= startDate && sp.CreatedDate <= endDate)
            .OrderBy(sp => sp.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves subscription plans by billing cycle
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlan>> GetPlansByBillingCycleAsync(Guid billingCycleId)
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Where(sp => sp.BillingCycleId == billingCycleId)
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves subscription plans by currency
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlan>> GetPlansByCurrencyAsync(Guid currencyId)
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Where(sp => sp.CurrencyId == currencyId)
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToListAsync();
    }

    #endregion

    #region Validation Operations

    /// <summary>
    /// Checks if a subscription plan exists
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.SubscriptionPlans.AnyAsync(sp => sp.Id == id);
    }

    /// <summary>
    /// Checks if a subscription plan name is unique
    /// </summary>
    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = _context.SubscriptionPlans.Where(sp => sp.Name == name);
        
        if (excludeId.HasValue)
        {
            query = query.Where(sp => sp.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <summary>
    /// Checks if a subscription plan has active subscriptions
    /// </summary>
    public async Task<bool> HasActiveSubscriptionsAsync(Guid id)
    {
        return await _context.Subscriptions
            .AnyAsync(s => s.SubscriptionPlanId == id && s.Status == "Active");
    }

    #endregion
} 
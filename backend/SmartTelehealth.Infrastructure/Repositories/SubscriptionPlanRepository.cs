using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.DTOs;
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

    // Custom methods with different names to avoid overriding base methods
    /// <summary>
    /// Retrieves a subscription plan by its unique identifier with related entities
    /// </summary>
    public async Task<SubscriptionPlan?> GetByIdWithDetailsAsync(Guid id)
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
    public async Task<IEnumerable<SubscriptionPlan>> GetAllWithDetailsAsync()
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
    /// Creates a new subscription plan
    /// </summary>
    public async Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan)
    {
        return await base.CreateAsync(plan);
    }

    /// <summary>
    /// Updates an existing subscription plan
    /// </summary>
    public async Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan plan)
    {
        return await base.UpdateAsync(plan);
    }

    /// <summary>
    /// Deletes a subscription plan by its unique identifier
    /// </summary>
    public async Task<bool> DeletePlanAsync(Guid id)
    {
        try
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null)
                return false;

            plan.IsActive = false;
            plan.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a subscription plan exists
    /// </summary>
    public async Task<bool> ExistsPlanAsync(Guid id)
    {
        return await _context.SubscriptionPlans.AnyAsync(sp => sp.Id == id);
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
    /// Retrieves subscription plans with comprehensive filtering using filter DTO
    /// </summary>
    public async Task<(IEnumerable<SubscriptionPlan> Plans, int TotalCount)> GetPlansWithAdvancedFilteringAsync(SubscriptionPlanFilterDto filter)
    {
        var query = _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Include(sp => sp.Currency)
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.PlanPrivileges)
                .ThenInclude(spp => spp.Privilege)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(sp => 
                sp.Name.ToLower().Contains(term) || 
                (sp.Description != null && sp.Description.ToLower().Contains(term)) ||
                (sp.ShortDescription != null && sp.ShortDescription.ToLower().Contains(term)));
        }

        // Apply category filters
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(sp => sp.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.CategoryName))
        {
            var categoryName = filter.CategoryName.ToLower();
            query = query.Where(sp => sp.Category != null && sp.Category.Name.ToLower().Contains(categoryName));
        }

        // Apply status filters
        if (filter.IsActive.HasValue)
        {
            query = query.Where(sp => sp.IsActive == filter.IsActive.Value);
        }

        if (filter.IsFeatured.HasValue)
        {
            query = query.Where(sp => sp.IsFeatured == filter.IsFeatured.Value);
        }

        if (filter.IsMostPopular.HasValue)
        {
            query = query.Where(sp => sp.IsMostPopular == filter.IsMostPopular.Value);
        }

        if (filter.IsTrending.HasValue)
        {
            query = query.Where(sp => sp.IsTrending == filter.IsTrending.Value);
        }

        if (filter.IsTrialAllowed.HasValue)
        {
            query = query.Where(sp => sp.IsTrialAllowed == filter.IsTrialAllowed.Value);
        }

        // Apply pricing filters
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(sp => sp.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(sp => sp.Price <= filter.MaxPrice.Value);
        }

        if (filter.ExactPrice.HasValue)
        {
            query = query.Where(sp => sp.Price == filter.ExactPrice.Value);
        }

        if (filter.CurrencyId.HasValue)
        {
            query = query.Where(sp => sp.CurrencyId == filter.CurrencyId.Value);
        }

        // Apply billing cycle filters
        if (filter.BillingCycleId.HasValue)
        {
            query = query.Where(sp => sp.BillingCycleId == filter.BillingCycleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.BillingCycleName))
        {
            var cycleName = filter.BillingCycleName.ToLower();
            query = query.Where(sp => sp.BillingCycle != null && sp.BillingCycle.Name.ToLower().Contains(cycleName));
        }

        // Apply date range filters
        if (filter.CreatedDateFrom.HasValue)
        {
            query = query.Where(sp => sp.CreatedDate >= filter.CreatedDateFrom.Value);
        }

        if (filter.CreatedDateTo.HasValue)
        {
            query = query.Where(sp => sp.CreatedDate <= filter.CreatedDateTo.Value);
        }

        if (filter.UpdatedDateFrom.HasValue)
        {
            query = query.Where(sp => sp.UpdatedDate >= filter.UpdatedDateFrom.Value);
        }

        if (filter.UpdatedDateTo.HasValue)
        {
            query = query.Where(sp => sp.UpdatedDate <= filter.UpdatedDateTo.Value);
        }

        if (filter.EffectiveDateFrom.HasValue)
        {
            query = query.Where(sp => sp.EffectiveDate >= filter.EffectiveDateFrom.Value);
        }

        if (filter.EffectiveDateTo.HasValue)
        {
            query = query.Where(sp => sp.EffectiveDate <= filter.EffectiveDateTo.Value);
        }

        // Apply trial duration filters
        if (filter.MinTrialDuration.HasValue)
        {
            query = query.Where(sp => sp.TrialDurationInDays >= filter.MinTrialDuration.Value);
        }

        if (filter.MaxTrialDuration.HasValue)
        {
            query = query.Where(sp => sp.TrialDurationInDays <= filter.MaxTrialDuration.Value);
        }

        // Apply display order filters
        if (filter.MinDisplayOrder.HasValue)
        {
            query = query.Where(sp => sp.DisplayOrder >= filter.MinDisplayOrder.Value);
        }

        if (filter.MaxDisplayOrder.HasValue)
        {
            query = query.Where(sp => sp.DisplayOrder <= filter.MaxDisplayOrder.Value);
        }

        // Apply Stripe integration filters
        if (!string.IsNullOrWhiteSpace(filter.StripeProductId))
        {
            query = query.Where(sp => sp.StripeProductId == filter.StripeProductId);
        }

        if (filter.HasStripeIntegration.HasValue)
        {
            if (filter.HasStripeIntegration.Value)
            {
                query = query.Where(sp => !string.IsNullOrEmpty(sp.StripeProductId));
            }
            else
            {
                query = query.Where(sp => string.IsNullOrEmpty(sp.StripeProductId));
            }
        }

        // Apply plan ID filters
        if (filter.PlanIds != null && filter.PlanIds.Any())
        {
            query = query.Where(sp => filter.PlanIds.Contains(sp.Id));
        }

        if (filter.ExcludePlanIds != null && filter.ExcludePlanIds.Any())
        {
            query = query.Where(sp => !filter.ExcludePlanIds.Contains(sp.Id));
        }

        // Apply subscription-related filters
        if (filter.HasActiveSubscriptions.HasValue)
        {
            if (filter.HasActiveSubscriptions.Value)
            {
                query = query.Where(sp => sp.Subscriptions.Any(s => s.Status == "Active"));
            }
            else
            {
                query = query.Where(sp => !sp.Subscriptions.Any(s => s.Status == "Active"));
            }
        }

        if (filter.HasSubscriptions.HasValue)
        {
            if (filter.HasSubscriptions.Value)
            {
                query = query.Where(sp => sp.Subscriptions.Any());
            }
            else
            {
                query = query.Where(sp => !sp.Subscriptions.Any());
            }
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply dynamic sorting
        query = ApplySorting(query, filter.SortColumn, filter.SortOrder);

        // Apply pagination
        var skip = (filter.Page - 1) * filter.PageSize;
        var plans = await query
            .Skip(skip)
            .Take(filter.PageSize)
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
            "isfeatured" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.IsFeatured)
                : query.OrderBy(sp => sp.IsFeatured),
            "ismostpopular" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.IsMostPopular)
                : query.OrderBy(sp => sp.IsMostPopular),
            "istrending" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.IsTrending)
                : query.OrderBy(sp => sp.IsTrending),
            "trialduration" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.TrialDurationInDays)
                : query.OrderBy(sp => sp.TrialDurationInDays),
            "effectivedate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(sp => sp.EffectiveDate)
                : query.OrderBy(sp => sp.EffectiveDate),
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

    /// <summary>
    /// Gets all privileges associated with a subscription plan
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlanPrivilege>> GetPlanPrivilegesAsync(Guid planId)
    {
        return await _context.SubscriptionPlanPrivileges
            .Include(spp => spp.Privilege)
            .Where(spp => spp.SubscriptionPlanId == planId && !spp.IsDeleted)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific plan privilege configuration
    /// </summary>
    public async Task<SubscriptionPlanPrivilege?> GetPlanPrivilegeAsync(Guid planId, Guid privilegeId)
    {
        return await _context.SubscriptionPlanPrivileges
            .Include(spp => spp.Privilege)
            .FirstOrDefaultAsync(spp => spp.SubscriptionPlanId == planId && 
                                       spp.PrivilegeId == privilegeId && 
                                       !spp.IsDeleted);
    }

    #endregion
    
    #region Plan Versioning Operations (Healthcare-Specific)
    
    /// <summary>
    /// Gets the latest version of a plan by its parent/original plan ID.
    /// Healthcare Feature: Supports plan versioning for price changes.
    /// </summary>
    public async Task<SubscriptionPlan?> GetLatestVersionOfPlanAsync(Guid planIdOrParentId)
    {
        // Find if this is a parent plan or a child plan
        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == planIdOrParentId);
        
        if (plan == null) return null;
        
        // Get the root parent plan ID
        var parentId = plan.ParentPlanId ?? plan.Id;
        
        // Get the latest version from the plan family
        return await _context.SubscriptionPlans
            .Include(sp => sp.PlanPrivileges)
                .ThenInclude(pp => pp.Privilege)
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.Currency)
            .Include(sp => sp.Category)
            .Where(p => (p.Id == parentId || p.ParentPlanId == parentId) && p.IsLatestVersion)
            .OrderByDescending(p => p.VersionNumber)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Gets all versions of a plan (including parent).
    /// Healthcare Feature: View complete plan version history.
    /// </summary>
    public async Task<IEnumerable<SubscriptionPlan>> GetAllVersionsOfPlanAsync(Guid planIdOrParentId)
    {
        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == planIdOrParentId);
        
        if (plan == null) return Enumerable.Empty<SubscriptionPlan>();
        
        // Get the root parent plan ID
        var parentId = plan.ParentPlanId ?? plan.Id;
        
        // Get all versions in the plan family
        return await _context.SubscriptionPlans
            .Include(sp => sp.PlanPrivileges)
                .ThenInclude(pp => pp.Privilege)
            .Include(sp => sp.BillingCycle)
            .Include(sp => sp.Currency)
            .Include(sp => sp.Category)
            .Where(p => p.Id == parentId || p.ParentPlanId == parentId)
            .OrderBy(p => p.VersionNumber)
            .ToListAsync();
    }
    
    /// <summary>
    /// Creates a new version of an existing plan.
    /// Healthcare Feature: Marks previous versions as not latest and sets up new version.
    /// </summary>
    public async Task<SubscriptionPlan> CreateNewPlanVersionAsync(SubscriptionPlan newVersion)
    {
        // Mark all previous versions as not latest
        var parentId = newVersion.ParentPlanId ?? newVersion.Id;
        var previousVersions = await _context.SubscriptionPlans
            .Where(p => (p.Id == parentId || p.ParentPlanId == parentId) && p.IsLatestVersion)
            .ToListAsync();
        
        foreach (var prev in previousVersions)
        {
            prev.IsLatestVersion = false;
        }
        
        // Set new version properties
        newVersion.IsLatestVersion = true;
        newVersion.VersionCreatedDate = DateTime.UtcNow;
        
        // Add new version to context
        await _context.SubscriptionPlans.AddAsync(newVersion);
        await _context.SaveChangesAsync();
        
        return newVersion;
    }
    
    /// <summary>
    /// Gets count of active subscriptions for a plan.
    /// Healthcare Feature: Determine if plan version migration is needed.
    /// </summary>
    public async Task<int> GetActiveSubscriptionsCountAsync(Guid planId)
    {
        return await _context.Subscriptions
            .CountAsync(s => s.SubscriptionPlanId == planId && 
                            s.Status == Subscription.SubscriptionStatuses.Active);
    }
    
    #endregion
} 
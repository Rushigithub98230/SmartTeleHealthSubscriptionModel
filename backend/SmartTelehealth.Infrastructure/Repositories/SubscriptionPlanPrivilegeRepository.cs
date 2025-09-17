using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionPlanPrivilegeRepository : RepositoryBase<SubscriptionPlanPrivilege>, ISubscriptionPlanPrivilegeRepository
{
    private readonly ApplicationDbContext _context;
    public SubscriptionPlanPrivilegeRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a subscription plan privilege by its unique identifier
    /// </summary>
    public override async Task<SubscriptionPlanPrivilege?> GetByIdAsync(object id)
    {
        if (id is not Guid privilegeId)
            return null;

        return await _context.SubscriptionPlanPrivileges.FindAsync(privilegeId);
    }

    public async Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPlanIdAsync(Guid planId)
        => await _context.SubscriptionPlanPrivileges
            .Where(x => x.SubscriptionPlanId == planId)
            .ToListAsync();

    public async Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPrivilegeIdAsync(Guid privilegeId)
        => await _context.SubscriptionPlanPrivileges.Where(x => x.PrivilegeId == privilegeId).ToListAsync();

    /// <summary>
    /// Retrieves all subscription plan privileges
    /// </summary>
    public override async Task<IEnumerable<SubscriptionPlanPrivilege>> GetAllAsync()
    {
        return await _context.SubscriptionPlanPrivileges.ToListAsync();
    }

    /// <summary>
    /// Creates a new subscription plan privilege
    /// </summary>
    public override async Task<SubscriptionPlanPrivilege> CreateAsync(SubscriptionPlanPrivilege planPrivilege)
    {
        planPrivilege.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(planPrivilege);
    }

    /// <summary>
    /// Updates an existing subscription plan privilege
    /// </summary>
    public override async Task<SubscriptionPlanPrivilege> UpdateAsync(SubscriptionPlanPrivilege planPrivilege)
    {
        planPrivilege.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(planPrivilege);
    }

    /// <summary>
    /// Deletes a subscription plan privilege by its unique identifier (hard delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid privilegeId)
            return false;

        var entity = await _context.SubscriptionPlanPrivileges.FindAsync(privilegeId);
        if (entity != null)
        {
            _context.SubscriptionPlanPrivileges.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a subscription plan privilege exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid privilegeId)
            return false;

        return await _context.SubscriptionPlanPrivileges.AnyAsync(x => x.Id == privilegeId);
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public async Task AddAsync(SubscriptionPlanPrivilege planPrivilege)
    {
        await CreateAsync(planPrivilege);
    }

    #region Advanced Query Operations

    /// <summary>
    /// Retrieves subscription plan privileges with comprehensive filtering, pagination, and sorting
    /// </summary>
    public async Task<(IEnumerable<SubscriptionPlanPrivilege> Privileges, int TotalCount)> GetPlanPrivilegesWithFilteringAsync(
        int page, int pageSize, Guid? planId = null, Guid? privilegeId = null, 
        string? search = null, bool? isActive = null, string? sortBy = "CreatedDate", string? sortOrder = "desc")
    {
        var query = _context.SubscriptionPlanPrivileges
            .Include(spp => spp.SubscriptionPlan)
            .Include(spp => spp.Privilege)
            .AsQueryable();

        // Apply filters
        if (planId.HasValue)
        {
            query = query.Where(spp => spp.SubscriptionPlanId == planId.Value);
        }

        if (privilegeId.HasValue)
        {
            query = query.Where(spp => spp.PrivilegeId == privilegeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(spp => 
                spp.Privilege.Name.ToLower().Contains(term) ||
                (spp.Privilege.Description != null && spp.Privilege.Description.ToLower().Contains(term)) ||
                spp.SubscriptionPlan.Name.ToLower().Contains(term));
        }

        if (isActive.HasValue)
        {
            query = query.Where(spp => spp.IsActive == isActive.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var privileges = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (privileges, totalCount);
    }

    /// <summary>
    /// Applies dynamic sorting to the query
    /// </summary>
    private static IQueryable<SubscriptionPlanPrivilege> ApplySorting(IQueryable<SubscriptionPlanPrivilege> query, string sortBy, string sortOrder)
    {
        return sortBy.ToLower() switch
        {
            "privilegename" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(spp => spp.Privilege.Name)
                : query.OrderBy(spp => spp.Privilege.Name),
            "planname" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(spp => spp.SubscriptionPlan.Name)
                : query.OrderBy(spp => spp.SubscriptionPlan.Name),
            "value" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(spp => spp.Value)
                : query.OrderBy(spp => spp.Value),
            "createddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(spp => spp.CreatedDate)
                : query.OrderBy(spp => spp.CreatedDate),
            "updateddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(spp => spp.UpdatedDate)
                : query.OrderBy(spp => spp.UpdatedDate),
            "isactive" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(spp => spp.IsActive)
                : query.OrderBy(spp => spp.IsActive),
            _ => query.OrderByDescending(spp => spp.CreatedDate)
        };
    }

    #endregion
} 
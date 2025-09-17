using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class UserSubscriptionPrivilegeUsageRepository : RepositoryBase<UserSubscriptionPrivilegeUsage>, IUserSubscriptionPrivilegeUsageRepository
{
    private readonly ApplicationDbContext _context;
    public UserSubscriptionPrivilegeUsageRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a user subscription privilege usage by its unique identifier
    /// </summary>
    public override async Task<UserSubscriptionPrivilegeUsage?> GetByIdAsync(object id)
    {
        if (id is not Guid usageId)
            return null;

        return await _context.UserSubscriptionPrivilegeUsages.FindAsync(usageId);
    }

    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId)
        => await _context.UserSubscriptionPrivilegeUsages.Where(x => x.SubscriptionId == subscriptionId).ToListAsync();

    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionPlanPrivilegeIdAsync(Guid subscriptionPlanPrivilegeId)
        => await _context.UserSubscriptionPrivilegeUsages.Where(x => x.SubscriptionPlanPrivilegeId == subscriptionPlanPrivilegeId).ToListAsync();

    /// <summary>
    /// Retrieves all user subscription privilege usages
    /// </summary>
    public override async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetAllAsync()
    {
        return await _context.UserSubscriptionPrivilegeUsages.ToListAsync();
    }

    /// <summary>
    /// Creates a new user subscription privilege usage
    /// </summary>
    public override async Task<UserSubscriptionPrivilegeUsage> CreateAsync(UserSubscriptionPrivilegeUsage usage)
    {
        usage.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(usage);
    }

    /// <summary>
    /// Updates an existing user subscription privilege usage
    /// </summary>
    public override async Task<UserSubscriptionPrivilegeUsage> UpdateAsync(UserSubscriptionPrivilegeUsage usage)
    {
        usage.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(usage);
    }

    /// <summary>
    /// Deletes a user subscription privilege usage by its unique identifier (hard delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid usageId)
            return false;

        var entity = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usageId);
        if (entity != null)
        {
            _context.UserSubscriptionPrivilegeUsages.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a user subscription privilege usage exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid usageId)
            return false;

        return await _context.UserSubscriptionPrivilegeUsages.AnyAsync(x => x.Id == usageId);
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public async Task AddAsync(UserSubscriptionPrivilegeUsage usage)
    {
        await CreateAsync(usage);
    }

    #region Advanced Query Operations

    /// <summary>
    /// Retrieves user subscription privilege usages with comprehensive filtering, pagination, and sorting
    /// </summary>
    public async Task<(IEnumerable<UserSubscriptionPrivilegeUsage> Usages, int TotalCount)> GetUsagesWithFilteringAsync(
        int page, int pageSize, Guid? subscriptionId = null, Guid? privilegeId = null, 
        int? userId = null, string? search = null, bool? isActive = null, 
        DateTime? startDate = null, DateTime? endDate = null, string? sortBy = "LastUsedAt", string? sortOrder = "desc")
    {
        var query = _context.UserSubscriptionPrivilegeUsages
            .Include(uspu => uspu.Subscription)
                .ThenInclude(s => s.User)
            .Include(uspu => uspu.SubscriptionPlanPrivilege)
                .ThenInclude(spp => spp.Privilege)
            .Include(uspu => uspu.Privilege)
            .AsQueryable();

        // Apply filters
        if (subscriptionId.HasValue)
        {
            query = query.Where(uspu => uspu.SubscriptionId == subscriptionId.Value);
        }

        if (privilegeId.HasValue)
        {
            query = query.Where(uspu => uspu.PrivilegeId == privilegeId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(uspu => uspu.Subscription.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(uspu => 
                uspu.Privilege.Name.ToLower().Contains(term) ||
                (uspu.Privilege.Description != null && uspu.Privilege.Description.ToLower().Contains(term)) ||
                uspu.Subscription.User.UserName.ToLower().Contains(term));
        }

        if (isActive.HasValue)
        {
            query = query.Where(uspu => uspu.IsActive == isActive.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(uspu => uspu.LastUsedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(uspu => uspu.LastUsedAt <= endDate.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var usages = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (usages, totalCount);
    }

    /// <summary>
    /// Applies dynamic sorting to the query
    /// </summary>
    private static IQueryable<UserSubscriptionPrivilegeUsage> ApplySorting(IQueryable<UserSubscriptionPrivilegeUsage> query, string sortBy, string sortOrder)
    {
        return sortBy.ToLower() switch
        {
            "privilegename" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(uspu => uspu.Privilege.Name)
                : query.OrderBy(uspu => uspu.Privilege.Name),
            "username" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(uspu => uspu.Subscription.User.UserName)
                : query.OrderBy(uspu => uspu.Subscription.User.UserName),
            "usedvalue" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(uspu => uspu.UsedValue)
                : query.OrderBy(uspu => uspu.UsedValue),
            "allowedvalue" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(uspu => uspu.AllowedValue)
                : query.OrderBy(uspu => uspu.AllowedValue),
            "lastusedat" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(uspu => uspu.LastUsedAt)
                : query.OrderBy(uspu => uspu.LastUsedAt),
            "createddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(uspu => uspu.CreatedDate)
                : query.OrderBy(uspu => uspu.CreatedDate),
            "updateddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(uspu => uspu.UpdatedDate)
                : query.OrderBy(uspu => uspu.UpdatedDate),
            "isactive" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(uspu => uspu.IsActive)
                : query.OrderBy(uspu => uspu.IsActive),
            _ => query.OrderByDescending(uspu => uspu.LastUsedAt)
        };
    }

    #endregion
} 
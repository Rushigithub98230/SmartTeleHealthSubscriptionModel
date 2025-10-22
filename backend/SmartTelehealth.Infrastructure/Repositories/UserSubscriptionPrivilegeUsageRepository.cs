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
    /// Retrieves a user subscription privilege usage by its unique identifier with related entities
    /// </summary>
    public async Task<UserSubscriptionPrivilegeUsage?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.UserSubscriptionPrivilegeUsages
            .Include(uspu => uspu.Subscription)
                .ThenInclude(s => s.User)
            .Include(uspu => uspu.SubscriptionPlanPrivilege)
                .ThenInclude(spp => spp.Privilege)
            .Include(uspu => uspu.Privilege)
            .FirstOrDefaultAsync(uspu => uspu.Id == id);
    }

    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId)
        => await _context.UserSubscriptionPrivilegeUsages.Where(x => x.SubscriptionId == subscriptionId).ToListAsync();

    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionPlanPrivilegeIdAsync(Guid subscriptionPlanPrivilegeId)
        => await _context.UserSubscriptionPrivilegeUsages.Where(x => x.SubscriptionPlanPrivilegeId == subscriptionPlanPrivilegeId).ToListAsync();

    /// <summary>
    /// Retrieves all user subscription privilege usages with related entities
    /// </summary>
    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetAllWithDetailsAsync()
    {
        return await _context.UserSubscriptionPrivilegeUsages
            .Include(uspu => uspu.Subscription)
                .ThenInclude(s => s.User)
            .Include(uspu => uspu.SubscriptionPlanPrivilege)
                .ThenInclude(spp => spp.Privilege)
            .Include(uspu => uspu.Privilege)
            .OrderByDescending(uspu => uspu.LastUsedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new user subscription privilege usage
    /// </summary>
    public async Task<UserSubscriptionPrivilegeUsage> CreateUsageAsync(UserSubscriptionPrivilegeUsage usage)
    {
        return await base.CreateAsync(usage);
    }

    /// <summary>
    /// Updates an existing user subscription privilege usage
    /// </summary>
    public async Task<UserSubscriptionPrivilegeUsage> UpdateUsageAsync(UserSubscriptionPrivilegeUsage usage)
    {
        return await base.UpdateAsync(usage);
    }

    /// <summary>
    /// Deletes a user subscription privilege usage by its unique identifier (soft delete)
    /// </summary>
    public async Task<bool> DeleteUsageAsync(Guid id)
    {
        var entity = await _context.UserSubscriptionPrivilegeUsages.FindAsync(id);
        if (entity != null)
        {
            entity.IsActive = false;
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a user subscription privilege usage exists
    /// </summary>
    public async Task<bool> ExistsUsageAsync(Guid id)
    {
        return await _context.UserSubscriptionPrivilegeUsages.AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public async Task AddAsync(UserSubscriptionPrivilegeUsage usage)
    {
        await CreateUsageAsync(usage);
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
    private static IQueryable<UserSubscriptionPrivilegeUsage> ApplySorting(IQueryable<UserSubscriptionPrivilegeUsage> query, string? sortBy, string? sortOrder)
    {
        // Default sorting if parameters are null or empty
        if (string.IsNullOrEmpty(sortBy) || string.IsNullOrEmpty(sortOrder))
        {
            return query.OrderByDescending(u => u.CreatedDate);
        }

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

    /// <summary>
    /// Gets privilege usage record for a specific user and privilege
    /// </summary>
    public async Task<UserSubscriptionPrivilegeUsage?> GetByUserAndPrivilegeAsync(int userId, Guid privilegeId)
    {
        return await _context.UserSubscriptionPrivilegeUsages
            .Include(uspu => uspu.Subscription)
            .Include(uspu => uspu.Privilege)
            .Include(uspu => uspu.SubscriptionPlanPrivilege)
            .FirstOrDefaultAsync(uspu => uspu.Subscription.UserId == userId && 
                                        uspu.PrivilegeId == privilegeId && 
                                        !uspu.IsDeleted);
    }

    /// <summary>
    /// Gets all privilege usage records for a specific user
    /// </summary>
    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetByUserIdAsync(int userId)
    {
        return await _context.UserSubscriptionPrivilegeUsages
            .Include(uspu => uspu.Subscription)
            .Include(uspu => uspu.Privilege)
            .Include(uspu => uspu.SubscriptionPlanPrivilege)
            .Where(uspu => uspu.Subscription.UserId == userId && !uspu.IsDeleted)
            .ToListAsync();
    }

    /// <summary>
    /// Updates a privilege usage record
    /// </summary>
    public async Task<UserSubscriptionPrivilegeUsage> UpdatePrivilegeUsageAsync(UserSubscriptionPrivilegeUsage usage)
    {
        return await base.UpdateAsync(usage);
    }

    /// <summary>
    /// Creates a new privilege usage record
    /// </summary>
    public async Task<UserSubscriptionPrivilegeUsage> CreatePrivilegeUsageAsync(UserSubscriptionPrivilegeUsage usage)
    {
        return await base.CreateAsync(usage);
    }

    #endregion
} 
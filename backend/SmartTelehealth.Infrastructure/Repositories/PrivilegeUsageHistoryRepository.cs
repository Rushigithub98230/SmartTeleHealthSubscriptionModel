using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class PrivilegeUsageHistoryRepository : RepositoryBase<PrivilegeUsageHistory>, IPrivilegeUsageHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public PrivilegeUsageHistoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a privilege usage history by its unique identifier
    /// </summary>
    public override async Task<PrivilegeUsageHistory?> GetByIdAsync(object id)
    {
        if (id is not Guid historyId)
            return null;

        return await _context.PrivilegeUsageHistories.FindAsync(historyId);
    }

    /// <summary>
    /// Retrieves all privilege usage histories
    /// </summary>
    public override async Task<IEnumerable<PrivilegeUsageHistory>> GetAllAsync()
    {
        return await _context.PrivilegeUsageHistories.ToListAsync();
    }

    /// <summary>
    /// Creates a new privilege usage history
    /// </summary>
    public override async Task<PrivilegeUsageHistory> CreateAsync(PrivilegeUsageHistory history)
    {
        history.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(history);
    }

    /// <summary>
    /// Updates an existing privilege usage history
    /// </summary>
    public override async Task<PrivilegeUsageHistory> UpdateAsync(PrivilegeUsageHistory history)
    {
        history.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(history);
    }

    /// <summary>
    /// Deletes a privilege usage history by its unique identifier (hard delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid historyId)
            return false;

        var history = await _context.PrivilegeUsageHistories.FindAsync(historyId);
        if (history != null)
        {
            _context.PrivilegeUsageHistories.Remove(history);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a privilege usage history exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid historyId)
            return false;

        return await _context.PrivilegeUsageHistories.AnyAsync(x => x.Id == historyId);
    }

    public async Task<IEnumerable<PrivilegeUsageHistory>> GetByUserSubscriptionPrivilegeUsageIdAsync(Guid userSubscriptionPrivilegeUsageId)
        => await _context.PrivilegeUsageHistories
            .Where(x => x.UserSubscriptionPrivilegeUsageId == userSubscriptionPrivilegeUsageId)
            .OrderByDescending(x => x.UsedAt)
            .ToListAsync();

    public async Task<IEnumerable<PrivilegeUsageHistory>> GetBySubscriptionIdAsync(Guid subscriptionId)
        => await _context.PrivilegeUsageHistories
            .Include(x => x.UserSubscriptionPrivilegeUsage)
            .Where(x => x.UserSubscriptionPrivilegeUsage.SubscriptionId == subscriptionId)
            .OrderByDescending(x => x.UsedAt)
            .ToListAsync();

    public async Task<IEnumerable<PrivilegeUsageHistory>> GetByDateRangeAsync(Guid subscriptionId, DateTime startDate, DateTime endDate)
        => await _context.PrivilegeUsageHistories
            .Include(x => x.UserSubscriptionPrivilegeUsage)
            .Where(x => x.UserSubscriptionPrivilegeUsage.SubscriptionId == subscriptionId &&
                       x.UsageDate >= startDate.Date && x.UsageDate <= endDate.Date)
            .OrderByDescending(x => x.UsedAt)
            .ToListAsync();

    public async Task<int> GetDailyUsageAsync(Guid subscriptionId, Guid privilegeId, DateTime date)
    {
        var usage = await _context.PrivilegeUsageHistories
            .Include(x => x.UserSubscriptionPrivilegeUsage)
            .Where(x => x.UserSubscriptionPrivilegeUsage.SubscriptionId == subscriptionId &&
                       x.UserSubscriptionPrivilegeUsage.SubscriptionPlanPrivilegeId == privilegeId &&
                       x.UsageDate == date.Date)
            .SumAsync(x => x.UsedValue);
        
        return usage;
    }

    public async Task<int> GetWeeklyUsageAsync(Guid subscriptionId, Guid privilegeId, DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(6);
        
        var usage = await _context.PrivilegeUsageHistories
            .Include(x => x.UserSubscriptionPrivilegeUsage)
            .Where(x => x.UserSubscriptionPrivilegeUsage.SubscriptionId == subscriptionId &&
                       x.UserSubscriptionPrivilegeUsage.SubscriptionPlanPrivilegeId == privilegeId &&
                       x.UsageDate >= weekStart.Date && x.UsageDate <= weekEnd.Date)
            .SumAsync(x => x.UsedValue);
        
        return usage;
    }

    public async Task<int> GetMonthlyUsageAsync(Guid subscriptionId, Guid privilegeId, DateTime monthStart)
    {
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        
        var usage = await _context.PrivilegeUsageHistories
            .Include(x => x.UserSubscriptionPrivilegeUsage)
            .Where(x => x.UserSubscriptionPrivilegeUsage.SubscriptionId == subscriptionId &&
                       x.UserSubscriptionPrivilegeUsage.SubscriptionPlanPrivilegeId == privilegeId &&
                       x.UsageDate >= monthStart.Date && x.UsageDate <= monthEnd.Date)
            .SumAsync(x => x.UsedValue);
        
        return usage;
    }

    public async Task AddAsync(PrivilegeUsageHistory usageHistory)
    {
        await _context.PrivilegeUsageHistories.AddAsync(usageHistory);
        await _context.SaveChangesAsync();
    }


    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.PrivilegeUsageHistories.FindAsync(id);
        if (entity != null)
        {
            _context.PrivilegeUsageHistories.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Retrieves privilege usage history with database-level filtering, pagination, and sorting
    /// </summary>
    public async Task<(IEnumerable<PrivilegeUsageHistory> History, int TotalCount)> GetUsageHistoryWithFilteringAsync(
        int page, int pageSize, string? privilegeId, string? userId, string? subscriptionId, 
        DateTime? startDate, DateTime? endDate, string? sortBy = "UsedAt", string? sortOrder = "desc")
    {
        var query = _context.PrivilegeUsageHistories
            .Include(h => h.UserSubscriptionPrivilegeUsage)
                .ThenInclude(uspu => uspu.Privilege)
            .Include(h => h.UserSubscriptionPrivilegeUsage)
                .ThenInclude(uspu => uspu.Subscription)
                    .ThenInclude(s => s.User)
            .AsQueryable();

        // Apply privilege filter
        if (!string.IsNullOrWhiteSpace(privilegeId) && Guid.TryParse(privilegeId, out var privId))
        {
            query = query.Where(h => h.UserSubscriptionPrivilegeUsage.PrivilegeId == privId);
        }

        // Apply user filter
        if (!string.IsNullOrWhiteSpace(userId) && int.TryParse(userId, out var uId))
        {
            query = query.Where(h => h.UserSubscriptionPrivilegeUsage.Subscription.UserId == uId);
        }

        // Apply subscription filter
        if (!string.IsNullOrWhiteSpace(subscriptionId) && Guid.TryParse(subscriptionId, out var subId))
        {
            query = query.Where(h => h.UserSubscriptionPrivilegeUsage.SubscriptionId == subId);
        }

        // Apply date range filters
        if (startDate.HasValue)
        {
            query = query.Where(h => h.UsedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(h => h.UsedAt <= endDate.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var history = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (history, totalCount);
    }

    /// <summary>
    /// Gets usage summary with database-level aggregation
    /// </summary>
    public async Task<object> GetUsageSummaryAsync(string? privilegeId, string? userId, string? subscriptionId, 
        DateTime? startDate, DateTime? endDate)
    {
        var query = _context.PrivilegeUsageHistories
            .Include(h => h.UserSubscriptionPrivilegeUsage)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(privilegeId) && Guid.TryParse(privilegeId, out var privId))
        {
            query = query.Where(h => h.UserSubscriptionPrivilegeUsage.PrivilegeId == privId);
        }

        if (!string.IsNullOrWhiteSpace(userId) && int.TryParse(userId, out var uId))
        {
            query = query.Where(h => h.UserSubscriptionPrivilegeUsage.Subscription.UserId == uId);
        }

        if (!string.IsNullOrWhiteSpace(subscriptionId) && Guid.TryParse(subscriptionId, out var subId))
        {
            query = query.Where(h => h.UserSubscriptionPrivilegeUsage.SubscriptionId == subId);
        }

        if (startDate.HasValue)
        {
            query = query.Where(h => h.UsedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(h => h.UsedAt <= endDate.Value);
        }

        // Calculate summary statistics
        var totalUsage = await query.CountAsync();
        var averageDailyUsage = totalUsage > 0 ? await query
            .GroupBy(h => h.UsedAt.Date)
            .Select(g => g.Count())
            .AverageAsync() : 0;

        var mostUsedPrivilege = await query
            .GroupBy(h => h.UserSubscriptionPrivilegeUsage.PrivilegeId)
            .Select(g => new { PrivilegeId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        return new
        {
            TotalUsage = totalUsage,
            AverageDailyUsage = Math.Round(averageDailyUsage, 2),
            MostUsedPrivilegeId = mostUsedPrivilege?.PrivilegeId,
            MostUsedPrivilegeCount = mostUsedPrivilege?.Count ?? 0
        };
    }

    /// <summary>
    /// Applies dynamic sorting to the query
    /// </summary>
    private static IQueryable<PrivilegeUsageHistory> ApplySorting(IQueryable<PrivilegeUsageHistory> query, string sortBy, string sortOrder)
    {
        return sortBy.ToLower() switch
        {
            "usedat" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(h => h.UsedAt)
                : query.OrderBy(h => h.UsedAt),
            "privilegename" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(h => h.UserSubscriptionPrivilegeUsage.Privilege.Name)
                : query.OrderBy(h => h.UserSubscriptionPrivilegeUsage.Privilege.Name),
            "username" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(h => h.UserSubscriptionPrivilegeUsage.Subscription.User.UserName)
                : query.OrderBy(h => h.UserSubscriptionPrivilegeUsage.Subscription.User.UserName),
            "createddate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(h => h.CreatedDate)
                : query.OrderBy(h => h.CreatedDate),
            _ => query.OrderByDescending(h => h.UsedAt)
        };
    }
}

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
}

using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class PrivilegeUsageHistoryRepository : IPrivilegeUsageHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public PrivilegeUsageHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrivilegeUsageHistory?> GetByIdAsync(Guid id)
        => await _context.PrivilegeUsageHistories.FindAsync(id);

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

    public async Task UpdateAsync(PrivilegeUsageHistory usageHistory)
    {
        _context.PrivilegeUsageHistories.Update(usageHistory);
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

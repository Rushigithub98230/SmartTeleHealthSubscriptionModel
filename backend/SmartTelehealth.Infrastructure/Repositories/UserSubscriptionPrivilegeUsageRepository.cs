using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class UserSubscriptionPrivilegeUsageRepository : IUserSubscriptionPrivilegeUsageRepository
{
    private readonly ApplicationDbContext _context;
    public UserSubscriptionPrivilegeUsageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserSubscriptionPrivilegeUsage?> GetByIdAsync(Guid id)
        => await _context.UserSubscriptionPrivilegeUsages.FindAsync(id);

    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId)
        => await _context.UserSubscriptionPrivilegeUsages.Where(x => x.SubscriptionId == subscriptionId).ToListAsync();

    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionPlanPrivilegeIdAsync(Guid subscriptionPlanPrivilegeId)
        => await _context.UserSubscriptionPrivilegeUsages.Where(x => x.SubscriptionPlanPrivilegeId == subscriptionPlanPrivilegeId).ToListAsync();

    public async Task AddAsync(UserSubscriptionPrivilegeUsage usage)
    {
        _context.UserSubscriptionPrivilegeUsages.Add(usage);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserSubscriptionPrivilegeUsage usage)
    {
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.UserSubscriptionPrivilegeUsages.FindAsync(id);
        if (entity != null)
        {
            _context.UserSubscriptionPrivilegeUsages.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
} 
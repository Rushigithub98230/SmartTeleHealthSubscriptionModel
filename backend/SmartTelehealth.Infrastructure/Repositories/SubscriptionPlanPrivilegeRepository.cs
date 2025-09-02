using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionPlanPrivilegeRepository : ISubscriptionPlanPrivilegeRepository
{
    private readonly ApplicationDbContext _context;
    public SubscriptionPlanPrivilegeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionPlanPrivilege?> GetByIdAsync(Guid id)
        => await _context.SubscriptionPlanPrivileges.FindAsync(id);

    public async Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPlanIdAsync(Guid planId)
        => await _context.SubscriptionPlanPrivileges
            .Where(x => x.SubscriptionPlanId == planId)
            .ToListAsync();

    public async Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPrivilegeIdAsync(Guid privilegeId)
        => await _context.SubscriptionPlanPrivileges.Where(x => x.PrivilegeId == privilegeId).ToListAsync();

    public async Task AddAsync(SubscriptionPlanPrivilege planPrivilege)
    {
        _context.SubscriptionPlanPrivileges.Add(planPrivilege);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SubscriptionPlanPrivilege planPrivilege)
    {
        _context.SubscriptionPlanPrivileges.Update(planPrivilege);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.SubscriptionPlanPrivileges.FindAsync(id);
        if (entity != null)
        {
            _context.SubscriptionPlanPrivileges.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
} 
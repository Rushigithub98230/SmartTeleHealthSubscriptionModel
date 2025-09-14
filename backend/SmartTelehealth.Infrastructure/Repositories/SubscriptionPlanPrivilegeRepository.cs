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
} 
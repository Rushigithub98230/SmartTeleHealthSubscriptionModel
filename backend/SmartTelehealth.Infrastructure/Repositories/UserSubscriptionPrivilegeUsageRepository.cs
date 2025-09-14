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
} 
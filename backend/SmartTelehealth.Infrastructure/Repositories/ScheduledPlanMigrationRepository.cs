using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

/// <summary>
/// Repository for scheduled plan migrations.
/// Healthcare Feature: Tracks individual user migrations at renewal dates.
/// </summary>
public class ScheduledPlanMigrationRepository : RepositoryBase<ScheduledPlanMigration>, IScheduledPlanMigrationRepository
{
    private readonly ApplicationDbContext _context;

    public ScheduledPlanMigrationRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets all pending migrations.
    /// </summary>
    public async Task<IEnumerable<ScheduledPlanMigration>> GetPendingMigrationsAsync()
    {
        return await _context.ScheduledPlanMigrations
            .Include(m => m.Subscription)
                .ThenInclude(s => s.User)
            .Include(m => m.FromPlan)
            .Include(m => m.ToPlan)
            .Where(m => m.Status == "Pending" && !m.IsDeleted)
            .OrderBy(m => m.ScheduledMigrationDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets migrations due by a specific date.
    /// Healthcare Feature: Process migrations at individual renewal dates.
    /// </summary>
    public async Task<IEnumerable<ScheduledPlanMigration>> GetMigrationsDueByDateAsync(DateTime date)
    {
        return await _context.ScheduledPlanMigrations
            .Include(m => m.Subscription)
                .ThenInclude(s => s.User)
            .Include(m => m.FromPlan)
            .Include(m => m.ToPlan)
            .Where(m => m.ScheduledMigrationDate <= date && 
                       m.Status == "Pending" && 
                       !m.IsDeleted)
            .OrderBy(m => m.ScheduledMigrationDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a migration by subscription ID.
    /// </summary>
    public async Task<ScheduledPlanMigration?> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.ScheduledPlanMigrations
            .Include(m => m.Subscription)
            .Include(m => m.FromPlan)
            .Include(m => m.ToPlan)
            .FirstOrDefaultAsync(m => m.SubscriptionId == subscriptionId && 
                                     m.Status == "Pending" && 
                                     !m.IsDeleted);
    }

    /// <summary>
    /// Gets all migrations for a specific plan.
    /// Healthcare Feature: Track how many users are migrating from a plan version.
    /// </summary>
    public async Task<IEnumerable<ScheduledPlanMigration>> GetMigrationsByPlanAsync(Guid planId)
    {
        return await _context.ScheduledPlanMigrations
            .Include(m => m.Subscription)
                .ThenInclude(s => s.User)
            .Include(m => m.FromPlan)
            .Include(m => m.ToPlan)
            .Where(m => (m.FromPlanId == planId || m.ToPlanId == planId) && !m.IsDeleted)
            .OrderBy(m => m.ScheduledMigrationDate)
            .ToListAsync();
    }
}


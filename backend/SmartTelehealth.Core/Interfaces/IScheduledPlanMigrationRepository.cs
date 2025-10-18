using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

/// <summary>
/// Repository interface for scheduled plan migrations.
/// Healthcare Feature: Tracks individual user migrations at renewal dates.
/// </summary>
public interface IScheduledPlanMigrationRepository : IRepositoryBase<ScheduledPlanMigration>
{
    /// <summary>
    /// Gets all pending migrations.
    /// </summary>
    /// <returns>Collection of pending migrations</returns>
    Task<IEnumerable<ScheduledPlanMigration>> GetPendingMigrationsAsync();
    
    /// <summary>
    /// Gets migrations due by a specific date.
    /// Healthcare Feature: Process migrations at individual renewal dates.
    /// </summary>
    /// <param name="date">The date to check for due migrations</param>
    /// <returns>Collection of migrations due by the specified date</returns>
    Task<IEnumerable<ScheduledPlanMigration>> GetMigrationsDueByDateAsync(DateTime date);
    
    /// <summary>
    /// Gets a migration by subscription ID.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID</param>
    /// <returns>Scheduled migration or null if not found</returns>
    Task<ScheduledPlanMigration?> GetBySubscriptionIdAsync(Guid subscriptionId);
    
    /// <summary>
    /// Gets all migrations for a specific plan.
    /// Healthcare Feature: Track how many users are migrating from a plan version.
    /// </summary>
    /// <param name="planId">The plan ID</param>
    /// <returns>Collection of migrations for the plan</returns>
    Task<IEnumerable<ScheduledPlanMigration>> GetMigrationsByPlanAsync(Guid planId);
}


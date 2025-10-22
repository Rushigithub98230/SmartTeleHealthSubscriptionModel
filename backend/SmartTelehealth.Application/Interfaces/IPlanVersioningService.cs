using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service interface for subscription plan versioning.
/// Healthcare Feature: Create plan versions instead of modifying existing plans.
/// Issue #1 Fix: Preserves existing subscriptions when plan changes.
/// </summary>
public interface IPlanVersioningService
{
    /// <summary>
    /// Creates a new version of a plan instead of modifying existing.
    /// Issue #1 Fix: Preserves existing subscriptions on old version.
    /// Choice 3a: Auto-versions existing plans as v1.0.
    /// </summary>
    /// <param name="existingPlanId">ID of the plan to version</param>
    /// <param name="updateDto">Updated plan details</param>
    /// <param name="tokenModel">User token for audit</param>
    /// <returns>JsonModel with new plan version and migration info</returns>
    Task<JsonModel> CreateNewPlanVersionAsync(
        Guid existingPlanId,
        UpdateSubscriptionPlanDto updateDto,
        TokenModel tokenModel);
    
    /// <summary>
    /// Gets version history for a plan.
    /// </summary>
    /// <param name="planId">Plan ID or parent plan ID</param>
    /// <returns>JsonModel with all plan versions</returns>
    Task<JsonModel> GetPlanVersionHistoryAsync(Guid planId);
    
    /// <summary>
    /// Schedules migrations for active subscribers when a new plan version is created.
    /// Healthcare Workflow: Each user migrates at their next individual renewal date.
    /// </summary>
    /// <param name="oldPlanId">Old plan version ID</param>
    /// <param name="newPlanId">New plan version ID</param>
    /// <param name="tokenModel">User token for audit</param>
    /// <returns>JsonModel with migration summary</returns>
    Task<JsonModel> ScheduleMigrationsForPlanVersionAsync(
        Guid oldPlanId,
        Guid newPlanId,
        TokenModel tokenModel);
    
    /// <summary>
    /// Processes user response to scheduled migration.
    /// Healthcare Workflow: User can accept, downgrade, or cancel.
    /// </summary>
    /// <param name="response">User's migration decision</param>
    /// <param name="tokenModel">User token for audit</param>
    /// <returns>JsonModel with updated migration status</returns>
    Task<JsonModel> ProcessUserMigrationResponseAsync(
        MigrationResponseDto response,
        TokenModel tokenModel);
    
    // ===== PHASE 6: PLAN VERSION MANAGEMENT ENHANCEMENTS =====
    
    /// <summary>
    /// Create new plan version (overload with change list)
    /// Phase 6: Simplified version creation
    /// </summary>
    Task<JsonModel> CreateNewPlanVersionAsync(Guid planId, List<string> changes, TokenModel tokenModel);
    
    /// <summary>
    /// Get plan version history (overload without tokenModel)
    /// Phase 6: Version history retrieval
    /// </summary>
    Task<JsonModel> GetPlanVersionHistoryAsync(Guid planId, TokenModel tokenModel);
    
    /// <summary>
    /// Get grandfathered users (overload with tokenModel)
    /// Phase 6: Grandfathered user tracking
    /// </summary>
    Task<JsonModel> GetGrandfatheredUsersAsync(Guid planId, TokenModel tokenModel);
    
    /// <summary>
    /// Migrate users to new version
    /// Phase 6: User migration with options
    /// </summary>
    Task<JsonModel> MigrateUsersToNewVersionAsync(Guid planId, MigrateUsersRequestDto request, TokenModel tokenModel);
    
    /// <summary>
    /// Execute a scheduled migration immediately
    /// Phase 6: Manual migration execution
    /// </summary>
    Task<JsonModel> ExecuteScheduledMigrationAsync(Guid migrationId, TokenModel tokenModel);
    
    /// <summary>
    /// Cancel a scheduled migration
    /// Phase 6: Migration cancellation
    /// </summary>
    Task<JsonModel> CancelScheduledMigrationAsync(Guid migrationId, TokenModel tokenModel);
}


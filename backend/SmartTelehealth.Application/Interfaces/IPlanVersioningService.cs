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
}


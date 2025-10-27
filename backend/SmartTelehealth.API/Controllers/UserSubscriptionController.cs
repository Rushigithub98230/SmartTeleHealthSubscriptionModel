using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller for user-facing subscription management features.
/// Healthcare Feature: Users can view and respond to scheduled plan migrations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserSubscriptionController : BaseController
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IScheduledPlanMigrationRepository _scheduledMigrationRepository;
    private readonly IPlanVersioningService _planVersioningService;
    private readonly ILogger<UserSubscriptionController> _logger;

    public UserSubscriptionController(
        ISubscriptionRepository subscriptionRepository,
        IScheduledPlanMigrationRepository scheduledMigrationRepository,
        IPlanVersioningService planVersioningService,
        ILogger<UserSubscriptionController> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _scheduledMigrationRepository = scheduledMigrationRepository;
        _planVersioningService = planVersioningService;
        _logger = logger;
    }

    /// <summary>
    /// User views their scheduled plan migration.
    /// Healthcare Feature: Users can see upcoming price changes.
    /// </summary>
    /// <returns>JsonModel with scheduled migration details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Shows the user's upcoming plan migration (if any)
    /// - Displays old plan, new plan, and migration date
    /// - Shows available options (Accept, Downgrade, Cancel)
    /// - User can only view their own migration
    /// </remarks>
    [HttpGet("my-subscription/migration")]
    public async Task<JsonModel> GetMyScheduledMigration()
    {
        try
        {
            var userId = GetToken(HttpContext).UserID;
            
            _logger.LogInformation("User {UserId} requesting scheduled migration info", userId);
            
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "No active subscription found",
                    StatusCode = 404
                };
            }
            
            var migration = await _scheduledMigrationRepository.GetBySubscriptionIdAsync(subscription.Id);
            
            if (migration == null)
            {
                return new JsonModel
                {
                    data = new { hasScheduledMigration = false },
                    Message = "No scheduled migration found",
                    StatusCode = 200
                };
            }
            
            return new JsonModel
            {
                data = new
                {
                    hasScheduledMigration = true,
                    migration = new
                    {
                        migration.Id,
                        migration.SubscriptionId,
                        fromPlan = new
                        {
                            migration.FromPlan.Id,
                            migration.FromPlan.Name,
                            migration.FromPlan.BasePrice,
                            migration.FromPlan.VersionNumber
                        },
                        toPlan = new
                        {
                            migration.ToPlan.Id,
                            migration.ToPlan.Name,
                            migration.ToPlan.BasePrice,
                            migration.ToPlan.VersionNumber
                        },
                        migration.NotificationDate,
                        migration.ScheduledMigrationDate,
                        migration.Status,
                        migration.UserDecision,
                        migration.UserDecisionDate,
                        daysUntilMigration = (migration.ScheduledMigrationDate - DateTime.UtcNow).Days
                    }
                },
                Message = "Scheduled migration retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scheduled migration for user");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving scheduled migration",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// User accepts/rejects scheduled price change.
    /// Healthcare Workflow: User can accept, downgrade, or cancel.
    /// </summary>
    /// <param name="response">User's migration decision</param>
    /// <returns>JsonModel with updated migration status</returns>
    /// <remarks>
    /// This endpoint:
    /// - Allows users to respond to upcoming price changes
    /// - Options: "Accept" (auto-migrate), "Downgrade" (choose cheaper plan), "Cancel" (end subscription)
    /// - Validates user owns the subscription
    /// - Updates migration record with user's decision
    /// </remarks>
    [HttpPost("my-subscription/migration/respond")]
    public async Task<JsonModel> RespondToMigration([FromBody] MigrationResponseDto response)
    {
        try
        {
            var token = GetToken(HttpContext);
            
            _logger.LogInformation(
                "User {UserId} responding to migration for subscription {SubId}: {Decision}",
                token.UserID, response.SubscriptionId, response.Decision);
            
            return await _planVersioningService.ProcessUserMigrationResponseAsync(response, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing migration response");
            return new JsonModel
            {
                data = new object(),
                Message = "Error processing migration response",
                StatusCode = 500
            };
        }
    }
}


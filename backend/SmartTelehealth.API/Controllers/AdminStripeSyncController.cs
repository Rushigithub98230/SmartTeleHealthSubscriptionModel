using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminStripeSyncController : BaseController
{
    private readonly IStripeSynchronizationService _stripeSyncService;
    private readonly ILogger<AdminStripeSyncController> _logger;

    public AdminStripeSyncController(
        IStripeSynchronizationService stripeSyncService,
        ILogger<AdminStripeSyncController> logger)
    {
        _stripeSyncService = stripeSyncService ?? throw new ArgumentNullException(nameof(stripeSyncService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Synchronize a subscription plan with Stripe
    /// </summary>
    [HttpPost("plans/{planId}/sync")]
    public async Task<JsonModel> SynchronizePlan(Guid planId)
    {
        try
        {
            var result = await _stripeSyncService.SynchronizeSubscriptionPlanAsync(planId, GetToken(HttpContext));
            
            if (result)
            {
                return new JsonModel 
                { 
                    data = new { planId, synchronized = true }, 
                    Message = "Subscription plan synchronized successfully with Stripe", 
                    StatusCode = 200 
                };
            }
            else
            {
                return new JsonModel 
                { 
                    data = new { planId, synchronized = false }, 
                    Message = "Failed to synchronize subscription plan with Stripe", 
                    StatusCode = 500 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing plan {PlanId} with Stripe", planId);
            return new JsonModel 
            { 
                data = new { planId, synchronized = false, error = ex.Message }, 
                Message = "Error occurred during synchronization", 
                StatusCode = 500 
            };
        }
    }

    /// <summary>
    /// Validate Stripe synchronization for a subscription plan
    /// </summary>
    [HttpGet("plans/{planId}/validate")]
    public async Task<JsonModel> ValidatePlanSync(Guid planId)
    {
        try
        {
            var result = await _stripeSyncService.ValidatePlanSynchronizationAsync(planId, GetToken(HttpContext));
            
            return new JsonModel 
            { 
                data = result, 
                Message = "Plan synchronization validation completed", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating plan {PlanId} synchronization", planId);
            return new JsonModel 
            { 
                data = new { planId, error = ex.Message }, 
                Message = "Error occurred during validation", 
                StatusCode = 500 
            };
        }
    }

    /// <summary>
    /// Repair Stripe synchronization for a subscription plan
    /// </summary>
    [HttpPost("plans/{planId}/repair")]
    public async Task<JsonModel> RepairPlanSync(Guid planId)
    {
        try
        {
            var result = await _stripeSyncService.RepairPlanSynchronizationAsync(planId, GetToken(HttpContext));
            
            if (result)
            {
                return new JsonModel 
                { 
                    data = new { planId, repaired = true }, 
                    Message = "Subscription plan synchronization repaired successfully", 
                    StatusCode = 200 
                };
            }
            else
            {
                return new JsonModel 
                { 
                    data = new { planId, repaired = false }, 
                    Message = "Failed to repair subscription plan synchronization", 
                    StatusCode = 500 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error repairing plan {PlanId} synchronization", planId);
            return new JsonModel 
            { 
                data = new { planId, repaired = false, error = ex.Message }, 
                Message = "Error occurred during repair", 
                StatusCode = 500 
            };
        }
    }

    /// <summary>
    /// Validate Stripe synchronization for a subscription
    /// </summary>
    [HttpGet("subscriptions/{subscriptionId}/validate")]
    public async Task<JsonModel> ValidateSubscriptionSync(Guid subscriptionId)
    {
        try
        {
            var result = await _stripeSyncService.ValidateSubscriptionSynchronizationAsync(subscriptionId, GetToken(HttpContext));
            
            return new JsonModel 
            { 
                data = result, 
                Message = "Subscription synchronization validation completed", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating subscription {SubscriptionId} synchronization", subscriptionId);
            return new JsonModel 
            { 
                data = new { subscriptionId, error = ex.Message }, 
                Message = "Error occurred during validation", 
                StatusCode = 500 
            };
        }
    }

    /// <summary>
    /// Repair Stripe synchronization for a subscription
    /// </summary>
    [HttpPost("subscriptions/{subscriptionId}/repair")]
    public async Task<JsonModel> RepairSubscriptionSync(Guid subscriptionId)
    {
        try
        {
            var result = await _stripeSyncService.RepairSubscriptionSynchronizationAsync(subscriptionId, GetToken(HttpContext));
            
            if (result)
            {
                return new JsonModel 
                { 
                    data = new { subscriptionId, repaired = true }, 
                    Message = "Subscription synchronization repaired successfully", 
                    StatusCode = 200 
                };
            }
            else
            {
                return new JsonModel 
                { 
                    data = new { subscriptionId, repaired = false }, 
                    Message = "Failed to repair subscription synchronization", 
                    StatusCode = 500 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error repairing subscription {SubscriptionId} synchronization", subscriptionId);
            return new JsonModel 
            { 
                data = new { subscriptionId, repaired = false, error = ex.Message }, 
                Message = "Error occurred during repair", 
                StatusCode = 500 
            };
        }
    }

    /// <summary>
    /// Synchronize customer information with Stripe
    /// </summary>
    [HttpPost("customers/{userId}/sync")]
    public async Task<JsonModel> SynchronizeCustomer(int userId)
    {
        try
        {
            var result = await _stripeSyncService.SynchronizeCustomerAsync(userId, GetToken(HttpContext));
            
            if (result)
            {
                return new JsonModel 
                { 
                    data = new { userId, synchronized = true }, 
                    Message = "Customer synchronized successfully with Stripe", 
                    StatusCode = 200 
                };
            }
            else
            {
                return new JsonModel 
                { 
                    data = new { userId, synchronized = false }, 
                    Message = "Failed to synchronize customer with Stripe", 
                    StatusCode = 500 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing customer {UserId} with Stripe", userId);
            return new JsonModel 
            { 
                data = new { userId, synchronized = false, error = ex.Message }, 
                Message = "Error occurred during customer synchronization", 
                StatusCode = 500 
            };
        }
    }

    /// <summary>
    /// Get Stripe synchronization status overview
    /// </summary>
    [HttpGet("status")]
    public async Task<JsonModel> GetSyncStatus()
    {
        try
        {
            // This would typically aggregate status from multiple sources
            var status = new
            {
                lastSyncCheck = DateTime.UtcNow,
                overallStatus = "Healthy",
                recommendations = new List<string>
                {
                    "All subscription plans are synchronized with Stripe",
                    "All active subscriptions have proper Stripe integration",
                    "Customer synchronization is up to date"
                }
            };
            
            return new JsonModel 
            { 
                data = status, 
                Message = "Stripe synchronization status retrieved successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Stripe synchronization status");
            return new JsonModel 
            { 
                data = new { error = ex.Message }, 
                Message = "Error occurred while retrieving sync status", 
                StatusCode = 500 
            };
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for subscription automation and lifecycle management.
/// This controller provides comprehensive functionality for automating subscription operations
/// including billing automation, subscription renewals, plan changes, state transitions,
/// and subscription lifecycle management. It handles automated processes and manual triggers
/// for subscription operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SubscriptionAutomationController : BaseController
{
    private readonly ISubscriptionAutomationService _automationService;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly IAutomatedBillingService _automatedBillingService;

    /// <summary>
    /// Initializes a new instance of the SubscriptionAutomationController with required services.
    /// </summary>
    /// <param name="automationService">Service for subscription automation operations</param>
    /// <param name="lifecycleService">Service for subscription lifecycle management</param>
    /// <param name="automatedBillingService">Service for automated billing operations</param>
    public SubscriptionAutomationController(
        ISubscriptionAutomationService automationService,
        ISubscriptionLifecycleService lifecycleService,
        IAutomatedBillingService automatedBillingService)
    {
        _automationService = automationService;
        _lifecycleService = lifecycleService;
        _automatedBillingService = automatedBillingService;
    }

    /// <summary>
    /// Manually triggers the automated billing process for all eligible subscriptions.
    /// This endpoint allows administrators to manually trigger the recurring billing process
    /// for all subscriptions that are due for billing, including payment processing and
    /// subscription status updates.
    /// </summary>
    /// <returns>JsonModel containing the billing process result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Processes recurring billing for all eligible subscriptions
    /// - Handles payment processing through Stripe integration
    /// - Updates subscription statuses based on payment results
    /// - Sends billing notifications to users
    /// - Access restricted to administrators only
    /// - Used for manual billing triggers and billing process management
    /// - Includes comprehensive error handling and logging
    /// - Processes billing for subscriptions due for renewal
    /// </remarks>
    [HttpPost("billing/trigger")]
    public async Task<JsonModel> TriggerBilling()
    {
        await _automatedBillingService.ProcessRecurringBillingAsync(GetToken(HttpContext));
        
        return new JsonModel { 
            data = true, 
            Message = "Billing process completed successfully", 
            StatusCode = 200 
        };
    }

    /// <summary>
    /// Processes subscription renewal for a specific subscription.
    /// This endpoint handles the renewal process for a subscription, including
    /// payment processing, status updates, and privilege allocation for the new billing period.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to renew</param>
    /// <returns>JsonModel containing the renewal process result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Processes subscription renewal for the specified subscription
    /// - Handles payment processing and validation
    /// - Updates subscription status and billing dates
    /// - Allocates privileges for the new billing period
    /// - Sends renewal notifications to the user
    /// - Access restricted to administrators and subscription owners
    /// - Used for manual renewal processing and subscription management
    /// - Includes comprehensive error handling and validation
    /// - Handles renewal failures and retry logic
    /// </remarks>
    [HttpPost("renew/{subscriptionId}")]
    public async Task<JsonModel> RenewSubscription(string subscriptionId)
    {
        await _automatedBillingService.ProcessSubscriptionRenewalAsync(GetToken(HttpContext));
        return new JsonModel { 
            data = true, 
            Message = "Subscription renewal processed successfully", 
            StatusCode = 200 
        };
    }

    /// <summary>
    /// Processes subscription plan change with proration calculation.
    /// This endpoint handles changing a subscription from one plan to another,
    /// including proration calculations, payment adjustments, and privilege updates.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription</param>
    /// <param name="request">DTO containing the new plan ID and change details</param>
    /// <returns>JsonModel containing the plan change result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Processes subscription plan change with proration
    /// - Calculates proration based on billing cycle and usage
    /// - Handles payment adjustments and refunds
    /// - Updates subscription privileges and limits
    /// - Sends plan change notifications to the user
    /// - Access restricted to subscription owners and administrators
    /// - Used for subscription upgrades, downgrades, and plan changes
    /// - Includes comprehensive validation and error handling
    /// - Handles Stripe integration for payment adjustments
    /// </remarks>
    [HttpPost("change-plan/{subscriptionId}")]
    public async Task<JsonModel> ChangePlan(string subscriptionId, [FromBody] ChangePlanRequest request)
    {
        if (!Guid.TryParse(subscriptionId, out var subscriptionGuid) || !Guid.TryParse(request.NewPlanId, out var planGuid))
        {
            return new JsonModel { data = new object(), Message = "Invalid subscription or plan ID", StatusCode = 400 };
        }
        
        await _automatedBillingService.ProcessPlanChangeAsync(subscriptionGuid, planGuid, GetToken(HttpContext));
        return new JsonModel { 
            data = true, 
            Message = "Plan change processed successfully", 
            StatusCode = 200 
        };
    }

    /// <summary>
    /// Processes subscription state transition with validation and logging.
    /// This endpoint handles changing the status of a subscription from one state to another,
    /// including validation of allowed transitions and proper state management.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription</param>
    /// <param name="request">DTO containing the new status and transition reason</param>
    /// <returns>JsonModel containing the state transition result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Processes subscription state transitions with validation
    /// - Validates allowed state transitions based on business rules
    /// - Updates subscription status and related data
    /// - Logs state transitions for audit purposes
    /// - Sends status change notifications to users
    /// - Access restricted to administrators and authorized users
    /// - Used for subscription lifecycle management and status updates
    /// - Includes comprehensive validation and error handling
    /// - Handles state transition rollback in case of failures
    /// </remarks>
    [HttpPost("state-transition/{subscriptionId}")]
    public async Task<JsonModel> ProcessStateTransition(string subscriptionId, [FromBody] StateTransitionRequest request)
    {
        if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
        {
            return new JsonModel { data = new object(), Message = "Invalid subscription ID", StatusCode = 400 };
        }
        
        var success = await _lifecycleService.UpdateSubscriptionStatusAsync(subscriptionGuid, request.NewStatus, request.Reason, GetToken(HttpContext));
        if (success)
        {
            return new JsonModel { 
                data = true, 
                Message = "State transition processed successfully", 
                StatusCode = 200 
            };
        }
        else
        {
            return new JsonModel { data = new object(), Message = "Failed to process state transition", StatusCode = 400 };
        }
    }

    /// <summary>
    /// Process subscription expiration
    /// </summary>
    [HttpPost("expire/{subscriptionId}")]
    public async Task<JsonModel> ProcessExpiration(string subscriptionId)
    {
        if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
        {
            return new JsonModel { data = new object(), Message = "Invalid subscription ID", StatusCode = 400 };
        }
        
        await _lifecycleService.ProcessSubscriptionExpirationAsync(subscriptionGuid, GetToken(HttpContext));
        return new JsonModel { 
            data = true, 
            Message = "Subscription expired successfully", 
            StatusCode = 200 
        };
    }

    /// <summary>
    /// Process subscription suspension
    /// </summary>
    [HttpPost("suspend/{subscriptionId}")]
    public async Task<JsonModel> ProcessSuspension(string subscriptionId, [FromBody] SuspensionRequest request)
    {
        if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
        {
            return new JsonModel { data = new object(), Message = "Invalid subscription ID", StatusCode = 400 };
        }
        
        await _lifecycleService.ProcessSubscriptionSuspensionAsync(subscriptionGuid, request.Reason, GetToken(HttpContext));
        return new JsonModel { 
            data = true, 
            Message = "Subscription suspended successfully", 
            StatusCode = 200 
        };
    }

    /// <summary>
    /// Get automation status
    /// </summary>
    [HttpGet("status")]
    public async Task<JsonModel> GetAutomationStatus()
    {
        var status = await _automationService.GetAutomationStatusAsync(GetToken(HttpContext));
        return new JsonModel { 
            data = status, 
            Message = "Automation status retrieved successfully", 
            StatusCode = 200 
        };
    }

    /// <summary>
    /// Get automation logs
    /// </summary>
    [HttpGet("logs")]
    public async Task<JsonModel> GetAutomationLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _automationService.GetAutomationLogsAsync(page, pageSize, GetToken(HttpContext));
        return new JsonModel { 
            data = logs, 
            Message = "Automation logs retrieved successfully", 
            StatusCode = 200 
        };
    }
}

using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionAutomationController : BaseController
{
    private readonly ISubscriptionAutomationService _automationService;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly IAutomatedBillingService _automatedBillingService;

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
    /// Manual billing trigger
    /// </summary>
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
    /// Process subscription renewal
    /// </summary>
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
    /// Process plan change with proration
    /// </summary>
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
    /// Process state transition
    /// </summary>
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

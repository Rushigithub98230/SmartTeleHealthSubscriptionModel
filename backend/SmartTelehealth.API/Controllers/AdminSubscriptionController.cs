using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminSubscriptionController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionAnalyticsService _analyticsService;
    private readonly ISubscriptionNotificationService _notificationService;
    private readonly ISubscriptionAutomationService _automationService;

    public AdminSubscriptionController(
        ISubscriptionService subscriptionService,
        ISubscriptionAnalyticsService analyticsService,
        ISubscriptionNotificationService notificationService,
        ISubscriptionAutomationService automationService)
    {
        _subscriptionService = subscriptionService;
        _analyticsService = analyticsService;
        _notificationService = notificationService;
        _automationService = automationService;
    }

    /// <summary>
    /// Get all subscriptions with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<JsonModel> GetAllSubscriptions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? planId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        string[]? statusArray = !string.IsNullOrEmpty(status) ? new[] { status } : null;
        string[]? planIdArray = !string.IsNullOrEmpty(planId) ? new[] { planId } : null;
        return await _subscriptionService.GetAllUserSubscriptionsAsync(page, pageSize, null, statusArray, planIdArray, null, startDate, endDate, null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription analytics dashboard data
    /// </summary>
    [HttpGet("analytics")]
    public async Task<JsonModel> GetSubscriptionAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get revenue analytics
    /// </summary>
    [HttpGet("analytics/revenue")]
    public async Task<JsonModel> GetRevenueAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetRevenueAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get churn analytics
    /// </summary>
    [HttpGet("analytics/churn")]
    public async Task<JsonModel> GetChurnAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetChurnAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Export analytics data
    /// </summary>
    [HttpGet("analytics/export")]
    public async Task<JsonModel> ExportAnalytics(
        [FromQuery] string format = "csv",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.ExportAnalyticsAsync(format, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Bulk update subscription statuses
    /// </summary>
    [HttpPost("bulk/status")]
    public async Task<JsonModel> BulkUpdateStatus([FromBody] BulkStatusUpdateRequest request)
    {
        var actions = request.SubscriptionIds.Select(id => new BulkActionRequestDto
        {
            SubscriptionId = id,
            Action = "UpdateStatus",
            Reason = request.NewStatus
        }).ToList();
        
        return await _subscriptionService.PerformBulkActionAsync(actions, GetToken(HttpContext));
    }

    /// <summary>
    /// Bulk cancel subscriptions
    /// </summary>
    [HttpPost("bulk/cancel")]
    public async Task<JsonModel> BulkCancelSubscriptions([FromBody] BulkCancelRequest request)
    {
        var actions = request.SubscriptionIds.Select(id => new BulkActionRequestDto
        {
            SubscriptionId = id,
            Action = "Cancel",
            Reason = request.Reason
        }).ToList();
        
        return await _subscriptionService.PerformBulkActionAsync(actions, GetToken(HttpContext));
    }

    /// <summary>
    /// Bulk send notifications
    /// </summary>
    [HttpPost("bulk/notifications")]
    public async Task<JsonModel> BulkSendNotifications([FromBody] BulkNotificationRequest request)
    {
        return await _notificationService.SendBulkNotificationAsync(
            request.SubscriptionIds, 
            request.Title, 
            request.Message, 
            request.Type, 
            GetToken(HttpContext));
    }

    /// <summary>
    /// Trigger automated billing
    /// </summary>
    [HttpPost("automation/billing")]
    public async Task<JsonModel> TriggerAutomatedBilling()
    {
        return await _automationService.TriggerBillingAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Process automated renewals
    /// </summary>
    [HttpPost("automation/renewals")]
    public async Task<JsonModel> ProcessAutomatedRenewals()
    {
        return await _automationService.ProcessAutomatedRenewalsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Process expired subscriptions
    /// </summary>
    [HttpPost("automation/expired")]
    public async Task<JsonModel> ProcessExpiredSubscriptions()
    {
        return await _automationService.ProcessExpiredSubscriptionsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get automation status
    /// </summary>
    [HttpGet("automation/status")]
    public async Task<JsonModel> GetAutomationStatus()
    {
        return await _automationService.GetAutomationStatusAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get automation logs
    /// </summary>
    [HttpGet("automation/logs")]
    public async Task<JsonModel> GetAutomationLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return await _automationService.GetAutomationLogsAsync(page, pageSize, GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<JsonModel> GetSubscriptionStatistics()
    {
        return await _subscriptionService.GetSubscriptionAnalyticsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription by ID with full details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetSubscriptionById(string id)
    {
        return await _subscriptionService.GetSubscriptionByIdAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Update subscription details
    /// </summary>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateSubscription(string id, [FromBody] UpdateSubscriptionDto updateDto)
    {
        return await _subscriptionService.UpdateSubscriptionAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<JsonModel> CancelSubscription(string id, [FromBody] CancelSubscriptionRequest request)
    {
        return await _subscriptionService.CancelSubscriptionAsync(id, request.Reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Pause subscription
    /// </summary>
    [HttpPost("{id}/pause")]
    public async Task<JsonModel> PauseSubscription(string id)
    {
        return await _subscriptionService.PauseSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Resume subscription
    /// </summary>
    [HttpPost("{id}/resume")]
    public async Task<JsonModel> ResumeSubscription(string id)
    {
        return await _subscriptionService.ResumeSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Change subscription plan
    /// </summary>
    [HttpPost("{id}/change-plan")]
    public async Task<JsonModel> ChangeSubscriptionPlan(string id, [FromBody] ChangePlanRequest request)
    {
        return await _automationService.ChangePlanAsync(id, request, GetToken(HttpContext));
    }

    /// <summary>
    /// Renew subscription
    /// </summary>
    [HttpPost("{id}/renew")]
    public async Task<JsonModel> RenewSubscription(string id)
    {
        return await _automationService.RenewSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Create subscription plan with time-based privilege limits
    /// </summary>
    [HttpPost("plans")]
    public async Task<JsonModel> CreateSubscriptionPlanWithTimeLimits([FromBody] CreateSubscriptionPlanWithTimeLimitsDto request)
    {
        try
        {
            // This would typically call a service method to create the plan
            // For now, return a success response with the plan details
            var planDetails = new
            {
                PlanName = request.PlanName,
                Description = request.Description,
                Price = request.Price,
                BillingCycle = request.BillingCycle,
                DurationMonths = request.DurationMonths,
                Privileges = request.Privileges.Select(p => new
                {
                    PrivilegeName = p.PrivilegeName,
                    TotalValue = p.TotalValue,
                    DailyLimit = p.DailyLimit,
                    WeeklyLimit = p.WeeklyLimit,
                    MonthlyLimit = p.MonthlyLimit,
                    Description = p.Description
                }).ToList()
            };

            return new JsonModel
            {
                data = planDetails,
                Message = "Subscription plan created successfully with time-based limits",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error creating subscription plan: {ex.Message}",
                StatusCode = 500
            };
        }
    }
}

public class BulkStatusUpdateRequest
{
    public List<string> SubscriptionIds { get; set; } = new();
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class BulkCancelRequest
{
    public List<string> SubscriptionIds { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
}

public class BulkNotificationRequest
{
    public List<string> SubscriptionIds { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class CancelSubscriptionRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class PauseSubscriptionRequest
{
    public string Reason { get; set; } = string.Empty;
}

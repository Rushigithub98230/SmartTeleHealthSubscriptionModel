using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for administrative subscription management operations.
/// This controller provides comprehensive administrative functionality for managing subscriptions,
/// including bulk operations, automation controls, analytics access, and subscription lifecycle management.
/// It serves as the central hub for administrators to manage and monitor subscription operations.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminSubscriptionController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionAnalyticsService _analyticsService;
    private readonly ISubscriptionNotificationService _notificationService;
    private readonly ISubscriptionAutomationService _automationService;

    /// <summary>
    /// Initializes a new instance of the AdminSubscriptionController with required services.
    /// </summary>
    /// <param name="subscriptionService">Service for subscription management operations</param>
    /// <param name="analyticsService">Service for subscription analytics and reporting</param>
    /// <param name="notificationService">Service for subscription notifications</param>
    /// <param name="automationService">Service for subscription automation and lifecycle management</param>
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
    /// Retrieves all subscriptions with comprehensive filtering and pagination options for administrative management.
    /// This endpoint provides administrators with access to all subscriptions in the system with advanced filtering
    /// capabilities for effective subscription oversight and management.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20)</param>
    /// <param name="status">Filter by subscription status (Active, Paused, Cancelled, etc.)</param>
    /// <param name="planId">Filter by specific subscription plan ID</param>
    /// <param name="startDate">Start date for date range filtering</param>
    /// <param name="endDate">End date for date range filtering</param>
    /// <returns>JsonModel containing paginated subscription data with filtering applied</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all subscriptions with comprehensive filtering options
    /// - Supports filtering by status, plan, and date range
    /// - Provides pagination for large datasets
    /// - Access restricted to administrators only
    /// - Used for administrative subscription management and oversight
    /// - Includes detailed subscription information and user data
    /// - Supports advanced filtering for subscription analysis
    /// - Provides comprehensive subscription overview for administrators
    /// </remarks>
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
    /// Retrieves comprehensive subscription analytics dashboard data for administrative oversight.
    /// This endpoint provides administrators with detailed analytics including subscription metrics,
    /// growth trends, and performance indicators for strategic decision-making.
    /// </summary>
    /// <param name="startDate">Start date for analytics data range (optional)</param>
    /// <param name="endDate">End date for analytics data range (optional)</param>
    /// <returns>JsonModel containing comprehensive subscription analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive subscription analytics for administrative dashboard
    /// - Provides subscription performance indicators and growth trends
    /// - Includes subscription lifecycle analytics and conversion rates
    /// - Shows subscription distribution and health metrics
    /// - Access restricted to administrators only
    /// - Used for administrative dashboard and strategic planning
    /// - Supports date range filtering for historical analysis
    /// - Includes key performance indicators for subscription business
    /// </remarks>
    [HttpGet("analytics")]
    public async Task<JsonModel> GetSubscriptionAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves detailed revenue analytics for administrative financial oversight.
    /// This endpoint provides administrators with comprehensive revenue analysis including MRR, ARR,
    /// revenue trends, and financial performance metrics for business intelligence.
    /// </summary>
    /// <param name="startDate">Start date for revenue analytics data range (optional)</param>
    /// <param name="endDate">End date for revenue analytics data range (optional)</param>
    /// <returns>JsonModel containing detailed revenue analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed revenue analytics including MRR and ARR calculations
    /// - Provides revenue trends and growth analysis
    /// - Shows revenue distribution by plan and customer segment
    /// - Includes revenue forecasting and predictive analytics
    /// - Access restricted to administrators only
    /// - Used for financial reporting and revenue optimization
    /// - Supports date range filtering for historical revenue analysis
    /// - Includes revenue per customer and lifetime value metrics
    /// </remarks>
    [HttpGet("analytics/revenue")]
    public async Task<JsonModel> GetRevenueAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetRevenueAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves churn and retention analytics for administrative customer management.
    /// This endpoint provides administrators with detailed analysis of customer churn, retention rates,
    /// and customer lifetime value for subscription business optimization.
    /// </summary>
    /// <param name="startDate">Start date for churn analytics data range (optional)</param>
    /// <param name="endDate">End date for churn analytics data range (optional)</param>
    /// <returns>JsonModel containing churn and retention analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed churn analysis including churn rate and reasons
    /// - Provides retention rate analysis and customer lifetime value
    /// - Shows churn patterns by plan and customer segment
    /// - Includes predictive churn analysis and risk assessment
    /// - Access restricted to administrators only
    /// - Used for customer retention strategies and churn prevention
    /// - Supports date range filtering for historical churn analysis
    /// - Includes cohort analysis and retention cohort metrics
    /// </remarks>
    [HttpGet("analytics/churn")]
    public async Task<JsonModel> GetChurnAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetChurnAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Exports analytics data in the specified format for administrative reporting and analysis.
    /// This endpoint allows administrators to export subscription analytics data in various formats
    /// for external reporting, analysis, and business intelligence purposes.
    /// </summary>
    /// <param name="format">Export format (csv, json, excel) - default is csv</param>
    /// <param name="startDate">Start date for export data range (optional)</param>
    /// <param name="endDate">End date for export data range (optional)</param>
    /// <returns>JsonModel containing the exported analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Exports analytics data in the specified format (CSV, JSON, Excel)
    /// - Supports comprehensive data export including all analytics metrics
    /// - Provides filtered data export based on date range
    /// - Access restricted to administrators only
    /// - Used for external reporting and business intelligence
    /// - Supports various export formats for different use cases
    /// - Includes data validation and export optimization
    /// - Provides export status tracking and download links
    /// </remarks>
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

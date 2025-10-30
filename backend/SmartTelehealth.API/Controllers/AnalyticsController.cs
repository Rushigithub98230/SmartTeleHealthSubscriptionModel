using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for providing analytics and reporting functionality for administrative management.
/// This controller provides comprehensive analytics including dashboard summaries, revenue metrics,
/// churn analysis, plan performance, and other business intelligence features.
/// </summary>
[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AnalyticsController : BaseController
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionPlanService _subscriptionPlanService;
    private readonly ISubscriptionBillingService _billingService;

    /// <summary>
    /// Initializes a new instance of the AnalyticsController with required services.
    /// </summary>
    public AnalyticsController(
        IAnalyticsService analyticsService,
        ISubscriptionService subscriptionService,
        ISubscriptionPlanService subscriptionPlanService,
        ISubscriptionBillingService billingService)
    {
        _analyticsService = analyticsService;
        _subscriptionService = subscriptionService;
        _subscriptionPlanService = subscriptionPlanService;
        _billingService = billingService;
    }

    /// <summary>
    /// Retrieves dashboard summary analytics for administrative overview.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<JsonModel> GetDashboardSummary()
    {
        try
        {
            var dashboardResult = await _analyticsService.GetSubscriptionDashboardAsync(null, null, GetToken(HttpContext));
            return dashboardResult;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving dashboard summary: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves revenue metrics for the specified date range.
    /// </summary>
    [HttpGet("revenue")]
    public async Task<JsonModel> GetRevenueMetrics([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var start = !string.IsNullOrEmpty(startDate) ? DateTime.Parse(startDate) : DateTime.UtcNow.AddMonths(-1);
            var end = !string.IsNullOrEmpty(endDate) ? DateTime.Parse(endDate) : DateTime.UtcNow;

            var revenueResult = await _analyticsService.GetRevenueAnalyticsAsync(start, end, GetToken(HttpContext));
            return revenueResult;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving revenue metrics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves churn analysis for the specified period.
    /// </summary>
    [HttpGet("churn")]
    public async Task<JsonModel> GetChurnAnalysis([FromQuery] string period = "month")
    {
        try
        {
            var startDate = period.ToLower() switch
            {
                "week" => DateTime.UtcNow.AddDays(-7),
                "month" => DateTime.UtcNow.AddMonths(-1),
                "quarter" => DateTime.UtcNow.AddMonths(-3),
                "year" => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.UtcNow.AddMonths(-1)
            };

            var churnResult = await _analyticsService.GetChurnAnalyticsAsync(startDate, DateTime.UtcNow);
            
            return new JsonModel
            {
                data = churnResult,
                Message = "Churn analysis retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving churn analysis: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves plan performance analytics.
    /// </summary>
    [HttpGet("plan-performance")]
    public async Task<JsonModel> GetPlanPerformance()
    {
        try
        {
            var planResult = await _analyticsService.GetPlanAnalyticsAsync(null, null, GetToken(HttpContext));
            return planResult;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving plan performance: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves subscription statistics for administrative overview.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<JsonModel> GetSubscriptionStatistics()
    {
        try
        {
            var statisticsResult = await _analyticsService.GetSubscriptionAnalyticsAsync(null, null, GetToken(HttpContext));
            return statisticsResult;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving subscription statistics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves subscription trends for the specified period.
    /// </summary>
    [HttpGet("trends")]
    public async Task<JsonModel> GetSubscriptionTrends([FromQuery] string period = "30days")
    {
        try
        {
            var startDate = period.ToLower() switch
            {
                "7days" => DateTime.UtcNow.AddDays(-7),
                "30days" => DateTime.UtcNow.AddDays(-30),
                "90days" => DateTime.UtcNow.AddDays(-90),
                "1year" => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.UtcNow.AddDays(-30)
            };

            var trendsResult = await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, DateTime.UtcNow, GetToken(HttpContext));
            return trendsResult;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving subscription trends: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves user growth metrics for administrative analysis.
    /// </summary>
    [HttpGet("user-growth")]
    public async Task<JsonModel> GetUserGrowthMetrics()
    {
        try
        {
            var userGrowthResult = await _analyticsService.GetUserAnalyticsAsync(null, null, GetToken(HttpContext));
            return userGrowthResult;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving user growth metrics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves payment analytics for administrative analysis.
    /// </summary>
    [HttpGet("payments")]
    public async Task<JsonModel> GetPaymentAnalytics()
    {
        try
        {
            var paymentResult = await _analyticsService.GetBillingAnalyticsAsync(null, null, GetToken(HttpContext));
            return paymentResult;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving payment analytics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Exports analytics data in the specified format.
    /// </summary>
    [HttpGet("export")]
    public async Task<JsonModel> ExportAnalytics([FromQuery] string type, [FromQuery] string format = "csv")
    {
        try
        {
            var exportResult = await _analyticsService.ExportSubscriptionAnalyticsAsync(null, null, GetToken(HttpContext));
            return exportResult;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error exporting analytics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves comprehensive churn analytics for subscription management.
    /// </summary>
    [HttpGet("churn-analytics")]
    public async Task<JsonModel> GetChurnAnalytics([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var start = !string.IsNullOrEmpty(startDate) ? DateTime.Parse(startDate) : DateTime.UtcNow.AddMonths(-12);
            var end = !string.IsNullOrEmpty(endDate) ? DateTime.Parse(endDate) : DateTime.UtcNow;

            var churnAnalytics = await _analyticsService.GetChurnAnalyticsAsync(start, end);

            return new JsonModel
            {
                data = churnAnalytics,
                Message = "Churn analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving churn analytics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves comprehensive privilege usage analytics.
    /// </summary>
    [HttpGet("privilege-usage-analytics")]
    public async Task<JsonModel> GetPrivilegeUsageAnalytics([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var start = !string.IsNullOrEmpty(startDate) ? DateTime.Parse(startDate) : DateTime.UtcNow.AddMonths(-12);
            var end = !string.IsNullOrEmpty(endDate) ? DateTime.Parse(endDate) : DateTime.UtcNow;

            var privilegeAnalytics = await _analyticsService.GetPrivilegeUsageAnalyticsAsync(start, end);

            return new JsonModel
            {
                data = privilegeAnalytics,
                Message = "Privilege usage analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving privilege usage analytics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves comprehensive subscription lifecycle analytics.
    /// </summary>
    [HttpGet("subscription-lifecycle-analytics")]
    public async Task<JsonModel> GetSubscriptionLifecycleAnalytics([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var start = !string.IsNullOrEmpty(startDate) ? DateTime.Parse(startDate) : DateTime.UtcNow.AddMonths(-12);
            var end = !string.IsNullOrEmpty(endDate) ? DateTime.Parse(endDate) : DateTime.UtcNow;

            var lifecycleAnalytics = await _analyticsService.GetSubscriptionLifecycleAnalyticsAsync(start, end);

            return new JsonModel
            {
                data = lifecycleAnalytics,
                Message = "Subscription lifecycle analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving subscription lifecycle analytics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves enhanced billing analytics with comprehensive metrics.
    /// </summary>
    [HttpGet("enhanced-billing-analytics")]
    public async Task<JsonModel> GetEnhancedBillingAnalytics([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var start = !string.IsNullOrEmpty(startDate) ? DateTime.Parse(startDate) : DateTime.UtcNow.AddMonths(-12);
            var end = !string.IsNullOrEmpty(endDate) ? DateTime.Parse(endDate) : DateTime.UtcNow;

            var enhancedBillingAnalytics = await _analyticsService.GetEnhancedBillingAnalyticsAsync(start, end);

            return new JsonModel
            {
                data = enhancedBillingAnalytics,
                Message = "Enhanced billing analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving enhanced billing analytics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves comprehensive subscription management analytics dashboard.
    /// </summary>
    [HttpGet("subscription-management-dashboard")]
    public async Task<JsonModel> GetSubscriptionManagementDashboard([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var start = !string.IsNullOrEmpty(startDate) ? DateTime.Parse(startDate) : DateTime.UtcNow.AddMonths(-12);
            var end = !string.IsNullOrEmpty(endDate) ? DateTime.Parse(endDate) : DateTime.UtcNow;

            // Get all analytics data in parallel for better performance
            var churnAnalyticsTask = _analyticsService.GetChurnAnalyticsAsync(start, end);
            var privilegeAnalyticsTask = _analyticsService.GetPrivilegeUsageAnalyticsAsync(start, end);
            var lifecycleAnalyticsTask = _analyticsService.GetSubscriptionLifecycleAnalyticsAsync(start, end);
            var enhancedBillingAnalyticsTask = _analyticsService.GetEnhancedBillingAnalyticsAsync(start, end);

            await Task.WhenAll(churnAnalyticsTask, privilegeAnalyticsTask, lifecycleAnalyticsTask, enhancedBillingAnalyticsTask);

            var dashboardData = new
            {
                ChurnAnalytics = await churnAnalyticsTask,
                PrivilegeUsageAnalytics = await privilegeAnalyticsTask,
                SubscriptionLifecycleAnalytics = await lifecycleAnalyticsTask,
                EnhancedBillingAnalytics = await enhancedBillingAnalyticsTask,
                GeneratedAt = DateTime.UtcNow,
                Period = new { StartDate = start, EndDate = end }
            };

            return new JsonModel
            {
                data = dashboardData,
                Message = "Subscription management dashboard retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving subscription management dashboard: {ex.Message}",
                StatusCode = 500
            };
        }
    }
    /// <summary>
    /// Retrieves real-time metrics for dashboard updates.
    /// This endpoint provides live metrics that are updated frequently for dashboard monitoring.
    /// </summary>
    [HttpGet("real-time-metrics")]
    public async Task<JsonModel> GetRealTimeMetrics()
    {
        try
        {
            var metrics = new
            {
                ActiveSubscriptionsNow = await _subscriptionService.GetActiveSubscriptionsCountAsync(),
                RevenueToday = await _billingService.GetRevenueTodayAsync(),
                NewSubscriptionsToday = await _subscriptionService.GetNewSubscriptionsCountAsync(DateTime.Today),
                TrialsEndingThisWeek = await _subscriptionService.GetTrialsEndingCountAsync(7),
                PendingPayments = await _billingService.GetPendingPaymentsCountAsync(),
                LastUpdated = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = metrics,
                Message = "Real-time metrics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving real-time metrics: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves plan migration analytics for admin dashboard.
    /// Provides visibility into scheduled migrations, user decisions (Accept/Cancel), and auto-cancellations.
    /// </summary>
    [HttpGet("plan-migrations")]
    public async Task<JsonModel> GetPlanMigrationAnalytics()
    {
        return await _analyticsService.GetPlanMigrationAnalyticsAsync(GetToken(HttpContext));
    }
} 
    
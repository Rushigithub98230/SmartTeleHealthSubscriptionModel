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

    /// <summary>
    /// Initializes a new instance of the AnalyticsController with required services.
    /// </summary>
    public AnalyticsController(
        IAnalyticsService analyticsService,
        ISubscriptionService subscriptionService,
        ISubscriptionPlanService subscriptionPlanService)
    {
        _analyticsService = analyticsService;
        _subscriptionService = subscriptionService;
        _subscriptionPlanService = subscriptionPlanService;
    }

    /// <summary>
    /// Retrieves dashboard summary analytics for administrative overview.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<JsonModel> GetDashboardSummary()
    {
        try
        {
            // Get basic subscription statistics
            var subscriptionsResult = await _subscriptionService.GetAllUserSubscriptionsAsync(
                1, 1, null, null, null, null, null, null, null, null, GetToken(HttpContext));

            // Get plan statistics
            var plansResult = await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(
                new SubscriptionPlanFilterDto { Page = 1, PageSize = 1000 }, GetToken(HttpContext));

            var dashboardData = new
            {
                TotalSubscriptions = subscriptionsResult.data?.GetType().GetProperty("TotalCount")?.GetValue(subscriptionsResult.data) ?? 0,
                ActiveSubscriptions = subscriptionsResult.data?.GetType().GetProperty("ActiveCount")?.GetValue(subscriptionsResult.data) ?? 0,
                TotalPlans = plansResult.data?.GetType().GetProperty("TotalCount")?.GetValue(plansResult.data) ?? 0,
                ActivePlans = plansResult.data?.GetType().GetProperty("ActiveCount")?.GetValue(plansResult.data) ?? 0,
                LastUpdated = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = dashboardData,
                Message = "Dashboard summary retrieved successfully",
                StatusCode = 200
            };
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

            // Get subscription data for revenue calculation
            var subscriptionsResult = await _subscriptionService.GetAllUserSubscriptionsAsync(
                1, int.MaxValue, null, new[] { "Active" }, null, null, start, end, null, null, GetToken(HttpContext));

            var revenueData = new
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = 0m, // Placeholder - would need actual billing data
                MonthlyRevenue = 0m, // Placeholder
                YearlyRevenue = 0m, // Placeholder
                AverageRevenuePerUser = 0m, // Placeholder
                RevenueByPlan = new object[0], // Placeholder
                LastUpdated = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = revenueData,
                Message = "Revenue metrics retrieved successfully",
                StatusCode = 200
            };
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
            var churnData = new
            {
                Period = period,
                ChurnRate = 0.0, // Placeholder - would need actual churn calculation
                ChurnedSubscriptions = 0, // Placeholder
                TotalSubscriptions = 0, // Placeholder
                ChurnByPlan = new object[0], // Placeholder
                ChurnTrends = new object[0], // Placeholder
                LastUpdated = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = churnData,
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
            // Get all plans
            var plansResult = await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(
                new SubscriptionPlanFilterDto { Page = 1, PageSize = 1000 }, GetToken(HttpContext));

            // Get subscription data for each plan
            var subscriptionsResult = await _subscriptionService.GetAllUserSubscriptionsAsync(
                1, int.MaxValue, null, null, null, null, null, null, null, null, GetToken(HttpContext));

            var planPerformanceData = new
            {
                Plans = new object[0], // Placeholder - would need actual plan performance calculation
                TotalPlans = 0, // Placeholder
                MostPopularPlan = (string?)null, // Placeholder
                LeastPopularPlan = (string?)null, // Placeholder
                AverageSubscriptionsPerPlan = 0.0, // Placeholder
                LastUpdated = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = planPerformanceData,
                Message = "Plan performance retrieved successfully",
                StatusCode = 200
            };
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
            var subscriptionsResult = await _subscriptionService.GetAllUserSubscriptionsAsync(
                1, int.MaxValue, null, null, null, null, null, null, null, null, GetToken(HttpContext));

            var statisticsData = new
            {
                TotalSubscriptions = 0, // Placeholder
                ActiveSubscriptions = 0, // Placeholder
                PausedSubscriptions = 0, // Placeholder
                CancelledSubscriptions = 0, // Placeholder
                TrialSubscriptions = 0, // Placeholder
                ExpiredSubscriptions = 0, // Placeholder
                LastUpdated = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = statisticsData,
                Message = "Subscription statistics retrieved successfully",
                StatusCode = 200
            };
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
            var trendsData = new
            {
                Period = period,
                SubscriptionGrowth = new object[0], // Placeholder
                RevenueTrends = new object[0], // Placeholder
                ChurnTrends = new object[0], // Placeholder
                PlanPopularityTrends = new object[0], // Placeholder
                LastUpdated = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = trendsData,
                Message = "Subscription trends retrieved successfully",
                StatusCode = 200
            };
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
            var userGrowthData = new
            {
                TotalUsers = 0, // Placeholder
                NewUsersThisMonth = 0, // Placeholder
                NewUsersThisYear = 0, // Placeholder
                UserGrowthRate = 0.0, // Placeholder
                UserGrowthTrends = new object[0], // Placeholder
                LastUpdated = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = userGrowthData,
                Message = "User growth metrics retrieved successfully",
                StatusCode = 200
            };
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
            var paymentAnalyticsData = new
            {
                TotalPayments = 0, // Placeholder
                SuccessfulPayments = 0, // Placeholder
                FailedPayments = 0, // Placeholder
                PaymentSuccessRate = 0.0, // Placeholder
                AveragePaymentAmount = 0m, // Placeholder
                PaymentMethods = new object[0], // Placeholder
                LastUpdated = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = paymentAnalyticsData,
                Message = "Payment analytics retrieved successfully",
                StatusCode = 200
            };
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
            var exportData = new
            {
                Type = type,
                Format = format,
                DownloadUrl = "", // Placeholder - would generate actual export file
                FileName = $"analytics_{type}_{DateTime.UtcNow:yyyyMMdd}.{format}",
                GeneratedAt = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = exportData,
                Message = "Analytics export generated successfully",
                StatusCode = 200
            };
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
} 
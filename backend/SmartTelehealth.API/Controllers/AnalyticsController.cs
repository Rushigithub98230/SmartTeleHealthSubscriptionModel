using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive analytics and reporting functionality.
/// This controller provides extensive analytics capabilities including subscription analytics,
/// billing analytics, user analytics, provider analytics, system analytics, and revenue tracking.
/// It supports data export, report generation, and business intelligence for decision-making.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class AnalyticsController : BaseController
{
    private readonly IAnalyticsService _analyticsService;

    /// <summary>
    /// Initializes a new instance of the AnalyticsController with the required analytics service.
    /// </summary>
    /// <param name="analyticsService">Service for handling analytics and reporting business logic</param>
    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Retrieves comprehensive dashboard analytics for the main administrative dashboard.
    /// This endpoint provides key performance indicators, metrics, and summary data
    /// for the main dashboard display including subscription, user, and system metrics.
    /// </summary>
    /// <returns>JsonModel containing dashboard analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive dashboard analytics and KPIs
    /// - Includes subscription, user, and system performance metrics
    /// - Provides summary data for dashboard display
    /// - Access restricted to administrators and authorized users
    /// - Used for main dashboard data loading and display
    /// - Includes comprehensive analytics information and metrics
    /// - Provides data for dashboard visualization and reporting
    /// - Handles analytics data retrieval and error responses
    /// </remarks>
    [HttpGet("dashboard")]
    public async Task<JsonModel> GetDashboardAnalytics()
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves comprehensive subscription analytics and performance metrics.
    /// This endpoint provides detailed subscription analytics including growth metrics,
    /// subscription performance indicators, and subscription lifecycle analytics.
    /// </summary>
    /// <returns>JsonModel containing subscription analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive subscription analytics and metrics
    /// - Includes subscription growth, performance, and lifecycle data
    /// - Shows subscription distribution and conversion rates
    /// - Access restricted to administrators and authorized users
    /// - Used for subscription analytics and performance monitoring
    /// - Includes comprehensive subscription information and metrics
    /// - Provides data for subscription analysis and reporting
    /// - Handles subscription analytics data retrieval and error responses
    /// </remarks>
    [HttpGet("subscriptions")]
    public async Task<JsonModel> GetSubscriptionAnalytics()
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves comprehensive billing analytics and financial metrics.
    /// This endpoint provides detailed billing analytics including revenue metrics,
    /// payment performance, billing trends, and financial performance indicators.
    /// </summary>
    /// <returns>JsonModel containing billing analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive billing analytics and financial metrics
    /// - Includes revenue, payment, and billing performance data
    /// - Shows billing trends and financial indicators
    /// - Access restricted to administrators and authorized users
    /// - Used for billing analytics and financial monitoring
    /// - Includes comprehensive billing information and metrics
    /// - Provides data for financial analysis and reporting
    /// - Handles billing analytics data retrieval and error responses
    /// </remarks>
    [HttpGet("billing")]
    public async Task<JsonModel> GetBillingAnalytics()
    {
        return await _analyticsService.GetBillingAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves comprehensive user analytics and engagement metrics.
    /// This endpoint provides detailed user analytics including user growth, engagement,
    /// activity patterns, and user behavior analytics for system optimization.
    /// </summary>
    /// <returns>JsonModel containing user analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive user analytics and engagement metrics
    /// - Includes user growth, activity, and behavior data
    /// - Shows user engagement patterns and trends
    /// - Access restricted to administrators and authorized users
    /// - Used for user analytics and engagement monitoring
    /// - Includes comprehensive user information and metrics
    /// - Provides data for user analysis and system optimization
    /// - Handles user analytics data retrieval and error responses
    /// </remarks>
    [HttpGet("users")]
    public async Task<JsonModel> GetUserAnalytics()
    {
        return await _analyticsService.GetUserAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves comprehensive provider analytics and performance metrics.
    /// This endpoint provides detailed provider analytics including provider performance,
    /// service metrics, provider engagement, and healthcare service analytics.
    /// </summary>
    /// <returns>JsonModel containing provider analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive provider analytics and performance metrics
    /// - Includes provider performance, service, and engagement data
    /// - Shows provider service trends and performance indicators
    /// - Access restricted to administrators and authorized users
    /// - Used for provider analytics and performance monitoring
    /// - Includes comprehensive provider information and metrics
    /// - Provides data for provider analysis and service optimization
    /// - Handles provider analytics data retrieval and error responses
    /// </remarks>
    [HttpGet("providers")]
    public async Task<JsonModel> GetProviderAnalytics()
    {
        return await _analyticsService.GetProviderAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves comprehensive system analytics and performance metrics.
    /// This endpoint provides detailed system analytics including system performance,
    /// resource utilization, system health, and infrastructure analytics.
    /// </summary>
    /// <returns>JsonModel containing system analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive system analytics and performance metrics
    /// - Includes system performance, resource, and health data
    /// - Shows system utilization and infrastructure metrics
    /// - Access restricted to administrators and authorized users
    /// - Used for system analytics and performance monitoring
    /// - Includes comprehensive system information and metrics
    /// - Provides data for system analysis and infrastructure optimization
    /// - Handles system analytics data retrieval and error responses
    /// </remarks>
    [HttpGet("system")]
    public async Task<JsonModel> GetSystemAnalytics()
    {
        return await _analyticsService.GetSystemAnalyticsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get system health
    /// </summary>
    [HttpGet("system/health")]
    public async Task<JsonModel> GetSystemHealth()
    {
        return await _analyticsService.GetSystemHealthAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get revenue analytics
    /// </summary>
    [HttpGet("revenue")]
    public async Task<JsonModel> GetRevenueAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetRevenueAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get user activity analytics
    /// </summary>
    [HttpGet("user-activity")]
    public async Task<JsonModel> GetUserActivityAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetUserActivityAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get appointment analytics
    /// </summary>
    [HttpGet("appointments")]
    public async Task<JsonModel> GetAppointmentAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetAppointmentAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription analytics with plan filter
    /// </summary>
    [HttpGet("subscriptions/plan/{planId}")]
    public async Task<JsonModel> GetSubscriptionAnalyticsByPlan(string planId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, endDate, planId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription dashboard
    /// </summary>
    [HttpGet("subscriptions/dashboard")]
    public async Task<JsonModel> GetSubscriptionDashboard([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetSubscriptionDashboardAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get churn analytics
    /// </summary>
    [HttpGet("churn")]
    public async Task<JsonModel> GetChurnAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetChurnAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get plan analytics
    /// </summary>
    [HttpGet("plans")]
    public async Task<JsonModel> GetPlanAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetPlanAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get usage analytics
    /// </summary>
    [HttpGet("usage")]
    public async Task<JsonModel> GetUsageAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetUsageAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate subscription report
    /// </summary>
    [HttpGet("reports/subscriptions")]
    public async Task<JsonModel> GenerateSubscriptionReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GenerateSubscriptionReportAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate billing report
    /// </summary>
    [HttpGet("reports/billing")]
    public async Task<JsonModel> GenerateBillingReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GenerateBillingReportAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate user report
    /// </summary>
    [HttpGet("reports/users")]
    public async Task<JsonModel> GenerateUserReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GenerateUserReportAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate provider report
    /// </summary>
    [HttpGet("reports/providers")]
    public async Task<JsonModel> GenerateProviderReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GenerateProviderReportAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Export subscription analytics
    /// </summary>
    [HttpGet("export/subscriptions")]
    public async Task<JsonModel> ExportSubscriptionAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.ExportSubscriptionAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }
} 
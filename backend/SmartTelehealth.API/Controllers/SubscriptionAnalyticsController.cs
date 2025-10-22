using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive subscription analytics and reporting functionality.
/// This controller provides detailed analytics for subscription performance, revenue tracking,
/// churn analysis, usage statistics, and business intelligence. It supports data export,
/// advanced reporting, and real-time analytics for strategic decision-making.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionAnalyticsController : BaseController
{
    private readonly ISubscriptionAnalyticsService _analyticsService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionPlanService _subscriptionPlanService;

    /// <summary>
    /// Initializes a new instance of the SubscriptionAnalyticsController with the required services.
    /// </summary>
    /// <param name="analyticsService">Service for handling subscription analytics and reporting</param>
    /// <param name="subscriptionService">Service for handling subscription operations</param>
    /// <param name="subscriptionPlanService">Service for handling subscription plan operations</param>
    public SubscriptionAnalyticsController(
        ISubscriptionAnalyticsService analyticsService,
        ISubscriptionService subscriptionService,
        ISubscriptionPlanService subscriptionPlanService)
    {
        _analyticsService = analyticsService;
        _subscriptionService = subscriptionService;
        _subscriptionPlanService = subscriptionPlanService;
    }

    /// <summary>
    /// Retrieves comprehensive subscription analytics for a specified date range.
    /// This endpoint provides detailed analytics including subscription metrics, growth trends,
    /// performance indicators, and key performance indicators (KPIs) for business intelligence.
    /// </summary>
    /// <param name="startDate">Start date for analytics data range (optional)</param>
    /// <param name="endDate">End date for analytics data range (optional)</param>
    /// <returns>JsonModel containing comprehensive subscription analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive subscription analytics including growth metrics
    /// - Provides subscription performance indicators and trends
    /// - Includes subscription lifecycle analytics and conversion rates
    /// - Shows subscription distribution by plan, status, and demographics
    /// - Access restricted to administrators and authorized users
    /// - Used for business intelligence and strategic decision-making
    /// - Supports date range filtering for historical analysis
    /// - Includes subscription health metrics and performance indicators
    /// </remarks>
    [HttpGet]
    public async Task<JsonModel> GetSubscriptionAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Alias endpoint for subscription analytics
    /// </summary>
    [HttpGet("subscription-analytics")]
    public async Task<JsonModel> GetSubscriptionAnalyticsAlias([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get Monthly Recurring Revenue (MRR) metrics
    /// </summary>
    [HttpGet("mrr")]
    public async Task<JsonModel> GetMRR([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var revenueResult = await _analyticsService.GetRevenueAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
        return revenueResult;
    }

    /// <summary>
    /// Get churn rate metrics
    /// </summary>
    [HttpGet("churn-rate")]
    public async Task<JsonModel> GetChurnRate([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var churnResult = await _analyticsService.GetChurnAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
        return churnResult;
    }

    /// <summary>
    /// Retrieves detailed revenue analytics for a specified date range.
    /// This endpoint provides comprehensive revenue analysis including MRR, ARR, revenue trends,
    /// and financial performance metrics for subscription business intelligence.
    /// </summary>
    /// <param name="startDate">Start date for revenue analytics data range (optional)</param>
    /// <param name="endDate">End date for revenue analytics data range (optional)</param>
    /// <returns>JsonModel containing detailed revenue analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed revenue analytics including MRR and ARR calculations
    /// - Provides revenue trends and growth analysis
    /// - Shows revenue distribution by plan, customer segment, and geography
    /// - Includes revenue forecasting and predictive analytics
    /// - Access restricted to administrators and authorized users
    /// - Used for financial reporting and revenue optimization
    /// - Supports date range filtering for historical revenue analysis
    /// - Includes revenue per customer and lifetime value metrics
    /// </remarks>
    [HttpGet("revenue")]
    public async Task<JsonModel> GetRevenueAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _analyticsService.GetRevenueAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves churn and retention analytics for a specified date range.
    /// This endpoint provides detailed analysis of customer churn, retention rates,
    /// and customer lifetime value for subscription business optimization.
    /// </summary>
    /// <param name="startDate">Start date for churn analytics data range (optional)</param>
    /// <param name="endDate">End date for churn analytics data range (optional)</param>
    /// <returns>JsonModel containing churn and retention analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed churn analysis including churn rate and reasons
    /// - Provides retention rate analysis and customer lifetime value
    /// - Shows churn patterns by plan, customer segment, and demographics
    /// - Includes predictive churn analysis and risk assessment
    /// - Access restricted to administrators and authorized users
    /// - Used for customer retention strategies and churn prevention
    /// - Supports date range filtering for historical churn analysis
    /// - Includes cohort analysis and retention cohort metrics
    /// </remarks>
    [HttpGet("churn")]
    public async Task<JsonModel> GetChurnAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _analyticsService.GetChurnAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves usage analytics for a specific subscription.
    /// This endpoint provides detailed usage statistics, privilege consumption,
    /// and usage patterns for individual subscription analysis and optimization.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription</param>
    /// <param name="startDate">Start date for usage analytics data range (optional)</param>
    /// <param name="endDate">End date for usage analytics data range (optional)</param>
    /// <returns>JsonModel containing detailed usage analytics for the subscription</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed usage statistics for the specified subscription
    /// - Provides privilege consumption analysis and usage patterns
    /// - Shows usage trends and peak usage periods
    /// - Includes usage efficiency metrics and optimization recommendations
    /// - Access restricted to subscription owner or administrators
    /// - Used for subscription optimization and usage monitoring
    /// - Supports date range filtering for historical usage analysis
    /// - Includes usage alerts and threshold monitoring
    /// </remarks>
    [HttpGet("usage/{subscriptionId}")]
    public async Task<JsonModel> GetUsageAnalytics(string subscriptionId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _analyticsService.GetUsageAnalyticsAsync(subscriptionId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Exports analytics data in the specified format for external analysis and reporting.
    /// This endpoint allows users to export subscription analytics data in various formats
    /// for further analysis, reporting, and business intelligence purposes.
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
    /// - Access restricted to administrators and authorized users
    /// - Used for external reporting and business intelligence
    /// - Supports various export formats for different use cases
    /// - Includes data validation and export optimization
    /// - Provides export status tracking and download links
    /// </remarks>
    [HttpGet("export")]
    public async Task<JsonModel> ExportAnalytics([FromQuery] string format = "csv", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.ExportAnalyticsAsync(format, startDate, endDate, GetToken(HttpContext));
    }

    #region Advanced Analytics

    /// <summary>
    /// Retrieves subscription growth analytics including new subscriptions, cancellations, and net growth.
    /// This endpoint provides detailed growth metrics for subscription business analysis and forecasting.
    /// </summary>
    /// <param name="startDate">Start date for growth analytics data range (optional)</param>
    /// <param name="endDate">End date for growth analytics data range (optional)</param>
    /// <param name="period">Analysis period (daily, weekly, monthly, quarterly, yearly) - default is monthly</param>
    /// <returns>JsonModel containing subscription growth analytics data</returns>
    [HttpGet("growth")]
    public async Task<JsonModel> GetGrowthAnalytics(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string period = "monthly")
    {
        return await _analyticsService.GetGrowthAnalyticsAsync(startDate, endDate, period, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves subscription plan performance analytics including popularity, conversion rates, and retention.
    /// This endpoint provides detailed plan performance metrics for subscription optimization.
    /// </summary>
    /// <param name="startDate">Start date for plan analytics data range (optional)</param>
    /// <param name="endDate">End date for plan analytics data range (optional)</param>
    /// <returns>JsonModel containing subscription plan performance analytics data</returns>
    [HttpGet("plans/performance")]
    public async Task<JsonModel> GetPlanPerformanceAnalytics(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        // Placeholder implementation - to be implemented in analytics service
        return new JsonModel 
        { 
            data = new { message = "Plan performance analytics feature not yet implemented" }, 
            Message = "Plan performance analytics not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Retrieves customer segmentation analytics including demographics, behavior patterns, and value analysis.
    /// This endpoint provides detailed customer segmentation for targeted marketing and retention strategies.
    /// </summary>
    /// <param name="startDate">Start date for segmentation analytics data range (optional)</param>
    /// <param name="endDate">End date for segmentation analytics data range (optional)</param>
    /// <returns>JsonModel containing customer segmentation analytics data</returns>
    [HttpGet("customers/segmentation")]
    public async Task<JsonModel> GetCustomerSegmentationAnalytics(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        // Placeholder implementation - to be implemented in analytics service
        return new JsonModel 
        { 
            data = new { message = "Customer segmentation analytics feature not yet implemented" }, 
            Message = "Customer segmentation analytics not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Retrieves subscription lifecycle analytics including conversion funnels, stage analysis, and optimization opportunities.
    /// This endpoint provides detailed lifecycle metrics for subscription process optimization.
    /// </summary>
    /// <param name="startDate">Start date for lifecycle analytics data range (optional)</param>
    /// <param name="endDate">End date for lifecycle analytics data range (optional)</param>
    /// <returns>JsonModel containing subscription lifecycle analytics data</returns>
    [HttpGet("lifecycle")]
    public async Task<JsonModel> GetLifecycleAnalytics(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        // Placeholder implementation - to be implemented in analytics service
        return new JsonModel 
        { 
            data = new { message = "Lifecycle analytics feature not yet implemented" }, 
            Message = "Lifecycle analytics not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Retrieves subscription health metrics including active subscriptions, engagement levels, and risk indicators.
    /// This endpoint provides comprehensive health monitoring for subscription business sustainability.
    /// </summary>
    /// <param name="startDate">Start date for health analytics data range (optional)</param>
    /// <param name="endDate">End date for health analytics data range (optional)</param>
    /// <returns>JsonModel containing subscription health metrics data</returns>
    [HttpGet("health")]
    public async Task<JsonModel> GetHealthMetrics(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        // Placeholder implementation - to be implemented in analytics service
        return new JsonModel 
        { 
            data = new { message = "Health metrics feature not yet implemented" }, 
            Message = "Health metrics not implemented", 
            StatusCode = 501 
        };
    }

    #endregion

    #region Real-time Analytics

    /// <summary>
    /// Retrieves real-time subscription metrics including current active subscriptions, recent activity, and live updates.
    /// This endpoint provides up-to-the-minute subscription data for real-time monitoring and dashboards.
    /// </summary>
    /// <returns>JsonModel containing real-time subscription metrics data</returns>
    [HttpGet("realtime")]
    public async Task<JsonModel> GetRealTimeMetrics()
    {
        return await _analyticsService.GetRealTimeMetricsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves subscription alerts and notifications including overdue payments, expiring subscriptions, and anomalies.
    /// This endpoint provides critical alerts for proactive subscription management and intervention.
    /// </summary>
    /// <param name="alertType">Type of alerts to retrieve (all, payment, expiration, usage, anomaly) - default is all</param>
    /// <returns>JsonModel containing subscription alerts and notifications data</returns>
    [HttpGet("alerts")]
    public async Task<JsonModel> GetSubscriptionAlerts([FromQuery] string alertType = "all")
    {
        // Placeholder implementation - to be implemented in analytics service
        return new JsonModel 
        { 
            data = new { message = "Subscription alerts feature not yet implemented" }, 
            Message = "Subscription alerts not implemented", 
            StatusCode = 501 
        };
    }

    #endregion

    #region Reporting and Dashboards

    /// <summary>
    /// Generates comprehensive subscription reports including executive summaries, detailed analysis, and recommendations.
    /// This endpoint provides formatted reports for business stakeholders and decision-makers.
    /// </summary>
    /// <param name="reportType">Type of report to generate (executive, detailed, summary, custom) - default is executive</param>
    /// <param name="startDate">Start date for report data range (optional)</param>
    /// <param name="endDate">End date for report data range (optional)</param>
    /// <returns>JsonModel containing the generated subscription report</returns>
    [HttpGet("reports")]
    public async Task<JsonModel> GenerateSubscriptionReport(
        [FromQuery] string reportType = "executive",
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        // Placeholder implementation - to be implemented in analytics service
        return new JsonModel 
        { 
            data = new { message = "Subscription report generation feature not yet implemented" }, 
            Message = "Subscription report generation not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Retrieves dashboard data including key metrics, charts, and visualizations for subscription management.
    /// This endpoint provides comprehensive dashboard data for subscription monitoring and analysis.
    /// </summary>
    /// <param name="dashboardType">Type of dashboard (overview, revenue, growth, churn, usage) - default is overview</param>
    /// <param name="startDate">Start date for dashboard data range (optional)</param>
    /// <param name="endDate">End date for dashboard data range (optional)</param>
    /// <returns>JsonModel containing dashboard data and visualizations</returns>
    [HttpGet("dashboard")]
    public async Task<JsonModel> GetDashboardData(
        [FromQuery] string dashboardType = "overview",
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetDashboardDataAsync(dashboardType, startDate, endDate, GetToken(HttpContext));
    }

    #endregion

    #region Data Export and Integration

    /// <summary>
    /// Exports subscription data in bulk for external systems and data warehouses.
    /// This endpoint provides comprehensive data export for business intelligence and analytics platforms.
    /// </summary>
    /// <param name="dataType">Type of data to export (subscriptions, analytics, usage, billing) - default is subscriptions</param>
    /// <param name="format">Export format (csv, json, excel, xml) - default is csv</param>
    /// <param name="startDate">Start date for export data range (optional)</param>
    /// <param name="endDate">End date for export data range (optional)</param>
    /// <returns>JsonModel containing the bulk export data</returns>
    [HttpGet("bulk-export")]
    public async Task<JsonModel> BulkExportData(
        [FromQuery] string dataType = "subscriptions",
        [FromQuery] string format = "csv",
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        // Placeholder implementation - to be implemented in analytics service
        return new JsonModel 
        { 
            data = new { message = "Bulk export feature not yet implemented" }, 
            Message = "Bulk export not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Retrieves analytics API status and available endpoints for integration monitoring.
    /// This endpoint provides system status and API health information for analytics services.
    /// </summary>
    /// <returns>JsonModel containing analytics API status and health information</returns>
    [HttpGet("status")]
    public async Task<JsonModel> GetAnalyticsStatus()
    {
        // Placeholder implementation - to be implemented in analytics service
        return new JsonModel 
        { 
            data = new { message = "Analytics status feature not yet implemented" }, 
            Message = "Analytics status not implemented", 
            StatusCode = 501 
        };
    }

    #endregion
} 
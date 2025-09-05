using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for subscription analytics and reporting functionality.
/// This controller provides comprehensive analytics for subscription performance,
/// revenue tracking, churn analysis, and usage statistics. It supports data export
/// and detailed reporting for business intelligence and decision-making.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class SubscriptionAnalyticsController : BaseController
{
    private readonly ISubscriptionAnalyticsService _analyticsService;

    /// <summary>
    /// Initializes a new instance of the SubscriptionAnalyticsController with the required analytics service.
    /// </summary>
    /// <param name="analyticsService">Service for handling subscription analytics and reporting</param>
    public SubscriptionAnalyticsController(ISubscriptionAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
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
} 
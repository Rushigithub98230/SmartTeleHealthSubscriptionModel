using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionAnalyticsController : BaseController
{
    private readonly ISubscriptionAnalyticsService _analyticsService;

    public SubscriptionAnalyticsController(ISubscriptionAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get comprehensive subscription analytics for a date range
    /// </summary>
    [HttpGet]
    public async Task<JsonModel> GetSubscriptionAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get detailed revenue analytics for a date range
    /// </summary>
    [HttpGet("revenue")]
    public async Task<JsonModel> GetRevenueAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _analyticsService.GetRevenueAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get churn and retention analytics for a date range
    /// </summary>
    [HttpGet("churn")]
    public async Task<JsonModel> GetChurnAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _analyticsService.GetChurnAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get usage analytics for a specific subscription
    /// </summary>
    [HttpGet("usage/{subscriptionId}")]
    public async Task<JsonModel> GetUsageAnalytics(string subscriptionId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _analyticsService.GetUsageAnalyticsAsync(subscriptionId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Export analytics data in specified format (CSV/JSON)
    /// </summary>
    [HttpGet("export")]
    public async Task<JsonModel> ExportAnalytics([FromQuery] string format = "csv", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.ExportAnalyticsAsync(format, startDate, endDate, GetToken(HttpContext));
    }
} 
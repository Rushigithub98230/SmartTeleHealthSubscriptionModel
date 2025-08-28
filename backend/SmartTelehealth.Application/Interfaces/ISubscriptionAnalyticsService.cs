using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ISubscriptionAnalyticsService
{
    /// <summary>
    /// Get comprehensive subscription analytics for a date range
    /// </summary>
    Task<JsonModel> GetSubscriptionAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);

    /// <summary>
    /// Get detailed revenue analytics for a date range
    /// </summary>
    Task<JsonModel> GetRevenueAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);

    /// <summary>
    /// Get churn and retention analytics for a date range
    /// </summary>
    Task<JsonModel> GetChurnAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);

    /// <summary>
    /// Get usage analytics for a specific subscription
    /// </summary>
    Task<JsonModel> GetUsageAnalyticsAsync(string subscriptionId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);

    /// <summary>
    /// Export analytics data in specified format (CSV/JSON)
    /// </summary>
    Task<JsonModel> ExportAnalyticsAsync(string format, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
}

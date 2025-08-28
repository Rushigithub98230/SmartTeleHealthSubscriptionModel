using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Linq;
using System.Text;

namespace SmartTelehealth.Application.Services;

public class SubscriptionAnalyticsService : ISubscriptionAnalyticsService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<SubscriptionAnalyticsService> _logger;

    public SubscriptionAnalyticsService(
        ISubscriptionRepository subscriptionRepository,
        IBillingRepository billingRepository,
        IUserRepository userRepository,
        IAuditService auditService,
        ILogger<SubscriptionAnalyticsService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _billingRepository = billingRepository;
        _userRepository = userRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<JsonModel> GetSubscriptionAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;

            var subscriptions = await _subscriptionRepository.GetSubscriptionsByDateRangeAsync(start, end);
            var billingRecords = await _billingRepository.GetBillingRecordsByDateRangeAsync(start, end);

            var analytics = new
            {
                Period = new { StartDate = start, EndDate = end },
                SubscriptionMetrics = await CalculateSubscriptionMetricsAsync(subscriptions, start, end),
                RevenueMetrics = await CalculateRevenueMetricsAsync(billingRecords, start, end),
                ChurnMetrics = await CalculateChurnMetricsAsync(subscriptions, start, end),
                GrowthMetrics = await CalculateGrowthMetricsAsync(subscriptions, start, end),
                PlanDistribution = await CalculatePlanDistributionAsync(subscriptions),
                GeographicDistribution = await CalculateGeographicDistributionAsync(subscriptions),
                UserEngagement = await CalculateUserEngagementAsync(subscriptions, start, end)
            };

            return new JsonModel
            {
                data = analytics,
                Message = "Subscription analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve subscription analytics",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetRevenueAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;

            var billingRecords = await _billingRepository.GetBillingRecordsByDateRangeAsync(start, end);

            var revenueAnalytics = new
            {
                Period = new { StartDate = start, EndDate = end },
                TotalRevenue = billingRecords.Where(b => b.Status == BillingRecord.BillingStatus.Paid).Sum(b => b.TotalAmount),
                MonthlyRecurringRevenue = await CalculateMonthlyRecurringRevenueAsync(billingRecords),
                AnnualRecurringRevenue = await CalculateAnnualRecurringRevenueAsync(billingRecords),
                RevenueByPlan = await CalculateRevenueByPlanAsync(billingRecords),
                RevenueByMonth = await CalculateRevenueByMonthAsync(billingRecords, start, end),
                PaymentSuccessRate = await CalculatePaymentSuccessRateAsync(billingRecords),
                AverageRevenuePerUser = await CalculateAverageRevenuePerUserAsync(billingRecords),
                RevenueGrowth = await CalculateRevenueGrowthAsync(billingRecords, start, end)
            };

            return new JsonModel
            {
                data = revenueAnalytics,
                Message = "Revenue analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue analytics by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve revenue analytics",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetChurnAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;

            var subscriptions = await _subscriptionRepository.GetSubscriptionsByDateRangeAsync(start, end);

            var churnAnalytics = new
            {
                Period = new { StartDate = start, EndDate = end },
                ChurnRate = await CalculateChurnRateAsync(subscriptions, start, end),
                ChurnByPlan = await CalculateChurnByPlanAsync(subscriptions, start, end),
                ChurnByMonth = await CalculateChurnByMonthAsync(subscriptions, start, end),
                ChurnReasons = await AnalyzeChurnReasonsAsync(subscriptions, start, end),
                RetentionByPlan = await CalculateRetentionByPlanAsync(subscriptions, start, end),
                CustomerLifetimeValue = await CalculateCustomerLifetimeValueAsync(subscriptions, start, end)
            };

            return new JsonModel
            {
                data = churnAnalytics,
                Message = "Churn analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting churn analytics by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve churn analytics",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetUsageAnalyticsAsync(string subscriptionId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var start = startDate ?? subscription.StartDate;
            var end = endDate ?? DateTime.UtcNow;

            var usageAnalytics = new
            {
                SubscriptionId = subscriptionId,
                Period = new { StartDate = start, EndDate = end },
                FeatureUsage = await CalculateFeatureUsageAsync(subscription, start, end),
                UsageTrends = await CalculateUsageTrendsAsync(subscription, start, end),
                PeakUsageTimes = await CalculatePeakUsageTimesAsync(subscription, start, end),
                UserBehavior = await AnalyzeUserBehaviorAsync(subscription, start, end)
            };

            return new JsonModel
            {
                data = usageAnalytics,
                Message = "Usage analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage analytics for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve usage analytics",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ExportAnalyticsAsync(string format, DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;

            var analytics = await GetSubscriptionAnalyticsAsync(start, end, tokenModel);
            if (analytics.StatusCode != 200)
            {
                return analytics;
            }

            byte[] exportData;
            string fileName;
            string contentType;

            switch (format.ToLower())
            {
                case "csv":
                    exportData = await ExportToCsvAsync(analytics.data);
                    fileName = $"subscription_analytics_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
                    contentType = "text/csv";
                    break;
                case "json":
                    exportData = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(analytics.data));
                    fileName = $"subscription_analytics_{start:yyyyMMdd}_{end:yyyyMMdd}.json";
                    contentType = "application/json";
                    break;
                default:
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Unsupported format. Use 'csv' or 'json'",
                        StatusCode = 400
                    };
            }

            var result = new
            {
                FileContent = Convert.ToBase64String(exportData),
                FileName = fileName,
                ContentType = contentType,
                FileSize = exportData.Length,
                Period = new { StartDate = start, EndDate = end }
            };

            return new JsonModel
            {
                data = result,
                Message = $"Analytics exported successfully in {format} format",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics in {Format} format by user {UserId}", format, tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to export analytics",
                StatusCode = 500
            };
        }
    }

    #region Private Helper Methods

    private async Task<object> CalculateSubscriptionMetricsAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var totalSubscriptions = subscriptions.Count();
        var activeSubscriptions = subscriptions.Count(s => s.Status == "Active");
        var trialSubscriptions = subscriptions.Count(s => s.Status == "TrialActive");
        var cancelledSubscriptions = subscriptions.Count(s => s.Status == "Cancelled");

        return new
        {
            TotalSubscriptions = totalSubscriptions,
            ActiveSubscriptions = activeSubscriptions,
            TrialSubscriptions = trialSubscriptions,
            CancelledSubscriptions = cancelledSubscriptions,
            ActivationRate = totalSubscriptions > 0 ? (double)activeSubscriptions / totalSubscriptions * 100 : 0,
            TrialConversionRate = trialSubscriptions > 0 ? (double)activeSubscriptions / trialSubscriptions * 100 : 0
        };
    }

    private async Task<object> CalculateRevenueMetricsAsync(IEnumerable<BillingRecord> billingRecords, DateTime start, DateTime end)
    {
        var paidRecords = billingRecords.Where(b => b.Status == BillingRecord.BillingStatus.Paid);
        var totalRevenue = paidRecords.Sum(b => b.TotalAmount);
        var averageOrderValue = paidRecords.Any() ? paidRecords.Average(b => b.TotalAmount) : 0;

        return new
        {
            TotalRevenue = totalRevenue,
            AverageOrderValue = averageOrderValue,
            TotalTransactions = paidRecords.Count(),
            RevenuePerDay = (end - start).TotalDays > 0 ? totalRevenue / (decimal)(end - start).TotalDays : 0
        };
    }

    private async Task<object> CalculateChurnMetricsAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var cancelledInPeriod = subscriptions.Count(s => s.Status == "Cancelled" && s.CancelledDate >= start && s.CancelledDate <= end);
        var totalAtStart = subscriptions.Count(s => s.StartDate <= start);
        var churnRate = totalAtStart > 0 ? (double)cancelledInPeriod / totalAtStart * 100 : 0;

        return new
        {
            ChurnRate = churnRate,
            CancelledSubscriptions = cancelledInPeriod,
            TotalAtStart = totalAtStart
        };
    }

    private async Task<object> CalculateGrowthMetricsAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var newSubscriptions = subscriptions.Count(s => s.StartDate >= start && s.StartDate <= end);
        var growthRate = start > DateTime.MinValue ? (double)newSubscriptions / start.Day * 100 : 0;

        return new
        {
            NewSubscriptions = newSubscriptions,
            GrowthRate = growthRate,
            AverageDailyGrowth = (end - start).TotalDays > 0 ? (double)newSubscriptions / (end - start).TotalDays : 0
        };
    }

    private async Task<object> CalculatePlanDistributionAsync(IEnumerable<Subscription> subscriptions)
    {
        var planGroups = subscriptions.GroupBy(s => s.SubscriptionPlan.Name)
                                   .Select(g => new { Plan = g.Key, Count = g.Count() })
                                   .OrderByDescending(x => x.Count);

        return planGroups;
    }

    private async Task<object> CalculateGeographicDistributionAsync(IEnumerable<Subscription> subscriptions)
    {
        // This would require user location data
        // For now, return placeholder
        return new { Message = "Geographic distribution requires user location data" };
    }

    private async Task<object> CalculateUserEngagementAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var activeUsers = subscriptions.Count(s => s.Status == "Active" && s.LastUsedDate >= start);
        var totalUsers = subscriptions.Count();

        return new
        {
            ActiveUsers = activeUsers,
            TotalUsers = totalUsers,
            EngagementRate = totalUsers > 0 ? (double)activeUsers / totalUsers * 100 : 0
        };
    }

    private async Task<decimal> CalculateMonthlyRecurringRevenueAsync(IEnumerable<BillingRecord> billingRecords)
    {
        var monthlyRecords = billingRecords.Where(b => b.Type == BillingRecord.BillingType.Subscription && 
                                                      b.Status == BillingRecord.BillingStatus.Paid);
        return monthlyRecords.Sum(b => b.TotalAmount);
    }

    private async Task<decimal> CalculateAnnualRecurringRevenueAsync(IEnumerable<BillingRecord> billingRecords)
    {
        var annualRecords = billingRecords.Where(b => b.Type == BillingRecord.BillingType.Subscription && 
                                                     b.Status == BillingRecord.BillingStatus.Paid);
        return annualRecords.Sum(b => b.TotalAmount) * 12;
    }

    private async Task<object> CalculateRevenueByPlanAsync(IEnumerable<BillingRecord> billingRecords)
    {
        // This would require joining with subscription data
        // For now, return placeholder
        return new { Message = "Revenue by plan requires subscription plan data" };
    }

    private async Task<object> CalculateRevenueByMonthAsync(IEnumerable<BillingRecord> billingRecords, DateTime start, DateTime end)
    {
        var monthlyRevenue = billingRecords.Where(b => b.Status == BillingRecord.BillingStatus.Paid)
                                         .GroupBy(b => new { b.BillingDate.Year, b.BillingDate.Month })
                                         .Select(g => new { 
                                             Month = $"{g.Key.Year}-{g.Key.Month:00}", 
                                             Revenue = g.Sum(b => b.TotalAmount) 
                                         })
                                         .OrderBy(x => x.Month);

        return monthlyRevenue;
    }

    private async Task<decimal> CalculatePaymentSuccessRateAsync(IEnumerable<BillingRecord> billingRecords)
    {
        var totalAttempts = billingRecords.Count();
        var successfulPayments = billingRecords.Count(b => b.Status == BillingRecord.BillingStatus.Paid);
        
        return totalAttempts > 0 ? (decimal)successfulPayments / totalAttempts * 100 : 0;
    }

    private async Task<decimal> CalculateAverageRevenuePerUserAsync(IEnumerable<BillingRecord> billingRecords)
    {
        var paidRecords = billingRecords.Where(b => b.Status == BillingRecord.BillingStatus.Paid);
        var uniqueUsers = paidRecords.Select(b => b.UserId).Distinct().Count();
        
        return uniqueUsers > 0 ? paidRecords.Sum(b => b.TotalAmount) / uniqueUsers : 0;
    }

    private async Task<object> CalculateRevenueGrowthAsync(IEnumerable<BillingRecord> billingRecords, DateTime start, DateTime end)
    {
        var midPoint = start.AddDays((end - start).TotalDays / 2);
        
        var firstHalfRevenue = billingRecords.Where(b => b.BillingDate >= start && b.BillingDate < midPoint && 
                                                        b.Status == BillingRecord.BillingStatus.Paid)
                                           .Sum(b => b.TotalAmount);
        
        var secondHalfRevenue = billingRecords.Where(b => b.BillingDate >= midPoint && b.BillingDate <= end && 
                                                         b.Status == BillingRecord.BillingStatus.Paid)
                                            .Sum(b => b.TotalAmount);

        var growthRate = firstHalfRevenue > 0 ? (secondHalfRevenue - firstHalfRevenue) / firstHalfRevenue * 100 : 0;

        return new
        {
            FirstHalfRevenue = firstHalfRevenue,
            SecondHalfRevenue = secondHalfRevenue,
            GrowthRate = growthRate
        };
    }

    private async Task<decimal> CalculateChurnRateAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var cancelledInPeriod = subscriptions.Count(s => s.Status == "Cancelled" && s.CancelledDate >= start && s.CancelledDate <= end);
        var totalAtStart = subscriptions.Count(s => s.StartDate <= start);
        
        return totalAtStart > 0 ? (decimal)cancelledInPeriod / totalAtStart * 100 : 0;
    }

    private async Task<object> CalculateChurnByPlanAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var churnByPlan = subscriptions.Where(s => s.Status == "Cancelled" && s.CancelledDate >= start && s.CancelledDate <= end)
                                     .GroupBy(s => s.SubscriptionPlan.Name)
                                     .Select(g => new { Plan = g.Key, ChurnedCount = g.Count() })
                                     .OrderByDescending(x => x.ChurnedCount);

        return churnByPlan;
    }

    private async Task<object> CalculateChurnByMonthAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var churnByMonth = subscriptions.Where(s => s.Status == "Cancelled" && s.CancelledDate >= start && s.CancelledDate <= end)
                                      .GroupBy(s => new { s.CancelledDate.Value.Year, s.CancelledDate.Value.Month })
                                      .Select(g => new { 
                                          Month = $"{g.Key.Year}-{g.Key.Month:00}", 
                                          ChurnedCount = g.Count() 
                                      })
                                      .OrderBy(x => x.Month);

        return churnByMonth;
    }

    private async Task<object> AnalyzeChurnReasonsAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var churnedSubscriptions = subscriptions.Where(s => s.Status == "Cancelled" && s.CancelledDate >= start && s.CancelledDate <= end);
        
        var reasons = churnedSubscriptions.GroupBy(s => s.CancellationReason ?? "No reason provided")
                                        .Select(g => new { Reason = g.Key, Count = g.Count() })
                                        .OrderByDescending(x => x.Count);

        return reasons;
    }

    private async Task<object> CalculateRetentionByPlanAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var retentionByPlan = subscriptions.GroupBy(s => s.SubscriptionPlan.Name)
                                         .Select(g => new
                                         {
                                             Plan = g.Key,
                                             TotalSubscriptions = g.Count(),
                                             ActiveSubscriptions = g.Count(s => s.Status == "Active"),
                                             RetentionRate = g.Count() > 0 ? (double)g.Count(s => s.Status == "Active") / g.Count() * 100 : 0
                                         })
                                         .OrderByDescending(x => x.RetentionRate);

        return retentionByPlan;
    }

    private async Task<decimal> CalculateCustomerLifetimeValueAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var activeSubscriptions = subscriptions.Where(s => s.Status == "Active");
        var totalValue = activeSubscriptions.Sum(s => s.CurrentPrice);
        var uniqueCustomers = activeSubscriptions.Select(s => s.UserId).Distinct().Count();

        return uniqueCustomers > 0 ? totalValue / uniqueCustomers : 0;
    }

    private async Task<object> CalculateFeatureUsageAsync(Subscription subscription, DateTime start, DateTime end)
    {
        // This would require feature usage tracking data
        // For now, return placeholder
        return new { Message = "Feature usage requires detailed usage tracking data" };
    }

    private async Task<object> CalculateUsageTrendsAsync(Subscription subscription, DateTime start, DateTime end)
    {
        // This would require time-series usage data
        // For now, return placeholder
        return new { Message = "Usage trends require time-series usage data" };
    }

    private async Task<object> CalculatePeakUsageTimesAsync(Subscription subscription, DateTime start, DateTime end)
    {
        // This would require hourly usage data
        // For now, return placeholder
        return new { Message = "Peak usage times require hourly usage data" };
    }

    private async Task<object> AnalyzeUserBehaviorAsync(Subscription subscription, DateTime start, DateTime end)
    {
        // This would require user behavior analytics
        // For now, return placeholder
        return new { Message = "User behavior analysis requires detailed user interaction data" };
    }

    private async Task<byte[]> ExportToCsvAsync(object data)
    {
        // Simple CSV export implementation
        var csv = new StringBuilder();
        csv.AppendLine("Metric,Value");
        
        // Add basic metrics
        csv.AppendLine($"Export Date,{DateTime.UtcNow:yyyy-MM-dd}");
        csv.AppendLine($"Data Period,{DateTime.UtcNow.AddMonths(-12):yyyy-MM-dd} to {DateTime.UtcNow:yyyy-MM-dd}");
        
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    #endregion
}

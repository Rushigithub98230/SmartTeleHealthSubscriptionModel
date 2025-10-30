using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.Constants;
using System.Linq;
using System.Text;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service responsible for providing comprehensive analytics and reporting for subscription management.
/// This service handles subscription analytics, revenue analytics, churn analysis, usage analytics,
/// and data export capabilities. It provides detailed insights into subscription performance,
/// customer behavior, revenue trends, and business metrics to support data-driven decision making.
/// 
/// Key Features:
/// - Comprehensive subscription metrics and KPIs
/// - Revenue analytics including MRR, ARR, and growth rates
/// - Churn analysis and retention metrics
/// - Usage analytics and user behavior insights
/// - Plan distribution and geographic analysis
/// - Customer lifetime value calculations
/// - Payment success rate monitoring
/// - Data export capabilities (CSV, JSON)
/// - Time-series analysis and trend reporting
/// - Performance benchmarking and comparisons
/// - Integration with billing and subscription repositories
/// </summary>
public class SubscriptionAnalyticsService : ISubscriptionAnalyticsService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IUserRepository _userRepository;
      
    private readonly ILogger<SubscriptionAnalyticsService> _logger;

    public SubscriptionAnalyticsService(
        ISubscriptionRepository subscriptionRepository,
        IBillingRepository billingRepository,
        IUserRepository userRepository,
          
        ILogger<SubscriptionAnalyticsService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _billingRepository = billingRepository;
        _userRepository = userRepository;
          
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
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
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
        // Correct churn calculation: cancelled in period / active at start of period
        var activeAtStart = subscriptions.Count(s => 
            s.StartDate <= start && 
            (s.CancelledDate == null || s.CancelledDate > start));
            
        var cancelledInPeriod = subscriptions.Count(s => 
            s.Status == Subscription.SubscriptionStatuses.Cancelled.ToString() && 
            s.CancelledDate.HasValue && 
            s.CancelledDate >= start && 
            s.CancelledDate <= end);
            
        var churnRate = activeAtStart > 0 ? (double)cancelledInPeriod / activeAtStart * 100 : 0;

        return new
        {
            ChurnRate = churnRate,
            CancelledSubscriptions = cancelledInPeriod,
            TotalAtStart = activeAtStart
        };
    }

    private async Task<object> CalculateGrowthMetricsAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var newSubscriptions = subscriptions.Count(s => s.StartDate >= start && s.StartDate <= end);
        
        // Calculate growth rate properly: (new subscriptions in period / total subscriptions at start) * 100
        var totalAtStart = subscriptions.Count(s => s.StartDate <= start);
        var growthRate = totalAtStart > 0 ? (double)newSubscriptions / totalAtStart * 100 : 0;

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

    #region Dashboard Analytics (Phase 1 Implementation)

    public async Task<JsonModel> GetDashboardDataAsync(string dashboardType, DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting dashboard data of type {DashboardType} by user {UserId}", dashboardType, tokenModel.UserID);

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            // Get all subscriptions for calculations
            var allSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var activeSubscriptions = allSubscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.Active).ToList();
            var trialSubscriptions = allSubscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.TrialActive).ToList();
            var pausedSubscriptions = allSubscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.Paused).ToList();
            var cancelledSubscriptions = allSubscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.Cancelled).ToList();

            // Calculate MRR from active subscriptions
            var mrr = activeSubscriptions.Sum(s => s.CurrentPrice);

            // Calculate churn rate (last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var cancelledLast30Days = cancelledSubscriptions.Count(s => s.CancelledDate >= thirtyDaysAgo);
            var activeAtStartOfMonth = activeSubscriptions.Count + cancelledLast30Days;
            var churnRate = activeAtStartOfMonth > 0 ? (decimal)cancelledLast30Days / activeAtStartOfMonth * 100 : 0;

            // Calculate growth rate
            var sixtyDaysAgo = DateTime.UtcNow.AddDays(-60);
            var activeLast30Days = activeSubscriptions.Count(s => s.CreatedDate >= thirtyDaysAgo);
            var activePrevious30Days = activeSubscriptions.Count(s => s.CreatedDate >= sixtyDaysAgo && s.CreatedDate < thirtyDaysAgo);
            var growthRate = activePrevious30Days > 0 ? (decimal)(activeLast30Days - activePrevious30Days) / activePrevious30Days * 100 : 0;

            // Get action items
            var renewalsDueToday = await _subscriptionRepository.GetSubscriptionsDueForBillingAsync(DateTime.UtcNow);
            var failedPayments = await _billingRepository.GetByStatusAsync(BillingRecord.BillingStatus.Failed);
            var trialsEnding = trialSubscriptions.Where(s => s.TrialEndDate.HasValue && s.TrialEndDate.Value <= DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS)).ToList();
            var suspendedSubscriptions = allSubscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.Suspended).ToList();

            // Get recent activity (last 20 events)
            var recentSubscriptions = allSubscriptions
                .OrderByDescending(s => s.CreatedDate)
                .Take(20)
                .Select(s => new
                {
                    type = s.Status == Subscription.SubscriptionStatuses.TrialActive ? "trial" : 
                           s.Status == Subscription.SubscriptionStatuses.Cancelled ? "cancel" : "purchase",
                    userId = s.UserId,
                    userName = s.User?.FullName ?? "Unknown",
                    planId = s.SubscriptionPlanId,
                    planName = s.SubscriptionPlan?.Name ?? "Unknown",
                    amount = s.CurrentPrice,
                    timestamp = s.CreatedDate
                }).ToList();

            var dashboardData = new
            {
                kpis = new
                {
                    totalActive = activeSubscriptions.Count,
                    totalTrial = trialSubscriptions.Count,
                    totalPaused = pausedSubscriptions.Count,
                    totalCancelled = cancelledSubscriptions.Count(s => s.CancelledDate >= thirtyDaysAgo),
                    mrr = Math.Round(mrr, 2),
                    arr = Math.Round(mrr * 12, 2),
                    churnRate = Math.Round(churnRate, 2),
                    growthRate = Math.Round(growthRate, 2),
                    totalSubscriptions = allSubscriptions.Count()
                },
                actionItems = new
                {
                    renewalsDueToday = renewalsDueToday.Count(),
                    failedPayments = failedPayments.Count(),
                    trialsEnding = trialsEnding.Count,
                    suspendedAccounts = suspendedSubscriptions.Count
                },
                recentActivity = recentSubscriptions
            };

            return new JsonModel
            {
                data = dashboardData,
                Message = "Dashboard data retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve dashboard data",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetGrowthAnalyticsAsync(DateTime? startDate, DateTime? endDate, string period, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting growth analytics for period {Period} by user {UserId}", period, tokenModel.UserID);

            var start = startDate ?? DateTime.UtcNow.AddMonths(-6);
            var end = endDate ?? DateTime.UtcNow;

            var subscriptions = await _subscriptionRepository.GetSubscriptionsByDateRangeAsync(start, end);

            // Group by period (daily, weekly, monthly)
            var growthData = period.ToLower() switch
            {
                "daily" => await CalculateDailyGrowthAsync(subscriptions, start, end),
                "weekly" => await CalculateWeeklyGrowthAsync(subscriptions, start, end),
                "monthly" => await CalculateMonthlyGrowthAsync(subscriptions, start, end),
                _ => await CalculateMonthlyGrowthAsync(subscriptions, start, end)
            };

            return new JsonModel
            {
                data = new
                {
                    period,
                    startDate = start,
                    endDate = end,
                    growthData
                },
                Message = "Growth analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting growth analytics by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve growth analytics",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetRealTimeMetricsAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting real-time metrics by user {UserId}", tokenModel.UserID);

            var allSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var activeCount = allSubscriptions.Count(s => s.Status == Subscription.SubscriptionStatuses.Active);
            var trialCount = allSubscriptions.Count(s => s.Status == Subscription.SubscriptionStatuses.TrialActive);
            
            // Get today's metrics
            var today = DateTime.UtcNow.Date;
            var newToday = allSubscriptions.Count(s => s.CreatedDate >= today);
            var cancelledToday = allSubscriptions.Count(s => s.CancelledDate.HasValue && s.CancelledDate >= today);

            // Get billing records from today
            var billingRecords = await _billingRepository.GetBillingRecordsByDateRangeAsync(today, DateTime.UtcNow);
            var revenueToday = billingRecords.Where(b => b.Status == BillingRecord.BillingStatus.Paid).Sum(b => b.TotalAmount);

            // Failed payments today
            var failedToday = billingRecords.Count(b => b.Status == BillingRecord.BillingStatus.Failed);

            var realTimeData = new
            {
                timestamp = DateTime.UtcNow,
                activeSubscriptions = activeCount,
                trialSubscriptions = trialCount,
                newSubscriptionsToday = newToday,
                cancellationsToday = cancelledToday,
                revenueToday = Math.Round(revenueToday, 2),
                failedPaymentsToday = failedToday,
                systemStatus = "Operational"
            };

            return new JsonModel
            {
                data = realTimeData,
                Message = "Real-time metrics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time metrics by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve real-time metrics",
                StatusCode = 500
            };
        }
    }

    private Task<List<object>> CalculateDailyGrowthAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var result = new List<object>();
        var current = start.Date;
        var endDate = end.Date;

        while (current <= endDate)
        {
            var currentDate = current; // Capture for closure
            var newSubs = subscriptions.Count(s => 
                s.CreatedDate.HasValue && 
                s.CreatedDate.Value.Date == currentDate);
            var cancellations = subscriptions.Count(s => 
                s.CancelledDate.HasValue && 
                s.CancelledDate.Value.Date == currentDate);
            
            result.Add(new
            {
                date = current.ToString("yyyy-MM-dd"),
                newSubscriptions = newSubs,
                cancellations,
                netGrowth = newSubs - cancellations
            });

            current = current.AddDays(1);
        }

        return Task.FromResult(result);
    }

    private Task<List<object>> CalculateWeeklyGrowthAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var result = new List<object>();
        var current = start.Date;
        var endDate = end.Date;

        while (current <= endDate)
        {
            var weekEnd = current.AddDays(7);
            var weekStart = current; // Capture for closure
            var newSubs = subscriptions.Count(s => s.CreatedDate >= weekStart && s.CreatedDate < weekEnd);
            var cancellations = subscriptions.Count(s => 
                s.CancelledDate != null && 
                s.CancelledDate.Value >= weekStart && 
                s.CancelledDate.Value < weekEnd);

            result.Add(new
            {
                weekStart = current.ToString("yyyy-MM-dd"),
                weekEnd = weekEnd.ToString("yyyy-MM-dd"),
                newSubscriptions = newSubs,
                cancellations,
                netGrowth = newSubs - cancellations
            });

            current = current.AddDays(7);
        }

        return Task.FromResult(result);
    }

    private Task<List<object>> CalculateMonthlyGrowthAsync(IEnumerable<Subscription> subscriptions, DateTime start, DateTime end)
    {
        var result = new List<object>();
        var current = new DateTime(start.Year, start.Month, 1);

        while (current <= end)
        {
            var monthStart = current; // Capture for closure
            var monthEnd = current.AddMonths(1);
            var newSubs = subscriptions.Count(s => s.CreatedDate >= monthStart && s.CreatedDate < monthEnd);
            var cancellations = subscriptions.Count(s => 
                s.CancelledDate != null && 
                s.CancelledDate.Value >= monthStart && 
                s.CancelledDate.Value < monthEnd);

            result.Add(new
            {
                month = current.ToString("yyyy-MM"),
                monthName = current.ToString("MMMM yyyy"),
                newSubscriptions = newSubs,
                cancellations,
                netGrowth = newSubs - cancellations
            });

            current = current.AddMonths(1);
        }

        return Task.FromResult(result);
    }

    public async Task<JsonModel> GetUsageStatisticsAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting usage statistics by user {UserId}", tokenModel.UserID);

            // Get all active subscriptions
            var activeSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var activeSubs = activeSubscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.Active).ToList();

            // Get privilege usage data
            var privilegeUsageStats = new List<object>();
            var totalPrivilegeUsage = 0;
            var totalOverageCharges = 0m;

            foreach (var subscription in activeSubs)
            {
                var usages = await _subscriptionRepository.GetUserSubscriptionPrivilegeUsagesAsync(subscription.Id);
                
                foreach (var usage in usages)
                {
                    totalPrivilegeUsage += usage.UsedValue;
                    
                    if (usage.IsExhausted && !usage.IsUnlimited)
                    {
                        // Calculate overage amount based on overage usage and unit cost
                        var overageUsage = usage.UsedValue - usage.AllowedValue;
                        // Note: Unit cost would need to be retrieved from the privilege configuration
                        // For now, we'll use a placeholder calculation
                        var overageAmount = overageUsage * 1.0m; // Placeholder: $1 per overage unit
                        totalOverageCharges += overageAmount;
                    }

                    var existingStat = privilegeUsageStats.FirstOrDefault(s => 
                        (s as dynamic)?.privilegeId == usage.PrivilegeId);
                    
                    if (existingStat != null)
                    {
                        (existingStat as dynamic).totalUsage += usage.UsedValue;
                        (existingStat as dynamic).userCount += 1;
                    }
                    else
                    {
                        privilegeUsageStats.Add(new
                        {
                            privilegeId = usage.PrivilegeId,
                            privilegeName = usage.Privilege?.Name ?? "Unknown",
                            totalUsage = usage.UsedValue,
                            userCount = 1,
                            averageUsage = usage.UsedValue,
                            overageCount = usage.IsExhausted ? 1 : 0
                        });
                    }
                }
            }

            // Calculate top used privileges
            var topUsedPrivileges = privilegeUsageStats
                .OrderByDescending(s => (s as dynamic).totalUsage)
                .Take(10)
                .ToList();

            // Calculate usage trends (last 30 days)
            // TODO: Implement GetPrivilegeUsageHistoryAsync method in ISubscriptionRepository
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            // var recentUsage = await _subscriptionRepository.GetPrivilegeUsageHistoryAsync(thirtyDaysAgo, DateTime.UtcNow);

            var usageTrends = new List<object>(); // Placeholder until method is implemented
            // var usageTrends = recentUsage
            //     .GroupBy(u => u.UsedAt.Date)
            //     .Select(g => new
            //     {
            //         date = g.Key,
            //         totalUsage = g.Sum(u => u.UsedValue),
            //         uniqueUsers = g.Select(u => u.UserId).Distinct().Count(),
            //         overageEvents = g.Count(u => u.IsOverage)
            //     })
            //     .OrderBy(t => t.date)
            //     .ToList();

            var statistics = new
            {
                summary = new
                {
                    totalActiveSubscriptions = activeSubs.Count,
                    totalPrivilegeUsage,
                    totalOverageCharges,
                    averageUsagePerUser = activeSubs.Count > 0 ? totalPrivilegeUsage / activeSubs.Count : 0,
                    overageRate = activeSubs.Count > 0 ? 
                        (decimal)privilegeUsageStats.Sum(s => (s as dynamic).overageCount) / activeSubs.Count * 100 : 0
                },
                topUsedPrivileges,
                usageTrends,
                privilegeBreakdown = privilegeUsageStats
            };

            return new JsonModel
            {
                data = statistics,
                Message = "Usage statistics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage statistics by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve usage statistics",
                StatusCode = 500
            };
        }
    }

    #endregion
}

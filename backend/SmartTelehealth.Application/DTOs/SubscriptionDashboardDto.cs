namespace SmartTelehealth.Application.DTOs;

public class SubscriptionDashboardDto
{
    public DateRangeDto Period { get; set; } = new();
    public OverviewMetricsDto Overview { get; set; } = new();
    public RevenueAnalyticsDto Revenue { get; set; } = new();
    public ChurnAnalyticsDto Churn { get; set; } = new();
    public PlanAnalyticsDto Plans { get; set; } = new();
    public UsageAnalyticsDto Usage { get; set; } = new();
    public TrendAnalyticsDto Trends { get; set; } = new();
    
    // Added missing properties to fix build errors
    public SubscriptionAnalyticsDto SubscriptionAnalytics { get; set; } = new();
    public IEnumerable<CategoryAnalyticsDto> TopCategories { get; set; } = new List<CategoryAnalyticsDto>();
    public IEnumerable<RevenueTrendDto> RevenueTrends { get; set; } = new List<RevenueTrendDto>();
    public IEnumerable<CategoryRevenueDto> CategoryRevenue { get; set; } = new List<CategoryRevenueDto>();
}



public class DateRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class OverviewMetricsDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public int PausedSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public int NewSubscriptionsThisPeriod { get; set; }
    public int CancelledSubscriptionsThisPeriod { get; set; }
    public decimal AverageSubscriptionValue { get; set; }
    public decimal TotalRevenue { get; set; }
}

// RevenueAnalyticsDto and MonthlyRevenueDto already exist in AnalyticsDtos.cs

public class ChurnAnalyticsDto
{
    public decimal ChurnRate { get; set; }
    public decimal RetentionRate { get; set; }
    public int CancelledSubscriptions { get; set; }
    public List<CancellationReasonDto> CancellationReasons { get; set; } = new();
    public decimal AverageLifetime { get; set; }
    public List<CohortRetentionDto> CohortRetention { get; set; } = new();
}

public class CancellationReasonDto
{
    public string Reason { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class CohortRetentionDto
{
    public string Cohort { get; set; } = string.Empty;
    public int InitialSubscriptions { get; set; }
    public int RetainedSubscriptions { get; set; }
    public decimal RetentionRate { get; set; }
}

public class PlanAnalyticsDto
{
    public List<PlanPerformanceDto> PlanPerformance { get; set; } = new();
    public List<PlanPerformanceDto> TopPerformingPlans { get; set; } = new();
    public List<PlanPerformanceDto> PlanComparison { get; set; } = new();
}

public class PlanPerformanceDto
{
    public string PlanName { get; set; } = string.Empty;
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public int NewSubscriptionsThisPeriod { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRevenue { get; set; }
    public decimal ChurnRate { get; set; }
    public decimal AverageSubscriptionValue { get; set; }
    public decimal ConversionRate { get; set; }
}

public class UsageAnalyticsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public decimal AverageUsage { get; set; }
    public List<FeatureUsageDto> FeatureUsage { get; set; } = new();
    public List<UserActivityDto> UserActivity { get; set; } = new();
    public decimal AverageUsagePerUser { get; set; }
    public List<UsageDistributionDto> UsageDistribution { get; set; } = new();
    public List<PeakUsageTimeDto> PeakUsageTimes { get; set; } = new();
    public int UnderutilizedSubscriptions { get; set; }
    public int OverutilizedSubscriptions { get; set; }
    public List<UsageTrendDto> UsageTrends { get; set; } = new();
}

public class FeatureUsageDto
{
    public string FeatureName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal UsagePercentage { get; set; }
}

public class UserActivityDto
{
    public string UserType { get; set; } = string.Empty;
    public int ActiveUsers { get; set; }
    public decimal ActivityRate { get; set; }
}

public class TrendAnalyticsDto
{
    public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
    public List<YearlyTrendDto> YearlyTrends { get; set; } = new();
    public List<SeasonalTrendDto> SeasonalTrends { get; set; } = new();
    public List<MonthlyTrendDto> RevenueTrend { get; set; } = new();
    public List<MonthlyTrendDto> SubscriptionGrowth { get; set; } = new();
    public List<MonthlyTrendDto> ChurnTrend { get; set; } = new();
    public ForecastDto Forecast { get; set; } = new();
}

public class MonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int NewSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public decimal Revenue { get; set; }
    public decimal GrowthRate { get; set; }
    public int PaymentCount { get; set; }
    public decimal AveragePayment { get; set; }
}

public class YearlyTrendDto
{
    public int Year { get; set; }
    public int TotalSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal GrowthRate { get; set; }
}

public class SeasonalTrendDto
{
    public string Season { get; set; } = string.Empty;
    public int Subscriptions { get; set; }
    public decimal Revenue { get; set; }
    public decimal SeasonalFactor { get; set; }
}

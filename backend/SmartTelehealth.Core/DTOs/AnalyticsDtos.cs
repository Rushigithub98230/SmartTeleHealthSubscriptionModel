namespace SmartTelehealth.Core.DTOs;

/// <summary>
/// DTOs for analytics data used in repository interfaces
/// </summary>

public class MonthlyRevenueData
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int SubscriptionCount { get; set; }
    public decimal AverageRevenuePerSubscription { get; set; }
}

public class CategoryRevenueData
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int SubscriptionCount { get; set; }
    public decimal Percentage { get; set; }
}

public class PaymentMethodAnalytics
{
    public string PaymentMethod { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageAmount { get; set; }
}

public class BillingStatusAnalytics
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}

public class RevenueTrendData
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal GrowthRate { get; set; }
    public int SubscriptionCount { get; set; }
}

public class OverageChargesAnalytics
{
    public decimal TotalOverageCharges { get; set; }
    public int OverageCount { get; set; }
    public decimal AverageOverageAmount { get; set; }
    public List<OverageByPlanDto> OverageByPlan { get; set; } = new();
    public List<OverageTrendDto> OverageTrend { get; set; } = new();
}

public class OverageByPlanDto
{
    public string PlanName { get; set; } = string.Empty;
    public decimal OverageAmount { get; set; }
    public int OverageCount { get; set; }
}

public class OverageTrendDto
{
    public string Period { get; set; } = string.Empty;
    public decimal OverageAmount { get; set; }
    public int OverageCount { get; set; }
}

public class BillingEfficiencyMetrics
{
    public decimal OverallEfficiency { get; set; }
    public decimal PaymentSuccessRate { get; set; }
    public decimal AverageBillingCycleTime { get; set; }
    public decimal RevenueRecoveryRate { get; set; }
    public List<BillingEfficiencyByMethodDto> EfficiencyByMethod { get; set; } = new();
}

public class BillingEfficiencyByMethodDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Efficiency { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageProcessingTime { get; set; }
}

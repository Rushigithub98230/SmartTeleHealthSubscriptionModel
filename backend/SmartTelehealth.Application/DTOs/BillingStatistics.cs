namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Billing statistics data model
/// </summary>
public class BillingStatistics
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenuePerSubscription { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
}

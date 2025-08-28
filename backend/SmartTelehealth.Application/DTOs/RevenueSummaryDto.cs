namespace SmartTelehealth.Application.DTOs;

public class RevenueSummaryDto
{
    public decimal TotalAccruedRevenue { get; set; }
    public decimal TotalCashRevenue { get; set; }
    public decimal TotalRefunded { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalSubscriptions { get; set; }
    public DateTime AsOf { get; set; }
    // Optionally add breakdowns by plan, type, etc. in future
} 
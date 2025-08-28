namespace SmartTelehealth.Application.DTOs;

public class RevenueExportDto
{
    public string BillingId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? PlanName { get; set; }
    public decimal Amount { get; set; }
    public decimal? AccruedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime BillingDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? AccrualStartDate { get; set; }
    public DateTime? AccrualEndDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? FailureReason { get; set; }
} 
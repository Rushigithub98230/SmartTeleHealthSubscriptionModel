using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.DTOs;

public class CreateBillingAdjustmentDto
{
    public Guid BillingRecordId { get; set; }
    public BillingAdjustment.AdjustmentType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool IsPercentage { get; set; } = false;
    public decimal? Percentage { get; set; }
    public bool IsApproved { get; set; } = true;
    public string? ApprovalNotes { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
} 
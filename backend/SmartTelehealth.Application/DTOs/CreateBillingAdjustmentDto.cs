namespace SmartTelehealth.Application.DTOs;

public class CreateBillingAdjustmentDto
{
    public Guid BillingRecordId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
} 
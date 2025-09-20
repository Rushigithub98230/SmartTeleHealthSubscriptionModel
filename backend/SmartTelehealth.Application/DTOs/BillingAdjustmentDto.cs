using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.DTOs;

public class BillingAdjustmentDto
{
    public Guid Id { get; set; }
    public Guid BillingRecordId { get; set; }
    public BillingAdjustment.AdjustmentType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool IsPercentage { get; set; } = false;
    public decimal? Percentage { get; set; }
    public DateTime AppliedAt { get; set; }
    public int? AppliedBy { get; set; }
    public bool IsApproved { get; set; } = true;
    public string? ApprovalNotes { get; set; }
    
    // Computed properties
    public bool IsCredit => Type == BillingAdjustment.AdjustmentType.Credit;
    public bool IsDiscount => Type == BillingAdjustment.AdjustmentType.Discount;
    public bool IsRefund => Type == BillingAdjustment.AdjustmentType.Refund;
    public bool IsLateFee => Type == BillingAdjustment.AdjustmentType.LateFee;
    public bool IsServiceFee => Type == BillingAdjustment.AdjustmentType.ServiceFee;
    public bool IsTaxAdjustment => Type == BillingAdjustment.AdjustmentType.TaxAdjustment;
}

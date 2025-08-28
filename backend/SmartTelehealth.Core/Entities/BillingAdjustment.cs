using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class BillingAdjustment : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum AdjustmentType
    {
        Discount,
        Credit,
        Refund,
        LateFee,
        ServiceFee,
        TaxAdjustment
    }
    
    // Foreign key
    public Guid BillingRecordId { get; set; }
    public virtual BillingRecord BillingRecord { get; set; } = null!;
    
    // Adjustment details
    public AdjustmentType Type { get; set; }
    
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    public bool IsPercentage { get; set; } = false;
    
    public decimal? Percentage { get; set; }
    
    public DateTime AppliedAt { get; set; }
    
    public int? AppliedBy { get; set; }
    public virtual User? AppliedByUser { get; set; }
    
    public bool IsApproved { get; set; } = true;
    
    [MaxLength(500)]
    public string? ApprovalNotes { get; set; }
    
    // Computed Properties
    [NotMapped]
    public bool IsCredit => Type == AdjustmentType.Credit;
    
    [NotMapped]
    public bool IsDiscount => Type == AdjustmentType.Discount;
    
    [NotMapped]
    public bool IsRefund => Type == AdjustmentType.Refund;
} 
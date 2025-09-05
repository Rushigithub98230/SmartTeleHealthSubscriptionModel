using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core billing adjustment entity that manages all billing adjustments in the system.
/// This entity handles billing adjustment creation, management, and approval for billing records.
/// It serves as the central hub for billing adjustment management, providing adjustment creation,
/// approval tracking, and billing modification capabilities.
/// </summary>
public class BillingAdjustment : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the billing adjustment.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each billing adjustment in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible types of billing adjustments.
    /// Used for adjustment type classification and management.
    /// </summary>
    public enum AdjustmentType
    {
        /// <summary>Discount adjustment for billing records.</summary>
        Discount,
        /// <summary>Credit adjustment for billing records.</summary>
        Credit,
        /// <summary>Refund adjustment for billing records.</summary>
        Refund,
        /// <summary>Late fee adjustment for billing records.</summary>
        LateFee,
        /// <summary>Service fee adjustment for billing records.</summary>
        ServiceFee,
        /// <summary>Tax adjustment for billing records.</summary>
        TaxAdjustment
    }
    
    /// <summary>
    /// Foreign key reference to the BillingRecord that this adjustment belongs to.
    /// Links this adjustment to the specific billing record.
    /// Required for billing record-adjustment relationship management.
    /// </summary>
    public Guid BillingRecordId { get; set; }
    
    /// <summary>
    /// Navigation property to the BillingRecord that this adjustment belongs to.
    /// Provides access to billing record information for adjustment management.
    /// Used for billing record-adjustment relationship operations.
    /// </summary>
    public virtual BillingRecord BillingRecord { get; set; } = null!;
    
    /// <summary>
    /// Type of billing adjustment applied.
    /// Used for adjustment type classification and management.
    /// Required for adjustment type enforcement and management.
    /// </summary>
    public AdjustmentType Type { get; set; }
    
    /// <summary>
    /// Amount of the billing adjustment.
    /// Used for adjustment amount management and billing.
    /// Set based on adjustment type and billing requirements.
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Description of the billing adjustment.
    /// Used for adjustment documentation and user communication.
    /// Required for adjustment management and user experience.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Reason for the billing adjustment.
    /// Used for adjustment documentation and audit trails.
    /// Optional - used for enhanced adjustment management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    /// <summary>
    /// Indicates whether this adjustment is percentage-based.
    /// Used for adjustment calculation and management.
    /// Defaults to false for standard amount-based adjustments.
    /// </summary>
    public bool IsPercentage { get; set; } = false;
    
    /// <summary>
    /// Percentage value for percentage-based adjustments.
    /// Used for adjustment calculation and management.
    /// Optional - used for percentage-based adjustment calculations.
    /// </summary>
    public decimal? Percentage { get; set; }
    
    /// <summary>
    /// Date and time when the billing adjustment was applied.
    /// Used for adjustment timing tracking and management.
    /// Set when the adjustment is applied to the billing record.
    /// </summary>
    public DateTime AppliedAt { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who applied this adjustment.
    /// Links this adjustment to the specific user who applied it.
    /// Optional - used for user tracking and audit capabilities.
    /// </summary>
    public int? AppliedBy { get; set; }
    
    /// <summary>
    /// Navigation property to the User who applied this adjustment.
    /// Provides access to user information for adjustment management.
    /// Used for user-adjustment relationship operations.
    /// </summary>
    public virtual User? AppliedByUser { get; set; }
    
    /// <summary>
    /// Indicates whether this billing adjustment has been approved.
    /// Used for adjustment approval tracking and management.
    /// Defaults to true for standard adjustment approval.
    /// </summary>
    public bool IsApproved { get; set; } = true;
    
    /// <summary>
    /// Notes about the approval of this billing adjustment.
    /// Used for adjustment approval documentation and management.
    /// Optional - used for enhanced adjustment approval management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? ApprovalNotes { get; set; }
    
    // Computed Properties
    /// <summary>
    /// Indicates whether this adjustment is a credit adjustment.
    /// Used for adjustment type checking and validation.
    /// Returns true if Type is Credit.
    /// </summary>
    [NotMapped]
    public bool IsCredit => Type == AdjustmentType.Credit;
    
    /// <summary>
    /// Indicates whether this adjustment is a discount adjustment.
    /// Used for adjustment type checking and validation.
    /// Returns true if Type is Discount.
    /// </summary>
    [NotMapped]
    public bool IsDiscount => Type == AdjustmentType.Discount;
    
    /// <summary>
    /// Indicates whether this adjustment is a refund adjustment.
    /// Used for adjustment type checking and validation.
    /// Returns true if Type is Refund.
    /// </summary>
    [NotMapped]
    public bool IsRefund => Type == AdjustmentType.Refund;
} 
namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for billing adjustment
/// </summary>
public class BillingAdjustmentDto
{
    /// <summary>
    /// Unique identifier for the billing adjustment
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Subscription ID for the adjustment
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Billing record ID (if applied)
    /// </summary>
    public Guid? BillingRecordId { get; set; }

    /// <summary>
    /// Type of adjustment
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Adjustment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Description of the adjustment
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Reason for the adjustment
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Date when the adjustment was applied
    /// </summary>
    public DateTime? AppliedDate { get; set; }

    /// <summary>
    /// Whether the adjustment is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether the adjustment is deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// User who created the adjustment
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Date when the adjustment was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// User who last updated the adjustment
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// Date when the adjustment was last updated
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// User who deleted the adjustment
    /// </summary>
    public int? DeletedBy { get; set; }

    /// <summary>
    /// Date when the adjustment was deleted
    /// </summary>
    public DateTime? DeletedDate { get; set; }

    /// <summary>
    /// Whether this is a percentage-based adjustment
    /// </summary>
    public bool IsPercentage { get; set; } = false;

    /// <summary>
    /// Percentage value for percentage-based adjustments
    /// </summary>
    public decimal? Percentage { get; set; }

    /// <summary>
    /// Date when the adjustment was applied
    /// </summary>
    public DateTime? AppliedAt { get; set; }

    /// <summary>
    /// User who applied the adjustment
    /// </summary>
    public int? AppliedBy { get; set; }

    /// <summary>
    /// Whether this adjustment is approved
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Approval notes
    /// </summary>
    public string? ApprovalNotes { get; set; }
}
using System.ComponentModel.DataAnnotations;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for creating a billing adjustment
/// </summary>
public class CreateBillingAdjustmentDto
{
    /// <summary>
    /// Billing record ID for the adjustment
    /// </summary>
    [Required]
    public Guid BillingRecordId { get; set; }

    /// <summary>
    /// Type of adjustment (Discount, Credit, Refund, LateFee, ServiceFee, TaxAdjustment, ManualPayment)
    /// </summary>
    [Required]
    public BillingAdjustment.AdjustmentType Type { get; set; }

    /// <summary>
    /// Adjustment amount (positive for credits, negative for charges)
    /// </summary>
    [Required]
    [Range(-999999.99, 999999.99, ErrorMessage = "Amount must be between -999999.99 and 999999.99")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Description of the adjustment
    /// </summary>
    [Required]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Reason for the adjustment
    /// </summary>
    [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
    public string? Reason { get; set; }

    /// <summary>
    /// Whether this is a percentage-based adjustment
    /// </summary>
    public bool IsPercentage { get; set; } = false;

    /// <summary>
    /// Percentage value for percentage-based adjustments
    /// </summary>
    [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
    public decimal? Percentage { get; set; }

    /// <summary>
    /// Whether this adjustment is approved
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Approval notes
    /// </summary>
    [StringLength(500, ErrorMessage = "Approval notes cannot exceed 500 characters")]
    public string? ApprovalNotes { get; set; }
}
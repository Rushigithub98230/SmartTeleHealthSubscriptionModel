using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for updating a billing adjustment
/// </summary>
public class UpdateBillingAdjustmentDto
{
    /// <summary>
    /// Adjustment amount (positive for credits, negative for charges)
    /// </summary>
    [Range(-999999.99, 999999.99, ErrorMessage = "Amount must be between -999999.99 and 999999.99")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Description of the adjustment
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Reason for the adjustment
    /// </summary>
    [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
    public string? Reason { get; set; }
}

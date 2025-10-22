namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Request DTO for manually marking a billing record as paid (admin override)
/// Phase 2: Billing Management
/// </summary>
public class MarkAsPaidRequestDto
{
    /// <summary>
    /// Reference transaction ID or payment confirmation number
    /// </summary>
    public string? TransactionReference { get; set; }

    /// <summary>
    /// Reason for manual payment marking (e.g., "Cash payment received", "Check cleared")
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Payment method used (Cash, Check, Bank Transfer, etc.)
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Date the payment was actually received
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    public string? Notes { get; set; }
}


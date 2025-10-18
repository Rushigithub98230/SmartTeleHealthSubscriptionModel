using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for purchasing additional privilege credits with upfront payment.
/// This implements the workflow requirement:
/// "Once a user has used all their included privileges, any additional usage 
/// would require upfront payment. Only after this payment would the extra 
/// privilege be added to their account."
/// </summary>
public class PurchaseAdditionalCreditsDto
{
    /// <summary>
    /// Name of the privilege to purchase additional credits for
    /// (e.g., "Teleconsultation", "Messaging", "Medication Delivery")
    /// </summary>
    [Required(ErrorMessage = "Privilege name is required")]
    [MaxLength(100)]
    public string PrivilegeName { get; set; } = string.Empty;

    /// <summary>
    /// Number of additional credits to purchase
    /// Must be between 1 and 100
    /// </summary>
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }

    /// <summary>
    /// Stripe payment method ID to use for upfront payment
    /// Format: "pm_xxxxxxxxxxxxx"
    /// </summary>
    [Required(ErrorMessage = "Payment method is required")]
    [MaxLength(100)]
    public string PaymentMethodId { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for successful credit purchase
/// </summary>
public class PurchaseCreditsResponseDto
{
    public Guid SubscriptionId { get; set; }
    public string PrivilegeName { get; set; } = string.Empty;
    public int CreditsAdded { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalPaid { get; set; }
    public int PreviousLimit { get; set; }
    public int NewLimit { get; set; }
    public int CurrentUsed { get; set; }
    public int NewRemaining { get; set; }
    public Guid BillingRecordId { get; set; }
    public DateTime PurchasedAt { get; set; }
}



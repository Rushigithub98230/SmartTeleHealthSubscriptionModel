using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core payment refund entity that manages all payment refunds in the system.
/// This entity handles refund processing, tracking, and management for subscription payments.
/// It serves as the central hub for refund management, integrating with subscription payments,
/// users, and Stripe. The entity includes comprehensive refund tracking, processing management,
/// and audit trail capabilities.
/// </summary>
public class PaymentRefund : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the payment refund.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each refund in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the SubscriptionPayment that this refund is for.
    /// Links this refund to the specific subscription payment.
    /// Required for payment-refund relationship management.
    /// </summary>
    [Required]
    public Guid SubscriptionPaymentId { get; set; }
    
    /// <summary>
    /// Navigation property to the SubscriptionPayment that this refund is for.
    /// Provides access to payment information for refund management.
    /// Used for payment-refund relationship operations.
    /// </summary>
    public virtual SubscriptionPayment SubscriptionPayment { get; set; } = null!;
    
    /// <summary>
    /// Amount of this refund in the specified currency.
    /// Used for refund amount tracking and billing management.
    /// Set when refund is processed and amount is determined.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Reason for this refund.
    /// Used for refund documentation and customer communication.
    /// Required for refund processing and audit trails.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Stripe refund ID for this refund.
    /// Links this refund to the corresponding Stripe refund.
    /// Used for Stripe integration and refund processing.
    /// </summary>
    [MaxLength(100)]
    public string? StripeRefundId { get; set; }
    
    /// <summary>
    /// Date and time when this refund was processed.
    /// Used for refund timing tracking and management.
    /// Set when refund is successfully processed.
    /// </summary>
    public DateTime RefundedAt { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who processed this refund.
    /// Links this refund to the specific user who processed it.
    /// Optional - used for refund processing tracking and audit trails.
    /// </summary>
    public int? ProcessedByUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who processed this refund.
    /// Provides access to processor information for refund management.
    /// Used for refund processing tracking and audit trails.
    /// </summary>
    public virtual User? ProcessedByUser { get; set; }
} 
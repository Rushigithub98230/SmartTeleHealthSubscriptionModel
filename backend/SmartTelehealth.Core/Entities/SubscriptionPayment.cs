using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core subscription payment entity that manages all subscription-related payments in the system.
/// This entity handles subscription payment tracking including payment processing, status management, and Stripe integration.
/// It serves as the central hub for subscription payment operations including recurring payments, trial payments,
/// upgrades, downgrades, and refunds. The entity includes comprehensive payment status tracking, billing period management,
/// and integration with Stripe for payment processing and invoice management.
/// </summary>
#region Improved SubscriptionPayment Entity
public class SubscriptionPayment : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the subscription payment.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each subscription payment record.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration of all possible payment statuses for subscription payments.
    /// Defines the current state of a payment in the processing workflow.
    /// Used for payment status tracking and payment management.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>Payment is created but not yet processed</summary>
        Pending,
        /// <summary>Payment is currently being processed</summary>
        Processing,
        /// <summary>Payment has been successfully processed and confirmed</summary>
        Succeeded,
        /// <summary>Payment processing failed and needs to be retried</summary>
        Failed,
        /// <summary>Payment has been cancelled and will not be processed</summary>
        Cancelled,
        /// <summary>Payment has been fully refunded</summary>
        Refunded,
        /// <summary>Payment has been partially refunded</summary>
        PartiallyRefunded
    }
    
    /// <summary>
    /// Enumeration of all possible payment types for subscription payments.
    /// Defines the category and purpose of the payment.
    /// Used for payment categorization and management.
    /// </summary>
    public enum PaymentType
    {
        /// <summary>Payment for subscription services</summary>
        Subscription,
        /// <summary>Payment for trial period services</summary>
        Trial,
        /// <summary>Payment for setup fees</summary>
        Setup,
        /// <summary>Payment for subscription upgrades</summary>
        Upgrade,
        /// <summary>Payment for subscription downgrades</summary>
        Downgrade,
        /// <summary>Payment for refunds and credits</summary>
        Refund,
        /// <summary>Payment for billing adjustments</summary>
        Adjustment
    }

    /// <summary>
    /// Foreign key reference to the Subscription that this payment belongs to.
    /// Links this payment to the specific subscription.
    /// Required for subscription-based payment tracking and management.
    /// </summary>
    [Required]
    public Guid SubscriptionId { get; set; }
    
    /// <summary>
    /// Navigation property to the Subscription that this payment belongs to.
    /// Provides access to subscription information for payment management.
    /// Used for subscription-based payment operations and tracking.
    /// </summary>
    public virtual Subscription Subscription { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Currency for this payment.
    /// Determines the currency for payment processing and billing.
    /// Required for international payments and currency management.
    /// </summary>
    [Required]
    public Guid CurrencyId { get; set; }
    
    /// <summary>
    /// Navigation property to the Currency for this payment.
    /// Provides access to currency information and exchange rates.
    /// Used for international payments and currency management.
    /// </summary>
    public virtual MasterCurrency Currency { get; set; } = null!;
    
    /// <summary>
    /// Base amount for this payment before taxes and fees.
    /// Used for payment calculations and processing.
    /// This is the core amount being charged for the subscription.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Tax amount for this payment.
    /// Used for tax calculations and compliance.
    /// Added to the base amount for total payment calculation.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } = 0;
    
    /// <summary>
    /// Net amount for this payment after taxes and fees.
    /// Used for payment calculations and processing.
    /// This is the final amount that will be charged to the customer.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetAmount { get; set; }
    
    /// <summary>
    /// Description of this payment and what it covers.
    /// Used for payment information display and customer communication.
    /// Provides context about what the payment is for.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of this payment in the processing workflow.
    /// Determines the payment's state and processing status.
    /// Used for payment status tracking and management.
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    /// <summary>
    /// Type of payment indicating the category and purpose.
    /// Determines the payment's category and management approach.
    /// Used for payment categorization and management.
    /// </summary>
    public PaymentType Type { get; set; } = PaymentType.Subscription;
    
    /// <summary>
    /// Reason for payment failure if applicable.
    /// Used for payment failure analysis and customer support.
    /// Captured when payment processing fails.
    /// </summary>
    [MaxLength(1000)]
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// Date when payment is due for this subscription payment.
    /// Used for payment scheduling and overdue tracking.
    /// Set when the payment is created or scheduled.
    /// </summary>
    [Required]
    public DateTime DueDate { get; set; }
    
    /// <summary>
    /// Date and time when payment was successfully processed.
    /// Used for payment history tracking and analytics.
    /// Set when payment is successfully confirmed.
    /// </summary>
    public DateTime? PaidAt { get; set; }
    
    /// <summary>
    /// Date and time when payment processing failed.
    /// Used for payment failure tracking and analytics.
    /// Set when payment processing fails.
    /// </summary>
    public DateTime? FailedAt { get; set; }
    
    // Billing period this payment covers
    /// <summary>
    /// Start date of the billing period that this payment covers.
    /// Used for billing period tracking and payment management.
    /// Defines when the billing period begins for this payment.
    /// </summary>
    public DateTime BillingPeriodStart { get; set; }
    
    /// <summary>
    /// End date of the billing period that this payment covers.
    /// Used for billing period tracking and payment management.
    /// Defines when the billing period ends for this payment.
    /// </summary>
    public DateTime BillingPeriodEnd { get; set; }
    
    // Stripe Integration
    /// <summary>
    /// Stripe payment intent ID for this payment.
    /// Links this payment to the corresponding Stripe payment intent.
    /// Used for Stripe integration and payment processing.
    /// </summary>
    [MaxLength(100)]
    public string? StripePaymentIntentId { get; set; }
    
    /// <summary>
    /// Stripe invoice ID for this payment.
    /// Links this payment to the corresponding Stripe invoice.
    /// Used for Stripe integration and invoice management.
    /// </summary>
    [MaxLength(100)]
    public string? StripeInvoiceId { get; set; }
    
    /// <summary>
    /// URL to the payment receipt for this payment.
    /// Used for receipt access and customer communication.
    /// Generated when payment is successfully processed.
    /// </summary>
    [MaxLength(500)]
    public string? ReceiptUrl { get; set; }
    
    // Legacy support properties
    /// <summary>
    /// Legacy payment intent ID for backward compatibility.
    /// Used for backward compatibility with existing systems.
    /// Can be removed if not needed in future versions.
    /// </summary>
    [MaxLength(100)]
    public string? PaymentIntentId { get; set; }
    
    /// <summary>
    /// Legacy invoice ID for backward compatibility.
    /// Used for backward compatibility with existing systems.
    /// Can be removed if not needed in future versions.
    /// </summary>
    [MaxLength(100)]
    public string? InvoiceId { get; set; }
    
    /// <summary>
    /// Number of payment attempts made for this payment.
    /// Used for payment retry logic and failure tracking.
    /// Incremented each time a payment attempt is made.
    /// </summary>
    public int AttemptCount { get; set; } = 0;
    
    /// <summary>
    /// Date and time when the next payment retry is scheduled.
    /// Used for payment retry logic and failure management.
    /// Set when payment fails and retry is scheduled.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
    
    /// <summary>
    /// Total amount that has been refunded for this payment.
    /// Used for refund tracking and payment management.
    /// Updated when refunds are processed for this payment.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal RefundedAmount { get; set; } = 0;
    
    // Navigation properties
    /// <summary>
    /// Collection of all refunds associated with this payment.
    /// Represents the complete refund history for this payment.
    /// Used for refund tracking and payment management.
    /// </summary>
    public virtual ICollection<PaymentRefund> Refunds { get; set; } = new List<PaymentRefund>();
    
    // Computed Properties
    /// <summary>
    /// Computed property that indicates whether this payment has been successfully paid.
    /// Returns true if status is "Succeeded", indicating successful payment.
    /// Used for payment status checking and payment management.
    /// </summary>
    [NotMapped]
    public bool IsPaid => Status == PaymentStatus.Succeeded;
    
    /// <summary>
    /// Computed property that indicates whether this payment has failed.
    /// Returns true if status is "Failed", indicating payment failure.
    /// Used for payment failure checking and payment management.
    /// </summary>
    [NotMapped]
    public bool IsFailed => Status == PaymentStatus.Failed;
    
    /// <summary>
    /// Computed property that indicates whether this payment has been refunded.
    /// Returns true if status is "Refunded" or "PartiallyRefunded", indicating refund.
    /// Used for refund status checking and payment management.
    /// </summary>
    [NotMapped]
    public bool IsRefunded => Status == PaymentStatus.Refunded || Status == PaymentStatus.PartiallyRefunded;
    
    /// <summary>
    /// Computed property that indicates whether this payment is overdue.
    /// Returns true if payment is not paid and due date has passed.
    /// Used for overdue checking and payment management.
    /// </summary>
    [NotMapped]
    public bool IsOverdue => !IsPaid && DateTime.UtcNow > DueDate;
    
    /// <summary>
    /// Computed property that returns the remaining amount after refunds.
    /// Returns the difference between total amount and refunded amount.
    /// Used for refund tracking and payment management.
    /// </summary>
    [NotMapped]
    public decimal RemainingAmount => Amount - RefundedAmount;
}
#endregion 
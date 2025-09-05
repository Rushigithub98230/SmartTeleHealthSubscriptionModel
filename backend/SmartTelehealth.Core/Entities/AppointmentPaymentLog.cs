using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Entity for tracking appointment payment transactions and logs.
/// This entity handles payment logging, transaction tracking, and refund management for appointments.
/// It serves as a comprehensive payment audit system that records all payment-related activities,
/// including successful payments, failed payments, and refunds with Stripe integration.
/// </summary>
public class AppointmentPaymentLog : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the appointment payment log.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each payment log entry in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key reference to the Appointment that this payment log is for.
    /// Links this payment log to the specific appointment.
    /// Required for appointment-payment relationship management.
    /// </summary>
    public Guid AppointmentId { get; set; }
    
    /// <summary>
    /// Navigation property to the Appointment that this payment log is for.
    /// Provides access to appointment information for payment management.
    /// Used for appointment-payment relationship operations.
    /// </summary>
    public virtual Appointment Appointment { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the User who made this payment.
    /// Links this payment log to the specific user who initiated the payment.
    /// Required for user-payment relationship management.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who made this payment.
    /// Provides access to user information for payment management.
    /// Used for user-payment relationship operations.
    /// </summary>
    public virtual User User { get; set; } = null!;

    // Status Foreign Keys
    /// <summary>
    /// Foreign key reference to the PaymentStatus that defines this payment's status.
    /// Links this payment log to the specific payment status (Pending, Paid, Failed, etc.).
    /// Required for payment status tracking and management.
    /// </summary>
    public Guid PaymentStatusId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the RefundStatus that defines this payment's refund status.
    /// Links this payment log to the specific refund status (None, Pending, Refunded, etc.).
    /// Required for refund status tracking and management.
    /// </summary>
    public Guid RefundStatusId { get; set; }
    
    /// <summary>
    /// Navigation property to the PaymentStatus that defines this payment's status.
    /// Provides access to payment status information for payment management.
    /// Used for payment status tracking and management.
    /// </summary>
    public virtual PaymentStatus? PaymentStatus { get; set; }
    
    /// <summary>
    /// Navigation property to the RefundStatus that defines this payment's refund status.
    /// Provides access to refund status information for payment management.
    /// Used for refund status tracking and management.
    /// </summary>
    public virtual RefundStatus? RefundStatus { get; set; }

    // Payment details
    /// <summary>
    /// Payment method used for this transaction.
    /// Used for payment method tracking and management.
    /// Examples: Stripe, PayPal, Credit Card, etc.
    /// </summary>
    [MaxLength(100)]
    public string PaymentMethod { get; set; } = string.Empty; // Stripe, PayPal, etc.

    /// <summary>
    /// Stripe Payment Intent ID for this transaction.
    /// Links this payment log to the corresponding Stripe payment intent.
    /// Used for Stripe integration and payment processing.
    /// </summary>
    [MaxLength(255)]
    public string? PaymentIntentId { get; set; } // Stripe Payment Intent ID

    /// <summary>
    /// Stripe Session ID for this transaction.
    /// Links this payment log to the corresponding Stripe checkout session.
    /// Used for Stripe integration and payment processing.
    /// </summary>
    [MaxLength(255)]
    public string? SessionId { get; set; } // Stripe Session ID

    /// <summary>
    /// Stripe Refund ID for this transaction.
    /// Links this payment log to the corresponding Stripe refund.
    /// Used for Stripe integration and refund processing.
    /// </summary>
    [MaxLength(255)]
    public string? RefundId { get; set; } // Stripe Refund ID

    /// <summary>
    /// Amount of this payment transaction.
    /// Used for payment amount tracking and billing management.
    /// Set when payment is processed or when amount is determined.
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Amount that has been refunded for this payment.
    /// Used for refund amount tracking and billing management.
    /// Set when refund is processed for this payment.
    /// </summary>
    public decimal? RefundedAmount { get; set; }

    /// <summary>
    /// Currency code for this payment transaction.
    /// Used for currency tracking and international payment support.
    /// Defaults to USD for standard transactions.
    /// </summary>
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Description of this payment transaction.
    /// Used for payment documentation and user interface display.
    /// Can include payment purpose, appointment details, or transaction context.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Reason for payment failure if applicable.
    /// Used for payment failure tracking and troubleshooting.
    /// Set when payment fails and failure reason is available.
    /// </summary>
    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    /// <summary>
    /// Reason for refund if applicable.
    /// Used for refund tracking and documentation.
    /// Set when refund is processed and refund reason is available.
    /// </summary>
    [MaxLength(1000)]
    public string? RefundReason { get; set; }

    // Timestamps
    /// <summary>
    /// Date and time when this payment was processed.
    /// Used for payment timing tracking and management.
    /// Set when payment is successfully processed.
    /// </summary>
    public DateTime? PaymentDate { get; set; }
    
    /// <summary>
    /// Date and time when this payment was refunded.
    /// Used for refund timing tracking and management.
    /// Set when refund is successfully processed.
    /// </summary>
    public DateTime? RefundDate { get; set; }
} 
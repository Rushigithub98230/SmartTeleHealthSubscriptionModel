using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class AppointmentPaymentLog : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    public Guid AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; } = null!;

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    // Status Foreign Keys
    public Guid PaymentStatusId { get; set; }
    public Guid RefundStatusId { get; set; }
    public virtual PaymentStatus? PaymentStatus { get; set; }
    public virtual RefundStatus? RefundStatus { get; set; }

    // Payment details
    [MaxLength(100)]
    public string PaymentMethod { get; set; } = string.Empty; // Stripe, PayPal, etc.

    [MaxLength(255)]
    public string? PaymentIntentId { get; set; } // Stripe Payment Intent ID

    [MaxLength(255)]
    public string? SessionId { get; set; } // Stripe Session ID

    [MaxLength(255)]
    public string? RefundId { get; set; } // Stripe Refund ID

    public decimal Amount { get; set; }
    public decimal? RefundedAmount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    [MaxLength(1000)]
    public string? RefundReason { get; set; }

    // Timestamps
    public DateTime? PaymentDate { get; set; }
    public DateTime? RefundDate { get; set; }
} 
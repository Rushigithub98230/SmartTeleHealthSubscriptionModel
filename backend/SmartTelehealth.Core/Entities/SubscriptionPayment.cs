using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

#region Improved SubscriptionPayment Entity
public class SubscriptionPayment : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Succeeded,
        Failed,
        Cancelled,
        Refunded,
        PartiallyRefunded
    }
    
    public enum PaymentType
    {
        Subscription,
        Trial,
        Setup,
        Upgrade,
        Downgrade,
        Refund,
        Adjustment
    }

    [Required]
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;
    
    [Required]
    public Guid CurrencyId { get; set; }
    public virtual MasterCurrency Currency { get; set; } = null!;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetAmount { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentType Type { get; set; } = PaymentType.Subscription;
    
    [MaxLength(1000)]
    public string? FailureReason { get; set; }
    
    [Required]
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    
    // Period this payment covers
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
    
    // Stripe Integration
    [MaxLength(100)]
    public string? StripePaymentIntentId { get; set; }
    
    [MaxLength(100)]
    public string? StripeInvoiceId { get; set; }
    
    [MaxLength(500)]
    public string? ReceiptUrl { get; set; }
    
    // Legacy support - remove these if not needed
    [MaxLength(100)]
    public string? PaymentIntentId { get; set; }
    
    [MaxLength(100)]
    public string? InvoiceId { get; set; }
    
    public int AttemptCount { get; set; } = 0;
    public DateTime? NextRetryAt { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal RefundedAmount { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<PaymentRefund> Refunds { get; set; } = new List<PaymentRefund>();
    
    // Alias properties for backward compatibility
    public DateTime? CreatedAt { get => CreatedDate; set => CreatedDate = value; }
    public DateTime? UpdatedAt { get => UpdatedDate; set => UpdatedDate = value; }
    
    // Computed Properties
    [NotMapped]
    public bool IsPaid => Status == PaymentStatus.Succeeded;
    
    [NotMapped]
    public bool IsFailed => Status == PaymentStatus.Failed;
    
    [NotMapped]
    public bool IsRefunded => Status == PaymentStatus.Refunded || Status == PaymentStatus.PartiallyRefunded;
    
    [NotMapped]
    public bool IsOverdue => !IsPaid && DateTime.UtcNow > DueDate;
    
    [NotMapped]
    public decimal RemainingAmount => Amount - RefundedAmount;
}
#endregion 
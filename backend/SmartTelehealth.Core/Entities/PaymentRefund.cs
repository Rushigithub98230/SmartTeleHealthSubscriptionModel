using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class PaymentRefund : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SubscriptionPaymentId { get; set; }
    public virtual SubscriptionPayment SubscriptionPayment { get; set; } = null!;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? StripeRefundId { get; set; }
    
    public DateTime RefundedAt { get; set; }
    
    public int? ProcessedByUserId { get; set; }
    public virtual User? ProcessedByUser { get; set; }
} 
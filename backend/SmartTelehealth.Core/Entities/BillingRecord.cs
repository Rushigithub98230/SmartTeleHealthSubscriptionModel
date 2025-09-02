using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class BillingRecord : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum BillingStatus
    {
        Pending,
        Paid,
        Failed,
        Cancelled,
        Refunded,
        Overdue,
        Upcoming
    }
    
    public enum BillingType
    {
        Subscription,
        Consultation,
        Medication,
        LateFee,
        Refund,
        Recurring,
        Upfront,
        Bundle,
        Invoice,
        Cycle
    }
    
    // Foreign keys
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public Guid? SubscriptionId { get; set; }
    public virtual Subscription? Subscription { get; set; }
    
    public Guid? ConsultationId { get; set; }
    public virtual Consultation? Consultation { get; set; }
    
    public Guid? MedicationDeliveryId { get; set; }
    public virtual MedicationDelivery? MedicationDelivery { get; set; }
    
    public Guid? BillingCycleId { get; set; }
    
    public Guid CurrencyId { get; set; }
    public virtual MasterCurrency Currency { get; set; } = null!;
    
    // Billing details
    public BillingStatus Status { get; set; } = BillingStatus.Pending;
    
    public BillingType Type { get; set; } = BillingType.Subscription;
    
    public decimal Amount { get; set; }
    
    public decimal TaxAmount { get; set; }
    
    public decimal ShippingAmount { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public DateTime BillingDate { get; set; }
    
    public DateTime? PaidAt { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    [MaxLength(100)]
    public string? InvoiceNumber { get; set; }
    
    [MaxLength(100)]
    public string? StripePaymentIntentId { get; set; }
    
    [MaxLength(100)]
    public string? StripeInvoiceId { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(500)]
    public string? FailureReason { get; set; }
    
    [MaxLength(100)]
    public string? PaymentMethod { get; set; }
    
    [MaxLength(100)]
    public string? TransactionId { get; set; }
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public bool IsRecurring { get; set; } = false;
    
    public DateTime? NextBillingDate { get; set; }
    
    public string? PaymentIntentId { get; set; }
    
    public decimal? AccruedAmount { get; set; }
    public DateTime? AccrualStartDate { get; set; }
    public DateTime? AccrualEndDate { get; set; }
    
    // Navigation properties
    public virtual ICollection<BillingAdjustment> Adjustments { get; set; } = new List<BillingAdjustment>();
    
    // Alias properties for backward compatibility - removed as they cause infinite recursion
    
    // Computed Properties
    [NotMapped]
    public bool IsPaid => Status == BillingStatus.Paid;
    
    [NotMapped]
    public bool IsFailed => Status == BillingStatus.Failed;
    
    [NotMapped]
    public bool IsRefunded => Status == BillingStatus.Refunded;
    
    [NotMapped]
    public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value && !IsPaid;
} 
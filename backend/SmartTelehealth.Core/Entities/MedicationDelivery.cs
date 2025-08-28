using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class MedicationDelivery : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum DeliveryStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Failed,
        Returned
    }
    
    // Foreign keys
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public Guid? SubscriptionId { get; set; }
    public virtual Subscription? Subscription { get; set; }
    
    public Guid? ConsultationId { get; set; }
    public virtual Consultation? Consultation { get; set; }
    
    public int? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }
    
    // Delivery details
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    
    [Required]
    [MaxLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(50)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    
    [MaxLength(100)]
    public string? TrackingNumber { get; set; }
    
    [MaxLength(100)]
    public string? Carrier { get; set; }
    
    public DateTime? ShippedAt { get; set; }
    
    public DateTime? DeliveredAt { get; set; }
    
    public DateTime? EstimatedDeliveryDate { get; set; }
    
    [MaxLength(1000)]
    public string? Medications { get; set; } // JSON string of medications
    
    [MaxLength(1000)]
    public string? Instructions { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public decimal ShippingCost { get; set; }
    
    public bool RequiresSignature { get; set; } = false;
    
    public bool IsRefrigerated { get; set; } = false;
    
    [MaxLength(500)]
    public string? FailureReason { get; set; }
    
    // Navigation properties
    public virtual ICollection<DeliveryTracking> TrackingEvents { get; set; } = new List<DeliveryTracking>();
    
    // Computed Properties
    [NotMapped]
    public bool IsDelivered => Status == DeliveryStatus.Delivered;
    
    [NotMapped]
    public bool IsShipped => Status == DeliveryStatus.Shipped;
    
    [NotMapped]
    public bool IsFailed => Status == DeliveryStatus.Failed;
    
    [NotMapped]
    public bool IsReturned => Status == DeliveryStatus.Returned;
} 
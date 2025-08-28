using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class DeliveryTracking : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum TrackingEventType
    {
        Created,
        Processing,
        Shipped,
        InTransit,
        OutForDelivery,
        Delivered,
        Failed,
        Returned,
        Exception
    }
    
    // Foreign key
    public Guid MedicationDeliveryId { get; set; }
    public virtual MedicationDelivery MedicationDelivery { get; set; } = null!;
    
    // Tracking details
    public TrackingEventType EventType { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Location { get; set; }
    
    public DateTime EventTime { get; set; }
    
    [MaxLength(100)]
    public string? TrackingNumber { get; set; }
    
    [MaxLength(100)]
    public string? Carrier { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Computed Properties
    [NotMapped]
    public bool IsDelivered => EventType == TrackingEventType.Delivered;
    
    [NotMapped]
    public bool IsFailed => EventType == TrackingEventType.Failed;
    
    [NotMapped]
    public bool IsReturned => EventType == TrackingEventType.Returned;
} 
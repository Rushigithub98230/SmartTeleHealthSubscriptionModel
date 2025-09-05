using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Entity for tracking delivery events and status updates for medication deliveries.
/// This entity handles delivery tracking, event logging, and status monitoring.
/// It serves as the central hub for delivery tracking management, providing detailed
/// event tracking and status updates for medication delivery processes.
/// </summary>
public class DeliveryTracking : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the delivery tracking event.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each delivery tracking event in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible types of delivery tracking events.
    /// Used for delivery event type management and categorization.
    /// </summary>
    public enum TrackingEventType
    {
        /// <summary>Delivery was created and is being prepared.</summary>
        Created,
        /// <summary>Delivery is being processed and prepared for shipping.</summary>
        Processing,
        /// <summary>Delivery has been shipped and is in transit.</summary>
        Shipped,
        /// <summary>Delivery is currently in transit.</summary>
        InTransit,
        /// <summary>Delivery is out for delivery to the recipient.</summary>
        OutForDelivery,
        /// <summary>Delivery has been successfully delivered.</summary>
        Delivered,
        /// <summary>Delivery failed and could not be completed.</summary>
        Failed,
        /// <summary>Delivery was returned to sender.</summary>
        Returned,
        /// <summary>Delivery encountered an exception or special circumstance.</summary>
        Exception
    }
    
    // Foreign key
    /// <summary>
    /// Foreign key reference to the MedicationDelivery that this tracking event is for.
    /// Links this tracking event to the specific medication delivery.
    /// Required for delivery-tracking relationship management.
    /// </summary>
    public Guid MedicationDeliveryId { get; set; }
    
    /// <summary>
    /// Navigation property to the MedicationDelivery that this tracking event is for.
    /// Provides access to delivery information for tracking management.
    /// Used for delivery-tracking relationship operations.
    /// </summary>
    public virtual MedicationDelivery MedicationDelivery { get; set; } = null!;
    
    // Tracking details
    /// <summary>
    /// Type of this delivery tracking event.
    /// Used for delivery event type management and categorization.
    /// Set when the tracking event is created.
    /// </summary>
    public TrackingEventType EventType { get; set; }
    
    /// <summary>
    /// Description of this delivery tracking event.
    /// Used for event documentation and user communication.
    /// Required for tracking event creation and documentation.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Location where this delivery tracking event occurred.
    /// Used for location tracking and delivery monitoring.
    /// Set when the tracking event occurs and location is available.
    /// </summary>
    [MaxLength(100)]
    public string? Location { get; set; }
    
    /// <summary>
    /// Date and time when this delivery tracking event occurred.
    /// Used for event timing tracking and chronological management.
    /// Set when the tracking event occurs.
    /// </summary>
    public DateTime EventTime { get; set; }
    
    /// <summary>
    /// Tracking number associated with this delivery tracking event.
    /// Used for delivery tracking and shipment monitoring.
    /// Set when the tracking event is associated with a tracking number.
    /// </summary>
    [MaxLength(100)]
    public string? TrackingNumber { get; set; }
    
    /// <summary>
    /// Carrier responsible for this delivery tracking event.
    /// Used for carrier tracking and delivery management.
    /// Set when the tracking event is associated with a specific carrier.
    /// </summary>
    [MaxLength(100)]
    public string? Carrier { get; set; }
    
    /// <summary>
    /// Additional notes or information about this delivery tracking event.
    /// Used for event documentation and additional context.
    /// Set when additional information is available about the tracking event.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Computed Properties
    /// <summary>
    /// Indicates whether this delivery tracking event represents a successful delivery.
    /// Returns true if event type is Delivered.
    /// Used for delivery completion checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsDelivered => EventType == TrackingEventType.Delivered;
    
    /// <summary>
    /// Indicates whether this delivery tracking event represents a failed delivery.
    /// Returns true if event type is Failed.
    /// Used for delivery failure checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsFailed => EventType == TrackingEventType.Failed;
    
    /// <summary>
    /// Indicates whether this delivery tracking event represents a returned delivery.
    /// Returns true if event type is Returned.
    /// Used for delivery return checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsReturned => EventType == TrackingEventType.Returned;
} 
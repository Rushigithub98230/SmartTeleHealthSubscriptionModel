using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core medication delivery entity that manages all medication deliveries in the system.
/// This entity handles medication delivery tracking, shipping management, and delivery status monitoring.
/// It serves as the central hub for medication delivery management, integrating with users, subscriptions,
/// consultations, providers, and delivery tracking. The entity includes comprehensive delivery tracking,
/// shipping management, and medication fulfillment capabilities.
/// </summary>
public class MedicationDelivery : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the medication delivery.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each medication delivery in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible statuses of a medication delivery.
    /// Used for delivery status tracking and workflow management.
    /// </summary>
    public enum DeliveryStatus
    {
        /// <summary>Delivery is pending and not yet processed.</summary>
        Pending,
        /// <summary>Delivery is being processed and prepared.</summary>
        Processing,
        /// <summary>Delivery has been shipped and is in transit.</summary>
        Shipped,
        /// <summary>Delivery has been successfully delivered.</summary>
        Delivered,
        /// <summary>Delivery failed and could not be completed.</summary>
        Failed,
        /// <summary>Delivery was returned to sender.</summary>
        Returned
    }
    
    // Foreign keys
    /// <summary>
    /// Foreign key reference to the User who is receiving this medication delivery.
    /// Links this delivery to the specific user account.
    /// Required for user-specific delivery management and tracking.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is receiving this medication delivery.
    /// Provides access to user information for delivery management.
    /// Used for user-specific delivery operations and tracking.
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Subscription that covers this medication delivery.
    /// Links this delivery to the specific user subscription for billing and access control.
    /// Optional - used for subscription-based delivery billing and privilege management.
    /// </summary>
    public Guid? SubscriptionId { get; set; }
    
    /// <summary>
    /// Navigation property to the Subscription that covers this medication delivery.
    /// Provides access to subscription information for delivery billing and access control.
    /// Used for subscription-based delivery operations and billing management.
    /// </summary>
    public virtual Subscription? Subscription { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Consultation that prescribed this medication delivery.
    /// Links this delivery to the specific consultation session.
    /// Optional - used for consultation-based delivery management and medical record keeping.
    /// </summary>
    public Guid? ConsultationId { get; set; }
    
    /// <summary>
    /// Navigation property to the Consultation that prescribed this medication delivery.
    /// Provides access to consultation information for delivery management.
    /// Used for consultation-based delivery operations and medical record management.
    /// </summary>
    public virtual Consultation? Consultation { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Provider who prescribed this medication delivery.
    /// Links this delivery to the specific healthcare provider.
    /// Optional - used for provider-specific delivery management and prescription tracking.
    /// </summary>
    public int? ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who prescribed this medication delivery.
    /// Provides access to provider information for delivery management.
    /// Used for provider-specific delivery operations and prescription tracking.
    /// </summary>
    public virtual Provider? Provider { get; set; }
    
    // Delivery details
    /// <summary>
    /// Current status of this medication delivery.
    /// Used for delivery status tracking and workflow management.
    /// Defaults to Pending when delivery is first created.
    /// </summary>
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    
    /// <summary>
    /// Complete delivery address for this medication delivery.
    /// Used for delivery address management and shipping coordination.
    /// Required for delivery processing and shipping management.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// City for this medication delivery address.
    /// Used for delivery address management and shipping coordination.
    /// Set when delivery address is specified.
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }
    
    /// <summary>
    /// State for this medication delivery address.
    /// Used for delivery address management and shipping coordination.
    /// Set when delivery address is specified.
    /// </summary>
    [MaxLength(50)]
    public string? State { get; set; }
    
    /// <summary>
    /// ZIP code for this medication delivery address.
    /// Used for delivery address management and shipping coordination.
    /// Set when delivery address is specified.
    /// </summary>
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    
    /// <summary>
    /// Tracking number for this medication delivery.
    /// Used for delivery tracking and shipment monitoring.
    /// Set when delivery is shipped and tracking number is available.
    /// </summary>
    [MaxLength(100)]
    public string? TrackingNumber { get; set; }
    
    /// <summary>
    /// Shipping carrier for this medication delivery.
    /// Used for delivery tracking and shipment management.
    /// Set when delivery is shipped and carrier is determined.
    /// </summary>
    [MaxLength(100)]
    public string? Carrier { get; set; }
    
    /// <summary>
    /// Date and time when this medication delivery was shipped.
    /// Used for delivery tracking and shipment management.
    /// Set when delivery is shipped and tracking begins.
    /// </summary>
    public DateTime? ShippedAt { get; set; }
    
    /// <summary>
    /// Date and time when this medication delivery was delivered.
    /// Used for delivery tracking and completion management.
    /// Set when delivery is successfully completed.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }
    
    /// <summary>
    /// Estimated delivery date for this medication delivery.
    /// Used for delivery planning and customer communication.
    /// Set when delivery is processed and estimated date is available.
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; set; }
    
    /// <summary>
    /// JSON string containing the medications included in this delivery.
    /// Used for medication management and delivery content tracking.
    /// Set when delivery is created and medications are specified.
    /// </summary>
    [MaxLength(1000)]
    public string? Medications { get; set; } // JSON string of medications
    
    /// <summary>
    /// Instructions for this medication delivery.
    /// Used for delivery instructions and special handling requirements.
    /// Set when delivery is created and instructions are specified.
    /// </summary>
    [MaxLength(1000)]
    public string? Instructions { get; set; }
    
    /// <summary>
    /// Additional notes for this medication delivery.
    /// Used for delivery documentation and special notes.
    /// Set when delivery is created and notes are specified.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Shipping cost for this medication delivery.
    /// Used for delivery cost tracking and billing management.
    /// Set when delivery is processed and shipping cost is determined.
    /// </summary>
    public decimal ShippingCost { get; set; }
    
    /// <summary>
    /// Indicates whether this medication delivery requires a signature upon delivery.
    /// Used for delivery requirements and special handling.
    /// Defaults to false for standard deliveries.
    /// </summary>
    public bool RequiresSignature { get; set; } = false;
    
    /// <summary>
    /// Indicates whether this medication delivery requires refrigeration.
    /// Used for delivery requirements and special handling.
    /// Defaults to false for standard deliveries.
    /// </summary>
    public bool IsRefrigerated { get; set; } = false;
    
    /// <summary>
    /// Reason for delivery failure if applicable.
    /// Used for delivery failure tracking and troubleshooting.
    /// Set when delivery fails and failure reason is available.
    /// </summary>
    [MaxLength(500)]
    public string? FailureReason { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Collection of tracking events for this medication delivery.
    /// Used for delivery tracking and shipment monitoring.
    /// Includes all tracking events and status updates for this delivery.
    /// </summary>
    public virtual ICollection<DeliveryTracking> TrackingEvents { get; set; } = new List<DeliveryTracking>();
    
    // Computed Properties
    /// <summary>
    /// Indicates whether this medication delivery has been delivered.
    /// Returns true if delivery status is Delivered.
    /// Used for delivery completion checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsDelivered => Status == DeliveryStatus.Delivered;
    
    /// <summary>
    /// Indicates whether this medication delivery has been shipped.
    /// Returns true if delivery status is Shipped.
    /// Used for delivery shipping checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsShipped => Status == DeliveryStatus.Shipped;
    
    /// <summary>
    /// Indicates whether this medication delivery has failed.
    /// Returns true if delivery status is Failed.
    /// Used for delivery failure checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsFailed => Status == DeliveryStatus.Failed;
    
    /// <summary>
    /// Indicates whether this medication delivery has been returned.
    /// Returns true if delivery status is Returned.
    /// Used for delivery return checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsReturned => Status == DeliveryStatus.Returned;
} 
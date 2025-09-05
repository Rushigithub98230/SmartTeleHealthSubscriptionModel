using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core subscription status history entity that manages all subscription status changes in the system.
/// This entity handles subscription status change tracking, audit trails, and history management.
/// It serves as the central hub for subscription status history management, providing status change
/// tracking, audit trails, and history capabilities.
/// </summary>
public class SubscriptionStatusHistory : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the subscription status history record.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each subscription status history record in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Subscription that this status change belongs to.
    /// Links this status change to the specific subscription.
    /// Required for subscription-status history relationship management.
    /// </summary>
    [Required]
    public Guid SubscriptionId { get; set; }
    
    /// <summary>
    /// Navigation property to the Subscription that this status change belongs to.
    /// Provides access to subscription information for status history management.
    /// Used for subscription-status history relationship operations.
    /// </summary>
    public virtual Subscription Subscription { get; set; } = null!;
    
    /// <summary>
    /// Previous status of the subscription before the change.
    /// Used for status change tracking and audit trails.
    /// Optional - used for status change history and audit capabilities.
    /// </summary>
    [MaxLength(50)]
    public string? FromStatus { get; set; }
    
    /// <summary>
    /// New status of the subscription after the change.
    /// Used for status change tracking and audit trails.
    /// Required for status change history and audit capabilities.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ToStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Reason for the subscription status change.
    /// Used for status change documentation and audit trails.
    /// Optional - used for enhanced status change management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who made this status change.
    /// Links this status change to the specific user who made the change.
    /// Optional - used for user tracking and audit capabilities.
    /// </summary>
    public int? ChangedByUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who made this status change.
    /// Provides access to user information for status change management.
    /// Used for user-status change relationship operations.
    /// </summary>
    public virtual User? ChangedByUser { get; set; }
    
    /// <summary>
    /// Date and time when the subscription status was changed.
    /// Used for status change timing tracking and audit trails.
    /// Required for status change history and audit capabilities.
    /// </summary>
    [Required]
    public DateTime ChangedAt { get; set; }
    
    /// <summary>
    /// Additional metadata about the subscription status change.
    /// Used for status change documentation and audit trails.
    /// Optional - used for enhanced status change management and documentation.
    /// </summary>
    [MaxLength(1000)]
    public string? Metadata { get; set; }
    
    // Computed properties (not mapped to database)
    /// <summary>
    /// Indicates whether this record represents a status change.
    /// Used for status change checking and validation.
    /// Returns true if FromStatus is not null and different from ToStatus.
    /// </summary>
    [NotMapped]
    public bool IsStatusChange => !string.IsNullOrEmpty(FromStatus) && FromStatus != ToStatus;

    /// <summary>
    /// Duration the subscription spent in the previous status.
    /// Used for status duration tracking and analysis.
    /// Returns the time span between CreatedDate and ChangedAt if FromStatus is not null.
    /// </summary>
    [NotMapped]
    public TimeSpan DurationInPreviousStatus => FromStatus != null ? ChangedAt - CreatedDate.GetValueOrDefault() : TimeSpan.Zero;
} 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core service constraint entity that manages all service constraints in the system.
/// This entity handles service limitation definitions, constraint management, and subscription plan restrictions.
/// It serves as the central hub for service constraint management, providing service limitation,
/// constraint enforcement, and subscription plan restriction capabilities.
/// </summary>
public class ServiceConstraint : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the service constraint.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each service constraint in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible types of service constraints.
    /// Used for constraint type classification and management.
    /// </summary>
    public enum ConstraintType
    {
        /// <summary>Service has no limitations or restrictions.</summary>
        Unlimited,
        /// <summary>Service is limited by session count.</summary>
        SessionCount,
        /// <summary>Service is limited by time duration.</summary>
        TimeBased,
        /// <summary>Service has multiple types of limitations.</summary>
        Hybrid
    }

    /// <summary>
    /// Name of the service that this constraint applies to.
    /// Used for service identification and constraint management.
    /// Examples: "Consultations", "InstantChat", "VideoCalls".
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Type of constraint applied to this service.
    /// Used for constraint type classification and management.
    /// Required for constraint type enforcement and management.
    /// </summary>
    [Required]
    public ConstraintType Type { get; set; }

    /// <summary>
    /// Value of the constraint applied to this service.
    /// Used for constraint value enforcement and management.
    /// -1 for unlimited, >0 for limited, 0 for disabled.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Description of the service constraint.
    /// Used for constraint documentation and user communication.
    /// Optional - used for enhanced constraint management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of sessions allowed per month for this service.
    /// Used for session-based constraint enforcement and management.
    /// Set based on subscription plan limitations and service requirements.
    /// </summary>
    public int MaxSessionsPerMonth { get; set; }
    
    /// <summary>
    /// Maximum duration allowed per session in minutes for this service.
    /// Used for session duration constraint enforcement and management.
    /// Set based on subscription plan limitations and service requirements.
    /// </summary>
    public int MaxDurationPerSession { get; set; }
    
    /// <summary>
    /// Maximum number of concurrent sessions allowed for this service.
    /// Used for concurrent session constraint enforcement and management.
    /// Set based on subscription plan limitations and service requirements.
    /// </summary>
    public int MaxConcurrentSessions { get; set; }

    /// <summary>
    /// Total minutes allowed per month for this service.
    /// Used for time-based constraint enforcement and management.
    /// Optional - used for time-based service limitations.
    /// </summary>
    public int? TotalMinutesPerMonth { get; set; }

    /// <summary>
    /// Indicates whether file sharing is allowed for this service.
    /// Used for file sharing constraint enforcement and management.
    /// Defaults to true for standard file sharing capabilities.
    /// </summary>
    public bool AllowFileSharing { get; set; } = true;
    
    /// <summary>
    /// Indicates whether video chat is allowed for this service.
    /// Used for video chat constraint enforcement and management.
    /// Defaults to false for standard video chat limitations.
    /// </summary>
    public bool AllowVideoChat { get; set; } = false;
    
    /// <summary>
    /// Indicates whether priority queue access is allowed for this service.
    /// Used for priority queue constraint enforcement and management.
    /// Defaults to false for standard queue access.
    /// </summary>
    public bool PriorityQueue { get; set; } = false;
    
    /// <summary>
    /// Maximum message length allowed for this service.
    /// Used for message length constraint enforcement and management.
    /// Defaults to 1000 characters for standard message limitations.
    /// </summary>
    public int MaxMessageLength { get; set; } = 1000;

    /// <summary>
    /// Foreign key reference to the SubscriptionPlan that this constraint belongs to.
    /// Links this constraint to the specific subscription plan.
    /// Required for plan-constraint relationship management.
    /// </summary>
    public Guid SubscriptionPlanId { get; set; }
    
    /// <summary>
    /// Navigation property to the SubscriptionPlan that this constraint belongs to.
    /// Provides access to subscription plan information for constraint management.
    /// Used for plan-constraint relationship operations.
    /// </summary>
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    // Computed properties
    /// <summary>
    /// Indicates whether this service constraint allows unlimited usage.
    /// Used for constraint status checking and management.
    /// Returns true if Value is -1 (unlimited).
    /// </summary>
    [NotMapped]
    public bool IsUnlimited => Value == -1;

    /// <summary>
    /// Indicates whether this service constraint has limited usage.
    /// Used for constraint status checking and management.
    /// Returns true if Value is greater than 0 (limited).
    /// </summary>
    [NotMapped]
    public bool IsLimited => Value > 0;

    /// <summary>
    /// Indicates whether this service constraint is disabled.
    /// Used for constraint status checking and management.
    /// Returns true if Value is 0 (disabled).
    /// </summary>
    [NotMapped]
    public bool IsDisabled => Value == 0;
} 
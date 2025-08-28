using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class ServiceConstraint : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum ConstraintType
    {
        Unlimited,
        SessionCount,
        TimeBased,
        Hybrid
    }

    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty; // e.g., "Consultations", "InstantChat"

    [Required]
    public ConstraintType Type { get; set; }

    public int Value { get; set; } // -1 for unlimited, >0 for limited

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    // Session-based limits
    public int MaxSessionsPerMonth { get; set; }
    public int MaxDurationPerSession { get; set; } // minutes
    public int MaxConcurrentSessions { get; set; }

    // Time-based limits
    public int? TotalMinutesPerMonth { get; set; }

    // Additional constraints
    public bool AllowFileSharing { get; set; } = true;
    public bool AllowVideoChat { get; set; } = false;
    public bool PriorityQueue { get; set; } = false;
    public int MaxMessageLength { get; set; } = 1000;

    // Foreign keys
    public Guid SubscriptionPlanId { get; set; }
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    // Computed properties
    [NotMapped]
    public bool IsUnlimited => Value == -1;

    [NotMapped]
    public bool IsLimited => Value > 0;

    [NotMapped]
    public bool IsDisabled => Value == 0;
} 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core entity that tracks how users have used their subscription privileges.
/// This entity handles privilege usage tracking including usage limits, period management, and access control.
/// It serves as the central hub for monitoring user privilege consumption, enforcing usage limits,
/// and providing analytics on privilege usage patterns. The entity includes comprehensive usage tracking,
/// period management, and integration with subscription and privilege systems.
/// </summary>
#region Improved UserSubscriptionPrivilegeUsage Entity
public class UserSubscriptionPrivilegeUsage : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the user subscription privilege usage record.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each privilege usage tracking record.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Subscription that this usage record belongs to.
    /// Links this usage record to the specific user subscription.
    /// Required for subscription-based usage tracking and management.
    /// </summary>
    [Required]
    public Guid SubscriptionId { get; set; }
    
    /// <summary>
    /// Navigation property to the Subscription that this usage record belongs to.
    /// Provides access to subscription information and details.
    /// Used for subscription-based usage tracking and management.
    /// </summary>
    public virtual Subscription Subscription { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the SubscriptionPlanPrivilege that defines this usage.
    /// Links this usage record to the specific privilege configuration.
    /// Required for privilege-based usage tracking and management.
    /// </summary>
    [Required]
    public Guid SubscriptionPlanPrivilegeId { get; set; }
    
    /// <summary>
    /// Navigation property to the SubscriptionPlanPrivilege that defines this usage.
    /// Provides access to privilege configuration and details.
    /// Used for privilege-based usage tracking and management.
    /// </summary>
    public virtual SubscriptionPlanPrivilege SubscriptionPlanPrivilege { get; set; } = null!;
    
    /// <summary>
    /// Number of times this privilege has been used by the user.
    /// Used for usage tracking and limit enforcement.
    /// Incremented each time the user uses the privilege.
    /// </summary>
    public int UsedValue { get; set; } = 0;
    
    /// <summary>
    /// Maximum number of times this privilege can be used by the user.
    /// -1 indicates unlimited usage, >0 indicates limited usage.
    /// Used for usage limit enforcement and access control.
    /// </summary>
    public int AllowedValue { get; set; }
    
    /// <summary>
    /// Start date of the usage period for this privilege.
    /// Used for usage period tracking and limit enforcement.
    /// Defines when the usage period begins for this privilege.
    /// </summary>
    [Required]
    public DateTime UsagePeriodStart { get; set; }
    
    /// <summary>
    /// End date of the usage period for this privilege.
    /// Used for usage period tracking and limit enforcement.
    /// Defines when the usage period ends for this privilege.
    /// </summary>
    [Required]
    public DateTime UsagePeriodEnd { get; set; }
    
    /// <summary>
    /// Date and time when this privilege was last used by the user.
    /// Used for usage tracking and analytics.
    /// Updated each time the user uses the privilege.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Date and time when this usage record was last reset.
    /// Used for usage period management and limit enforcement.
    /// Set when the usage period resets or when usage is manually reset.
    /// </summary>
    public DateTime? ResetAt { get; set; }
    
    /// <summary>
    /// Additional notes or comments about this privilege usage record.
    /// Used for usage tracking, analytics, and customer support.
    /// Can include usage context, special circumstances, or administrative notes.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation property for detailed usage history
    /// <summary>
    /// Collection of all detailed usage history records for this privilege usage.
    /// Represents the complete history of privilege usage events.
    /// Used for detailed usage tracking, analytics, and audit trails.
    /// </summary>
    public virtual ICollection<PrivilegeUsageHistory> UsageHistory { get; set; } = new List<PrivilegeUsageHistory>();
    
    // Computed Properties
    /// <summary>
    /// Computed property that returns the remaining usage value for this privilege.
    /// Returns int.MaxValue for unlimited privileges, otherwise returns the difference between allowed and used values.
    /// Used for usage limit checking and access control.
    /// </summary>
    [NotMapped]
    public int RemainingValue => AllowedValue == -1 ? int.MaxValue : Math.Max(0, AllowedValue - UsedValue);
    
    /// <summary>
    /// Computed property that indicates whether this privilege has unlimited usage.
    /// Returns true if AllowedValue is -1, indicating unlimited usage.
    /// Used for usage limit checking and access control.
    /// </summary>
    [NotMapped]
    public bool IsUnlimited => AllowedValue == -1;
    
    /// <summary>
    /// Computed property that indicates whether this privilege usage is exhausted.
    /// Returns true if usage is not unlimited and used value equals or exceeds allowed value.
    /// Used for usage limit checking and access control.
    /// </summary>
    [NotMapped]
    public bool IsExhausted => !IsUnlimited && UsedValue >= AllowedValue;
    
    /// <summary>
    /// Computed property that returns the usage percentage for this privilege.
    /// Returns 0 for unlimited privileges, 100 for exhausted privileges, otherwise returns the percentage used.
    /// Used for usage analytics and progress tracking.
    /// </summary>
    [NotMapped]
    public decimal UsagePercentage => IsUnlimited ? 0 : AllowedValue == 0 ? 100 : (decimal)UsedValue / AllowedValue * 100;
    
    /// <summary>
    /// Computed property that indicates whether this usage record is for the current period.
    /// Returns true if current date is within the usage period start and end dates.
    /// Used for usage period checking and access control.
    /// </summary>
    [NotMapped]
    public bool IsCurrentPeriod => DateTime.UtcNow >= UsagePeriodStart && DateTime.UtcNow <= UsagePeriodEnd;
}
#endregion 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Junction entity that defines the relationship between subscription plans and privileges.
/// This entity handles the mapping of privileges to subscription plans, including usage limits,
/// time-based restrictions, and access control. It serves as the bridge between subscription plans
/// and privileges, defining what privileges are available to users of each plan and how they can be used.
/// The entity includes comprehensive usage limit management, time-based restrictions, and access control.
/// </summary>
#region Improved SubscriptionPlanPrivilege Entity
public class SubscriptionPlanPrivilege : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the subscription plan privilege.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each subscription plan privilege mapping.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the SubscriptionPlan that includes this privilege.
    /// Links this privilege mapping to the specific subscription plan.
    /// Required for subscription plan privilege management.
    /// </summary>
    [Required]
    public Guid SubscriptionPlanId { get; set; }
    
    /// <summary>
    /// Navigation property to the SubscriptionPlan that includes this privilege.
    /// Provides access to subscription plan information and details.
    /// Used for subscription plan privilege management and access control.
    /// </summary>
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Privilege that is included in this subscription plan.
    /// Links this privilege mapping to the specific privilege.
    /// Required for subscription plan privilege management.
    /// </summary>
    [Required]
    public Guid PrivilegeId { get; set; }
    
    /// <summary>
    /// Navigation property to the Privilege that is included in this subscription plan.
    /// Provides access to privilege information and details.
    /// Used for privilege management and access control.
    /// </summary>
    public virtual Privilege Privilege { get; set; } = null!;
    
    /// <summary>
    /// Usage limit value for this privilege in the subscription plan.
    /// -1 indicates unlimited usage, 0 indicates disabled, >0 indicates limited usage.
    /// Used for usage limit enforcement and access control.
    /// </summary>
    public int Value { get; set; }
    
    /// <summary>
    /// REMOVED: UsagePeriodId was not used in business logic.
    /// Privilege resets are based on subscription's BillingCycle, not a separate UsagePeriod.
    /// See: PrivilegeAllocationCalculator.cs - uses subscription.BillingCycle.DurationInDays
    /// </summary>
    // [Required]
    // public Guid UsagePeriodId { get; set; }
    
    // public virtual MasterBillingCycle UsagePeriod { get; set; } = null!;
    
    /// <summary>
    /// Duration in months for this privilege in the subscription plan.
    /// Used for privilege duration management and access control.
    /// Defaults to 1 month for standard privilege durations.
    /// </summary>
    public int DurationMonths { get; set; } = 1;
    
    /// <summary>
    /// Additional description for this privilege in the subscription plan.
    /// Used for privilege information display and user education.
    /// Provides context-specific information about the privilege's availability.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Date when this privilege becomes effective in the subscription plan.
    /// Used for privilege activation and availability control.
    /// Privileges are not available before this date.
    /// </summary>
    public DateTime? EffectiveDate { get; set; }
    
    /// <summary>
    /// Date when this privilege expires in the subscription plan.
    /// Used for privilege deactivation and availability control.
    /// Privileges are not available after this date.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
    
    /// <summary>
    /// BASE COST: Cost per unit for THIS PLAN (contributes to plan's base price).
    /// Used to calculate: Plan Price = Σ(Value × PrivilegeBaseCost) + Commission.
    /// Example: 5 consultations × $3 base cost = $15 contribution to plan price.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrivilegeBaseCost { get; set; } = 0;
    
    /// <summary>
    /// OVERAGE COST: Cost per unit when user exceeds limit.
    /// Healthcare Rule: Overage uses LATEST plan version pricing to prevent abuse.
    /// Example: 6th consultation costs $15 (from latest plan, not user's old plan).
    /// Used for calculating overage charges when users exceed their plan limits.
    /// This allows different plans to charge different rates for the same privilege.
    /// Example: Basic plan charges $2 per teleconsultation overage, Premium plan charges $4.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; } = 0;
    
    // Computed Properties
    /// <summary>
    /// Computed property that indicates whether this privilege has unlimited usage.
    /// Returns true if Value is -1, indicating unlimited usage.
    /// Used for usage limit checking and access control.
    /// </summary>
    [NotMapped]
    public bool IsUnlimited => Value == -1;
    
    /// <summary>
    /// Computed property that indicates whether this privilege is disabled.
    /// Returns true if Value is 0, indicating the privilege is disabled.
    /// Used for privilege availability checking and access control.
    /// </summary>
    [NotMapped]
    public bool IsDisabled => Value == 0;
    
    /// <summary>
    /// Computed property that indicates whether this privilege has limited usage.
    /// Returns true if Value is greater than 0, indicating limited usage.
    /// Used for usage limit checking and access control.
    /// </summary>
    [NotMapped]
    public bool IsLimited => Value > 0;
    
    /// <summary>
    /// Computed property that indicates whether this privilege is currently active.
    /// Returns true if privilege is active and within the effective date range.
    /// Used for privilege availability checking and access control.
    /// </summary>
    [NotMapped]
    public bool IsCurrentlyActive => IsActive && 
        (!EffectiveDate.HasValue || EffectiveDate.Value <= DateTime.UtcNow) &&
        (!ExpirationDate.HasValue || ExpirationDate.Value >= DateTime.UtcNow);
    
    
    /// <summary>
    /// Computed property that indicates whether this privilege has overage charges.
    /// Returns true if the privilege has a unit cost greater than 0 and is not unlimited.
    /// Used for overage billing calculations and user notifications.
    /// </summary>
    [NotMapped]
    public bool HasOverageCharges => UnitCost > 0 && !IsUnlimited;
}
#endregion 
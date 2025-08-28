using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

#region Improved SubscriptionPlanPrivilege Entity
public class SubscriptionPlanPrivilege : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SubscriptionPlanId { get; set; }
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    [Required]
    public Guid PrivilegeId { get; set; }
    public virtual Privilege Privilege { get; set; } = null!;
    
    public int Value { get; set; } // -1 for unlimited, 0 for disabled, >0 for limited
    
    [Required]
    public Guid UsagePeriodId { get; set; }
    public virtual MasterBillingCycle UsagePeriod { get; set; } = null!;
    
    public int DurationMonths { get; set; } = 1;
    
    // IsActive is inherited from BaseEntity
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    // New properties for time-based limits
    public int? DailyLimit { get; set; }        // Max per day (null = no limit)
    public int? WeeklyLimit { get; set; }       // Max per week (null = no limit)
    public int? MonthlyLimit { get; set; }      // Max per month (null = no limit)
    
    // Computed Properties
    [NotMapped]
    public bool IsUnlimited => Value == -1;
    
    [NotMapped]
    public bool IsDisabled => Value == 0;
    
    [NotMapped]
    public bool IsLimited => Value > 0;
    
    [NotMapped]
    public bool IsCurrentlyActive => IsActive && 
        (!EffectiveDate.HasValue || EffectiveDate.Value <= DateTime.UtcNow) &&
        (!ExpirationDate.HasValue || ExpirationDate.Value >= DateTime.UtcNow);
    
    [NotMapped]
    public bool HasTimeRestrictions => DailyLimit.HasValue || WeeklyLimit.HasValue || MonthlyLimit.HasValue;
}
#endregion 
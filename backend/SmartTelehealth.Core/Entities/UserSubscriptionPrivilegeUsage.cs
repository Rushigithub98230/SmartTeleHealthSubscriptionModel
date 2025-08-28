using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

#region Improved UserSubscriptionPrivilegeUsage Entity
public class UserSubscriptionPrivilegeUsage : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;
    
    [Required]
    public Guid SubscriptionPlanPrivilegeId { get; set; }
    public virtual SubscriptionPlanPrivilege SubscriptionPlanPrivilege { get; set; } = null!;
    
    public int UsedValue { get; set; } = 0;
    public int AllowedValue { get; set; }
    
    [Required]
    public DateTime UsagePeriodStart { get; set; }
    [Required]
    public DateTime UsagePeriodEnd { get; set; }
    
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ResetAt { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation property for detailed usage history
    public virtual ICollection<PrivilegeUsageHistory> UsageHistory { get; set; } = new List<PrivilegeUsageHistory>();
    
    // Computed Properties
    [NotMapped]
    public int RemainingValue => AllowedValue == -1 ? int.MaxValue : Math.Max(0, AllowedValue - UsedValue);
    
    [NotMapped]
    public bool IsUnlimited => AllowedValue == -1;
    
    [NotMapped]
    public bool IsExhausted => !IsUnlimited && UsedValue >= AllowedValue;
    
    [NotMapped]
    public decimal UsagePercentage => IsUnlimited ? 0 : AllowedValue == 0 ? 100 : (decimal)UsedValue / AllowedValue * 100;
    
    [NotMapped]
    public bool IsCurrentPeriod => DateTime.UtcNow >= UsagePeriodStart && DateTime.UtcNow <= UsagePeriodEnd;
}
#endregion 
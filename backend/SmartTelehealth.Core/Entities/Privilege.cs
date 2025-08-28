using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class Privilege : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid PrivilegeTypeId { get; set; }
    public virtual MasterPrivilegeType PrivilegeType { get; set; } = null!;

    // Navigation
    public virtual ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; } = new List<SubscriptionPlanPrivilege>();
    public virtual ICollection<UserSubscriptionPrivilegeUsage> UsageRecords { get; set; } = new List<UserSubscriptionPrivilegeUsage>();
} 
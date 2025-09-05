using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core privilege entity that defines the available privileges and permissions in the system.
/// This entity handles privilege management including privilege types, descriptions, and access control.
/// It serves as the foundation for the privilege system, defining what actions and features users can access
/// based on their subscription plans. The entity includes comprehensive privilege type management and
/// integration with subscription plans and usage tracking.
/// </summary>
public class Privilege : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the privilege.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each privilege in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the privilege for display and identification purposes.
    /// Required field for privilege management and user interface display.
    /// Used in privilege selection, access control, and user interface.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the privilege and what it allows users to do.
    /// Used for privilege information display and user education.
    /// Provides comprehensive information about the privilege's capabilities.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Foreign key reference to the PrivilegeType that categorizes this privilege.
    /// Determines the category and type of privilege (e.g., messaging, consultation, medication).
    /// Required for privilege categorization and management.
    /// </summary>
    public Guid PrivilegeTypeId { get; set; }
    
    /// <summary>
    /// Navigation property to the PrivilegeType that categorizes this privilege.
    /// Provides access to privilege type information and categorization.
    /// Used for privilege type management and access control.
    /// </summary>
    public virtual MasterPrivilegeType PrivilegeType { get; set; } = null!;

    // Navigation properties - These establish relationships with other entities
    
    /// <summary>
    /// Collection of all subscription plan privileges that include this privilege.
    /// Represents which subscription plans include this privilege.
    /// Used for privilege management and subscription plan configuration.
    /// </summary>
    public virtual ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; } = new List<SubscriptionPlanPrivilege>();
    
    /// <summary>
    /// Collection of all usage records for this privilege.
    /// Represents how users have used this privilege across all subscriptions.
    /// Used for usage tracking, analytics, and privilege management.
    /// </summary>
    public virtual ICollection<UserSubscriptionPrivilegeUsage> UsageRecords { get; set; } = new List<UserSubscriptionPrivilegeUsage>();
} 
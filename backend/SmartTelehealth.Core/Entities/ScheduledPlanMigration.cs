using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Tracks scheduled migrations of subscriptions from old to new plan versions.
/// Healthcare Workflow: Users migrate at their next individual renewal date.
/// </summary>
public class ScheduledPlanMigration : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;
    
    [Required]
    public Guid FromPlanId { get; set; }
    public virtual SubscriptionPlan FromPlan { get; set; } = null!;
    
    [Required]
    public Guid ToPlanId { get; set; }
    public virtual SubscriptionPlan ToPlan { get; set; } = null!;
    
    /// <summary>
    /// Date when user was notified about the price change.
    /// </summary>
    [Required]
    public DateTime NotificationDate { get; set; }
    
    /// <summary>
    /// User's next renewal date when migration will occur.
    /// Healthcare Rule: Migrate at next renewal, not fixed date.
    /// </summary>
    [Required]
    public DateTime ScheduledMigrationDate { get; set; }
    
    /// <summary>
    /// Status: Pending, UserOptedOut, Completed, Failed.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";
    
    /// <summary>
    /// User's choice: Accept, Downgrade, Cancel.
    /// </summary>
    [MaxLength(50)]
    public string? UserDecision { get; set; }
    
    public DateTime? UserDecisionDate { get; set; }
    
    /// <summary>
    /// If user chose to downgrade, the plan they selected.
    /// </summary>
    public Guid? DowngradeToPlanId { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}


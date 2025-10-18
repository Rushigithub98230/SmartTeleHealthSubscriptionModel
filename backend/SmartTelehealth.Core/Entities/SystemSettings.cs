using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// System-wide settings for subscription management.
/// Choice 2c: Global defaults with per-plan overrides.
/// </summary>
public class SystemSettings : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Global default admin commission percentage.
    /// Used when SubscriptionPlan.AdminCommissionPercent is null.
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal DefaultAdminCommissionPercent { get; set; } = 20; // 20%
    
    /// <summary>
    /// Global default price change notice period in days.
    /// Used when plan doesn't specify custom period.
    /// </summary>
    public int DefaultPriceChangeNoticeDays { get; set; } = 10;
    
    /// <summary>
    /// Maximum number of failed payment retry attempts before suspension.
    /// </summary>
    public int MaxFailedPaymentAttempts { get; set; } = 3;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}


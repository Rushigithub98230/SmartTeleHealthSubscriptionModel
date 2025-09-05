using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core privilege usage history entity that manages all privilege usage history in the system.
/// This entity handles privilege usage tracking, time-based limit enforcement, and usage analytics.
/// It serves as the central hub for privilege usage history management, providing usage tracking,
/// time-based limit enforcement, and analytics capabilities.
/// </summary>
public class PrivilegeUsageHistory : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the privilege usage history record.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each privilege usage history record in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key reference to the UserSubscriptionPrivilegeUsage that this history record belongs to.
    /// Links this history record to the specific privilege usage.
    /// Required for privilege usage-history relationship management.
    /// </summary>
    [Required]
    public Guid UserSubscriptionPrivilegeUsageId { get; set; }
    
    /// <summary>
    /// Navigation property to the UserSubscriptionPrivilegeUsage that this history record belongs to.
    /// Provides access to privilege usage information for history management.
    /// Used for privilege usage-history relationship operations.
    /// </summary>
    public virtual UserSubscriptionPrivilegeUsage UserSubscriptionPrivilegeUsage { get; set; } = null!;
    
    /// <summary>
    /// Amount of privilege used in this instance.
    /// Used for privilege usage tracking and limit enforcement.
    /// Defaults to 1 for standard privilege usage tracking.
    /// </summary>
    public int UsedValue { get; set; } = 1;
    
    /// <summary>
    /// Date and time when the privilege was used.
    /// Used for privilege usage timing tracking and management.
    /// Set when the privilege is used by the user.
    /// </summary>
    [Required]
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date when the privilege was used (without time component).
    /// Used for privilege usage date tracking and management.
    /// Set to the date when the privilege is used by the user.
    /// </summary>
    [Required]
    public DateTime UsageDate { get; set; } = DateTime.UtcNow.Date;
    
    /// <summary>
    /// Week identifier for the privilege usage in YYYY-WW format.
    /// Used for privilege usage week tracking and management.
    /// Set based on the usage date for week-based analytics.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string UsageWeek { get; set; } = string.Empty;
    
    /// <summary>
    /// Month identifier for the privilege usage in YYYY-MM format.
    /// Used for privilege usage month tracking and management.
    /// Set based on the usage date for month-based analytics.
    /// </summary>
    [Required]
    [MaxLength(7)]
    public string UsageMonth { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional notes about the privilege usage.
    /// Used for privilege usage documentation and management.
    /// Optional - used for enhanced privilege usage management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Computed Properties
    /// <summary>
    /// Week key for the privilege usage in YYYY-WW format.
    /// Used for privilege usage week tracking and management.
    /// Returns formatted week key based on the usage date.
    /// </summary>
    [NotMapped]
    public string WeekKey => $"{UsageDate:yyyy}-{GetWeekNumber(UsageDate):D2}";
    
    /// <summary>
    /// Month key for the privilege usage in YYYY-MM format.
    /// Used for privilege usage month tracking and management.
    /// Returns formatted month key based on the usage date.
    /// </summary>
    [NotMapped]
    public string MonthKey => $"{UsageDate:yyyy-MM}";
    
    /// <summary>
    /// Gets the week number for a given date.
    /// Used for privilege usage week calculation and management.
    /// Returns the week number based on the calendar year.
    /// </summary>
    /// <param name="date">The date to get the week number for</param>
    /// <returns>The week number for the given date</returns>
    private static int GetWeekNumber(DateTime date)
    {
        var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}

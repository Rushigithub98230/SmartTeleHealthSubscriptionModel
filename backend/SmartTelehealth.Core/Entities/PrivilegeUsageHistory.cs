using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Tracks detailed privilege usage history for time-based limit enforcement
/// </summary>
public class PrivilegeUsageHistory : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserSubscriptionPrivilegeUsageId { get; set; }
    public virtual UserSubscriptionPrivilegeUsage UserSubscriptionPrivilegeUsage { get; set; } = null!;
    
    public int UsedValue { get; set; } = 1;  // Amount used in this instance
    
    [Required]
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UsageDate { get; set; } = DateTime.UtcNow.Date;  // Date of usage
    
    [Required]
    [MaxLength(10)]
    public string UsageWeek { get; set; } = string.Empty;  // YYYY-WW format
    
    [Required]
    [MaxLength(7)]
    public string UsageMonth { get; set; } = string.Empty;  // YYYY-MM format
    
    [MaxLength(500)]
    public string? Notes { get; set; }  // Optional notes about the usage
    
    // Computed Properties
    [NotMapped]
    public string WeekKey => $"{UsageDate:yyyy}-{GetWeekNumber(UsageDate):D2}";
    
    [NotMapped]
    public string MonthKey => $"{UsageDate:yyyy-MM}";
    
    private static int GetWeekNumber(DateTime date)
    {
        var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}

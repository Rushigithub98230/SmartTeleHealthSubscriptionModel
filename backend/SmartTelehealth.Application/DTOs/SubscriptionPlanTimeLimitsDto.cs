using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for creating subscription plans with time-based privilege limits
/// </summary>
public class CreateSubscriptionPlanWithTimeLimitsDto
{
    [Required]
    [MaxLength(100)]
    public string PlanName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    public string BillingCycle { get; set; } = string.Empty; // "Monthly", "Quarterly", "Annual"
    
    [Required]
    [Range(1, 120)]
    public int DurationMonths { get; set; } = 1;
    
    [Required]
    public List<PrivilegeTimeLimitDto> Privileges { get; set; } = new();
}

/// <summary>
/// DTO for individual privilege with time-based limits
/// </summary>
public class PrivilegeTimeLimitDto
{
    [Required]
    [MaxLength(100)]
    public string PrivilegeName { get; set; } = string.Empty;
    
    [Required]
    public int TotalValue { get; set; } // -1 for unlimited, >0 for limited
    
    public int? DailyLimit { get; set; }        // Max per day (null = no limit)
    public int? WeeklyLimit { get; set; }       // Max per week (null = no limit)
    public int? MonthlyLimit { get; set; }      // Max per month (null = no limit)
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

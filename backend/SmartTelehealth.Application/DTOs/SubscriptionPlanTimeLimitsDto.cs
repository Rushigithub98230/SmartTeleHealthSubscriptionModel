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
    
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

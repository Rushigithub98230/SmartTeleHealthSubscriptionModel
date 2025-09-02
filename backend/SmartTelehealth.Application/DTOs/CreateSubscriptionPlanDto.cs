using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Application.DTOs;

public class CreateSubscriptionPlanDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(200)]
    public string? ShortDescription { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountedPrice { get; set; }
    
    public DateTime? DiscountValidUntil { get; set; }
    
    [Required]
    public Guid BillingCycleId { get; set; }
    
    [Required]
    public Guid CurrencyId { get; set; }
    
    // Trial configuration
    public bool IsTrialAllowed { get; set; } = false;
    
    [Range(0, int.MaxValue, ErrorMessage = "Trial duration must be 0 or positive")]
    public int TrialDurationInDays { get; set; } = 0;
    
    // Marketing and display properties
    public bool IsFeatured { get; set; } = false;
    public bool IsMostPopular { get; set; } = false;
    public bool IsTrending { get; set; } = false;
    public int DisplayOrder { get; set; }
    
    // Plan features and limits
    [Range(0, int.MaxValue, ErrorMessage = "Messaging count must be 0 or positive")]
    public int MessagingCount { get; set; } = 10;
    
    public bool IncludesMedicationDelivery { get; set; } = true;
    public bool IncludesFollowUpCare { get; set; } = true;
    
    [Range(1, int.MaxValue, ErrorMessage = "Delivery frequency must be at least 1 day")]
    public int DeliveryFrequencyDays { get; set; } = 30;
    
    [Range(0, int.MaxValue, ErrorMessage = "Max pause duration must be 0 or positive")]
    public int MaxPauseDurationDays { get; set; } = 90;
    
    [Range(1, int.MaxValue, ErrorMessage = "Max concurrent users must be at least 1")]
    public int MaxConcurrentUsers { get; set; } = 1;
    
    [Range(0, int.MaxValue, ErrorMessage = "Grace period must be 0 or positive")]
    public int GracePeriodDays { get; set; } = 0;
    
    // Plan status
    public bool IsActive { get; set; } = true;
    
    // Plan metadata
    [MaxLength(1000)]
    public string? Features { get; set; }
    
    [MaxLength(500)]
    public string? Terms { get; set; }
    
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    // Privilege configuration - NEW
    public List<PlanPrivilegeDto> Privileges { get; set; } = new List<PlanPrivilegeDto>();
}

/// <summary>
/// DTO for configuring privileges within a subscription plan
/// </summary>
public class PlanPrivilegeDto
{
    [Required]
    [CustomValidation(typeof(PlanPrivilegeDto), nameof(ValidateGuidNotEmpty))]
    public Guid PrivilegeId { get; set; }
    
    [Required]
    [Range(-1, int.MaxValue, ErrorMessage = "Value must be -1 (unlimited), 0 (disabled), or positive number")]
    public int Value { get; set; } // -1 for unlimited, 0 for disabled, >0 for limited
    
    [Required]
    public Guid UsagePeriodId { get; set; }
    
    public int DurationMonths { get; set; } = 1;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime? EffectiveDate { get; set; }
    
    [CustomValidation(typeof(PlanPrivilegeDto), nameof(ValidateExpirationDate))]
    public DateTime? ExpirationDate { get; set; }
    
    // Time-based limits
    [Range(0, int.MaxValue, ErrorMessage = "Daily limit must be 0 or positive")]
    public int? DailyLimit { get; set; }        // Max per day (null = no limit)
    
    [Range(0, int.MaxValue, ErrorMessage = "Weekly limit must be 0 or positive")]
    public int? WeeklyLimit { get; set; }       // Max per week (null = no limit)
    
    [Range(0, int.MaxValue, ErrorMessage = "Monthly limit must be 0 or positive")]
    public int? MonthlyLimit { get; set; }      // Max per month (null = no limit)
    
    public static ValidationResult? ValidateExpirationDate(DateTime? expirationDate, ValidationContext validationContext)
    {
        if (expirationDate.HasValue && expirationDate.Value < DateTime.UtcNow)
        {
            return new ValidationResult("Expiration date cannot be in the past", new[] { nameof(ExpirationDate) });
        }
        return ValidationResult.Success;
    }
    
    public static ValidationResult? ValidateGuidNotEmpty(Guid guid, ValidationContext validationContext)
    {
        if (guid == Guid.Empty)
        {
            return new ValidationResult("GUID cannot be empty", new[] { validationContext.MemberName ?? "Guid" });
        }
        return ValidationResult.Success;
    }
} 
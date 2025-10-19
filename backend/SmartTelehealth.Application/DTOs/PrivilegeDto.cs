using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class CreatePrivilegeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid PrivilegeTypeId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdatePrivilegeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid PrivilegeTypeId { get; set; }

    public bool IsActive { get; set; }
}

public class PrivilegeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid PrivilegeTypeId { get; set; }
    public string PrivilegeTypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

/// <summary>
/// DTO for updating time-based usage limits for a subscription plan privilege
/// </summary>
public class UpdateTimeBasedLimitsDto
{
    [Required]
    public Guid PrivilegeId { get; set; }
    
    
    // REMOVED: Not used in business logic - privileges reset based on subscription billing cycle
    // public Guid UsagePeriodId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 month")]
    public int DurationMonths { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DateTime EffectiveDate { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
}
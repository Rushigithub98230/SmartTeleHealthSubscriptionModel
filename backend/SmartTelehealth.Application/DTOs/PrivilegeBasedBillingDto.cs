using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for calculating subscription plan base price
/// </summary>
public class CalculatePlanPriceDto
{
    /// <summary>
    /// Subscription plan ID
    /// </summary>
    [Required]
    public Guid PlanId { get; set; }

    /// <summary>
    /// Admin commission percentage (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Admin commission percentage must be between 0 and 100")]
    public decimal AdminCommissionPercentage { get; set; } = 0;

    /// <summary>
    /// Fixed admin commission amount
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Admin commission amount must be positive")]
    public decimal AdminCommissionFixed { get; set; } = 0;
}

/// <summary>
/// DTO for processing privilege usage
/// </summary>
public class ProcessPrivilegeUsageDto
{
    /// <summary>
    /// User ID who is using the privilege
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Privilege ID being used
    /// </summary>
    [Required]
    public Guid PrivilegeId { get; set; }

    /// <summary>
    /// Number of units being consumed
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Usage count must be at least 1")]
    public int UsageCount { get; set; } = 1;

    /// <summary>
    /// Description of the usage
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

/// <summary>
/// DTO for privilege usage summary
/// </summary>
public class PrivilegeUsageSummaryDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Subscription ID
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// List of privilege usage details
    /// </summary>
    public List<PrivilegeUsageDetailDto> UsageDetails { get; set; } = new();

    /// <summary>
    /// Total overage charges
    /// </summary>
    public decimal TotalOverageCharges { get; set; }

    /// <summary>
    /// Summary generated at
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO for individual privilege usage detail
/// </summary>
public class PrivilegeUsageDetailDto
{
    /// <summary>
    /// Privilege ID
    /// </summary>
    public Guid PrivilegeId { get; set; }

    /// <summary>
    /// Privilege name
    /// </summary>
    public string PrivilegeName { get; set; } = string.Empty;

    /// <summary>
    /// Number of units used
    /// </summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// Daily limit for this privilege
    /// </summary>
    public int DailyLimit { get; set; }

    /// <summary>
    /// Unit cost for this privilege
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Whether usage exceeds the limit
    /// </summary>
    public bool IsOverLimit { get; set; }

    /// <summary>
    /// Number of units over the limit
    /// </summary>
    public int OverageCount { get; set; }

    /// <summary>
    /// Charge for overage
    /// </summary>
    public decimal OverageCharge { get; set; }

    /// <summary>
    /// Number of units remaining in the limit
    /// </summary>
    public int RemainingCount { get; set; }
}

/// <summary>
/// DTO for plan price calculation result
/// </summary>
public class PlanPriceCalculationDto
{
    /// <summary>
    /// Plan ID
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Plan name
    /// </summary>
    public string PlanName { get; set; } = string.Empty;

    /// <summary>
    /// Base price calculated from privileges
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Admin commission amount
    /// </summary>
    public decimal AdminCommission { get; set; }

    /// <summary>
    /// Final price including commission
    /// </summary>
    public decimal FinalPrice { get; set; }

    /// <summary>
    /// Breakdown of privilege costs
    /// </summary>
    public List<PrivilegeCostBreakdownDto> PrivilegeBreakdown { get; set; } = new();

    /// <summary>
    /// Calculation timestamp
    /// </summary>
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// DTO for privilege cost breakdown
/// </summary>
public class PrivilegeCostBreakdownDto
{
    /// <summary>
    /// Privilege ID
    /// </summary>
    public Guid PrivilegeId { get; set; }

    /// <summary>
    /// Privilege name
    /// </summary>
    public string PrivilegeName { get; set; } = string.Empty;

    /// <summary>
    /// Daily limit for this privilege
    /// </summary>
    public int DailyLimit { get; set; }

    /// <summary>
    /// Unit cost for this privilege
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Total cost for this privilege (limit * unit cost)
    /// </summary>
    public decimal TotalCost { get; set; }
}

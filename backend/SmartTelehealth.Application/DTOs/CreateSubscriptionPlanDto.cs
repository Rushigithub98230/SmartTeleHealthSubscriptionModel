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
    
    // ✅ BasePrice is now calculated automatically from privileges + commission
    // Only used for manual override if IsAutoCalculatedPrice = false
    [Range(0, double.MaxValue, ErrorMessage = "Base price must be 0 or positive")]
    public decimal BasePrice { get; set; } = 0; // Default to 0, will be calculated
    
    [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100%")]
    public decimal? DiscountPercentage { get; set; }
    
    public DateTime? DiscountValidUntil { get; set; }
    
    [Required]
    public Guid BillingCycleId { get; set; }
    
    [Required]
    public Guid CurrencyId { get; set; }
    
    [Required]
    public Guid CategoryId { get; set; }
    
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
    
    // Stripe integration fields
    [MaxLength(100)]
    public string? StripeProductId { get; set; }
    
    // NEW ARCHITECTURE: Each plan has ONE billing cycle, therefore ONE Stripe price
    [MaxLength(100)]
    public string? StripePriceId { get; set; }
    
    // Privilege configuration - NEW
    public List<PlanPrivilegeDto> Privileges { get; set; } = new List<PlanPrivilegeDto>();
    
    // ═══════════════════════════════════════════════════════════
    // HEALTHCARE PRICING MODEL (Choices 1c, 2c, 4d)
    // ═══════════════════════════════════════════════════════════
    
    /// <summary>
    /// Choice 1c: Pricing mode selection.
    /// true = Auto-calculate from privileges, false = Manual price entry.
    /// </summary>
    public bool IsAutoCalculatedPrice { get; set; } = true;
    
    /// <summary>
    /// Choice 2c: Per-plan commission override (percentage).
    /// Null = use global default from SystemSettings.
    /// </summary>
    [Range(0, 100, ErrorMessage = "Commission must be between 0 and 100%")]
    public decimal? AdminCommissionPercent { get; set; }
    
    
    /// <summary>
    /// Choice 4d: Configurable notice period per plan.
    /// How many days notice users get before price changes.
    /// </summary>
    [Range(7, 365, ErrorMessage = "Notice period must be between 7 and 365 days")]
    public int PriceChangeNoticeDays { get; set; } = 10; // Healthcare default
    
    // ═══════════════════════════════════════════════════════════
    // BILLING CYCLE DISCOUNTS (Solution A Implementation)
    // ═══════════════════════════════════════════════════════════
    
    // NEW ARCHITECTURE: Billing cycle discount fields removed
    // Each plan (Monthly, Quarterly, Annual) is now a separate entity with its own explicit price
    // Discounts are applied by setting different privilege costs per plan, not via discount percentages
    // For example:
    //   - Monthly plan: 10 consultations × $15 = $150
    //   - Annual plan: 150 consultations × $12 = $1,800 (lower unit cost = implicit discount)
    
    /// <summary>
    /// Billing cycle discount percentage (0-100).
    /// Applied after promotional discount.
    /// Used for applying discounts based on billing frequency.
    /// </summary>
    [Range(0, 100, ErrorMessage = "Billing discount percentage must be between 0 and 100%")]
    public decimal? BillingDiscountPercentage { get; set; }
    
    /// <summary>
    /// Default tax percentage to apply to this plan (0-100)
    /// Admin can set this manually for tax compliance
    /// </summary>
    [Range(0, 100, ErrorMessage = "Tax percentage must be between 0 and 100%")]
    public decimal? DefaultTaxPercentage { get; set; }

    /// <summary>
    /// Notes about tax applicability for this plan.
    /// </summary>
    [MaxLength(500)]
    public string? TaxNotes { get; set; }
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
    
    // REMOVED: Not used in business logic - privileges reset based on subscription billing cycle
    // [Required]
    // public Guid UsagePeriodId { get; set; }
    
    public int DurationMonths { get; set; } = 1;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime? EffectiveDate { get; set; }
    
    [CustomValidation(typeof(PlanPrivilegeDto), nameof(ValidateExpirationDate))]
    public DateTime? ExpirationDate { get; set; }
    
    // ═══════════════════════════════════════════════════════════
    // HEALTHCARE PRICING MODEL
    // ═══════════════════════════════════════════════════════════
    
    /// <summary>
    /// Base cost per unit for plan pricing calculation.
    /// Used to calculate: Plan Price = Σ(Value × PrivilegeBaseCost) + Commission.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Base cost must be 0 or positive")]
    public decimal PrivilegeBaseCost { get; set; } = 0;
    
    /// <summary>
    /// Overage cost per unit when user exceeds limits.
    /// Healthcare Rule: Uses LATEST plan version pricing to prevent abuse.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Unit cost must be 0 or positive")]
    public decimal UnitCost { get; set; } = 0;  // Cost per unit when used beyond limits
    
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
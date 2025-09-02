using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class SubscriptionPlanDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public DateTime? DiscountValidUntil { get; set; }
    public Guid BillingCycleId { get; set; }
    public Guid CurrencyId { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsTrialAllowed { get; set; }
    public int TrialDurationInDays { get; set; }
    
    // Marketing and display properties
    public bool IsMostPopular { get; set; }
    public bool IsTrending { get; set; }
    
    public int DisplayOrder { get; set; }
    public string? StripeProductId { get; set; }
    public string? StripeMonthlyPriceId { get; set; }
    public string? StripeQuarterlyPriceId { get; set; }
    public string? StripeAnnualPriceId { get; set; }
    public string? Features { get; set; }
    public string? Terms { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public decimal EffectivePrice { get; set; }
    public bool HasActiveDiscount { get; set; }
    public bool IsCurrentlyAvailable { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
} 
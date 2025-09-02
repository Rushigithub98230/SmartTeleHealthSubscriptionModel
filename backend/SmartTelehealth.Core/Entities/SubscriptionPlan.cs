using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

#region Improved SubscriptionPlan Entity
public class SubscriptionPlan : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(200)]
    public string? ShortDescription { get; set; }
    
    // IsActive is inherited from BaseEntity
    public bool IsFeatured { get; set; } = false;
    public bool IsTrialAllowed { get; set; } = false;
    public int TrialDurationInDays { get; set; } = 0;
    
    // Marketing and display properties
    public bool IsMostPopular { get; set; } = false;
    
    public bool IsTrending { get; set; } = false;
    
    public int DisplayOrder { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountedPrice { get; set; }
    
    public DateTime? DiscountValidUntil { get; set; }
    
    // Foreign keys
    [Required]
    public Guid BillingCycleId { get; set; }
    [Required]
    public Guid CurrencyId { get; set; }
    
    // Navigation properties
    public virtual MasterBillingCycle BillingCycle { get; set; } = null!;
    public virtual MasterCurrency Currency { get; set; } = null!;
    
    // Stripe Integration - Multiple price points for different billing cycles
    [MaxLength(100)]
    public string? StripeProductId { get; set; }
    
    [MaxLength(100)]
    public string? StripeMonthlyPriceId { get; set; }
    
    [MaxLength(100)]
    public string? StripeQuarterlyPriceId { get; set; }
    
    [MaxLength(100)]
    public string? StripeAnnualPriceId { get; set; }
    
    // Plan features and limits
    public int MessagingCount { get; set; } = 10;
    public bool IncludesMedicationDelivery { get; set; } = true;
    public bool IncludesFollowUpCare { get; set; } = true;
    public int DeliveryFrequencyDays { get; set; } = 30;
    public int MaxPauseDurationDays { get; set; } = 90;
    
    // Metadata
    [MaxLength(1000)]
    public string? Features { get; set; } // JSON string of features for display
    
    [MaxLength(500)]
    public string? Terms { get; set; }
    
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    // Alias properties for backward compatibility
   
    
    // Collection Navigation properties
    public virtual ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; } = new List<SubscriptionPlanPrivilege>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    // Computed Properties
    [NotMapped]
    public decimal EffectivePrice => DiscountedPrice ?? Price;
    
    [NotMapped]
    public bool HasActiveDiscount => DiscountedPrice.HasValue && 
        (!DiscountValidUntil.HasValue || DiscountValidUntil.Value >= DateTime.UtcNow);
    
    [NotMapped]
    public bool IsCurrentlyAvailable => IsActive && 
        (!EffectiveDate.HasValue || EffectiveDate.Value <= DateTime.UtcNow) &&
        (!ExpirationDate.HasValue || ExpirationDate.Value >= DateTime.UtcNow);
}
#endregion 
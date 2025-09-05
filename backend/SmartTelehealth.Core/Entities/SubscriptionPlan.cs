using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core subscription plan entity that defines the available subscription plans and their features.
/// This entity handles subscription plan management including pricing, features, billing cycles, and Stripe integration.
/// It serves as the template for creating user subscriptions and defines what services and privileges are available
/// to users who subscribe to each plan. The entity includes comprehensive pricing management, feature definitions,
/// and integration with Stripe for payment processing.
/// </summary>
#region Improved SubscriptionPlan Entity
public class SubscriptionPlan : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the subscription plan.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each subscription plan in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the subscription plan for display and identification purposes.
    /// Required field for subscription plan management and user interface display.
    /// Used in plan selection, billing, and subscription management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the subscription plan and its features.
    /// Used for plan information display and user education.
    /// Provides comprehensive information about what the plan includes.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Short description of the subscription plan for quick reference.
    /// Used for plan summaries, mobile displays, and quick plan identification.
    /// Provides concise information about the plan's key features.
    /// </summary>
    [MaxLength(200)]
    public string? ShortDescription { get; set; }
    
    /// <summary>
    /// Indicates whether this subscription plan is featured or highlighted.
    /// Used for marketing and promotional purposes.
    /// Featured plans are typically displayed prominently in the user interface.
    /// </summary>
    public bool IsFeatured { get; set; } = false;
    
    /// <summary>
    /// Indicates whether this subscription plan allows trial periods.
    /// Used for trial period access control and billing logic.
    /// Determines if users can start with a trial before committing to the plan.
    /// </summary>
    public bool IsTrialAllowed { get; set; } = false;
    
    /// <summary>
    /// Duration of the trial period in days for this subscription plan.
    /// Used for trial period calculations and display.
    /// Set when the plan includes a trial period option.
    /// </summary>
    public int TrialDurationInDays { get; set; } = 0;
    
    /// <summary>
    /// Indicates whether this subscription plan is marked as most popular.
    /// Used for marketing and promotional purposes.
    /// Most popular plans are typically highlighted in the user interface.
    /// </summary>
    public bool IsMostPopular { get; set; } = false;
    
    /// <summary>
    /// Indicates whether this subscription plan is currently trending.
    /// Used for marketing and promotional purposes.
    /// Trending plans are typically highlighted in the user interface.
    /// </summary>
    public bool IsTrending { get; set; } = false;
    
    /// <summary>
    /// Display order for the subscription plan in user interface.
    /// Used for controlling the order in which plans are displayed.
    /// Lower numbers are displayed first in the plan selection interface.
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Base price of the subscription plan in the specified currency.
    /// Used for billing calculations and payment processing.
    /// This is the standard price before any discounts or promotions.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Discounted price of the subscription plan if applicable.
    /// Used for promotional pricing and special offers.
    /// When set, this price is used instead of the base price.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountedPrice { get; set; }
    
    /// <summary>
    /// Date until which the discounted price is valid.
    /// Used for time-limited promotional pricing.
    /// After this date, the base price is used for billing.
    /// </summary>
    public DateTime? DiscountValidUntil { get; set; }
    
    // Foreign keys
    /// <summary>
    /// Foreign key reference to the BillingCycle that defines this plan's billing frequency.
    /// Determines how often users are billed (monthly, yearly, etc.).
    /// Required for billing cycle management and payment scheduling.
    /// </summary>
    [Required]
    public Guid BillingCycleId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Currency for this subscription plan.
    /// Determines the currency for pricing and billing.
    /// Required for international pricing and currency management.
    /// </summary>
    [Required]
    public Guid CurrencyId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Category that this subscription plan belongs to.
    /// Determines which category this plan is associated with (e.g., Mental Health, Physical Health).
    /// Required for category-based plan organization and filtering.
    /// </summary>
    public Guid? CategoryId { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Navigation property to the BillingCycle that defines this plan's billing frequency.
    /// Provides access to billing cycle details and payment scheduling.
    /// Used for billing cycle management and payment operations.
    /// </summary>
    public virtual MasterBillingCycle BillingCycle { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the Currency for this subscription plan.
    /// Provides access to currency information and exchange rates.
    /// Used for international pricing and currency management.
    /// </summary>
    public virtual MasterCurrency Currency { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the Category that this subscription plan belongs to.
    /// Provides access to category information and category-based plan management.
    /// Used for category-plan relationship operations and filtering.
    /// </summary>
    public virtual Category Category { get; set; } = null!;
    
    // Stripe Integration - Multiple price points for different billing cycles
    /// <summary>
    /// Stripe product ID for this subscription plan.
    /// Links this plan to the corresponding Stripe product.
    /// Used for Stripe integration and product management.
    /// </summary>
    [MaxLength(100)]
    public string? StripeProductId { get; set; }
    
    /// <summary>
    /// Stripe price ID for monthly billing of this subscription plan.
    /// Links this plan to the monthly price in Stripe.
    /// Used for Stripe integration and monthly billing.
    /// </summary>
    [MaxLength(100)]
    public string? StripeMonthlyPriceId { get; set; }
    
    /// <summary>
    /// Stripe price ID for quarterly billing of this subscription plan.
    /// Links this plan to the quarterly price in Stripe.
    /// Used for Stripe integration and quarterly billing.
    /// </summary>
    [MaxLength(100)]
    public string? StripeQuarterlyPriceId { get; set; }
    
    /// <summary>
    /// Stripe price ID for annual billing of this subscription plan.
    /// Links this plan to the annual price in Stripe.
    /// Used for Stripe integration and annual billing.
    /// </summary>
    [MaxLength(100)]
    public string? StripeAnnualPriceId { get; set; }
    
    // Plan features and limits
    /// <summary>
    /// Number of messages included in this subscription plan.
    /// Used for messaging limits and usage tracking.
    /// Determines how many messages users can send per billing cycle.
    /// </summary>
    public int MessagingCount { get; set; } = 10;
    
    /// <summary>
    /// Indicates whether this subscription plan includes medication delivery services.
    /// Used for service access control and feature management.
    /// Determines if users can access medication delivery features.
    /// </summary>
    public bool IncludesMedicationDelivery { get; set; } = true;
    
    /// <summary>
    /// Indicates whether this subscription plan includes follow-up care services.
    /// Used for service access control and feature management.
    /// Determines if users can access follow-up care features.
    /// </summary>
    public bool IncludesFollowUpCare { get; set; } = true;
    
    /// <summary>
    /// Frequency of medication delivery in days for this subscription plan.
    /// Used for delivery scheduling and service management.
    /// Determines how often users receive medication deliveries.
    /// </summary>
    public int DeliveryFrequencyDays { get; set; } = 30;
    
    /// <summary>
    /// Maximum duration in days that this subscription plan can be paused.
    /// Used for pause management and subscription control.
    /// Determines the maximum pause duration allowed for this plan.
    /// </summary>
    public int MaxPauseDurationDays { get; set; } = 90;
    
    // Metadata
    /// <summary>
    /// JSON string containing additional features and details for this subscription plan.
    /// Used for storing complex feature information and display data.
    /// Can include feature lists, benefits, and other plan-specific information.
    /// </summary>
    [MaxLength(1000)]
    public string? Features { get; set; }
    
    /// <summary>
    /// Terms and conditions for this subscription plan.
    /// Used for legal compliance and user agreement.
    /// Contains the terms that users agree to when subscribing to this plan.
    /// </summary>
    [MaxLength(500)]
    public string? Terms { get; set; }
    
    /// <summary>
    /// Date when this subscription plan becomes effective.
    /// Used for plan activation and availability control.
    /// Plans are not available for subscription before this date.
    /// </summary>
    public DateTime? EffectiveDate { get; set; }
    
    /// <summary>
    /// Date when this subscription plan expires and becomes unavailable.
    /// Used for plan deactivation and availability control.
    /// Plans are not available for subscription after this date.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
    
    // Alias properties for backward compatibility
   
    
    // Collection Navigation properties
    /// <summary>
    /// Collection of all privileges associated with this subscription plan.
    /// Represents the privileges and permissions available to users of this plan.
    /// Used for privilege management and access control.
    /// </summary>
    public virtual ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; } = new List<SubscriptionPlanPrivilege>();
    
    /// <summary>
    /// Collection of all subscriptions that use this subscription plan.
    /// Represents the user subscriptions based on this plan.
    /// Used for subscription management and plan usage tracking.
    /// </summary>
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    // Computed Properties
    /// <summary>
    /// Computed property that returns the effective price of the subscription plan.
    /// Returns the discounted price if available and valid, otherwise returns the base price.
    /// Used for billing calculations and price display.
    /// </summary>
    [NotMapped]
    public decimal EffectivePrice => DiscountedPrice ?? Price;
    
    /// <summary>
    /// Computed property that indicates whether the subscription plan has an active discount.
    /// Returns true if discounted price is set and discount is still valid.
    /// Used for discount management and promotional pricing.
    /// </summary>
    [NotMapped]
    public bool HasActiveDiscount => DiscountedPrice.HasValue && 
        (!DiscountValidUntil.HasValue || DiscountValidUntil.Value >= DateTime.UtcNow);
    
    /// <summary>
    /// Computed property that indicates whether the subscription plan is currently available.
    /// Returns true if plan is active and within the effective date range.
    /// Used for plan availability checking and subscription management.
    /// </summary>
    [NotMapped]
    public bool IsCurrentlyAvailable => IsActive && 
        (!EffectiveDate.HasValue || EffectiveDate.Value <= DateTime.UtcNow) &&
        (!ExpirationDate.HasValue || ExpirationDate.Value >= DateTime.UtcNow);
}
#endregion 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core category entity that manages all categories in the system.
/// This entity handles category creation, management, and configuration for healthcare services.
/// It serves as the central hub for category management, providing category creation,
/// service configuration, and pricing management capabilities.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the category.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each category in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the category.
    /// Used for category identification and display.
    /// Required for category management and user experience.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the category.
    /// Used for category documentation and user communication.
    /// Optional - used for enhanced category management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Icon identifier for UI display of this category.
    /// Used for category visualization and user experience.
    /// Optional - used for enhanced category presentation and icon support.
    /// </summary>
    [MaxLength(100)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// Color code for UI display of this category.
    /// Used for category visualization and user experience.
    /// Optional - used for enhanced category presentation and color support.
    /// </summary>
    [MaxLength(100)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Display order for sorting this category in UI.
    /// Used for category ordering and user experience.
    /// Lower values appear first in sorted lists.
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// JSON string of features available in this category.
    /// Used for category feature management and configuration.
    /// Optional - used for enhanced category feature tracking and management.
    /// </summary>
    [MaxLength(1000)]
    public string? Features { get; set; }
    
    /// <summary>
    /// Description of consultation services in this category.
    /// Used for category service documentation and user communication.
    /// Optional - used for enhanced category service management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? ConsultationDescription { get; set; }
    
    /// <summary>
    /// Base price for services in this category.
    /// Used for category pricing management and billing.
    /// Set based on category service requirements and market pricing.
    /// </summary>
    public decimal BasePrice { get; set; }
    
    /// <summary>
    /// Consultation fee for services in this category.
    /// Used for category pricing management and billing.
    /// Set based on category service requirements and market pricing.
    /// </summary>
    public decimal ConsultationFee { get; set; }
    
    /// <summary>
    /// Duration of consultations in this category in minutes.
    /// Used for category service timing management and scheduling.
    /// Defaults to 30 minutes for standard consultation duration.
    /// </summary>
    public int ConsultationDurationMinutes { get; set; } = 30;
    
    /// <summary>
    /// Indicates whether this category requires health assessment.
    /// Used for category service requirement management and validation.
    /// Defaults to true for standard health assessment requirements.
    /// </summary>
    public bool RequiresHealthAssessment { get; set; } = true;
    
    /// <summary>
    /// Indicates whether this category allows medication delivery.
    /// Used for category service capability management and validation.
    /// Defaults to true for standard medication delivery capabilities.
    /// </summary>
    public bool AllowsMedicationDelivery { get; set; } = true;
    
    /// <summary>
    /// Indicates whether this category allows follow-up messaging.
    /// Used for category service capability management and validation.
    /// Defaults to true for standard follow-up messaging capabilities.
    /// </summary>
    public bool AllowsFollowUpMessaging { get; set; } = true;
    
    /// <summary>
    /// Indicates whether this category allows one-time consultations.
    /// Used for category service capability management and validation.
    /// Defaults to true for standard one-time consultation capabilities.
    /// </summary>
    public bool AllowsOneTimeConsultation { get; set; } = true;
    
    /// <summary>
    /// Fee for one-time consultations in this category.
    /// Used for category pricing management and billing.
    /// Set based on category service requirements and market pricing.
    /// </summary>
    public decimal OneTimeConsultationFee { get; set; }
    
    /// <summary>
    /// Duration of one-time consultations in this category in minutes.
    /// Used for category service timing management and scheduling.
    /// Defaults to 30 minutes for standard one-time consultation duration.
    /// </summary>
    public int OneTimeConsultationDurationMinutes { get; set; } = 30;
    
    // Marketing and display properties
    /// <summary>
    /// Indicates whether this category is marked as most popular.
    /// Used for category marketing and user experience.
    /// Defaults to false for standard category popularity.
    /// </summary>
    public bool IsMostPopular { get; set; } = false;
    
    /// <summary>
    /// Indicates whether this category is marked as trending.
    /// Used for category marketing and user experience.
    /// Defaults to false for standard category trending status.
    /// </summary>
    public bool IsTrending { get; set; } = false;
    
    // Navigation properties
    /// <summary>
    /// Navigation property to the SubscriptionPlans that belong to this category.
    /// Provides access to subscription plan information for category management.
    /// Used for category-plan relationship operations.
    /// </summary>
    public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
    
    /// <summary>
    /// Navigation property to the ProviderCategories that belong to this category.
    /// Provides access to provider category information for category management.
    /// Used for category-provider relationship operations.
    /// </summary>
    public virtual ICollection<ProviderCategory> ProviderCategories { get; set; } = new List<ProviderCategory>();
    
    /// <summary>
    /// Navigation property to the HealthAssessments that belong to this category.
    /// Provides access to health assessment information for category management.
    /// Used for category-assessment relationship operations.
    /// </summary>
    public virtual ICollection<HealthAssessment> HealthAssessments { get; set; } = new List<HealthAssessment>();
    
    /// <summary>
    /// Navigation property to the Consultations that belong to this category.
    /// Provides access to consultation information for category management.
    /// Used for category-consultation relationship operations.
    /// </summary>
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
} 
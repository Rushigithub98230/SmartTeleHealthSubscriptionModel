using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core provider category entity that manages all provider categories in the system.
/// This entity handles provider category creation, management, and configuration for healthcare providers.
/// It serves as the central hub for provider category management, providing category creation,
/// provider configuration, and service management capabilities.
/// </summary>
public class ProviderCategory : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the provider category.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each provider category in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Provider that this category belongs to.
    /// Links this category to the specific healthcare provider.
    /// Required for provider-category relationship management.
    /// </summary>
    public int ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider that this category belongs to.
    /// Provides access to provider information for category management.
    /// Used for provider-category relationship operations.
    /// </summary>
    public virtual Provider Provider { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Category that this provider category belongs to.
    /// Links this provider category to the specific category.
    /// Required for category-provider relationship management.
    /// </summary>
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Navigation property to the Category that this provider category belongs to.
    /// Provides access to category information for provider category management.
    /// Used for category-provider relationship operations.
    /// </summary>
    public virtual Category Category { get; set; } = null!;
    
    /// <summary>
    /// Indicates whether this is the primary category for the provider.
    /// Used for provider category priority management and display.
    /// Defaults to false for standard category priority.
    /// </summary>
    public bool IsPrimary { get; set; } = false;
    
    /// <summary>
    /// Number of years of experience the provider has in this category.
    /// Used for provider experience tracking and management.
    /// Set based on provider experience and category requirements.
    /// </summary>
    public int YearsOfExperience { get; set; }
    
    /// <summary>
    /// Consultation fee for this provider in this category.
    /// Used for provider pricing management and billing.
    /// Set based on provider experience and category requirements.
    /// </summary>
    public decimal ConsultationFee { get; set; }
    
    /// <summary>
    /// Indicates whether this provider is available for this category.
    /// Used for provider availability management and scheduling.
    /// Defaults to true for standard provider availability.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
    
    /// <summary>
    /// Date and time when this provider becomes available for this category.
    /// Used for provider availability timing management and scheduling.
    /// Optional - used for time-based provider availability management.
    /// </summary>
    public DateTime? AvailableFrom { get; set; }
    
    /// <summary>
    /// Date and time when this provider becomes unavailable for this category.
    /// Used for provider availability timing management and scheduling.
    /// Optional - used for time-based provider availability management.
    /// </summary>
    public DateTime? AvailableTo { get; set; }
    
    /// <summary>
    /// Specialization or additional details about this provider in this category.
    /// Used for provider specialization documentation and management.
    /// Optional - used for enhanced provider specialization management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? Specialization { get; set; }
    
    /// <summary>
    /// Display order for sorting this provider category in UI.
    /// Used for provider category ordering and user experience.
    /// Lower values appear first in sorted lists.
    /// </summary>
    public int DisplayOrder { get; set; }
} 
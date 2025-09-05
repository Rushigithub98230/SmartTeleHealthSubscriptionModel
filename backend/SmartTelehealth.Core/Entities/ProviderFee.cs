using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core provider fee entity that manages all provider fees in the system.
/// This entity handles provider fee creation, management, and approval for healthcare providers.
/// It serves as the central hub for provider fee management, providing fee creation,
/// approval tracking, and pricing management capabilities.
/// </summary>
public class ProviderFee : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the provider fee.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each provider fee in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User (Provider) who this fee belongs to.
    /// Links this fee to the specific healthcare provider.
    /// Required for provider-fee relationship management.
    /// </summary>
    [Required]
    public int ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the User (Provider) who this fee belongs to.
    /// Provides access to provider information for fee management.
    /// Used for provider-fee relationship operations.
    /// </summary>
    public virtual User Provider { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Category that this fee belongs to.
    /// Links this fee to the specific category.
    /// Required for category-fee relationship management.
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Navigation property to the Category that this fee belongs to.
    /// Provides access to category information for fee management.
    /// Used for category-fee relationship operations.
    /// </summary>
    public virtual Category Category { get; set; } = null!;
    
    /// <summary>
    /// Fee proposed by the provider for this category.
    /// Used for provider fee proposal management and validation.
    /// Required for fee proposal enforcement and management.
    /// </summary>
    [Required]
    [Range(0, 10000)]
    public decimal ProposedFee { get; set; }
    
    /// <summary>
    /// Fee approved by administrators for this category.
    /// Used for provider fee approval management and validation.
    /// Required for fee approval enforcement and management.
    /// </summary>
    [Required]
    [Range(0, 10000)]
    public decimal ApprovedFee { get; set; }
    
    /// <summary>
    /// Current status of the provider fee.
    /// Used for fee status tracking and management.
    /// Defaults to Pending when fee is created.
    /// </summary>
    public FeeStatus Status { get; set; } = FeeStatus.Pending;
    
    /// <summary>
    /// Administrative remarks about this provider fee.
    /// Used for fee documentation and management.
    /// Optional - used for enhanced fee management and documentation.
    /// </summary>
    [MaxLength(1000)]
    public string? AdminRemarks { get; set; }
    
    /// <summary>
    /// Notes from the provider about this fee.
    /// Used for fee documentation and management.
    /// Optional - used for enhanced fee management and documentation.
    /// </summary>
    [MaxLength(1000)]
    public string? ProviderNotes { get; set; }
    
    /// <summary>
    /// Date and time when the fee was proposed by the provider.
    /// Used for fee proposal timing tracking and management.
    /// Set when the fee is proposed by the provider.
    /// </summary>
    public DateTime? ProposedAt { get; set; }
    
    /// <summary>
    /// Date and time when the fee was reviewed by administrators.
    /// Used for fee review timing tracking and management.
    /// Set when the fee is reviewed by administrators.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who reviewed this fee.
    /// Links this fee to the specific administrator who reviewed it.
    /// Optional - used for reviewer tracking and management.
    /// </summary>
    public int? ReviewedByUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who reviewed this fee.
    /// Provides access to reviewer information for fee management.
    /// Used for reviewer-fee relationship operations.
    /// </summary>
    public virtual User? ReviewedByUser { get; set; }
    
    // Alias properties for backward compatibility
    /// <summary>
    /// Alias property for CreatedDate from BaseEntity.
    /// Used for backward compatibility and legacy system integration.
    /// </summary>
    public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
    
    /// <summary>
    /// Alias property for UpdatedDate from BaseEntity.
    /// Used for backward compatibility and legacy system integration.
    /// </summary>
    public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
}

/// <summary>
/// Enumeration defining the possible statuses of provider fees.
/// Used for fee status tracking and management.
/// </summary>
public enum FeeStatus
{
    /// <summary>Fee is pending review by administrators.</summary>
    Pending = 0,
    /// <summary>Fee is under administrative review.</summary>
    UnderReview = 1,
    /// <summary>Fee has been approved by administrators.</summary>
    Approved = 2,
    /// <summary>Fee has been rejected by administrators.</summary>
    Rejected = 3,
    /// <summary>Fee requires additional information from the provider.</summary>
    RequiresMoreInfo = 4
} 
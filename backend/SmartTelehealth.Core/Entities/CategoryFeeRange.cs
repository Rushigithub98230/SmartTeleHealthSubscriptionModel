using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core category fee range entity that manages all category fee ranges in the system.
/// This entity handles category fee range creation, management, and configuration for healthcare categories.
/// It serves as the central hub for category fee range management, providing fee range creation,
/// commission management, and pricing configuration capabilities.
/// </summary>
public class CategoryFeeRange : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the category fee range.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each category fee range in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Category that this fee range belongs to.
    /// Links this fee range to the specific category.
    /// Required for category-fee range relationship management.
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Navigation property to the Category that this fee range belongs to.
    /// Provides access to category information for fee range management.
    /// Used for category-fee range relationship operations.
    /// </summary>
    public virtual Category Category { get; set; } = null!;
    
    /// <summary>
    /// Minimum fee allowed for this category.
    /// Used for category fee range management and validation.
    /// Required for fee range enforcement and management.
    /// </summary>
    [Required]
    [Range(0, 10000)]
    public decimal MinimumFee { get; set; }
    
    /// <summary>
    /// Maximum fee allowed for this category.
    /// Used for category fee range management and validation.
    /// Required for fee range enforcement and management.
    /// </summary>
    [Required]
    [Range(0, 10000)]
    public decimal MaximumFee { get; set; }
    
    /// <summary>
    /// Platform commission percentage for this category.
    /// Used for category commission management and billing.
    /// Required for commission calculation and management.
    /// </summary>
    [Required]
    [Range(0, 100)]
    public decimal PlatformCommission { get; set; }
    
    /// <summary>
    /// Description of this category fee range.
    /// Used for fee range documentation and user communication.
    /// Optional - used for enhanced fee range management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
} 
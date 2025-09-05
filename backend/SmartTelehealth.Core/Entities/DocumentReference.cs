using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Entity for managing document references and associations.
/// This entity handles document linking, reference management, and access control for documents.
/// It serves as the central hub for document reference management, providing document linking,
/// reference tracking, and access control capabilities.
/// </summary>
public class DocumentReference : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the document reference.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each document reference in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Document that this reference points to.
    /// Links this reference to the specific document.
    /// Required for document-reference relationship management.
    /// </summary>
    public Guid DocumentId { get; set; }
    
    /// <summary>
    /// Navigation property to the Document that this reference points to.
    /// Provides access to document information for reference management.
    /// Used for document-reference relationship operations.
    /// </summary>
    public virtual Document Document { get; set; } = null!;
    
    /// <summary>
    /// Type of entity that this document reference belongs to.
    /// Used for document reference classification and management.
    /// Examples: "Appointment", "User", "Chat", "Consultation".
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the entity that this document reference belongs to.
    /// Used for document reference linking and management.
    /// Links the document to the specific entity instance.
    /// </summary>
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Type of reference or purpose of this document reference.
    /// Used for document reference classification and management.
    /// Examples: "profile_picture", "medical_report", "chat_attachment".
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceType { get; set; }
    
    /// <summary>
    /// Description or notes about this document reference.
    /// Used for document reference documentation and management.
    /// Optional - used for enhanced reference management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Indicates whether this document reference is publicly accessible.
    /// Used for document access control and security management.
    /// Defaults to false for security and privacy protection.
    /// </summary>
    public bool IsPublic { get; set; } = false;
    
    /// <summary>
    /// Date and time when this document reference expires.
    /// Used for document reference lifecycle management and access control.
    /// Optional - used for time-limited document access and management.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
} 
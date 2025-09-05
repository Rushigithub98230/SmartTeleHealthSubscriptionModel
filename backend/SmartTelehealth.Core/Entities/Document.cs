using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core document entity that manages all documents in the system.
/// This entity handles document storage, metadata, security, and access control.
/// It serves as the central hub for document management, integrating with document types,
/// references, and various system entities. The entity includes comprehensive document
/// tracking, security management, and file storage capabilities.
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the document.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each document in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    // File information
    /// <summary>
    /// Original filename of the uploaded document.
    /// Used for document identification and user interface display.
    /// Required for document management and user experience.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string OriginalName { get; set; } = string.Empty; // e.g., "testdocument.pdf"
    
    /// <summary>
    /// Unique filename generated for this document to prevent conflicts.
    /// Used for document storage and conflict prevention.
    /// Required for document management and file system operations.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string UniqueName { get; set; } = string.Empty; // e.g., "a1b2c3d4-e5f6-7890-abcd-ef1234567890_testdocument.pdf"
    
    /// <summary>
    /// Complete file path where the document is stored in the system.
    /// Used for document storage and access management.
    /// Required for document retrieval and file operations.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty; // Full path including folder
    
    /// <summary>
    /// Folder path where the document is stored for organization.
    /// Used for document organization and folder management.
    /// Required for document organization and file system structure.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string FolderPath { get; set; } = string.Empty; // e.g., "appointments/123"
    
    /// <summary>
    /// MIME type or content type of the document.
    /// Used for document type identification and processing.
    /// Required for document handling and content type management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty; // e.g., "application/pdf"
    
    /// <summary>
    /// Size of the document file in bytes.
    /// Used for storage management and file size validation.
    /// Helps with storage quota management and file size limits.
    /// </summary>
    public long FileSize { get; set; }
    
    // Document metadata
    /// <summary>
    /// Description or notes about the document.
    /// Used for document context and additional information.
    /// Can include document purpose, content summary, or special notes.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    // Document Type relationship
    /// <summary>
    /// Foreign key reference to the DocumentType that defines this document's type.
    /// Links this document to the specific document type (Report, Image, Prescription, etc.).
    /// Required for document type management and categorization.
    /// </summary>
    public Guid DocumentTypeId { get; set; }
    
    /// <summary>
    /// Navigation property to the DocumentType that defines this document's type.
    /// Provides access to document type information for document management.
    /// Used for document type management and categorization.
    /// </summary>
    public virtual DocumentType DocumentType { get; set; } = null!;
    
    /// <summary>
    /// Document category for backward compatibility and legacy support.
    /// Used for document categorization and legacy system integration.
    /// Examples: "appointment", "profile", "chat".
    /// </summary>
    [MaxLength(50)]
    public string? DocumentCategory { get; set; } // e.g., "appointment", "profile", "chat" - for backward compatibility
    
    // Security and access
    /// <summary>
    /// Indicates whether this document content is encrypted.
    /// Used for document security and privacy protection.
    /// Defaults to false for standard document handling.
    /// </summary>
    public bool IsEncrypted { get; set; } = false;
    
    /// <summary>
    /// Encryption key used for this document.
    /// Used for document encryption and decryption.
    /// Set when document is encrypted for security.
    /// </summary>
    [MaxLength(100)]
    public string? EncryptionKey { get; set; }
    
    /// <summary>
    /// Indicates whether this document can be accessed without authentication.
    /// Used for document access control and security management.
    /// Defaults to false for secure document handling.
    /// </summary>
    public bool IsPublic { get; set; } = false; // Can be accessed without authentication
    
    // Navigation properties
    /// <summary>
    /// Collection of references to this document from other entities.
    /// Used for document reference management and relationship tracking.
    /// Includes all references to this document from appointments, consultations, etc.
    /// </summary>
    public virtual ICollection<DocumentReference> References { get; set; } = new List<DocumentReference>();
} 
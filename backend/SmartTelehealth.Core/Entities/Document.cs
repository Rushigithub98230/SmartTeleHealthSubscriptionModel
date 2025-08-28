using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class Document : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // File information
    [Required]
    [MaxLength(255)]
    public string OriginalName { get; set; } = string.Empty; // e.g., "testdocument.pdf"
    
    [Required]
    [MaxLength(255)]
    public string UniqueName { get; set; } = string.Empty; // e.g., "a1b2c3d4-e5f6-7890-abcd-ef1234567890_testdocument.pdf"
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty; // Full path including folder
    
    [Required]
    [MaxLength(200)]
    public string FolderPath { get; set; } = string.Empty; // e.g., "appointments/123"
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty; // e.g., "application/pdf"
    
    public long FileSize { get; set; }
    
    // Document metadata
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    // Document Type relationship
    public Guid DocumentTypeId { get; set; }
    public virtual DocumentType DocumentType { get; set; } = null!;
    
    [MaxLength(50)]
    public string? DocumentCategory { get; set; } // e.g., "appointment", "profile", "chat" - for backward compatibility
    
    // Security and access
    public bool IsEncrypted { get; set; } = false;
    
    [MaxLength(100)]
    public string? EncryptionKey { get; set; }
    
    public bool IsPublic { get; set; } = false; // Can be accessed without authentication
    
    // Navigation properties
    public virtual ICollection<DocumentReference> References { get; set; } = new List<DocumentReference>();
} 
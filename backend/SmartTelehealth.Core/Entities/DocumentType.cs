using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Entity for managing document types and file validation rules.
/// This entity handles document type definitions, file validation, and UI/UX properties for documents.
/// It serves as the central hub for document type management, providing type classification,
/// file validation, and display customization capabilities.
/// </summary>
public class DocumentType : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the document type.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each document type in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of the document type.
    /// Used for document type identification and display.
    /// Examples: "Prescription", "Blood Report", "License", "Invoice".
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description or additional context about this document type.
    /// Used for document type documentation and management.
    /// Optional - used for enhanced type management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Indicates whether this document type is system-defined or admin-created.
    /// Used for document type management and access control.
    /// System-defined types cannot be modified by administrators.
    /// </summary>
    public bool IsSystemDefined { get; set; } = false;
    
    /// <summary>
    /// Comma-separated list of allowed file extensions for this document type.
    /// Used for file validation and type enforcement.
    /// Examples: ".pdf,.jpg,.png", ".docx,.pdf", ".jpg,.jpeg,.png".
    /// </summary>
    [MaxLength(1000)]
    public string? AllowedExtensions { get; set; }
    
    /// <summary>
    /// Maximum file size in bytes allowed for this document type.
    /// Used for file validation and size enforcement.
    /// Examples: 5242880 (5MB), 10485760 (10MB), 20971520 (20MB).
    /// </summary>
    public long? MaxFileSizeBytes { get; set; }
    
    /// <summary>
    /// Indicates whether files should be validated against the defined rules.
    /// Used for file validation control and management.
    /// When false, files are accepted without validation.
    /// </summary>
    public bool RequireFileValidation { get; set; } = true;
    
    /// <summary>
    /// Icon identifier for UI display of this document type.
    /// Used for document type visualization and user experience.
    /// Examples: "file-pdf", "file-image", "file-document".
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// Color code for UI display of this document type.
    /// Used for document type visualization and user experience.
    /// Examples: "#FF5733", "#33FF57", "#3357FF".
    /// </summary>
    [MaxLength(20)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Display order for sorting this document type in UI.
    /// Used for document type ordering and user experience.
    /// Lower values appear first in sorted lists.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Number of documents currently using this document type.
    /// Used for usage tracking and type management.
    /// Incremented when documents are created with this type.
    /// </summary>
    public int UsageCount { get; set; } = 0;
    
    /// <summary>
    /// Date and time when this document type was last used.
    /// Used for usage tracking and type management.
    /// Updated when documents are created with this type.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Navigation property to the Documents that use this type.
    /// Provides access to document information for type management.
    /// Used for document-type relationship operations.
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    
    /// <summary>
    /// Validates whether a file extension is allowed for this document type.
    /// Used for file validation and type enforcement.
    /// Returns true if the file extension is allowed or validation is disabled.
    /// </summary>
    /// <param name="fileName">The name of the file to validate</param>
    /// <returns>True if the file extension is valid, false otherwise</returns>
    public bool IsValidFileExtension(string fileName)
    {
        if (string.IsNullOrEmpty(AllowedExtensions) || !RequireFileValidation)
            return true;
            
        var fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension))
            return false;
            
        var allowedExtensions = AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim().ToLowerInvariant())
            .ToList();
            
        return allowedExtensions.Contains(fileExtension);
    }
    
    /// <summary>
    /// Validates whether a file size is within the allowed limit for this document type.
    /// Used for file validation and size enforcement.
    /// Returns true if the file size is within limits or validation is disabled.
    /// </summary>
    /// <param name="fileSizeBytes">The size of the file in bytes</param>
    /// <returns>True if the file size is valid, false otherwise</returns>
    public bool IsValidFileSize(long fileSizeBytes)
    {
        if (!MaxFileSizeBytes.HasValue || !RequireFileValidation)
            return true;
            
        return fileSizeBytes <= MaxFileSizeBytes.Value;
    }
    
    /// <summary>
    /// Gets a human-readable display string for the maximum file size.
    /// Used for UI display and user communication.
    /// Returns formatted size string (e.g., "5 MB", "10 KB", "No limit").
    /// </summary>
    /// <returns>Formatted file size string</returns>
    public string GetMaxFileSizeDisplay()
    {
        if (!MaxFileSizeBytes.HasValue)
            return "No limit";
            
        var bytes = MaxFileSizeBytes.Value;
        if (bytes < 1024)
            return $"{bytes} B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024} KB";
        if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024 * 1024)} MB";
        return $"{bytes / (1024 * 1024 * 1024)} GB";
    }
    
    /// <summary>
    /// Gets a list of allowed file extensions for this document type.
    /// Used for file validation and type enforcement.
    /// Returns a list of allowed extensions without leading dots.
    /// </summary>
    /// <returns>List of allowed file extensions</returns>
    public List<string> GetAllowedExtensionsList()
    {
        if (string.IsNullOrEmpty(AllowedExtensions))
            return new List<string>();
            
        return AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim())
            .ToList();
    }
} 
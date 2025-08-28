namespace SmartTelehealth.Application.DTOs;

public class DocumentDto
{
    public Guid DocumentId { get; set; }
    public string OriginalName { get; set; } = string.Empty;
    public string UniqueName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    
    // Document Type information
    public Guid DocumentTypeId { get; set; }
    public DocumentTypeDto? DocumentType { get; set; }
    public string? DocumentCategory { get; set; } // For backward compatibility
    
    public bool IsEncrypted { get; set; }
    public bool IsPublic { get; set; }
    public int? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    
    // File content (for download operations)
    public byte[]? Content { get; set; }
    
    // URLs
    public string? DownloadUrl { get; set; }
    public string? SecureUrl { get; set; }
    
    // References
    public List<DocumentReferenceDto> References { get; set; } = new List<DocumentReferenceDto>();
}

public class DocumentReferenceDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadDocumentRequest
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsEncrypted { get; set; } = false;
    public int? CreatedById { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    // Document Type information
    public Guid DocumentTypeId { get; set; } // Required - user must select document type
}

public class DocumentSearchRequest
{
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? ReferenceType { get; set; }
    public string? DocumentCategory { get; set; } // For backward compatibility
    
    // Document Type filters
    public Guid? DocumentTypeId { get; set; }
    public string? DocumentTypeName { get; set; }
    
    public bool? IsPublic { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
} 

public class UploadUserDocumentRequest
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int UserId { get; set; } // User ID (int)
    public string? ReferenceType { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsEncrypted { get; set; } = false;
    public int? CreatedById { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    // Document Type information
    public Guid DocumentTypeId { get; set; } // Required - user must select document type
} 
namespace SmartTelehealth.Application.DTOs;

public class DocumentTypeDto
{
    public Guid DocumentTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // System vs Admin defined
    public bool IsSystemDefined { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    
    // File validation rules
    public string? AllowedExtensions { get; set; }
    public long? MaxFileSizeBytes { get; set; }
    public bool RequireFileValidation { get; set; }
    
    // UI/UX properties
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    
    // Usage tracking
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsedAt { get; set; }
    
    // Audit fields
    public int? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Additional metadata
    public int DocumentCount { get; set; } = 0; // Number of documents of this type
    public string MaxFileSizeDisplay { get; set; } = "No limit";
    public List<string> AllowedExtensionsList { get; set; } = new List<string>();
}

public class CreateDocumentTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystemDefined { get; set; } = false;
    
    // File validation rules
    public string? AllowedExtensions { get; set; } // Comma-separated: ".pdf,.jpg,.png"
    public long? MaxFileSizeBytes { get; set; } // Maximum file size in bytes
    public bool RequireFileValidation { get; set; } = true;
    
    // UI/UX properties
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; } = 0;
    
    public Guid CreatedById { get; set; }
}

public class UpdateDocumentTypeRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    
    // File validation rules (only for non-system types)
    public string? AllowedExtensions { get; set; }
    public long? MaxFileSizeBytes { get; set; }
    public bool? RequireFileValidation { get; set; }
    
    // UI/UX properties
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int? DisplayOrder { get; set; }
    
    public Guid UpdatedById { get; set; }
}

public class DocumentTypeSearchRequest
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsSystemDefined { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class DocumentTypeValidationRequest
{
    public Guid DocumentTypeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}

public class DocumentTypeValidationResponse
{
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new List<string>();
    public string? MaxFileSizeDisplay { get; set; }
    public List<string> AllowedExtensions { get; set; } = new List<string>();
} 
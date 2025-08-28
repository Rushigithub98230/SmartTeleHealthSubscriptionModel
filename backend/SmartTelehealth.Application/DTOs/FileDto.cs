using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class FileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsPublic { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
}

public class CreateFileDto
{
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = false;
    public string? Tags { get; set; }
}

public class UpdateFileDto
{
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
    public string? Tags { get; set; }
} 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class MessageAttachment : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Foreign key
    public Guid MessageId { get; set; }
    public virtual Message Message { get; set; } = null!;
    
    // Attachment details
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? ContentType { get; set; }
    
    public bool IsImage { get; set; } = false;
    
    public bool IsDocument { get; set; } = false;
    
    public bool IsVideo { get; set; } = false;
    
    public bool IsAudio { get; set; } = false;
} 
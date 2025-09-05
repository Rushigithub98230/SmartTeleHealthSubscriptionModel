using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Entity for managing message attachments and file uploads.
/// This entity handles attachment storage, metadata, and file type classification for messages.
/// It serves as the central hub for message attachment management, providing file storage,
/// type detection, and attachment metadata capabilities.
/// </summary>
public class MessageAttachment : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the message attachment.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each message attachment in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Message that this attachment belongs to.
    /// Links this attachment to the specific message.
    /// Required for message-attachment relationship management.
    /// </summary>
    public Guid MessageId { get; set; }
    
    /// <summary>
    /// Navigation property to the Message that this attachment belongs to.
    /// Provides access to message information for attachment management.
    /// Used for message-attachment relationship operations.
    /// </summary>
    public virtual Message Message { get; set; } = null!;
    
    /// <summary>
    /// Original name of the attached file.
    /// Used for file identification and display purposes.
    /// Required for attachment management and user experience.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of the attached file (e.g., pdf, jpg, docx).
    /// Used for file type classification and processing.
    /// Required for attachment management and file handling.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty;
    
    /// <summary>
    /// Size of the attached file in bytes.
    /// Used for file size tracking and storage management.
    /// Required for attachment management and storage optimization.
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// URL or path to the attached file.
    /// Used for file access and retrieval.
    /// Required for attachment management and file serving.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME content type of the attached file.
    /// Used for file type identification and browser compatibility.
    /// Optional - used for enhanced file handling and display.
    /// </summary>
    [MaxLength(100)]
    public string? ContentType { get; set; }
    
    /// <summary>
    /// Indicates whether the attached file is an image.
    /// Used for file type classification and display optimization.
    /// Set based on file type detection for enhanced user experience.
    /// </summary>
    public bool IsImage { get; set; } = false;
    
    /// <summary>
    /// Indicates whether the attached file is a document.
    /// Used for file type classification and processing.
    /// Set based on file type detection for enhanced file handling.
    /// </summary>
    public bool IsDocument { get; set; } = false;
    
    /// <summary>
    /// Indicates whether the attached file is a video.
    /// Used for file type classification and media handling.
    /// Set based on file type detection for enhanced media processing.
    /// </summary>
    public bool IsVideo { get; set; } = false;
    
    /// <summary>
    /// Indicates whether the attached file is an audio file.
    /// Used for file type classification and media handling.
    /// Set based on file type detection for enhanced media processing.
    /// </summary>
    public bool IsAudio { get; set; } = false;
} 
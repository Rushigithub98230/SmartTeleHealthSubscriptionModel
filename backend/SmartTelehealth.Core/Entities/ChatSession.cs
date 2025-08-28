using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class ChatSession : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum ChatStatus
    {
        Active,
        Ended,
        Timeout,
        Cancelled
    }

    // Foreign keys
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }

    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;

    // Session details
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }

    public ChatStatus Status { get; set; } = ChatStatus.Active;

    // Usage tracking
    public int MessageCount { get; set; } = 0;
    public bool HasFileSharing { get; set; } = false;
    public bool HasVideoChat { get; set; } = false;

    // Session metadata
    [MaxLength(500)]
    public string? SessionNotes { get; set; }

    [MaxLength(100)]
    public string? SessionType { get; set; } // "Urgent", "FollowUp", "General"

    public bool IsPriority { get; set; } = false;

    // Navigation properties
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<ChatAttachment> Attachments { get; set; } = new List<ChatAttachment>();

    // Computed properties
    [NotMapped]
    public bool IsChatActive => Status == ChatStatus.Active;

    [NotMapped]
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.UtcNow - StartTime;

    [NotMapped]
    public bool HasEnded => Status == ChatStatus.Ended || Status == ChatStatus.Timeout || Status == ChatStatus.Cancelled;
}

public class ChatMessage : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }
    public virtual ChatSession Session { get; set; } = null!;

    public int SenderId { get; set; }
    public string SenderType { get; set; } = string.Empty; // "User", "Provider"

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    [MaxLength(50)]
    public string? MessageType { get; set; } // "Text", "Image", "File", "System"

    public bool IsSystemMessage { get; set; } = false;
}

public class ChatAttachment : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }
    public virtual ChatSession Session { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }

    public Guid UploadedBy { get; set; }
} 
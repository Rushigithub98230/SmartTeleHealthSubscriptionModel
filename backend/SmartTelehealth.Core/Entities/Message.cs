using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class Message : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum MessageType
    {
        Text,
        Image,
        Video,
        Document,
        Audio,
        System
    }

    public enum MessageStatus
    {
        Sent,
        Delivered,
        Read,
        Failed
    }

    // Foreign keys
    public int SenderId { get; set; }
    public virtual User Sender { get; set; } = null!;

    public Guid ChatRoomId { get; set; }
    public virtual ChatRoom ChatRoom { get; set; } = null!;

    public Guid? ReplyToMessageId { get; set; }
    public virtual Message? ReplyToMessage { get; set; }

    // Message content
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    // File attachment information
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }

    // Metadata
    public DateTime? ReadAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Encryption
    public bool IsEncrypted { get; set; } = true;
    public string? EncryptionKey { get; set; }

    // Navigation properties
    public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
    public virtual ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    public virtual ICollection<MessageReadReceipt> ReadReceipts { get; set; } = new List<MessageReadReceipt>();
    public virtual ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
} 
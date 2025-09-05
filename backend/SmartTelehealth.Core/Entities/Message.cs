using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core message entity that manages all communication messages in the system.
/// This entity handles message content, delivery tracking, and communication management.
/// It serves as the central hub for messaging functionality, integrating with users, chat rooms,
/// consultations, and file attachments. The entity includes comprehensive message tracking,
/// delivery status management, and multimedia support capabilities.
/// </summary>
public class Message : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the message.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each message in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible types of messages.
    /// Used for message type management and content handling.
    /// </summary>
    public enum MessageType
    {
        /// <summary>Text-based message content.</summary>
        Text,
        /// <summary>Image file attachment message.</summary>
        Image,
        /// <summary>Video file attachment message.</summary>
        Video,
        /// <summary>Document file attachment message.</summary>
        Document,
        /// <summary>Audio file attachment message.</summary>
        Audio,
        /// <summary>System-generated message.</summary>
        System
    }

    /// <summary>
    /// Enumeration defining the possible statuses of messages.
    /// Used for message delivery tracking and status management.
    /// </summary>
    public enum MessageStatus
    {
        /// <summary>Message has been sent but not yet delivered.</summary>
        Sent,
        /// <summary>Message has been delivered to the recipient.</summary>
        Delivered,
        /// <summary>Message has been read by the recipient.</summary>
        Read,
        /// <summary>Message delivery failed.</summary>
        Failed
    }

    // Foreign keys
    /// <summary>
    /// Foreign key reference to the User who sent this message.
    /// Links this message to the specific user who sent it.
    /// Required for message sender identification and user management.
    /// </summary>
    public int SenderId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who sent this message.
    /// Provides access to sender information for message management.
    /// Used for sender identification and user operations.
    /// </summary>
    public virtual User Sender { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the ChatRoom that this message belongs to.
    /// Links this message to the specific chat room.
    /// Required for message-room relationship management.
    /// </summary>
    public Guid ChatRoomId { get; set; }
    
    /// <summary>
    /// Navigation property to the ChatRoom that this message belongs to.
    /// Provides access to chat room information for message management.
    /// Used for chat room operations and message organization.
    /// </summary>
    public virtual ChatRoom ChatRoom { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the Message that this message is replying to.
    /// Links this message to the specific message being replied to.
    /// Optional - used for message threading and conversation management.
    /// </summary>
    public Guid? ReplyToMessageId { get; set; }
    
    /// <summary>
    /// Navigation property to the Message that this message is replying to.
    /// Provides access to the original message for reply management.
    /// Used for message threading and conversation operations.
    /// </summary>
    public virtual Message? ReplyToMessage { get; set; }

    // Message content
    /// <summary>
    /// The actual content of this message.
    /// Used for message content storage and display.
    /// Required for message creation and content management.
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Type of this message (Text, Image, Video, Document, Audio, System).
    /// Used for message type management and content handling.
    /// Defaults to Text when message is first created.
    /// </summary>
    public MessageType Type { get; set; } = MessageType.Text;

    /// <summary>
    /// Current status of this message (Sent, Delivered, Read, Failed).
    /// Used for message delivery tracking and status management.
    /// Defaults to Sent when message is first created.
    /// </summary>
    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    // File attachment information
    /// <summary>
    /// Original filename of the attached file.
    /// Used for file attachment identification and user interface display.
    /// Set when message includes a file attachment.
    /// </summary>
    public string? FileName { get; set; }
    
    /// <summary>
    /// File path where the attached file is stored.
    /// Used for file attachment storage and access management.
    /// Set when message includes a file attachment.
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// MIME type or file extension of the attached file.
    /// Used for file type identification and processing.
    /// Set when message includes a file attachment.
    /// </summary>
    public string? FileType { get; set; }
    
    /// <summary>
    /// Size of the attached file in bytes.
    /// Used for file size validation and storage management.
    /// Set when message includes a file attachment.
    /// </summary>
    public long? FileSize { get; set; }

    // Metadata
    /// <summary>
    /// Date and time when this message was read by the recipient.
    /// Used for message read tracking and delivery management.
    /// Set when message is read by the recipient.
    /// </summary>
    public DateTime? ReadAt { get; set; }
    
    /// <summary>
    /// Date and time when this message was delivered to the recipient.
    /// Used for message delivery tracking and status management.
    /// Set when message is successfully delivered.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    // Encryption
    /// <summary>
    /// Indicates whether this message content is encrypted.
    /// Used for message security and privacy protection.
    /// Defaults to true for secure message handling.
    /// </summary>
    public bool IsEncrypted { get; set; } = true;
    
    /// <summary>
    /// Encryption key used for this message.
    /// Used for message encryption and decryption.
    /// Set when message is encrypted for security.
    /// </summary>
    public string? EncryptionKey { get; set; }

    // Navigation properties
    /// <summary>
    /// Collection of messages that are replies to this message.
    /// Used for message threading and conversation management.
    /// Includes all messages that reply to this message.
    /// </summary>
    public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
    
    /// <summary>
    /// Collection of reactions to this message.
    /// Used for message interaction and user engagement tracking.
    /// Includes all reactions (likes, emojis, etc.) for this message.
    /// </summary>
    public virtual ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    
    /// <summary>
    /// Collection of read receipts for this message.
    /// Used for message read tracking and delivery confirmation.
    /// Includes read receipts from all recipients of this message.
    /// </summary>
    public virtual ICollection<MessageReadReceipt> ReadReceipts { get; set; } = new List<MessageReadReceipt>();
    
    /// <summary>
    /// Collection of attachments for this message.
    /// Used for file attachment management and message content.
    /// Includes all file attachments associated with this message.
    /// </summary>
    public virtual ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
} 
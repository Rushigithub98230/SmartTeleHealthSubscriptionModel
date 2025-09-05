using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core chat session entity that manages all chat sessions in the system.
/// This entity handles chat session creation, management, and tracking for real-time communication.
/// It serves as the central hub for chat session management, providing session creation,
/// message tracking, and communication capabilities.
/// </summary>
public class ChatSession : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the chat session.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each chat session in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible statuses of chat sessions.
    /// Used for session status tracking and management.
    /// </summary>
    public enum ChatStatus
    {
        /// <summary>Chat session is active and ongoing.</summary>
        Active,
        /// <summary>Chat session has ended normally.</summary>
        Ended,
        /// <summary>Chat session has timed out.</summary>
        Timeout,
        /// <summary>Chat session has been cancelled.</summary>
        Cancelled
    }

    /// <summary>
    /// Foreign key reference to the User who is participating in this chat session.
    /// Links this session to the specific user account.
    /// Required for user-session relationship management.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is participating in this chat session.
    /// Provides access to user information for session management.
    /// Used for user-session relationship operations.
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the Provider who is participating in this chat session.
    /// Links this session to the specific healthcare provider.
    /// Optional - used for provider-session relationship management.
    /// </summary>
    public int? ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who is participating in this chat session.
    /// Provides access to provider information for session management.
    /// Used for provider-session relationship operations.
    /// </summary>
    public virtual Provider? Provider { get; set; }

    /// <summary>
    /// Foreign key reference to the Subscription that this chat session belongs to.
    /// Links this session to the specific subscription.
    /// Required for subscription-session relationship management.
    /// </summary>
    public Guid SubscriptionId { get; set; }
    
    /// <summary>
    /// Navigation property to the Subscription that this chat session belongs to.
    /// Provides access to subscription information for session management.
    /// Used for subscription-session relationship operations.
    /// </summary>
    public virtual Subscription Subscription { get; set; } = null!;

    /// <summary>
    /// Date and time when the chat session started.
    /// Used for session timing tracking and management.
    /// Set when the chat session is created.
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Date and time when the chat session ended.
    /// Used for session timing tracking and management.
    /// Set when the chat session is ended.
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Duration of the chat session in minutes.
    /// Used for session duration tracking and management.
    /// Calculated based on start and end times.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Current status of the chat session.
    /// Used for session status tracking and management.
    /// Defaults to Active when session is created.
    /// </summary>
    public ChatStatus Status { get; set; } = ChatStatus.Active;

    /// <summary>
    /// Number of messages exchanged in this chat session.
    /// Used for session usage tracking and management.
    /// Incremented when messages are sent in the session.
    /// </summary>
    public int MessageCount { get; set; } = 0;
    
    /// <summary>
    /// Indicates whether file sharing was used in this chat session.
    /// Used for session feature tracking and management.
    /// Set to true when files are shared in the session.
    /// </summary>
    public bool HasFileSharing { get; set; } = false;
    
    /// <summary>
    /// Indicates whether video chat was used in this chat session.
    /// Used for session feature tracking and management.
    /// Set to true when video chat is used in the session.
    /// </summary>
    public bool HasVideoChat { get; set; } = false;

    /// <summary>
    /// Notes or additional information about this chat session.
    /// Used for session documentation and management.
    /// Optional - used for enhanced session management and documentation.
    /// </summary>
    [MaxLength(500)]
    public string? SessionNotes { get; set; }

    /// <summary>
    /// Type of chat session (e.g., "Urgent", "FollowUp", "General").
    /// Used for session classification and management.
    /// Optional - used for enhanced session management and classification.
    /// </summary>
    [MaxLength(100)]
    public string? SessionType { get; set; }

    /// <summary>
    /// Indicates whether this chat session has priority status.
    /// Used for session priority management and user experience.
    /// Defaults to false for standard session priority.
    /// </summary>
    public bool IsPriority { get; set; } = false;

    // Navigation properties
    /// <summary>
    /// Navigation property to the ChatMessages that belong to this session.
    /// Provides access to message information for session management.
    /// Used for session-message relationship operations.
    /// </summary>
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    
    /// <summary>
    /// Navigation property to the ChatAttachments that belong to this session.
    /// Provides access to attachment information for session management.
    /// Used for session-attachment relationship operations.
    /// </summary>
    public virtual ICollection<ChatAttachment> Attachments { get; set; } = new List<ChatAttachment>();

    // Computed properties
    /// <summary>
    /// Indicates whether this chat session is currently active.
    /// Used for session status checking and validation.
    /// Returns true if Status is Active.
    /// </summary>
    [NotMapped]
    public bool IsChatActive => Status == ChatStatus.Active;

    /// <summary>
    /// Duration of the chat session as a TimeSpan.
    /// Used for session duration calculation and management.
    /// Returns the time span between start and end times, or current time if still active.
    /// </summary>
    [NotMapped]
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.UtcNow - StartTime;

    /// <summary>
    /// Indicates whether this chat session has ended.
    /// Used for session status checking and validation.
    /// Returns true if Status is Ended, Timeout, or Cancelled.
    /// </summary>
    [NotMapped]
    public bool HasEnded => Status == ChatStatus.Ended || Status == ChatStatus.Timeout || Status == ChatStatus.Cancelled;
}

/// <summary>
/// Core chat message entity that manages all chat messages in the system.
/// This entity handles chat message creation, management, and tracking for real-time communication.
/// It serves as the central hub for chat message management, providing message creation,
/// content tracking, and communication capabilities.
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the chat message.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each chat message in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the ChatSession that this message belongs to.
    /// Links this message to the specific chat session.
    /// Required for session-message relationship management.
    /// </summary>
    public Guid SessionId { get; set; }
    
    /// <summary>
    /// Navigation property to the ChatSession that this message belongs to.
    /// Provides access to session information for message management.
    /// Used for session-message relationship operations.
    /// </summary>
    public virtual ChatSession Session { get; set; } = null!;

    /// <summary>
    /// ID of the user who sent this message.
    /// Used for message sender tracking and management.
    /// Required for message sender identification.
    /// </summary>
    public int SenderId { get; set; }
    
    /// <summary>
    /// Type of sender who sent this message (e.g., "User", "Provider").
    /// Used for message sender classification and management.
    /// Required for message sender type identification.
    /// </summary>
    public string SenderType { get; set; } = string.Empty;

    /// <summary>
    /// Content of the chat message.
    /// Used for message content display and user communication.
    /// Required for message management and user experience.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the message was sent.
    /// Used for message timing tracking and management.
    /// Set when the message is sent.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Indicates whether this message has been read by the recipient.
    /// Used for message read status tracking and management.
    /// Defaults to false when message is sent.
    /// </summary>
    public bool IsRead { get; set; } = false;
    
    /// <summary>
    /// Date and time when the message was read by the recipient.
    /// Used for message read timing tracking and management.
    /// Set when the message is read by the recipient.
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Type of message (e.g., "Text", "Image", "File", "System").
    /// Used for message type classification and management.
    /// Optional - used for enhanced message management and classification.
    /// </summary>
    [MaxLength(50)]
    public string? MessageType { get; set; }

    /// <summary>
    /// Indicates whether this message is a system-generated message.
    /// Used for message type classification and management.
    /// Defaults to false for standard user messages.
    /// </summary>
    public bool IsSystemMessage { get; set; } = false;
}

/// <summary>
/// Core chat attachment entity that manages all chat attachments in the system.
/// This entity handles chat attachment creation, management, and tracking for file sharing.
/// It serves as the central hub for chat attachment management, providing attachment creation,
/// file tracking, and sharing capabilities.
/// </summary>
public class ChatAttachment : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the chat attachment.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each chat attachment in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the ChatSession that this attachment belongs to.
    /// Links this attachment to the specific chat session.
    /// Required for session-attachment relationship management.
    /// </summary>
    public Guid SessionId { get; set; }
    
    /// <summary>
    /// Navigation property to the ChatSession that this attachment belongs to.
    /// Provides access to session information for attachment management.
    /// Used for session-attachment relationship operations.
    /// </summary>
    public virtual ChatSession Session { get; set; } = null!;

    /// <summary>
    /// Name of the attached file.
    /// Used for file identification and display purposes.
    /// Required for attachment management and user experience.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Type of the attached file.
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
    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the file was uploaded.
    /// Used for file upload timing tracking and management.
    /// Set when the file is uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// ID of the user who uploaded this file.
    /// Used for file upload tracking and management.
    /// Required for file upload identification.
    /// </summary>
    public Guid UploadedBy { get; set; }
} 
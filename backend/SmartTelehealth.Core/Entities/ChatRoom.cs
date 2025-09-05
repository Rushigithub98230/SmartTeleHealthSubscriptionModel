using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core chat room entity that manages all chat rooms in the system.
/// This entity handles chat room creation, management, and participant coordination.
/// It serves as the central hub for chat functionality, integrating with users, providers,
/// subscriptions, consultations, and messages. The entity includes comprehensive chat room
/// management, security features, and communication capabilities.
/// </summary>
public class ChatRoom : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the chat room.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each chat room in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible types of chat rooms.
    /// Used for chat room type management and categorization.
    /// </summary>
    public enum ChatRoomType
    {
        /// <summary>Chat room between a patient and provider.</summary>
        PatientProvider,
        /// <summary>Group chat room with multiple participants.</summary>
        Group,
        /// <summary>Support chat room for customer service.</summary>
        Support,
        /// <summary>Consultation-specific chat room.</summary>
        Consultation
    }

    /// <summary>
    /// Enumeration defining the possible statuses of chat rooms.
    /// Used for chat room status tracking and lifecycle management.
    /// </summary>
    public enum ChatRoomStatus
    {
        /// <summary>Chat room is active and available for use.</summary>
        Active,
        /// <summary>Chat room is temporarily paused.</summary>
        Paused,
        /// <summary>Chat room has been archived.</summary>
        Archived,
        /// <summary>Chat room has been deleted.</summary>
        Deleted
    }

    // Chat room details
    /// <summary>
    /// Name of this chat room.
    /// Used for chat room identification and user interface display.
    /// Required for chat room creation and management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of this chat room.
    /// Used for chat room context and additional information.
    /// Optional - set when chat room description is provided.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Type of this chat room.
    /// Used for chat room type management and categorization.
    /// Defaults to PatientProvider when chat room is first created.
    /// </summary>
    public ChatRoomType Type { get; set; } = ChatRoomType.PatientProvider;

    /// <summary>
    /// Current status of this chat room.
    /// Used for chat room status tracking and lifecycle management.
    /// Defaults to Active when chat room is first created.
    /// </summary>
    public ChatRoomStatus Status { get; set; } = ChatRoomStatus.Active;

    // Foreign keys for different chat types
    /// <summary>
    /// Foreign key reference to the User who is the patient in this chat room.
    /// Links this chat room to the specific patient user account.
    /// Optional - used for patient-provider chat rooms.
    /// </summary>
    public int? PatientId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is the patient in this chat room.
    /// Provides access to patient information for chat room management.
    /// Used for patient-provider chat room operations.
    /// </summary>
    public virtual User? Patient { get; set; }

    /// <summary>
    /// Foreign key reference to the Provider who is in this chat room.
    /// Links this chat room to the specific healthcare provider.
    /// Optional - used for patient-provider chat rooms.
    /// </summary>
    public int? ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who is in this chat room.
    /// Provides access to provider information for chat room management.
    /// Used for patient-provider chat room operations.
    /// </summary>
    public virtual Provider? Provider { get; set; }

    /// <summary>
    /// Foreign key reference to the Subscription that covers this chat room.
    /// Links this chat room to the specific user subscription for access control.
    /// Optional - used for subscription-based chat room access.
    /// </summary>
    public Guid? SubscriptionId { get; set; }
    
    /// <summary>
    /// Navigation property to the Subscription that covers this chat room.
    /// Provides access to subscription information for chat room access control.
    /// Used for subscription-based chat room operations.
    /// </summary>
    public virtual Subscription? Subscription { get; set; }

    /// <summary>
    /// Foreign key reference to the Consultation that this chat room is for.
    /// Links this chat room to the specific consultation session.
    /// Optional - used for consultation-specific chat rooms.
    /// </summary>
    public Guid? ConsultationId { get; set; }
    
    /// <summary>
    /// Navigation property to the Consultation that this chat room is for.
    /// Provides access to consultation information for chat room management.
    /// Used for consultation-specific chat room operations.
    /// </summary>
    public virtual Consultation? Consultation { get; set; }

    // Security and compliance
    /// <summary>
    /// Indicates whether this chat room content is encrypted.
    /// Used for chat room security and privacy protection.
    /// Defaults to true for secure chat room handling.
    /// </summary>
    public bool IsEncrypted { get; set; } = true;
    
    /// <summary>
    /// Encryption key used for this chat room.
    /// Used for chat room encryption and decryption.
    /// Set when chat room is encrypted for security.
    /// </summary>
    public string? EncryptionKey { get; set; }
    
    /// <summary>
    /// Date and time of the last activity in this chat room.
    /// Used for chat room activity tracking and management.
    /// Updated when messages are sent or received in the chat room.
    /// </summary>
    public DateTime? LastActivityAt { get; set; }
    
    /// <summary>
    /// Date and time when this chat room was archived.
    /// Used for chat room archival tracking and management.
    /// Set when chat room is archived.
    /// </summary>
    public DateTime? ArchivedAt { get; set; }
    
    /// <summary>
    /// Indicates whether file sharing is allowed in this chat room.
    /// Used for chat room feature control and security management.
    /// Defaults to true for standard chat room functionality.
    /// </summary>
    public bool AllowFileSharing { get; set; } = true;
    
    /// <summary>
    /// Indicates whether voice calls are allowed in this chat room.
    /// Used for chat room feature control and communication management.
    /// Defaults to true for standard chat room functionality.
    /// </summary>
    public bool AllowVoiceCalls { get; set; } = true;
    
    /// <summary>
    /// Indicates whether video calls are allowed in this chat room.
    /// Used for chat room feature control and communication management.
    /// Defaults to true for standard chat room functionality.
    /// </summary>
    public bool AllowVideoCalls { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Collection of messages in this chat room.
    /// Used for message management and chat room communication.
    /// Includes all messages sent in this chat room.
    /// </summary>
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    
    /// <summary>
    /// Collection of participants in this chat room.
    /// Used for participant management and chat room access control.
    /// Includes all participants who have access to this chat room.
    /// </summary>
    public virtual ICollection<ChatRoomParticipant> Participants { get; set; } = new List<ChatRoomParticipant>();
} 
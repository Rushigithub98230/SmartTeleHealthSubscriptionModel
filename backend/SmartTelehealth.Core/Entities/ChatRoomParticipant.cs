using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Entity for managing chat room participants and their roles and permissions.
/// This entity handles participant management, role assignment, and permission control for chat rooms.
/// It serves as the central hub for chat room participant management, providing role-based access control,
/// permission management, and participant status tracking.
/// </summary>
public class ChatRoomParticipant : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the chat room participant.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each chat room participant in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible statuses of chat room participants.
    /// Used for participant status tracking and management.
    /// </summary>
    public enum ParticipantStatus
    {
        /// <summary>Participant is active and can participate in the chat room.</summary>
        Active,
        /// <summary>Participant is inactive and cannot participate in the chat room.</summary>
        Inactive,
        /// <summary>Participant is banned from the chat room.</summary>
        Banned,
        /// <summary>Participant has left the chat room.</summary>
        Left
    }

    /// <summary>
    /// Enumeration defining the possible roles of chat room participants.
    /// Used for participant role management and access control.
    /// </summary>
    public enum ParticipantRole
    {
        /// <summary>Standard member with basic permissions.</summary>
        Member,
        /// <summary>Administrator with full permissions.</summary>
        Admin,
        /// <summary>Moderator with moderation permissions.</summary>
        Moderator,
        /// <summary>Guest with limited permissions.</summary>
        Guest,
        /// <summary>Healthcare provider with provider-specific permissions.</summary>
        Provider,
        /// <summary>Patient with patient-specific permissions.</summary>
        Patient,
        /// <summary>External participant with external permissions.</summary>
        External
    }

    /// <summary>
    /// Foreign key reference to the ChatRoom that this participant belongs to.
    /// Links this participant to the specific chat room.
    /// Required for chat room-participant relationship management.
    /// </summary>
    public Guid ChatRoomId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who is this participant.
    /// Links this participant to the specific user account.
    /// Required for user-participant relationship management.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Role of this participant in the chat room.
    /// Used for participant role management and access control.
    /// Set when participant is added to the chat room.
    /// </summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of this participant in the chat room.
    /// Used for participant status tracking and management.
    /// Defaults to Active when participant is added to the chat room.
    /// </summary>
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Active;
    
    /// <summary>
    /// Indicates whether this participant can send messages in the chat room.
    /// Used for participant permission management and access control.
    /// Defaults to true for standard participants.
    /// </summary>
    public bool CanSendMessages { get; set; } = true;
    
    /// <summary>
    /// Indicates whether this participant can send files in the chat room.
    /// Used for participant permission management and access control.
    /// Defaults to true for standard participants.
    /// </summary>
    public bool CanSendFiles { get; set; } = true;
    
    /// <summary>
    /// Indicates whether this participant can invite others to the chat room.
    /// Used for participant permission management and access control.
    /// Defaults to false for standard participants.
    /// </summary>
    public bool CanInviteOthers { get; set; } = false;
    
    /// <summary>
    /// Indicates whether this participant can moderate the chat room.
    /// Used for participant permission management and access control.
    /// Defaults to false for standard participants.
    /// </summary>
    public bool CanModerate { get; set; } = false;
    
    /// <summary>
    /// Date and time when this participant joined the chat room.
    /// Used for participant timing tracking and management.
    /// Set when participant is added to the chat room.
    /// </summary>
    public DateTime JoinedAt { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Provider who is this participant.
    /// Links this participant to the specific healthcare provider.
    /// Optional - used for provider-participant relationship management.
    /// </summary>
    public int? ProviderId { get; set; }
    
    /// <summary>
    /// Date and time when this participant left the chat room.
    /// Used for participant timing tracking and management.
    /// Set when participant leaves the chat room.
    /// </summary>
    public DateTime? LeftAt { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Navigation property to the ChatRoom that this participant belongs to.
    /// Provides access to chat room information for participant management.
    /// Used for chat room-participant relationship operations.
    /// </summary>
    public virtual ChatRoom? ChatRoom { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is this participant.
    /// Provides access to user information for participant management.
    /// Used for user-participant relationship operations.
    /// </summary>
    public virtual User? User { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who is this participant.
    /// Provides access to provider information for participant management.
    /// Used for provider-participant relationship operations.
    /// </summary>
    public virtual Provider? Provider { get; set; }
} 
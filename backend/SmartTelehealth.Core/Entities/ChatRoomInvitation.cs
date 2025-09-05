using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core chat room invitation entity that manages all chat room invitations in the system.
/// This entity handles chat room invitation creation, management, and response tracking.
/// It serves as the central hub for chat room invitation management, providing invitation
/// creation, response tracking, and participant management capabilities.
/// </summary>
public class ChatRoomInvitation : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the chat room invitation.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each chat room invitation in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible statuses of chat room invitations.
    /// Used for invitation status tracking and management.
    /// </summary>
    public enum InvitationStatus
    {
        /// <summary>Invitation is pending response from the invitee.</summary>
        Pending,
        /// <summary>Invitation has been accepted by the invitee.</summary>
        Accepted,
        /// <summary>Invitation has been declined by the invitee.</summary>
        Declined,
        /// <summary>Invitation has expired without response.</summary>
        Expired
    }

    /// <summary>
    /// Foreign key reference to the ChatRoom that this invitation belongs to.
    /// Links this invitation to the specific chat room.
    /// Required for chat room-invitation relationship management.
    /// </summary>
    public Guid ChatRoomId { get; set; }
    
    /// <summary>
    /// Navigation property to the ChatRoom that this invitation belongs to.
    /// Provides access to chat room information for invitation management.
    /// Used for chat room-invitation relationship operations.
    /// </summary>
    public virtual ChatRoom ChatRoom { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the User who sent this invitation.
    /// Links this invitation to the specific user who sent it.
    /// Required for user-invitation relationship management.
    /// </summary>
    public int InvitedByUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who sent this invitation.
    /// Provides access to user information for invitation management.
    /// Used for user-invitation relationship operations.
    /// </summary>
    public virtual User InvitedByUser { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the User who was invited.
    /// Links this invitation to the specific user who was invited.
    /// Required for user-invitation relationship management.
    /// </summary>
    public int InvitedUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who was invited.
    /// Provides access to user information for invitation management.
    /// Used for user-invitation relationship operations.
    /// </summary>
    public virtual User InvitedUser { get; set; } = null!;

    /// <summary>
    /// Current status of the chat room invitation.
    /// Used for invitation status tracking and management.
    /// Defaults to Pending when invitation is created.
    /// </summary>
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    /// <summary>
    /// Message or notes included with the invitation.
    /// Used for invitation communication and user experience.
    /// Optional - used for enhanced invitation management and communication.
    /// </summary>
    [MaxLength(500)]
    public string? Message { get; set; }

    /// <summary>
    /// Date and time when this invitation expires.
    /// Used for invitation expiration tracking and management.
    /// Set when the invitation is created for expiration management.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Date and time when the invitation was responded to.
    /// Used for invitation response tracking and management.
    /// Set when the invitation is responded to by the invitee.
    /// </summary>
    public DateTime? RespondedAt { get; set; }
} 
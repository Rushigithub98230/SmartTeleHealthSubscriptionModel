using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core appointment invitation entity that manages all appointment invitations in the system.
/// This entity handles appointment invitation creation, management, and response tracking.
/// It serves as the central hub for appointment invitation management, providing invitation
/// creation, response tracking, and participant management capabilities.
/// </summary>
public class AppointmentInvitation : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the appointment invitation.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each appointment invitation in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Appointment that this invitation belongs to.
    /// Links this invitation to the specific appointment.
    /// Required for appointment-invitation relationship management.
    /// </summary>
    public Guid AppointmentId { get; set; }
    
    /// <summary>
    /// Navigation property to the Appointment that this invitation belongs to.
    /// Provides access to appointment information for invitation management.
    /// Used for appointment-invitation relationship operations.
    /// </summary>
    public virtual Appointment Appointment { get; set; } = null!;

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

    // Internal or external invitee
    /// <summary>
    /// Foreign key reference to the User who was invited (for internal users).
    /// Links this invitation to the specific user who was invited.
    /// Optional - used for internal user invitation management.
    /// </summary>
    public int? InvitedUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who was invited (for internal users).
    /// Provides access to user information for invitation management.
    /// Used for user-invitation relationship operations.
    /// </summary>
    public virtual User? InvitedUser { get; set; }
    
    /// <summary>
    /// Email address of the external invitee.
    /// Used for external user invitation management and communication.
    /// Optional - used for external user invitation management.
    /// </summary>
    [MaxLength(256)]
    public string? InvitedEmail { get; set; }
    
    /// <summary>
    /// Phone number of the external invitee.
    /// Used for external user invitation management and communication.
    /// Optional - used for external user invitation management.
    /// </summary>
    [MaxLength(32)]
    public string? InvitedPhone { get; set; }

    /// <summary>
    /// Foreign key reference to the InvitationStatus of this invitation.
    /// Links this invitation to the specific invitation status.
    /// Required for invitation status management.
    /// </summary>
    public Guid InvitationStatusId { get; set; }
    
    /// <summary>
    /// Navigation property to the InvitationStatus of this invitation.
    /// Provides access to invitation status information for invitation management.
    /// Used for invitation-status relationship operations.
    /// </summary>
    public virtual InvitationStatus? InvitationStatus { get; set; }

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
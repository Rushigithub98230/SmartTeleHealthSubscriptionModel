using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Entity for managing appointment participants and their roles.
/// This entity handles participant management, role assignment, and participation tracking for appointments.
/// It serves as a comprehensive participant system that supports both internal users and external participants,
/// with role-based access control and participation status tracking.
/// </summary>
public class AppointmentParticipant : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the appointment participant.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each participant in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key reference to the Appointment that this participant is part of.
    /// Links this participant to the specific appointment.
    /// Required for appointment-participant relationship management.
    /// </summary>
    public Guid AppointmentId { get; set; }
    
    /// <summary>
    /// Navigation property to the Appointment that this participant is part of.
    /// Provides access to appointment information for participant management.
    /// Used for appointment-participant relationship operations.
    /// </summary>
    public virtual Appointment Appointment { get; set; } = null!;

    // Internal user
    /// <summary>
    /// Foreign key reference to the User who is this participant.
    /// Links this participant to the specific user account.
    /// Optional - used for internal user participants.
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is this participant.
    /// Provides access to user information for participant management.
    /// Used for internal user participant operations.
    /// </summary>
    public virtual User? User { get; set; }

    // External participant
    /// <summary>
    /// Email address of external participant who is not a registered user.
    /// Used for external participant identification and communication.
    /// Set when participant is external and not a registered user.
    /// </summary>
    [MaxLength(256)]
    public string? ExternalEmail { get; set; }
    
    /// <summary>
    /// Phone number of external participant who is not a registered user.
    /// Used for external participant identification and communication.
    /// Set when participant is external and not a registered user.
    /// </summary>
    [MaxLength(32)]
    public string? ExternalPhone { get; set; }

    // Status and Role Foreign Keys
    /// <summary>
    /// Foreign key reference to the ParticipantRole that defines this participant's role.
    /// Links this participant to the specific role (Patient, Provider, Observer, etc.).
    /// Required for participant role management and access control.
    /// </summary>
    public Guid ParticipantRoleId { get; set; }
    
    /// <summary>
    /// Navigation property to the ParticipantRole that defines this participant's role.
    /// Provides access to role information for participant management.
    /// Used for participant role management and access control.
    /// </summary>
    public virtual ParticipantRole? ParticipantRole { get; set; }

    /// <summary>
    /// Foreign key reference to the ParticipantStatus that defines this participant's status.
    /// Links this participant to the specific status (Invited, Joined, Left, etc.).
    /// Required for participant status tracking and management.
    /// </summary>
    public Guid ParticipantStatusId { get; set; }
    
    /// <summary>
    /// Navigation property to the ParticipantStatus that defines this participant's status.
    /// Provides access to status information for participant management.
    /// Used for participant status tracking and management.
    /// </summary>
    public virtual ParticipantStatus? ParticipantStatus { get; set; }

    /// <summary>
    /// Date and time when this participant was invited to the appointment.
    /// Used for invitation tracking and participant management.
    /// Set when participant is invited to the appointment.
    /// </summary>
    public DateTime? InvitedAt { get; set; }
    
    /// <summary>
    /// Date and time when this participant joined the appointment.
    /// Used for participation tracking and participant management.
    /// Set when participant joins the appointment (e.g., enters video call).
    /// </summary>
    public DateTime? JoinedAt { get; set; }
    
    /// <summary>
    /// Date and time when this participant left the appointment.
    /// Used for participation tracking and participant management.
    /// Set when participant leaves the appointment (e.g., exits video call).
    /// </summary>
    public DateTime? LeftAt { get; set; }
    
    /// <summary>
    /// Date and time when this participant was last seen in the appointment.
    /// Used for activity tracking and participant management.
    /// Updated when participant is active in the appointment.
    /// </summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>
    /// Foreign key reference to the User who invited this participant.
    /// Links this participant to the specific user who sent the invitation.
    /// Optional - used for invitation tracking and participant management.
    /// </summary>
    public int? InvitedByUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who invited this participant.
    /// Provides access to inviter information for participant management.
    /// Used for invitation tracking and participant management.
    /// </summary>
    public virtual User? InvitedByUser { get; set; }
} 
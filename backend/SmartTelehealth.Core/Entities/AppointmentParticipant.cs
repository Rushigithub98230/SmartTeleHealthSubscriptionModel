using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class AppointmentParticipant : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    public Guid AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; } = null!;

    // Internal user
    public int? UserId { get; set; }
    public virtual User? User { get; set; }

    // External participant
    [MaxLength(256)]
    public string? ExternalEmail { get; set; }
    [MaxLength(32)]
    public string? ExternalPhone { get; set; }

    // Status and Role Foreign Keys
    public Guid ParticipantRoleId { get; set; }
    public virtual ParticipantRole? ParticipantRole { get; set; }

    public Guid ParticipantStatusId { get; set; }
    public virtual ParticipantStatus? ParticipantStatus { get; set; }

    public DateTime? InvitedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    public int? InvitedByUserId { get; set; }
    public virtual User? InvitedByUser { get; set; }
} 
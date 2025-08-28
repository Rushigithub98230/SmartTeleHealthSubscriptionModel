using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class ChatRoomParticipant : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum ParticipantStatus
    {
        Active,
        Inactive,
        Banned,
        Left
    }

    public enum ParticipantRole
    {
        Member,
        Admin,
        Moderator,
        Guest,
        Provider,
        Patient,
        External
    }

    public Guid ChatRoomId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Active;
    public bool CanSendMessages { get; set; } = true;
    public bool CanSendFiles { get; set; } = true;
    public bool CanInviteOthers { get; set; } = false;
    public bool CanModerate { get; set; } = false;
    public DateTime JoinedAt { get; set; }
    public int? ProviderId { get; set; }
    public DateTime? LeftAt { get; set; }
    
    // Navigation properties
    public virtual ChatRoom? ChatRoom { get; set; }
    public virtual User? User { get; set; }
    public virtual Provider? Provider { get; set; }
} 
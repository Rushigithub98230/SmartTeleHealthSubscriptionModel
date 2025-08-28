using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class ChatRoomInvitation : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum InvitationStatus
    {
        Pending,
        Accepted,
        Declined,
        Expired
    }

    // Foreign keys
    public Guid ChatRoomId { get; set; }
    public virtual ChatRoom ChatRoom { get; set; } = null!;

    public int InvitedByUserId { get; set; }
    public virtual User InvitedByUser { get; set; } = null!;

    public int InvitedUserId { get; set; }
    public virtual User InvitedUser { get; set; } = null!;

    // Invitation information
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    [MaxLength(500)]
    public string? Message { get; set; }

    public DateTime ExpiresAt { get; set; }

    // Timestamps
    public DateTime? RespondedAt { get; set; }
} 
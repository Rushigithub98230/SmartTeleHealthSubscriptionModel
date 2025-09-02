using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

// Invitation DTOs
public class ChatRoomInvitationDto
{
    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public string ChatRoomName { get; set; } = string.Empty;
    public Guid InvitedByUserId { get; set; }
    public string InvitedByUserName { get; set; } = string.Empty;
    public Guid InvitedUserId { get; set; }
    public string InvitedUserName { get; set; } = string.Empty;
    public Guid InvitationStatusId { get; set; }
    public string InvitationStatusName { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public class CreateInvitationDto
{
    [Required]
    public Guid InvitedUserId { get; set; }

    [MaxLength(500)]
    public string? Message { get; set; }

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
}

// Reaction DTOs
public class AddReactionDto
{
    [Required]
    [MaxLength(10)]
    public string Emoji { get; set; } = string.Empty;
}

// Read Receipt DTOs
public class MessageReadReceiptDto
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public DateTime ReadAt { get; set; }
    public string? DeviceInfo { get; set; }
}

// Real-time DTOs
public class TypingIndicatorDto
{
    public Guid ChatRoomId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsTyping { get; set; }
}

public class MessageStatusUpdateDto
{
    public Guid MessageId { get; set; }
    public int MessageStatusId { get; set; }
    public string MessageStatusName { get; set; } = string.Empty;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class MessageSearchDto
{
    [Required]
    public Guid ChatRoomId { get; set; }

    public string SearchTerm { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MessageTypeId { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}

public class ChatRoomStatisticsDto
{
    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public string ChatRoomName { get; set; } = string.Empty;
    public int TotalMessages { get; set; }
    public int TotalParticipants { get; set; }
    public DateTime? LastActivity { get; set; }
    public int UnreadCount { get; set; }
    public Dictionary<string, int> MessageTypes { get; set; } = new();
    public Dictionary<string, int> ParticipantActivity { get; set; } = new();
} 
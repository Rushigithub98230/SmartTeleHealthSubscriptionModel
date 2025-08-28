using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.DTOs;

// Chat Notification DTO - This is unique to ChatDtos.cs
public class ChatNotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ChatRoomId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}

// Chat Room Invitation DTO
public class InviteToChatRoomDto
{
    public int InviteeId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
} 
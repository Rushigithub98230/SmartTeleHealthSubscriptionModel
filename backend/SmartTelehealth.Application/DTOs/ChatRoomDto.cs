namespace SmartTelehealth.Application.DTOs
{
    public class ChatRoomDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Direct", "Group", "Channel"
        public string Status { get; set; } = string.Empty; // "Active", "Archived", "Deleted"
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int ParticipantCount { get; set; }
        public int UnreadMessageCount { get; set; }
        public string? LastMessageId { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessageSender { get; set; }
        public string? LastMessageContent { get; set; }
    }

    public class CreateChatRoomDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ChatRoomType { get; set; } = "General";
        public bool IsPrivate { get; set; } = false;
        public string? ImageUrl { get; set; }
        public List<string> ParticipantIds { get; set; } = new();
    }

    public class UpdateChatRoomDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Status { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPrivate { get; set; }
    }
} 
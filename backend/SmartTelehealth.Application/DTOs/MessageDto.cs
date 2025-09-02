namespace SmartTelehealth.Application.DTOs
{
    public class MessageDto
    {
        public string Id { get; set; } = string.Empty;
        public string ChatRoomId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty; // "text", "image", "file", "audio", "video"
        public string Status { get; set; } = string.Empty; // "sent", "delivered", "read", "failed"
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? ReplyToMessageId { get; set; }
        public string? ReplyToMessageContent { get; set; }
        public List<string> AttachmentIds { get; set; } = new();
        public List<MessageReactionDto> Reactions { get; set; } = new();
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string ChatRoomName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public int MessageTypeId { get; set; }
        public string MessageTypeName { get; set; } = string.Empty;
        public int MessageStatusId { get; set; }
        public string MessageStatusName { get; set; } = string.Empty;
    }

    public class CreateMessageDto
    {
        public string ChatRoomId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text";
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }
        public long? AttachmentSize { get; set; }
        public string? ReplyToMessageId { get; set; }
    }

    public class UpdateMessageDto
    {
        public string? Content { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }
        public long? AttachmentSize { get; set; }
        public bool? IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
    }

    public class MessageReactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string ReactionType { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class ChatStatisticsDto
    {
        public string ChatRoomId { get; set; } = string.Empty;
        public int TotalMessages { get; set; }
        public int TotalParticipants { get; set; }
        public int ActiveParticipants { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public int UnreadMessages { get; set; }
        public Dictionary<string, int> MessageTypeDistribution { get; set; } = new();
        public Dictionary<string, int> ReactionDistribution { get; set; } = new();
    }
} 
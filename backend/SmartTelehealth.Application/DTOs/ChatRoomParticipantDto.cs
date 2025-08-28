namespace SmartTelehealth.Application.DTOs
{
    public class ChatRoomParticipantDto
    {
        public string Id { get; set; } = string.Empty;
        public string ChatRoomId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Admin", "Moderator", "Member"
        public string ProviderName { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public bool IsOnline { get; set; }
        public bool IsMuted { get; set; }
        public bool IsBlocked { get; set; }
        public int UnreadMessageCount { get; set; }
    }
} 
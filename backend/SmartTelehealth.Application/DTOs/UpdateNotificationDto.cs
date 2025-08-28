namespace SmartTelehealth.Application.DTOs
{
    public class UpdateNotificationDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }
} 
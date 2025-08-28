namespace SmartTelehealth.Application.DTOs
{
    public class TrackUsageDto
    {
        public string PrivilegeName { get; set; } = string.Empty;
        public int UsageCount { get; set; } = 1;
        public string Description { get; set; } = string.Empty;
        public DateTime? UsageDate { get; set; }
        public string? SessionId { get; set; }
        public string? ConsultationId { get; set; }
    }
} 
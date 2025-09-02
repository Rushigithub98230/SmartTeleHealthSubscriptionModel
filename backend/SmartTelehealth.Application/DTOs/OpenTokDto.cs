namespace SmartTelehealth.Application.DTOs;

public class OpenTokSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string MeetingUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string ParticipantId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // publisher, subscriber
    public string SessionName { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public int StreamCount { get; set; }
}

public class OpenTokRecordingDto
{
    public string RecordingId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty; // started, stopped, available, deleted
    public TimeSpan Duration { get; set; }
}

public class ConsultationAnalyticsDto
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public int TotalConsultations { get; set; }
    public int CompletedConsultations { get; set; }
    public int CancelledConsultations { get; set; }
    public int NoShowConsultations { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageConsultationDuration { get; set; }
    public decimal AverageRating { get; set; }
    public int PatientCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IEnumerable<ConsultationTrendDto> Trends { get; set; } = new List<ConsultationTrendDto>();
}

public class ConsultationTrendDto
{
    public DateTime Date { get; set; }
    public int ConsultationCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageDuration { get; set; }
}

public class CreateOneTimeConsultationDto
{
    public Guid UserId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public decimal Fee { get; set; }
    public string? Notes { get; set; }
    public string? Symptoms { get; set; }
    public string? MedicalHistory { get; set; }
} 
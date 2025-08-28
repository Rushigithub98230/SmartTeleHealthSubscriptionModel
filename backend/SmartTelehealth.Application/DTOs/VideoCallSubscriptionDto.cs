using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Application.DTOs;

public class VideoCallAccessDto
{
    public bool HasAccess { get; set; }
    public Guid? SubscriptionId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int ConsultationLimit { get; set; }
    public int RemainingConsultations { get; set; }
    public int MaxDurationMinutes { get; set; }
    public bool CanRecord { get; set; }
    public bool CanBroadcast { get; set; }
    public bool IsOneTime { get; set; }
    public Guid? ConsultationId { get; set; }
    public string? Reason { get; set; }
}

public class VideoCallBillingDto
{
    public Guid? BillingRecordId { get; set; }
    public decimal Amount { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsIncludedInSubscription { get; set; }
    public Guid ConsultationId { get; set; }
    public string? Description { get; set; }
}

public class VideoCallUsageDto
{
    public Guid SubscriptionId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime CurrentBillingPeriodStart { get; set; }
    public DateTime CurrentBillingPeriodEnd { get; set; }
    public int TotalVideoCallsThisPeriod { get; set; }
    public int TotalDurationThisPeriod { get; set; }
    public double AverageDurationMinutes { get; set; }
}

public class CreateVideoCallSessionDto
{
    public Guid ConsultationId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public bool IsOneTime { get; set; } = false;
}

public class VideoCallTokenDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public OpenTokRole Role { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsValid { get; set; }
}

public class VideoCallSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public Guid ConsultationId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsRecording { get; set; }
    public bool IsBroadcasting { get; set; }
    public int ParticipantCount { get; set; }
}

public class VideoCallLimitsDto
{
    public int MaxDurationMinutes { get; set; }
    public int MaxParticipants { get; set; }
    public bool CanRecord { get; set; }
    public bool CanBroadcast { get; set; }
    public bool CanShareScreen { get; set; }
    public bool CanUseChat { get; set; }
    public int MaxRecordingDuration { get; set; }
    public string[] AllowedRoles { get; set; } = Array.Empty<string>();
} 
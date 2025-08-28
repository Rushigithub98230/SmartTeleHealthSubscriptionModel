using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs
{
    public class VideoCallDto
    {
        public Guid Id { get; set; }
        public Guid CallId => Id; // For compatibility with hub usage
        public Guid AppointmentId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RecordingUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateVideoCallDto
    {
        [Required]
        public Guid AppointmentId { get; set; }
        public Guid ChatRoomId { get; set; }
        public VideoCallType Type { get; set; } = VideoCallType.OneOnOne;
        public bool IsVideoEnabled { get; set; } = true;
        public bool IsAudioEnabled { get; set; } = true;
        public string? SessionId { get; set; }
        public string? Token { get; set; }
    }

    public enum VideoCallType
    {
        OneOnOne,
        Group
    }

    public class UpdateVideoCallDto
    {
        public string? Status { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? RecordingUrl { get; set; }
    }

    public class VideoCallEventDto
    {
        public Guid Id { get; set; }
        public Guid VideoCallId { get; set; }
        public SmartTelehealth.Core.Entities.VideoCallEventType Type { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Description { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVideoCallEventDto
    {
        [Required]
        public Guid VideoCallId { get; set; }
        
        [Required]
        public SmartTelehealth.Core.Entities.VideoCallEventType Type { get; set; }
        
        public string? Description { get; set; }
        public string? Metadata { get; set; }
    }

    public class VideoCallParticipantDto
    {
        public Guid Id { get; set; }
        public Guid VideoCallId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ProviderId { get; set; }
        public bool IsInitiator { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public int DurationSeconds { get; set; }
        public bool IsVideoEnabled { get; set; }
        public bool IsAudioEnabled { get; set; }
        public bool IsScreenSharingEnabled { get; set; }
        public int? AudioQuality { get; set; }
        public int? VideoQuality { get; set; }
        public int? NetworkQuality { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateVideoCallParticipantDto
    {
        [Required]
        public Guid VideoCallId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        public Guid? ProviderId { get; set; }
        public bool IsInitiator { get; set; }
        public bool IsVideoEnabled { get; set; } = true;
        public bool IsAudioEnabled { get; set; } = true;
        public bool IsScreenSharingEnabled { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class UpdateVideoCallParticipantDto
    {
        public bool? IsVideoEnabled { get; set; }
        public bool? IsAudioEnabled { get; set; }
        public bool? IsScreenSharingEnabled { get; set; }
        public int? AudioQuality { get; set; }
        public int? VideoQuality { get; set; }
        public int? NetworkQuality { get; set; }
        public DateTime? LeftAt { get; set; }
    }

    public class UpdateVideoCallStatusDto
    {
        public string? Status { get; set; }
        public DateTime? ConnectedAt { get; set; }
        public DateTime? DisconnectedAt { get; set; }
        public int? DurationSeconds { get; set; }
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
        public string? FailureReason { get; set; }
    }

    public class UpdateParticipantSettingsDto
    {
        public bool? IsVideoEnabled { get; set; }
        public bool? IsAudioEnabled { get; set; }
        public bool? IsScreenSharingEnabled { get; set; }
    }

    public class LogVideoCallEventDto
    {
        public Guid? UserId { get; set; }
        public Guid? ProviderId { get; set; }
        public SmartTelehealth.Core.Entities.VideoCallEventType Type { get; set; }
        public string? Description { get; set; }
        public string? Metadata { get; set; }
    }

    public class UpdateCallQualityDto
    {
        public int? AudioQuality { get; set; }
        public int? VideoQuality { get; set; }
        public int? NetworkQuality { get; set; }
    }

    public class VideoCallAnalyticsDto
    {
        public Guid CallId { get; set; }
        public int TotalParticipants { get; set; }
        public int ActiveParticipants { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public double AverageAudioQuality { get; set; }
        public double AverageVideoQuality { get; set; }
        public double AverageNetworkQuality { get; set; }
        public int TotalEvents { get; set; }
        public bool WasRecorded { get; set; }
        public bool HadScreenSharing { get; set; }
        public bool WasEncrypted { get; set; }
        public bool WasHIPAACompliant { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Video Call Control DTOs
    public class EndVideoCallDto
    {
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class RejectVideoCallDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class ToggleVideoDto
    {
        public bool Enabled { get; set; }
        public bool IsVideoEnabled { get; set; }
        public string? Reason { get; set; }
    }

    public class ToggleAudioDto
    {
        public bool Enabled { get; set; }
        public bool IsAudioEnabled { get; set; }
        public string? Reason { get; set; }
    }
} 
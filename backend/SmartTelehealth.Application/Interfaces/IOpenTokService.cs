using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IOpenTokService
{
    // Session management
    Task<JsonModel> CreateSessionAsync(string sessionName, bool isArchived, TokenModel tokenModel);
    Task<JsonModel> GetSessionAsync(string sessionId, TokenModel tokenModel);
    Task<JsonModel> ArchiveSessionAsync(string sessionId, TokenModel tokenModel);
    Task<JsonModel> DeleteSessionAsync(string sessionId, TokenModel tokenModel);

    // Token generation
    Task<JsonModel> GenerateTokenAsync(string sessionId, string userId, string userName, OpenTokRole role, TokenModel tokenModel);
    Task<JsonModel> GenerateTokenAsync(string sessionId, string userId, string userName, DateTime expireTime, OpenTokRole role, TokenModel tokenModel);

    // Stream management
    Task<JsonModel> GetSessionStreamsAsync(string sessionId, TokenModel tokenModel);
    Task<JsonModel> ForceDisconnectAsync(string sessionId, string connectionId, TokenModel tokenModel);
    Task<JsonModel> MuteStreamAsync(string sessionId, string streamId, bool mute, TokenModel tokenModel);

    // Recording
    Task<JsonModel> StartRecordingAsync(string sessionId, OpenTokRecordingOptions options, TokenModel tokenModel);
    Task<JsonModel> StopRecordingAsync(string recordingId, TokenModel tokenModel);
    Task<JsonModel> GetRecordingAsync(string recordingId, TokenModel tokenModel);
    Task<JsonModel> GetSessionRecordingsAsync(string sessionId, TokenModel tokenModel);
    Task<JsonModel> GetRecordingUrlAsync(string recordingId, TokenModel tokenModel);

    // Broadcasting
    Task<JsonModel> StartBroadcastAsync(string sessionId, OpenTokBroadcastOptions options, TokenModel tokenModel);
    Task<JsonModel> StopBroadcastAsync(string broadcastId, TokenModel tokenModel);
    Task<JsonModel> GetBroadcastAsync(string broadcastId, TokenModel tokenModel);

    // Webhook handling
    Task<JsonModel> HandleWebhookAsync(OpenTokWebhookDto webhook, TokenModel tokenModel);
    Task<JsonModel> LogSessionEventAsync(string sessionId, OpenTokEventType eventType, string connectionId, string? streamId, TokenModel tokenModel);

    // Analytics and monitoring
    Task<JsonModel> GetSessionAnalyticsAsync(string sessionId, TokenModel tokenModel);
    Task<JsonModel> GetConnectionQualityAsync(string sessionId, string connectionId, TokenModel tokenModel);

    // Health check
    Task<JsonModel> IsServiceHealthyAsync(TokenModel tokenModel);
}

public enum OpenTokRole
{
    Publisher,
    Subscriber,
    Moderator
}

public enum OpenTokEventType
{
    ConnectionCreated,
    ConnectionDestroyed,
    StreamCreated,
    StreamDestroyed,
    RecordingStarted,
    RecordingStopped,
    ArchiveStarted,
    ArchiveStopped,
    BroadcastStarted,
    BroadcastStopped
}

public class OpenTokStreamDto
{
    public string StreamId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
            public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool HasVideo { get; set; }
    public bool HasAudio { get; set; }
    public bool IsScreenSharing { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public enum OpenTokRecordingStatus
{
    Started,
    Stopped,
    Available,
    Failed,
    Deleted
}

public class OpenTokRecordingOptions
{
    public string Name { get; set; } = string.Empty;
    public bool HasAudio { get; set; } = true;
    public bool HasVideo { get; set; } = true;
    public OpenTokRecordingOutputMode OutputMode { get; set; } = OpenTokRecordingOutputMode.Composed;
    public string? Resolution { get; set; } = "1280x720";
    public string? Layout { get; set; }
    public int? MaxDuration { get; set; }
    public string? Storage { get; set; } = "cloud";
}

public enum OpenTokRecordingOutputMode
{
    Composed,
    Individual
}

public class OpenTokBroadcastDto
{
    public string BroadcastId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HlsUrl { get; set; } = string.Empty;
    public string RtmpUrl { get; set; } = string.Empty;
    public OpenTokBroadcastStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
}

public enum OpenTokBroadcastStatus
{
    Started,
    Stopped,
    Failed
}

public class OpenTokBroadcastOptions
{
    public string Name { get; set; } = string.Empty;
    public string HlsUrl { get; set; } = string.Empty;
    public string? RtmpUrl { get; set; }
    public int? MaxDuration { get; set; }
    public string? Resolution { get; set; } = "1280x720";
    public string? Layout { get; set; }
}

public class OpenTokWebhookDto
{
    public string EventType { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string? ConnectionId { get; set; }
    public string? StreamId { get; set; }
    public string? RecordingId { get; set; }
    public string? BroadcastId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class OpenTokSessionAnalyticsDto
{
    public string SessionId { get; set; } = string.Empty;
    public int TotalConnections { get; set; }
    public int TotalStreams { get; set; }
    public int TotalRecordings { get; set; }
    public int TotalBroadcasts { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double AverageAudioQuality { get; set; }
    public double AverageVideoQuality { get; set; }
    public double AverageNetworkQuality { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastActivity { get; set; }
}

public class OpenTokConnectionQualityDto
{
    public string ConnectionId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public double AudioLevel { get; set; }
    public double VideoLevel { get; set; }
    public double NetworkQuality { get; set; }
    public int PacketLoss { get; set; }
    public int RoundTripTime { get; set; }
    public DateTime Timestamp { get; set; }
} 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoCallController : BaseController
{
    private readonly IOpenTokService _openTokService;
    private readonly IConsultationService _consultationService;
    private readonly IVideoCallService _videoCallService;

    public VideoCallController(
        IOpenTokService openTokService,
        IConsultationService consultationService,
        IVideoCallService videoCallService)
    {
        _openTokService = openTokService;
        _consultationService = consultationService;
        _videoCallService = videoCallService;
    }

    /// <summary>
    /// Create a new video call session
    /// </summary>
    [HttpPost("sessions")]
    public async Task<JsonModel> CreateSession([FromBody] CreateVideoSessionDto createDto)
    {
        return await _openTokService.CreateSessionAsync(createDto.SessionName, createDto.IsArchived, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate a token for joining a video session
    /// </summary>
    [HttpPost("sessions/{sessionId}/token")]
    public async Task<JsonModel> GenerateToken(
        string sessionId, 
        [FromBody] GenerateTokenDto generateDto)
    {
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        return await _openTokService.GenerateTokenAsync(
            sessionId, 
            userId.ToString(), 
            userName, 
            DateTime.UtcNow.AddHours(24), 
            generateDto.Role, 
            GetToken(HttpContext));
    }

    /// <summary>
    /// Get session information
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    public async Task<JsonModel> GetSession(string sessionId)
    {
        return await _openTokService.GetSessionAsync(sessionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Archive a video session
    /// </summary>
    [HttpPost("sessions/{sessionId}/archive")]
    public async Task<JsonModel> ArchiveSession(string sessionId)
    {
        return await _openTokService.ArchiveSessionAsync(sessionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete a video session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    public async Task<JsonModel> DeleteSession(string sessionId)
    {
        return await _openTokService.DeleteSessionAsync(sessionId, GetToken(HttpContext));
    }



    /// <summary>
    /// Create a new video call
    /// </summary>
    [HttpPost]
    public async Task<JsonModel> CreateVideoCall([FromBody] CreateVideoCallDto createDto)
    {
        return await _videoCallService.CreateAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get video call by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetVideoCall(Guid id)
    {
        return await _videoCallService.GetByIdAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get video calls by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetVideoCallsByUser(int userId)
    {
        return await _videoCallService.GetByUserIdAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get all video calls
    /// </summary>
    [HttpGet]
    public async Task<JsonModel> GetAllVideoCalls()
    {
        return await _videoCallService.GetAllAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Update video call
    /// </summary>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateVideoCall(Guid id, [FromBody] UpdateVideoCallDto updateDto)
    {
        return await _videoCallService.UpdateAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete video call
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<JsonModel> DeleteVideoCall(Guid id)
    {
        return await _videoCallService.DeleteAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Initiate video call
    /// </summary>
    [HttpPost("initiate")]
    public async Task<JsonModel> InitiateVideoCall([FromBody] CreateVideoCallDto createDto)
    {
        return await _videoCallService.InitiateVideoCallAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Join video call
    /// </summary>
    [HttpPost("{callId}/join")]
    public async Task<JsonModel> JoinVideoCall(Guid callId)
    {
        var userId = GetCurrentUserId();
        return await _videoCallService.JoinVideoCallAsync(callId, userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Leave video call
    /// </summary>
    [HttpPost("{callId}/leave")]
    public async Task<JsonModel> LeaveVideoCall(Guid callId)
    {
        var userId = GetCurrentUserId();
        return await _videoCallService.LeaveVideoCallAsync(callId, userId, GetToken(HttpContext));
    }

    /// <summary>
    /// End video call
    /// </summary>
    [HttpPost("{callId}/end")]
    public async Task<JsonModel> EndVideoCall(Guid callId, [FromBody] EndVideoCallDto endDto)
    {
        return await _videoCallService.EndVideoCallAsync(callId, endDto.Reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Reject video call
    /// </summary>
    [HttpPost("{callId}/reject")]
    public async Task<JsonModel> RejectVideoCall(Guid callId, [FromBody] RejectVideoCallDto rejectDto)
    {
        return await _videoCallService.RejectVideoCallAsync(callId, rejectDto.Reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Toggle video
    /// </summary>
    [HttpPost("{callId}/video")]
    public async Task<JsonModel> ToggleVideo(Guid callId, [FromBody] ToggleVideoDto toggleDto)
    {
        return await _videoCallService.ToggleVideoAsync(callId, toggleDto.Enabled, GetToken(HttpContext));
    }

    /// <summary>
    /// Toggle audio
    /// </summary>
    [HttpPost("{callId}/audio")]
    public async Task<JsonModel> ToggleAudio(Guid callId, [FromBody] ToggleAudioDto toggleDto)
    {
        return await _videoCallService.ToggleAudioAsync(callId, toggleDto.Enabled, GetToken(HttpContext));
    }

    /// <summary>
    /// Start screen sharing
    /// </summary>
    [HttpPost("{callId}/screen-sharing/start")]
    public async Task<JsonModel> StartScreenSharing(Guid callId)
    {
        return await _videoCallService.StartScreenSharingAsync(callId, GetToken(HttpContext));
    }

    /// <summary>
    /// Stop screen sharing
    /// </summary>
    [HttpPost("{callId}/screen-sharing/stop")]
    public async Task<JsonModel> StopScreenSharing(Guid callId)
    {
        return await _videoCallService.StopScreenSharingAsync(callId, GetToken(HttpContext));
    }

    /// <summary>
    /// Update call quality
    /// </summary>
    [HttpPost("{callId}/quality")]
    public async Task<JsonModel> UpdateCallQuality(Guid callId, [FromBody] UpdateCallQualityDto qualityDto)
    {
        return await _videoCallService.UpdateCallQualityAsync(
            callId, 
            qualityDto.AudioQuality ?? 0, 
            qualityDto.VideoQuality ?? 0, 
            qualityDto.NetworkQuality ?? 0, 
            GetToken(HttpContext));
    }

    /// <summary>
    /// Get video call participants
    /// </summary>
    [HttpGet("{callId}/participants")]
    public async Task<JsonModel> GetVideoCallParticipants(Guid callId)
    {
        return await _videoCallService.GetVideoCallParticipantsAsync(callId, GetToken(HttpContext));
    }

    /// <summary>
    /// Log video call event
    /// </summary>
    [HttpPost("{callId}/events")]
    public async Task<JsonModel> LogVideoCallEvent(Guid callId, [FromBody] LogVideoCallEventDto eventDto)
    {
        return await _videoCallService.LogVideoCallEventAsync(callId, eventDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get current user name from claims
    /// </summary>
    private string GetCurrentUserName()
    {
        var userNameClaim = HttpContext.User.FindFirst(ClaimTypes.Name);
        return userNameClaim?.Value ?? "Unknown User";
    }
}

// DTOs for video call operations
public class CreateVideoSessionDto
{
    public string SessionName { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
    public Guid? ConsultationId { get; set; }
}

public class GenerateTokenDto
{
    public OpenTokRole Role { get; set; } = OpenTokRole.Publisher;
    public DateTime? ExpireTime { get; set; }
}

public class StartRecordingDto
{
    public string Name { get; set; } = string.Empty;
    public bool HasAudio { get; set; } = true;
    public bool HasVideo { get; set; } = true;
    public OpenTokRecordingOutputMode OutputMode { get; set; } = OpenTokRecordingOutputMode.Composed;
    public string? Resolution { get; set; } = "1280x720";
    public string? Layout { get; set; }
    public int? MaxDuration { get; set; }
}

public class StartBroadcastDto
{
    public string Name { get; set; } = string.Empty;
    public string HlsUrl { get; set; } = string.Empty;
    public string? RtmpUrl { get; set; }
    public int? MaxDuration { get; set; }
    public string? Resolution { get; set; } = "1280x720";
    public string? Layout { get; set; }
} 
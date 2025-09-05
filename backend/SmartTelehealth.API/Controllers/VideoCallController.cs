using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive video call management and OpenTok integration.
/// This controller provides extensive functionality for video call sessions, OpenTok integration,
/// video call lifecycle management, participant management, and video call quality monitoring.
/// It handles the complete video call workflow from session creation to call completion.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VideoCallController : BaseController
{
    private readonly IOpenTokService _openTokService;
    private readonly IConsultationService _consultationService;
    private readonly IVideoCallService _videoCallService;

    /// <summary>
    /// Initializes a new instance of the VideoCallController with required services.
    /// </summary>
    /// <param name="openTokService">Service for OpenTok video conferencing integration</param>
    /// <param name="consultationService">Service for consultation-related operations</param>
    /// <param name="videoCallService">Service for video call management operations</param>
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
    /// Creates a new OpenTok video call session for video conferencing.
    /// This endpoint creates a new video session with OpenTok integration including
    /// session configuration, archiving settings, and session metadata setup.
    /// </summary>
    /// <param name="createDto">DTO containing video session creation details</param>
    /// <returns>JsonModel containing the created session information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates new OpenTok video session with configuration
    /// - Sets up session archiving and metadata
    /// - Configures session for video conferencing
    /// - Access restricted to authenticated users
    /// - Used for video call session creation and setup
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on session creation
    /// - Maintains session audit trails and creation history
    /// </remarks>
    [HttpPost("sessions")]
    public async Task<JsonModel> CreateSession([FromBody] CreateVideoSessionDto createDto)
    {
        return await _openTokService.CreateSessionAsync(createDto.SessionName, createDto.IsArchived, GetToken(HttpContext));
    }

    /// <summary>
    /// Generates an OpenTok token for joining a video session.
    /// This endpoint creates a secure token that allows users to join a specific
    /// video session with appropriate permissions and role-based access control.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the video session</param>
    /// <param name="generateDto">DTO containing token generation details and role</param>
    /// <returns>JsonModel containing the generated token information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Generates secure OpenTok token for session access
    /// - Sets up role-based permissions and access control
    /// - Configures token expiration and user identification
    /// - Access restricted to authenticated users
    /// - Used for video session access and participant management
    /// - Includes comprehensive validation and error handling
    /// - Provides secure token generation and access control
    /// - Maintains token audit trails and access history
    /// </remarks>
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
    /// Retrieves detailed information about a specific video session.
    /// This endpoint provides comprehensive session details including session status,
    /// participant information, session configuration, and session metadata.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the video session</param>
    /// <returns>JsonModel containing the session information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed video session information
    /// - Includes session status, participants, and configuration
    /// - Shows session metadata and OpenTok session details
    /// - Access restricted to session participants and authorized users
    /// - Used for session information retrieval and management
    /// - Includes comprehensive session information and metadata
    /// - Provides secure access to session information
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("sessions/{sessionId}")]
    public async Task<JsonModel> GetSession(string sessionId)
    {
        return await _openTokService.GetSessionAsync(sessionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Archives a video session for later retrieval and analysis.
    /// This endpoint archives the video session recording and metadata for
    /// compliance, quality assurance, and session review purposes.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the video session to archive</param>
    /// <returns>JsonModel containing the archive operation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Archives video session with recording and metadata
    /// - Stores session data for compliance and review
    /// - Updates session status and archive information
    /// - Access restricted to session participants and administrators
    /// - Used for session archiving and compliance management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on archive operations
    /// - Maintains session audit trails and archive history
    /// </remarks>
    [HttpPost("sessions/{sessionId}/archive")]
    public async Task<JsonModel> ArchiveSession(string sessionId)
    {
        return await _openTokService.ArchiveSessionAsync(sessionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a video session and all associated data.
    /// This endpoint permanently removes the video session including recordings,
    /// metadata, and all associated session data from the system.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the video session to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Deletes video session with all associated data
    /// - Removes session recordings and metadata
    /// - Cleans up session resources and storage
    /// - Access restricted to session owners and administrators
    /// - Used for session cleanup and data management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on deletion operations
    /// - Maintains session audit trails and deletion history
    /// </remarks>
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
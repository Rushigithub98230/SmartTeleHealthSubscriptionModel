using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Hubs;

[Authorize]
public class VideoCallHub : Hub
{
    private readonly IVideoCallService _videoCallService;
    private readonly IChatStorageService _chatStorageService;
    private readonly IOpenTokService _openTokService;
    private readonly ILogger<VideoCallHub> _logger;
    private static readonly Dictionary<string, string> _userConnections = new();
    private static readonly Dictionary<string, HashSet<string>> _callGroups = new();
    private static readonly Dictionary<string, VideoCallDto> _activeCalls = new();

    private TokenModel GetTokenModel()
    {
        var userId = GetUserId();
        return new TokenModel
        {
            UserID = userId,
            RoleID = 0 // Default role
        };
    }

    public VideoCallHub(
        IVideoCallService videoCallService,
        IChatStorageService chatStorageService,
        IOpenTokService openTokService,
        ILogger<VideoCallHub> logger)
    {
        _videoCallService = videoCallService;
        _chatStorageService = chatStorageService;
        _openTokService = openTokService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != 0)
        {
            _userConnections[userId.ToString()] = Context.ConnectionId;
            _logger.LogInformation("User {UserId} connected to video call hub", userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != 0)
        {
            _userConnections.Remove(userId.ToString());
            _logger.LogInformation("User {UserId} disconnected from video call hub", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Initiate a video call
    public async Task InitiateVideoCall(string chatRoomId, string callType = "OneOnOne")
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            // Validate chat room access
            var hasAccess = await _chatStorageService.ValidateChatAccessAsync(userId, chatRoomId);
            if (!hasAccess)
            {
                await Clients.Caller.SendAsync("CallAccessDenied", "You don't have access to this chat room");
                return;
            }

            var createCallDto = new CreateVideoCallDto
            {
                ChatRoomId = Guid.Parse(chatRoomId),
                Type = Enum.Parse<VideoCallType>(callType),
                IsVideoEnabled = true,
                IsAudioEnabled = true
            };

            var result = await _videoCallService.InitiateVideoCallAsync(createCallDto, GetTokenModel());
            
                    if (result.StatusCode == 200 && result.data != null)
        {
            var call = (VideoCallDto)result.data;
                _activeCalls[call.CallId.ToString()] = call;

                // Join the call group
                await Groups.AddToGroupAsync(Context.ConnectionId, call.CallId.ToString());
                
                if (!_callGroups.ContainsKey(call.CallId.ToString()))
                    _callGroups[call.CallId.ToString()] = new HashSet<string>();
                
                _callGroups[call.CallId.ToString()].Add(Context.ConnectionId);

                // Notify all participants in the chat room
                await Clients.Group(chatRoomId.ToString()).SendAsync("VideoCallInitiated", call);
                await Clients.Caller.SendAsync("CallInitiated", call);

                // Log the event
                await _videoCallService.LogVideoCallEventAsync(call.Id, new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = SmartTelehealth.Core.Entities.VideoCallEventType.CallInitiated,
                    Description = $"Video call initiated by {GetUserName()}"
                }, GetTokenModel());
            }
            else
            {
                await Clients.Caller.SendAsync("CallFailed", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating video call in chat room {ChatRoomId}", chatRoomId);
            await Clients.Caller.SendAsync("CallFailed", "Failed to initiate video call");
        }
    }

    // Join a video call
    public async Task JoinVideoCall(string callId)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _videoCallService.JoinVideoCallAsync(Guid.Parse(callId), userId, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, callId.ToString());
                
                if (!_callGroups.ContainsKey(callId.ToString()))
                    _callGroups[callId.ToString()] = new HashSet<string>();
                
                _callGroups[callId.ToString()].Add(Context.ConnectionId);

                await Clients.Caller.SendAsync("JoinedVideoCall", callId);
                await Clients.OthersInGroup(callId.ToString()).SendAsync("ParticipantJoined", callId, userId, GetUserName());

                // Log the event
                await _videoCallService.LogVideoCallEventAsync(Guid.Parse(callId), new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = SmartTelehealth.Core.Entities.VideoCallEventType.ParticipantJoined,
                    Description = $"{GetUserName()} joined the call"
                }, GetTokenModel());
            }
            else
            {
                await Clients.Caller.SendAsync("CallJoinFailed", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining video call {CallId}", callId);
            await Clients.Caller.SendAsync("CallJoinFailed", "Failed to join video call");
        }
    }

    // Leave a video call
    public async Task LeaveVideoCall(string callId)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _videoCallService.LeaveVideoCallAsync(Guid.Parse(callId), userId, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, callId.ToString());
                
                if (_callGroups.ContainsKey(callId.ToString()))
                    _callGroups[callId.ToString()].Remove(Context.ConnectionId);

                await Clients.OthersInGroup(callId.ToString()).SendAsync("ParticipantLeft", callId, userId, GetUserName());

                // Log the event
                await _videoCallService.LogVideoCallEventAsync(Guid.Parse(callId), new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = SmartTelehealth.Core.Entities.VideoCallEventType.ParticipantLeft,
                    Description = $"{GetUserName()} left the call"
                }, GetTokenModel());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving video call {CallId}", callId);
        }
    }

    // End a video call
    public async Task EndVideoCall(string callId, string? reason = null)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _videoCallService.EndVideoCallAsync(Guid.Parse(callId), reason, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                await Clients.Group(callId.ToString()).SendAsync("VideoCallEnded", callId, reason);
                
                if (_callGroups.ContainsKey(callId.ToString()))
                    _callGroups.Remove(callId.ToString());
                
                if (_activeCalls.ContainsKey(callId.ToString()))
                    _activeCalls.Remove(callId.ToString());

                // Log the event
                await _videoCallService.LogVideoCallEventAsync(Guid.Parse(callId), new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = SmartTelehealth.Core.Entities.VideoCallEventType.CallDisconnected,
                    Description = $"Call ended by {GetUserName()}"
                }, GetTokenModel());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending video call {CallId}", callId);
        }
    }

    // Reject a video call
    public async Task RejectVideoCall(string callId, string reason)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _videoCallService.RejectVideoCallAsync(Guid.Parse(callId), reason, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                await Clients.Group(callId.ToString()).SendAsync("VideoCallRejected", callId, userId, reason);
                
                // Log the event
                await _videoCallService.LogVideoCallEventAsync(Guid.Parse(callId), new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = SmartTelehealth.Core.Entities.VideoCallEventType.CallRejected,
                    Description = $"Call rejected by {GetUserName()}: {reason}"
                }, GetTokenModel());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting video call {CallId}", callId);
        }
    }

    // Toggle video
    public async Task ToggleVideo(string callId, bool enabled)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _videoCallService.ToggleVideoAsync(Guid.Parse(callId), enabled, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                await Clients.OthersInGroup(callId.ToString()).SendAsync("VideoToggled", callId, userId, enabled);

                // Log the event
                await _videoCallService.LogVideoCallEventAsync(Guid.Parse(callId), new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = enabled ? SmartTelehealth.Core.Entities.VideoCallEventType.VideoEnabled : SmartTelehealth.Core.Entities.VideoCallEventType.VideoDisabled,
                    Description = $"{GetUserName()} {(enabled ? "enabled" : "disabled")} video"
                }, GetTokenModel());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling video for call {CallId}", callId);
        }
    }

    // Toggle audio
    public async Task ToggleAudio(string callId, bool enabled)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _videoCallService.ToggleAudioAsync(Guid.Parse(callId), enabled, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                await Clients.OthersInGroup(callId.ToString()).SendAsync("AudioToggled", callId, userId, enabled);

                // Log the event
                await _videoCallService.LogVideoCallEventAsync(Guid.Parse(callId), new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = enabled ? SmartTelehealth.Core.Entities.VideoCallEventType.AudioEnabled : SmartTelehealth.Core.Entities.VideoCallEventType.AudioDisabled,
                    Description = $"{GetUserName()} {(enabled ? "enabled" : "disabled")} audio"
                }, GetTokenModel());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling audio for call {CallId}", callId);
        }
    }

    // Start screen sharing
    public async Task StartScreenSharing(string callId)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _videoCallService.StartScreenSharingAsync(Guid.Parse(callId), GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                await Clients.OthersInGroup(callId.ToString()).SendAsync("ScreenSharingStarted", callId, userId);

                // Log the event
                await _videoCallService.LogVideoCallEventAsync(Guid.Parse(callId), new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = SmartTelehealth.Core.Entities.VideoCallEventType.ScreenSharingStarted,
                    Description = $"{GetUserName()} started screen sharing"
                }, GetTokenModel());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting screen sharing for call {CallId}", callId);
        }
    }

    // Stop screen sharing
    public async Task StopScreenSharing(string callId)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _videoCallService.StopScreenSharingAsync(Guid.Parse(callId), GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                await Clients.OthersInGroup(callId.ToString()).SendAsync("ScreenSharingStopped", callId, userId);

                // Log the event
                await _videoCallService.LogVideoCallEventAsync(Guid.Parse(callId), new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = SmartTelehealth.Core.Entities.VideoCallEventType.ScreenSharingStopped,
                    Description = $"{GetUserName()} stopped screen sharing"
                }, GetTokenModel());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping screen sharing for call {CallId}", callId);
        }
    }

    // Update call quality
    public async Task UpdateCallQuality(string callId, int audioQuality, int videoQuality, int networkQuality)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _videoCallService.UpdateCallQualityAsync(Guid.Parse(callId), audioQuality, videoQuality, networkQuality, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                // Log the event
                await _videoCallService.LogVideoCallEventAsync(Guid.Parse(callId), new LogVideoCallEventDto
                {
                    UserId = Guid.Empty, // TODO: Convert int UserId to Guid when DTOs are updated
                    Type = SmartTelehealth.Core.Entities.VideoCallEventType.QualityChanged,
                    Description = $"Call quality updated: Audio={audioQuality}, Video={videoQuality}, Network={networkQuality}"
                }, GetTokenModel());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating call quality for call {CallId}", callId);
        }
    }

    // Get call participants
    public async Task GetCallParticipants(string callId)
    {
        try
        {
            var result = await _videoCallService.GetVideoCallParticipantsAsync(Guid.Parse(callId), GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                await Clients.Caller.SendAsync("CallParticipants", callId, result.data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participants for call {CallId}", callId);
        }
    }

    // Send WebRTC signaling
    public async Task SendSignaling(string callId, string targetUserId, string signalType, string signalData)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            // Forward the signaling to the target user
            if (_userConnections.ContainsKey(targetUserId))
            {
                await Clients.Client(_userConnections[targetUserId]).SendAsync("Signaling", callId, userId.ToString(), signalType, signalData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending signaling for call {CallId}", callId);
        }
    }

    // OpenTok specific methods
    public async Task<string> GetOpenTokToken(string sessionId, string userName)
    {
        var userId = GetUserId();
        if (userId == 0) return string.Empty;

        try
        {
            var result = await _openTokService.GenerateTokenAsync(sessionId, userId.ToString(), userName, DateTime.UtcNow.AddHours(24), OpenTokRole.Publisher, GetTokenModel());
            if (result.StatusCode == 200)
            {
                return (string)result.data;
            }
            else
            {
                _logger.LogError("Failed to generate OpenTok token: {Error}", result.Message);
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OpenTok token for session {SessionId}", sessionId);
            return string.Empty;
        }
    }

    public async Task<OpenTokSessionDto> CreateOpenTokSession(string sessionName)
    {
        try
        {
            var result = await _openTokService.CreateSessionAsync(sessionName, true, GetTokenModel()); // Enable archiving for HIPAA compliance
            if (result.StatusCode == 200)
            {
                return (OpenTokSessionDto)result.data;
            }
            else
            {
                _logger.LogError("Failed to create OpenTok session: {Error}", result.Message);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OpenTok session {SessionName}", sessionName);
            return null;
        }
    }

    public async Task<bool> StartOpenTokRecording(string sessionId, string recordingName)
    {
        try
        {
            var options = new OpenTokRecordingOptions
            {
                Name = recordingName,
                HasAudio = true,
                HasVideo = true,
                OutputMode = OpenTokRecordingOutputMode.Composed,
                Resolution = "1280x720",
                Storage = "cloud"
            };

            var result = await _openTokService.StartRecordingAsync(sessionId, options, GetTokenModel());
            return result.StatusCode == 200;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting OpenTok recording for session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> StopOpenTokRecording(string recordingId)
    {
        try
        {
            var result = await _openTokService.StopRecordingAsync(recordingId, GetTokenModel());
            return result.StatusCode == 200;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping OpenTok recording {RecordingId}", recordingId);
            return false;
        }
    }

    public async Task<string> GetOpenTokRecordingUrl(string recordingId)
    {
        try
        {
            var result = await _openTokService.GetRecordingUrlAsync(recordingId, GetTokenModel());
            return result.StatusCode == 200 ? (string)result.data : string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OpenTok recording URL {RecordingId}", recordingId);
            return string.Empty;
        }
    }

    // Private methods
    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetUserName()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
    }
} 
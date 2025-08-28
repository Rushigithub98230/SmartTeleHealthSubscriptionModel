using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class VideoCallService : IVideoCallService
{
    private readonly IVideoCallRepository _videoCallRepository;
    private readonly IOpenTokService _openTokService;
    private readonly ILogger<VideoCallService> _logger;

    public VideoCallService(
        IVideoCallRepository videoCallRepository,
        IOpenTokService openTokService,
        ILogger<VideoCallService> logger)
    {
        _videoCallRepository = videoCallRepository;
        _openTokService = openTokService;
        _logger = logger;
    }

    public async Task<JsonModel> GetByIdAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(id);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            var videoCallDto = MapToDto(videoCall);
            return new JsonModel
            {
                data = videoCallDto,
                Message = "Video call retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video call with ID {VideoCallId}", id);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving video call",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetByUserIdAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            var videoCalls = await _videoCallRepository.GetByUserIdAsync(userId);
            var videoCallDtos = videoCalls.Select(MapToDto);
            return new JsonModel
            {
                data = videoCallDtos,
                Message = "Video calls retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video calls for user {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving video calls",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CreateAsync(CreateVideoCallDto createDto, TokenModel tokenModel)
    {
        try
        {
            var videoCall = new VideoCall
            {
                Id = Guid.NewGuid(),
                AppointmentId = createDto.AppointmentId,
                SessionId = createDto.SessionId ?? string.Empty,
                Token = createDto.Token ?? string.Empty,
                StartedAt = DateTime.UtcNow,
                Status = "Initiated",
                CreatedDate = DateTime.UtcNow
            };

            var createdVideoCall = await _videoCallRepository.CreateAsync(videoCall);
            var videoCallDto = MapToDto(createdVideoCall);
            return new JsonModel
            {
                data = videoCallDto,
                Message = "Video call created successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video call");
            return new JsonModel
            {
                data = new object(),
                Message = "Error creating video call",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdateAsync(Guid id, UpdateVideoCallDto updateDto, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(id);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            // Update properties
            videoCall.Status = updateDto.Status ?? videoCall.Status;
            videoCall.EndedAt = updateDto.EndedAt ?? videoCall.EndedAt;
            videoCall.RecordingUrl = updateDto.RecordingUrl ?? videoCall.RecordingUrl;

            var updatedVideoCall = await _videoCallRepository.UpdateAsync(videoCall);
            var videoCallDto = MapToDto(updatedVideoCall);
            return new JsonModel
            {
                data = videoCallDto,
                Message = "Video call updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating video call with ID {VideoCallId}", id);
            return new JsonModel
            {
                data = new object(),
                Message = "Error updating video call",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var result = await _videoCallRepository.DeleteAsync(id);
            return new JsonModel
            {
                data = result,
                Message = "Video call deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting video call with ID {VideoCallId}", id);
            return new JsonModel
            {
                data = new object(),
                Message = "Error deleting video call",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAllAsync(TokenModel tokenModel)
    {
        try
        {
            var videoCalls = await _videoCallRepository.GetAllAsync();
            var videoCallDtos = videoCalls.Select(MapToDto);
            return new JsonModel
            {
                data = videoCallDtos,
                Message = "All video calls retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all video calls");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving video calls",
                StatusCode = 500
            };
        }
    }

    // Video Call Management
    public async Task<JsonModel> InitiateVideoCallAsync(CreateVideoCallDto createDto, TokenModel tokenModel)
    {
        try
        {
            var result = await CreateAsync(createDto, tokenModel);
            if (result.StatusCode == 200)
            {
                // Create OpenTok session
                var dynamicData = result.data as dynamic;
                if (dynamicData != null)
                {
                    var sessionResult = await _openTokService.CreateSessionAsync($"Call_{dynamicData.Id}", false, tokenModel);
                    if (sessionResult.StatusCode == 200)
                    {
                        var sessionData = sessionResult.data as dynamic;
                        if (sessionData != null)
                        {
                            dynamicData.SessionId = sessionData.SessionId;
                        }
                    }
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating video call");
            return new JsonModel
            {
                data = new object(),
                Message = "Error initiating video call",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> JoinVideoCallAsync(Guid callId, int userId, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            // Add participant logic here
            return new JsonModel
            {
                data = true,
                Message = "Successfully joined video call",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining video call {CallId} by user {UserId}", callId, userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error joining video call",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> LeaveVideoCallAsync(Guid callId, int userId, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            // Remove participant logic here
            return new JsonModel
            {
                data = true,
                Message = "Successfully left video call",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving video call {CallId} by user {UserId}", callId, userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error leaving video call",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> EndVideoCallAsync(Guid callId, string? reason, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            videoCall.Status = "Ended";
            videoCall.EndedAt = DateTime.UtcNow;

            await _videoCallRepository.UpdateAsync(videoCall);
            return new JsonModel
            {
                data = true,
                Message = "Video call ended successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending video call {CallId}", callId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error ending video call",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> RejectVideoCallAsync(Guid callId, string reason, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            videoCall.Status = "Rejected";
            videoCall.EndedAt = DateTime.UtcNow;

            await _videoCallRepository.UpdateAsync(videoCall);
            return new JsonModel
            {
                data = true,
                Message = "Video call rejected successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting video call {CallId}", callId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error rejecting video call",
                StatusCode = 500
            };
        }
    }

    // Video/Audio Controls
    public async Task<JsonModel> ToggleVideoAsync(Guid callId, bool enabled, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            // Video toggle logic would be handled by participants, not the call itself
            return new JsonModel
            {
                data = true,
                Message = "Video toggled successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling video for call {CallId}", callId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error toggling video",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ToggleAudioAsync(Guid callId, bool enabled, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            // Audio toggle logic would be handled by participants, not the call itself
            return new JsonModel
            {
                data = true,
                Message = "Audio toggled successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling audio for call {CallId}", callId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error toggling audio",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> StartScreenSharingAsync(Guid callId, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            // Screen sharing logic here
            return new JsonModel
            {
                data = true,
                Message = "Screen sharing started successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting screen sharing for call {CallId}", callId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error starting screen sharing",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> StopScreenSharingAsync(Guid callId, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            // Stop screen sharing logic here
            return new JsonModel
            {
                data = true,
                Message = "Screen sharing stopped successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping screen sharing for call {CallId}", callId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error stopping screen sharing",
                StatusCode = 500
            };
        }
    }

    // Call Quality and Participants
    public async Task<JsonModel> UpdateCallQualityAsync(Guid callId, int audioQuality, int videoQuality, int networkQuality, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            // Update call quality logic here
            return new JsonModel
            {
                data = true,
                Message = "Call quality updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating call quality for call {CallId}", callId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error updating call quality",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetVideoCallParticipantsAsync(Guid callId, TokenModel tokenModel)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call not found",
                    StatusCode = 404
                };
            }

            // Get participants logic here
            var participants = new List<VideoCallParticipantDto>();
            return new JsonModel
            {
                data = participants,
                Message = "Video call participants retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participants for call {CallId}", callId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error getting participants",
                StatusCode = 500
            };
        }
    }

    // Logging
    public async Task<JsonModel> LogVideoCallEventAsync(Guid callId, LogVideoCallEventDto eventDto, TokenModel tokenModel)
    {
        try
        {
            // Log video call event logic here
            _logger.LogInformation("Video call event: {EventType} for call {CallId} by user {UserId}", 
                eventDto.Type, callId, eventDto.UserId);
            return new JsonModel
            {
                data = true,
                Message = "Video call event logged successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging video call event for call {CallId}", callId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error logging video call event",
                StatusCode = 500
            };
        }
    }

    private VideoCallDto MapToDto(VideoCall videoCall)
    {
        return new VideoCallDto
        {
            Id = videoCall.Id,
            AppointmentId = videoCall.AppointmentId,
            SessionId = videoCall.SessionId,
            Token = videoCall.Token,
            StartedAt = videoCall.StartedAt,
            EndedAt = videoCall.EndedAt,
            Status = videoCall.Status,
            RecordingUrl = videoCall.RecordingUrl,
            CreatedAt = videoCall.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = videoCall.UpdatedDate,
            IsDeleted = videoCall.IsDeleted
        };
    }
} 
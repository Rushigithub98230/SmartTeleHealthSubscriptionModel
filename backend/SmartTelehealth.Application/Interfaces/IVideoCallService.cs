using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IVideoCallService
{
    Task<JsonModel> GetByIdAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetByUserIdAsync(int userId, TokenModel tokenModel);
    Task<JsonModel> CreateAsync(CreateVideoCallDto createDto, TokenModel tokenModel);
    Task<JsonModel> UpdateAsync(Guid id, UpdateVideoCallDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetAllAsync(TokenModel tokenModel);
    
    // Video Call Management
    Task<JsonModel> InitiateVideoCallAsync(CreateVideoCallDto createDto, TokenModel tokenModel);
    Task<JsonModel> JoinVideoCallAsync(Guid callId, int userId, TokenModel tokenModel);
    Task<JsonModel> LeaveVideoCallAsync(Guid callId, int userId, TokenModel tokenModel);
    Task<JsonModel> EndVideoCallAsync(Guid callId, string? reason, TokenModel tokenModel);
    Task<JsonModel> RejectVideoCallAsync(Guid callId, string reason, TokenModel tokenModel);
    
    // Video/Audio Controls
    Task<JsonModel> ToggleVideoAsync(Guid callId, bool enabled, TokenModel tokenModel);
    Task<JsonModel> ToggleAudioAsync(Guid callId, bool enabled, TokenModel tokenModel);
    Task<JsonModel> StartScreenSharingAsync(Guid callId, TokenModel tokenModel);
    Task<JsonModel> StopScreenSharingAsync(Guid callId, TokenModel tokenModel);
    
    // Call Quality and Participants
    Task<JsonModel> UpdateCallQualityAsync(Guid callId, int audioQuality, int videoQuality, int networkQuality, TokenModel tokenModel);
    Task<JsonModel> GetVideoCallParticipantsAsync(Guid callId, TokenModel tokenModel);
    
    // Logging
    Task<JsonModel> LogVideoCallEventAsync(Guid callId, LogVideoCallEventDto eventDto, TokenModel tokenModel);
} 
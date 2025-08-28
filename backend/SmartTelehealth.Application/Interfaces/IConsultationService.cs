using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IConsultationService
{
    // CRUD Operations
    Task<JsonModel> CreateConsultationAsync(CreateConsultationDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetConsultationByIdAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetProviderConsultationsAsync(Guid providerId, TokenModel tokenModel);
    Task<JsonModel> GetUpcomingConsultationsAsync(TokenModel tokenModel);
    Task<JsonModel> UpdateConsultationAsync(Guid id, UpdateConsultationDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteConsultationAsync(Guid id, TokenModel tokenModel);
    
    // Consultation Management
    Task<JsonModel> CancelConsultationAsync(Guid id, string reason, TokenModel tokenModel);
    Task<JsonModel> StartConsultationAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> CompleteConsultationAsync(Guid id, string notes, TokenModel tokenModel);
    Task<JsonModel> RescheduleConsultationAsync(Guid id, DateTime newScheduledAt, TokenModel tokenModel);
    Task<JsonModel> MarkNoShowAsync(Guid id, TokenModel tokenModel);
    
    // OpenTok Integration
    Task<JsonModel> GenerateMeetingUrlAsync(Guid consultationId, TokenModel tokenModel);
    Task<JsonModel> JoinMeetingAsync(Guid consultationId, string participantId, string role, TokenModel tokenModel);
    Task<JsonModel> LeaveMeetingAsync(Guid consultationId, string participantId, TokenModel tokenModel);
    Task<JsonModel> StartRecordingAsync(Guid consultationId, TokenModel tokenModel);
    Task<JsonModel> StopRecordingAsync(Guid consultationId, TokenModel tokenModel);
    Task<JsonModel> GetRecordingsAsync(Guid consultationId, TokenModel tokenModel);
    
    // Consultation Analytics
    Task<JsonModel> GetConsultationAnalyticsAsync(Guid providerId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetConsultationsByDateRangeAsync(DateTime startDate, DateTime endDate, TokenModel tokenModel);
    Task<JsonModel> CalculateProviderRevenueAsync(Guid providerId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    
    // One-Time Consultations
    Task<JsonModel> CreateOneTimeConsultationAsync(CreateOneTimeConsultationDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessOneTimePaymentAsync(Guid consultationId, string paymentMethodId, TokenModel tokenModel);
    
    // Follow-up Management
    Task<JsonModel> GetFollowUpConsultationsAsync(Guid userId, TokenModel tokenModel);
    Task<JsonModel> ScheduleFollowUpAsync(Guid consultationId, DateTime followUpDate, TokenModel tokenModel);
    Task<JsonModel> CancelFollowUpAsync(Guid consultationId, TokenModel tokenModel);
    Task<JsonModel> GetUserOneTimeConsultationsAsync(int userId, TokenModel tokenModel);
    
    // User Consultations
    Task<JsonModel> GetUserConsultationsAsync(int userId, TokenModel tokenModel);
} 
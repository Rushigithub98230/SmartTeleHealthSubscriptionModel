using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IAppointmentService
{
    // --- APPOINTMENT MANAGEMENT ---
    Task<JsonModel> CreateAppointmentAsync(CreateAppointmentDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetAppointmentByIdAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetPatientAppointmentsAsync(int patientId, TokenModel tokenModel);
    Task<JsonModel> GetProviderAppointmentsAsync(int providerId, TokenModel tokenModel);
    Task<JsonModel> GetPendingAppointmentsAsync(TokenModel tokenModel);
    Task<JsonModel> UpdateAppointmentAsync(Guid id, UpdateAppointmentDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteAppointmentAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> BookAppointmentAsync(BookAppointmentDto bookDto, TokenModel tokenModel);
    Task<JsonModel> BookAppointmentAsync(AppointmentBookingDto bookingDto, TokenModel tokenModel);
    Task<JsonModel> ProcessPaymentAsync(Guid appointmentId, ProcessPaymentDto paymentDto, TokenModel tokenModel);
    Task<JsonModel> ConfirmPaymentAsync(Guid appointmentId, string paymentIntentId, TokenModel tokenModel);
    Task<JsonModel> ProviderActionAsync(Guid appointmentId, string action, string? notes, TokenModel tokenModel);

    // --- DOCUMENT MANAGEMENT (Updated to use centralized DocumentService) ---
    Task<JsonModel> UploadDocumentAsync(Guid appointmentId, UploadDocumentDto uploadDto, TokenModel tokenModel);
    Task<JsonModel> GetAppointmentDocumentsAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> DeleteDocumentAsync(Guid documentId, TokenModel tokenModel);

    // --- PARTICIPANT MANAGEMENT ---
    Task<JsonModel> AddParticipantAsync(Guid appointmentId, int? userId, string? email, string? phone, Guid participantRoleId, int invitedByUserId, TokenModel tokenModel);
    Task<JsonModel> InviteExternalAsync(Guid appointmentId, string email, string? phone, string? message, int invitedByUserId, TokenModel tokenModel);
    Task<JsonModel> MarkParticipantJoinedAsync(Guid appointmentId, int? userId, string? email, TokenModel tokenModel);
    Task<JsonModel> MarkParticipantLeftAsync(Guid appointmentId, int? userId, string? email, TokenModel tokenModel);
    Task<JsonModel> GetParticipantsAsync(Guid appointmentId, TokenModel tokenModel);

    // --- VIDEO CALL MANAGEMENT ---
    Task<string> GetOrCreateVideoSessionAsync(Guid appointmentId, TokenModel tokenModel);
    Task<string> GenerateVideoTokenAsync(Guid appointmentId, int? userId, string? email, Guid participantRoleId, TokenModel tokenModel);
    Task<JsonModel> GetOpenTokTokenAsync(Guid appointmentId, int userId, TokenModel tokenModel);
    Task<JsonModel> GenerateMeetingLinkAsync(Guid appointmentId, TokenModel tokenModel);

    // --- PAYMENT MANAGEMENT ---
    Task<JsonModel> CreatePaymentLogAsync(Guid appointmentId, int userId, decimal amount, string paymentMethod, string? paymentIntentId, string? sessionId, TokenModel tokenModel);
    Task<JsonModel> UpdatePaymentStatusAsync(Guid paymentLogId, Guid paymentStatusId, string? failureReason, TokenModel tokenModel);
    Task<JsonModel> ProcessRefundAsync(Guid appointmentId, decimal refundAmount, string reason, TokenModel tokenModel);
    Task<JsonModel> GetPaymentLogsAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> GetPaymentStatusAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> CapturePaymentAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> RefundPaymentAsync(Guid appointmentId, decimal? amount, TokenModel tokenModel);

    // --- PROVIDER ACTIONS ---
    Task<JsonModel> ProviderAcceptAppointmentAsync(Guid appointmentId, ProviderAcceptDto acceptDto, TokenModel tokenModel);
    Task<JsonModel> ProviderRejectAppointmentAsync(Guid appointmentId, ProviderRejectDto rejectDto, TokenModel tokenModel);
    Task<JsonModel> StartMeetingAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> EndMeetingAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> CompleteAppointmentAsync(Guid appointmentId, CompleteAppointmentDto completeDto, TokenModel tokenModel);
    Task<JsonModel> CancelAppointmentAsync(Guid appointmentId, string reason, TokenModel tokenModel);

    // --- RECORDING MANAGEMENT ---
    Task<JsonModel> StartRecordingAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> StopRecordingAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> GetRecordingUrlAsync(Guid appointmentId, TokenModel tokenModel);

    // --- REMINDER MANAGEMENT ---
    Task<JsonModel> ScheduleReminderAsync(Guid appointmentId, ScheduleReminderDto reminderDto, TokenModel tokenModel);
    Task<JsonModel> GetAppointmentRemindersAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> SendReminderAsync(Guid reminderId, TokenModel tokenModel);

    // --- EVENT LOGGING ---
    Task<JsonModel> LogAppointmentEventAsync(Guid appointmentId, LogAppointmentEventDto eventDto, TokenModel tokenModel);
    Task<JsonModel> GetAppointmentEventsAsync(Guid appointmentId, TokenModel tokenModel);

    // --- AVAILABILITY MANAGEMENT ---
    Task<JsonModel> GetProviderAvailabilityAsync(Guid providerId, DateTime date, TokenModel tokenModel);
    Task<JsonModel> CheckProviderAvailabilityAsync(Guid providerId, DateTime startTime, DateTime endTime, TokenModel tokenModel);

    // --- SUBSCRIPTION & BILLING ---
    Task<JsonModel> ValidateSubscriptionAccessAsync(Guid patientId, Guid categoryId, TokenModel tokenModel);
    Task<JsonModel> CalculateAppointmentFeeAsync(int patientId, int providerId, Guid categoryId, TokenModel tokenModel);
    Task<JsonModel> ApplySubscriptionDiscountAsync(Guid appointmentId, TokenModel tokenModel);

    // --- SYSTEM OPERATIONS ---
    Task<JsonModel> ProcessExpiredAppointmentsAsync(TokenModel tokenModel);
    Task<JsonModel> AutoCancelAppointmentAsync(Guid appointmentId, TokenModel tokenModel);
    Task<JsonModel> GetAppointmentAnalyticsAsync(DateTime startDate, DateTime endDate, TokenModel tokenModel);
    Task<JsonModel> IsAppointmentServiceHealthyAsync(TokenModel tokenModel);

    // --- ADDITIONAL QUERIES ---
    Task<JsonModel> GetAppointmentsByStatusAsync(Guid appointmentStatusId, TokenModel tokenModel);
    Task<JsonModel> GetUpcomingAppointmentsAsync(TokenModel tokenModel);
    Task<JsonModel> GetCategoriesWithSubscriptionsAsync(TokenModel tokenModel);
    Task<JsonModel> GetFeaturedProvidersAsync(TokenModel tokenModel);

    // --- HOMEPAGE & HOME DATA ---
    Task<JsonModel> GetHomepageDataAsync(TokenModel tokenModel);
    Task<JsonModel> GetHomeDataAsync(TokenModel tokenModel);

    // --- CHAT MANAGEMENT ---
    Task SendAppointmentChatMessageAsync(Guid appointmentId, MessageDto message, TokenModel tokenModel);
} 
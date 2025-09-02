using SmartTelehealth.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class AppointmentDto
{
    public string Id { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string ConsultationId { get; set; } = string.Empty;
    public Guid AppointmentTypeId { get; set; }
    public Guid ConsultationModeId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? Symptoms { get; set; }
    public string? PatientNotes { get; set; }
    public decimal Fee { get; set; }
    public bool IsRecordingEnabled { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid AppointmentStatusId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public string? ProviderNotes { get; set; }
    public string? FollowUpInstructions { get; set; }
    public string? OpenTokSessionId { get; set; }
    public string? MeetingUrl { get; set; }
    public string? MeetingId { get; set; }
    public bool IsVideoCallStarted { get; set; }
    public bool IsVideoCallEnded { get; set; }
    public string? RecordingId { get; set; }
    public string? RecordingUrl { get; set; }
    public bool IsPatientNotified { get; set; }
    public bool IsProviderNotified { get; set; }
    public DateTime? LastReminderSent { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    
    // Additional properties for service compatibility
    public string Status { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string AppointmentTypeName { get; set; } = string.Empty;
    public string ConsultationModeName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string ProviderEmail { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public string AppointmentStatusName { get; set; } = string.Empty;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public string StripeSessionId { get; set; } = string.Empty;
    public bool IsPaymentCaptured { get; set; }
    public bool IsRefunded { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? AutoCancellationAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCancelled { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool IsExpired { get; set; }
    public int DocumentCount { get; set; }
    public int ReminderCount { get; set; }
    public int EventCount { get; set; }
}

public class CreateAppointmentDto
{
    public string PatientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string ConsultationId { get; set; } = string.Empty;
    public Guid AppointmentTypeId { get; set; }
    public Guid ConsultationModeId { get; set; }
    public string ScheduledAt { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? Symptoms { get; set; }
    public string? PatientNotes { get; set; }
    public decimal Fee { get; set; }
    public bool IsRecordingEnabled { get; set; }
    public string? ExpiresAt { get; set; }
}

public class UpdateAppointmentDto
{
    public Guid? AppointmentStatusId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public string? ProviderNotes { get; set; }
    public string? FollowUpInstructions { get; set; }
    public string? OpenTokSessionId { get; set; }
    public string? MeetingUrl { get; set; }
    public string? MeetingId { get; set; }
    public bool? IsVideoCallStarted { get; set; }
    public bool? IsVideoCallEnded { get; set; }
    public string? RecordingId { get; set; }
    public string? RecordingUrl { get; set; }
    public bool? IsPatientNotified { get; set; }
    public bool? IsProviderNotified { get; set; }
    public DateTime? LastReminderSent { get; set; }
}

public class AppointmentParticipantDto
{
    public string Id { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Name { get; set; }
    public Guid ParticipantRoleId { get; set; }
    public string ParticipantRoleName { get; set; } = string.Empty;
    public bool HasJoined { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public string? InvitedByUserId { get; set; }
    public DateTime CreatedDate { get; set; }
    // Added properties
    public string? UserName { get; set; }
    public string? ExternalEmail { get; set; }
    public string? ExternalPhone { get; set; }
    public Guid? ParticipantStatusId { get; set; }
    public string? ParticipantStatusName { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public string? InvitedByUserName { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class AppointmentInvitationDto
{
    public string Id { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Name { get; set; }
    public string? Message { get; set; }
    public string? InvitedByUserId { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedDate { get; set; }
    // Added properties
    public string? InvitedByUserName { get; set; }
    public string? InvitedUserId { get; set; }
    public string? InvitedUserName { get; set; }
    public string? InvitedEmail { get; set; }
    public string? InvitedPhone { get; set; }
    public Guid? InvitationStatusId { get; set; }
    public string? InvitationStatusName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public class AppointmentPaymentLogDto
{
    public string Id { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public Guid PaymentStatusId { get; set; }
    public string PaymentStatusName { get; set; } = string.Empty;
    public string? PaymentIntentId { get; set; }
    public string? SessionId { get; set; }
    public string? FailureReason { get; set; }
    public decimal? RefundedAmount { get; set; }
    public string? RefundId { get; set; }
    public string? RefundReason { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? RefundDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    // Added properties
    public string? UserName { get; set; }
    public Guid? RefundStatusId { get; set; }
    public string? RefundStatusName { get; set; }
    public string? Description { get; set; }
}

public class BookAppointmentDto
{
    public string PatientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public Guid AppointmentTypeId { get; set; }
    public Guid ConsultationModeId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string ReasonForVisit { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? PatientNotes { get; set; }
    public bool IsUrgent { get; set; } = false;
    public string? PaymentMethodId { get; set; }
    public bool CapturePaymentImmediately { get; set; } = false;
    // Add these for compatibility with controller
    public string? Type { get; set; }
    public string? Mode { get; set; }
}

public class ProcessPaymentDto
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? SessionId { get; set; }
    public string SubscriptionId { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class ProviderAcceptDto
{
    public string? Notes { get; set; }
    public string? ProviderNotes { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
}

public class ProviderRejectDto
{
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class CompleteAppointmentDto
{
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public string? ProviderNotes { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateTime? EndedAt { get; set; }
}



public class CategoryWithSubscriptionsDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<SubscriptionDto> Subscriptions { get; set; } = new();
}

public class FeaturedProviderDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    // public decimal ConsultationFee { get; set; }
}

public class ProviderAvailabilityDto
{
    public string Id { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}

public class PaymentStatusDto
{
    public string AppointmentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PaymentIntentId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? FailureReason { get; set; }
}

public class AppointmentDocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class UploadDocumentDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileContent { get; set; } = string.Empty; // Base64 encoded
}

public class AppointmentReminderDto
{
    public string Id { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
}

public class ScheduleReminderDto
{
    public DateTime ScheduledAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class LogAppointmentEventDto
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AppointmentEventDto
{
    public string Id { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class WeeklyScheduleDto
{
    public string Id { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
}

public class ProviderScheduleDto
{
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<TimeSlotDto> AvailableSlots { get; set; } = new();
    public List<TimeSlotDto> BookedSlots { get; set; } = new();
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
    public WeeklyScheduleDto WeeklySchedule { get; set; } = new();
    public List<DateTime> AvailableDates { get; set; } = new();
    public List<DateTime> UnavailableDates { get; set; } = new();
    public int DefaultDurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}

public class TimeSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsBooked { get; set; }
    public string? AppointmentId { get; set; }
    public string? PatientName { get; set; }
    public Guid? AppointmentStatusId { get; set; }
    public string? AppointmentStatusName { get; set; }
}

public class AppointmentBookingDto
{
    public string PatientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string? SubscriptionId { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public Guid ConsultationModeId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string ReasonForVisit { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? PatientNotes { get; set; }
    public bool IsUrgent { get; set; } = false;
    public string? PaymentMethodId { get; set; }
    public bool CapturePaymentImmediately { get; set; } = false;
    // Add these for compatibility with controller
    public string? Type { get; set; }
    public string? Mode { get; set; }
}

public class AppointmentConfirmationDto
{
    public string AppointmentId { get; set; } = string.Empty;
    public string AppointmentNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public decimal Fee { get; set; }
    public string? MeetingUrl { get; set; }
    public string? OpenTokSessionId { get; set; }
    public Guid AppointmentStatusId { get; set; }
    public string AppointmentStatusName { get; set; } = string.Empty;
    public string? StripePaymentIntentId { get; set; }
    public bool IsPaymentCaptured { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Status { get; set; } // For compatibility
}

public class InviteExternalDto
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Name { get; set; }
    public string? Message { get; set; }
    public int? InvitedByUserId { get; set; }
}

public class JoinAppointmentDto
{
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}

public class LeaveAppointmentDto
{
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public string? Reason { get; set; }
}

public class AddParticipantDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string InvitedByUserId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Message { get; set; }
}

public class AppointmentSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid AppointmentStatusId { get; set; }
    public string AppointmentStatusName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public decimal Fee { get; set; }
    public bool IsVideoCallStarted { get; set; }
    public bool IsVideoCallEnded { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class AppointmentDashboardDto
{
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int ApprovedAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public List<AppointmentSummaryDto> RecentAppointments { get; set; } = new();
    public List<AppointmentSummaryDto> UpcomingAppointments { get; set; } = new();
    public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
    public Dictionary<string, int> AppointmentsByCategory { get; set; } = new();
}

public class ConfirmPaymentDto
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ProcessRefundDto
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RefundAmount { get; set; }
    public string? Reason { get; set; }
}

 
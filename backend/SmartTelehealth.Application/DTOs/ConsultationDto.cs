using System;

namespace SmartTelehealth.Application.DTOs;

public class ConsultationDto
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsOneTime { get; set; } = false;
    public string? CategoryId { get; set; }
    public string? ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal? Fee { get; set; }
    public string? ConsultationMode { get; set; }
    public string? Reason { get; set; }
    public string? Symptoms { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateConsultationDto
{
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsOneTime { get; set; } = false;
    public string? CategoryId { get; set; }
    public string? ProviderId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal? Fee { get; set; }
    public string? ConsultationMode { get; set; }
    public string? Reason { get; set; }
    public string? Symptoms { get; set; }
}

public class UpdateConsultationDto
{
    public DateTime? ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Reason { get; set; }
    public string? Symptoms { get; set; }
    public string? PatientNotes { get; set; }
    public string? ConsultationMode { get; set; }
    public decimal? Fee { get; set; }
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public string? ProviderNotes { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
} 
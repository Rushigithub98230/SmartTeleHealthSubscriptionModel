using System;

namespace SmartTelehealth.Application.DTOs;

public class HealthAssessmentDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string AssessmentData { get; set; } = string.Empty;
    public int AssessmentScore { get; set; }
    public string ProviderNotes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class CreateHealthAssessmentDto
{
    public string PatientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string AssessmentType { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? MedicalHistory { get; set; }
    public string? CurrentMedications { get; set; }
    public string? Allergies { get; set; }
    public string? LifestyleFactors { get; set; }
    public DateTime ScheduledAt { get; set; }
    public decimal Fee { get; set; }
}

public class UpdateHealthAssessmentDto
{
    public string? AssessmentType { get; set; }
    public string? Symptoms { get; set; }
    public string? MedicalHistory { get; set; }
    public string? CurrentMedications { get; set; }
    public string? Allergies { get; set; }
    public string? LifestyleFactors { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public decimal? Fee { get; set; }
    public string? AssessmentResults { get; set; }
    public string? Recommendations { get; set; }
    public string? ProviderNotes { get; set; }
    public DateTime? CompletedAt { get; set; }
} 
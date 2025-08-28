using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class HealthAssessment : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public enum AssessmentStatus
    {
        Pending,
        InProgress,
        Completed,
        Reviewed,
        Cancelled
    }
    
    // Foreign keys
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    public int? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }
    
    // Assessment details
    public AssessmentStatus Status { get; set; } = AssessmentStatus.InProgress;
    
    [MaxLength(1000)]
    public string? Symptoms { get; set; }
    
    [MaxLength(1000)]
    public string? MedicalHistory { get; set; }
    
    [MaxLength(1000)]
    public string? CurrentMedications { get; set; }
    
    [MaxLength(1000)]
    public string? Allergies { get; set; }
    
    [MaxLength(1000)]
    public string? LifestyleFactors { get; set; }
    
    [MaxLength(1000)]
    public string? FamilyHistory { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public DateTime? ReviewedAt { get; set; }
    
    [MaxLength(1000)]
    public string? ProviderNotes { get; set; }
    
    [MaxLength(500)]
    public string? RejectionReason { get; set; }
    
    public bool IsEligibleForTreatment { get; set; }
    
    public bool RequiresFollowUp { get; set; }
    
    // Navigation properties
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
} 
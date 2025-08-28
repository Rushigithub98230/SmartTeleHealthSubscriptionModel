using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class Category : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Icon { get; set; }
    
    [MaxLength(100)]
    public string? Color { get; set; }
    
    public int DisplayOrder { get; set; }
    
    [MaxLength(1000)]
    public string? Features { get; set; } // JSON string of features
    
    [MaxLength(500)]
    public string? ConsultationDescription { get; set; }
    
    public decimal BasePrice { get; set; }
    
    public decimal ConsultationFee { get; set; }
    
    public int ConsultationDurationMinutes { get; set; } = 30;
    
    public bool RequiresHealthAssessment { get; set; } = true;
    
    public bool AllowsMedicationDelivery { get; set; } = true;
    
    public bool AllowsFollowUpMessaging { get; set; } = true;
    
    public bool AllowsOneTimeConsultation { get; set; } = true;
    
    public decimal OneTimeConsultationFee { get; set; }
    
    public int OneTimeConsultationDurationMinutes { get; set; } = 30;
    
    // Marketing and display properties
    public bool IsMostPopular { get; set; } = false;
    
    public bool IsTrending { get; set; } = false;
    
    // Optional: Add navigation property for plans by billing cycle
    public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
    public virtual ICollection<ProviderCategory> ProviderCategories { get; set; } = new List<ProviderCategory>();
    public virtual ICollection<HealthAssessment> HealthAssessments { get; set; } = new List<HealthAssessment>();
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
} 
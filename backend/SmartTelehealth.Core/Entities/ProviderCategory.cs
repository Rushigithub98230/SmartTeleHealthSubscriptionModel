using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class ProviderCategory : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    public int ProviderId { get; set; }
    public virtual Provider Provider { get; set; } = null!;
    
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    // Provider category specific details
    public bool IsPrimary { get; set; } = false;
    
    public int YearsOfExperience { get; set; }
    
    public decimal ConsultationFee { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    public DateTime? AvailableFrom { get; set; }
    
    public DateTime? AvailableTo { get; set; }
    
    [MaxLength(500)]
    public string? Specialization { get; set; }
    
    public int DisplayOrder { get; set; }
} 
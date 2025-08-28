using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class Provider : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string LicenseNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    [MaxLength(100)]
    public string? Education { get; set; }
    
    [MaxLength(500)]
    public string? Certifications { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    public TimeSpan? AvailableFrom { get; set; }
    
    public TimeSpan? AvailableTo { get; set; }
    
    public decimal ConsultationFee { get; set; }
    
    // Navigation properties
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<ProviderCategory> ProviderCategories { get; set; } = new List<ProviderCategory>();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
} 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class ProviderFee : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public int ProviderId { get; set; }
    public virtual User Provider { get; set; } = null!;
    
    [Required]
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    [Required]
    [Range(0, 10000)]
    public decimal ProposedFee { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal ApprovedFee { get; set; }
    
    public FeeStatus Status { get; set; } = FeeStatus.Pending;
    
    [MaxLength(1000)]
    public string? AdminRemarks { get; set; }
    
    [MaxLength(1000)]
    public string? ProviderNotes { get; set; }
    
    public DateTime? ProposedAt { get; set; }
    
    public DateTime? ReviewedAt { get; set; }
    
    public int? ReviewedByUserId { get; set; }
    public virtual User? ReviewedByUser { get; set; }
    
    // Alias properties for backward compatibility
    public DateTime? CreatedAt { get => CreatedDate; set => CreatedDate = value; }
    public DateTime? UpdatedAt { get => UpdatedDate; set => UpdatedDate = value; }
}

public enum FeeStatus
{
    Pending = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    RequiresMoreInfo = 4
} 
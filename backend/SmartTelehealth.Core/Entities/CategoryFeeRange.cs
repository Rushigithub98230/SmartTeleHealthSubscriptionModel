using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class CategoryFeeRange : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    [Required]
    [Range(0, 10000)]
    public decimal MinimumFee { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal MaximumFee { get; set; }
    
    [Required]
    [Range(0, 100)]
    public decimal PlatformCommission { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    

} 
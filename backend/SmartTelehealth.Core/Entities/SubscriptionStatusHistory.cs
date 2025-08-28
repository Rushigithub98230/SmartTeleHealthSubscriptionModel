using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class SubscriptionStatusHistory : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;
    
    [MaxLength(50)]
    public string? FromStatus { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ToStatus { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    public int? ChangedByUserId { get; set; }
    public virtual User? ChangedByUser { get; set; }
    
    [Required]
    public DateTime ChangedAt { get; set; }
    
    [MaxLength(1000)]
    public string? Metadata { get; set; }
    
    // Computed properties (not mapped to database)
    [NotMapped]
    public bool IsStatusChange => !string.IsNullOrEmpty(FromStatus) && FromStatus != ToStatus;

    // Alias properties for backward compatibility - now using BaseEntity properties
    public DateTime? CreatedAt { get => CreatedDate; set => CreatedDate = value; }
    public DateTime? UpdatedAt { get => UpdatedDate; set => UpdatedDate = value; }
    
    [NotMapped]
    public TimeSpan DurationInPreviousStatus => FromStatus != null ? ChangedAt - CreatedDate.GetValueOrDefault() : TimeSpan.Zero;
} 
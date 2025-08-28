using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class MessageReadReceipt : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    public Guid MessageId { get; set; }
    public virtual Message Message { get; set; } = null!;

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }

    // Read receipt details
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    // Device information for audit
    [MaxLength(100)]
    public string? DeviceInfo { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }
} 
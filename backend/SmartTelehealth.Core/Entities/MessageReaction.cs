using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class MessageReaction : BaseEntity
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

    // Reaction details
    [Required]
    [MaxLength(10)]
    public string Emoji { get; set; } = string.Empty;
} 
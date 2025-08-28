using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class DocumentReference : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Primary identification
    public Guid DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    
    // Reference information
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty; // e.g., "Appointment", "User", "Chat"
    
    public Guid EntityId { get; set; } // ID of the referenced entity
    
    [MaxLength(100)]
    public string? ReferenceType { get; set; } // e.g., "profile_picture", "medical_report", "chat_attachment"
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Access control
    public bool IsPublic { get; set; } = false;
    
    public DateTime? ExpiresAt { get; set; }
} 
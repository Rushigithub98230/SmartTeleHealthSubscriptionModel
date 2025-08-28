using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    public class AuditLog : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? EntityId { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [MaxLength(100)]
        public string? UserEmail { get; set; }

        [MaxLength(50)]
        public string? UserRole { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } // Success, Failed, Pending

        [MaxLength(1000)]
        public string? OldValues { get; set; }

        [MaxLength(1000)]
        public string? NewValues { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Alias properties for backward compatibility
        public DateTime? CreatedAt { get => CreatedDate; set => CreatedDate = value; }
        public DateTime? UpdatedAt { get => UpdatedDate; set => UpdatedDate = value; }
    }
} 
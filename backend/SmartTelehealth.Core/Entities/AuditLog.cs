using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? AffectedColumns { get; set; }
        public string? PrimaryKey { get; set; }
        public int? OrganizationId { get; set; }
    }
} 
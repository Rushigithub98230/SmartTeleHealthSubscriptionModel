using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? Description { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAuditLogDto
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        public string EntityType { get; set; } = string.Empty;
        
        public string? EntityId { get; set; }
        public string? Description { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class AuditLogSearchDto
    {
        public int? UserId { get; set; }
        public string? Action { get; set; }
        public string? EntityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? SearchTerm { get; set; }
    }
} 
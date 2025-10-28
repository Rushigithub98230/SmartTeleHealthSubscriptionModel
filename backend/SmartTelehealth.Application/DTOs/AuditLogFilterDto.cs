namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Filter DTO for audit log queries.
/// </summary>
public class AuditLogFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? Type { get; set; }       // Changed to List<string>
    public List<string>? TableName { get; set; }  // Changed to List<string>
    public string? EntityId { get; set; }
    public int? UserId { get; set; }
    public string? SearchText { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
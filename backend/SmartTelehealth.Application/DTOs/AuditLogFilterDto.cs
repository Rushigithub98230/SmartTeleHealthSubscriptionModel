namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Filter DTO for audit log queries.
/// </summary>
public class AuditLogFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Type { get; set; }
    public string? TableName { get; set; }
    public string? EntityId { get; set; }
    public int? UserId { get; set; }
    public string? SearchText { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
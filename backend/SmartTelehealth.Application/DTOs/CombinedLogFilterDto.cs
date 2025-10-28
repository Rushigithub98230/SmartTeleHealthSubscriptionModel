namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Filter DTO for combined log queries (both application and audit logs).
/// </summary>
public class CombinedLogFilterDto
{
    public string LogType { get; set; } = "application"; // "application" or "audit"
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Application log filters
    public List<string>? LogLevel { get; set; }  // Changed to List<string>
    public List<string>? Source { get; set; }     // Changed to List<string>
    
    // Audit log filters
    public List<string>? Type { get; set; }       // Changed to List<string>
    public List<string>? TableName { get; set; }  // Changed to List<string>
    public string? EntityId { get; set; }
    
    // Common filters
    public int? UserId { get; set; }
    public string? SearchText { get; set; }
    
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}


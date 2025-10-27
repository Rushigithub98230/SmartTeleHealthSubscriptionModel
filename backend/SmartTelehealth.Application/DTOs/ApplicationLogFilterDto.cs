namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Filter DTO for application log queries.
/// </summary>
public class ApplicationLogFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? LogLevel { get; set; }
    public string? Source { get; set; }
    public int? UserId { get; set; }
    public string? SearchText { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}


namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Data Transfer Object for ApplicationLog entity.
/// Used for transferring application log information between layers.
/// </summary>
public class ApplicationLogDto
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string LogLevel { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Operation { get; set; }
    public string? AdditionalData { get; set; }
    public string? CorrelationId { get; set; }
}


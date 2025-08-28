namespace SmartTelehealth.Application.DTOs;

public class ProviderActionDto
{
    public string Action { get; set; } = string.Empty; // "approve", "reject", "complete"
    public string? Notes { get; set; }
} 
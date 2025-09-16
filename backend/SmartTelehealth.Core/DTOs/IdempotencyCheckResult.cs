using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.DTOs;

/// <summary>
/// Result of an idempotency check
/// </summary>
public class IdempotencyCheckResult
{
    public bool ShouldProcess { get; set; }
    public bool IsNewEvent { get; set; }
    public ProcessedWebhookEvent? WebhookEvent { get; set; }
    public string? Reason { get; set; }
}

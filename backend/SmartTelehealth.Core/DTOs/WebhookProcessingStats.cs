namespace SmartTelehealth.Core.DTOs;

/// <summary>
/// Statistics about webhook event processing
/// </summary>
public class WebhookProcessingStats
{
    public int TotalEvents { get; set; }
    public int SuccessfulEvents { get; set; }
    public int FailedEvents { get; set; }
    public int PermanentlyFailedEvents { get; set; }
    public int RetryableEvents { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public Dictionary<string, int> EventTypes { get; set; } = new();
}


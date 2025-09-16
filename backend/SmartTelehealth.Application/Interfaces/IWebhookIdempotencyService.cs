using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Interface for webhook idempotency service to ensure webhook events are processed only once
/// </summary>
public interface IWebhookIdempotencyService
{
    /// <summary>
    /// Checks if a webhook event should be processed based on idempotency rules
    /// </summary>
    /// <param name="eventId">The Stripe event ID</param>
    /// <param name="eventType">The Stripe event type</param>
    /// <returns>IdempotencyCheckResult indicating whether to process, skip, or retry</returns>
    Task<IdempotencyCheckResult> CheckIdempotencyAsync(string eventId, string eventType);

    /// <summary>
    /// Marks a webhook event as successfully processed
    /// </summary>
    /// <param name="stripeEventId">The Stripe event ID</param>
    /// <param name="processingDurationMs">Processing duration in milliseconds</param>
    /// <param name="metadata">Optional metadata about the processing</param>
    Task MarkAsProcessedAsync(string stripeEventId, long? processingDurationMs = null, string? metadata = null);

    /// <summary>
    /// Marks a webhook event as failed processing
    /// </summary>
    /// <param name="stripeEventId">The Stripe event ID</param>
    /// <param name="errorMessage">Error message describing the failure</param>
    /// <param name="maxRetries">Maximum number of retries allowed</param>
    Task MarkAsFailedAsync(string stripeEventId, string errorMessage, int maxRetries = 3);

    /// <summary>
    /// Gets webhook processing statistics
    /// </summary>
    /// <param name="hours">Number of hours to look back for statistics</param>
    /// <returns>WebhookProcessingStats containing processing statistics</returns>
    Task<WebhookProcessingStats> GetProcessingStatsAsync(int hours = 24);
}


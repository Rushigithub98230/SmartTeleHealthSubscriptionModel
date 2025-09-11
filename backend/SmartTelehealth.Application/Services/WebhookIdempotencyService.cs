using Microsoft.Extensions.Logging;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services
{
    /// <summary>
    /// Service responsible for ensuring webhook idempotency and preventing duplicate processing
    /// </summary>
    public class WebhookIdempotencyService
    {
        private readonly IProcessedWebhookEventRepository _webhookEventRepository;
        private readonly ILogger<WebhookIdempotencyService> _logger;

        public WebhookIdempotencyService(
            IProcessedWebhookEventRepository webhookEventRepository,
            ILogger<WebhookIdempotencyService> logger)
        {
            _webhookEventRepository = webhookEventRepository ?? throw new ArgumentNullException(nameof(webhookEventRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks if a webhook event should be processed based on idempotency rules
        /// </summary>
        /// <param name="eventId">The Stripe event ID</param>
        /// <param name="eventType">The Stripe event type</param>
        /// <returns>IdempotencyCheckResult indicating whether to process, skip, or retry</returns>
        public async Task<IdempotencyCheckResult> CheckIdempotencyAsync(string eventId, string eventType)
        {
            try
            {
                _logger.LogInformation("Checking idempotency for webhook event {EventId} of type {EventType}", eventId, eventType);

                // Check if this event has already been processed
                var existingEvent = await _webhookEventRepository.GetByStripeEventIdAsync(eventId);

                if (existingEvent == null)
                {
                    // New event - create tracking record
                    var newEvent = new ProcessedWebhookEvent
                    {
                        Id = Guid.NewGuid(),
                        StripeEventId = eventId,
                        EventType = eventType,
                        ReceivedAt = DateTime.UtcNow,
                        IsSuccess = false,
                        RetryCount = 0,
                        MaxRetries = 3
                    };

                    await _webhookEventRepository.CreateAsync(newEvent);
                    _logger.LogInformation("Created new webhook event tracking record for {EventId}", eventId);

                    return new IdempotencyCheckResult
                    {
                        ShouldProcess = true,
                        IsNewEvent = true,
                        WebhookEvent = newEvent
                    };
                }

                // Event already exists - check its status
                if (existingEvent.IsSuccess)
                {
                    _logger.LogInformation("Webhook event {EventId} already processed successfully - skipping", eventId);
                    return new IdempotencyCheckResult
                    {
                        ShouldProcess = false,
                        IsNewEvent = false,
                        WebhookEvent = existingEvent,
                        Reason = "Already processed successfully"
                    };
                }

                if (existingEvent.IsPermanentlyFailed)
                {
                    _logger.LogWarning("Webhook event {EventId} has permanently failed after {RetryCount} attempts - skipping", 
                        eventId, existingEvent.RetryCount);
                    return new IdempotencyCheckResult
                    {
                        ShouldProcess = false,
                        IsNewEvent = false,
                        WebhookEvent = existingEvent,
                        Reason = "Permanently failed"
                    };
                }

                if (existingEvent.ShouldRetry)
                {
                    _logger.LogInformation("Webhook event {EventId} failed previously but should be retried (attempt {RetryCount}/{MaxRetries})", 
                        eventId, existingEvent.RetryCount + 1, existingEvent.MaxRetries);
                    return new IdempotencyCheckResult
                    {
                        ShouldProcess = true,
                        IsNewEvent = false,
                        WebhookEvent = existingEvent,
                        Reason = "Retry attempt"
                    };
                }

                // This shouldn't happen, but handle gracefully
                _logger.LogWarning("Webhook event {EventId} in unexpected state - processing anyway", eventId);
                return new IdempotencyCheckResult
                {
                    ShouldProcess = true,
                    IsNewEvent = false,
                    WebhookEvent = existingEvent,
                    Reason = "Unexpected state"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking idempotency for webhook event {EventId}", eventId);
                // In case of error, allow processing to prevent blocking legitimate events
                return new IdempotencyCheckResult
                {
                    ShouldProcess = true,
                    IsNewEvent = true,
                    WebhookEvent = null,
                    Reason = "Idempotency check failed - allowing processing"
                };
            }
        }

        /// <summary>
        /// Marks a webhook event as successfully processed
        /// </summary>
        /// <param name="stripeEventId">The Stripe event ID</param>
        /// <param name="processingDurationMs">Processing duration in milliseconds</param>
        /// <param name="metadata">Additional metadata about the processing</param>
        public async Task MarkAsProcessedAsync(string stripeEventId, long? processingDurationMs = null, string? metadata = null)
        {
            try
            {
                var success = await _webhookEventRepository.MarkEventAsProcessedAsync(stripeEventId, processingDurationMs, metadata);
                if (success)
                {
                    _logger.LogInformation("Marked webhook event {EventId} as successfully processed", stripeEventId);
                }
                else
                {
                    _logger.LogWarning("Failed to mark webhook event {EventId} as processed - event not found", stripeEventId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking webhook event {EventId} as processed", stripeEventId);
            }
        }

        /// <summary>
        /// Marks a webhook event as failed and increments retry count
        /// </summary>
        /// <param name="stripeEventId">The Stripe event ID</param>
        /// <param name="errorMessage">Error message describing the failure</param>
        /// <param name="maxRetries">Maximum number of retries allowed</param>
        public async Task MarkAsFailedAsync(string stripeEventId, string errorMessage, int maxRetries = 3)
        {
            try
            {
                var success = await _webhookEventRepository.MarkEventAsFailedAsync(stripeEventId, errorMessage, maxRetries);
                if (success)
                {
                    _logger.LogWarning("Marked webhook event {EventId} as failed: {ErrorMessage}", stripeEventId, errorMessage);
                }
                else
                {
                    _logger.LogWarning("Failed to mark webhook event {EventId} as failed - event not found", stripeEventId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking webhook event {EventId} as failed", stripeEventId);
            }
        }

        /// <summary>
        /// Gets statistics about webhook event processing
        /// </summary>
        /// <param name="hours">Number of hours to look back</param>
        /// <returns>Webhook processing statistics</returns>
        public async Task<WebhookProcessingStats> GetProcessingStatsAsync(int hours = 24)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddHours(-hours);
                var endDate = DateTime.UtcNow;

                var allEvents = await _webhookEventRepository.GetEventsByTypeAsync(null, startDate, endDate);
                var eventsList = allEvents.ToList();

                return new WebhookProcessingStats
                {
                    TotalEvents = eventsList.Count,
                    SuccessfulEvents = eventsList.Count(e => e.IsSuccess),
                    FailedEvents = eventsList.Count(e => !e.IsSuccess),
                    PermanentlyFailedEvents = eventsList.Count(e => e.IsPermanentlyFailed),
                    RetryableEvents = eventsList.Count(e => e.ShouldRetry),
                    AverageProcessingTimeMs = eventsList.Where(e => e.ProcessingDurationMs.HasValue)
                        .Average(e => e.ProcessingDurationMs.Value),
                    EventTypes = eventsList.GroupBy(e => e.EventType)
                        .ToDictionary(g => g.Key, g => g.Count())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webhook processing statistics");
                return new WebhookProcessingStats();
            }
        }
    }

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
}

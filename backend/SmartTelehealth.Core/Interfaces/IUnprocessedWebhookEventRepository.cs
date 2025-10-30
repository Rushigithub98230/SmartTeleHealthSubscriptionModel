using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces
{
    /// <summary>
    /// Repository interface for managing unprocessed webhook events
    /// </summary>
    public interface IUnprocessedWebhookEventRepository
    {
        /// <summary>
        /// Creates a new unprocessed webhook event record
        /// </summary>
        Task<UnprocessedWebhookEvent> CreateAsync(UnprocessedWebhookEvent unprocessedEvent);

        /// <summary>
        /// Gets an unprocessed webhook event by Stripe event ID
        /// </summary>
        Task<UnprocessedWebhookEvent?> GetByStripeEventIdAsync(string stripeEventId);

        /// <summary>
        /// Gets all events ready for retry processing
        /// </summary>
        Task<IEnumerable<UnprocessedWebhookEvent>> GetEventsReadyForRetryAsync();

        /// <summary>
        /// Gets events by Stripe subscription ID
        /// </summary>
        Task<IEnumerable<UnprocessedWebhookEvent>> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);

        /// <summary>
        /// Updates an unprocessed webhook event
        /// </summary>
        Task<UnprocessedWebhookEvent> UpdateAsync(UnprocessedWebhookEvent unprocessedEvent);

        /// <summary>
        /// Marks an event as completed
        /// </summary>
        Task MarkAsCompletedAsync(Guid eventId);

        /// <summary>
        /// Marks an event as failed and schedules next retry
        /// </summary>
        Task MarkAsFailedAsync(Guid eventId, string errorMessage, DateTime nextRetryAt);

        /// <summary>
        /// Marks an event as expired (max retries exceeded)
        /// </summary>
        Task MarkAsExpiredAsync(Guid eventId);

        /// <summary>
        /// Gets expired events for cleanup
        /// </summary>
        Task<IEnumerable<UnprocessedWebhookEvent>> GetExpiredEventsAsync();

        /// <summary>
        /// Deletes expired events
        /// </summary>
        Task DeleteExpiredEventsAsync();
    }
}


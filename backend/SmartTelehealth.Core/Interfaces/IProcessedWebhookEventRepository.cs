using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces
{
    /// <summary>
    /// Repository interface for managing processed webhook events to ensure idempotency
    /// </summary>
    public interface IProcessedWebhookEventRepository : IRepositoryBase<ProcessedWebhookEvent>
    {
        /// <summary>
        /// Retrieves a processed webhook event by Stripe event ID
        /// </summary>
        /// <param name="stripeEventId">The Stripe event ID to search for</param>
        /// <returns>The processed webhook event if found, null otherwise</returns>
        Task<ProcessedWebhookEvent?> GetByStripeEventIdAsync(string stripeEventId);

        /// <summary>
        /// Retrieves all failed webhook events that should be retried
        /// </summary>
        /// <returns>Collection of webhook events that should be retried</returns>
        Task<IEnumerable<ProcessedWebhookEvent>> GetFailedEventsForRetryAsync();

        /// <summary>
        /// Retrieves webhook events by type within a date range
        /// </summary>
        /// <param name="eventType">The event type to filter by</param>
        /// <param name="startDate">Start date for filtering</param>
        /// <param name="endDate">End date for filtering</param>
        /// <returns>Collection of webhook events matching the criteria</returns>
        Task<IEnumerable<ProcessedWebhookEvent>> GetEventsByTypeAsync(string eventType, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Retrieves webhook events that are permanently failed
        /// </summary>
        /// <param name="limit">Maximum number of events to return</param>
        /// <returns>Collection of permanently failed webhook events</returns>
        Task<IEnumerable<ProcessedWebhookEvent>> GetPermanentlyFailedEventsAsync(int limit = 100);

        /// <summary>
        /// Checks if a webhook event has already been processed successfully
        /// </summary>
        /// <param name="stripeEventId">The Stripe event ID to check</param>
        /// <returns>True if the event has been processed successfully, false otherwise</returns>
        Task<bool> IsEventProcessedAsync(string stripeEventId);

        /// <summary>
        /// Marks a webhook event as successfully processed
        /// </summary>
        /// <param name="stripeEventId">The Stripe event ID</param>
        /// <param name="processingDurationMs">Processing duration in milliseconds</param>
        /// <param name="metadata">Additional metadata about the processing</param>
        /// <returns>True if the event was marked as processed, false otherwise</returns>
        Task<bool> MarkEventAsProcessedAsync(string stripeEventId, long? processingDurationMs = null, string? metadata = null);

        /// <summary>
        /// Marks a webhook event as failed and increments retry count
        /// </summary>
        /// <param name="stripeEventId">The Stripe event ID</param>
        /// <param name="errorMessage">Error message describing the failure</param>
        /// <param name="maxRetries">Maximum number of retries allowed</param>
        /// <returns>True if the event was marked as failed, false otherwise</returns>
        Task<bool> MarkEventAsFailedAsync(string stripeEventId, string errorMessage, int maxRetries = 3);

        /// <summary>
        /// Cleans up old processed webhook events (for maintenance)
        /// </summary>
        /// <param name="olderThanDays">Delete events older than this many days</param>
        /// <returns>Number of events deleted</returns>
        Task<int> CleanupOldEventsAsync(int olderThanDays = 30);

        /// <summary>
        /// Retrieves processed webhook events with database-level filtering, pagination, and sorting
        /// </summary>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="eventType">Filter by event type</param>
        /// <param name="isSuccess">Filter by success status</param>
        /// <param name="search">Search term for filtering</param>
        /// <param name="startDate">Start date for filtering</param>
        /// <param name="endDate">End date for filtering</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="sortOrder">Sort order (asc/desc)</param>
        /// <returns>Tuple containing filtered events and total count</returns>
        Task<(IEnumerable<ProcessedWebhookEvent> Events, int TotalCount)> GetEventsWithFilteringAsync(
            int page, int pageSize, string? eventType = null, bool? isSuccess = null, 
            string? search = null, DateTime? startDate = null, DateTime? endDate = null, 
            string? sortBy = "ReceivedAt", string? sortOrder = "desc");
    }
}

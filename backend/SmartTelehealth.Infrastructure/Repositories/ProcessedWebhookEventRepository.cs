using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing processed webhook events
    /// </summary>
    public class ProcessedWebhookEventRepository : RepositoryBase<ProcessedWebhookEvent>, IProcessedWebhookEventRepository
    {
        private readonly ApplicationDbContext _context;

        public ProcessedWebhookEventRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a processed webhook event by Stripe event ID
        /// </summary>
        public async Task<ProcessedWebhookEvent?> GetByStripeEventIdAsync(string stripeEventId)
        {
            return await _context.ProcessedWebhookEvents
                .FirstOrDefaultAsync(e => e.StripeEventId == stripeEventId);
        }

        /// <summary>
        /// Retrieves all failed webhook events that should be retried
        /// </summary>
        public async Task<IEnumerable<ProcessedWebhookEvent>> GetFailedEventsForRetryAsync()
        {
            return await _context.ProcessedWebhookEvents
                .Where(e => !e.IsSuccess && e.RetryCount < e.MaxRetries)
                .OrderBy(e => e.ReceivedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves webhook events by type within a date range
        /// </summary>
        public async Task<IEnumerable<ProcessedWebhookEvent>> GetEventsByTypeAsync(string eventType, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ProcessedWebhookEvents
                .Where(e => e.EventType == eventType);

            if (startDate.HasValue)
            {
                query = query.Where(e => e.ReceivedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ReceivedAt <= endDate.Value);
            }

            return await query
                .OrderByDescending(e => e.ReceivedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves webhook events that are permanently failed
        /// </summary>
        public async Task<IEnumerable<ProcessedWebhookEvent>> GetPermanentlyFailedEventsAsync(int limit = 100)
        {
            return await _context.ProcessedWebhookEvents
                .Where(e => !e.IsSuccess && e.RetryCount >= e.MaxRetries)
                .OrderByDescending(e => e.LastAttemptAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a webhook event has already been processed successfully
        /// </summary>
        public async Task<bool> IsEventProcessedAsync(string stripeEventId)
        {
            return await _context.ProcessedWebhookEvents
                .AnyAsync(e => e.StripeEventId == stripeEventId && e.IsSuccess);
        }

        /// <summary>
        /// Marks a webhook event as successfully processed
        /// </summary>
        public async Task<bool> MarkEventAsProcessedAsync(string stripeEventId, long? processingDurationMs = null, string? metadata = null)
        {
            var webhookEvent = await GetByStripeEventIdAsync(stripeEventId);
            if (webhookEvent == null)
            {
                return false;
            }

            webhookEvent.IsSuccess = true;
            webhookEvent.ProcessedAt = DateTime.UtcNow;
            webhookEvent.ErrorMessage = null;
            webhookEvent.ProcessingDurationMs = processingDurationMs;
            webhookEvent.Metadata = metadata;

            await UpdateAsync(webhookEvent);
            return true;
        }

        /// <summary>
        /// Marks a webhook event as failed and increments retry count
        /// </summary>
        public async Task<bool> MarkEventAsFailedAsync(string stripeEventId, string errorMessage, int maxRetries = 3)
        {
            var webhookEvent = await GetByStripeEventIdAsync(stripeEventId);
            if (webhookEvent == null)
            {
                return false;
            }

            webhookEvent.IsSuccess = false;
            webhookEvent.RetryCount++;
            webhookEvent.LastAttemptAt = DateTime.UtcNow;
            webhookEvent.ErrorMessage = errorMessage;
            webhookEvent.MaxRetries = maxRetries;

            await UpdateAsync(webhookEvent);
            return true;
        }

        /// <summary>
        /// Cleans up old processed webhook events (for maintenance)
        /// </summary>
        public async Task<int> CleanupOldEventsAsync(int olderThanDays = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            var oldEvents = await _context.ProcessedWebhookEvents
                .Where(e => e.ReceivedAt < cutoffDate)
                .ToListAsync();

            if (oldEvents.Any())
            {
                _context.ProcessedWebhookEvents.RemoveRange(oldEvents);
                await _context.SaveChangesAsync();
            }

            return oldEvents.Count;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing unprocessed webhook events
    /// </summary>
    public class UnprocessedWebhookEventRepository : IUnprocessedWebhookEventRepository
    {
        private readonly ApplicationDbContext _context;

        public UnprocessedWebhookEventRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<UnprocessedWebhookEvent> CreateAsync(UnprocessedWebhookEvent unprocessedEvent)
        {
            _context.UnprocessedWebhookEvents.Add(unprocessedEvent);
            await _context.SaveChangesAsync();
            return unprocessedEvent;
        }

        public async Task<UnprocessedWebhookEvent?> GetByStripeEventIdAsync(string stripeEventId)
        {
            return await _context.UnprocessedWebhookEvents
                .FirstOrDefaultAsync(e => e.StripeEventId == stripeEventId && e.IsActive);
        }

        public async Task<IEnumerable<UnprocessedWebhookEvent>> GetEventsReadyForRetryAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.UnprocessedWebhookEvents
                .Where(e => e.IsActive && 
                           e.Status == UnprocessedWebhookEvent.ProcessingStatus.Pending &&
                           e.NextRetryAt <= now &&
                           e.RetryCount < e.MaxRetries)
                .OrderBy(e => e.NextRetryAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UnprocessedWebhookEvent>> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
        {
            return await _context.UnprocessedWebhookEvents
                .Where(e => e.StripeSubscriptionId == stripeSubscriptionId && e.IsActive)
                .OrderBy(e => e.ReceivedAt)
                .ToListAsync();
        }

        public async Task<UnprocessedWebhookEvent> UpdateAsync(UnprocessedWebhookEvent unprocessedEvent)
        {
            _context.UnprocessedWebhookEvents.Update(unprocessedEvent);
            await _context.SaveChangesAsync();
            return unprocessedEvent;
        }

        public async Task MarkAsCompletedAsync(Guid eventId)
        {
            var unprocessedEvent = await _context.UnprocessedWebhookEvents.FindAsync(eventId);
            if (unprocessedEvent != null)
            {
                unprocessedEvent.Status = UnprocessedWebhookEvent.ProcessingStatus.Completed;
                unprocessedEvent.LastProcessedAt = DateTime.UtcNow;
                unprocessedEvent.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAsFailedAsync(Guid eventId, string errorMessage, DateTime nextRetryAt)
        {
            var unprocessedEvent = await _context.UnprocessedWebhookEvents.FindAsync(eventId);
            if (unprocessedEvent != null)
            {
                unprocessedEvent.Status = UnprocessedWebhookEvent.ProcessingStatus.Pending;
                unprocessedEvent.RetryCount++;
                unprocessedEvent.NextRetryAt = nextRetryAt;
                unprocessedEvent.LastError = errorMessage;
                unprocessedEvent.LastProcessedAt = DateTime.UtcNow;
                
                // Mark as expired if max retries exceeded
                if (unprocessedEvent.RetryCount >= unprocessedEvent.MaxRetries)
                {
                    unprocessedEvent.Status = UnprocessedWebhookEvent.ProcessingStatus.Expired;
                    unprocessedEvent.IsActive = false;
                }
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAsExpiredAsync(Guid eventId)
        {
            var unprocessedEvent = await _context.UnprocessedWebhookEvents.FindAsync(eventId);
            if (unprocessedEvent != null)
            {
                unprocessedEvent.Status = UnprocessedWebhookEvent.ProcessingStatus.Expired;
                unprocessedEvent.IsActive = false;
                unprocessedEvent.LastProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<UnprocessedWebhookEvent>> GetExpiredEventsAsync()
        {
            return await _context.UnprocessedWebhookEvents
                .Where(e => e.Status == UnprocessedWebhookEvent.ProcessingStatus.Expired ||
                          (e.RetryCount >= e.MaxRetries && e.IsActive))
                .ToListAsync();
        }

        public async Task DeleteExpiredEventsAsync()
        {
            var expiredEvents = await GetExpiredEventsAsync();
            if (expiredEvents.Any())
            {
                _context.UnprocessedWebhookEvents.RemoveRange(expiredEvents);
                await _context.SaveChangesAsync();
            }
        }
    }
}


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Stripe;

namespace SmartTelehealth.Infrastructure.Services
{
    /// <summary>
    /// Background service for processing unprocessed webhook events
    /// Runs every 5 minutes to retry events that couldn't be processed initially
    /// </summary>
    public class UnprocessedWebhookRetryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnprocessedWebhookRetryService> _logger;
        private readonly TimeSpan _retryInterval = TimeSpan.FromMinutes(5);

        public UnprocessedWebhookRetryService(
            IServiceProvider serviceProvider,
            ILogger<UnprocessedWebhookRetryService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UnprocessedWebhookRetryService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessUnprocessedEventsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing unprocessed webhook events");
                }

                await Task.Delay(_retryInterval, stoppingToken);
            }

            _logger.LogInformation("UnprocessedWebhookRetryService stopped");
        }

        private async Task ProcessUnprocessedEventsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unprocessedEventRepository = scope.ServiceProvider.GetRequiredService<IUnprocessedWebhookEventRepository>();
            var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();

            try
            {
                var eventsToRetry = await unprocessedEventRepository.GetEventsReadyForRetryAsync();
                
                if (!eventsToRetry.Any())
                {
                    _logger.LogDebug("No unprocessed webhook events ready for retry");
                    return;
                }

                _logger.LogInformation("Processing {Count} unprocessed webhook events", eventsToRetry.Count());

                foreach (var unprocessedEvent in eventsToRetry)
                {
                    try
                    {
                        _logger.LogInformation("Retrying webhook event {EventId} (attempt {RetryCount}/{MaxRetries})", 
                            unprocessedEvent.Id, unprocessedEvent.RetryCount + 1, unprocessedEvent.MaxRetries);

                        // Mark as processing
                        unprocessedEvent.Status = UnprocessedWebhookEvent.ProcessingStatus.Processing;
                        await unprocessedEventRepository.UpdateAsync(unprocessedEvent);

                        // Reconstruct Stripe event from stored data
                        var stripeEvent = Event.FromJson(unprocessedEvent.EventData);

                        // Process the event based on its type
                        var success = await ProcessWebhookEventAsync(webhookService, stripeEvent, unprocessedEvent);

                        if (success)
                        {
                            // Mark as completed
                            await unprocessedEventRepository.MarkAsCompletedAsync(unprocessedEvent.Id);
                            _logger.LogInformation("Successfully processed webhook event {EventId}", unprocessedEvent.Id);
                        }
                        else
                        {
                            // Schedule next retry
                            var nextRetryAt = DateTime.UtcNow.AddMinutes(5);
                            await unprocessedEventRepository.MarkAsFailedAsync(unprocessedEvent.Id, "Processing failed", nextRetryAt);
                            _logger.LogWarning("Failed to process webhook event {EventId}, will retry at {NextRetryAt}", 
                                unprocessedEvent.Id, nextRetryAt);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing unprocessed webhook event {EventId}", unprocessedEvent.Id);
                        
                        // Schedule next retry
                        var nextRetryAt = DateTime.UtcNow.AddMinutes(5);
                        await unprocessedEventRepository.MarkAsFailedAsync(unprocessedEvent.Id, ex.Message, nextRetryAt);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unprocessed webhook events");
            }
        }

        private async Task<bool> ProcessWebhookEventAsync(IWebhookService webhookService, Event stripeEvent, UnprocessedWebhookEvent unprocessedEvent)
        {
            try
            {
                switch (unprocessedEvent.EventType)
                {
                    case "invoice.created":
                        await webhookService.HandleInvoiceCreatedAsync(stripeEvent);
                        return true;

                    case "invoice.finalized":
                        await webhookService.HandleInvoiceFinalizedAsync(stripeEvent);
                        return true;

                    case "invoice.payment_succeeded":
                        await webhookService.HandlePaymentSucceededAsync(stripeEvent);
                        return true;

                    case "invoice.payment_failed":
                        await webhookService.HandlePaymentFailedAsync(stripeEvent);
                        return true;

                    case "customer.subscription.updated":
                        await webhookService.HandleSubscriptionUpdatedAsync(stripeEvent);
                        return true;

                    case "customer.subscription.deleted":
                        await webhookService.HandleSubscriptionDeletedAsync(stripeEvent);
                        return true;

                    default:
                        _logger.LogWarning("Unsupported event type for retry: {EventType}", unprocessedEvent.EventType);
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook event {EventType} for event {EventId}", 
                    unprocessedEvent.EventType, unprocessedEvent.Id);
                return false;
            }
        }
    }
}


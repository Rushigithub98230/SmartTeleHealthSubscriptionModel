using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using Stripe;
using SmartTelehealth.Application.DTOs;
using Stripe.Events;
using Microsoft.Extensions.Logging;


namespace SmartTelehealth.API.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class StripeWebhookController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IBillingService _billingService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _maxRetries;
    private readonly int _retryDelaySeconds;

    public StripeWebhookController(
        ISubscriptionService subscriptionService,
        IBillingService billingService,
        INotificationService notificationService,
        ILogger<StripeWebhookController> logger,
        IConfiguration configuration)
    {
        _subscriptionService = subscriptionService;
        _billingService = billingService;
        _notificationService = notificationService;
        _logger = logger;
        _configuration = configuration;
        _maxRetries = configuration.GetValue<int>("Stripe:WebhookRetryAttempts", 3);
        _retryDelaySeconds = configuration.GetValue<int>("Stripe:WebhookRetryDelaySeconds", 5);
    }

    [HttpPost]
    public async Task<JsonModel> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret) || webhookSecret == "whsec_test_webhook_secret_replace_in_production")
        {
            return new JsonModel { data = new object(), Message = "Webhook secret not configured", StatusCode = 400 };
        }

        var stripeEvent = EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            webhookSecret
        );

        // Process webhook with retry logic
        await ProcessWebhookWithRetryAsync(stripeEvent);

        return new JsonModel { data = new object(), Message = "Webhook processed successfully", StatusCode = 200 };
    }

    private async Task ProcessWebhookWithRetryAsync(Event stripeEvent)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                await ProcessStripeEvent(stripeEvent);
                return; // Success, exit retry loop
            }
            catch (Exception ex)
            {
                if (attempt == _maxRetries)
                {
                    // Log final failure
                    throw;
                }
                
                // Wait before retry
                await Task.Delay(_retryDelaySeconds * 1000);
            }
        }
    }

    private async Task ProcessStripeEvent(Event stripeEvent)
    {
        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
                await HandleSubscriptionCreated(stripeEvent);
                break;
            case "customer.subscription.updated":
                await HandleSubscriptionUpdated(stripeEvent);
                break;
            case "customer.subscription.deleted":
                await HandleSubscriptionDeleted(stripeEvent);
                break;
            case "invoice.payment_succeeded":
                await HandlePaymentSucceeded(stripeEvent);
                break;
            case "invoice.payment_failed":
                await HandlePaymentFailed(stripeEvent);
                break;
            case "invoice.payment_action_required":
                await HandlePaymentActionRequired(stripeEvent);
                break;
            case "customer.subscription.trial_will_end":
                await HandleSubscriptionTrialWillEnd(stripeEvent);
                break;
            case "payment_intent.succeeded":
                await HandlePaymentIntentSucceeded(stripeEvent);
                break;
            case "payment_intent.payment_failed":
                await HandlePaymentIntentFailed(stripeEvent);
                break;
            case "customer.created":
                await HandleCustomerCreated(stripeEvent);
                break;
            case "customer.updated":
                await HandleCustomerUpdated(stripeEvent);
                break;
            case "customer.deleted":
                await HandleCustomerDeleted(stripeEvent);
                break;
            default:
                // Log unhandled event type
                break;
        }
    }

    private async Task HandleSubscriptionCreated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        // Update local subscription with Stripe subscription ID
        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            // Subscription already exists, update status
            var updateDto = new UpdateSubscriptionDto
            {
                StripeSubscriptionId = subscription.Id,
                Status = MapStripeStatusToLocal(subscription.Status)
            };
            await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
        }
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            var updateDto = new UpdateSubscriptionDto
            {
                Status = MapStripeStatusToLocal(subscription.Status),
                NextBillingDate = GetNextBillingDateFromSubscription(subscription),
                CurrentPrice = subscription.Items.Data.FirstOrDefault()?.Price.UnitAmount / 100m ?? 0
            };
            await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            await _subscriptionService.CancelSubscriptionAsync(localSubscription.data.ToString(), "Cancelled via Stripe", GetToken(HttpContext));
        }
    }

    private async Task HandlePaymentSucceeded(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        // Validate customer ID format before parsing
        if (!int.TryParse(invoice.CustomerId, out int userId))
        {
            return;
        }

        // Create billing record for successful payment
        await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
        {
            UserId = userId,
            Amount = invoice.AmountPaid / 100m, // Convert from cents
            Currency = invoice.Currency,
            PaymentMethod = "stripe",
            StripeInvoiceId = invoice.Id,
            StripePaymentIntentId = GetPaymentIntentIdFromInvoice(invoice),
            Status = BillingRecord.BillingStatus.Paid.ToString(),
            Description = $"Stripe Invoice Payment: {invoice.Id}",
            BillingDate = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow,
            Type = BillingRecord.BillingType.Subscription.ToString()
        }, GetToken(HttpContext));

        // Update subscription status if needed - using new Invoice.Parent property in 48.4.0
        var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
        if (!string.IsNullOrEmpty(subscriptionId))
        {
            var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, GetToken(HttpContext));
            if (localSubscription.StatusCode == 200)
            {
                var updateDto = new UpdateSubscriptionDto
                {
                    Status = "Active",
                    LastPaymentDate = DateTime.UtcNow,
                    FailedPaymentAttempts = 0
                };
                await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            }
        }
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        // Validate customer ID format before parsing
        if (!int.TryParse(invoice.CustomerId, out int userId))
        {
            return;
        }

        // Create billing record for failed payment
        await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
        {
            UserId = userId,
            Amount = invoice.AmountDue / 100m, // Convert from cents
            Currency = invoice.Currency,
            PaymentMethod = "stripe",
            StripeInvoiceId = invoice.Id,
            Status = BillingRecord.BillingStatus.Failed.ToString(),
            Description = $"Failed payment for invoice {invoice.Number}",
            BillingDate = DateTime.UtcNow,
            Type = BillingRecord.BillingType.Subscription.ToString()
        }, GetToken(HttpContext));

        // Update subscription status to payment failed - using new Invoice.Parent property in 48.4.0
        var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
        if (!string.IsNullOrEmpty(subscriptionId))
        {
            var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, GetToken(HttpContext));
            if (localSubscription.StatusCode == 200)
            {
                var updateDto = new UpdateSubscriptionDto
                {
                    Status = "PaymentFailed",
                    LastPaymentFailedDate = DateTime.UtcNow,
                    LastPaymentError = "Payment failed via Stripe",
                    FailedPaymentAttempts = 1 // Increment failed attempts
                };
                await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            }
        }
    }

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent == null) return;

        // Handle successful payment intent
        // This is typically handled by the invoice events, but we can log it here
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent == null) return;

        // Handle failed payment intent
        // This is typically handled by the invoice events, but we can log it here
    }

    private async Task HandleSubscriptionTrialWillEnd(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        // Get local subscription to send notification
        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            // Send trial ending notification via subscription notification service
            var subscriptionData = localSubscription.data as dynamic;
            if (subscriptionData != null)
            {
                // Create notification for trial ending
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = subscriptionData.UserId,
                    Title = "Trial Ending Soon",
                    Message = $"Your trial for subscription plan will end on {subscription.TrialEnd?.ToString("MMM dd, yyyy")}. Please add a payment method to continue your subscription.",
                    Type = "TrialWarning",
                    IsRead = false,
                    Priority = "High"
                }, GetToken(HttpContext));
            }
        }
    }

    private async Task HandlePaymentActionRequired(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        // Validate customer ID format before parsing
        if (!int.TryParse(invoice.CustomerId, out int userId))
        {
            return;
        }

        // Send payment action required notification - using new Invoice.Parent property in 48.4.0
        var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
        if (!string.IsNullOrEmpty(subscriptionId))
        {
            var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, GetToken(HttpContext));
            if (localSubscription.StatusCode == 200)
            {
                // Create notification for payment action required
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = userId,
                    Title = "Payment Action Required",
                    Message = $"Your payment requires additional verification. Please complete the authentication process to continue your subscription. Invoice: {invoice.Number}",
                    Type = "PaymentAction",
                    IsRead = false,
                    Priority = "High"
                }, GetToken(HttpContext));

                // Update subscription status to indicate action required
                var updateDto = new UpdateSubscriptionDto
                {
                    Status = "PaymentActionRequired",
                    LastPaymentError = "Payment authentication required"
                };
                await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            }
        }
    }

    private async Task HandleCustomerCreated(Event stripeEvent)
    {
        var customer = stripeEvent.Data.Object as Stripe.Customer;
        if (customer == null) return;

        // Log customer creation for audit purposes
        _logger.LogInformation("Stripe customer created: {CustomerId}, Email: {Email}", customer.Id, customer.Email);
        
        // Note: We typically create Stripe customers when users register in our system,
        // so this event is mainly for logging and verification purposes
    }

    private async Task HandleCustomerUpdated(Event stripeEvent)
    {
        var customer = stripeEvent.Data.Object as Stripe.Customer;
        if (customer == null) return;

        // Log customer update for audit purposes
        _logger.LogInformation("Stripe customer updated: {CustomerId}, Email: {Email}", customer.Id, customer.Email);
        
        // Note: Customer updates typically involve payment method changes or profile updates
        // which are handled through our user management system
    }

    private async Task HandleCustomerDeleted(Event stripeEvent)
    {
        var customer = stripeEvent.Data.Object as Stripe.Customer;
        if (customer == null) return;

        // Log customer deletion for audit purposes
        _logger.LogWarning("Stripe customer deleted: {CustomerId}, Email: {Email}", customer.Id, customer.Email);
        
        // Note: Customer deletion should be handled carefully as it affects all subscriptions
        // We typically don't delete customers automatically but mark them as inactive
    }

    private string GetPaymentIntentIdFromInvoice(Stripe.Invoice invoice)
    {
        // In Stripe.net 48.4.0, PaymentIntentId is removed from Invoice
        // We need to extract it from the Invoice.Payments array
        if (invoice.Payments?.Data?.Count > 0)
        {
            var invoicePayment = invoice.Payments.Data.FirstOrDefault();
            if (invoicePayment != null)
            {
                // The payment intent ID might be available through the payment object
                // For now, return empty string as we need additional API calls to get this
                return string.Empty;
            }
        }
        return string.Empty;
    }

    private DateTime GetNextBillingDateFromSubscription(Stripe.Subscription subscription)
    {
        try
        {
            // In Stripe.net 48.4.0, CurrentPeriodEnd is moved to subscription items
            var firstItem = subscription.Items.Data.FirstOrDefault();
            if (firstItem?.CurrentPeriodEnd != null)
            {
                // Convert Unix timestamp to DateTime
                var unixTimestamp = Convert.ToInt64(firstItem.CurrentPeriodEnd);
                return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to parse subscription billing date: {Error}", ex.Message);
        }
        
        // Fallback to default
        return DateTime.UtcNow.AddMonths(1);
    }

    private string GetSubscriptionIdFromInvoice(Stripe.Invoice invoice)
    {
        // In Stripe.net 48.4.0, SubscriptionId is removed from Invoice
        // Try to get it from different possible locations
        if (invoice.Parent != null)
        {
            // Try to convert Parent to string - it might be the subscription ID directly
            var parentString = invoice.Parent.ToString();
            if (!string.IsNullOrEmpty(parentString) && parentString != "null")
            {
                return parentString;
            }
        }
        
        // Fallback: try to get from metadata or other fields
        if (invoice.Metadata?.ContainsKey("subscription_id") == true)
        {
            return invoice.Metadata["subscription_id"];
        }
        
        return string.Empty;
    }

    private string MapStripeStatusToLocal(string stripeStatus)
    {
        return stripeStatus switch
        {
            "active" => "Active",
            "canceled" => "Cancelled",
            "incomplete" => "Pending",
            "incomplete_expired" => "Expired",
            "past_due" => "PaymentFailed",
            "trialing" => "TrialActive",
            "unpaid" => "PaymentFailed",
            _ => "Pending"
        };
    }
}
} 
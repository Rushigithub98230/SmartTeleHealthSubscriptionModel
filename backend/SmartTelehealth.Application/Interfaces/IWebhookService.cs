using Stripe;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Interfaces
{
    /// <summary>
    /// Interface for handling Stripe webhook events and maintaining data consistency
    /// between Stripe and the local database.
    /// </summary>
    public interface IWebhookService
    {
        #region Core Subscription Events

        /// <summary>
        /// Handles subscription created event from Stripe
        /// </summary>
        Task HandleSubscriptionCreatedAsync(Event stripeEvent);

        /// <summary>
        /// Handles subscription updated event from Stripe
        /// </summary>
        Task HandleSubscriptionUpdatedAsync(Event stripeEvent);

        /// <summary>
        /// Handles subscription deleted event from Stripe
        /// </summary>
        Task HandleSubscriptionDeletedAsync(Event stripeEvent);

        /// <summary>
        /// Handles subscription paused event from Stripe
        /// </summary>
        Task HandleSubscriptionPausedAsync(Event stripeEvent);

        /// <summary>
        /// Handles subscription resumed event from Stripe
        /// </summary>
        Task HandleSubscriptionResumedAsync(Event stripeEvent);

        /// <summary>
        /// Handles subscription past due event from Stripe
        /// </summary>
        Task HandleSubscriptionPastDueAsync(Event stripeEvent);

        /// <summary>
        /// Handles subscription unpaid event from Stripe
        /// </summary>
        Task HandleSubscriptionUnpaidAsync(Event stripeEvent);

        /// <summary>
        /// Handles subscription trial will end event from Stripe
        /// </summary>
        Task HandleSubscriptionTrialWillEndAsync(Event stripeEvent);

        #endregion

        #region Payment Events

        /// <summary>
        /// Handles payment succeeded event from Stripe
        /// </summary>
        Task HandlePaymentSucceededAsync(Event stripeEvent);

        /// <summary>
        /// Handles payment failed event from Stripe
        /// </summary>
        Task HandlePaymentFailedAsync(Event stripeEvent);

        /// <summary>
        /// Handles payment intent succeeded event from Stripe
        /// </summary>
        Task HandlePaymentIntentSucceededAsync(Event stripeEvent);

        /// <summary>
        /// Handles payment intent failed event from Stripe
        /// </summary>
        Task HandlePaymentIntentFailedAsync(Event stripeEvent);

        /// <summary>
        /// Handles payment intent requires action event from Stripe
        /// </summary>
        Task HandlePaymentIntentRequiresActionAsync(Event stripeEvent);

        #endregion

        #region Invoice Events

        /// <summary>
        /// Handles invoice created event from Stripe
        /// </summary>
        Task HandleInvoiceCreatedAsync(Event stripeEvent);

        /// <summary>
        /// Handles invoice finalized event from Stripe
        /// </summary>
        Task HandleInvoiceFinalizedAsync(Event stripeEvent);

        /// <summary>
        /// Handles invoice sent event from Stripe
        /// </summary>
        Task HandleInvoiceSentAsync(Event stripeEvent);

        /// <summary>
        /// Handles invoice upcoming event from Stripe
        /// </summary>
        Task HandleInvoiceUpcomingAsync(Event stripeEvent);

        /// <summary>
        /// Handles invoice finalization failed event from Stripe
        /// </summary>
        Task HandleInvoiceFinalizationFailedAsync(Event stripeEvent);

        /// <summary>
        /// Handles invoice voided event from Stripe
        /// </summary>
        Task HandleInvoiceVoidedAsync(Event stripeEvent);

        /// <summary>
        /// Handles invoice payment action required event from Stripe
        /// </summary>
        Task HandleInvoicePaymentActionRequiredAsync(Event stripeEvent);

        #endregion

        #region Payment Method Events

        /// <summary>
        /// Handles payment method attached event from Stripe
        /// </summary>
        Task HandlePaymentMethodAttachedAsync(Event stripeEvent);

        /// <summary>
        /// Handles payment method updated event from Stripe
        /// </summary>
        Task HandlePaymentMethodUpdatedAsync(Event stripeEvent);

        /// <summary>
        /// Handles payment method detached event from Stripe
        /// </summary>
        Task HandlePaymentMethodDetachedAsync(Event stripeEvent);

        #endregion

        #region Charge Events

        /// <summary>
        /// Handles charge refunded event from Stripe
        /// </summary>
        Task HandleChargeRefundedAsync(Event stripeEvent);

        /// <summary>
        /// Handles charge dispute created event from Stripe
        /// </summary>
        Task HandleChargeDisputeCreatedAsync(Event stripeEvent);

        /// <summary>
        /// Handles charge dispute closed event from Stripe
        /// </summary>
        Task HandleChargeDisputeClosedAsync(Event stripeEvent);

        #endregion

        #region Customer Events

        /// <summary>
        /// Handles customer created event from Stripe
        /// </summary>
        Task HandleCustomerCreatedAsync(Event stripeEvent);

        /// <summary>
        /// Handles customer updated event from Stripe
        /// </summary>
        Task HandleCustomerUpdatedAsync(Event stripeEvent);

        /// <summary>
        /// Handles customer deleted event from Stripe
        /// </summary>
        Task HandleCustomerDeletedAsync(Event stripeEvent);

        #endregion

        #region Setup Intent Events

        /// <summary>
        /// Handles setup intent succeeded event from Stripe
        /// </summary>
        Task HandleSetupIntentSucceededAsync(Event stripeEvent);

        /// <summary>
        /// Handles setup intent failed event from Stripe
        /// </summary>
        Task HandleSetupIntentFailedAsync(Event stripeEvent);

        #endregion

        #region Checkout Events

        /// <summary>
        /// Handles checkout session completed event from Stripe
        /// </summary>
        Task HandleCheckoutSessionCompletedAsync(Event stripeEvent);

        #endregion
    }
}
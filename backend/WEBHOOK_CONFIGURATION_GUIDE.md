# Stripe Webhook Configuration Guide

## Overview
This guide provides step-by-step instructions for configuring Stripe webhooks for the SmartTelehealth subscription management system.

## Webhook Endpoint Configuration

### 1. Stripe Dashboard Setup
1. Go to [Stripe Dashboard](https://dashboard.stripe.com)
2. Switch to **Test mode** (toggle in top-left)
3. Navigate to **Developers** → **Webhooks**
4. Click **"Add endpoint"**

### 2. Endpoint Configuration
- **Endpoint URL**: `https://pwlkgvc0-61376.inc1.devtunnels.ms/api/StripeWebhook/webhook`
- **Description**: "SmartTelehealth Subscription Management Webhooks"

### 3. Events to Listen For
Select the following events for comprehensive subscription management:

#### Subscription Events
- `customer.subscription.created`
- `customer.subscription.updated`
- `customer.subscription.deleted`
- `customer.subscription.paused`
- `customer.subscription.resumed`
- `customer.subscription.past_due`
- `customer.subscription.unpaid`
- `customer.subscription.trial_will_end`

#### Payment Events
- `invoice.payment_succeeded`
- `invoice.payment_failed`
- `invoice.payment_action_required`
- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `payment_intent.requires_action`

#### Invoice Events
- `invoice.created`
- `invoice.finalized`
- `invoice.sent`
- `invoice.upcoming`
- `invoice.finalization_failed`
- `invoice.voided`

#### Customer Events
- `customer.created`
- `customer.updated`
- `customer.deleted`

#### Payment Method Events
- `payment_method.attached`
- `payment_method.updated`
- `payment_method.detached`

#### Setup Intent Events
- `setup_intent.succeeded`
- `setup_intent.setup_failed`

#### Charge Events
- `charge.refunded`
- `charge.dispute.created`
- `charge.dispute.closed`

### 4. Webhook Secret Configuration
1. After creating the webhook endpoint, copy the **Signing Secret**
2. Update your `appsettings.json`:
```json
{
  "StripeSettings": {
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_...",
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5
  }
}
```

## Testing Webhooks

### 1. Using Stripe CLI (Recommended)
```bash
# Install Stripe CLI
# https://stripe.com/docs/stripe-cli

# Login to Stripe
stripe login

# Forward webhooks to local development
stripe listen --forward-to https://pwlkgvc0-61376.inc1.devtunnels.ms/api/StripeWebhook/webhook

# Trigger test events
stripe trigger customer.subscription.created
stripe trigger invoice.payment_succeeded
stripe trigger invoice.payment_failed
```

### 2. Using Stripe Dashboard
1. Go to **Webhooks** → **Your Endpoint**
2. Click **"Send test webhook"**
3. Select event type and click **"Send test webhook"**

### 3. Manual Testing
Create test subscriptions and payments to trigger real webhook events.

## Webhook Security

### 1. Signature Verification
The webhook controller automatically verifies Stripe signatures using:
- `EventUtility.ConstructEvent()` method
- Webhook secret from configuration
- Proper error handling for invalid signatures

### 2. Idempotency
- All webhook events are tracked in `ProcessedWebhookEvents` table
- Duplicate events are automatically skipped
- Failed events are retried with exponential backoff

### 3. Error Handling
- Comprehensive error logging
- Proper HTTP status codes
- Retry mechanism for transient failures
- Permanent failure tracking

## Database Synchronization

### 1. Subscription Synchronization
Webhooks automatically update local subscriptions with:
- Status changes (Active, Paused, Cancelled, etc.)
- Billing dates and trial information
- Payment method updates
- Pause/resume information

### 2. Billing Record Creation
Webhooks create billing records for:
- Successful payments
- Failed payments
- Refunds and disputes
- Invoice events

### 3. Notification System
Webhooks trigger notifications for:
- Payment success/failure
- Trial ending warnings
- Subscription status changes
- Payment action required

## Monitoring and Debugging

### 1. Logging
All webhook events are logged with:
- Event ID and type
- Processing duration
- Success/failure status
- Error details

### 2. Webhook Event Tracking
Check the `ProcessedWebhookEvents` table for:
- Event processing history
- Retry attempts
- Error messages
- Processing statistics

### 3. Stripe Dashboard
Monitor webhook delivery in Stripe Dashboard:
- Delivery attempts
- Response codes
- Error messages
- Retry history

## Troubleshooting

### Common Issues

1. **Webhook Secret Mismatch**
   - Ensure `StripeSettings:WebhookSecret` is correctly configured
   - Verify the secret starts with `whsec_`

2. **Signature Verification Failed**
   - Check that the webhook secret matches Stripe dashboard
   - Ensure the endpoint URL is correct

3. **Database Synchronization Issues**
   - Check that local subscription exists for Stripe subscription
   - Verify user ID mapping in customer metadata

4. **Missing Billing Records**
   - Ensure invoice events are properly handled
   - Check that subscription ID is correctly extracted from invoices

### Debug Steps

1. Check application logs for webhook processing errors
2. Verify webhook endpoint is accessible and returns 200 status
3. Test with Stripe CLI to isolate issues
4. Check database for missing or incorrect data

## Production Considerations

### 1. Webhook Secret Rotation
- Regularly rotate webhook secrets
- Update configuration before disabling old secret
- Test thoroughly after rotation

### 2. Rate Limiting
- Monitor webhook processing performance
- Implement rate limiting if needed
- Scale infrastructure as required

### 3. Monitoring
- Set up alerts for webhook failures
- Monitor processing times
- Track error rates and patterns

## Support

For issues with webhook configuration or processing:
1. Check application logs
2. Review Stripe webhook logs
3. Test with Stripe CLI
4. Contact development team with specific error details


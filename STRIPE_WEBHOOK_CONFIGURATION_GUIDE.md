# Stripe Webhook Configuration Guide
## Complete Setup for SmartTelehealth Subscription Management

### Executive Summary

This guide provides comprehensive instructions for configuring Stripe webhooks to ensure your SmartTelehealth backend correctly receives and processes all subscription-related events. Your backend already has robust webhook handling implemented - you just need to configure Stripe to send the events.

---

## 🎯 **WEBHOOK ENDPOINT URL**

### **Production Environment**
```
https://yourdomain.com/api/StripeWebhook/webhook
```

### **Development Environment**
```
https://your-ngrok-url.ngrok.io/api/StripeWebhook/webhook
```

### **Local Testing (ngrok)**
```bash
# Install ngrok
npm install -g ngrok

# Expose your local API
ngrok http 5000

# Use the HTTPS URL provided by ngrok
https://abc123.ngrok.io/api/StripeWebhook/webhook
```

---

## 📋 **REQUIRED WEBHOOK EVENTS**

Based on your backend implementation, configure these **essential events** in your Stripe Dashboard:

### **🔴 CRITICAL EVENTS (Must Have)**

#### **Subscription Lifecycle Events**
```
customer.subscription.created
customer.subscription.updated
customer.subscription.deleted
customer.subscription.paused
customer.subscription.resumed
customer.subscription.past_due
customer.subscription.unpaid
customer.subscription.trial_will_end
```

#### **Payment Events**
```
invoice.payment_succeeded
invoice.payment_failed
invoice.payment_action_required
payment_intent.succeeded
payment_intent.payment_failed
payment_intent.requires_action
```

#### **Invoice Events**
```
invoice.created
invoice.finalized
invoice.sent
invoice.upcoming
invoice.voided
invoice.finalization_failed
```

#### **Customer Events**
```
customer.created
customer.updated
customer.deleted
```

#### **Payment Method Events**
```
payment_method.attached
payment_method.updated
payment_method.detached
```

### **🟡 IMPORTANT EVENTS (Recommended)**

#### **Checkout Events**
```
checkout.session.completed
```

#### **Setup Intent Events**
```
setup_intent.succeeded
setup_intent.setup_failed
```

#### **Charge Events**
```
charge.refunded
charge.dispute.created
charge.dispute.closed
```

#### **Product & Price Events**
```
product.created
product.updated
product.deleted
price.created
price.updated
price.deleted
```

### **🟢 OPTIONAL EVENTS (Nice to Have)**

#### **Payout Events**
```
payout.created
payout.updated
payout.paid
payout.failed
payout.canceled
```

#### **Subscription Schedule Events**
```
subscription_schedule.canceled
subscription_schedule.completed
subscription_schedule.created
subscription_schedule.released
subscription_schedule.updated
```

#### **Other Events**
```
balance.available
mandate.updated
review.opened
review.closed
tax_rate.created
tax_rate.updated
transfer.created
transfer.failed
transfer.paid
transfer.reversed
transfer.updated
```

---

## ⚙️ **STRIPE DASHBOARD CONFIGURATION**

### **Step 1: Access Webhook Settings**
1. Log in to your [Stripe Dashboard](https://dashboard.stripe.com)
2. Navigate to **Developers** → **Webhooks**
3. Click **"Add endpoint"**

### **Step 2: Configure Endpoint**
```
Endpoint URL: https://yourdomain.com/api/StripeWebhook/webhook
Description: SmartTelehealth Subscription Management
```

### **Step 3: Select Events**
Select all the events listed above, or use this **bulk selection**:

#### **Quick Selection Groups**
```
✅ Select all "customer.subscription.*" events
✅ Select all "invoice.*" events  
✅ Select all "payment_intent.*" events
✅ Select all "customer.*" events
✅ Select all "payment_method.*" events
✅ Select all "checkout.session.*" events
✅ Select all "setup_intent.*" events
✅ Select all "charge.*" events
✅ Select all "product.*" events
✅ Select all "price.*" events
```

### **Step 4: Advanced Settings**
```
API Version: 2023-10-16 (or latest)
Connect: No (unless using Connect)
```

---

## 🔐 **WEBHOOK SECRET CONFIGURATION**

### **Step 1: Get Webhook Secret**
1. After creating the webhook, click on it
2. Click **"Reveal"** next to **Signing secret**
3. Copy the webhook secret (starts with `whsec_`)

### **Step 2: Configure in Your Backend**

#### **appsettings.json**
```json
{
  "StripeSettings": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_...",
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5
  }
}
```

#### **appsettings.Production.json**
```json
{
  "StripeSettings": {
    "SecretKey": "sk_live_...",
    "PublishableKey": "pk_live_...",
    "WebhookSecret": "whsec_...",
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5
  }
}
```

#### **Environment Variables (Recommended for Production)**
```bash
StripeSettings__SecretKey=sk_live_...
StripeSettings__PublishableKey=pk_live_...
StripeSettings__WebhookSecret=whsec_...
StripeSettings__WebhookRetryAttempts=3
StripeSettings__WebhookRetryDelaySeconds=5
```

---

## 🧪 **TESTING WEBHOOK CONFIGURATION**

### **Step 1: Test Webhook Endpoint**
1. In Stripe Dashboard, go to your webhook
2. Click **"Send test webhook"**
3. Select **"customer.subscription.created"**
4. Click **"Send test webhook"**

### **Step 2: Verify Backend Logs**
Check your backend logs for:
```
✅ "Processing webhook event {EventId} of type {EventType}"
✅ "Successfully processed webhook event {EventId} in {Duration}ms"
```

### **Step 3: Test Real Events**
1. Create a test subscription in your app
2. Check webhook delivery in Stripe Dashboard
3. Verify backend processing logs

---

## 📊 **WEBHOOK MONITORING & DEBUGGING**

### **Stripe Dashboard Monitoring**
1. Go to **Developers** → **Webhooks**
2. Click on your webhook endpoint
3. Monitor **"Recent deliveries"** tab
4. Check for failed deliveries

### **Backend Logging**
Your backend already includes comprehensive logging:

```csharp
// Success logs
_logger.LogInformation("Successfully processed webhook event {EventId} in {Duration}ms", 
    stripeEvent.Id, stopwatch.ElapsedMilliseconds);

// Error logs
_logger.LogError(ex, "Stripe error processing webhook event {EventId} of type {EventType}", 
    stripeEvent.Id, stripeEvent.Type);
```

### **Common Issues & Solutions**

#### **❌ 400 Bad Request - Invalid Signature**
```
Solution: Verify webhook secret is correct
Check: StripeSettings__WebhookSecret in configuration
```

#### **❌ 500 Internal Server Error**
```
Solution: Check backend logs for specific error
Common causes: Database connection, service dependencies
```

#### **❌ Webhook Not Receiving Events**
```
Solution: 
1. Verify endpoint URL is accessible
2. Check firewall/security group settings
3. Ensure HTTPS is used (not HTTP)
```

#### **❌ Events Not Processing**
```
Solution:
1. Check webhook event selection in Stripe Dashboard
2. Verify backend service dependencies are registered
3. Check database connectivity
```

---

## 🔄 **WEBHOOK RETRY CONFIGURATION**

### **Stripe Retry Policy**
Stripe automatically retries failed webhooks:
- **Immediate**: 1 retry
- **5 minutes**: 1 retry  
- **1 hour**: 1 retry
- **6 hours**: 1 retry
- **12 hours**: 1 retry
- **24 hours**: 1 retry

### **Backend Retry Configuration**
```json
{
  "StripeSettings": {
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5
  }
}
```

---

## 🚀 **PRODUCTION DEPLOYMENT CHECKLIST**

### **Pre-Deployment**
- [ ] Configure webhook endpoint URL
- [ ] Set up webhook secret in production environment
- [ ] Test webhook with Stripe test events
- [ ] Verify all required events are selected
- [ ] Check SSL certificate is valid

### **Post-Deployment**
- [ ] Monitor webhook delivery success rate
- [ ] Check backend logs for processing errors
- [ ] Test real subscription creation
- [ ] Verify payment processing works
- [ ] Monitor webhook retry attempts

### **Ongoing Monitoring**
- [ ] Set up alerts for webhook failures
- [ ] Monitor webhook delivery latency
- [ ] Review failed webhook logs weekly
- [ ] Update webhook events as needed

---

## 📱 **WEBHOOK EVENT FLOW EXAMPLES**

### **Subscription Creation Flow**
```
1. User creates subscription → Frontend
2. Backend creates Stripe subscription → Stripe API
3. Stripe sends webhook → customer.subscription.created
4. Backend processes webhook → Updates local database
5. User receives confirmation → Frontend
```

### **Payment Success Flow**
```
1. Stripe processes payment → Stripe
2. Stripe sends webhook → invoice.payment_succeeded
3. Backend processes webhook → Updates billing records
4. User receives payment confirmation → Email/Notification
```

### **Payment Failure Flow**
```
1. Payment fails → Stripe
2. Stripe sends webhook → invoice.payment_failed
3. Backend processes webhook → Updates subscription status
4. User receives failure notification → Email/Notification
```

---

## 🛠️ **TROUBLESHOOTING COMMANDS**

### **Test Webhook Endpoint**
```bash
# Test if endpoint is accessible
curl -X POST https://yourdomain.com/api/StripeWebhook/webhook \
  -H "Content-Type: application/json" \
  -d '{"test": "webhook"}'
```

### **Check Webhook Secret**
```bash
# Verify webhook secret is configured
echo $StripeSettings__WebhookSecret
```

### **Monitor Webhook Logs**
```bash
# Check webhook processing logs
tail -f /var/log/your-app/webhook.log | grep "webhook event"
```

---

## 📋 **FINAL CONFIGURATION SUMMARY**

### **Required Stripe Dashboard Settings**
```
✅ Endpoint URL: https://yourdomain.com/api/StripeWebhook/webhook
✅ Events: All subscription, payment, invoice, customer events
✅ API Version: 2023-10-16 (or latest)
✅ Signing Secret: whsec_... (configured in backend)
```

### **Required Backend Configuration**
```json
{
  "StripeSettings": {
    "SecretKey": "sk_live_...",
    "PublishableKey": "pk_live_...", 
    "WebhookSecret": "whsec_...",
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5
  }
}
```

### **Verification Steps**
```
✅ Webhook endpoint responds to test events
✅ Backend logs show successful processing
✅ Real subscription creation triggers webhooks
✅ Payment events are processed correctly
✅ Error handling works for failed events
```

---

## 🎉 **CONCLUSION**

Your SmartTelehealth backend already has **comprehensive webhook handling** implemented. You just need to:

1. **Configure the webhook endpoint** in Stripe Dashboard
2. **Select all required events** (use the list provided)
3. **Set the webhook secret** in your backend configuration
4. **Test the configuration** with Stripe test events
5. **Monitor webhook delivery** in production

The backend will automatically handle all webhook events, maintain data consistency, and provide comprehensive logging for debugging.

**Status: Ready for Production** ✅

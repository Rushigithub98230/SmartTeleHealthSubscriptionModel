# 🔧 Stripe Integration Fixes - Implementation Guide

**Date:** Thursday, October 16, 2025  
**Current Stripe.NET Version:** 48.4.0  
**Recommended Upgrade Version:** Latest (46.x+)  
**Priority:** 🟡 **LOW-MEDIUM** (Non-blocking, but improves robustness)

---

## 📊 ISSUES TO FIX

### **Issue #1: Checkout Session Handler - Incomplete** 🟡
**File:** `backend/SmartTelehealth.API/Controllers/StripeWebhookController.cs`  
**Line:** 1550-1556  
**Severity:** LOW  
**Impact:** Checkout sessions cannot be processed if used

### **Issue #2: Payment Intent/Invoice ID Extraction - Metadata Dependency** 🟡
**File:** `backend/SmartTelehealth.API/Controllers/StripeWebhookController.cs`  
**Lines:** 841-860, 887-911  
**Severity:** LOW  
**Impact:** Relies on manually set metadata (works but not ideal)

---

## 🎯 SOLUTION APPROACH

You have **TWO OPTIONS**:

### **Option A: Upgrade Stripe.NET (RECOMMENDED)** ⭐
- **Pros:** Full access to all properties, better performance, latest features
- **Cons:** Requires testing, potential breaking changes
- **Effort:** 2-3 hours (upgrade + testing)

### **Option B: Fix with Current Version**
- **Pros:** No dependency changes, minimal testing
- **Cons:** Limited property access, relies on workarounds
- **Effort:** 30 minutes

**RECOMMENDATION:** ✅ **Option A** - Upgrade to latest Stripe.NET

---

## 🚀 OPTION A: UPGRADE STRIPE.NET (RECOMMENDED)

### **Step 1: Check Latest Version**

```bash
# In terminal
dotnet list package --include-transitive | findstr Stripe

# Or check online
https://www.nuget.org/packages/Stripe.net
```

**Latest stable version as of Oct 2024:** **v45.x+**

### **Step 2: Upgrade Package**

**File:** `backend/SmartTelehealth.Infrastructure/SmartTelehealth.Infrastructure.csproj`

```xml
<!-- BEFORE (Line 19): -->
<PackageReference Include="Stripe.net" Version="48.4.0" />

<!-- AFTER: -->
<PackageReference Include="Stripe.net" Version="45.14.0" />
<!-- Or use latest: -->
<PackageReference Include="Stripe.net" Version="*" />
```

**File:** `backend/SmartTelehealth.API/SmartTelehealth.API.csproj`

```xml
<!-- BEFORE (Line 17): -->
<PackageReference Include="Stripe.net" Version="48.4.0" />

<!-- AFTER: -->
<PackageReference Include="Stripe.net" Version="45.14.0" />
```

### **Step 3: Restore Packages**

```bash
cd backend
dotnet restore
dotnet build
```

### **Step 4: Fix Checkout Session Handler**

**File:** `backend/SmartTelehealth.API/Controllers/StripeWebhookController.cs`

**Replace lines 1550-1556 with:**

```csharp
private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
{
    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
    if (session == null)
    {
        _logger.LogWarning("Checkout session event received but session object is null");
        return;
    }

    try
    {
        _logger.LogInformation("Processing checkout session completed: {SessionId}, Customer: {CustomerId}", 
            session.Id, session.CustomerId);

        // Get subscription from checkout session
        string subscriptionId = session.SubscriptionId;
        if (string.IsNullOrEmpty(subscriptionId))
        {
            _logger.LogInformation("Checkout session {SessionId} does not have a subscription", session.Id);
            return;
        }

        // Get local subscription by Stripe subscription ID
        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200 && localSubscription.data != null)
        {
            var subscriptionData = localSubscription.data as dynamic;
            if (subscriptionData != null)
            {
                // Update subscription with checkout session details
                var updateDto = new UpdateSubscriptionDto
                {
                    Status = "Active",
                    LastPaymentDate = DateTime.UtcNow,
                    StripeCustomerId = session.CustomerId,
                    UpdatedDate = DateTime.UtcNow
                };

                await _subscriptionLifecycleService.UpdateSubscriptionAsync(
                    subscriptionData.Id?.ToString(), 
                    updateDto, 
                    GetToken(HttpContext));

                // Create billing record for checkout session payment
                if (session.AmountTotal.HasValue && session.AmountTotal.Value > 0)
                {
                    var billingRecordDto = new CreateBillingRecordDto
                    {
                        UserId = subscriptionData.UserId,
                        Amount = session.AmountTotal.Value / 100m, // Convert from cents
                        CurrencyId = null,
                        PaymentMethod = "stripe",
                        StripePaymentIntentId = session.PaymentIntentId,
                        Status = BillingRecord.BillingStatus.Paid.ToString(),
                        Description = $"Checkout session payment - Session: {session.Id}",
                        BillingDate = DateTime.UtcNow,
                        PaidDate = DateTime.UtcNow,
                        Type = BillingRecord.BillingType.Subscription.ToString(),
                        SubscriptionId = subscriptionId
                    };

                    var billingResult = await _billingService.CreateBillingRecordAsync(billingRecordDto, GetToken(HttpContext));
                    if (billingResult.StatusCode != 200)
                    {
                        _logger.LogError("Failed to create billing record for checkout session {SessionId}", session.Id);
                    }
                }

                // Send success notification
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = subscriptionData.UserId,
                    Title = "Subscription Activated",
                    Message = $"Your subscription has been successfully activated via checkout. Session: {session.Id}",
                    Type = "SubscriptionActivated",
                    IsRead = false,
                    Priority = "Normal"
                }, GetToken(HttpContext));

                _logger.LogInformation("Checkout session {SessionId} processed successfully for subscription {SubscriptionId}", 
                    session.Id, subscriptionId);
            }
        }
        else
        {
            _logger.LogWarning("Local subscription not found for Stripe subscription {SubscriptionId} from checkout session {SessionId}", 
                subscriptionId, session.Id);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling checkout session completed for session {SessionId}", session.Id);
        throw; // Re-throw to trigger retry mechanism
    }
}
```

### **Step 5: Fix Payment Intent ID Extraction**

**File:** `backend/SmartTelehealth.API/Controllers/StripeWebhookController.cs`

**Replace lines 841-860 with:**

```csharp
private string GetPaymentIntentIdFromInvoice(Stripe.Invoice invoice)
{
    try
    {
        // Method 1: Direct property access (available in newer Stripe.NET versions)
        if (!string.IsNullOrEmpty(invoice.PaymentIntentId))
        {
            _logger.LogDebug("Payment intent ID extracted directly from invoice: {PaymentIntentId}", invoice.PaymentIntentId);
            return invoice.PaymentIntentId;
        }

        // Method 2: Try to get from PaymentIntent object
        if (invoice.PaymentIntent != null)
        {
            var paymentIntentId = invoice.PaymentIntent is string strId ? strId : (invoice.PaymentIntent as Stripe.PaymentIntent)?.Id;
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                _logger.LogDebug("Payment intent ID extracted from PaymentIntent object: {PaymentIntentId}", paymentIntentId);
                return paymentIntentId;
            }
        }

        // Method 3: Fallback to metadata (for backward compatibility)
        if (invoice.Metadata?.ContainsKey("payment_intent_id") == true)
        {
            _logger.LogDebug("Payment intent ID extracted from metadata: {PaymentIntentId}", invoice.Metadata["payment_intent_id"]);
            return invoice.Metadata["payment_intent_id"];
        }

        _logger.LogDebug("No payment intent ID found for invoice {InvoiceId}", invoice.Id);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Error extracting payment intent ID from invoice {InvoiceId}", invoice.Id);
    }
    
    return string.Empty;
}
```

### **Step 6: Fix Subscription ID Extraction**

**File:** `backend/SmartTelehealth.API/Controllers/StripeWebhookController.cs`

**Replace lines 887-911 with:**

```csharp
private string GetSubscriptionIdFromInvoice(Stripe.Invoice invoice)
{
    try
    {
        // Method 1: Direct property access (available in newer Stripe.NET versions)
        if (!string.IsNullOrEmpty(invoice.SubscriptionId))
        {
            _logger.LogDebug("Subscription ID extracted directly from invoice: {SubscriptionId}", invoice.SubscriptionId);
            return invoice.SubscriptionId;
        }

        // Method 2: Try to get from Subscription object
        if (invoice.Subscription != null)
        {
            var subscriptionId = invoice.Subscription is string strId ? strId : (invoice.Subscription as Stripe.Subscription)?.Id;
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                _logger.LogDebug("Subscription ID extracted from Subscription object: {SubscriptionId}", subscriptionId);
                return subscriptionId;
            }
        }

        // Method 3: Fallback to metadata (for backward compatibility)
        if (invoice.Metadata?.ContainsKey("subscription_id") == true)
        {
            _logger.LogDebug("Subscription ID extracted from metadata: {SubscriptionId}", invoice.Metadata["subscription_id"]);
            return invoice.Metadata["subscription_id"];
        }

        _logger.LogDebug("No subscription ID found for invoice {InvoiceId}", invoice.Id);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Error extracting subscription ID from invoice {InvoiceId}", invoice.Id);
    }
    
    return string.Empty;
}
```

### **Step 7: Update Subscription Property Access**

**File:** `backend/SmartTelehealth.API/Controllers/StripeWebhookController.cs`

**Replace lines 862-885 with:**

```csharp
private DateTime GetNextBillingDateFromSubscription(Stripe.Subscription subscription)
{
    try
    {
        // Method 1: Direct property access (available in newer versions)
        if (subscription.CurrentPeriodEnd.HasValue)
        {
            _logger.LogDebug("Next billing date extracted from CurrentPeriodEnd: {Date}", subscription.CurrentPeriodEnd.Value);
            return subscription.CurrentPeriodEnd.Value;
        }

        // Method 2: Try Unix timestamp conversion
        // Some versions expose CurrentPeriodEnd as Unix timestamp
        var currentPeriodEnd = subscription.GetType().GetProperty("CurrentPeriodEnd")?.GetValue(subscription);
        if (currentPeriodEnd != null)
        {
            if (currentPeriodEnd is long unixTimestamp)
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
                _logger.LogDebug("Next billing date calculated from Unix timestamp: {Date}", date);
                return date;
            }
        }

        // Method 3: Fallback - try subscription items
        var firstItem = subscription.Items?.Data?.FirstOrDefault();
        if (firstItem?.CurrentPeriodEnd != null)
        {
            var unixTs = Convert.ToInt64(firstItem.CurrentPeriodEnd);
            var date = DateTimeOffset.FromUnixTimeSeconds(unixTs).DateTime;
            _logger.LogDebug("Next billing date extracted from subscription item: {Date}", date);
            return date;
        }

        _logger.LogWarning("Could not extract next billing date from subscription {SubscriptionId}, using default", subscription.Id);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to parse subscription billing date for {SubscriptionId}", subscription.Id);
    }
    
    // Fallback to default (1 month from now)
    return DateTime.UtcNow.AddMonths(1);
}
```

### **Step 8: Test the Changes**

```bash
# Run tests
cd backend
dotnet test

# Check for compilation errors
dotnet build --no-restore

# Run the application
dotnet run --project SmartTelehealth.API
```

### **Step 9: Verify Webhook Processing**

1. **Test with Stripe CLI:**
```bash
stripe listen --forward-to localhost:5000/api/stripewebhook/webhook
stripe trigger checkout.session.completed
stripe trigger invoice.payment_succeeded
```

2. **Check logs** for successful extraction of IDs

3. **Verify database** - Billing records created with proper Stripe IDs

---

## 🔨 OPTION B: FIX WITH CURRENT VERSION (Quick Fix)

If you cannot upgrade immediately, implement these workarounds:

### **Fix #1: Ensure Metadata is Always Set**

#### **Update StripeService.CreateSubscriptionAsync()**

**File:** `backend/SmartTelehealth.Infrastructure/Services/StripeService.cs`

**Find the CreateSubscriptionAsync method and ensure metadata is set:**

```csharp
public async Task<string> CreateSubscriptionAsync(
    string customerId, 
    string priceId, 
    TokenModel tokenModel,
    int? trialPeriodDays = null,
    Dictionary<string, string>? metadata = null)
{
    try
    {
        var subscriptionOptions = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions { Price = priceId }
            },
            Metadata = new Dictionary<string, string>
            {
                { "source", "smart_telehealth" },
                { "created_at", DateTime.UtcNow.ToString("O") },
                { "user_id", tokenModel.UserID.ToString() }
            },
            // CRITICAL: Set payment behavior to ensure invoices are created
            PaymentBehavior = "default_incomplete",
            PaymentSettings = new SubscriptionPaymentSettingsOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                SaveDefaultPaymentMethod = "on_subscription"
            }
        };

        // Add custom metadata if provided
        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                subscriptionOptions.Metadata[kvp.Key] = kvp.Value;
            }
        }

        if (trialPeriodDays.HasValue && trialPeriodDays.Value > 0)
        {
            subscriptionOptions.TrialPeriodDays = trialPeriodDays.Value;
        }

        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.CreateAsync(subscriptionOptions);

        _logger.LogInformation("Created Stripe subscription {SubscriptionId} for customer {CustomerId}", 
            subscription.Id, customerId);
        
        return subscription.Id;
    }
    catch (StripeException ex)
    {
        _logger.LogError(ex, "Stripe error creating subscription for customer {CustomerId}", customerId);
        throw new InvalidOperationException($"Failed to create Stripe subscription: {ex.Message}", ex);
    }
}
```

#### **Update Subscription Creation to Pass Metadata**

**File:** `backend/SmartTelehealth.Application/Services/SubscriptionLifecycleService.cs`

**Find where Stripe subscription is created (around lines 156-184) and add metadata:**

```csharp
// Create Stripe subscription with metadata
var subscriptionMetadata = new Dictionary<string, string>
{
    { "local_subscription_id", entity.Id.ToString() },
    { "local_plan_id", createDto.PlanId },
    { "local_user_id", createDto.UserId.ToString() }
};

stripeSubscriptionId = await _stripeService.CreateSubscriptionAsync(
    stripeCustomerId,
    createDto.StripePriceId,
    tokenModel,
    createDto.TrialPeriodDays,
    subscriptionMetadata  // ✅ Pass metadata
);
```

### **Fix #2: Implement Enhanced Metadata Extraction**

**File:** `backend/SmartTelehealth.API/Controllers/StripeWebhookController.cs`

**Add helper method to fetch invoice with expanded data:**

```csharp
/// <summary>
/// Fetches invoice with expanded payment intent and subscription for complete data access
/// </summary>
private async Task<Stripe.Invoice> GetInvoiceWithExpandedDataAsync(string invoiceId)
{
    try
    {
        var invoiceService = new Stripe.InvoiceService();
        var invoiceOptions = new Stripe.InvoiceGetOptions
        {
            Expand = new List<string> 
            { 
                "payment_intent",  // Expand payment intent
                "subscription",    // Expand subscription
                "customer"         // Expand customer
            }
        };

        var invoice = await invoiceService.GetAsync(invoiceId, invoiceOptions);
        _logger.LogDebug("Fetched invoice {InvoiceId} with expanded data", invoiceId);
        return invoice;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching expanded invoice {InvoiceId}", invoiceId);
        return null;
    }
}
```

**Update HandlePaymentSucceeded to use expanded data:**

```csharp
private async Task HandlePaymentSucceeded(Event stripeEvent)
{
    var invoice = stripeEvent.Data.Object as Stripe.Invoice;
    if (invoice == null) return;

    try
    {
        // Fetch invoice with expanded data for complete access
        var expandedInvoice = await GetInvoiceWithExpandedDataAsync(invoice.Id);
        if (expandedInvoice != null)
        {
            invoice = expandedInvoice;  // Use expanded invoice
        }

        // Now extraction should work better
        var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
        var paymentIntentId = GetPaymentIntentIdFromInvoice(invoice);

        // Rest of the method remains the same...
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling payment succeeded webhook for invoice {InvoiceNumber}", invoice.Number);
        throw;
    }
}
```

### **Fix #3: Basic Checkout Session Handler**

**File:** `backend/SmartTelehealth.API/Controllers/StripeWebhookController.cs`

**Replace lines 1550-1556 with basic implementation:**

```csharp
private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
{
    try
    {
        // Extract data from event JSON
        var sessionData = stripeEvent.Data.Object as Newtonsoft.Json.Linq.JObject;
        if (sessionData == null)
        {
            _logger.LogWarning("Checkout session event received but data is null");
            return;
        }

        // Extract key fields manually
        var sessionId = sessionData["id"]?.ToString();
        var customerId = sessionData["customer"]?.ToString();
        var subscriptionId = sessionData["subscription"]?.ToString();
        var paymentIntentId = sessionData["payment_intent"]?.ToString();
        var amountTotal = sessionData["amount_total"]?.Value<long?>();

        _logger.LogInformation("Processing checkout session completed: {SessionId}, Subscription: {SubscriptionId}", 
            sessionId, subscriptionId);

        if (string.IsNullOrEmpty(subscriptionId))
        {
            _logger.LogInformation("Checkout session {SessionId} does not have a subscription", sessionId);
            return;
        }

        // Get local subscription
        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200 && localSubscription.data != null)
        {
            var subscriptionData = localSubscription.data as dynamic;
            if (subscriptionData != null)
            {
                // Update subscription
                var updateDto = new UpdateSubscriptionDto
                {
                    Status = "Active",
                    LastPaymentDate = DateTime.UtcNow,
                    StripeCustomerId = customerId,
                    UpdatedDate = DateTime.UtcNow
                };

                await _subscriptionLifecycleService.UpdateSubscriptionAsync(
                    subscriptionData.Id?.ToString(), 
                    updateDto, 
                    GetToken(HttpContext));

                // Create billing record if payment was made
                if (amountTotal.HasValue && amountTotal.Value > 0)
                {
                    var billingRecordDto = new CreateBillingRecordDto
                    {
                        UserId = subscriptionData.UserId,
                        Amount = amountTotal.Value / 100m,
                        PaymentMethod = "stripe",
                        StripePaymentIntentId = paymentIntentId,
                        Status = BillingRecord.BillingStatus.Paid.ToString(),
                        Description = $"Checkout session payment - {sessionId}",
                        BillingDate = DateTime.UtcNow,
                        PaidDate = DateTime.UtcNow,
                        Type = BillingRecord.BillingType.Subscription.ToString()
                    };

                    await _billingService.CreateBillingRecordAsync(billingRecordDto, GetToken(HttpContext));
                }

                _logger.LogInformation("Checkout session {SessionId} processed successfully", sessionId);
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling checkout session completed");
        throw;
    }
}
```

---

## 📋 TESTING CHECKLIST

After implementing fixes, test the following:

### **For Checkout Sessions:**
- [ ] Create subscription via Stripe Checkout
- [ ] Verify `checkout.session.completed` webhook is processed
- [ ] Confirm local subscription is updated
- [ ] Verify billing record is created
- [ ] Check notification is sent

### **For Payment Intent Extraction:**
- [ ] Trigger `invoice.payment_succeeded` webhook
- [ ] Verify `StripePaymentIntentId` is populated in billing record
- [ ] Check logs for successful ID extraction
- [ ] Confirm no "empty ID" warnings in logs

### **For Subscription ID Extraction:**
- [ ] Trigger invoice webhook
- [ ] Verify local subscription is found by Stripe ID
- [ ] Confirm `GetSubscriptionIdFromInvoice()` returns correct ID
- [ ] Check logs for successful extraction

---

## 🎯 RECOMMENDATION SUMMARY

### **What to Do:**

1. ✅ **Immediate Action (5 minutes):**
   - Add metadata to subscription creation (Fix #1 from Option B)
   - This ensures your current system works reliably

2. ✅ **Short-term (2-3 hours):**
   - Upgrade to latest Stripe.NET (Option A)
   - Implement proper checkout session handler
   - Fix payment intent/subscription ID extraction

3. ✅ **Testing (1 hour):**
   - Run Stripe CLI tests
   - Verify all webhook events process correctly
   - Check database for proper data

### **Priority Order:**

| Fix | Priority | Effort | Impact |
|-----|----------|--------|--------|
| Add metadata to subscriptions | 🔴 HIGH | 5 min | Immediate reliability |
| Upgrade Stripe.NET | 🟡 MEDIUM | 2 hrs | Long-term robustness |
| Fix checkout handler | 🟢 LOW | 30 min | Only if using checkout |

---

## 📄 FILES TO MODIFY

### **Option A (Upgrade):**
1. ✅ `SmartTelehealth.Infrastructure.csproj` - Update Stripe.NET version
2. ✅ `SmartTelehealth.API.csproj` - Update Stripe.NET version
3. ✅ `StripeWebhookController.cs` - Fix 3 methods (checkout, payment intent, subscription ID)

### **Option B (Current Version):**
1. ✅ `StripeService.cs` - Ensure metadata in subscription creation
2. ✅ `SubscriptionLifecycleService.cs` - Pass metadata when creating subscriptions
3. ✅ `StripeWebhookController.cs` - Add expanded data fetching + basic checkout handler

---

## ✅ FINAL CHECKLIST

Before deploying:

- [ ] Backup current code
- [ ] Update Stripe.NET packages (if Option A)
- [ ] Implement fixes
- [ ] Run `dotnet build` - ensure no errors
- [ ] Run `dotnet test` - ensure tests pass
- [ ] Test with Stripe CLI
- [ ] Verify logs show successful ID extraction
- [ ] Check database for proper Stripe ID population
- [ ] Deploy to staging first
- [ ] Monitor webhook processing in production

---

**RECOMMENDATION:** Start with adding metadata (5 minutes), then schedule the Stripe.NET upgrade for your next sprint.



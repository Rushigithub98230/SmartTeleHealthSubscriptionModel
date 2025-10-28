# Plan Change Behavior Specification

## When Admin Changes a Subscription Plan - Expected Behavior

### Context
- **No Upgrade/Downgrade**: Users cannot switch between plans
- **Operations Allowed**: Cancel, Renew, Pause, Resume only
- **Refunds**: Manual only, handled by admin
- **Requirement**: Billing must remain accurate and consistent

---

## Scenario 1: Admin Changes Plan Pricing (Privileges/Commission/Discounts)

### **What Admin Does:**
- Changes `AdminCommissionPercent` (e.g., 20% → 25%)
- Adds/removes privileges from the plan
- Changes privilege quantities
- Changes `DiscountPercentage` or `BillingDiscountPercentage`
- Manually sets `BasePrice` (if using manual pricing)

### **What SHOULD Happen:**

#### **A. For Future Subscriptions (Not Yet Created)**
```
✅ New users subscribing AFTER the change
   → Get the NEW pricing immediately
   → No grace period needed
   → Clean slate, simple
```

**Example:**
```
Day 1: Plan price = $100/month
Day 5: Admin changes commission, new price = $120/month
Day 6: New user subscribes → Pays $120/month ✅
```

**Implementation:**
- Calculate price fresh when user subscribes
- Use current plan values at subscription creation time
- No special handling needed

---

#### **B. For Existing Active Subscriptions**

This is where it gets interesting. You have **3 options**:

### **Option 1: Grandfather Existing Subscriptions (RECOMMENDED)**

**Concept**: Existing users keep their original price, new users get new price

```
✅ User subscribed at $100/month
✅ Admin changes plan to $120/month
✅ Existing user continues paying $100/month
✅ New users pay $120/month
✅ No immediate impact on existing customers
```

**Advantages:**
- ✅ User-friendly (no surprise price changes)
- ✅ Legally safer (users agreed to specific price)
- ✅ No customer complaints
- ✅ Simple to implement

**Disadvantages:**
- ❌ Creates pricing inconsistency (same plan, different prices)
- ❌ Revenue loss if price increased
- ❌ May need to track "legacy" pricing

**Implementation:**
```csharp
// When user subscribes, LOCK the price
subscription.CurrentPrice = BillingCalculationService.GetEffectivePlanPrice(plan);
subscription.LockedPrice = subscription.CurrentPrice;  // Store original price
subscription.PriceLockedDate = DateTime.UtcNow;

// When recurring billing runs
// Use subscription.LockedPrice, NOT plan's current price
var billingAmount = subscription.LockedPrice;  // ✅ Grandfathered
```

**When Price Changes:**
```
Admin UI shows:
- "This plan currently has 150 active subscriptions"
- "100 subscribers at $100/month (legacy pricing)"
- "50 subscribers at $120/month (current pricing)"
```

---

### **Option 2: Apply Changes at Next Renewal (RECOMMENDED WITH NOTICE)**

**Concept**: Price changes apply at next billing cycle, with advance notice

```
Day 1:  User subscribed at $100/month, next billing Feb 1
Day 15: Admin changes plan to $120/month
Day 15: System schedules price update for Feb 1
Day 15: System sends notification to user about upcoming change
Feb 1:  User charged $120/month (new price)
```

**Advantages:**
- ✅ Gives users advance notice
- ✅ Allows users to cancel if they don't want new price
- ✅ Fair to both business and users
- ✅ Revenue captured at next cycle

**Disadvantages:**
- ❌ More complex implementation
- ❌ Need notification system
- ❌ Some users may cancel

**Implementation:**
```csharp
// When admin changes plan pricing
public async Task UpdatePlanPricingAsync(Guid planId, UpdatePlanPricingDto dto, TokenModel tokenModel)
{
    var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
    
    // Calculate new price
    var oldBasePrice = plan.BasePrice;
    var newBasePrice = CalculateNewBasePrice(dto);
    
    if (oldBasePrice != newBasePrice)
    {
        // Update plan with new pricing
        plan.BasePrice = newBasePrice;
        plan.AdminCommissionPercent = dto.CommissionPercent;
        // ... other updates
        
        await _subscriptionPlanRepository.UpdateAsync(plan);
        
        // Get all active subscriptions on this plan
        var activeSubscriptions = await _subscriptionRepository
            .GetActiveSubscriptionsByPlanIdAsync(planId);
        
        foreach (var subscription in activeSubscriptions)
        {
            // Schedule price change for next billing date
            await SchedulePriceChangeAsync(
                subscription, 
                oldPrice: subscription.CurrentPrice,
                newPrice: newBasePrice,
                effectiveDate: subscription.NextBillingDate
            );
            
            // Notify user
            await _notificationService.SendPriceChangeNotificationAsync(
                subscription,
                oldPrice: subscription.CurrentPrice,
                newPrice: newBasePrice,
                effectiveDate: subscription.NextBillingDate,
                gracePeriodDays: 7  // Time to cancel without penalty
            );
        }
        
        _logger.LogInformation(
            "Plan {PlanId} price changed from ${OldPrice} to ${NewPrice}. " +
            "{SubscriptionCount} subscriptions scheduled for update at next billing.",
            planId, oldBasePrice, newBasePrice, activeSubscriptions.Count);
    }
    
    return new JsonModel { Message = "Plan updated. Active subscriptions will be updated at next billing.", StatusCode = 200 };
}

// New entity to track scheduled price changes
public class SubscriptionPriceChange
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ScheduledDate { get; set; }
    public bool Applied { get; set; }
    public DateTime? AppliedDate { get; set; }
    public string Reason { get; set; }
}

// During recurring billing
private async Task ProcessRecurringBillingAsync(Subscription subscription)
{
    // Check for scheduled price changes
    var scheduledChange = await _priceChangeRepository
        .GetPendingChangeForSubscriptionAsync(subscription.Id, DateTime.UtcNow);
    
    if (scheduledChange != null)
    {
        _logger.LogInformation(
            "Applying scheduled price change for subscription {SubscriptionId}: ${OldPrice} → ${NewPrice}",
            subscription.Id, scheduledChange.OldPrice, scheduledChange.NewPrice);
        
        subscription.CurrentPrice = scheduledChange.NewPrice;
        scheduledChange.Applied = true;
        scheduledChange.AppliedDate = DateTime.UtcNow;
        
        await _subscriptionRepository.UpdateAsync(subscription);
        await _priceChangeRepository.UpdateAsync(scheduledChange);
    }
    
    // Then process billing with current price
    var billingAmount = subscription.CurrentPrice;
    await ProcessPaymentAsync(subscription, billingAmount);
}
```

**User Notification Example:**
```
Subject: Your subscription price will change on February 1, 2025

Dear Customer,

Your Healthcare Telehealth Professional Plan subscription price will change:

Current Price: $100.00/month
New Price: $120.00/month
Effective Date: February 1, 2025 (your next billing date)
Reason: Updated plan pricing

You have until January 25, 2025 to cancel without penalty if you do not wish to continue at the new price.

To cancel, please visit your account settings or contact support.

Thank you for your understanding.
```

---

### **Option 3: Apply Changes Immediately**

**Concept**: Price changes apply immediately to all subscriptions

```
❌ NOT RECOMMENDED for your use case
Reasons:
- Users might be mid-cycle (already paid for current period)
- Creates customer complaints
- May require prorated refunds/charges
- You said refunds are manual only
```

**When This Makes Sense:**
- When price DECREASES (users happy to save money)
- When legally required (e.g., tax rate changes)
- When feature set changes significantly

**If You Must Do This:**
```csharp
// Update all subscriptions immediately
var activeSubscriptions = await _subscriptionRepository
    .GetActiveSubscriptionsByPlanIdAsync(planId);

foreach (var subscription in activeSubscriptions)
{
    var oldPrice = subscription.CurrentPrice;
    var newPrice = BillingCalculationService.GetEffectivePlanPrice(plan);
    
    subscription.CurrentPrice = newPrice;
    
    // Log the change for manual refund processing
    if (newPrice < oldPrice)
    {
        // Price decreased - consider partial refund for current period
        var daysRemaining = (subscription.NextBillingDate - DateTime.UtcNow).Days;
        var daysInCycle = (subscription.NextBillingDate - subscription.LastBilledDate).Days;
        var refundAmount = (oldPrice - newPrice) * (daysRemaining / (decimal)daysInCycle);
        
        await CreateManualRefundTaskAsync(subscription, refundAmount, 
            $"Price decrease from ${oldPrice} to ${newPrice}");
    }
    else if (newPrice > oldPrice)
    {
        // Price increased - user keeps current price until next renewal
        // OR charge prorated difference immediately
        // Since you do manual refunds only, probably keep current price
        subscription.CurrentPrice = oldPrice;  // Keep until renewal
        subscription.PendingPriceChange = newPrice;  // Apply next cycle
    }
    
    await _subscriptionRepository.UpdateAsync(subscription);
    await _notificationService.SendPriceChangeNotificationAsync(subscription, oldPrice, newPrice);
}
```

---

## Scenario 2: Admin Changes Plan Features/Privileges

### **What Admin Does:**
- Adds a new privilege to the plan (e.g., adds "Group Video Calls")
- Removes a privilege from the plan
- Changes privilege quantities (e.g., 50 video calls → 100 video calls)

### **What SHOULD Happen:**

#### **A. If Price Changes Due to Feature Changes:**
Follow **Option 1 or 2** from above (Grandfather or Apply at Renewal)

#### **B. If Price Stays Same but Features Change:**

```csharp
// Example: Admin adds "Chat Support" privilege at no extra cost
// All existing subscriptions should get the new feature immediately

foreach (var subscription in activeSubscriptions)
{
    // Allocate new privilege to existing subscription
    await AllocateNewPrivilegeAsync(subscription, newPrivilege);
    
    _logger.LogInformation(
        "Added new privilege {PrivilegeName} to subscription {SubscriptionId}",
        newPrivilege.Name, subscription.Id);
    
    // Notify user about the enhancement
    await _notificationService.SendFeatureAddedNotificationAsync(
        subscription, newPrivilege);
}
```

**User Notification:**
```
Subject: New feature added to your plan!

We've added a new feature to your Healthcare Telehealth Professional Plan:

✨ NEW: Unlimited Chat Support

This feature is now available in your account at no additional cost.

Log in to start using it today!
```

---

## Scenario 3: Admin Deprecates/Deactivates Plan

### **What Admin Does:**
- Sets `IsActive = false` on the plan
- Wants to stop new subscriptions but keep existing ones

### **What SHOULD Happen:**

```csharp
// When admin deactivates plan
plan.IsActive = false;
plan.DeactivatedDate = DateTime.UtcNow;
await _subscriptionPlanRepository.UpdateAsync(plan);

// Prevent NEW subscriptions
public async Task<JsonModel> CreateSubscriptionAsync(CreateSubscriptionDto dto)
{
    var plan = await _subscriptionPlanRepository.GetByIdAsync(dto.PlanId);
    
    if (!plan.IsActive)
    {
        return new JsonModel 
        { 
            Message = "This plan is no longer available for new subscriptions", 
            StatusCode = 400 
        };
    }
    
    // Continue with subscription creation...
}

// KEEP existing subscriptions active
// They continue until user cancels or subscription expires
// Renewals continue normally
```

**Best Practice:**
```csharp
// Admin UI warning
"You are about to deactivate this plan.
 - New subscriptions will be blocked
 - {activeSubscriptionCount} existing subscriptions will continue unchanged
 - Users can still renew, pause, and resume
 - To migrate users, please contact support"
```

---

## Scenario 4: Admin Changes Discount Percentages

### **What Admin Does:**
- Changes `DiscountPercentage` (e.g., 10% → 15% holiday sale)
- Changes `BillingDiscountPercentage` (e.g., removes annual discount)
- Sets `DiscountValidUntil` date

### **What SHOULD Happen:**

#### **A. Discount Increase (More Savings)**
```csharp
// Apply immediately to all subscriptions (users happy)
foreach (var subscription in activeSubscriptions)
{
    var oldPrice = subscription.CurrentPrice;
    var newPrice = BillingCalculationService.GetEffectivePlanPrice(plan);
    
    if (newPrice < oldPrice)
    {
        subscription.CurrentPrice = newPrice;
        await _subscriptionRepository.UpdateAsync(subscription);
        
        await _notificationService.SendGoodNewsAsync(
            subscription,
            $"Great news! Your subscription price decreased from ${oldPrice} to ${newPrice}/month"
        );
    }
}
```

#### **B. Discount Decrease (Less Savings)**
```csharp
// Apply at next renewal (give notice)
// Same as Option 2 from Scenario 1
await SchedulePriceChangeAsync(subscription, oldPrice, newPrice, subscription.NextBillingDate);
```

#### **C. Discount Expiry**
```csharp
// When discount expires (DiscountValidUntil passes)
// Automatic background job handles this

public class DiscountExpiryBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiredDiscountsAsync();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
    
    private async Task CheckExpiredDiscountsAsync()
    {
        var plansWithExpiredDiscounts = await _subscriptionPlanRepository
            .GetPlansWithExpiredDiscountsAsync(DateTime.UtcNow);
        
        foreach (var plan in plansWithExpiredDiscounts)
        {
            _logger.LogInformation(
                "Discount expired for plan {PlanId}. {DiscountPercent}% discount no longer valid.",
                plan.Id, plan.DiscountPercentage);
            
            // Get subscriptions on this plan
            var subscriptions = await _subscriptionRepository
                .GetActiveSubscriptionsByPlanIdAsync(plan.Id);
            
            foreach (var subscription in subscriptions)
            {
                var oldPrice = subscription.CurrentPrice;
                
                // Recalculate without expired discount
                var newPrice = BillingCalculationService.GetEffectivePlanPrice(plan);
                
                // Schedule price change for next billing
                await SchedulePriceChangeAsync(
                    subscription, 
                    oldPrice, 
                    newPrice, 
                    subscription.NextBillingDate
                );
                
                // Notify user
                await _notificationService.SendDiscountExpiryNotificationAsync(
                    subscription, 
                    oldPrice, 
                    newPrice, 
                    subscription.NextBillingDate
                );
            }
        }
    }
}
```

---

## Scenario 5: Admin Changes Commission Percentage

### **What Admin Does:**
- Changes `AdminCommissionPercent` from 20% to 25%
- Business decision to increase revenue

### **What SHOULD Happen:**

This is a **business policy decision**. Two approaches:

#### **Approach A: Transparent to Users**
```csharp
// Users don't see commission breakdown
// They just see total price
// Follow Option 1 or 2 from Scenario 1

// If BasePrice increases due to commission change:
// - Grandfather existing users (Option 1), OR
// - Apply at next renewal with notice (Option 2)
```

#### **Approach B: Commission Shown Separately**
```csharp
// If users see commission as separate line item
// More transparency but more complex

// Billing breakdown shown to user:
// Base Price (Privileges): $1000
// Admin Commission (20%): $200
// Total: $1200

// When commission changes to 25%:
// Base Price (Privileges): $1000  (unchanged)
// Admin Commission (25%): $250     (changed)
// Total: $1250

// Apply change at next renewal with explanation
```

---

## Recommended Implementation Strategy

### **For Your Use Case (Cancel/Renew/Pause/Resume Only):**

**I RECOMMEND: Option 2 - Apply Changes at Next Renewal with Notice**

**Why:**
- ✅ Fair to users (advance notice)
- ✅ Fair to business (revenue captured)
- ✅ Legally safer (no surprise charges)
- ✅ Reduces customer complaints
- ✅ Aligns with "manual refunds" approach (no complex proration)

### **Implementation Checklist:**

```csharp
✅ 1. Auto-recalculate BasePrice when admin changes plan
     → Prevents stale pricing

✅ 2. Track scheduled price changes
     → New entity: SubscriptionPriceChange

✅ 3. Apply changes at next billing date
     → Check for scheduled changes during renewal

✅ 4. Send advance notifications
     → Email/SMS 7-14 days before change

✅ 5. Allow grace period for cancellation
     → Users can cancel without penalty

✅ 6. Log all price changes
     → Audit trail for compliance

✅ 7. Admin dashboard shows impact
     → "This change affects 150 subscriptions"
     → "Estimated revenue impact: +$7,500/month"

✅ 8. Handle discount expiry automatically
     → Background job checks daily

✅ 9. Grandfather OR migrate user choice
     → Admin selects strategy per change
```

### **Admin UI Flow:**

```
Admin changes plan pricing:

┌─────────────────────────────────────────────────────────────┐
│ Update Plan Pricing                                         │
├─────────────────────────────────────────────────────────────┤
│ Current Price: $100/month                                   │
│ New Price: $120/month                                       │
│                                                             │
│ ⚠️  This plan has 150 active subscriptions                  │
│                                                             │
│ How should existing subscriptions be handled?              │
│                                                             │
│ ○ Grandfather (keep current pricing)                       │
│   → Users continue paying $100/month                        │
│   → New users pay $120/month                                │
│   → Revenue impact: $0 (legacy users unchanged)             │
│                                                             │
│ ● Apply at next renewal (recommended)                      │
│   → Users notified 7 days in advance                        │
│   → New price applies at next billing date                  │
│   → Revenue impact: +$3,000/month                           │
│                                                             │
│ ○ Apply immediately                                         │
│   → ⚠️  May cause customer complaints                       │
│   → ⚠️  May require manual refunds                          │
│   → Not recommended                                         │
│                                                             │
│ Notification message:                                       │
│ ┌───────────────────────────────────────────────────────┐ │
│ │ Your plan pricing will change on {NextBillingDate}   │ │
│ │ from $100 to $120/month due to updated pricing.      │ │
│ │ You may cancel before {GraceDate} without penalty.   │ │
│ └───────────────────────────────────────────────────────┘ │
│                                                             │
│ [ Cancel ]  [ Update Plan ]                                 │
└─────────────────────────────────────────────────────────────┘
```

---

## Summary

### **Key Principles:**

1. **New Subscriptions**: Always use current plan pricing
2. **Existing Subscriptions**: Apply changes at next renewal with notice (recommended)
3. **Price Decreases**: Apply immediately (users happy)
4. **Price Increases**: Apply at renewal with 7-14 day notice
5. **Discount Expiry**: Automatic background job handles
6. **Feature Additions**: Apply immediately at no cost
7. **Plan Deactivation**: Block new subscriptions, keep existing
8. **Always Log Changes**: Full audit trail
9. **Always Notify Users**: Transparency builds trust
10. **Auto-recalculate BasePrice**: Prevent stale pricing

### **What You Should Implement Now:**

1. **Auto-recalculate BasePrice** when admin changes commission/privileges
2. **Add SubscriptionPriceChange** entity to track scheduled changes
3. **Update recurring billing** to check for scheduled price changes
4. **Add notification system** for price change alerts
5. **Add grace period** for cancellations without penalty

This ensures your billing remains **accurate, consistent, and fair** to both business and users!



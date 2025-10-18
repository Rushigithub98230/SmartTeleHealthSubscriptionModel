# 🏥 Example: Creating a Healthcare Subscription Plan

## Complete Example - Mental Health Support Plan

This example demonstrates creating a **production-ready healthcare subscription plan** with the new system.

---

## Step 1: Define Your Plan Structure

### Plan Overview:
- **Name:** Mental Health Comprehensive
- **Target Users:** Patients seeking regular mental health support
- **Billing:** Monthly subscription
- **Pricing Mode:** Auto-calculated from privileges
- **Commission:** 20% (global default)
- **Notice Period:** 10 days (default)

### Included Privileges:

| Privilege | Quantity | Base Cost | Overage Cost | Contribution to Plan |
|-----------|----------|-----------|--------------|---------------------|
| Therapy Sessions | 4 | $30.00 | $60.00 | $120.00 |
| Group Therapy | 2 | $15.00 | $30.00 | $30.00 |
| Messaging | 100 | $0.10 | $0.25 | $10.00 |
| Crisis Support | Unlimited | $0.00 | $0.00 | $0.00 |
| Medication Delivery | 1 | $10.00 | $20.00 | $10.00 |

**Total Privilege Cost:** $170.00  
**Commission (20%):** $34.00  
**Final Plan Price:** **$204.00/month**

---

## Step 2: API Request

```http
POST /api/SubscriptionPlans
Content-Type: application/json
Authorization: Bearer <admin-token>

{
  "name": "Mental Health Comprehensive",
  "description": "Complete mental health support with therapy, group sessions, unlimited messaging, and medication delivery",
  "shortDescription": "Comprehensive mental health care plan",
  
  // Required IDs (get these from master data endpoints)
  "billingCycleId": "11111111-1111-1111-1111-111111111111",  // Monthly
  "currencyId": "22222222-2222-2222-2222-222222222222",      // USD
  "categoryId": "33333333-3333-3333-3333-333333333333",      // Mental Health
  
  // Healthcare Pricing Model
  "isAutoCalculatedPrice": true,     // ✅ Auto-calculate from privileges
  "price": 0,                         // Will be auto-calculated to $204.00
  "adminCommissionPercent": null,     // ✅ Use global default (20%)
  "adminCommissionFixed": null,
  "priceChangeNoticeDays": 10,        // ✅ 10 days notice for price changes
  
  // Trial Configuration
  "isTrialAllowed": true,
  "trialDurationInDays": 14,
  
  // Marketing
  "isFeatured": true,
  "isMostPopular": true,
  "isTrending": false,
  "displayOrder": 1,
  
  // Plan Features
  "messagingCount": 100,
  "includesMedicationDelivery": true,
  "includesFollowUpCare": true,
  "deliveryFrequencyDays": 30,
  "maxPauseDurationDays": 60,
  "maxConcurrentUsers": 1,
  "gracePeriodDays": 3,
  
  // Status
  "isActive": true,
  
  // Metadata
  "features": "4 therapy sessions, 2 group sessions, unlimited crisis support, medication delivery",
  "terms": "Subject to availability. 24-hour cancellation policy applies.",
  
  // ═══════════════════════════════════════════════════════════
  // PRIVILEGES CONFIGURATION
  // ═══════════════════════════════════════════════════════════
  
  "privileges": [
    {
      // Privilege 1: Individual Therapy Sessions
      "privilegeId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "value": 4,                    // 4 sessions included
      "usagePeriodId": "period-monthly-id",
      "durationMonths": 1,
      "privilegeBaseCost": 30.00,    // ✅ $30 per session for plan calculation
      "unitCost": 60.00,             // ✅ $60 per overage session (2× base)
      "dailyLimit": 1,               // Max 1 session per day
      "weeklyLimit": 2,              // Max 2 sessions per week
      "monthlyLimit": 4,             // Max 4 sessions per month
      "description": "One-on-one therapy with licensed therapist"
    },
    {
      // Privilege 2: Group Therapy Sessions
      "privilegeId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      "value": 2,                    // 2 group sessions included
      "usagePeriodId": "period-monthly-id",
      "privilegeBaseCost": 15.00,    // ✅ $15 per session for plan calculation
      "unitCost": 30.00,             // ✅ $30 per overage session
      "monthlyLimit": 2,
      "description": "Group therapy sessions (max 8 participants)"
    },
    {
      // Privilege 3: Secure Messaging
      "privilegeId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
      "value": 100,                  // 100 messages included
      "usagePeriodId": "period-monthly-id",
      "privilegeBaseCost": 0.10,     // ✅ $0.10 per message for plan calculation
      "unitCost": 0.25,              // ✅ $0.25 per overage message
      "dailyLimit": 20,
      "monthlyLimit": 100,
      "description": "Secure messaging with care team"
    },
    {
      // Privilege 4: Crisis Support (Unlimited)
      "privilegeId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
      "value": -1,                   // ✅ -1 = Unlimited
      "usagePeriodId": "period-monthly-id",
      "privilegeBaseCost": 0.00,     // ✅ Unlimited contributes $0 to plan price
      "unitCost": 0.00,              // ✅ No overage for unlimited
      "description": "24/7 crisis support hotline"
    },
    {
      // Privilege 5: Medication Delivery
      "privilegeId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
      "value": 1,                    // 1 delivery included
      "usagePeriodId": "period-monthly-id",
      "privilegeBaseCost": 10.00,    // ✅ $10 for plan calculation
      "unitCost": 20.00,             // ✅ $20 per extra delivery
      "monthlyLimit": 1,
      "description": "Monthly medication delivery to your door"
    }
  ]
}
```

---

## Step 3: Expected Response

```json
{
  "data": {
    "id": "12345678-1234-1234-1234-123456789012",
    "name": "Mental Health Comprehensive",
    "description": "Complete mental health support...",
    
    // ✅ HEALTHCARE PRICING CALCULATED AUTOMATICALLY
    "price": 204.00,                    // Auto-calculated!
    "privilegesTotalCost": 170.00,      // $120 + $30 + $10 + $0 + $10
    "calculatedPrice": 204.00,           // $170 + $34 commission
    
    // Versioning
    "versionNumber": 1,
    "isLatestVersion": true,
    "parentPlanId": null,
    "versionCreatedDate": "2025-01-20T10:30:00Z",
    
    // Pricing Configuration
    "isAutoCalculatedPrice": true,
    "adminCommissionPercent": null,      // Using global 20%
    "priceChangeNoticeDays": 10,
    
    // Stripe Integration
    "stripeProductId": "prod_ABC123",
    "stripeMonthlyPriceId": "price_XYZ789",
    "stripeQuarterlyPriceId": "price_XYZ790",
    "stripeAnnualPriceId": "price_XYZ791",
    
    // Trial
    "isTrialAllowed": true,
    "trialDurationInDays": 14,
    
    // Status
    "isActive": true,
    "isCurrentlyAvailable": true,
    "hasActiveDiscount": false
  },
  "message": "Plan created successfully with privileges",
  "statusCode": 201
}
```

---

## Step 4: Verify Pricing Breakdown

```http
GET /api/SubscriptionPlans/12345678-1234-1234-1234-123456789012/pricing-breakdown
```

**Response:**
```json
{
  "data": {
    "planId": "12345678-1234-1234-1234-123456789012",
    "planName": "Mental Health Comprehensive",
    "isAutoCalculated": true,
    "privilegeBreakdown": [
      {
        "privilegeName": "Therapy Sessions",
        "quantity": 4,
        "unitBaseCost": 30.00,
        "totalCost": 120.00,        // ✅ 4 × $30
        "overageUnitCost": 60.00
      },
      {
        "privilegeName": "Group Therapy",
        "quantity": 2,
        "unitBaseCost": 15.00,
        "totalCost": 30.00,         // ✅ 2 × $15
        "overageUnitCost": 30.00
      },
      {
        "privilegeName": "Messaging",
        "quantity": 100,
        "unitBaseCost": 0.10,
        "totalCost": 10.00,         // ✅ 100 × $0.10
        "overageUnitCost": 0.25
      },
      {
        "privilegeName": "Medication Delivery",
        "quantity": 1,
        "unitBaseCost": 10.00,
        "totalCost": 10.00,         // ✅ 1 × $10
        "overageUnitCost": 20.00
      }
    ],
    "privilegesTotalCost": 170.00,  // ✅ Sum of all privilege costs
    "commissionPercent": 20,        // ✅ Global default
    "commissionAmount": 34.00,      // ✅ $170 × 20%
    "isFixedCommission": false,
    "finalPrice": 204.00            // ✅ $170 + $34
  },
  "message": "Pricing breakdown retrieved successfully",
  "statusCode": 200
}
```

---

## Step 5: User Subscribes

Alice subscribes on **January 20, 2025**:

```http
POST /api/Subscriptions
{
  "userId": 12345,
  "planId": "12345678-1234-1234-1234-123456789012",
  "billingCycleId": "monthly-id",
  "startTrial": true
}
```

**Result:**
- Alice gets Plan v1 at **$204.00/month**
- Next billing: **February 20, 2025**
- Trial ends: **February 3, 2025** (14 days)

---

## Step 6: Admin Updates Pricing (February 1)

Market conditions change, admin needs to increase price to $250/month:

```http
POST /api/SubscriptionPlans/12345678-1234-1234-1234-123456789012/versions
{
  "name": "Mental Health Comprehensive",
  "description": "Updated pricing for 2025",
  "price": 250.00,                 // Manual pricing for this version
  "billingCycleId": "monthly-id",
  "currencyId": "usd-id",
  "categoryId": "mental-health-id",
  "isActive": true,
  "isAutoCalculatedPrice": false,  // ✅ Manual pricing override
  "priceChangeNoticeDays": 10
}
```

**What Happens:**

1. ✅ System creates **Plan v2** at $250/month
2. ✅ Alice **stays on Plan v1** at $204/month
3. ✅ System schedules migration:
   - Alice's next renewal: **Feb 20**
   - Minimum notice: **10 days** (Feb 1 + 10 = Feb 11)
   - Migration date: **Feb 20** ✅ (meets 10-day requirement)
4. ✅ Alice receives notification email
5. ✅ New subscribers get Plan v2 at $250/month

---

## Step 7: Alice Buys Overage (February 5)

Alice needs 2 extra therapy sessions before her renewal:

```http
POST /api/Subscriptions/<alice-subscription-id>/purchase-privilege
{
  "privilegeId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "quantity": 2
}
```

**Healthcare Pricing Logic:**

```
Alice's current plan: v1 ($204/month)
Latest plan version: v2 ($250/month)

❓ Which overage price to use?
✅ ANSWER: Use v2 pricing (abuse prevention)

Calculation:
- Get overage cost from Plan v2: $60 per session
- Total: 2 × $60 = $120
- Alice is charged $120 (not v1's $30 × 2 = $60)

Why?
- Prevents users from staying on old plans for cheaper overages
- Fair market pricing
- Healthcare-compliant
```

**Backend Code:**
```csharp
var billingResult = await _billingService.CreateHealthcareOverageBillingAsync(
    aliceSubscriptionId,
    therapyPrivilegeId,
    2,  // quantity
    tokenModel
);

// Result: Billing record created for $120
```

---

## Step 8: Alice Responds to Migration (February 10)

Alice checks her migration:

```http
GET /api/UserSubscription/my-subscription/migration
Authorization: Bearer <alice-token>
```

**Response:**
```json
{
  "data": {
    "hasScheduledMigration": true,
    "migration": {
      "fromPlan": {
        "name": "Mental Health Comprehensive",
        "price": 204.00,
        "versionNumber": 1
      },
      "toPlan": {
        "name": "Mental Health Comprehensive",
        "price": 250.00,
        "versionNumber": 2
      },
      "scheduledMigrationDate": "2025-02-20",
      "daysUntilMigration": 10,
      "status": "Pending"
    }
  }
}
```

Alice decides to **accept** the price change:

```http
POST /api/UserSubscription/my-subscription/migration/respond
Authorization: Bearer <alice-token>

{
  "subscriptionId": "<alice-subscription-id>",
  "decision": "Accept",
  "reason": "The service is valuable, I accept the new pricing"
}
```

---

## Step 9: Automated Migration (February 20 @ 2 AM)

Background service runs:

```
ScheduledMigrationBackgroundService @ 2:00 AM
┌─────────────────────────────────────────────────┐
│ 1. Query: Migrations due on Feb 20?            │
│    → Found: Alice's migration                   │
│                                                  │
│ 2. Process Alice's migration:                   │
│    ✅ Update subscription.PlanId: v1 → v2       │
│    ✅ Update subscription.Price: $204 → $250    │
│    ✅ Update Stripe subscription to new price   │
│    ✅ Mark migration status: "Completed"        │
│                                                  │
│ 3. Log: "1 migration completed, 0 failed"       │
└─────────────────────────────────────────────────┘
```

**Database Changes:**
```sql
-- Before migration (Feb 19)
SELECT * FROM Subscriptions WHERE Id = '<alice-sub-id>';
-- SubscriptionPlanId: <v1-id>
-- CurrentPrice: 204.00
-- NextBillingDate: 2025-02-20

-- After migration (Feb 20)
SELECT * FROM Subscriptions WHERE Id = '<alice-sub-id>';
-- SubscriptionPlanId: <v2-id>  ✅ CHANGED
-- CurrentPrice: 250.00         ✅ CHANGED
-- NextBillingDate: 2025-03-20  ✅ Next billing updated

SELECT * FROM ScheduledPlanMigrations WHERE SubscriptionId = '<alice-sub-id>';
-- Status: "Completed"  ✅
-- CompletedDate: 2025-02-20 02:00:05
```

---

## Step 10: Alice's Next Billing (March 20)

```
March 20 @ 2 AM:
┌─────────────────────────────────────────────────┐
│ AutomatedBillingBackgroundService               │
├─────────────────────────────────────────────────┤
│ 1. Find subscriptions due for billing           │
│    → Alice's subscription (now on Plan v2)      │
│                                                  │
│ 2. Create billing record:                       │
│    Amount: $250.00  ✅ (v2 pricing)             │
│    Type: "Subscription"                          │
│    Description: "Mental Health Comprehensive v2"│
│                                                  │
│ 3. Process Stripe payment                       │
│    ✅ Alice charged $250.00                     │
│                                                  │
│ 4. Update next billing: April 20                │
└─────────────────────────────────────────────────┘
```

---

## 🎯 Complete Example with Different Scenarios

### Scenario A: Bob Downgrades

Bob receives the same price change notification but chooses to downgrade:

```http
POST /api/UserSubscription/my-subscription/migration/respond
Authorization: Bearer <bob-token>

{
  "subscriptionId": "<bob-subscription-id>",
  "decision": "Downgrade",
  "downgradeToPlanId": "<basic-plan-id>",  // Basic plan at $99/month
  "reason": "New price is outside my budget, switching to basic plan"
}
```

**On Bob's renewal date:**
- ❌ Does NOT migrate to v2 ($250)
- ✅ Migrates to Basic plan ($99/month)
- ✅ Loses some privileges from Comprehensive plan
- ✅ Saves money while keeping core features

### Scenario C: Charlie Cancels

Charlie decides not to continue with the price increase:

```http
POST /api/UserSubscription/my-subscription/migration/respond
Authorization: Bearer <charlie-token>

{
  "subscriptionId": "<charlie-subscription-id>",
  "decision": "Cancel",
  "reason": "Price increase is too high for my budget"
}
```

**What Happens:**
- ✅ `AutoRenew` set to `false`
- ✅ Migration status: "UserOptedOut"
- ✅ Subscription continues until current period ends
- ✅ No renewal on next billing date
- ✅ Subscription expires gracefully

---

## 💰 Pricing Comparison

### Without Healthcare Model (OLD - Bad):
```
Plan at $204/month, overage therapy $30 each

User abuses:
1. Stay on old plan forever
2. Buy overage at $30 (old price)
3. Market rate: $60 (new price)
4. Your loss: $30 per overage session

If 100 users do this × 5 extra sessions each = $15,000 loss/month!
```

### With Healthcare Model (NEW - Good):
```
Plan v1 at $204/month
Plan v2 at $250/month (therapy overage $60)

User on v1 buys overage:
1. System checks: Is v1 latest? NO
2. Gets latest version: v2
3. Gets overage from v2: $60 per session
4. ✅ Charges user $60 (latest pricing)
5. ✅ No abuse possible
6. User migrates to v2 at next renewal anyway

Result: Fair pricing, no losses!
```

---

## 📊 Admin Dashboard Queries

### View All Plan Versions

```sql
SELECT 
    sp.Id,
    sp.Name,
    sp.VersionNumber,
    sp.IsLatestVersion,
    sp.Price,
    sp.IsAutoCalculatedPrice,
    sp.PrivilegesTotalCost,
    sp.PriceChangeNoticeDays,
    COUNT(s.Id) AS ActiveSubscriptions,
    SUM(CASE WHEN s.Status = 'Active' THEN s.CurrentPrice ELSE 0 END) AS MonthlyRevenue
FROM SubscriptionPlans sp
LEFT JOIN Subscriptions s ON sp.Id = s.SubscriptionPlanId AND s.Status = 'Active'
WHERE sp.ParentPlanId = '<parent-plan-id>' OR sp.Id = '<parent-plan-id>'
GROUP BY sp.Id, sp.Name, sp.VersionNumber, sp.IsLatestVersion, sp.Price, 
         sp.IsAutoCalculatedPrice, sp.PrivilegesTotalCost, sp.PriceChangeNoticeDays
ORDER BY sp.VersionNumber;
```

### View Pending Migrations

```sql
SELECT 
    m.Id,
    m.ScheduledMigrationDate,
    m.Status,
    m.UserDecision,
    u.Email AS UserEmail,
    fp.Name AS FromPlan,
    fp.Price AS OldPrice,
    tp.Name AS ToPlan,
    tp.Price AS NewPrice,
    DATEDIFF(day, GETUTCDATE(), m.ScheduledMigrationDate) AS DaysUntilMigration
FROM ScheduledPlanMigrations m
INNER JOIN Subscriptions s ON m.SubscriptionId = s.Id
INNER JOIN Users u ON s.UserId = u.Id
INNER JOIN SubscriptionPlans fp ON m.FromPlanId = fp.Id
INNER JOIN SubscriptionPlans tp ON m.ToPlanId = tp.Id
WHERE m.Status = 'Pending'
ORDER BY m.ScheduledMigrationDate;
```

---

## 🎓 Key Takeaways

### For Plan Creation:
1. ✅ Set `isAutoCalculatedPrice = true` for transparency
2. ✅ Define `privilegeBaseCost` for each privilege (plan pricing)
3. ✅ Define `unitCost` for overage (can be different from base cost)
4. ✅ Commission applied automatically (20% default)
5. ✅ Price calculated: Σ(quantity × baseCost) + commission

### For Price Changes:
1. ✅ Create new version, don't modify existing
2. ✅ Users stay on old version until their renewal
3. ✅ Each user gets 10 days minimum notice
4. ✅ Users can accept, downgrade, or cancel
5. ✅ Overage uses latest pricing (abuse prevention)

### For Users:
1. ✅ No surprise charges
2. ✅ Individual migration dates (fair)
3. ✅ Clear options (accept/downgrade/cancel)
4. ✅ Market-rate overage pricing (prevents abuse)
5. ✅ Transparent pricing breakdown

---

## ✨ Production Checklist

Before going live:

- [ ] Run `VersionExistingPlans.sql` to auto-version existing plans
- [ ] Verify all plans have `VersionNumber = 1`
- [ ] Set global commission in `SystemSettings`
- [ ] Test plan creation with auto-pricing
- [ ] Test plan creation with manual pricing
- [ ] Test version creation with active subscribers
- [ ] Verify migrations are scheduled correctly
- [ ] Test user migration response flow
- [ ] Verify background service runs daily
- [ ] Test overage billing uses latest pricing
- [ ] Monitor logs for any errors
- [ ] Set up alerts for failed migrations

---

**You're ready for production! 🚀**


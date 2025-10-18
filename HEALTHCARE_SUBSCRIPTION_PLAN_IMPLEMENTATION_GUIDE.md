# 🏥 Healthcare Subscription Plan Management - Implementation Guide

## Overview

This guide demonstrates how to use the new healthcare-specific subscription plan management system with:
- **Plan Versioning** (Issue #1 Fix)
- **Privilege-Based Pricing** (Auto-calculated or Manual)
- **Configurable Commission** (Global or Per-Plan)
- **Migrate-at-Renewal** (Healthcare-safe price changes)
- **Abuse Prevention** (Overage uses latest plan pricing)

---

## ✅ Features Implemented

### Core Features
- ✅ Plan versioning (v1, v2, v3, etc.)
- ✅ Auto-calculated pricing from privileges + commission
- ✅ Manual pricing override option (Choice 1c)
- ✅ Global commission with per-plan override (Choice 2c)
- ✅ Auto-version existing plans as v1.0 (Choice 3a)
- ✅ Configurable notice period per plan (Choice 4d) - **Default: 10 days**
- ✅ Individual user migration at renewal dates
- ✅ Overage pricing from latest plan version (abuse prevention)
- ✅ User migration response options (Accept/Downgrade/Cancel)
- ✅ Background service for automated migrations
- ✅ Comprehensive API endpoints

---

## 🚀 Quick Start

### Step 1: Run Database Migration

```bash
# Navigate to Infrastructure project
cd backend/SmartTelehealth.Infrastructure

# Apply EF Core migration
dotnet ef database update --context ApplicationDbContext --startup-project ../SmartTelehealth.API

# Run SQL script to auto-version existing plans
# Execute: Migrations/Scripts/VersionExistingPlans.sql
```

### Step 2: Create a Healthcare-Compliant Subscription Plan

#### Example: Mental Health Plan with Auto-Calculated Pricing

```http
POST /api/SubscriptionPlans
Content-Type: application/json

{
  "name": "Mental Health Basic",
  "description": "Comprehensive mental health support with therapy sessions and messaging",
  "shortDescription": "Basic mental health support plan",
  "billingCycleId": "<monthly-billing-cycle-id>",
  "currencyId": "<usd-currency-id>",
  "categoryId": "<mental-health-category-id>",
  
  // Healthcare Pricing Model (Choice 1c: Auto-calculated)
  "isAutoCalculatedPrice": true,
  
  // Choice 2c: Per-plan commission (or use null for global default of 20%)
  "adminCommissionPercent": 25,  // 25% for this plan
  
  // Choice 4d: Configurable notice period (default: 10 days)
  "priceChangeNoticeDays": 10,
  
  // Trial configuration
  "isTrialAllowed": true,
  "trialDurationInDays": 14,
  
  // Marketing
  "isFeatured": true,
  "isMostPopular": true,
  "displayOrder": 1,
  
  // Privileges with base costs and overage costs
  "privileges": [
    {
      "privilegeId": "<therapy-session-privilege-id>",
      "value": 4,  // 4 therapy sessions included
      "usagePeriodId": "<monthly-usage-period-id>",
      "privilegeBaseCost": 25.00,  // Each session costs $25 for plan calculation
      "unitCost": 50.00,           // Overage sessions cost $50 each
      "dailyLimit": 1,
      "monthlyLimit": 4
    },
    {
      "privilegeId": "<messaging-privilege-id>",
      "value": 50,  // 50 messages included
      "usagePeriodId": "<monthly-usage-period-id>",
      "privilegeBaseCost": 0.20,  // Each message costs $0.20 for plan calculation
      "unitCost": 0.50,           // Overage messages cost $0.50 each
      "dailyLimit": 10,
      "monthlyLimit": 50
    },
    {
      "privilegeId": "<crisis-support-privilege-id>",
      "value": -1,  // Unlimited crisis support
      "usagePeriodId": "<monthly-usage-period-id>",
      "privilegeBaseCost": 0,  // Unlimited = $0 contribution
      "unitCost": 0            // No overage for unlimited
    }
  ]
}
```

**Result:**
```json
{
  "data": {
    "id": "<new-plan-id>",
    "name": "Mental Health Basic",
    "versionNumber": 1,
    "isLatestVersion": true,
    "isAutoCalculatedPrice": true,
    
    // Auto-calculated pricing:
    // Therapy: 4 × $25 = $100
    // Messaging: 50 × $0.20 = $10
    // Crisis Support: Unlimited = $0
    // Subtotal: $110
    // Commission (25%): $27.50
    // FINAL PRICE: $137.50
    
    "price": 137.50,
    "privilegesTotalCost": 110.00,
    "adminCommissionPercent": 25,
    "priceChangeNoticeDays": 10
  },
  "message": "Plan created successfully with privileges",
  "statusCode": 201
}
```

---

## 📊 Pricing Breakdown Endpoint

```http
GET /api/SubscriptionPlans/<plan-id>/pricing-breakdown
```

**Response:**
```json
{
  "data": {
    "planId": "<plan-id>",
    "planName": "Mental Health Basic",
    "isAutoCalculated": true,
    "privilegeBreakdown": [
      {
        "privilegeName": "Therapy Sessions",
        "quantity": 4,
        "unitBaseCost": 25.00,
        "totalCost": 100.00,
        "overageUnitCost": 50.00
      },
      {
        "privilegeName": "Messaging",
        "quantity": 50,
        "unitBaseCost": 0.20,
        "totalCost": 10.00,
        "overageUnitCost": 0.50
      }
    ],
    "privilegesTotalCost": 110.00,
    "commissionPercent": 25,
    "commissionAmount": 27.50,
    "isFixedCommission": false,
    "finalPrice": 137.50
  },
  "message": "Pricing breakdown retrieved successfully",
  "statusCode": 200
}
```

---

## 🔄 Creating a New Plan Version (Price Change)

### Scenario: Increase price from $137.50 to $175

```http
POST /api/SubscriptionPlans/<plan-id>/versions
Content-Type: application/json

{
  "name": "Mental Health Basic",
  "description": "Updated pricing for 2025",
  "price": 175.00,
  "billingCycleId": "<monthly-billing-cycle-id>",
  "currencyId": "<usd-currency-id>",
  "categoryId": "<mental-health-category-id>",
  "isActive": true,
  "isAutoCalculatedPrice": false,  // Using manual pricing for this version
  "priceChangeNoticeDays": 10      // 10 days notice
}
```

**What Happens:**

1. ✅ System creates **Plan v2** at $175/month
2. ✅ All existing subscribers **stay on Plan v1** at $137.50
3. ✅ System schedules **individual migrations**:
   - Alice (subscribed Jan 5) → Migrates Feb 5
   - Bob (subscribed Jan 10) → Migrates Feb 10
   - Each user migrates at **THEIR renewal date** (10 days minimum notice)
4. ✅ All users receive **email notification** with options
5. ✅ New subscribers get **Plan v2** at $175

**Response:**
```json
{
  "data": {
    "id": "<plan-v2-id>",
    "name": "Mental Health Basic",
    "versionNumber": 2,
    "isLatestVersion": true,
    "price": 175.00,
    "isAutoCalculatedPrice": false
  },
  "message": "Plan version 2 created. 150 users will migrate at their next renewal.",
  "statusCode": 201
}
```

---

## 👤 User Experience - Migration Notification

Users receive an email like this:

```
Subject: Important Update to Your Subscription Plan

Dear Alice,

We are updating the pricing for your subscription plan 'Mental Health Basic'.

Current Plan: Mental Health Basic v1 - $137.50/month
New Plan: Mental Health Basic v2 - $175.00/month

Migration Date: February 5, 2025 (Your next renewal date)
Notice Period: 10 days

What This Means:
- You will continue to enjoy your current plan at $137.50/month until Feb 5, 2025
- On Feb 5, 2025, you will automatically migrate to the new plan at $175.00/month
- Any additional privileges you purchase before migration will be billed at current market rates

Your Options:
1. Accept: Continue with the automatic migration (no action needed)
2. Downgrade: Switch to a different plan that better fits your needs
3. Cancel: Cancel your subscription before the migration date

Note: If you purchase additional privileges during this period, they will be charged at our current pricing to ensure fairness.

To review your options, visit: /my-subscription/migration
```

---

## 📱 User API - View Migration

```http
GET /api/UserSubscription/my-subscription/migration
Authorization: Bearer <user-token>
```

**Response:**
```json
{
  "data": {
    "hasScheduledMigration": true,
    "migration": {
      "id": "<migration-id>",
      "subscriptionId": "<subscription-id>",
      "fromPlan": {
        "id": "<plan-v1-id>",
        "name": "Mental Health Basic",
        "price": 137.50,
        "versionNumber": 1
      },
      "toPlan": {
        "id": "<plan-v2-id>",
        "name": "Mental Health Basic",
        "price": 175.00,
        "versionNumber": 2
      },
      "notificationDate": "2025-01-20T00:00:00Z",
      "scheduledMigrationDate": "2025-02-05T00:00:00Z",
      "status": "Pending",
      "userDecision": null,
      "daysUntilMigration": 16
    }
  },
  "message": "Scheduled migration retrieved successfully",
  "statusCode": 200
}
```

---

## ✋ User Response to Migration

### Option 1: Accept (Do Nothing)
The migration will proceed automatically on Feb 5.

### Option 2: Downgrade to a Cheaper Plan

```http
POST /api/UserSubscription/my-subscription/migration/respond
Authorization: Bearer <user-token>
Content-Type: application/json

{
  "subscriptionId": "<subscription-id>",
  "decision": "Downgrade",
  "downgradeToPlanId": "<basic-plan-id>",
  "reason": "New price is outside my budget"
}
```

### Option 3: Cancel Subscription

```http
POST /api/UserSubscription/my-subscription/migration/respond
Authorization: Bearer <user-token>
Content-Type: application/json

{
  "subscriptionId": "<subscription-id>",
  "decision": "Cancel",
  "reason": "Price increase is too high"
}
```

---

## 🛡️ Healthcare Abuse Prevention

### The Problem We Solved:

**OLD SYSTEM (BAD):**
```
User on Plan v1 ($10/month, overage $5 each)
Admin creates Plan v2 ($20/month, overage $15 each)

Abuse scenario:
- User stays on v1 forever
- Buys overage at $5 (old price)
- Your cost: $50 (market rate)
- Your loss: $45 per overage!
```

**NEW SYSTEM (GOOD):**
```
User on Plan v1 ($10/month) until renewal
Admin creates Plan v2 ($20/month)

Healthcare-compliant:
- User stays on v1 until Feb 5 (their renewal)
- User buys overage on Jan 20
- ✅ Overage charged at v2 pricing ($15, not $5)
- Fair market rate applied
- No abuse opportunity
```

### Implementation:

When user purchases additional privileges:

```http
POST /api/Subscriptions/<subscription-id>/purchase-privilege
Content-Type: application/json

{
  "privilegeId": "<therapy-session-privilege-id>",
  "quantity": 2  // Want 2 extra sessions
}
```

**Backend Logic:**
```csharp
// ✅ Uses CreateHealthcareOverageBillingAsync
var billingResult = await _billingService.CreateHealthcareOverageBillingAsync(
    subscriptionId,
    privilegeId,
    quantity,
    tokenModel);

// Inside CreateHealthcareOverageBillingAsync:
// 1. Get subscription (currently on Plan v1)
// 2. Check: Is v1 the latest version? NO
// 3. Get latest version (Plan v2)
// 4. Get overage cost from v2: $50 per session
// 5. Calculate: 2 × $50 = $100
// 6. Create billing record for $100 (not old $25 × 2 = $50)
// ✅ Abuse prevented!
```

---

## 📅 Background Service - Automated Migration

The `ScheduledMigrationBackgroundService` runs daily at 2 AM:

```
Daily at 2 AM:
┌─────────────────────────────────────────────────┐
│ 1. Query: GetMigrationsDueByDateAsync(today)   │
│    → Finds migrations with ScheduledDate = today│
│                                                  │
│ 2. For each migration:                          │
│    ✅ Update subscription.PlanId (v1 → v2)      │
│    ✅ Update subscription.Price ($137 → $175)   │
│    ✅ Update Stripe subscription                │
│    ✅ Mark migration "Completed"                │
│                                                  │
│ 3. Log: "2 migrations completed, 0 failed"      │
└─────────────────────────────────────────────────┘
```

**Monitoring:**
- Check logs for failed migrations
- Alert if migration fails 3 times
- Retry failed migrations next day

---

## 📋 Admin Dashboard Views

### View Plan Version History

```http
GET /api/SubscriptionPlans/<plan-id>/versions
```

**Response:**
```json
{
  "data": {
    "parentPlanId": "<plan-id>",
    "planName": "Mental Health Basic",
    "totalVersions": 3,
    "totalActiveSubscriptions": 450,
    "versions": [
      {
        "id": "<v1-id>",
        "versionNumber": 1,
        "isLatestVersion": false,
        "price": 137.50,
        "calculatedPrice": 137.50,
        "versionCreatedDate": "2024-01-01",
        "activeSubscriptionsCount": 200  // Still on v1
      },
      {
        "id": "<v2-id>",
        "versionNumber": 2,
        "isLatestVersion": false,
        "price": 175.00,
        "calculatedPrice": 175.00,
        "versionCreatedDate": "2025-01-15",
        "activeSubscriptionsCount": 180  // Migrated to v2
      },
      {
        "id": "<v3-id>",
        "versionNumber": 3,
        "isLatestVersion": true,
        "price": 200.00,
        "calculatedPrice": 200.00,
        "versionCreatedDate": "2025-02-01",
        "activeSubscriptionsCount": 70   // New subscribers
      }
    ]
  }
}
```

### View Scheduled Migrations for a Plan

```http
GET /api/SubscriptionPlans/<plan-id>/scheduled-migrations
```

**Response:**
```json
{
  "data": [
    {
      "id": "<migration-id>",
      "subscriptionId": "<sub-id>",
      "fromPlan": { "id": "<v2-id>", "name": "Mental Health Basic", "versionNumber": 2 },
      "toPlan": { "id": "<v3-id>", "name": "Mental Health Basic", "versionNumber": 3 },
      "scheduledMigrationDate": "2025-03-01",
      "status": "Pending",
      "userDecision": null
    },
    {
      "id": "<migration-id-2>",
      "subscriptionId": "<sub-id-2>",
      "fromPlan": { "id": "<v2-id>", "name": "Mental Health Basic", "versionNumber": 2 },
      "toPlan": { "id": "<v3-id>", "name": "Mental Health Basic", "versionNumber": 3 },
      "scheduledMigrationDate": "2025-03-05",
      "status": "Pending",
      "userDecision": "Downgrade",
      "downgradeToPlanId": "<basic-plan-id>"
    }
  ]
}
```

---

## 🎯 End-to-End Healthcare Workflow

### Timeline Example:

```
📅 JAN 1, 2025
├─ Admin creates Mental Health Basic v1
│  ├─ Auto-calculated: $137.50/month
│  └─ (4 therapy @ $25) + (50 msgs @ $0.20) + 25% commission
│
│
📅 JAN 5, 2025
├─ Alice subscribes to v1 at $137.50/month
│  └─ Next billing: Feb 5
│
│
📅 JAN 10, 2025
├─ Bob subscribes to v1 at $137.50/month
│  └─ Next billing: Feb 10
│
│
📅 JAN 20, 2025
├─ Admin creates Plan v2 (price increase to $175)
│  ├─ System creates ScheduledPlanMigrations:
│  │  ├─ Alice: Migrate on Feb 5 (10 days notice ✅)
│  │  └─ Bob: Migrate on Feb 10 (21 days notice ✅)
│  ├─ Notifications sent to all users
│  └─ Alice and Bob stay on v1 until their renewal
│
│
📅 JAN 25, 2025
├─ Alice needs 2 extra therapy sessions (overage)
│  ├─ Current plan: v1 ($137.50/month)
│  ├─ Latest plan: v2 ($175.00/month)
│  ├─ ✅ HEALTHCARE RULE: Use v2 overage pricing
│  ├─ Overage cost from v2: 2 × $50 = $100
│  └─ Alice charged $100 (not v1's $25 × 2 = $50)
│
│
📅 FEB 5, 2025 @ 2 AM
├─ Background Service Runs
│  ├─ Finds Alice's migration due today
│  ├─ Updates Alice's subscription: v1 → v2
│  ├─ Updates Stripe subscription
│  ├─ Marks migration "Completed"
│  └─ Alice now billed $175/month
│
│
📅 FEB 10, 2025 @ 2 AM
├─ Background Service Runs
│  ├─ Finds Bob's migration due today
│  ├─ Bob chose "Downgrade" to Basic plan
│  ├─ Updates Bob's subscription: v1 → Basic
│  ├─ Marks migration "Completed"
│  └─ Bob now billed at Basic plan price
│
│
📅 FEB 15, 2025
├─ Charlie subscribes (new user)
│  └─ Gets latest version: v2 at $175/month
```

---

## 🔧 Configuration

### Global Settings (SystemSettings table)

```sql
SELECT * FROM SystemSettings;

-- Result:
Id: 00000000-0000-0000-0000-000000000001
DefaultAdminCommissionPercent: 20
DefaultPriceChangeNoticeDays: 10
MaxFailedPaymentAttempts: 3
LastUpdated: 2025-01-01
```

To change global defaults:
```csharp
var settings = await _systemSettingsRepository.GetSettingsAsync();
settings.DefaultAdminCommissionPercent = 25;  // Change to 25%
settings.DefaultPriceChangeNoticeDays = 30;    // Change to 30 days
await _systemSettingsRepository.UpdateSettingsAsync(settings);
```

---

## 🧪 Testing Checklist

### Manual Testing:

- [ ] Create plan with auto-calculated pricing
- [ ] Verify price = Σ(privileges) + commission
- [ ] Create plan with manual pricing
- [ ] Verify manual price is used
- [ ] Subscribe user to plan
- [ ] Create new plan version
- [ ] Verify user stays on old version
- [ ] Verify scheduled migration created
- [ ] User purchases overage
- [ ] Verify overage uses latest plan pricing
- [ ] Wait for migration date (or manually trigger)
- [ ] Verify user migrated to new plan
- [ ] Verify user can downgrade/cancel

---

## 📚 API Endpoints Summary

### Admin Endpoints:
- `POST /api/SubscriptionPlans` - Create plan (with healthcare pricing)
- `POST /api/SubscriptionPlans/{id}/versions` - Create new version
- `GET /api/SubscriptionPlans/{id}/versions` - Get version history
- `POST /api/SubscriptionPlans/{id}/calculate-price` - Calculate auto price
- `GET /api/SubscriptionPlans/{id}/scheduled-migrations` - View migrations

### Public Endpoints:
- `GET /api/SubscriptionPlans/active` - Browse plans (shows latest versions only)
- `GET /api/SubscriptionPlans/{id}/pricing-breakdown` - Transparency

### User Endpoints:
- `GET /api/UserSubscription/my-subscription/migration` - View my migration
- `POST /api/UserSubscription/my-subscription/migration/respond` - Respond to migration

---

## 🎓 Best Practices

### For Admins:

1. **Always use versioning for price changes**
   - Don't modify existing plans directly
   - Create new version instead

2. **Choose appropriate notice period**
   - Default: 10 days
   - Healthcare regulations may require more
   - Configurable per plan (7-365 days)

3. **Monitor migrations**
   - Check daily for failed migrations
   - Alert users if issues arise
   - Track user responses (Accept/Downgrade/Cancel)

4. **Price transparency**
   - Use auto-calculated pricing when possible
   - Show pricing breakdown to users
   - Document commission structure

### For Developers:

1. **Use healthcare-compliant overage billing**
   ```csharp
   // ✅ GOOD: Uses latest plan pricing
   await _billingService.CreateHealthcareOverageBillingAsync(
       subscriptionId, privilegeId, quantity, tokenModel);
   
   // ❌ BAD: Uses subscription's current plan (exploitable)
   await _billingService.CreateOverageBillingAsync(
       subscription, privilegeName, amount, tokenModel);
   ```

2. **Always check plan version**
   ```csharp
   // Get latest version for new subscribers
   var latestPlan = await _planRepository.GetLatestVersionOfPlanAsync(planId);
   ```

3. **Schedule migrations properly**
   ```csharp
   // Individual renewal dates, not fixed grace period
   await _versioningService.ScheduleMigrationsForPlanVersionAsync(
       oldPlanId, newPlanId, tokenModel);
   ```

---

## 🚨 Troubleshooting

### Migration Not Executing?
- Check `ScheduledPlanMigrations` table for status
- Verify `ScheduledMigrationBackgroundService` is running
- Check logs for errors at 2 AM

### Price Not Auto-Calculating?
- Verify `IsAutoCalculatedPrice = true`
- Check all privileges have `PrivilegeBaseCost` set
- Verify commission settings (global or per-plan)

### User Can't See Migration?
- Check subscription is active
- Verify migration exists for their subscription
- Check `ScheduledMigrationDate` is in future

---

## 📊 Database Schema Reference

### New Tables:

```sql
-- System-wide settings
CREATE TABLE SystemSettings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DefaultAdminCommissionPercent DECIMAL(5,2) DEFAULT 20,
    DefaultPriceChangeNoticeDays INT DEFAULT 10,
    MaxFailedPaymentAttempts INT DEFAULT 3,
    LastUpdated DATETIME2,
    -- BaseEntity fields
);

-- Scheduled migrations
CREATE TABLE ScheduledPlanMigrations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    FromPlanId UNIQUEIDENTIFIER NOT NULL,
    ToPlanId UNIQUEIDENTIFIER NOT NULL,
    NotificationDate DATETIME2 NOT NULL,
    ScheduledMigrationDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Pending',
    UserDecision NVARCHAR(50),
    UserDecisionDate DATETIME2,
    DowngradeToPlanId UNIQUEIDENTIFIER,
    CompletedDate DATETIME2,
    Notes NVARCHAR(500),
    -- BaseEntity fields
);
```

### New Columns in Existing Tables:

```sql
-- SubscriptionPlans table
ALTER TABLE SubscriptionPlans ADD
    VersionNumber INT DEFAULT 1,
    IsLatestVersion BIT DEFAULT 1,
    ParentPlanId UNIQUEIDENTIFIER NULL,
    VersionCreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    IsAutoCalculatedPrice BIT DEFAULT 1,
    PrivilegesTotalCost DECIMAL(18,2) DEFAULT 0,
    AdminCommissionPercent DECIMAL(5,2) NULL,
    AdminCommissionFixed DECIMAL(18,2) NULL,
    PriceChangeNoticeDays INT DEFAULT 10;

-- SubscriptionPlanPrivileges table
ALTER TABLE SubscriptionPlanPrivileges ADD
    PrivilegeBaseCost DECIMAL(18,2) DEFAULT 0;
    -- UnitCost already exists
```

---

## 🎉 Success Indicators

Your implementation is successful when:

✅ All existing plans show `VersionNumber = 1`  
✅ Creating new version preserves old subscriptions  
✅ Users migrate at individual renewal dates  
✅ Overage charges use latest plan pricing  
✅ Users can view and respond to migrations  
✅ Background service processes migrations daily  
✅ Price breakdown shows transparent calculations  
✅ No service abuse opportunities exist  

---

## 📞 Support

For issues or questions:
1. Check logs in `/logs` directory
2. Query `ScheduledPlanMigrations` table for migration status
3. Verify `SystemSettings` for configuration
4. Test in staging environment first

---

**Implementation Complete! 🎊**

Your healthcare subscription plan management system is now production-ready with robust versioning, transparent pricing, and abuse prevention.


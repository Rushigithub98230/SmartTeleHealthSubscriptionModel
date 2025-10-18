# 🧪 Healthcare Subscription Plan API - Testing Guide

## Complete API Testing Workflow

This guide provides step-by-step API tests for the healthcare subscription plan management system.

---

## Prerequisites

1. ✅ Database migrations applied
2. ✅ `VersionExistingPlans.sql` executed
3. ✅ Application running on `https://localhost:5001`
4. ✅ Admin token obtained
5. ✅ User token obtained

---

## Test Suite 1: Plan Creation with Auto-Pricing

### Test 1.1: Create Plan with Auto-Calculated Pricing

```bash
curl -X POST "https://localhost:5001/api/SubscriptionPlans" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -d '{
  "name": "Test Mental Health Basic",
  "description": "Test plan for auto-pricing",
  "billingCycleId": "MONTHLY_BILLING_CYCLE_ID",
  "currencyId": "USD_CURRENCY_ID",
  "categoryId": "MENTAL_HEALTH_CATEGORY_ID",
  "isAutoCalculatedPrice": true,
  "adminCommissionPercent": 20,
  "priceChangeNoticeDays": 10,
  "isTrialAllowed": true,
  "trialDurationInDays": 14,
  "isActive": true,
  "displayOrder": 1,
  "privileges": [
    {
      "privilegeId": "THERAPY_PRIVILEGE_ID",
      "value": 4,
      "usagePeriodId": "MONTHLY_PERIOD_ID",
      "privilegeBaseCost": 25.00,
      "unitCost": 50.00,
      "monthlyLimit": 4
    },
    {
      "privilegeId": "MESSAGING_PRIVILEGE_ID",
      "value": 50,
      "usagePeriodId": "MONTHLY_PERIOD_ID",
      "privilegeBaseCost": 0.20,
      "unitCost": 0.50,
      "monthlyLimit": 50
    }
  ]
}'
```

**Expected Response:**
```json
{
  "data": {
    "id": "NEW_PLAN_ID",
    "price": 120.00,  // (4×$25)+(50×$0.20)+20% = $120
    "versionNumber": 1,
    "isLatestVersion": true,
    "isAutoCalculatedPrice": true
  },
  "statusCode": 201
}
```

**Validation:**
- ✅ Price auto-calculated to $120
- ✅ Version number = 1
- ✅ Latest version = true

---

### Test 1.2: Get Pricing Breakdown

```bash
curl "https://localhost:5001/api/SubscriptionPlans/NEW_PLAN_ID/pricing-breakdown"
```

**Expected Response:**
```json
{
  "data": {
    "planName": "Test Mental Health Basic",
    "isAutoCalculated": true,
    "privilegeBreakdown": [
      {
        "privilegeName": "Therapy Sessions",
        "quantity": 4,
        "unitBaseCost": 25.00,
        "totalCost": 100.00
      },
      {
        "privilegeName": "Messaging",
        "quantity": 50,
        "unitBaseCost": 0.20,
        "totalCost": 10.00
      }
    ],
    "privilegesTotalCost": 110.00,
    "commissionPercent": 20,
    "commissionAmount": 22.00,
    "finalPrice": 132.00
  }
}
```

---

## Test Suite 2: Plan Versioning

### Test 2.1: Create New Plan Version

```bash
curl -X POST "https://localhost:5001/api/SubscriptionPlans/PLAN_ID/versions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -d '{
  "name": "Test Mental Health Basic",
  "description": "Updated pricing v2",
  "price": 150.00,
  "billingCycleId": "MONTHLY_BILLING_CYCLE_ID",
  "currencyId": "USD_CURRENCY_ID",
  "categoryId": "MENTAL_HEALTH_CATEGORY_ID",
  "isActive": true,
  "isAutoCalculatedPrice": false,
  "priceChangeNoticeDays": 10
}'
```

**Expected Response:**
```json
{
  "data": {
    "id": "NEW_VERSION_ID",
    "versionNumber": 2,
    "isLatestVersion": true,
    "price": 150.00
  },
  "message": "Plan version 2 created. X users will migrate at their next renewal.",
  "statusCode": 201
}
```

**Validation:**
- ✅ New version created (v2)
- ✅ Old version (v1) marked as not latest
- ✅ Migrations scheduled for active users

---

### Test 2.2: View Version History

```bash
curl "https://localhost:5001/api/SubscriptionPlans/PLAN_ID/versions"
```

**Expected Response:**
```json
{
  "data": {
    "planName": "Test Mental Health Basic",
    "totalVersions": 2,
    "versions": [
      {
        "versionNumber": 1,
        "isLatestVersion": false,
        "price": 120.00,
        "activeSubscriptionsCount": 5
      },
      {
        "versionNumber": 2,
        "isLatestVersion": true,
        "price": 150.00,
        "activeSubscriptionsCount": 0
      }
    ]
  }
}
```

---

## Test Suite 3: User Migration

### Test 3.1: User Views Scheduled Migration

```bash
curl "https://localhost:5001/api/UserSubscription/my-subscription/migration" \
  -H "Authorization: Bearer USER_TOKEN"
```

**Expected Response:**
```json
{
  "data": {
    "hasScheduledMigration": true,
    "migration": {
      "fromPlan": {
        "name": "Test Mental Health Basic",
        "price": 120.00,
        "versionNumber": 1
      },
      "toPlan": {
        "name": "Test Mental Health Basic",
        "price": 150.00,
        "versionNumber": 2
      },
      "scheduledMigrationDate": "2025-03-01",
      "daysUntilMigration": 14,
      "status": "Pending"
    }
  }
}
```

---

### Test 3.2: User Accepts Migration

```bash
curl -X POST "https://localhost:5001/api/UserSubscription/my-subscription/migration/respond" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer USER_TOKEN" \
  -d '{
  "subscriptionId": "USER_SUBSCRIPTION_ID",
  "decision": "Accept",
  "reason": "I accept the new pricing"
}'
```

**Expected Response:**
```json
{
  "data": {
    "userDecision": "Accept",
    "status": "Pending"
  },
  "message": "Migration response 'Accept' processed successfully",
  "statusCode": 200
}
```

---

### Test 3.3: User Downgrades

```bash
curl -X POST "https://localhost:5001/api/UserSubscription/my-subscription/migration/respond" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer USER_TOKEN" \
  -d '{
  "subscriptionId": "USER_SUBSCRIPTION_ID",
  "decision": "Downgrade",
  "downgradeToPlanId": "BASIC_PLAN_ID",
  "reason": "New price is too high"
}'
```

---

### Test 3.4: User Cancels

```bash
curl -X POST "https://localhost:5001/api/UserSubscription/my-subscription/migration/respond" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer USER_TOKEN" \
  -d '{
  "subscriptionId": "USER_SUBSCRIPTION_ID",
  "decision": "Cancel",
  "reason": "Price increase is unaffordable"
}'
```

---

## Test Suite 4: Healthcare Overage Pricing

### Test 4.1: Purchase Overage (Healthcare Pricing)

This test verifies that overage uses the LATEST plan pricing, not the user's current plan pricing.

**Setup:**
1. User subscribed to Plan v1 ($120/month, therapy overage $50)
2. Admin created Plan v2 ($150/month, therapy overage $75)
3. User still on v1 until renewal

**Test:**
```bash
# Note: This endpoint needs to be implemented in SubscriptionsController
curl -X POST "https://localhost:5001/api/Subscriptions/SUBSCRIPTION_ID/purchase-privilege" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer USER_TOKEN" \
  -d '{
  "privilegeId": "THERAPY_PRIVILEGE_ID",
  "quantity": 2
}'
```

**Expected Behavior:**
```
User on Plan v1 (overage $50)
Latest Plan v2 (overage $75)

Calculation:
✅ Uses v2 pricing: 2 × $75 = $150
❌ Does NOT use v1 pricing: 2 × $50 = $100

Result: User charged $150 (fair market rate)
```

---

## Test Suite 5: Admin Dashboard

### Test 5.1: View Scheduled Migrations for a Plan

```bash
curl "https://localhost:5001/api/SubscriptionPlans/PLAN_ID/scheduled-migrations" \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

**Expected Response:**
```json
{
  "data": [
    {
      "subscriptionId": "sub-1",
      "scheduledMigrationDate": "2025-03-01",
      "status": "Pending",
      "userDecision": null
    },
    {
      "subscriptionId": "sub-2",
      "scheduledMigrationDate": "2025-03-05",
      "status": "Pending",
      "userDecision": "Accept"
    },
    {
      "subscriptionId": "sub-3",
      "scheduledMigrationDate": "2025-03-10",
      "status": "Pending",
      "userDecision": "Downgrade"
    }
  ],
  "statusCode": 200
}
```

---

## Database Verification Queries

### Verify Plan Versioning

```sql
-- Check all plan versions
SELECT 
    Id,
    Name,
    VersionNumber,
    IsLatestVersion,
    ParentPlanId,
    Price,
    IsAutoCalculatedPrice,
    PriceChangeNoticeDays
FROM SubscriptionPlans
ORDER BY Name, VersionNumber;
```

**Expected:**
- All plans have `VersionNumber >= 1`
- Only one version per plan family has `IsLatestVersion = 1`
- Child versions have `ParentPlanId` set

---

### Verify Scheduled Migrations

```sql
-- Check scheduled migrations
SELECT 
    m.Id,
    m.ScheduledMigrationDate,
    m.Status,
    m.UserDecision,
    fp.Name + ' v' + CAST(fp.VersionNumber AS VARCHAR) AS FromPlan,
    tp.Name + ' v' + CAST(tp.VersionNumber AS VARCHAR) AS ToPlan,
    DATEDIFF(day, GETUTCDATE(), m.ScheduledMigrationDate) AS DaysUntil
FROM ScheduledPlanMigrations m
INNER JOIN SubscriptionPlans fp ON m.FromPlanId = fp.Id
INNER JOIN SubscriptionPlans tp ON m.ToPlanId = tp.Id
WHERE m.Status = 'Pending'
ORDER BY m.ScheduledMigrationDate;
```

---

### Verify System Settings

```sql
-- Check global configuration
SELECT * FROM SystemSettings;
```

**Expected:**
```
Id: 00000000-0000-0000-0000-000000000001
DefaultAdminCommissionPercent: 20
DefaultPriceChangeNoticeDays: 10
MaxFailedPaymentAttempts: 3
```

---

## Integration Test Scenarios

### Scenario A: Complete Plan Lifecycle

```
1. ✅ Create plan v1 with auto-pricing ($120)
2. ✅ User subscribes to v1
3. ✅ Create plan v2 ($150)
4. ✅ Verify user stays on v1
5. ✅ Verify migration scheduled
6. ✅ User buys overage
7. ✅ Verify overage charged at v2 pricing
8. ✅ Wait for migration date
9. ✅ Verify user migrated to v2
10. ✅ Verify next billing uses v2 price
```

### Scenario B: User Downgrade Flow

```
1. ✅ User on Plan v1 ($120)
2. ✅ Admin creates v2 ($150)
3. ✅ User receives notification
4. ✅ User chooses "Downgrade" to Basic ($80)
5. ✅ On renewal date, user migrates to Basic
6. ✅ Verify user billed $80 (not $150)
```

### Scenario C: User Cancellation Flow

```
1. ✅ User on Plan v1 ($120)
2. ✅ Admin creates v2 ($150)
3. ✅ User receives notification
4. ✅ User chooses "Cancel"
5. ✅ AutoRenew set to false
6. ✅ Subscription continues until current period ends
7. ✅ No renewal on next billing date
```

---

## Postman Collection

### Environment Variables

```json
{
  "baseUrl": "https://localhost:5001",
  "adminToken": "YOUR_ADMIN_TOKEN",
  "userToken": "YOUR_USER_TOKEN",
  "planId": "",
  "subscriptionId": "",
  "privilegeId": ""
}
```

### Collection Structure

```
📁 Healthcare Subscription Plans
├── 📁 1. Plan Creation
│   ├── POST Create Auto-Priced Plan
│   ├── POST Create Manual-Priced Plan
│   └── GET Pricing Breakdown
├── 📁 2. Plan Versioning
│   ├── POST Create New Version
│   ├── GET Version History
│   └── GET Scheduled Migrations
├── 📁 3. User Migration
│   ├── GET My Scheduled Migration
│   ├── POST Accept Migration
│   ├── POST Downgrade
│   └── POST Cancel
├── 📁 4. Pricing & Calculations
│   ├── POST Calculate Plan Price
│   └── GET Pricing Breakdown
└── 📁 5. Verification
    ├── GET All Plans
    ├── GET Plan Details
    └── GET Subscription Details
```

---

## Expected Results Summary

### Plan Creation ✅
- Plan created with v1
- Price auto-calculated from privileges
- Stripe resources created

### Version Creation ✅
- New version (v2) created
- Old version marked not latest
- Migrations scheduled
- Users notified

### User Migration ✅
- User sees migration details
- User can respond
- Migration executes at renewal
- Stripe updated

### Overage Billing ✅
- Uses latest plan pricing
- Prevents abuse
- Fair market rate applied

---

## Troubleshooting

### Price Not Auto-Calculating?
```
Check:
1. IsAutoCalculatedPrice = true?
2. All privileges have PrivilegeBaseCost > 0?
3. IPlanPricingService registered in DI?
```

### Migration Not Scheduled?
```
Check:
1. Does plan have active subscriptions?
2. Is PriceChangeNoticeDays set correctly?
3. Are migrations in ScheduledPlanMigrations table?
```

### Background Service Not Running?
```
Check:
1. Is ScheduledMigrationBackgroundService registered?
2. Check logs at 2 AM
3. Verify ScheduledMigrationDate <= current date
```

---

**Ready to test! 🚀**


# 📊 Database Relationships & Data Flow - Visual Guide

## Complete Database Schema with Relationships

---

## 1. ENTITY RELATIONSHIP DIAGRAM

```
┌─────────────────────────────────────────────────────────────────────┐
│                        USERS (AspNetUsers)                           │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ Id (INT) PK                                                     │ │
│  │ Email                                                           │ │
│  │ FirstName, LastName                                             │ │
│  │ StripeCustomerId ──────────────────┐                           │ │
│  └────────────────────────────────────│───────────────────────────┘ │
└────────────────────────────────────────│─────────────────────────────┘
                                         │
                    ┌────────────────────┴────────────────────┐
                    │                                         │
┌───────────────────▼─────────────────┐    ┌─────────────────▼───────────┐
│        SUBSCRIPTIONS                │    │     STRIPE                  │
│  ┌──────────────────────────────┐  │    │  ┌──────────────────────┐  │
│  │ Id (GUID) PK                 │  │    │  │ Customer             │  │
│  │ UserId (INT) FK ─────────────┼──┘    │  │  Id: cus_XYZ789      │  │
│  │ SubscriptionPlanId (GUID) FK─┼───┐   │  │  Email               │  │
│  │ Status                       │   │   │  │  Metadata: {userId}  │  │
│  │ StartDate, EndDate           │   │   │  └──────────────────────┘  │
│  │ NextBillingDate              │   │   │            ↓                │
│  │ CurrentPrice                 │   │   │  ┌──────────────────────┐  │
│  │ StripeCustomerId ────────────┼───┼───┼─→│ Subscription         │  │
│  │ StripeSubscriptionId ────────┼───┼───┼─→│  Id: sub_stripe_AAA  │  │
│  │ AutoRenew                    │   │   │  │  Status: active      │  │
│  │ FailedPaymentAttempts        │   │   │  │  Metadata: {subId}   │  │
│  └──────────────────────────────┘   │   │  └──────────────────────┘  │
└─────────────────┬────────────────────┘   │   └─────────────────────────┘
                  │                        │
    ┌─────────────┼────────────┬──────────┼──────────┐
    │             │            │          │          │
    ↓             ↓            ↓          ↓          ↓
┌────────┐  ┌──────────┐ ┌─────────┐ ┌─────────┐ ┌──────────────┐
│Billing │  │Payments  │ │Privilege│ │Status   │ │ SUBSCRIPTION │
│Records │  │          │ │Usage    │ │History  │ │ PLANS        │
└────────┘  └──────────┘ └─────────┘ └─────────┘ └──────────────┘
                                                         │
                                                         ↓
┌────────────────────────────────────────────────────────────────────┐
│                    SUBSCRIPTIONPLANS                                │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ Id (GUID) PK                                                  │  │
│  │ Name, Description                                             │  │
│  │ Price                                                         │  │
│  │ BillingCycleId (GUID) FK                                      │  │
│  │ IsAutoCalculatedPrice                                         │  │
│  │ AdminCommissionPercent                                        │  │
│  │ PrivilegesTotalCost                                           │  │
│  │ StripeProductId ──────────────────┐                          │  │
│  │ StripeMonthlyPriceId              │                          │  │
│  │ VersionNumber, IsLatestVersion    │                          │  │
│  │ ParentPlanId (GUID) FK (self-ref) │                          │  │
│  └───────────────────────────────────│───────────────────────── │  │
└────────────────────────────────────────│──────────────────────────┘
                    │                    │
                    │                    │    ┌──────────────────────┐
                    │                    └────┤ STRIPE               │
                    ↓                         │  Product             │
┌───────────────────────────────────────┐    │   Id: prod_ABC123    │
│ SUBSCRIPTIONPLANPRIVILEGES            │    │   Name, Description  │
│  ┌────────────────────────────────┐   │    │  Prices:             │
│  │ Id (GUID) PK                   │   │    │   - price_1Month_XYZ │
│  │ SubscriptionPlanId (GUID) FK ──┼───┘    │   - price_3Month_XYZ │
│  │ PrivilegeId (GUID) FK ─────────┼───┐    │   - price_12Month_XYZ│
│  │ Value (quantity in plan)       │   │    └──────────────────────┘
│  │ PrivilegeBaseCost ($20)        │   │
│  │ UnitCost ($25)                 │   │
│  │ DailyLimit, WeeklyLimit        │   │
│  │ MonthlyLimit                   │   │
│  └────────────────────────────────┘   │
└───────────────────────────────────────┘
                    │
                    ↓
┌───────────────────────────────────────┐
│ PRIVILEGES (Master List)              │
│  ┌────────────────────────────────┐   │
│  │ Id (GUID) PK                   │   │
│  │ Name (Teleconsultation)        │   │
│  │ Description                    │   │
│  │ Category                       │   │
│  │ IsActive                       │   │
│  └────────────────────────────────┘   │
└───────────────────────────────────────┘
```

---

## 2. DATA FLOW: SUBSCRIPTION CREATION

```
┌──────────────┐
│ INPUT: User  │
│ subscribes   │
└──────┬───────┘
       │
       ↓
┌──────────────────────────────────────────────────────┐
│ STEP 1: CREATE IN YOUR DATABASE                      │
├──────────────────────────────────────────────────────┤
│                                                       │
│ Subscriptions Table:                                 │
│ INSERT → Id: sub_111, UserId: 456, Status: Pending   │
│                                                       │
│ UserSubscriptionPrivilegeUsage Table:                │
│ INSERT → 2 records (Teleconsult: 5, Medication: 3)   │
│                                                       │
│ SubscriptionStatusHistory Table:                     │
│ INSERT → FromStatus: NULL, ToStatus: Pending         │
│                                                       │
│ BillingRecords Table:                                │
│ INSERT → Type: Subscription, Amount: $275, Pending   │
└───────────────────────┬──────────────────────────────┘
                        │
                        ↓
┌──────────────────────────────────────────────────────┐
│ STEP 2: CREATE IN STRIPE                             │
├──────────────────────────────────────────────────────┤
│                                                       │
│ Stripe Customers:                                    │
│ If not exists: CREATE → cus_XYZ789                   │
│                                                       │
│ Stripe Subscriptions:                                │
│ CREATE → sub_stripe_AAA (links to cus_XYZ789)        │
│                                                       │
│ Stripe Invoices:                                     │
│ AUTO-CREATED → in_stripe_BBB (Amount: $275)          │
└───────────────────────┬──────────────────────────────┘
                        │
                        ↓
┌──────────────────────────────────────────────────────┐
│ STEP 3: LINK TOGETHER                                │
├──────────────────────────────────────────────────────┤
│                                                       │
│ UPDATE Subscriptions:                                │
│ SET StripeSubscriptionId = 'sub_stripe_AAA'          │
│ WHERE Id = 'sub_111'                                 │
│                                                       │
│ Stripe Subscription Metadata:                        │
│ SET metadata.subscriptionId = 'sub_111'              │
└───────────────────────┬──────────────────────────────┘
                        │
                        ↓
┌──────────────────────────────────────────────────────┐
│ STEP 4: PAYMENT WEBHOOK                              │
├──────────────────────────────────────────────────────┤
│                                                       │
│ Stripe sends: invoice.payment_succeeded              │
│                                                       │
│ YOUR SYSTEM updates:                                 │
│ ├─ Subscriptions: Status → Active                    │
│ ├─ BillingRecords: Status → Paid                     │
│ ├─ SubscriptionPayments: INSERT new record           │
│ └─ SubscriptionStatusHistory: Pending → Active       │
└───────────────────────┬──────────────────────────────┘
                        │
                        ↓
                 ✅ COMPLETE
```

---

## 3. DATA FLOW: PRIVILEGE USAGE

```
User Books Consultation
       │
       ↓
┌──────────────────────────────────────┐
│ QUERY: UserSubscriptionPrivilegeUsage│
│ WHERE UserId = 456                   │
│   AND PrivilegeId = teleconsult-guid │
├──────────────────────────────────────┤
│ Result:                              │
│   AllocatedLimit: 5                  │
│   UsedValue: 2                       │
│   AllowedValue: 3 ← Check this!      │
└──────────────┬───────────────────────┘
               │
               ↓ IF AllowedValue >= 1
┌──────────────────────────────────────┐
│ UPDATE: UserSubscriptionPrivilegeUsage│
│ SET UsedValue = UsedValue + 1        │
│ SET AllowedValue = AllocatedLimit    │
│     - UsedValue                      │
│ SET LastUsedAt = GETUTCDATE()        │
│ WHERE Id = usage-guid                │
├──────────────────────────────────────┤
│ Result:                              │
│   UsedValue: 2 → 3                   │
│   AllowedValue: 3 → 2                │
└──────────────┬───────────────────────┘
               │
               ↓
┌──────────────────────────────────────┐
│ INSERT: PrivilegeUsageHistory        │
│ VALUES (                             │
│   UserId: 456,                       │
│   PrivilegeId: teleconsult-guid,     │
│   UsageDate: GETUTCDATE(),           │
│   QuantityUsed: 1,                   │
│   RemainingAfterUse: 2,              │
│   UsageType: 'Included',             │
│   Cost: 0.00                         │
│ )                                    │
└──────────────┬───────────────────────┘
               │
               ↓
        ✅ Service Allowed
```

---

## 4. DATA FLOW: OVERAGE WITH UPFRONT PAYMENT

```
User Has 0 Credits
       │
       ↓
┌──────────────────────────────────────┐
│ QUERY: AllowedValue                  │
│ Result: 0 ❌                          │
└──────────────┬───────────────────────┘
               │
               ↓
    Return 402 Payment Required
       │
       ↓ User Pays $25
┌──────────────────────────────────────────┐
│ BEGIN TRANSACTION                         │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ INSERT: BillingRecords                    │
│   Type: 'Overage'                         │
│   Amount: 25.00                           │
│   Status: 'Pending'                       │
└──────────────┬───────────────────────────┘
               │
               ↓ Stripe Charges
┌──────────────────────────────────────────┐
│ UPDATE: BillingRecords                    │
│   Status: 'Pending' → 'Paid' ✅           │
│   StripePaymentIntentId: 'pi_...'         │
└──────────────┬───────────────────────────┘
               │
               ↓ ONLY AFTER PAYMENT SUCCEEDS
┌──────────────────────────────────────────┐
│ UPDATE: UserSubscriptionPrivilegeUsage   │
│   AllocatedLimit: 5 → 6                  │
│   AllowedValue: 0 → 1                    │
└──────────────┬───────────────────────────┘
               │
               ↓ Immediately use credit
┌──────────────────────────────────────────┐
│ UPDATE: UserSubscriptionPrivilegeUsage   │
│   UsedValue: 5 → 6                       │
│   AllowedValue: 1 → 0                    │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ INSERT: PrivilegeUsageHistory             │
│   UsageType: 'Overage' ⚠️                │
│   Cost: 25.00 ✅                          │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ INSERT: SubscriptionPayments              │
│   Amount: 25.00                           │
│   Status: 'Success'                       │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ COMMIT TRANSACTION ✅                     │
└──────────────────────────────────────────┘
```

---

## 5. DATA FLOW: MONTHLY RENEWAL

```
Stripe Billing Date Arrives
       │
       ↓
┌──────────────────────────────────────────┐
│ STRIPE (Automatic)                        │
│ ├─ Create Invoice: in_stripe_DDD         │
│ ├─ Charge Customer                       │
│ └─ Send Webhook                          │
└──────────────┬───────────────────────────┘
               │
               ↓ Webhook: invoice.payment_succeeded
┌──────────────────────────────────────────┐
│ BEGIN TRANSACTION                         │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ INSERT: BillingRecords                    │
│   Type: 'Subscription'                    │
│   Amount: 275.00                          │
│   Status: 'Paid'                          │
│   BillingPeriodStart: 2025-11-17          │
│   BillingPeriodEnd: 2025-12-17            │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ INSERT: SubscriptionPayments              │
│   Amount: 275.00                          │
│   Status: 'Success'                       │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ UPDATE: Subscriptions                     │
│   EndDate: +1 month                       │
│   NextBillingDate: +1 month               │
│   LastPaymentDate: NOW                    │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ UPDATE: UserSubscriptionPrivilegeUsage   │
│ FOR EACH privilege:                       │
│   SET AllocatedLimit = [Plan Value]      │
│   SET UsedValue = 0                      │
│   SET AllowedValue = [Plan Value]        │
│   SET ResetAt = NOW                      │
│                                          │
│ Example:                                 │
│   Teleconsult: 6→5, 6→0, 0→5 (RESET)     │
│   Medication:  3→3, 2→0, 1→3 (RESET)     │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ INSERT: SubscriptionStatusHistory         │
│   FromStatus: 'Active'                    │
│   ToStatus: 'Active'                      │
│   Reason: 'Subscription renewed'          │
└──────────────┬───────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────┐
│ COMMIT TRANSACTION ✅                     │
└──────────────────────────────────────────┘
```

---

## 6. TABLE RELATIONSHIPS

### Core Relationships

```
Users (1) ──→ (Many) Subscriptions
  "One user can have multiple subscriptions"

SubscriptionPlans (1) ──→ (Many) Subscriptions
  "One plan can be purchased by many users"

Subscriptions (1) ──→ (Many) BillingRecords
  "One subscription generates multiple billing records over time"

Subscriptions (1) ──→ (Many) SubscriptionPayments
  "One subscription has multiple payments (monthly renewals)"

Subscriptions (1) ──→ (Many) UserSubscriptionPrivilegeUsage
  "One subscription has usage tracking for multiple privileges"

Subscriptions (1) ──→ (Many) SubscriptionStatusHistory
  "One subscription has multiple status changes tracked"

BillingRecords (1) ──→ (Many) SubscriptionPayments
  "One billing record can have multiple payment attempts"

BillingRecords (1) ──→ (Many) BillingAdjustments
  "One billing record can have multiple adjustments (refunds, credits)"

SubscriptionPlans (1) ──→ (Many) SubscriptionPlanPrivileges
  "One plan has multiple privileges configured"

Privileges (1) ──→ (Many) SubscriptionPlanPrivileges
  "One privilege can be in multiple plans"

Privileges (1) ──→ (Many) UserSubscriptionPrivilegeUsage
  "One privilege can be tracked for many users"

SubscriptionPlans (1) ──→ (Many) SubscriptionPlans (Self-Reference)
  "Plan versioning: ParentPlanId links to previous version"
```

---

## 7. FOREIGN KEY CONSTRAINTS

### Critical Constraints

```sql
-- Subscription → User
ALTER TABLE Subscriptions
ADD CONSTRAINT FK_Subscription_User
FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
ON DELETE CASCADE;

-- Subscription → Plan
ALTER TABLE Subscriptions
ADD CONSTRAINT FK_Subscription_Plan
FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id)
ON DELETE RESTRICT;  -- Can't delete plan with active subscriptions

-- SubscriptionPlanPrivilege → Plan
ALTER TABLE SubscriptionPlanPrivileges
ADD CONSTRAINT FK_PlanPrivilege_Plan
FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id)
ON DELETE CASCADE;  -- Delete privileges when plan deleted

-- SubscriptionPlanPrivilege → Privilege
ALTER TABLE SubscriptionPlanPrivileges
ADD CONSTRAINT FK_PlanPrivilege_Privilege
FOREIGN KEY (PrivilegeId) REFERENCES Privileges(Id)
ON DELETE CASCADE;

-- UserSubscriptionPrivilegeUsage → Subscription
ALTER TABLE UserSubscriptionPrivilegeUsage
ADD CONSTRAINT FK_Usage_Subscription
FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id)
ON DELETE CASCADE;  -- Delete usage when subscription deleted

-- BillingRecord → Subscription
ALTER TABLE BillingRecords
ADD CONSTRAINT FK_Billing_Subscription
FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id)
ON DELETE SET NULL;  -- Keep billing records even if subscription deleted

-- SubscriptionPayment → BillingRecord
ALTER TABLE SubscriptionPayments
ADD CONSTRAINT FK_Payment_Billing
FOREIGN KEY (BillingRecordId) REFERENCES BillingRecords(Id)
ON DELETE CASCADE;

-- SubscriptionStatusHistory → Subscription
ALTER TABLE SubscriptionStatusHistory
ADD CONSTRAINT FK_StatusHistory_Subscription
FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id)
ON DELETE CASCADE;
```

---

## 8. INDEXES FOR PERFORMANCE

### Critical Indexes

```sql
-- Subscriptions
CREATE INDEX IX_Subscriptions_UserId ON Subscriptions(UserId);
CREATE INDEX IX_Subscriptions_PlanId ON Subscriptions(SubscriptionPlanId);
CREATE INDEX IX_Subscriptions_Status ON Subscriptions(Status);
CREATE INDEX IX_Subscriptions_NextBillingDate ON Subscriptions(NextBillingDate);
CREATE INDEX IX_Subscriptions_StripeSubscriptionId ON Subscriptions(StripeSubscriptionId);

-- UserSubscriptionPrivilegeUsage
CREATE INDEX IX_Usage_SubscriptionId ON UserSubscriptionPrivilegeUsage(SubscriptionId);
CREATE INDEX IX_Usage_UserId_PrivilegeId ON UserSubscriptionPrivilegeUsage(UserId, PrivilegeId);

-- BillingRecords
CREATE INDEX IX_Billing_UserId ON BillingRecords(UserId);
CREATE INDEX IX_Billing_SubscriptionId ON BillingRecords(SubscriptionId);
CREATE INDEX IX_Billing_Status ON BillingRecords(Status);
CREATE INDEX IX_Billing_InvoiceNumber ON BillingRecords(InvoiceNumber);
CREATE UNIQUE INDEX IX_Billing_StripeInvoiceId ON BillingRecords(StripeInvoiceId);

-- PrivilegeUsageHistory
CREATE INDEX IX_History_UserId ON PrivilegeUsageHistory(UserId);
CREATE INDEX IX_History_SubscriptionId ON PrivilegeUsageHistory(SubscriptionId);
CREATE INDEX IX_History_UsageDate ON PrivilegeUsageHistory(UsageDate);
CREATE INDEX IX_History_UsageType ON PrivilegeUsageHistory(UsageType);
```

---

## 9. DATA LIFECYCLE EXAMPLE

### Month-by-Month Database Changes

```
═══════════════════════════════════════════════════════════
MONTH 1: October 17 - November 17
═══════════════════════════════════════════════════════════

DAY 1 (Oct 17): Subscription Created
─────────────────────────────────────
Subscriptions:
  ├─ INSERT: sub_111 (Status: Pending)
  └─ UPDATE: sub_111 (Status: Active after payment)

UserSubscriptionPrivilegeUsage:
  ├─ INSERT: Teleconsultation (5, 0, 5)
  └─ INSERT: Medication (3, 0, 3)

BillingRecords:
  ├─ INSERT: bill_001 (Subscription, $275, Paid)

SubscriptionPayments:
  └─ INSERT: pay_001 ($275, Success)

SubscriptionStatusHistory:
  ├─ INSERT: NULL → Pending
  └─ INSERT: Pending → Active

DAY 3-22: Progressive Usage
─────────────────────────────────────
UserSubscriptionPrivilegeUsage:
  Oct 19: Teleconsult (5, 1, 4)
  Oct 23: Teleconsult (5, 2, 3)
  Oct 26: Teleconsult (5, 3, 2)
  Oct 31: Teleconsult (5, 4, 1)
  Nov 07: Teleconsult (5, 5, 0)

PrivilegeUsageHistory:
  └─ INSERT: 5 records (All Type: Included, Cost: $0)

DAY 25 (Nov 10): Overage Purchase
─────────────────────────────────────
BillingRecords:
  └─ INSERT: bill_002 (Overage, $25, Paid)

UserSubscriptionPrivilegeUsage:
  ├─ UPDATE: Teleconsult (6, 5, 1) ← Credit added
  └─ UPDATE: Teleconsult (6, 6, 0) ← Credit used

PrivilegeUsageHistory:
  └─ INSERT: 1 record (Type: Overage, Cost: $25)

SubscriptionPayments:
  └─ INSERT: pay_002 ($25, Success)

DAY 30 (Nov 17): Monthly Renewal
─────────────────────────────────────
BillingRecords:
  └─ INSERT: bill_003 (Subscription, $275, Paid)

UserSubscriptionPrivilegeUsage:
  ├─ UPDATE: Teleconsult (5, 0, 5) ← RESET!
  └─ UPDATE: Medication (3, 0, 3) ← RESET!

SubscriptionPayments:
  └─ INSERT: pay_003 ($275, Success)

Subscriptions:
  ├─ UPDATE: EndDate (Nov 17 → Dec 17)
  └─ UPDATE: NextBillingDate (Dec 17)

SubscriptionStatusHistory:
  └─ INSERT: Active → Active (renewed)

═══════════════════════════════════════════════════════════
MONTH 2: November 17 - December 17
═══════════════════════════════════════════════════════════

Fresh cycle starts with reset privileges...
(Pattern repeats)
```

---

## 10. STRIPE ↔ DATABASE SYNCHRONIZATION

### Bidirectional Sync Map

```
YOUR DATABASE          DIRECTION          STRIPE
─────────────────────────────────────────────────────

Users.StripeCustomerId ←─────── cus_XYZ789
                       ────────→ metadata.userId

SubscriptionPlans      ────────→ Product
  .StripeProductId     ←─────── prod_ABC123
  .StripeMonthlyPriceId ←────── price_1Month_XYZ
                       ────────→ metadata.planId

Subscriptions          ←────┬──→ Subscription
  .StripeSubscriptionId      │   sub_stripe_AAA
  .Status                    │   .status
  .NextBillingDate           │   .current_period_end
                       ──────┴──→ metadata.subscriptionId

BillingRecords         ←────┬──→ Invoice
  .StripeInvoiceId           │   in_stripe_BBB
  .Amount                    │   .amount_due
  .Status                    │   .status
  .PaidDate                  │   .status_transitions
                       ──────┴──→ metadata.billingRecordId

BillingRecords         ←─────── PaymentIntent
  .StripePaymentIntentId      pi_DEF456
                       ────────→ metadata.billingRecordId
```

### When Sync Happens

**PUSH (Your System → Stripe):**
- Admin creates plan → Create Stripe product
- User subscribes → Create Stripe subscription
- User updates payment method → Update Stripe customer
- Admin cancels subscription → Cancel Stripe subscription

**PULL (Stripe → Your System via Webhooks):**
- Payment succeeds → Update subscription status
- Payment fails → Update failure count
- Renewal occurs → Create billing record, reset privileges
- Subscription cancelled in Stripe → Update local status

---

## KEY TAKEAWAYS

### ✅ Database Design Principles

1. **Normalized Design** - No data duplication
2. **Audit Trail** - History tables for everything
3. **Soft Deletes** - IsActive flags, not DELETE
4. **Foreign Keys** - Referential integrity enforced
5. **Indexed** - Fast queries on common lookups
6. **Guid PKs** - Scalable, distributed-friendly

### ✅ Synchronization Strategy

1. **Two-Way Sync** - PUSH (API) + PULL (Webhooks)
2. **Metadata Links** - IDs stored in both systems
3. **Idempotency** - Prevent duplicate webhook processing
4. **Eventual Consistency** - Webhook retries ensure sync
5. **Cleanup on Failure** - Delete Stripe resources if DB fails

---

**Use this as a reference while developing!**

**Document Version:** 1.0  
**Last Updated:** October 17, 2025


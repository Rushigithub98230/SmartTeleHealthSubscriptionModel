# Subscription Lifecycle & Billing - Complete Client Walkthrough

**SmartTeleHealth Subscription Management System**  
**For:** Client Understanding & Stakeholder Review  
**Version:** 1.0 | **Date:** October 18, 2025

---

## 📑 Table of Contents

1. [System Overview](#1-system-overview)
2. [Subscription Plan Creation (Admin)](#2-subscription-plan-creation-admin)
3. [User Subscription Purchase](#3-user-subscription-purchase)
4. [Subscription Lifecycle Management](#4-subscription-lifecycle-management)
5. [Privilege Assignment & Tracking](#5-privilege-assignment--tracking)
6. [Billing Cycle Operations](#6-billing-cycle-operations)
7. [Subscription Renewal Process](#7-subscription-renewal-process)
8. [Complete Flow Diagrams](#8-complete-flow-diagrams)

---

## 1. System Overview

### What This System Does

The SmartTeleHealth subscription system manages the complete lifecycle of healthcare subscriptions from creation to renewal, including:

- **Plans:** Admin-defined packages with pricing and features
- **Subscriptions:** User purchases with flexible billing cycles
- **Privileges:** Feature access (consultations, uploads, etc.)
- **Billing:** Automated recurring charges with Stripe
- **Tracking:** Real-time usage monitoring
- **Renewal:** Automatic billing and privilege reset

### Key Components

```
┌──────────────────────────────────────────────────────────────┐
│                    SYSTEM COMPONENTS                          │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  1. SUBSCRIPTION PLANS (Admin Creates)                       │
│     ├─ Base monthly price                                   │
│     ├─ Included privileges (consultations, uploads, etc.)   │
│     ├─ Billing cycle options (Monthly/Quarterly/Annual)     │
│     └─ Discounts for longer commitments                     │
│                                                              │
│  2. USER SUBSCRIPTIONS (Users Purchase)                      │
│     ├─ Selected plan + billing cycle                        │
│     ├─ Calculated price (scaled & discounted)               │
│     ├─ Allocated privileges (scaled to billing cycle)       │
│     └─ Payment through Stripe                               │
│                                                              │
│  3. PRIVILEGE MANAGEMENT (System Tracks)                     │
│     ├─ Real-time usage monitoring                           │
│     ├─ Limit enforcement                                    │
│     ├─ Overage handling                                     │
│     └─ Period-based reset                                   │
│                                                              │
│  4. BILLING & RENEWAL (Automated)                            │
│     ├─ Daily background job checks due subscriptions        │
│     ├─ Calculate billing amount (base + overage)            │
│     ├─ Process payment via Stripe                           │
│     └─ Reset privileges on success                          │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

## 2. Subscription Plan Creation (Admin)

### Step-by-Step: Admin Creates "Family Care" Plan

**Stage 1: Create Base Plan**

Admin logs into dashboard and navigates to Plans → Create New

```
┌─────────────────────────────────────────────────────────┐
│  CREATE SUBSCRIPTION PLAN                                │
├─────────────────────────────────────────────────────────┤
│  Plan Name: Family Care                                 │
│  Description: Comprehensive family healthcare coverage  │
│  Category: Healthcare Plans                             │
│                                                          │
│  BASE PRICING:                                           │
│  Monthly Price: $150.00                                  │
│                                                          │
│  BILLING CYCLE DISCOUNTS:                                │
│  ○ Monthly Billing:    0% discount                       │
│  ○ Quarterly Billing:  5% discount (save $22.50)        │
│  ○ Annual Billing:     15% discount (save $270)         │
│                                                          │
│  [Next: Add Privileges →]                                │
└─────────────────────────────────────────────────────────┘
```

**Backend Processing:**
- **API:** `POST /api/SubscriptionPlans`
- **Service:** `SubscriptionPlanService`
- **Database:** INSERT into `SubscriptionPlans` table
  ```sql
  INSERT INTO SubscriptionPlans (
      Id, Name, Price,
      MonthlyBillingDiscount, QuarterlyBillingDiscount, AnnualBillingDiscount,
      IsActive
  ) VALUES (
      NEWID(), 'Family Care', 150.00,
      0.00, 5.00, 15.00,
      0  -- Not active yet
  );
  ```

---

**Stage 2: Add Privileges to Plan**

```
┌─────────────────────────────────────────────────────────┐
│  CONFIGURE PRIVILEGES - Family Care Plan                 │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ☑ Video Consultations                                  │
│     Monthly Allocation: 10 consultations                 │
│     Overage Price: $15 per additional                    │
│     Daily Limit: 3 per day (optional)                    │
│                                                          │
│  ☑ Chat Messages                                        │
│     Monthly Allocation: Unlimited                        │
│     Overage Price: N/A                                   │
│                                                          │
│  ☑ Document Uploads                                     │
│     Monthly Allocation: 20 uploads                       │
│     Overage Price: $2 per additional                     │
│                                                          │
│  ☑ Prescription Refills                                 │
│     Monthly Allocation: 10 refills                       │
│     Overage Price: $5 per additional                     │
│                                                          │
│  ☑ Health Reports                                       │
│     Monthly Allocation: 5 reports                        │
│     Overage Price: $10 per additional                    │
│                                                          │
│  ☑ Specialist Consultations                             │
│     Monthly Allocation: 2 consultations                  │
│     Overage Price: $50 per additional                    │
│                                                          │
│  [Save Configuration]                                    │
└─────────────────────────────────────────────────────────┘
```

**Backend Processing:**
- **API:** `POST /api/SubscriptionPlans/{planId}/privileges` (called 6 times)
- **Service:** `SubscriptionPlanService.AddPrivilegeToPlanAsync()`
- **Database:** INSERT into `SubscriptionPlanPrivileges` table
  ```sql
  INSERT INTO SubscriptionPlanPrivileges (
      Id, SubscriptionPlanId, PrivilegeId,
      Value, MonthlyLimit, UnitCost
  ) VALUES
      (NEWID(), @planId, @videoConsultPrivilegeId, 10, 10, 15.00),
      (NEWID(), @planId, @chatPrivilegeId, -1, -1, 0),  -- Unlimited
      (NEWID(), @planId, @uploadPrivilegeId, 20, 20, 2.00),
      (NEWID(), @planId, @refillPrivilegeId, 10, 10, 5.00),
      (NEWID(), @planId, @reportPrivilegeId, 5, 5, 10.00),
      (NEWID(), @planId, @specialistPrivilegeId, 2, 2, 50.00);
  ```

**Key Concept:** Admin sets **monthly limits**. The system automatically scales these to the user's billing cycle.

---

**Stage 3: Activate Plan**

Admin reviews and activates the plan:

```
┌─────────────────────────────────────────────────────────┐
│  PLAN REVIEW - Family Care                               │
├─────────────────────────────────────────────────────────┤
│  Base Price: $150/month                                  │
│  Privileges: 6 configured ✓                             │
│  Stripe Integration: Connected ✓                        │
│  Billing Cycles: 3 options configured ✓                 │
│                                                          │
│  PREVIEW FOR USERS:                                      │
│  Monthly: $150/month (no discount)                       │
│  Quarterly: $427.50/quarter (save 5%)                    │
│  Annual: $1,530/year (save 15%)                          │
│                                                          │
│  [Activate Plan] [Save as Draft]                         │
└─────────────────────────────────────────────────────────┘
```

**Backend:**
- **API:** `PUT /api/SubscriptionPlans/{planId}`
- **Update:** `IsActive = 1`
- **Result:** Plan appears in public listing

---

## 3. User Subscription Purchase

### Complete Purchase Flow

**Step 1: User Browses Plans**

User visits the website and views available plans:

```
┌──────────────────────────────────────────────────────────────┐
│              CHOOSE YOUR HEALTHCARE PLAN                      │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────┐   ┌──────────────┐   ┌─────────────────┐   │
│  │ Basic Care │   │ Family Care  │   │ Premium Care    │   │
│  │  $50/mo    │   │  $150/mo ★   │   │   $300/mo       │   │
│  │ 3 consults │   │ 10 consults  │   │ 25 consults     │   │
│  │            │   │  POPULAR     │   │                 │   │
│  └────────────┘   └──────────────┘   └─────────────────┘   │
│                          ↑ SELECTED                          │
└──────────────────────────────────────────────────────────────┘
```

**API Call:**
```
GET /api/SubscriptionPlans/active?page=1&pageSize=10

Response: List of active plans with privileges
```

---

**Step 2: Select Billing Cycle**

User clicks on "Family Care" and sees billing options:

```
┌──────────────────────────────────────────────────────────────┐
│  FAMILY CARE PLAN - SELECT BILLING                           │
├──────────────────────────────────────────────────────────────┤
│  Base Price: $150/month                                      │
│  What's Included Each Month:                                 │
│  • 10 Video Consultations                                    │
│  • Unlimited Chat Messages                                   │
│  • 20 Document Uploads                                       │
│  • 10 Prescription Refills                                   │
│  • 5 Health Reports                                          │
│  • 2 Specialist Consultations                                │
├──────────────────────────────────────────────────────────────┤
│  CHOOSE BILLING CYCLE:                                       │
│                                                              │
│  ○ Monthly - $150/month                                      │
│    No discount | Cancel anytime                             │
│    Total per year: $1,800                                    │
│                                                              │
│  ● Annual - $1,530/year ⭐ SAVE $270!                       │
│    15% discount | Best value                                │
│                                                              │
│    YOUR BENEFITS FOR THE FULL YEAR:                          │
│    • 122 Video Consultations (10 × 12.17 months, rounded)   │
│    • Unlimited Chat Messages                                 │
│    • 244 Document Uploads (20 × 12.17 months, rounded)       │
│    • 122 Prescription Refills (10 × 12.17 months, rounded)  │
│    • 61 Health Reports (5 × 12.17 months, rounded)           │
│    • 25 Specialist Consultations (2 × 12.17 months, rounded)│
│                                                              │
│    Valid for entire year (no monthly resets!)                │
│                                                              │
│  [Continue to Payment →]                                     │
└──────────────────────────────────────────────────────────────┘
```

**Key Point:** Notice how privileges are **multiplied by 12** for annual billing!

---

**Step 3: Enter Payment Information**

```
┌──────────────────────────────────────────────────────────────┐
│  CHECKOUT - Family Care (Annual)                             │
├──────────────────────────────────────────────────────────────┤
│  Order Summary:                                              │
│  Family Care Plan (Annual)                     $1,800.00     │
│  Annual Billing Discount (15%)                  -$270.00     │
│  ─────────────────────────────────────────────────────────   │
│  TOTAL DUE TODAY:                              $1,530.00     │
│                                                              │
│  Valid: Jan 1, 2025 - Dec 31, 2025                          │
│  Next Billing: Jan 1, 2026 ($1,530.00)                      │
├──────────────────────────────────────────────────────────────┤
│  PAYMENT METHOD:                                             │
│                                                              │
│  Card Number: [____-____-____-____]                         │
│  Expiry: [MM/YY]  CVV: [___]                                │
│  Name on Card: [________________]                            │
│                                                              │
│  ☑ Save card for automatic renewals                         │
│                                                              │
│  🔒 Secure payment powered by Stripe                        │
│                                                              │
│  [Complete Purchase] →                                       │
└──────────────────────────────────────────────────────────────┘
```

---

**Step 4: Backend Processing**

**API Call:**
```http
POST /api/Subscriptions
{
  "userId": 12345,
  "planId": "family-care-plan-guid",
  "billingCycleId": "annual-cycle-guid",
  "paymentMethodId": "pm_stripe_1234567890"
}
```

**Service Flow:**

**A) Validation** (`SubscriptionLifecycleService.CreateSubscriptionAsync()` - Line 85)
```
1. Check plan exists and is active ✓
2. Prevent duplicate active subscriptions ✓
3. Validate payment method with Stripe ✓
4. Check billing cycle allowed for this plan ✓
   → BillingCycleValidator.IsValidBillingCycleForPlan()
```

**B) Price Calculation** (Line 170-180)
```csharp
Monthly Price: $150
Billing Cycle: Annual (365 days)
Months in Cycle: 365 / 30 = 12.17

Base Price: $150 × 12.17 = $1,825
Discount: 15% = $273.75
Final Price: $1,825 - $273.75 = $1,551.25 → $1,530 (rounded)
```

**C) Subscription Creation**
```sql
INSERT INTO Subscriptions (
    UserId, SubscriptionPlanId, BillingCycleId,
    CurrentPrice,        -- $1,530
    StartDate,           -- Jan 1, 2025
    NextBillingDate,     -- Jan 1, 2026
    LastBillingDate,     -- NULL (first subscription)
    Status              -- 'PendingPayment'
) VALUES (...);
```

**D) Privilege Allocation** (`PrivilegeService.CalculatePrivilegeAllocationAsync()` - Line 1207)

For each plan privilege:
```csharp
// Video Consultations
Monthly Limit: 10
Billing Cycle Days: 365
Months in Cycle: 365 / 30 = 12.17
Allowed for Cycle: Math.Ceiling(10 × 12.17) = 122 consultations

// Create usage record
INSERT INTO UserSubscriptionPrivilegeUsages (
    SubscriptionId,
    PrivilegeId,
    AllowedValue,        -- 122
    UsedValue,           -- 0
    UsagePeriodStart,    -- Jan 1, 2025
    UsagePeriodEnd       -- Jan 1, 2026
);
```

**E) Payment Processing** (`PaymentService.ProcessPaymentAsync()` - Line 78)
```
1. Create BillingRecord (Amount: $1,530, Type: Subscription)
2. Create SubscriptionPayment record
3. Call Stripe API → Charge $1,530
4. Stripe responds: "succeeded"
5. Update records (transaction-safe):
   - BillingRecord → Paid
   - SubscriptionPayment → Completed
   - Subscription → Active
6. Send confirmation email
```

**Result:**
```
✅ Subscription Active
✅ 122 Video Consultations allocated
✅ 244 Document Uploads allocated
✅ All privileges valid until Jan 1, 2026
✅ User can start using services
```

---

## 4. Subscription Lifecycle Management

### Subscription States

```
┌─────────────────────────────────────────────────────────────┐
│             SUBSCRIPTION LIFECYCLE STATES                    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. PendingPayment → Waiting for initial payment            │
│  2. Active → Fully operational, can use all privileges      │
│  3. Trial → Trial period, limited or full access            │
│  4. PastDue → Payment failed, grace period (3 days)         │
│  5. Suspended → After failed payment attempts (3x)          │
│  6. Paused → User temporarily paused subscription           │
│  7. Cancelled → User cancelled, may have end date           │
│  8. Expired → Subscription ended naturally                  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### State Transitions

```
            ┌─────────────┐
            │PendingPayment│
            └──────┬──────┘
                   │ Payment Succeeds
                   ↓
            ┌─────────────┐
      ┌────→│   Active    │←────┐
      │     └──────┬──────┘     │
      │            │             │ Resume
      │            ↓             │
      │     ┌─────────────┐     │
      │     │  PastDue    │     │
      │     └──────┬──────┘     │
      │            │ 3 failures  │
      │            ↓             │
      │     ┌─────────────┐     │
      │     │ Suspended   │─────┘
      │     └─────────────┘  Payment Updated
      │
      │     ┌─────────────┐
      └─────│   Paused    │
User Pauses └──────┬──────┘
                   │ User Resumes
                   ↓
            ┌─────────────┐
            │  Cancelled  │
            └──────┬──────┘
                   │ End Date Reached
                   ↓
            ┌─────────────┐
            │   Expired   │
            └─────────────┘
```

---

### Lifecycle Operations

**Active Subscription:**
```
What User Can Do:
✓ Use all allocated privileges
✓ View usage dashboard
✓ Book consultations/services
✓ Upload documents
✓ Access health reports

What System Does:
→ Track usage in real-time
→ Enforce limits
→ Allow overage with payment
→ Prepare for next billing
```

**Paused Subscription:**
```
User Action: "I'm traveling for 2 months, pause my subscription"

API: POST /api/Subscriptions/{id}/pause
Service: SubscriptionLifecycleService.PauseSubscriptionAsync()

Changes:
- Status: Active → Paused
- NextBillingDate: Postponed (optional)
- Privileges: Frozen (usage = current state)

User Cannot:
✗ Use privileges
✗ Book new consultations

User Can:
✓ Resume subscription anytime
✓ View past usage history
```

**Suspended Subscription:**
```
Trigger: 3 failed payment attempts

System Actions:
1. Status: Active → Suspended
2. Block all privilege usage
3. Send email: "Subscription Suspended - Update Payment"
4. Retain usage data (for resume)

To Reactivate:
→ User updates payment method
→ System retries payment
→ If success: Status → Active, privileges restored
```

---

## 5. Privilege Assignment & Tracking

### How Privileges Flow from Plan to User

```
┌──────────────────────────────────────────────────────────────┐
│         PRIVILEGE FLOW: PLAN → SUBSCRIPTION → USAGE          │
└──────────────────────────────────────────────────────────────┘

STAGE 1: Admin Configures Plan
┌─────────────────────────────────┐
│ SubscriptionPlanPrivilege       │
│ ─────────────────────────────── │
│ Plan: Family Care               │
│ Privilege: Video Consultation   │
│ MonthlyLimit: 10                │  ← Admin sets this
│ UnitCost: $15 (overage)         │
│ IsUnlimited: false              │
└─────────────────────────────────┘
            ↓
STAGE 2: User Subscribes (Annual Billing)
┌─────────────────────────────────┐
│ Subscription                    │
│ ─────────────────────────────── │
│ User: Sarah                     │
│ Plan: Family Care               │
│ BillingCycle: Annual (365 days) │
│ CurrentPrice: $1,530            │
│ NextBillingDate: Jan 1, 2026    │
└─────────────────────────────────┘
            ↓
STAGE 3: System Calculates Allocation
┌─────────────────────────────────┐
│ CalculatePrivilegeAllocation    │
│ ─────────────────────────────── │
│ MonthlyLimit: 10                │
│ BillingCycleDays: 365           │
│ MonthsInCycle: 365/30 = 12.17   │
│ Allowed: 10 × 12.17 = 122       │  ← System calculates
└─────────────────────────────────┘
            ↓
STAGE 4: Create Usage Record
┌─────────────────────────────────┐
│ UserSubscriptionPrivilegeUsage  │
│ ─────────────────────────────── │
│ Subscription: Sarah's           │
│ Privilege: Video Consultation   │
│ AllowedValue: 122               │  ← Calculated allocation
│ UsedValue: 0                    │  ← Starts at zero
│ UsagePeriodStart: Jan 1, 2025   │
│ UsagePeriodEnd: Jan 1, 2026     │
└─────────────────────────────────┘
            ↓
STAGE 5: User Uses Privilege
┌─────────────────────────────────┐
│ Update Usage                    │
│ ─────────────────────────────── │
│ UsedValue: 0 → 1 → 2 → ... → 45│  ← Incremented on each use
│ LastUsedAt: Updated             │
└─────────────────────────────────┘
            ↓
STAGE 6: History Recorded
┌─────────────────────────────────┐
│ PrivilegeUsageHistory           │
│ ─────────────────────────────── │
│ Each use creates new record:    │
│ UsedAt: Oct 15, 2025 2:30 PM   │
│ Amount: 1                       │
│ UsageDate, Week, Month          │  ← For time-based limits
└─────────────────────────────────┘
```

---

### Real-Time Tracking Example

**Scenario:** Sarah books a video consultation on March 15, 2025

**Frontend View Before:**
```
┌──────────────────────────────────────────────────────────────┐
│  MY SUBSCRIPTION - Family Care (Annual)                      │
├──────────────────────────────────────────────────────────────┤
│  Video Consultations                                         │
│  ███████████░░░░░░░░░░░░░░░░░░░░░░░░░ 44 / 122 (36%)       │
│  Last used: March 10, 2025                                   │
│  Remaining: 78 consultations                                 │
│                                                              │
│  [Book Video Consultation] →                                 │
└──────────────────────────────────────────────────────────────┘
```

**User Clicks "Book Video Consultation"**

**Backend Processing:**
```
1. Check Availability (before booking)
   API: GET /api/Privileges/availability?subscriptionId=xxx&privilegeName=Video+Consultation&amount=1
   
   Service: PrivilegeService.CheckPrivilegeAvailabilityAsync()
   
   Query:
   SELECT AllowedValue, UsedValue
   FROM UserSubscriptionPrivilegeUsages
   WHERE SubscriptionId = 'sarah-sub-guid'
     AND PrivilegeId = 'video-consult-guid';
   
   Result: Allowed=122, Used=44, Remaining=78
   Response: { "available": true, "remaining": 78 }

2. User Completes Booking

3. Consume Privilege
   API: POST /api/Privileges/use
   Body: { subscriptionId, privilegeName: "Video Consultation", amount: 1 }
   
   Service: PrivilegeService.UsePrivilegeAsync() - Line 232
   
   Updates:
   UPDATE UserSubscriptionPrivilegeUsages
   SET UsedValue = 45,              -- 44 + 1
       LastUsedAt = '2025-03-15 14:30:00',
       UpdatedDate = GETUTCDATE()
   WHERE Id = 'usage-record-guid';
   
   INSERT INTO PrivilegeUsageHistory (
       UserSubscriptionPrivilegeUsageId,
       UsedValue, UsedAt, UsageDate, UsageMonth
   ) VALUES (
       'usage-record-guid',
       1, '2025-03-15 14:30:00', '2025-03-15', '2025-03'
   );

4. Return Success
```

**Frontend View After:**
```
┌──────────────────────────────────────────────────────────────┐
│  ✓ CONSULTATION BOOKED                                       │
│  Dr. Emily Smith - March 15, 2025 at 2:30 PM                │
│                                                              │
│  Video Consultations                                         │
│  ████████████░░░░░░░░░░░░░░░░░░░░░░░ 45 / 122 (37%)        │
│  Last used: Today at 2:30 PM                                 │
│  Remaining: 77 consultations                                 │
└──────────────────────────────────────────────────────────────┘
```

---

### Privilege Tracking Entities & Services

**Entities Involved:**

1. **SubscriptionPlanPrivilege** (Configuration)
   - File: `backend/SmartTelehealth.Core/Entities/SubscriptionPlanPrivilege.cs`
   - Fields: `MonthlyLimit`, `UnitCost`, `DailyLimit`, `WeeklyLimit`

2. **UserSubscriptionPrivilegeUsage** (Current State)
   - File: `backend/SmartTelehealth.Core/Entities/UserSubscriptionPrivilegeUsage.cs`
   - Fields: `AllowedValue`, `UsedValue`, `UsagePeriodStart`, `UsagePeriodEnd`
   - Computed: `RemainingValue`, `UsagePercentage`, `IsExhausted`

3. **PrivilegeUsageHistory** (Audit Trail)
   - File: `backend/SmartTelehealth.Core/Entities/PrivilegeUsageHistory.cs`
   - Fields: `UsedAt`, `UsageDate`, `UsageWeek`, `UsageMonth`

**Services Involved:**

1. **PrivilegeService**
   - File: `backend/SmartTelehealth.Application/Services/PrivilegeService.cs`
   - Methods:
     - `UsePrivilegeAsync()` - Consume privilege
     - `GetRemainingPrivilegeAsync()` - Check remaining
     - `CalculatePrivilegeAllocationAsync()` - Calculate for billing cycle
     - `CheckTimeBasedLimitsAsync()` - Daily/weekly/monthly caps

2. **PaymentService**
   - File: `backend/SmartTelehealth.Application/Services/PaymentService.cs`
   - Method: `ResetPrivilegesForNewBillingPeriodAsync()` - Reset on billing

---

## 6. Billing Cycle Operations

### How Billing Cycles Work

**Monthly Billing:**
```
Timeline: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━→
          Jan      Feb      Mar      Apr      May
          ↓        ↓        ↓        ↓        ↓
Payment:  $150     $150     $150     $150     $150
Privileges: 10↻     10↻      10↻      10↻      10↻
Period:   30d      30d      30d      30d      30d

Characteristics:
- Pay every month
- Privileges reset every month
- No long-term commitment
- No discount
- Total/year: $1,800
```

**Quarterly Billing:**
```
Timeline: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━→
          Q1           Q2           Q3           Q4
          Jan-Mar      Apr-Jun      Jul-Sep      Oct-Dec
          ↓            ↓            ↓            ↓
Payment:  $427.50      $427.50      $427.50      $427.50
Privileges: 30↻         30↻          30↻          30↻
Period:   90d         90d          90d          90d

Characteristics:
- Pay every 3 months
- Privileges reset every quarter
- 5% discount
- Total/year: $1,710 (save $90)
```

**Annual Billing:**
```
Timeline: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━→
          Year 1                                   Year 2
          Jan 2025 ─────────────────────────────→ Jan 2026
          ↓                                        ↓
Payment:  $1,530 (once)                           $1,530 (once)
Privileges: 122↻                                   122↻
Period:   365 days                                365 days

Characteristics:
- Pay once per year
- Privileges valid for full year
- NO monthly resets
- 15% discount
- Total/year: $1,530 (save $270)
```

### Billing Cycle Services

**AutomatedBillingService** - Handles recurring billing  
**File:** `backend/SmartTelehealth.Application/Services/AutomatedBillingService.cs`

**Key Methods:**
- `ProcessRecurringBillingAsync()` - Daily job (runs at 2 AM)
- `ProcessSubscriptionBillingAsync()` - Individual subscription billing
- `CalculateBillingAmountAsync()` - Calculate amount with scaling
- `CalculateBillingCycleDiscount()` - Apply discounts

---

### Daily Billing Job

**Schedule:** Runs every day at 2:00 AM

**Process:**
```
2:00 AM Daily
    ↓
AutomatedBillingService.ProcessRecurringBillingAsync()
    ↓
Query: Get subscriptions where NextBillingDate = Today
    ↓
For each subscription due:
    ├─ Check: Is subscription active? ✓
    ├─ Migration: Fix pricing if needed
    ├─ Calculate: Billing amount (base + overage)
    ├─ Create: BillingRecord
    ├─ Process: Payment via Stripe
    └─ On Success:
        ├─ Update subscription dates
        └─ Reset privileges
```

**SQL Query:**
```sql
SELECT s.*
FROM Subscriptions s
WHERE s.NextBillingDate <= CAST(GETUTCDATE() AS DATE)
  AND s.IsActive = 1
  AND s.Status = 'Active'
ORDER BY s.NextBillingDate;
```

---

## 7. Subscription Renewal Process

### Complete Renewal Flow (Day 365)

**Scenario:** Sarah's annual subscription renews on Jan 1, 2026

**Current State (Dec 31, 2025 - 11:59 PM):**
```
Subscription:
- Status: Active
- LastBillingDate: Jan 1, 2025
- NextBillingDate: Jan 1, 2026
- CurrentPrice: $1,530

Privilege Usage:
- Video Consultations: 125/122 used (3 over limit)
- Document Uploads: 200/244 used
- Prescription Refills: 110/120 used

Overage Charges:
- 3 extra video consultations × $15 = $45
```

---

**Renewal Processing (Jan 1, 2026 - 2:00 AM):**

**Step 1: Background Service Runs**
```
AutomatedBillingService scheduled task executes
    ↓
ProcessRecurringBillingAsync(systemToken)
    ↓
Find subscriptions due for billing:
    Query: NextBillingDate = '2026-01-01'
    Found: Sarah's subscription
```

**Step 2: Price Migration Check**
```
Service: AutomatedBillingService.MigrateSubscriptionPricingIfNeededAsync()
Line: 577

Check if pricing is correct:
    Expected Price = CalculateBillingAmountAsync()
    Current Price = subscription.CurrentPrice
    
    If (Math.Abs(Expected - Current) > 0.01) {
        Log: "Migrating pricing from $1,530 to $1,530"
        Update subscription.CurrentPrice
    }
    
Result: ✓ Price correct, no migration needed
```

**Step 3: Calculate Billing Amount**
```
Service: AutomatedBillingService.CalculateBillingAmountAsync()
Line: 932

Calculation:
    Monthly Price: $150
    Billing Cycle: 365 days
    Months: 365 / 30 = 12.17
    
    Base Price: $150 × 12.17 = $1,825
    
    Billing Cycle Discount: (Line 969)
        Annual discount: 15%
        Discount Amount: $1,825 × 0.15 = $273.75
        
    Discounted Price: $1,825 - $273.75 = $1,551.25
    
    Round to: $1,530
    
    Additional Discounts: $0 (none applied)
    Adjustments: $0 (none)
    
    FINAL AMOUNT: $1,530
```

**Step 4: Calculate Overage**
```
Service: AutomatedBillingService.ProcessOverageChargesAsync()
Line: 1667

Check each privilege:
    Video Consultations:
        Used: 125
        Allowed: 122
        Overage: 125 - 122 = 3
        Cost: 3 × $15 = $45
    
    Document Uploads:
        Used: 200
        Allowed: 244
        Overage: 0 (under limit)
    
    Total Overage: $45
```

**Step 5: Create Billing Records**
```
Two billing records created:

1. Recurring Subscription Billing:
   INSERT INTO BillingRecords (
       SubscriptionId, Amount, Type, Description, DueDate
   ) VALUES (
       'sarah-sub-guid', 1530.00, 'Recurring',
       'Annual subscription renewal', '2026-01-01'
   );

2. Overage Billing:
   INSERT INTO BillingRecords (
       SubscriptionId, Amount, Type, Description, DueDate
   ) VALUES (
       'sarah-sub-guid', 45.00, 'Overage',
       '3 additional video consultations', '2026-01-01'
   );

Total to charge: $1,530 + $45 = $1,575
```

**Step 6: Process Payment**
```
Service: PaymentService.ProcessPaymentAsync()
Line: 78

For recurring billing record:
    1. Create SubscriptionPayment record
    2. Call StripeBillingService.ProcessStripePaymentAsync()
    3. Stripe charges saved card: $1,530
    4. Response: "succeeded" ✓

For overage billing record:
    1. Create SubscriptionPayment record
    2. Call StripeBillingService.ProcessStripePaymentAsync()
    3. Stripe charges saved card: $45
    4. Response: "succeeded" ✓

Both payments succeeded!
```

**Step 7: Update Records (Transaction)**
```
Service: PaymentService.UpdatePaymentRecordsAsync()
Line: 1120

using var transaction = await _unitOfWork.BeginTransactionAsync();
try {
    // Update billing records
    UPDATE BillingRecords 
    SET Status = 'Paid', PaidDate = GETUTCDATE()
    WHERE Id IN ('recurring-guid', 'overage-guid');
    
    // Update subscription payments
    UPDATE SubscriptionPayments
    SET Status = 'Completed', PaidDate = GETUTCDATE()
    WHERE BillingRecordId IN ('recurring-guid', 'overage-guid');
    
    // Update subscription
    UPDATE Subscriptions
    SET LastBillingDate = '2026-01-01',
        NextBillingDate = '2027-01-01',  -- +365 days
        Status = 'Active'
    WHERE Id = 'sarah-sub-guid';
    
    // RESET PRIVILEGES
    await ResetPrivilegesForNewBillingPeriodAsync(subscription);
    
    await _unitOfWork.CommitTransactionAsync();
}
catch {
    await _unitOfWork.RollbackTransactionAsync();
}
```

**Step 8: Reset Privileges**
```
Service: PaymentService.ResetPrivilegesForNewBillingPeriodAsync()
Line: 1197

Get all privilege usage records for Sarah's subscription

For each privilege:
    
    Video Consultations:
        Monthly Limit: 10
        Billing Cycle: 365 days
        Months: 12.17
        New Allowed: Math.Ceiling(10 × 12.17) = 122
        
        UPDATE UserSubscriptionPrivilegeUsages
        SET UsedValue = 0,                    -- Reset to zero!
            AllowedValue = 122,                -- Recalculated
            UsagePeriodStart = '2026-01-02',  -- Day after billing
            UsagePeriodEnd = '2027-01-01',    -- Next billing date
            ResetAt = '2026-01-01 02:00:15'
        WHERE Id = 'video-usage-guid';
    
    Document Uploads:
        Reset similar: Used=0, Allowed=244
    
    Prescription Refills:
        Reset similar: Used=0, Allowed=122
    
    ... (all privileges reset)
```

**Step 9: Send Email**
```
To: sarah@email.com
Subject: 🎉 Subscription Renewed - Family Care

Hi Sarah,

Your Family Care subscription has been renewed!

PAYMENT PROCESSED:
━━━━━━━━━━━━━━━━━━━━━━━━
Subscription Renewal: $1,530.00
Overage Charges:        $45.00
                      ─────────
Total Charged:       $1,575.00
━━━━━━━━━━━━━━━━━━━━━━━━

YOUR FRESH PRIVILEGES (Year 2):
✓ 122 Video Consultations
✓ Unlimited Chat Messages
✓ 244 Document Uploads
✓ 122 Prescription Refills
✓ 61 Health Reports
✓ 25 Specialist Consultations

Valid Until: January 1, 2027

Start using your benefits today!
[Open Dashboard] [View Receipt]
```

---

### Renewal Failure Scenario

**If Payment Fails:**

```
Stripe response: "payment_failed" (e.g., card expired)
    ↓
UpdatePaymentRecordsAsync():
    ├─ BillingRecord → Failed
    ├─ SubscriptionPayment → Failed
    ├─ Subscription → PastDue
    ├─ Set NextRetryAt: Tomorrow (Jan 2)
    └─ AttemptCount: 1

Email sent:
"⚠️ Payment Failed - Subscription Renewal"
"We'll retry automatically tomorrow"
"Please update your payment method"

Day 2 (Jan 2):
    ProcessFailedPaymentRetryAsync()
    → Retry payment → Fails again
    → NextRetryAt: Jan 3
    → AttemptCount: 2

Day 3 (Jan 3):
    Retry → Fails again
    → NextRetryAt: Jan 4
    → AttemptCount: 3

Day 4 (Jan 4): FINAL ATTEMPT
    Retry → Still fails
    → AttemptCount: 3 (max reached)
    → HandleMaxRetriesExceededAsync():
        ├─ Subscription → Suspended
        ├─ SubscriptionPayment → Failed (final)
        └─ Email: "Subscription Suspended"

User cannot use privileges until payment method updated
```

---

## 8. Complete Flow Diagrams

### End-to-End System Flow

```
┌─────────────────────────────────────────────────────────────────┐
│  COMPLETE SUBSCRIPTION LIFECYCLE FLOW                           │
└─────────────────────────────────────────────────────────────────┘

═══════════════════════════════════════════════════════════════════
PHASE 1: PLAN CREATION (Admin Side)
═══════════════════════════════════════════════════════════════════

Admin Dashboard
    ↓
[Create Plan] → Enter details (Name, Price, Description)
    ↓
SubscriptionPlanService
    ↓
INSERT SubscriptionPlans (Price:$150, Discounts: 0%/5%/15%)
    ↓
[Add Privileges] → Configure 6 privileges
    ↓
FOR EACH Privilege:
    ├─ Video Consultation: MonthlyLimit=10, UnitCost=$15
    ├─ Chat Messages: MonthlyLimit=-1 (unlimited)
    ├─ Document Upload: MonthlyLimit=20, UnitCost=$2
    ├─ Prescription Refill: MonthlyLimit=10, UnitCost=$5
    ├─ Health Report: MonthlyLimit=5, UnitCost=$10
    └─ Specialist Consult: MonthlyLimit=2, UnitCost=$50
    ↓
INSERT SubscriptionPlanPrivileges (6 records)
    ↓
[Activate Plan] → Set IsActive=1
    ↓
Plan appears in: GET /api/SubscriptionPlans/active

═══════════════════════════════════════════════════════════════════
PHASE 2: USER SUBSCRIPTION (User Side)
═══════════════════════════════════════════════════════════════════

User browses plans
    ↓
GET /api/SubscriptionPlans/active
    ↓
Shows: Basic ($50), Family ($150), Premium ($300)
    ↓
User selects: Family Care + Annual Billing
    ↓
Frontend shows:
    Price: $1,530/year (save $270!)
    Privileges: 122 consultations (10×12.17 months), 244 uploads, etc.
    ↓
User enters payment method (Stripe Elements)
    ↓
POST /api/Subscriptions
Body: { userId, planId, billingCycleId, paymentMethodId }
    ↓
SubscriptionLifecycleService.CreateSubscriptionAsync() [Line 85]
    ├─ Validate plan ✓
    ├─ Check duplicate ✓
    ├─ Validate billing cycle ✓ (BillingCycleValidator)
    ├─ Calculate CurrentPrice:
    │   Base: $150 × 12.17 = $1,825
    │   Discount: 15% = $273.75
    │   Final: $1,530
    ├─ Create Subscription (Status: PendingPayment)
    ├─ Calculate Privileges (CalculatePrivilegeAllocationAsync):
    │   Video: 10 × 12.17 = 122
    │   Uploads: 20 × 12.17 = 244
    │   Refills: 10 × 12.17 = 122
    │   ...
    └─ Create UserSubscriptionPrivilegeUsage records (6)
    ↓
PaymentService.ProcessPaymentAsync() [Line 78]
    ├─ Create BillingRecord ($1,530)
    ├─ Create SubscriptionPayment
    ├─ Stripe charge: $1,530 → Success!
    └─ UpdatePaymentRecordsAsync():
        ├─ BillingRecord → Paid
        ├─ SubscriptionPayment → Completed
        └─ Subscription → Active
    ↓
Email: "Welcome! Your subscription is active"
    ↓
User Dashboard shows: 122 consultations available

═══════════════════════════════════════════════════════════════════
PHASE 3: ACTIVE SUBSCRIPTION (Daily Usage)
═══════════════════════════════════════════════════════════════════

User opens app
    ↓
Dashboard shows current usage:
    Video: ████████░░░░░░░░░░░░ 45/122 (37%)
    Uploads: ███░░░░░░░░░░░░░░░░ 85/244 (35%)
    ↓
User books video consultation
    ↓
POST /api/Privileges/use
Body: { subscriptionId, privilegeName: "Video Consultation", amount: 1 }
    ↓
PrivilegeService.UsePrivilegeAsync() [Line 232]
    ├─ Get usage record: Allowed=122, Used=45
    ├─ Check remaining: 122 - 45 = 77 ≥ 1 ✓
    ├─ Check time limits (if any): Daily 3/day → 2 used today ✓
    ├─ Update: UsedValue = 45 + 1 = 46
    ├─ Update: LastUsedAt = Now
    └─ INSERT PrivilegeUsageHistory (audit trail)
    ↓
Return: Success ✓
    ↓
Consultation proceeds
    ↓
Dashboard updates: Video: 46/122 (38%)

... User continues using throughout the year ...

═══════════════════════════════════════════════════════════════════
PHASE 4: OVERAGE USAGE (December)
═══════════════════════════════════════════════════════════════════

Dec 15, 2025: User has used 122/122 consultations
    ↓
User tries to book 123rd consultation
    ↓
PrivilegeService.UsePrivilegeAsync()
    ├─ Check: 122 - 122 = 0 < 1
    └─ Return: false ❌
    ↓
Frontend receives failure
    ↓
Shows popup:
┌───────────────────────────────────────┐
│ ⚠️ Consultation Limit Reached         │
│                                       │
│ You've used all 122 consultations    │
│ for this period.                      │
│                                       │
│ Additional consultation: $15          │
│                                       │
│ [Cancel] [Pay & Continue]             │
└───────────────────────────────────────┘
    ↓
User clicks "Pay & Continue"
    ↓
POST /api/Billing/overage
    ↓
AutomatedBillingService.ProcessOverageChargesAsync() [Line 1667]
    ├─ Calculate overage: (123-122) × $15 = $15
    ├─ CreateOverageBillingRecordAsync():
    │   INSERT BillingRecords (Amount:$15, Type:Overage)
    └─ ProcessPaymentAsync():
        ├─ Charge via Stripe: $15
        ├─ Success! ✓
        ├─ Update BillingRecord → Paid
        └─ Allow privilege: UsedValue = 122 → 123
    ↓
Consultation proceeds
    ↓
Dashboard shows: Video: 123/122 (100%, 1 over)

═══════════════════════════════════════════════════════════════════
PHASE 5: AUTOMATED RENEWAL (Jan 1, 2026 - 2 AM)
═══════════════════════════════════════════════════════════════════

2:00 AM - AutomatedBillingService runs
    ↓
ProcessRecurringBillingAsync()
    ↓
Find: Sarah's subscription (NextBillingDate = Jan 1, 2026)
    ↓
ProcessSubscriptionBillingAsync() [Line 618]
    ↓
Calculate Total Billing:
    Base Subscription: $1,530
    Overage (3 extra consults): $45
    TOTAL: $1,575
    ↓
Create BillingRecord ($1,575, Type: Recurring)
    ↓
PaymentService.ProcessPaymentAsync()
    ├─ Create SubscriptionPayment
    ├─ Stripe charge: $1,575
    └─ Response: "succeeded" ✓
    ↓
UpdatePaymentRecordsAsync() [Transaction]:
    ├─ UPDATE BillingRecords → Paid
    ├─ UPDATE SubscriptionPayments → Completed
    ├─ UPDATE Subscriptions:
    │   LastBillingDate = '2026-01-01'
    │   NextBillingDate = '2027-01-01'  -- +365 days
    │   Status = 'Active'
    └─ ResetPrivilegesForNewBillingPeriodAsync() [Line 1197]:
        FOR EACH privilege:
            UPDATE UserSubscriptionPrivilegeUsages
            SET UsedValue = 0,              -- Reset!
                AllowedValue = 122,          -- Recalculated
                UsagePeriodStart = '2026-01-02',
                UsagePeriodEnd = '2027-01-01',
                ResetAt = '2026-01-01 02:00:15';
    ↓
Commit transaction ✓
    ↓
Email: "Subscription Renewed! $1,575 charged. Fresh 122 consultations."
    ↓
User wakes up with renewed subscription and reset privileges!

═══════════════════════════════════════════════════════════════════
PHASE 6: YEAR 2 BEGINS
═══════════════════════════════════════════════════════════════════

Jan 2, 2026: Sarah opens dashboard
    ↓
Shows:
    Video Consultations: ░░░░░░░░░░░░░░░░░░ 0/122 (0%) ← Fresh!
    Document Uploads: ░░░░░░░░░░░░░░░░░░░ 0/244 (0%) ← Reset!
    All privileges reset to zero
    ↓
Cycle repeats for Year 2...
```

---

### Detailed Service Method Mapping

**Renewal Process Methods:**

| Step | Service | Method | Line | Purpose |
|------|---------|--------|------|---------|
| 1 | AutomatedBillingService | ProcessRecurringBillingAsync() | 89 | Daily job finds due subscriptions |
| 2 | AutomatedBillingService | ProcessSubscriptionBillingAsync() | 728 | Process individual subscription |
| 3 | AutomatedBillingService | MigrateSubscriptionPricingIfNeededAsync() | 679 | Auto-correct pricing |
| 4 | AutomatedBillingService | CalculateBillingAmountAsync() | 1047 | Calculate with scaling |
| 5 | AutomatedBillingService | CalculateBillingCycleDiscount() | 1071 | Apply discount |
| 6 | AutomatedBillingService | ProcessOverageChargesAsync() | 1769 | Calculate overage |
| 7 | SubscriptionBillingService | CreateSubscriptionBillingAsync() | - | Create billing record |
| 8 | PaymentService | ProcessPaymentAsync() | 78 | Process payment |
| 9 | StripeBillingService | ProcessStripePaymentAsync() | - | Charge via Stripe |
| 10 | PaymentService | UpdatePaymentRecordsAsync() | 1125 | Update records |
| 11 | PaymentService | ResetPrivilegesForNewBillingPeriodAsync() | 1197 | Reset privileges |
| 12 | SubscriptionRepository | UpdatePrivilegeUsageAsync() | - | Save reset values |

---

### Database Changes During Renewal

**Before Renewal (Dec 31, 2025):**
```sql
-- Subscription
Id: sub-guid-123
LastBillingDate: 2025-01-01
NextBillingDate: 2026-01-01
Status: Active

-- Usage
PrivilegeName: Video Consultation
AllowedValue: 122
UsedValue: 125  -- 3 over!
UsagePeriodStart: 2025-01-01
UsagePeriodEnd: 2026-01-01
```

**After Renewal (Jan 1, 2026 - 2:05 AM):**
```sql
-- Subscription
Id: sub-guid-123
LastBillingDate: 2026-01-01  ← Updated
NextBillingDate: 2027-01-01  ← +365 days
Status: Active

-- Usage
PrivilegeName: Video Consultation
AllowedValue: 122
UsedValue: 0                 ← RESET!
UsagePeriodStart: 2026-01-02 ← New period
UsagePeriodEnd: 2027-01-01   ← Aligned with billing
ResetAt: 2026-01-01 02:00:15 ← Timestamp

-- Billing Records Created
Id: billing-guid-1, Amount: 1530.00, Type: Recurring, Status: Paid
Id: billing-guid-2, Amount: 45.00, Type: Overage, Status: Paid

-- Payment Records Created
Id: payment-guid-1, Amount: 1530.00, BillingRecordId: billing-guid-1, Status: Completed
Id: payment-guid-2, Amount: 45.00, BillingRecordId: billing-guid-2, Status: Completed
```

---

## Summary

### How Everything Works Together

**1. Plan Creation (Admin):**
- Admin creates plan with monthly price
- Adds privileges with monthly limits
- Sets billing cycle discounts
- System validates and stores configuration

**2. User Subscribes:**
- User selects plan + billing cycle
- System calculates scaled price and privileges
- Creates subscription with allocated privileges
- Processes initial payment via Stripe

**3. Privilege Management:**
- System tracks usage in real-time
- Enforces limits before each use
- Records detailed history for audit
- Allows overage with additional payment

**4. Billing Cycles:**
- Background service runs daily at 2 AM
- Finds subscriptions due for billing
- Calculates amount (base + overage + discounts)
- Processes payment automatically

**5. Renewal:**
- Payment succeeds → Update subscription dates
- Reset ALL privileges to zero
- Recalculate allowed amounts for new period
- Send confirmation email
- User gets fresh privileges

**6. Failure Handling:**
- Payment fails → Retry 3 times
- After 3 failures → Suspend subscription
- User updates payment → Retry immediately
- Success → Reactivate and reset privileges

---

### Key Formulas

**Privilege Allocation:**
```
AllowedValue = MonthlyLimit × (BillingCycleDays ÷ 30)
Result = Math.Ceiling(AllowedValue)
```

**Billing Amount:**
```
Base = MonthlyPrice × (BillingCycleDays ÷ 30)
Discount = Base × (DiscountPercent ÷ 100)
Overage = Sum of (OverLimit × UnitCost) for each privilege
Total = Base - Discount + Overage
```

**Privilege Reset Trigger:**
- Only when payment succeeds
- Part of atomic transaction
- Recalculated for new period (not copied)
- Period aligned with billing dates

---

*This document provides a complete client-friendly walkthrough of the subscription lifecycle, privilege management, and billing operations.*

**Status:** Production Ready | **Version:** 1.0 | **Date:** October 18, 2025



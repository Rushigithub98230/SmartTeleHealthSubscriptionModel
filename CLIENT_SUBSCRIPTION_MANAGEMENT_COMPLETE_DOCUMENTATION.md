# 🏥 SmartTelehealth Subscription Management System
## Complete End-to-End Technical Documentation

**Version:** 1.0  
**Date:** October 17, 2025  
**Prepared for:** Client Review  
**Status:** Production Ready ✅

---

## 📋 TABLE OF CONTENTS

1. [Executive Summary](#executive-summary)
2. [System Architecture Overview](#system-architecture-overview)
3. [Database Structure & Synchronization](#database-structure--synchronization)
4. [Complete Subscription Lifecycle](#complete-subscription-lifecycle)
5. [Billing & Payment Workflows](#billing--payment-workflows)
6. [Privilege Management](#privilege-management)
7. [Stripe Integration](#stripe-integration)
8. [All Subscription Scenarios](#all-subscription-scenarios)
9. [Admin Capabilities](#admin-capabilities)
10. [Technical Implementation Details](#technical-implementation-details)

---

## 1. EXECUTIVE SUMMARY

### System Overview

SmartTelehealth implements a **privilege-based subscription management system** that handles the complete lifecycle of healthcare subscriptions from creation to renewal/cancellation. The system integrates seamlessly with **Stripe** for payment processing while maintaining comprehensive local tracking for business intelligence and privilege management.

### Key Features ✅

- ✅ **Flexible Plan Creation** - Admins define plans with multiple privileges, usage limits, and costs
- ✅ **Automated Billing** - Recurring payments handled automatically via Stripe
- ✅ **Privilege Tracking** - Real-time tracking of included and overage usage
- ✅ **Upfront Payment for Overage** - Users must pay before exceeding limits
- ✅ **Trial Subscriptions** - Support for trial periods with automatic conversion
- ✅ **Plan Upgrades/Downgrades** - Seamless plan changes with proration
- ✅ **Failed Payment Handling** - Automatic retries with progressive suspension
- ✅ **Stripe Synchronization** - Real-time webhook integration for data consistency
- ✅ **Comprehensive Auditing** - Complete history of all status changes and usage

### Business Workflow (Client Requirements)

**Your subscription model works as follows:**

```
1. ADMIN CREATES PLAN
   └─ Defines privileges (e.g., 5 consultations @ $20 base, $25 overage)
   └─ System calculates base price: (5 × $20) + (3 × $50) + $30 commission = $280

2. USER SUBSCRIBES
   └─ Pays $280 upfront via Stripe
   └─ Gets 5 consultations + 3 medication months
   └─ Subscription becomes Active

3. USER CONSUMES SERVICES
   └─ Books consultation → Counter decrements (5→4→3→2→1→0)
   └─ Each use tracked in privilege usage history

4. USER EXCEEDS LIMITS (OVERAGE)
   └─ Tries to book 6th consultation
   └─ System blocks: "Payment Required - $25 per extra consultation"
   └─ User pays $25 immediately
   └─ Credit added, consultation allowed

5. MONTHLY RENEWAL
   └─ Stripe auto-charges $280 on billing date
   └─ System resets all privilege counters to original limits
   └─ Cycle continues
```

---

## 2. SYSTEM ARCHITECTURE OVERVIEW

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    CLIENT APPLICATIONS                           │
│         (Web Portal, Mobile App, Admin Dashboard)                │
└────────────────┬────────────────────────────────────────────────┘
                 │
                 │ HTTPS/REST API
                 ↓
┌─────────────────────────────────────────────────────────────────┐
│               BACKEND API (.NET 8 Web API)                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │           Controllers (Presentation Layer)                │  │
│  │  • SubscriptionsController                               │  │
│  │  • SubscriptionPlansController                           │  │
│  │  • BillingController                                     │  │
│  │  • PaymentController                                     │  │
│  │  • StripeWebhookController                               │  │
│  └──────────────────┬───────────────────────────────────────┘  │
│                     │                                            │
│  ┌──────────────────▼───────────────────────────────────────┐  │
│  │           Services (Business Logic Layer)                │  │
│  │  • SubscriptionService                                   │  │
│  │  • SubscriptionLifecycleService                          │  │
│  │  • SubscriptionPlanService                               │  │
│  │  • SubscriptionBillingService (Consolidated)             │  │
│  │  • PrivilegeService                                      │  │
│  │  • PaymentService                                        │  │
│  │  • StripeService                                         │  │
│  │  • AutomatedBillingService                               │  │
│  │  • SubscriptionAutomationService                         │  │
│  └──────────────────┬───────────────────────────────────────┘  │
│                     │                                            │
│  ┌──────────────────▼───────────────────────────────────────┐  │
│  │         Repositories (Data Access Layer)                 │  │
│  │  • SubscriptionRepository                                │  │
│  │  • BillingRepository                                     │  │
│  │  • PrivilegeRepository                                   │  │
│  │  • Unit of Work (Transaction Management)                 │  │
│  └──────────────────┬───────────────────────────────────────┘  │
└────────────────────┬┼───────────────────────────────────────────┘
                     ││
        ┌────────────┘└────────────┐
        │                           │
        ↓                           ↓
┌─────────────────┐        ┌──────────────────┐
│  YOUR DATABASE  │        │   STRIPE API     │
│  (SQL Server)   │        │   (External)     │
│                 │        │                  │
│ • Subscriptions │        │ • Customers      │
│ • Plans         │◄──────►│ • Subscriptions  │
│ • Billing       │  Sync  │ • Invoices       │
│ • Privileges    │        │ • Products       │
│ • Users         │        │ • Prices         │
└─────────────────┘        └──────────────────┘
                                    ↓
                              Webhooks
                                    ↓
                     ┌──────────────────────────┐
                     │  Webhook Handler         │
                     │  • Validates signature   │
                     │  • Ensures idempotency   │
                     │  • Updates database      │
                     └──────────────────────────┘
```

### Service Responsibilities (Single Responsibility Principle)

| Service | Responsibility |
|---------|----------------|
| **SubscriptionService** | Core subscription CRUD, filtering, user subscription management |
| **SubscriptionLifecycleService** | Status transitions (create, cancel, pause, resume, expire, activate) |
| **SubscriptionPlanService** | Plan creation, updates, versioning, privilege assignment |
| **SubscriptionBillingService** | All billing operations (subscription, overage, refunds, adjustments) |
| **PrivilegeService** | Privilege usage validation, consumption tracking, limit enforcement |
| **PaymentService** | Payment processing, payment methods, refunds |
| **StripeService** | Direct Stripe API calls (customers, subscriptions, invoices, products) |
| **AutomatedBillingService** | Scheduled billing, automated renewals, overdue handling |
| **SubscriptionAutomationService** | Background tasks (expirations, reminders, resets) |

---

## 3. DATABASE STRUCTURE & SYNCHRONIZATION

### Core Database Tables

#### **Subscriptions** (Main Entity)
```sql
Id                      UNIQUEIDENTIFIER PRIMARY KEY
UserId                  INT FOREIGN KEY → Users.Id
SubscriptionPlanId      UNIQUEIDENTIFIER FOREIGN KEY → SubscriptionPlans.Id
BillingCycleId          UNIQUEIDENTIFIER FOREIGN KEY → MasterBillingCycles.Id
Status                  NVARCHAR(50)  -- Active, Paused, Cancelled, etc.
StartDate               DATETIME2
EndDate                 DATETIME2
NextBillingDate         DATETIME2
CurrentPrice            DECIMAL(18,2)
StripeCustomerId        NVARCHAR(255)  -- Link to Stripe Customer
StripeSubscriptionId    NVARCHAR(255)  -- Link to Stripe Subscription
IsTrialSubscription     BIT
TrialStartDate          DATETIME2
TrialEndDate            DATETIME2
FailedPaymentAttempts   INT
LastPaymentFailedDate   DATETIME2
CancelledDate           DATETIME2
CancellationReason      NVARCHAR(MAX)
```

#### **SubscriptionPlans** (Plan Definitions)
```sql
Id                      UNIQUEIDENTIFIER PRIMARY KEY
Name                    NVARCHAR(200)
Description             NVARCHAR(MAX)
Price                   DECIMAL(18,2)
BillingCycleId          UNIQUEIDENTIFIER
IsActive                BIT
IsAutoCalculatedPrice   BIT
AdminCommissionPercent  DECIMAL(5,2)
AdminCommissionFixed    DECIMAL(18,2)
PrivilegesTotalCost     DECIMAL(18,2)
StripeProductId         NVARCHAR(255)  -- Link to Stripe Product
StripeMonthlyPriceId    NVARCHAR(255)  -- Link to Stripe Price (Monthly)
StripeQuarterlyPriceId  NVARCHAR(255)  -- Link to Stripe Price (Quarterly)
StripeAnnualPriceId     NVARCHAR(255)  -- Link to Stripe Price (Annual)
VersionNumber           INT
IsLatestVersion         BIT
ParentPlanId            UNIQUEIDENTIFIER  -- For versioning
```

#### **SubscriptionPlanPrivileges** (Plan-Privilege Configuration)
```sql
Id                      UNIQUEIDENTIFIER PRIMARY KEY
SubscriptionPlanId      UNIQUEIDENTIFIER FK → SubscriptionPlans.Id
PrivilegeId             UNIQUEIDENTIFIER FK → Privileges.Id
Value                   INT  -- Quantity included in plan (e.g., 5 consultations)
PrivilegeBaseCost       DECIMAL(18,2)  -- Used for PLAN PRICE calculation
UnitCost                DECIMAL(18,2)  -- Used for OVERAGE billing
DailyLimit              INT
WeeklyLimit             INT
MonthlyLimit            INT
```

#### **UserSubscriptionPrivilegeUsage** (Usage Tracking)
```sql
Id                              UNIQUEIDENTIFIER PRIMARY KEY
SubscriptionId                  UNIQUEIDENTIFIER FK → Subscriptions.Id
PrivilegeId                     UNIQUEIDENTIFIER FK → Privileges.Id
SubscriptionPlanPrivilegeId     UNIQUEIDENTIFIER FK
AllocatedLimit                  INT  -- Total allowed (e.g., 5)
UsedCount                       INT  -- How many used (e.g., 3)
RemainingLimit                  INT  -- How many left (e.g., 2)
LastUsedDate                    DATETIME2
LastResetDate                   DATETIME2
NextResetDate                   DATETIME2
```

#### **PrivilegeUsageHistory** (Audit Trail)
```sql
Id                      UNIQUEIDENTIFIER PRIMARY KEY
UserId                  INT
SubscriptionId          UNIQUEIDENTIFIER
PrivilegeId             UNIQUEIDENTIFIER
UsageDate               DATETIME2
QuantityUsed            INT
RemainingAfterUse       INT
UsageType               NVARCHAR(50)  -- "Included" or "Overage"
Cost                    DECIMAL(18,2)  -- $0 for included, $X for overage
RelatedEntityId         NVARCHAR(255)  -- Link to consultation, appointment, etc.
Notes                   NVARCHAR(MAX)
```

#### **BillingRecords** (All Billing Events)
```sql
Id                      UNIQUEIDENTIFIER PRIMARY KEY
UserId                  INT
SubscriptionId          UNIQUEIDENTIFIER
Type                    NVARCHAR(50)  -- Subscription, Overage, Consultation, etc.
Status                  NVARCHAR(50)  -- Pending, Paid, Failed, Refunded
Amount                  DECIMAL(18,2)
TotalAmount             DECIMAL(18,2)
TaxAmount               DECIMAL(18,2)
DiscountAmount          DECIMAL(18,2)
BillingDate             DATETIME2
DueDate                 DATETIME2
PaidDate                DATETIME2
InvoiceNumber           NVARCHAR(100)
StripeInvoiceId         NVARCHAR(255)  -- Link to Stripe Invoice
StripePaymentIntentId   NVARCHAR(255)  -- Link to Stripe PaymentIntent
Description             NVARCHAR(MAX)
```

#### **SubscriptionPayments** (Payment Records)
```sql
Id                      UNIQUEIDENTIFIER PRIMARY KEY
SubscriptionId          UNIQUEIDENTIFIER
BillingRecordId         UNIQUEIDENTIFIER
Amount                  DECIMAL(18,2)
PaymentMethod           NVARCHAR(50)
Status                  NVARCHAR(50)
TransactionId           NVARCHAR(255)
PaymentDate             DATETIME2
```

#### **SubscriptionStatusHistory** (Status Change Tracking)
```sql
Id                      UNIQUEIDENTIFIER PRIMARY KEY
SubscriptionId          UNIQUEIDENTIFIER
OldStatus               NVARCHAR(50)
NewStatus               NVARCHAR(50)
ChangedDate             DATETIME2
Reason                  NVARCHAR(MAX)
ChangedBy               INT  -- UserId or System
```

### Stripe Database (External - Managed by Stripe)

```
Customers
├─ Id: cus_XYZ789
├─ Email: user@example.com
└─ Metadata: { userId: "123", platform: "SmartTelehealth" }

Products
├─ Id: prod_ABC123
├─ Name: "Basic Health Plan"
├─ Active: true
└─ Metadata: { planId: "f3a1b2c3-..." }

Prices
├─ Id: price_1Month_XYZ
├─ Product: prod_ABC123
├─ Amount: 27500 (in cents = $275.00)
└─ Recurring: { interval: "month", interval_count: 1 }

Subscriptions
├─ Id: sub_stripe_AAA
├─ Customer: cus_XYZ789
├─ Price: price_1Month_XYZ
├─ Status: active
├─ Current_period_start: 2025-10-17
├─ Current_period_end: 2025-11-17
└─ Metadata: { subscriptionId: "sub_111", planId: "f3a1b2c3-..." }

Invoices
├─ Id: in_stripe_BBB
├─ Customer: cus_XYZ789
├─ Subscription: sub_stripe_AAA
├─ Amount_due: 27500
├─ Status: paid
└─ Metadata: { billingRecordId: "bill_001" }
```

### Data Synchronization Strategy

#### **Push (Your System → Stripe)**
```
When admin creates plan:
  YOUR DB: SubscriptionPlan created
  ↓
  STRIPE: Product created → Returns prod_ABC123
  ↓
  YOUR DB: Update SubscriptionPlan.StripeProductId = prod_ABC123

When user subscribes:
  YOUR DB: Subscription created (Status: Pending)
  ↓
  STRIPE: Customer created (if not exists) → Returns cus_XYZ789
  ↓
  YOUR DB: Update User.StripeCustomerId = cus_XYZ789
  ↓
  STRIPE: Subscription created → Returns sub_stripe_AAA
  ↓
  YOUR DB: Update Subscription.StripeSubscriptionId = sub_stripe_AAA
```

#### **Pull (Stripe → Your System via Webhooks)**
```
Stripe event occurs (e.g., payment succeeded):
  STRIPE: Sends webhook POST to your endpoint
  ↓
  YOUR SYSTEM: Validates webhook signature (security)
  ↓
  YOUR SYSTEM: Checks idempotency (prevents duplicates)
  ↓
  YOUR SYSTEM: Processes event (update subscription status, create billing record)
  ↓
  YOUR DB: Updated with Stripe data
  ↓
  YOUR SYSTEM: Returns 200 OK to Stripe
```

---

## 4. COMPLETE SUBSCRIPTION LIFECYCLE

### State Diagram

```
┌──────────┐
│ PENDING  │ ← Subscription created, awaiting payment
└────┬─────┘
     │ Payment succeeds
     ↓
┌────────────┐
│ TRIAL_     │ ← If plan has trial period
│ ACTIVE     │
└────┬───────┘
     │ Trial ends + payment succeeds
     ↓
┌──────────┐
│  ACTIVE  │ ← Fully active subscription
└────┬─────┘
     │
     ├─→ User pauses ──→ ┌─────────┐
     │                   │ PAUSED  │ ──→ User resumes ──┐
     │                   └─────────┘                    │
     │                                                   ↓
     ├─→ Payment fails ─→ ┌──────────────┐      ┌──────────┐
     │                    │ PAYMENT_     │      │  ACTIVE  │
     │                    │ FAILED       │──────┤          │
     │                    └──────────────┘      └──────────┘
     │                     (Retry attempts)
     │
     ├─→ Too many failures → ┌────────────┐
     │                        │ SUSPENDED  │
     │                        └────────────┘
     │
     ├─→ User cancels ────→ ┌───────────┐
     │                       │ CANCELLED │
     │                       └───────────┘
     │
     └─→ Billing date passes without renewal → ┌─────────┐
                                                 │ EXPIRED │
                                                 └─────────┘
```

### Detailed Lifecycle Workflows

---



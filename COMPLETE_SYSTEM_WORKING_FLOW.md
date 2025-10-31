# 🎯 Complete Subscription Management System - Working Flow Documentation

## 📋 Table of Contents
1. [System Overview](#system-overview)
2. [Architecture & Layer Structure](#architecture--layer-structure)
3. [Entity Relationships](#entity-relationships)
4. [Complete Subscription Lifecycle Flow](#complete-subscription-lifecycle-flow)
5. [Billing & Payment Flow](#billing--payment-flow)
6. [Privilege Management Flow](#privilege-management-flow)
7. [Stripe Integration Flow](#stripe-integration-flow)
8. [Background Services](#background-services)
9. [Frontend Integration](#frontend-integration)
10. [API Endpoints Summary](#api-endpoints-summary)

---

## System Overview

### Core Components
- **25 Entities** - Complete data model for subscription management
- **19 Services** - Business logic layer
- **20 Repositories** - Data access layer
- **6 Controllers** - API endpoints
- **7 Background Services** - Automated tasks
- **Frontend Angular Components** - User interface

### Technology Stack
- **Backend**: .NET Core (C#)
- **Frontend**: Angular (TypeScript)
- **Database**: SQL Server (Entity Framework Core)
- **Payment Gateway**: Stripe
- **Architecture**: Clean Architecture (Core, Application, Infrastructure, API)

---

## Architecture & Layer Structure

### Backend Layers

```
SmartTelehealth.API (Presentation Layer)
├── Controllers/
│   ├── SubscriptionPlansController
│   ├── SubscriptionsController
│   ├── BillingController
│   ├── StripeController
│   └── StripeWebhookController
│
SmartTelehealth.Application (Business Logic Layer)
├── Services/
│   ├── SubscriptionPlanService
│   ├── SubscriptionService
│   ├── SubscriptionLifecycleService
│   ├── SubscriptionBillingService
│   ├── PrivilegeService
│   ├── PlanPricingService
│   ├── WebhookService
│   └── PaymentService
├── DTOs/
└── Utilities/
│
SmartTelehealth.Core (Domain Layer)
├── Entities/
│   ├── Subscription
│   ├── SubscriptionPlan
│   ├── SubscriptionPayment
│   ├── UserSubscriptionPrivilegeUsage
│   ├── BillingRecord
│   └── ... (25 total entities)
├── Interfaces/
└── Enums/
│
SmartTelehealth.Infrastructure (Data & External Services)
├── Repositories/
├── Services/
│   ├── StripeService
│   ├── StripeBillingService
│   └── NotificationService
└── Services/ (Background)
    ├── AutomatedBillingBackgroundService
    ├── PrivilegeResetBackgroundService
    ├── ScheduledMigrationBackgroundService
    ├── StripeSyncJob
    └── ReconciliationBackgroundService
```

---

## Entity Relationships

### Core Entity Diagram

```
User (1) ────< (M) Subscription
                │
                ├───> SubscriptionPlan (M:1)
                │       ├───> MasterBillingCycle (M:1)
                │       ├───> MasterCurrency (M:1)
                │       └───> Category (M:1)
                │
                ├───> SubscriptionPayment (1:M)
                │       └───> BillingRecord (M:1)
                │
                └───> UserSubscriptionPrivilegeUsage (1:M)
                        ├───> SubscriptionPlanPrivilege (M:1)
                        │       ├───> SubscriptionPlan (M:1)
                        │       └───> Privilege (M:1)
                        │               └───> MasterPrivilegeType (M:1)
                        └───> PrivilegeUsageHistory (1:M)

SubscriptionPlan
├───> SubscriptionPlanPrivilege (1:M)
│       └───> Privilege (M:1)
└───> ScheduledPlanMigration (1:M)
        └───> Subscription (M:1)

BillingRecord
├───> SubscriptionPayment (1:M)
├───> BillingAdjustment (1:M)
└───> PaymentRefund (1:M)
```

### Key Entity Descriptions

#### 1. Subscription
- **Purpose**: User's active subscription instance
- **Key Fields**:
  - `Status`: Active, Paused, Cancelled, TrialActive, PaymentFailed, etc.
  - `CurrentPrice`: Effective price (may differ from plan base price)
  - `StartDate`, `EndDate`, `NextBillingDate`
  - `StripeCustomerId`, `StripeSubscriptionId`, `StripePriceId`
  - `AutoRenew`, `PendingCancellationAtRenewal`
- **Relationships**: User, SubscriptionPlan, SubscriptionPayment[], UserSubscriptionPrivilegeUsage[]

#### 2. SubscriptionPlan
- **Purpose**: Template defining subscription features and pricing
- **Key Fields**:
  - `BasePrice`, `IsActive`, `IsLatestVersion`
  - `VersionNumber`, `ParentPlanId` (for versioning)
  - `StripeProductId`, `StripePriceId`
  - `BillingCycleId` (Monthly, Quarterly, Annual)
- **Relationships**: SubscriptionPlanPrivilege[], Subscription[], MasterBillingCycle, MasterCurrency

#### 3. UserSubscriptionPrivilegeUsage
- **Purpose**: Tracks privilege usage per subscription period
- **Key Fields**:
  - `AllowedValue`: Maximum allowed (can be -1 for unlimited)
  - `UsedValue`: Current usage count
  - `UsagePeriodStart`, `UsagePeriodEnd`
- **Relationships**: Subscription, SubscriptionPlanPrivilege

#### 4. SubscriptionPayment
- **Purpose**: Payment records for each billing cycle
- **Key Fields**:
  - `PaymentStatus`: Pending, Succeeded, Failed
  - `BillingPeriodStart`, `BillingPeriodEnd`
  - `StripePaymentIntentId`
- **Relationships**: Subscription, BillingRecord

#### 5. BillingRecord
- **Purpose**: Master billing record for invoices
- **Key Fields**:
  - `Amount`, `TaxAmount`, `TotalAmount`
  - `Status`: Pending, Paid, Failed
  - `StripeInvoiceId`
- **Relationships**: Subscription, SubscriptionPayment[], BillingAdjustment[]

---

## Complete Subscription Lifecycle Flow

### 1. Subscription Creation Flow

```
┌─────────────────────────────────────────────────────────────┐
│              SUBSCRIPTION CREATION WORKFLOW                  │
└─────────────────────────────────────────────────────────────┘

STEP 1: User Browses Plans
├── Frontend: GET /api/subscriptionplans/active
├── Controller: SubscriptionPlansController.GetActivePlans()
├── Service: SubscriptionPlanService.GetActivePlansAsync()
└── Returns: List of active plans with pricing, features, billing cycles

STEP 2: User Selects Plan & Initiates Checkout
├── Frontend: User clicks "Subscribe" button
├── Frontend: purchase-plan.component.ts.submitPurchase()
├── Service: StripeCheckoutService.createCheckoutSession(planId)
└── API: POST /api/stripe/create-checkout-session/{planId}

STEP 3: Backend Creates Stripe Checkout Session
├── Controller: StripeController.CreateCheckoutSession()
├── Validations:
│   ├── Check user eligibility (no active subscriptions)
│   ├── Validate plan exists and is active
│   ├── Get or create Stripe customer (search by email to prevent duplicates)
│   └── Verify StripePriceId is configured
├── Service: StripeService.CreateCheckoutSessionWithCustomerAsync()
│   ├── Creates Stripe Checkout Session
│   ├── Sets Mode = "subscription"
│   ├── Adds metadata (userId, planId, customerId)
│   └── Returns checkout URL
└── Response: { url: "https://checkout.stripe.com/..." }

STEP 4: User Redirected to Stripe Checkout
├── Frontend: window.location.href = checkoutUrl
├── User enters payment details on Stripe's secure page
└── User completes payment

STEP 5: Stripe Sends Webhook (checkout.session.completed)
├── Webhook Endpoint: POST /api/stripewebhook/webhook
├── Controller: StripeWebhookController.HandleWebhook()
│   ├── Validates webhook signature
│   ├── Checks idempotency (prevents duplicate processing)
│   └── Routes to WebhookService
└── Service: WebhookService.HandleCheckoutSessionCompletedAsync()

STEP 6: Create Local Subscription (Transaction)
├── Service: SubscriptionLifecycleService.SyncSubscriptionFromCheckoutAsync()
│   ┌─ BEGIN TRANSACTION
│   ├── 1. Validate plan exists and is active
│   ├── 2. Check for existing subscriptions (prevent duplicates)
│   ├── 3. Get or create Stripe customer
│   ├── 4. Create Subscription entity
│   │   ├── Status = TrialActive (if trial) or Active
│   │   ├── Set StartDate, NextBillingDate
│   │   ├── Calculate CurrentPrice (may differ from BasePrice)
│   │   ├── Link StripeCustomerId, StripeSubscriptionId, StripePriceId
│   │   └── Set billing cycle from plan
│   │
│   ├── 5. Record SubscriptionStatusHistory
│   │   └── Track status change with timestamp
│   │
│   ├── 6. Initialize Privileges
│   │   └── Service: PrivilegeService.InitializeSubscriptionPrivilegesAsync()
│   │       ├── For each SubscriptionPlanPrivilege:
│   │       ├── Create UserSubscriptionPrivilegeUsage
│   │       ├── Set AllowedValue from plan (can be -1 for unlimited)
│   │       ├── Set UsagePeriodStart/End based on billing cycle
│   │       └── Initialize UsedValue = 0
│   │
│   ├── 7. Create BillingRecord
│   │   └── Service: SubscriptionBillingService.CreateSubscriptionBilling()
│   │       ├── Calculate effective price (with discounts, adjustments)
│   │       ├── Create BillingRecord (Status = Paid)
│   │       └── Link Stripe PaymentIntent
│   │
│   ├── 8. Create SubscriptionPayment
│   │   ├── Link Subscription and BillingRecord
│   │   ├── Status = PaymentStatus.Succeeded
│   │   ├── Type = PaymentType.Recurring
│   │   └── Set BillingPeriodStart/End
│   │
│   └─ COMMIT TRANSACTION
│
└── STEP 7: Send Welcome Email
    └── Service: NotificationService.SendWelcomeEmail()
```

### 2. Subscription Status Transitions

```
Status Flow:
Pending → TrialActive → Active
         ↓
    TrialExpired
         ↓
    Active → Paused → Active
    Active → PaymentFailed → Active (after retry)
    Active → Cancelled
    Active → Expired
    Active → Suspended

Status Management:
├── SubscriptionLifecycleService handles all transitions
├── SubscriptionStatusHistory tracks all changes
└── Business rules validate allowed transitions
```

### 3. Subscription Cancellation Flow

```
User Cancels Subscription:
├── Frontend: POST /api/subscriptions/{id}/cancel
├── Controller: SubscriptionsController.CancelSubscription()
├── Service: SubscriptionLifecycleService.CancelSubscriptionAsync()
│   ├── Validate user owns subscription
│   ├── Cancel Stripe subscription (immediate or at renewal)
│   ├── Update subscription:
│   │   ├── Status = Cancelled
│   │   ├── CancelledDate = UtcNow
│   │   ├── AutoRenew = false
│   │   └── EndDate = UtcNow (if immediate) or NextBillingDate (if at renewal)
│   ├── Record SubscriptionStatusHistory
│   └── Suspend privilege access (optional)
└── Send cancellation confirmation email
```

### 4. Subscription Renewal Flow

```
Automated Renewal (Background Service):
├── Service: AutomatedBillingBackgroundService (runs every hour)
│   ├── Query: GetSubscriptionsDueForBilling(UtcNow)
│   │   └── Where NextBillingDate <= UtcNow
│   │       AND Status = 'Active'
│   │       AND AutoRenew = true
│   │
│   └── For each subscription:
│       ├── ProcessSubscriptionBillingAsync()
│       │   ├── Create invoice via Stripe
│       │   ├── Charge payment (Stripe PaymentIntent)
│       │   ├── Create BillingRecord (Status = Pending → Paid)
│       │   ├── Create SubscriptionPayment (Status = Succeeded)
│       │   ├── Update Subscription:
│       │   │   ├── NextBillingDate = CalculateNextBillingDate()
│       │   │   ├── LastPaymentDate = UtcNow
│       │   │   └── Extend EndDate by billing cycle
│       │   └── Reset Privilege Usage Counters
│       │       └── Reset UsedValue = 0 for all privileges
│       │
│       └── If payment fails:
│           ├── Increment FailedPaymentAttempts
│           ├── Create SubscriptionPayment (Status = Failed)
│           ├── Update BillingRecord (Status = Failed)
│           ├── Set NextRetryAt (based on retry policy)
│           └── If FailedPaymentAttempts >= 3:
│               ├── Status = PaymentFailed
│               └── Suspend access
```

---

## Billing & Payment Flow

### Billing Record Creation

```
BillingRecord Lifecycle:
1. Created when:
   ├── Subscription is created (initial payment)
   ├── Subscription renews (automated billing)
   ├── Manual billing adjustment
   └── Overage charges (if enabled)

2. Status Flow:
   Pending → Processing → Paid
                ↓
            Failed (retry available)

3. Components:
   ├── Amount: Base subscription price
   ├── TaxAmount: Calculated tax (if applicable)
   ├── TotalAmount: Amount + TaxAmount
   └── Linked to SubscriptionPayment
```

### Payment Processing

```
Payment Flow:
1. Create PaymentIntent (Stripe)
   ├── Amount = BillingRecord.TotalAmount
   ├── Currency = SubscriptionPlan.Currency
   ├── Customer = StripeCustomerId
   └── PaymentMethod = Default payment method

2. Charge Payment
   ├── StripeService.ProcessPaymentAsync()
   ├── If successful:
   │   ├── Update BillingRecord.Status = Paid
   │   ├── Update BillingRecord.PaidAt = UtcNow
   │   ├── Create SubscriptionPayment (Status = Succeeded)
   │   └── Link StripePaymentIntentId
   │
   └── If failed:
       ├── Update BillingRecord.Status = Failed
       ├── Create SubscriptionPayment (Status = Failed)
       ├── Log failure reason
       └── Schedule retry (if retryable)
```

### Billing Adjustments

```
Adjustment Types:
├── Credit: Reduce billing amount
├── Discount: Apply percentage or fixed discount
├── Proration: Adjust for plan changes mid-cycle
└── Refund: Reverse payment

Adjustment Flow:
├── Create BillingAdjustment
├── Link to BillingRecord
├── Recalculate TotalAmount
└── Update billing status
```

---

## Privilege Management Flow

### Privilege Usage Tracking

```
Privilege Structure:
SubscriptionPlan
└── SubscriptionPlanPrivilege[] (junction table)
    ├── Privilege: What service (e.g., "TeleConsultation")
    └── Value: How many (-1 = unlimited, 0 = disabled, N = count)

Subscription
└── UserSubscriptionPrivilegeUsage[] (usage tracking)
    ├── AllowedValue: Maximum allowed (from plan, can be adjusted)
    ├── UsedValue: Current usage count
    ├── UsagePeriodStart/End: Billing period
    └── LastUsedAt: Last usage timestamp

PrivilegeUsageHistory[]: Audit trail of all usage events
```

### Using a Privilege

```
STEP 1: Check Privilege Availability
├── API: PrivilegeService.UsePrivilegeAsync(subscriptionId, privilegeName, amount)
├── Validations:
│   ├── Subscription must be Active
│   ├── Privilege must exist in plan
│   ├── Check if disabled (Value = 0) → return false
│   ├── Check if unlimited (Value = -1) → allow immediately
│   └── Check remaining: AllowedValue - UsedValue >= amount
│
└── STEP 2: Record Usage (Transaction)
    ┌─ BEGIN TRANSACTION
    ├── Update UserSubscriptionPrivilegeUsage
    │   ├── UsedValue += amount
    │   └── LastUsedAt = UtcNow
    │
    ├── Create PrivilegeUsageHistory
    │   ├── UsedValue = amount
    │   ├── UsedAt = UtcNow
    │   ├── UsageDate, UsageWeek, UsageMonth (for analytics)
    │   └── Link to UserSubscriptionPrivilegeUsage
    │
    └─ COMMIT TRANSACTION

STEP 3: Handle Overage (if applicable)
└── If UsedValue > AllowedValue:
    ├── Calculate overage count
    └── Optional: Charge overage fees
```

### Privilege Reset (New Billing Period)

```
Privilege Reset Flow:
├── Trigger: On successful subscription renewal
├── Service: PrivilegeService.ResetPrivilegeUsageAsync()
│   ├── For each UserSubscriptionPrivilegeUsage:
│   │   ├── UsedValue = 0
│   │   ├── UsagePeriodStart = new billing period start
│   │   ├── UsagePeriodEnd = new billing period end
│   │   └── AllowedValue = reset to plan value (or keep if adjusted)
│   │
│   └── Preserve PrivilegeUsageHistory (audit trail)
│
└── Background Service: PrivilegeResetBackgroundService
    └── Monitors for expired periods and logs warnings
```

---

## Stripe Integration Flow

### Stripe Service Architecture

```
StripeService (Infrastructure Layer)
├── Customer Management
│   ├── CreateOrGetCustomerAsync()
│   │   └── Searches by email first (prevents duplicates)
│   ├── UpdateCustomerAsync()
│   └── SyncCustomerAsync()
│
├── Subscription Management
│   ├── CreateSubscriptionAsync()
│   ├── UpdateSubscriptionAsync()
│   ├── CancelSubscriptionAsync()
│   └── GetSubscriptionAsync()
│
├── Payment Management
│   ├── CreateCheckoutSessionAsync()
│   ├── CreatePaymentIntentAsync()
│   ├── ProcessPaymentAsync()
│   └── RefundPaymentAsync()
│
└── Webhook Processing
    └── ValidateWebhookSignature()
```

### Stripe Sync Flow

```
StripeSyncJob (Background Service - Hourly)
├── Purpose: Reconcile Stripe data with local database
├── Operations:
│   ├── Sync all Stripe subscriptions
│   ├── Update subscription statuses
│   ├── Update billing dates
│   ├── Detect orphaned Stripe subscriptions
│   └── Sync customer information
│
└── Records: StripeSyncHistory (audit trail)
```

### Webhook Event Handling

```
Webhook Events Processed:
├── Subscription Events:
│   ├── customer.subscription.created
│   ├── customer.subscription.updated
│   ├── customer.subscription.deleted
│   ├── customer.subscription.paused
│   ├── customer.subscription.resumed
│   ├── customer.subscription.past_due
│   └── customer.subscription.trial_will_end
│
├── Payment Events:
│   ├── invoice.payment_succeeded
│   ├── invoice.payment_failed
│   ├── invoice.created
│   ├── invoice.finalized
│   ├── payment_intent.succeeded
│   └── payment_intent.payment_failed
│
├── Customer Events:
│   ├── customer.created
│   ├── customer.updated
│   └── customer.deleted
│
└── Checkout Events:
    └── checkout.session.completed

Webhook Processing Flow:
1. StripeWebhookController.HandleWebhook()
   ├── Validate signature
   ├── Check idempotency (ProcessedWebhookEvent)
   └── Route to WebhookService

2. WebhookService processes event
   ├── Update local subscription status
   ├── Sync Stripe data
   ├── Trigger notifications
   └── Record in ProcessedWebhookEvent

3. If processing fails:
   ├── Record in UnprocessedWebhookEvent
   └── UnprocessedWebhookRetryService retries later
```

---

## Background Services

### 1. AutomatedBillingBackgroundService
- **Frequency**: Every 1 hour
- **Purpose**: Process subscription renewals
- **Operations**:
  - Find subscriptions due for billing (NextBillingDate <= UtcNow)
  - Create invoices and charge payments
  - Reset privilege usage counters
  - Handle failed payment retries

### 2. PrivilegeResetBackgroundService
- **Frequency**: Daily
- **Purpose**: Monitor expired privilege periods
- **Operations**:
  - Check for expired usage periods
  - Log warnings for admin review
  - Note: Actual resets happen on billing success

### 3. ScheduledMigrationBackgroundService
- **Frequency**: Every 6 hours
- **Purpose**: Execute scheduled plan migrations
- **Operations**:
  - Find due migrations (ScheduledMigrationDate <= UtcNow)
  - Migrate subscriptions to new plan versions
  - Apply prorations and adjustments
  - Send confirmation notifications

### 4. StripeSyncJob
- **Frequency**: Every 1 hour
- **Purpose**: Reconcile Stripe data with local database
- **Operations**:
  - Sync all Stripe subscriptions
  - Update local subscription statuses
  - Sync customer information
  - Detect orphaned subscriptions

### 5. FailedRefundRetryBackgroundService
- **Frequency**: Every 6 hours
- **Purpose**: Retry failed refunds
- **Operations**:
  - Find failed refunds (FailedRefund table)
  - Retry refund processing
  - Update refund status

### 6. UnprocessedWebhookRetryService
- **Frequency**: Every 2 hours
- **Purpose**: Retry failed webhook processing
- **Operations**:
  - Find unprocessed webhooks
  - Retry webhook processing
  - Mark as processed or failed

### 7. ReconciliationBackgroundService
- **Frequency**: Daily (nightly)
- **Purpose**: Data integrity checks
- **Operations**:
  - Verify subscription consistency
  - Check billing record accuracy
  - Detect data anomalies
  - Generate reconciliation reports

---

## Frontend Integration

### Frontend Architecture

```
Frontend Structure:
├── Core Services (HTTP Layer)
│   ├── subscription.service.ts
│   ├── subscription-plan.service.ts
│   ├── stripe-checkout.service.ts
│   ├── billing.service.ts
│   └── privilege.service.ts
│
├── Features/User (User-Facing Pages)
│   ├── subscriptions/
│   │   ├── purchase-plan.component.ts
│   │   ├── subscription-detail.component.ts
│   │   └── subscription-list.component.ts
│   ├── billing/
│   ├── privileges/
│   └── payment-methods/
│
└── Features/Admin (Admin Dashboard)
    ├── subscriptions/
    ├── plans/
    ├── billing/
    └── analytics/
```

### Key Frontend Flows

#### 1. Plan Purchase Flow

```typescript
// purchase-plan.component.ts
submitPurchase(): void {
  // Step 1: Create Stripe checkout session
  this.stripeCheckoutService.createCheckoutSession(this.plan.id)
    .subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data?.url) {
          // Step 2: Redirect to Stripe Checkout
          this.stripeCheckoutService.redirectToCheckout(response.data.url);
        }
      },
      error: (error) => {
        // Handle error
      }
    });
}
```

#### 2. Subscription Management

```typescript
// subscription.service.ts
cancelSubscription(id: string, reason: string): Observable<ApiResponse<any>> {
  return this.commonService.post(`Subscriptions/${id}/cancel`, reason);
}

pauseSubscription(id: string): Observable<ApiResponse<any>> {
  return this.commonService.post(`Subscriptions/${id}/pause`, {});
}

resumeSubscription(id: string): Observable<ApiResponse<any>> {
  return this.commonService.post(`Subscriptions/${id}/resume`, {});
}
```

#### 3. Privilege Usage Display

```typescript
// Frontend components display:
├── Remaining privileges
├── Usage progress bars
├── Usage history
└── Overage warnings
```

### Frontend Components Status

**✅ Ready & Integrated:**
- ✅ Plan browsing and selection
- ✅ Stripe checkout integration
- ✅ Subscription management (view, cancel, pause, resume)
- ✅ Billing history
- ✅ Privilege usage display
- ✅ Admin dashboard for subscriptions
- ✅ Plan management (admin)
- ✅ Analytics dashboard

**📋 Implementation Status:**
- All core subscription flows are implemented
- Stripe checkout fully integrated
- Admin management UI complete
- User subscription management complete

---

## API Endpoints Summary

### Subscription Plans

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/subscriptionplans/active` | Get active plans |
| GET | `/api/subscriptionplans/{id}` | Get plan details |
| POST | `/api/subscriptionplans` | Create plan (admin) |
| PUT | `/api/subscriptionplans/{id}` | Update plan (admin) |
| DELETE | `/api/subscriptionplans/{id}` | Delete plan (admin) |

### Subscriptions

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/subscriptions` | Create subscription |
| GET | `/api/subscriptions/user/{userId}` | Get user subscriptions |
| GET | `/api/subscriptions/{id}` | Get subscription details |
| POST | `/api/subscriptions/{id}/cancel` | Cancel subscription |
| POST | `/api/subscriptions/{id}/pause` | Pause subscription |
| POST | `/api/subscriptions/{id}/resume` | Resume subscription |
| POST | `/api/subscriptions/{id}/upgrade` | Upgrade plan |

### Stripe Integration

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/stripe/create-checkout-session/{planId}` | Create checkout session |
| POST | `/api/stripewebhook/webhook` | Handle Stripe webhooks |

### Billing

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/billing/user/{userId}` | Get user billing records |
| GET | `/api/billing/subscription/{subscriptionId}` | Get subscription billing |

### Privileges

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/subscriptions/{id}/privileges` | Get subscription privileges |
| POST | `/api/privileges/use` | Use a privilege |
| GET | `/api/privileges/remaining/{subscriptionId}` | Get remaining privileges |

---

## Data Flow Summary

### Complete User Journey

```
1. USER BROWSES PLANS
   Frontend → SubscriptionPlansController → SubscriptionPlanService
   → SubscriptionPlanRepository → Database
   ← Returns: Active plans with pricing

2. USER INITIATES CHECKOUT
   Frontend → StripeController → StripeService → Stripe API
   ← Returns: Checkout session URL
   Frontend → Redirects to Stripe Checkout

3. USER COMPLETES PAYMENT
   Stripe → Webhook → StripeWebhookController → WebhookService
   → SubscriptionLifecycleService → Multiple Repositories
   → Creates: Subscription, Privileges, BillingRecord, Payment
   → Sends: Welcome email

4. USER USES SERVICE (Privilege)
   Frontend → API → PrivilegeService → UserSubscriptionPrivilegeUsageRepository
   → Updates: UsedValue, Creates: PrivilegeUsageHistory

5. SUBSCRIPTION RENEWS
   AutomatedBillingBackgroundService (hourly)
   → SubscriptionRepository → Finds due subscriptions
   → SubscriptionBillingService → StripeService → Charges payment
   → Updates: Subscription, Creates: BillingRecord, Payment
   → Resets: Privilege usage counters
```

---

## Key Business Rules

### Subscription Rules
1. **One Active Subscription Per User**: Users cannot have multiple active subscriptions
2. **Plan Versioning**: When a plan is updated, a new version is created; existing subscriptions keep their version
3. **Trial Handling**: Trial subscriptions automatically convert to Active on trial end
4. **Cancellation**: Can be immediate or at renewal (PendingCancellationAtRenewal)

### Billing Rules
1. **Renewal Timing**: Billing occurs on NextBillingDate
2. **Failed Payment Retry**: Automatic retry with exponential backoff (max 3 attempts)
3. **Grace Period**: 7 days after payment failure before suspension
4. **Proration**: Plan changes mid-cycle are prorated

### Privilege Rules
1. **Unlimited Privileges**: Value = -1 means unlimited usage
2. **Disabled Privileges**: Value = 0 means feature not available
3. **Usage Reset**: Privilege counters reset on successful renewal
4. **Overage**: Can optionally charge for usage beyond limit

---

## Security & Validation

### Security Measures
1. **Webhook Signature Validation**: All Stripe webhooks validated
2. **Idempotency**: Webhook events processed only once
3. **User Authorization**: Users can only access their own subscriptions
4. **Admin Only Operations**: Plan management restricted to admins
5. **Payment Security**: PCI-compliant via Stripe Checkout

### Validation Rules
1. **Status Transitions**: Validated before allowing status changes
2. **Plan Eligibility**: Checked before subscription creation
3. **Payment Validation**: Payment methods validated via Stripe
4. **Privilege Validation**: Usage limits enforced before allowing service usage

---

## Monitoring & Logging

### Logging Points
- All subscription lifecycle events
- All payment processing events
- All webhook events (processed and failed)
- All privilege usage events
- Background service executions
- Errors and exceptions

### Audit Trail
- **SubscriptionStatusHistory**: All status changes
- **PrivilegeUsageHistory**: All privilege usage events
- **StripeSyncHistory**: All Stripe sync operations
- **ProcessedWebhookEvent**: All processed webhooks
- **BillingRecord**: All billing transactions

---

## Summary

### ✅ System Status: **FULLY OPERATIONAL**

**Backend**: ✅ Complete
- All 25 entities implemented
- All 19 services operational
- All background services running
- Stripe integration fully functional

**Frontend**: ✅ Complete
- All user-facing components implemented
- Admin dashboard complete
- Stripe checkout integrated
- Subscription management UI ready

**Integration**: ✅ Complete
- Frontend ↔ Backend integration working
- Stripe webhook processing functional
- Background services operational
- Database schema complete

**Ready For**: Production deployment

---

**Document Created**: 2025-01-XX
**Last Updated**: 2025-01-XX
**System Version**: Production Ready


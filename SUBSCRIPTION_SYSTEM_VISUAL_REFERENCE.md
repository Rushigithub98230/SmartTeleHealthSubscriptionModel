# Subscription Management System - Visual Quick Reference

## System Architecture at a Glance

```
┌─────────────────────────────────────────────────────────────────────┐
│                     CLIENT APPLICATION (React)                       │
│                  Frontend makes API calls to backend                 │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             │ HTTPS API Calls
                             │
┌────────────────────────────▼────────────────────────────────────────┐
│                        API CONTROLLERS                               │
├─────────────────────────────────────────────────────────────────────┤
│  • SubscriptionsController    → Subscription CRUD & Lifecycle       │
│  • BillingController          → Billing & Invoices                  │
│  • PaymentController          → Payment Processing                  │
│  • StripeWebhookController    → Stripe Event Handling               │
│  • SubscriptionPlansController → Plan Management                     │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             │ Service Layer Calls
                             │
┌────────────────────────────▼────────────────────────────────────────┐
│                        SERVICE LAYER                                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │           SUBSCRIPTION SERVICES                            │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │ SubscriptionService              │ Get, Filter, Query      │   │
│  │ SubscriptionLifecycleService     │ Create, Cancel, Pause   │   │
│  │ SubscriptionPlanService          │ Plan CRUD & Management  │   │
│  │ SubscriptionAutomationService    │ Automated Jobs          │   │
│  │ SubscriptionNotificationService  │ Email Notifications     │   │
│  └────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │            BILLING & PAYMENT SERVICES                      │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │ BillingService                   │ Billing Records         │   │
│  │ PaymentService                   │ Payment Processing      │   │
│  │ AutomatedBillingService          │ Recurring Billing       │   │
│  │ PrivilegeBasedBillingService     │ Overage Billing         │   │
│  └────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │           PRIVILEGE SERVICES                               │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │ PrivilegeService                 │ Usage Validation        │   │
│  │                                  │ Limit Enforcement       │   │
│  └────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │          STRIPE INTEGRATION SERVICES                       │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │ StripeService                    │ All Stripe API Calls    │   │
│  │ StripeSynchronizationService     │ Sync Operations         │   │
│  │ WebhookIdempotencyService        │ Webhook Deduplication   │   │
│  └────────────────────────────────────────────────────────────┘   │
│                                                                      │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             │ Repository Calls
                             │
┌────────────────────────────▼────────────────────────────────────────┐
│                     REPOSITORY LAYER                                 │
├─────────────────────────────────────────────────────────────────────┤
│  • SubscriptionRepository                                           │
│  • SubscriptionPlanRepository                                       │
│  • BillingRepository                                                │
│  • PrivilegeRepository                                              │
│  • UserSubscriptionPrivilegeUsageRepository                         │
│  • SubscriptionStatusHistoryRepository                              │
│  • ProcessedWebhookEventRepository                                  │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             │ Entity Framework Core
                             │
┌────────────────────────────▼────────────────────────────────────────┐
│                       SQL SERVER DATABASE                            │
├─────────────────────────────────────────────────────────────────────┤
│  Tables:                                                             │
│  • Users                    • Subscriptions                          │
│  • SubscriptionPlans        • BillingRecords                         │
│  • SubscriptionPayments     • Privileges                             │
│  • SubscriptionPlanPrivileges                                        │
│  • UserSubscriptionPrivilegeUsage                                    │
│  • SubscriptionStatusHistory                                         │
│  • ProcessedWebhookEvents   • MasterBillingCycles                    │
└──────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                      EXTERNAL INTEGRATIONS                           │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────────────┐              ┌──────────────────────┐   │
│  │   STRIPE API         │              │   EMAIL SERVICE      │   │
│  ├──────────────────────┤              ├──────────────────────┤   │
│  │ • Customers          │              │ • Welcome Emails     │   │
│  │ • Subscriptions      │              │ • Billing Reminders  │   │
│  │ • Products & Prices  │              │ • Payment Alerts     │   │
│  │ • Payments           │              │ • Trial Warnings     │   │
│  │ • Invoices           │              └──────────────────────┘   │
│  │ • Webhooks           │                                          │
│  └──────────────────────┘                                          │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Entity Relationship Map

```
┌──────────────┐
│     USER     │ (1 user has many subscriptions)
│──────────────│
│ Id           │
│ Email        │◄─────────────┐
│ StripeCustomer│             │
│ Id           │              │
└──────┬───────┘              │
       │                      │
       │ 1:N                  │ N:1
       │                      │
       ▼                      │
┌──────────────────────────┐  │  ┌────────────────────┐
│     SUBSCRIPTION         │──┘  │ SUBSCRIPTION PLAN  │
│──────────────────────────│     │────────────────────│
│ Id                       │◄────┤ Id                 │
│ UserId                   │     │ Name               │
│ SubscriptionPlanId       │     │ Price              │
│ Status                   │     │ StripeProductId    │
│ StripeSubscriptionId     │     │ BillingCycleId     │
│ StartDate                │     │ IsTrialAllowed     │
│ NextBillingDate          │     └────────┬───────────┘
│ CurrentPrice             │              │
└─────┬────────────┬───────┘              │ 1:N
      │            │                      │
      │ 1:N        │ 1:N                 ▼
      │            │              ┌──────────────────────────┐
      ▼            │              │ SUBSCRIPTION PLAN        │
┌─────────────┐   │              │ PRIVILEGE                │
│  BILLING    │   │              │──────────────────────────│
│  RECORD     │   │              │ Id                       │
│─────────────│   │              │ SubscriptionPlanId       │
│ Id          │   │              │ PrivilegeId              │
│ Subscription│   │              │ Value (-1=unlimited)     │
│ Id          │   │              │ DailyLimit               │
│ Amount      │   │              │ WeeklyLimit              │
│ Status      │   │              │ MonthlyLimit             │
│ StripeInvoice│  │              │ UnitCost (overage)       │
│ Id          │   │              └─────────┬────────────────┘
└─────────────┘   │                        │
                  │                        │ 1:N
                  ▼                        │
         ┌──────────────────┐             ▼
         │ SUBSCRIPTION     │  ┌──────────────────────────┐
         │ PAYMENT          │  │ USER SUBSCRIPTION        │
         │──────────────────│  │ PRIVILEGE USAGE          │
         │ Id               │  │──────────────────────────│
         │ SubscriptionId   │  │ Id                       │
         │ Amount           │  │ SubscriptionId           │
         │ Status           │  │ PrivilegeId              │
         │ StripePayment    │  │ AllowedValue             │
         │ IntentId         │  │ UsedValue                │
         └──────────────────┘  │ RemainingValue           │
                               │ UsagePeriodStart         │
                               │ UsagePeriodEnd           │
                               └──────────────────────────┘

┌─────────────────────────────────────────────┐
│        PRIVILEGE (Master Data)              │
│─────────────────────────────────────────────│
│ Id                                          │
│ Name (e.g., "Teleconsultation")            │
│ Description                                 │
│ PrivilegeTypeId                             │
└─────────────────────────────────────────────┘
```

---

## Subscription Lifecycle States

```
                         NEW USER
                             │
                             ↓
                  ┌──────────────────┐
                  │  Create Account  │
                  └────────┬─────────┘
                           │
                           ↓
                  ┌──────────────────┐
                  │ Select Plan      │
                  │ Add Payment      │
                  └────────┬─────────┘
                           │
                           ↓
        ┌──────────────────┴──────────────────┐
        │                                     │
        ↓                                     ↓
┌───────────────┐                    ┌───────────────┐
│ TRIAL ACTIVE  │                    │    ACTIVE     │
│ (if allowed)  │                    │  (immediate)  │
└───────┬───────┘                    └───┬───────────┘
        │                                │
        │ Trial Ends                     │
        ↓                                │
┌───────────────┐                        │
│ TRIAL EXPIRED │                        │
└───────┬───────┘                        │
        │                                │
        │ Convert to Paid                │
        └────────────┬───────────────────┘
                     │
                     ↓
            ┌────────────────┐
            │    ACTIVE      │◄────┐
            │  Subscription  │     │
            └────┬─────┬─────┘     │
                 │     │            │
        Pause    │     │  Resume    │
                 │     │            │
                 ↓     │            │
            ┌────────────────┐     │
            │    PAUSED      │─────┘
            └────────────────┘
                     │
        Cancel       │
                     ↓
            ┌────────────────┐
            │   CANCELLED    │
            └────────────────┘
                     │
        Reactivate   │
                     └──────► ACTIVE

            PAYMENT FAILURE FLOW:
            ┌────────────────┐
            │    ACTIVE      │
            └────────┬───────┘
                     │
        Payment Fails│
                     ↓
            ┌────────────────┐
            │ PAYMENT FAILED │
            └────┬─────┬─────┘
                 │     │
        Payment  │     │ Too many
        Success  │     │ failures
                 │     │
                 ↓     ↓
            ACTIVE   SUSPENDED
```

---

## Payment Processing Flow

```
┌──────────────────────────────────────────────────────────┐
│                   PAYMENT FLOW                            │
└──────────────────────────────────────────────────────────┘

USER ACTION:
  │
  ├─► Subscribe to Plan
  │
  ↓
CREATE SUBSCRIPTION
  │
  ├─► 1. Validate Plan
  ├─► 2. Create/Get Stripe Customer
  ├─► 3. Validate Payment Method
  ├─► 4. Create Stripe Subscription
  ├─► 5. Create Local Subscription
  ├─► 6. Create Initial Billing Record
  │
  ↓
BILLING RECORD CREATED
  │   Status: Pending
  │   Amount: $99.00
  │   DueDate: Today
  │
  ↓
PROCESS PAYMENT
  │
  ├─► 1. Create Stripe Payment Intent
  ├─► 2. Confirm Payment Intent
  │
  ↓
┌─────────┴─────────┐
│                   │
▼                   ▼
SUCCESS           FAILURE
│                   │
├─► Update:        ├─► Update:
│   Status = Paid  │   Status = Failed
│   PaidAt = Now   │   FailureReason
│                   │   Attempts++
├─► Update Sub:    │
│   LastBilling    ├─► Retry Logic:
│   NextBilling    │   Attempt 1: +1 hour
│   Attempts = 0   │   Attempt 2: +1 day
│                   │   Attempt 3: +3 days
├─► Send Email:    │
│   Payment        ├─► Send Email:
│   Confirmation   │   Payment Failed
│                   │
│                   ├─► If 3 failures:
│                   │   Sub Status = PaymentFailed
│                   │   Send Suspension Warning
│                   │
└───────────────────┴──────────────────────────────────────┘
```

---

## Privilege Usage Flow

```
┌──────────────────────────────────────────────────────────┐
│              PRIVILEGE USAGE VALIDATION                   │
└──────────────────────────────────────────────────────────┘

USER REQUESTS SERVICE (e.g., Teleconsultation)
  │
  ↓
CHECK SUBSCRIPTION STATUS
  ├─► Active? ✓
  ├─► Not Paused? ✓
  ├─► Not Expired? ✓
  │
  ↓
GET PLAN PRIVILEGE CONFIGURATION
  │
  ├─► Privilege: "Teleconsultation"
  ├─► Value: 5 (5 consultations/month)
  ├─► DailyLimit: 1
  ├─► WeeklyLimit: 2
  ├─► MonthlyLimit: 5
  ├─► UnitCost: $50 (overage)
  │
  ↓
CHECK IF DISABLED
  │
  ├─► Value = 0? → DENY ACCESS
  │
  ↓
CHECK IF UNLIMITED
  │
  ├─► Value = -1? → ALLOW (skip all limit checks)
  │
  ↓
CHECK TIME-BASED LIMITS
  │
  ├─► Daily Usage: 0/1 ✓ (Can use)
  ├─► Weekly Usage: 1/2 ✓ (Can use)
  ├─► Monthly Usage: 3/5 ✓ (Can use)
  │
  ↓
GET CURRENT USAGE
  │
  ├─► AllowedValue: 5
  ├─► UsedValue: 3
  ├─► RemainingValue: 2
  │
  ↓
CHECK QUANTITY LIMIT
  │
  ├─► Remaining (2) >= Requested (1)? ✓
  │
  ↓
✓ ALLOW ACCESS
  │
  ├─► Increment UsedValue: 3 → 4
  ├─► Update LastUsedAt: Now
  ├─► Create Usage History Record
  │
  ↓
PROCEED WITH SERVICE

┌─────────────────────────────────────────────┐
│           OVERAGE SCENARIO                  │
├─────────────────────────────────────────────┤
│ If user exceeds limit:                      │
│   AllowedValue: 5                           │
│   UsedValue: 6 (exceeded by 1)              │
│                                             │
│ → Create Billing Record:                   │
│   Type: Overage                             │
│   Amount: 1 × $50 = $50                     │
│   Description: "Teleconsultation overage"   │
│                                             │
│ → Process Payment                           │
│ → Send Overage Notification                │
└─────────────────────────────────────────────┘
```

---

## Stripe Webhook Integration

```
┌──────────────────────────────────────────────────────────┐
│                 STRIPE WEBHOOK FLOW                       │
└──────────────────────────────────────────────────────────┘

STRIPE EVENT OCCURS
  │
  ├─► customer.subscription.created
  ├─► customer.subscription.updated
  ├─► customer.subscription.deleted
  ├─► invoice.payment_succeeded
  ├─► invoice.payment_failed
  ├─► payment_intent.succeeded
  │
  ↓
STRIPE SENDS WEBHOOK → https://api.smarttelehealth.com/api/stripewebhook/webhook
  │
  ↓
RECEIVE WEBHOOK
  │
  ├─► 1. Read Request Body
  ├─► 2. Get Stripe-Signature Header
  │
  ↓
VERIFY SIGNATURE
  │
  ├─► Use Webhook Secret
  ├─► EventUtility.ConstructEvent()
  │
  ├─► Valid? → Continue
  ├─► Invalid? → Return 400 Bad Request
  │
  ↓
CHECK IDEMPOTENCY
  │
  ├─► Query ProcessedWebhookEvent by EventId
  │
  ├─► Already Processed? → Return 200 OK (skip)
  ├─► Failed 3+ times? → Return 200 OK (skip)
  ├─► New Event? → Continue
  │
  ↓
CREATE PROCESSING RECORD
  │
  ├─► EventId: evt_xxx
  ├─► Status: "Processing"
  ├─► ReceivedAt: Now
  ├─► Attempts: 1
  │
  ↓
PROCESS EVENT (with retry logic)
  │
  ├─► Switch on event.Type
  │
  ├─► invoice.payment_succeeded:
  │   ├─► Get BillingRecord by StripeInvoiceId
  │   ├─► Update Status = Paid
  │   ├─► Update Subscription LastBillingDate
  │   ├─► Send Payment Confirmation
  │
  ├─► invoice.payment_failed:
  │   ├─► Get BillingRecord by StripeInvoiceId
  │   ├─► Update Status = Failed
  │   ├─► Update Subscription FailedAttempts++
  │   ├─► If attempts >= 3: Status = PaymentFailed
  │   ├─► Send Payment Failure Alert
  │
  ├─► customer.subscription.updated:
  │   ├─► Get Subscription by StripeSubscriptionId
  │   ├─► Update Status from Stripe
  │   ├─► Update CurrentPrice from Stripe
  │   ├─► Update NextBillingDate
  │   ├─► Create Status History Entry
  │
  ├─► customer.subscription.deleted:
  │   ├─► Get Subscription by StripeSubscriptionId
  │   ├─► Update Status = Cancelled
  │   ├─► Set CancelledDate = Now
  │   ├─► Send Cancellation Notification
  │
  ↓
┌─────────┴─────────┐
│                   │
▼                   ▼
SUCCESS           FAILURE
│                   │
├─► Update Record: ├─► Update Record:
│   Status =       │   Status = Failed
│   "Processed"    │   ErrorMessage
│   ProcessedAt    │   Attempts++
│   Duration       │
│                   ├─► If attempts < 3:
├─► Return 200 OK  │   Schedule Retry
│                   │
│                   ├─► Return 500 Error
└───────────────────┴──────────────────────────────────────┘
```

---

## Automated Jobs

```
┌──────────────────────────────────────────────────────────┐
│           SUBSCRIPTION AUTOMATION SERVICE                 │
│              (Background Jobs / Scheduled Tasks)          │
└──────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────┐
│  JOB 1: Process Subscription Renewals          │
│  Schedule: Daily at 2:00 AM                    │
├────────────────────────────────────────────────┤
│  1. Get subscriptions where:                   │
│     NextBillingDate <= Today                   │
│     Status = Active                            │
│                                                │
│  2. For each subscription:                     │
│     ├─► Create billing record                  │
│     ├─► Process payment                        │
│     ├─► Update NextBillingDate                 │
│     ├─► Send renewal confirmation              │
│                                                │
│  3. Handle failures:                           │
│     ├─► Schedule retry                         │
│     ├─► Update failure count                   │
│     ├─► Send failure notification              │
└────────────────────────────────────────────────┘

┌────────────────────────────────────────────────┐
│  JOB 2: Process Trial Expirations              │
│  Schedule: Daily at 3:00 AM                    │
├────────────────────────────────────────────────┤
│  1. Get subscriptions where:                   │
│     Status = TrialActive                       │
│     TrialEndDate <= Today                      │
│                                                │
│  2. For each trial:                            │
│     ├─► Update Status = TrialExpired           │
│     ├─► Create status history                  │
│     ├─► Send conversion reminder               │
│                                                │
│  3. Send early warnings:                       │
│     ├─► 7 days before expiration               │
│     ├─► 3 days before expiration               │
│     ├─► 1 day before expiration                │
└────────────────────────────────────────────────┘

┌────────────────────────────────────────────────┐
│  JOB 3: Process Failed Payments                │
│  Schedule: Every 6 hours                       │
├────────────────────────────────────────────────┤
│  1. Get billing records where:                 │
│     Status = Failed                            │
│     NextRetryAt <= Now                         │
│     AttemptCount < 3                           │
│                                                │
│  2. For each failed payment:                   │
│     ├─► Retry payment processing               │
│     ├─► Update attempt count                   │
│     ├─► Calculate next retry time              │
│     ├─► Send retry notification                │
│                                                │
│  3. If final retry fails:                      │
│     ├─► Update subscription status             │
│     ├─► Send suspension warning                │
└────────────────────────────────────────────────┘

┌────────────────────────────────────────────────┐
│  JOB 4: Send Billing Reminders                 │
│  Schedule: Daily at 9:00 AM                    │
├────────────────────────────────────────────────┤
│  1. Get subscriptions where:                   │
│     NextBillingDate = Today + 3 days           │
│     Status = Active                            │
│                                                │
│  2. For each subscription:                     │
│     ├─► Send billing reminder email            │
│     ├─► Include amount, date, plan details     │
└────────────────────────────────────────────────┘

┌────────────────────────────────────────────────┐
│  JOB 5: Reset Privilege Counters               │
│  Schedule: Daily at 12:00 AM                   │
├────────────────────────────────────────────────┤
│  1. Get privilege usages where:                │
│     UsagePeriodEnd <= Today                    │
│                                                │
│  2. For each usage:                            │
│     ├─► Reset UsedValue = 0                    │
│     ├─► Update UsagePeriodStart = Today        │
│     ├─► Update UsagePeriodEnd = Calculated     │
│     ├─► Set ResetAt = Now                      │
└────────────────────────────────────────────────┘
```

---

## Key Workflows Summary

### 1. CREATE SUBSCRIPTION
```
User → Select Plan → Enter Payment → 
  Validate → Create Stripe Customer → 
  Create Stripe Subscription → 
  Create Local Subscription → 
  Initialize Privileges → 
  Send Welcome Email → Done
```

### 2. CANCEL SUBSCRIPTION
```
User → Request Cancel → 
  Validate Can Cancel → 
  Cancel in Stripe → 
  Update Local Status → 
  Create Status History → 
  Send Confirmation → Done
```

### 3. PROCESS PAYMENT
```
Billing Due → Create Billing Record → 
  Create Payment Intent in Stripe → 
  Confirm Payment → 
  Update Billing Status → 
  Update Subscription → 
  Send Receipt → Done
```

### 4. USE PRIVILEGE
```
Request Service → Get Subscription → 
  Validate Status → Get Privilege Config → 
  Check Limits (Time & Quantity) → 
  Increment Usage → 
  Create History → 
  Check Overage → 
  Allow/Deny Access
```

### 5. WEBHOOK PROCESSING
```
Stripe Event → Receive Webhook → 
  Verify Signature → 
  Check Idempotency → 
  Process Event → 
  Update Database → 
  Send Notifications → 
  Mark as Processed → Done
```

---

## Database Tables Quick Reference

| Table Name | Purpose | Key Columns |
|------------|---------|-------------|
| **Users** | User accounts | Id, Email, StripeCustomerId |
| **SubscriptionPlans** | Plan templates | Id, Name, Price, StripeProductId |
| **Subscriptions** | User subscriptions | Id, UserId, PlanId, Status, StripeSubscriptionId |
| **BillingRecords** | All billing transactions | Id, UserId, SubscriptionId, Amount, Status |
| **SubscriptionPayments** | Subscription-specific payments | Id, SubscriptionId, Amount, Status, StripePaymentIntentId |
| **Privileges** | Available services | Id, Name, Description |
| **SubscriptionPlanPrivileges** | Privileges per plan | Id, PlanId, PrivilegeId, Value, Limits, UnitCost |
| **UserSubscriptionPrivilegeUsage** | User privilege consumption | Id, SubscriptionId, UsedValue, AllowedValue |
| **SubscriptionStatusHistory** | Status change audit trail | Id, SubscriptionId, FromStatus, ToStatus, ChangedAt |
| **ProcessedWebhookEvents** | Webhook idempotency | EventId, Status, ProcessedAt, Attempts |

---

## API Endpoints Quick Reference

### Subscription Endpoints
```
GET    /api/subscriptions/{id}                  - Get subscription by ID
GET    /api/subscriptions/user/{userId}         - Get user subscriptions
POST   /api/subscriptions                       - Create subscription
POST   /api/subscriptions/{id}/cancel           - Cancel subscription
POST   /api/subscriptions/{id}/pause            - Pause subscription
POST   /api/subscriptions/{id}/resume           - Resume subscription
POST   /api/subscriptions/{id}/upgrade          - Upgrade subscription
POST   /api/subscriptions/{id}/reactivate       - Reactivate subscription
```

### Billing Endpoints
```
GET    /api/billing/{id}                        - Get billing record
GET    /api/billing/user/{userId}               - Get user billing history
POST   /api/billing                             - Create billing record
POST   /api/billing/{id}/process-payment        - Process payment
POST   /api/billing/{id}/retry                  - Retry failed payment
```

### Plan Endpoints
```
GET    /api/subscriptionplans                   - Get all plans
GET    /api/subscriptionplans/{id}              - Get plan by ID
POST   /api/subscriptionplans                   - Create plan (admin)
PUT    /api/subscriptionplans/{id}              - Update plan (admin)
DELETE /api/subscriptionplans/{id}              - Delete plan (admin)
```

### Webhook Endpoints
```
POST   /api/stripewebhook/webhook               - Handle Stripe webhooks
```

---

**End of Visual Reference**



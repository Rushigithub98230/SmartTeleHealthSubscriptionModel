# Subscription Management System - Flow Diagrams

## 1. Complete Subscription Creation Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                    USER SUBSCRIPTION CREATION FLOW                    │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────┐
│    USER     │
│  Browses    │
│   Plans     │
└──────┬──────┘
       │
       │ GET /api/subscriptionplans/active
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  SubscriptionPlansController.GetActivePlans()                     │
│  - Returns active plans with pricing and features                 │
│  - Includes billing cycle comparison                              │
└──────────────────────────────────────────────────────────────────┘
       │
       │ User selects plan
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  POST /api/Checkout/create-session/{planId}                       │
│  StripeController.CreateCheckoutSession()                         │
│  - Creates Stripe Checkout Session                                │
│  - Returns checkout URL                                           │
└──────────────────────────────────────────────────────────────────┘
       │
       │ User redirected to Stripe
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  USER COMPLETES PAYMENT ON STRIPE                                 │
└──────────────────────────────────────────────────────────────────┘
       │
       │ Stripe webhook: checkout.session.completed
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  StripeWebhookController.HandleWebhook()                          │
│  - Validates webhook signature                                    │
│  - Checks idempotency                                             │
│  - Routes to appropriate handler                                  │
└──────────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  WebhookService.ProcessCheckoutCompletedAsync()                   │
│  - Extracts userId, planId, stripeSubscriptionId                  │
│  - Calls SubscriptionLifecycleService                             │
└──────────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  SubscriptionLifecycleService.SyncSubscriptionFromCheckoutAsync() │
│                                                                   │
│  ┌─ BEGIN TRANSACTION                                            │
│  │                                                                │
│  │  1. Validate plan exists and is active                         │
│  │  2. Check for existing active subscriptions                    │
│  │  3. Get or create Stripe customer                             │
│  │  4. Create local Subscription entity                          │
│  │     - Set status (TrialActive or Active)                      │
│  │     - Set StartDate, NextBillingDate                          │
│  │     - Set CurrentPrice from plan                              │
│  │     - Link Stripe IDs                                         │
│  │                                                                │
│  │  5. Record SubscriptionStatusHistory                          │
│  │                                                                │
│  │  6. Initialize Privileges                                     │
│  │     PrivilegeService.InitializeSubscriptionPrivileges()       │
│  │     For each PlanPrivilege:                                   │
│  │       - Create UserSubscriptionPrivilegeUsage record          │
│  │       - Set AllowedValue from plan                            │
│  │       - Set UsagePeriodStart/End from billing cycle           │
│  │                                                                │
│  │  7. Create BillingRecord                                      │
│  │     SubscriptionBillingService.CreateSubscriptionBilling()    │
│  │     - Calculate effective price                               │
│  │     - Create BillingRecord with Status=Paid                   │
│  │     - Link to Stripe PaymentIntent                            │
│  │                                                                │
│  │  8. Create SubscriptionPayment                                │
│  │     - Link Subscription, BillingRecord                        │
│  │     - Set PaymentStatus=Succeeded                             │
│  │     - Set billing period dates                                │
│  │                                                                │
│  └─ COMMIT TRANSACTION                                           │
│                                                                │
│  9. Send Welcome Email                                           │
│     NotificationService.SendWelcomeEmail()                       │
└──────────────────────────────────────────────────────────────────┘
       │
       │ Success
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  USER RECEIVES CONFIRMATION                                       │
│  - Subscription activated                                         │
│  - Privileges allocated                                           │
│  - Access granted                                                 │
└──────────────────────────────────────────────────────────────────┘
```

---

## 2. Automated Billing Cycle Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│              AUTOMATED BILLING BACKGROUND SERVICE                    │
│                    (Runs Every Hour)                                 │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│  AutomatedBillingBackgroundService.ExecuteAsync()                 │
│  while (!cancellationToken.IsCancellationRequested)               │
│    - ProcessBillingCycleAsync()                                   │
│    - Wait 1 hour                                                  │
└──────────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  ProcessBillingCycleAsync()                                       │
│                                                                   │
│  1. ProcessDueSubscriptionsAsync()                                │
│  2. ProcessFailedPaymentRetriesAsync()                            │
│  3. ResetUsageCountersAsync()                                     │
└──────────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  ProcessDueSubscriptionsAsync()                                   │
│                                                                   │
│  Query: GetSubscriptionsDueForBilling(UtcNow)                    │
│  - Where NextBillingDate <= UtcNow                               │
│  - And Status = 'Active'                                          │
│  - And AutoRenew = true                                           │
└──────────────────────────────────────────────────────────────────┘
       │
       │ For each subscription
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  ProcessSubscriptionBillingAsync(subscription)                    │
│                                                                   │
│  ┌─ BEGIN TRANSACTION                                            │
│  │                                                                │
│  │  1. Check if Stripe subscription exists                       │
│  │     - If no StripeSubscriptionId, skip                        │
│  │                                                                │
│  │  2. Create invoice via Stripe                                 │
│  │     StripeService.CreateInvoice()                             │
│  │     StripeService.PayInvoice()                                │
│  │                                                                │
│  │  3. Create BillingRecord                                      │
│  │     - Type = BillingType.Subscription                         │
│  │     - Status = BillingStatus.Pending                          │
│  │     - Amount = subscription.CurrentPrice                      │
│  │     - Calculate tax                                            │
│  │     - TotalAmount = Amount + TaxAmount                        │
│  │                                                                │
│  │  4. Process Payment                                           │
│  │     PaymentService.ProcessPayment()                           │
│  │     - Charge via Stripe PaymentIntent                         │
│  │     - Update BillingRecord.Status = Paid                      │
│  │     - Update BillingRecord.PaidAt = UtcNow                    │
│  │                                                                │
│  │  5. Create SubscriptionPayment                                │
│  │     - Link to Subscription and BillingRecord                  │
│  │     - Status = PaymentStatus.Succeeded                        │
│  │     - Type = PaymentType.Recurring                            │
│  │     - Set BillingPeriodStart/End                             │
│  │                                                                │
│  │  6. Update Subscription                                       │
│  │     - NextBillingDate = next billing cycle                    │
│  │     - LastPaymentDate = UtcNow                                │
│  │                                                                │
│  └─ COMMIT TRANSACTION                                           │
│                                                                │
│  7. Reset Privilege Usage Counters                               │
│     PrivilegeResetHelper.ResetUsageCounters()                    │
│     For each UserSubscriptionPrivilegeUsage:                     │
│       - UsedValue = 0                                            │
│       - UsagePeriodStart = now                                   │
│       - UsagePeriodEnd = next billing cycle                      │
│                                                                │
│  8. Send Renewal Confirmation Email                              │
│     NotificationService.SendRenewalConfirmation()                │
└──────────────────────────────────────────────────────────────────┘
       │
       │ If payment failed
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  Handle Failed Payment                                            │
│                                                                   │
│  1. Increment subscription.FailedPaymentAttempts                 │
│  2. Create SubscriptionPayment with Status=Failed                │
│  3. Update BillingRecord.Status = Failed                         │
│  4. Set NextRetryAt (based on retry policy)                      │
│                                                                   │
│  If FailedPaymentAttempts >= 3:                                  │
│    - Update Subscription.Status = PaymentFailed                  │
│    - Send Payment Failed Notification                            │
│    - Suspend access to services                                  │
│                                                                   │
│  Otherwise:                                                       │
│    - Schedule retry                                               │
└──────────────────────────────────────────────────────────────────┘
```

---

## 3. Privilege Usage Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                    PRIVILEGE USAGE FLOW                               │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────┐
│    USER     │
│  Requests   │
│  Service    │
│ (e.g.,      │
│Consultation)│
└──────┬──────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  API Endpoint (e.g., POST /api/consultations)                    │
│  - User authenticated                                             │
│  - Request validated                                              │
└──────────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  PrivilegeService.UsePrivilegeAsync()                            │
│                                                                   │
│  1. Get active subscription for user                             │
│     subscription = GetActiveSubscriptionByUserId(userId)         │
│                                                                │
│  2. Get plan privilege configuration                             │
│     planPrivilege = GetPlanPrivilege(subscriptionId, privilege)  │
│                                                                │
│  3. Check if privilege is disabled (Value = 0)                  │
│     If yes → return false                                        │
│                                                                │
│  4. Check if privilege is unlimited (Value = -1)                │
│     If yes:                                                      │
│       - Log usage                                                │
│       - return true                                              │
│                                                                │
│  5. Get current usage                                            │
│     usage = GetPrivilegeUsage(subscriptionId, privilege)         │
│                                                                │
│  6. Check remaining usage                                        │
│     remaining = usage.AllowedValue - usage.UsedValue            │
│     If remaining < requested amount → return false              │
│                                                                │
│  7. Update usage record                                          │
│     ┌─ BEGIN TRANSACTION                                        │
│     │  - usage.UsedValue += amount                              │
│     │  - usage.LastUsedAt = UtcNow                              │
│     │  - Update UserSubscriptionPrivilegeUsage                  │
│     │                                                            │
│     │  - Create PrivilegeUsageHistory                           │
│     │    - UsedValue = amount                                   │
│     │    - UsedAt = UtcNow                                      │
│     │    - UsageDate, UsageWeek, UsageMonth                     │
│     └─ COMMIT TRANSACTION                                       │
│                                                                │
│  8. Check for overage                                            │
│     If usage.UsedValue > usage.AllowedValue:                    │
│       - Calculate overage count                                 │
│       - Optional: Charge overage                                │
│                                                                 │
│  9. return true                                                  │
└──────────────────────────────────────────────────────────────────┘
       │
       │ Success
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  Service Proceeds                                                 │
│  - Create consultation appointment                                │
│  - Send confirmations                                             │
│  - Grant access                                                   │
└──────────────────────────────────────────────────────────────────┘
```

---

## 4. Plan Versioning & Migration Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                 PLAN VERSIONING WORKFLOW                              │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────┐
│   ADMIN UPDATES  │
│      PLAN        │
└────────┬─────────┘
         │
         │ PUT /api/subscriptionplans/{id}
         ▼
┌──────────────────────────────────────────────────────────────────┐
│  PlanVersioningService.CreateNewPlanVersionAsync()                │
│                                                                   │
│  1. Get existing plan                                            │
│     existingPlan = GetSubscriptionPlan(oldPlanId)                │
│                                                                │
│  2. Check for active subscriptions                              │
│     activeSubs = GetActiveSubscriptionsCount(oldPlanId)          │
│                                                                │
│  3. Determine parent plan                                        │
│     parentPlanId = existingPlan.ParentPlanId ?? existingPlan.Id │
│                                                                │
│  4. Calculate new version number                                 │
│     allVersions = GetAllVersionsOfPlan(parentPlanId)             │
│     newVersionNumber = max(version) + 1                          │
│                                                                │
│  5. Create new version entity                                    │
│     newVersion = {                                               │
│       Name: updateDto.Name,                                      │
│       BasePrice: updateDto.BasePrice,                            │
│       PlanPrivileges: [],                                        │
│       VersionNumber: newVersionNumber,                           │
│       ParentPlanId: parentPlanId,                                │
│       IsLatestVersion: true                                      │
│     }                                                             │
│                                                                │
│  6. Copy privileges from old to new version                      │
│     For each oldPlan.PlanPrivileges:                             │
│       - Create SubscriptionPlanPrivilege                         │
│       - Link to new plan                                         │
│                                                                │
│  7. Create Stripe resources                                      │
│     - Create Stripe Product                                      │
│     - Create Stripe Price                                        │
│     - Link to plan                                               │
│                                                                │
│  8. Calculate auto price if enabled                              │
│     basePrice = CalculatePlanPrice(newVersionId)                 │
│                                                                │
│  9. Mark old version as not latest                               │
│     oldVersion.IsLatestVersion = false                           │
│                                                                │
│  10. Save both versions                                          │
│                                                                │
│  11. If activeSubs > 0:                                          │
│      ScheduleMigrationsForActiveSubscribers()                    │
└──────────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  ScheduleMigrationsForActiveSubscribers()                         │
│                                                                   │
│  For each active subscription on old plan:                        │
│                                                                   │
│    1. Calculate notification date                                │
│       notificationDate = UtcNow                                  │
│                                                                   │
│    2. Calculate migration date (user's next renewal)             │
│       migrationDate = subscription.NextBillingDate              │
│                                                                   │
│    3. Create ScheduledPlanMigration                              │
│       migration = {                                               │
│         SubscriptionId: subscription.Id,                         │
│         FromPlanId: oldPlanId,                                   │
│         ToPlanId: newPlanId,                                     │
│         NotificationDate: notificationDate,                      │
│         ScheduledMigrationDate: migrationDate,                   │
│         Status: 'Pending'                                         │
│       }                                                           │
│                                                                   │
│    4. Send notification to user                                  │
│       "Your plan will change to ${newPlanName} on ${date}"       │
│       "Effective price: ${newPrice}"                             │
│       "You can accept or cancel"                                 │
│                                                                   │
│    5. Save migration record                                      │
└──────────────────────────────────────────────────────────────────┘
       │
       │ Later: User's renewal date arrives
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  ScheduledMigrationBackgroundService                              │
│  (Runs periodically)                                             │
│                                                                   │
│  1. Query due migrations                                         │
│     migrations = GetMigrationsWhere(                             │
│       ScheduledMigrationDate <= UtcNow                           │
│       AND Status = 'Pending'                                     │
│     )                                                             │
│                                                                   │
│  2. For each migration:                                          │
│                                                                   │
│     ┌─ BEGIN TRANSACTION                                        │
│     │                                                            │
│     │  a. Get subscription and plans                            │
│     │  b. Calculate price difference                            │
│     │                                                           │
│     │  c. Update subscription                                   │
│     │     subscription.SubscriptionPlanId = newPlanId           │
│     │     subscription.CurrentPrice = newPlanPrice              │
│     │                                                            │
│     │  d. If price increased:                                   │
│     │     - Create BillingAdjustment                            │
│     │     - Adjust next invoice                                 │
│     │                                                            │
│     │  e. Update migration status                               │
│     │     migration.Status = 'Completed'                        │
│     │     migration.CompletedDate = UtcNow                      │
│     │                                                            │
│     └─ COMMIT TRANSACTION                                       │
│                                                                   │
│     f. Send confirmation email                                   │
│        "Your plan has been updated"                              │
└──────────────────────────────────────────────────────────────────┘
```

---

## 5. Subscription Cancellation Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                  SUBSCRIPTION CANCELLATION FLOW                       │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────┐
│    USER     │
│  Cancels    │
│Subscription │
└──────┬──────┘
       │
       │ POST /api/subscriptions/{id}/cancel
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  SubscriptionLifecycleService.CancelSubscriptionAsync()           │
│                                                                   │
│  1. Get subscription                                             │
│     subscription = GetById(subscriptionId)                       │
│                                                                │
│  2. Validate access                                             │
│     - User owns subscription OR is admin                        │
│                                                                │
│  3. Validate status transition                                  │
│     - Can transition from current status to 'Cancelled'?        │
│                                                                │
│  4. Cancel Stripe subscription                                  │
│     StripeService.CancelSubscription(stripeSubscriptionId)       │
│                                                                │
│  5. Update subscription                                         │
│     ┌─ BEGIN TRANSACTION                                        │
│     │  - Status = 'Cancelled'                                   │
│     │  - CancelledDate = UtcNow                                 │
│     │  - CancellationReason = reason                            │
│     │  - EndDate = UtcNow                                       │
│     │  - AutoRenew = false                                      │
│     │                                                            │
│     │  - Record status history                                  │
│     │    StatusHistory = {                                       │
│     │      SubscriptionId: subscriptionId,                      │
│     │      Status: 'Cancelled',                                  │
│     │      Reason: reason                                        │
│     │    }                                                       │
│     │                                                            │
│     │  - Update UserSubscriptionPrivilegeUsage                  │
│     │    For each privilege usage:                              │
│     │      - Suspend access (optional)                          │
│     └─ COMMIT TRANSACTION                                       │
│                                                                │
│  6. Send cancellation confirmation email                        │
│     NotificationService.SendCancellationConfirmation()          │
│                                                                │
│  7. Handle cleanup (optional)                                   │
│     - Archive historical data                                   │
│     - Final billing adjustments                                 │
│                                                                │
│  return success                                                 │
└──────────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  USER ACCESS REVOKED                                              │
│  - No further charges                                            │
│  - Current billing period honored                                │
│  - Data preserved for historical purposes                        │
└──────────────────────────────────────────────────────────────────┘
```

---

## 6. Component Interaction Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                  COMPONENT INTERACTION ARCHITECTURE                    │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                           API LAYER                                  │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌────────────────────┐  ┌────────────────────┐  ┌──────────────┐  │
│  │ SubscriptionPlans  │  │  Subscriptions     │  │  Billing     │  │
│  │   Controller       │  │   Controller       │  │  Controller  │  │
│  └────────────────────┘  └────────────────────┘  └──────────────┘  │
│                                                                      │
│  ┌────────────────────┐  ┌────────────────────┐  ┌──────────────┐  │
│  │ StripeWebhook      │  │ StripeController   │  │ Payment      │  │
│  │   Controller       │  │                    │  │ Controller   │  │
│  └────────────────────┘  └────────────────────┘  └──────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
                          │            │            │
                          ▼            ▼            ▼
┌──────────────────────────────────────────────────────────────────────┐
│                        APPLICATION LAYER                              │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌────────────────────┐  ┌────────────────────┐  ┌──────────────┐  │
│  │ SubscriptionPlan   │  │ Subscription       │  │ Subscription │  │
│  │   Service          │  │   Service          │  │ Lifecycle    │  │
│  │                    │  │                    │  │   Service    │  │
│  │ - CRUD plans       │  │ - Query subs       │  │ - Create     │  │
│  │ - Activate/        │  │ - Get by user      │  │ - Cancel     │  │
│  │   Deactivate       │  │                    │  │ - Pause      │  │
│  │ - Manage           │  │                    │  │ - Resume     │  │
│  │   privileges       │  │                    │  │ - Upgrade    │  │
│  └────────────────────┘  └────────────────────┘  └──────────────┘  │
│                                                                      │
│  ┌────────────────────┐  ┌────────────────────┐  ┌──────────────┐  │
│  │ Subscription       │  │ Privilege          │  │ Plan         │  │
│  │ Billing Service    │  │   Service          │  │ Versioning   │  │
│  │                    │  │                    │  │   Service    │  │
│  │ - Create billing   │  │ - Use privilege    │  │ - Create     │  │
│  │ - Process          │  │ - Check remaining  │  │   versions   │  │
│  │   payments         │  │ - Track usage      │  │ - Schedule   │  │
│  │ - Handle overages  │  │ - Reset counters   │  │   migrations │  │
│  └────────────────────┘  └────────────────────┘  └──────────────┘  │
│                                                                      │
│  ┌────────────────────┐  ┌────────────────────┐  ┌──────────────┐  │
│  │ Plan Pricing       │  │ Automated Billing  │  │ Notification │  │
│  │   Service          │  │   Service          │  │   Service    │  │
│  │                    │  │                    │  │              │  │
│  │ - Calculate        │  │ - Process renewal  │  │ - Email/SMS  │  │
│  │   base price       │  │ - Handle failures  │  │ - Alerts     │  │
│  │ - Apply discounts  │  │ - Reset counters   │  │              │  │
│  └────────────────────┘  └────────────────────┘  └──────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
                          │            │            │
                          ▼            ▼            ▼
┌──────────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                             │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌────────────────────┐  ┌────────────────────┐  ┌──────────────┐  │
│  │ SubscriptionPlan   │  │ Subscription       │  │ Privilege    │  │
│  │   Repository       │  │   Repository       │  │   Repository │  │
│  │                    │  │                    │  │              │  │
│  │ - GetById          │  │ - GetByUser        │  │ - GetById    │  │
│  │ - GetActive        │  │ - GetDueForBilling │  │ - GetByType  │  │
│  │ - Create/Update    │  │ - Create/Update    │  │              │  │
│  └────────────────────┘  └────────────────────┘  └──────────────┘  │
│                                                                      │
│  ┌────────────────────┐  ┌────────────────────┐  ┌──────────────┐  │
│  │ Billing            │  │ PrivilegeUsage     │  │ Stripe       │  │
│  │   Repository       │  │   Repository       │  │   Service    │  │
│  │                    │  │                    │  │              │  │
│  │ - Create           │  │ - GetByUser        │  │ - Customers  │  │
│  │ - GetByUser        │  │ - UpdateUsage      │  │ - Subscriptions│  │
│  │                    │  │                    │  │ - Payments   │  │
│  └────────────────────┘  └────────────────────┘  └──────────────┘  │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                   BACKGROUND SERVICES                         │  │
│  ├──────────────────────────────────────────────────────────────┤  │
│  │                                                              │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐           │  │
│  │  │ Automated  │  │ Privilege  │  │ Scheduled  │           │  │
│  │  │ Billing    │  │   Reset    │  │ Migration  │           │  │
│  │  │ (Hourly)   │  │ (Periodic) │  │ (Periodic) │           │  │
│  │  └────────────┘  └────────────┘  └────────────┘           │  │
│  │                                                              │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐           │  │
│  │  │ Failed     │  │ Unprocessed│  │ Stripe     │           │  │
│  │  │ Refund     │  │ Webhook    │  │ Sync Job   │           │  │
│  │  │ Retry      │  │ Retry      │  │ (Hourly)   │           │  │
│  │  └────────────┘  └────────────┘  └────────────┘           │  │
│  │                                                              │  │
│  │  ┌────────────────────────────────────────────────────────┐│  │
│  │  │ ReconciliationBackgroundService                        ││  │
│  │  │ (Nightly data integrity checks)                        ││  │
│  │  └────────────────────────────────────────────────────────┘│  │
│  └──────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌──────────────────────────────────────────────────────────────────────┐
│                         DATABASE LAYER                                │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  SubscriptionPlans  Subscriptions  SubscriptionPayments             │
│  PlansPrivileges   UserSubscriptionPrivilegeUsages                 │
│  Privileges        BillingRecords  PaymentRefunds                  │
│  ScheduledPlanMigrations  SubscriptionStatusHistories              │
│  PrivilegeUsageHistories  MasterTables                             │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 7. Data Flow Summary

```
USER REQUEST
    │
    ├─→ VIEW PLANS → SubscriptionPlanService → SubscriptionPlanRepository → DB
    │
    ├─→ SUBSCRIBE → StripeController → StripeService → Stripe API
    │               └─→ Webhook → StripeWebhookController
    │                           └─→ SubscriptionLifecycleService
    │                                          └─→ Multiple Repositories → DB
    │
    ├─→ USE SERVICE → PrivilegeService → UserSubscriptionPrivilegeUsage Repository → DB
    │
    ├─→ CANCEL → SubscriptionLifecycleService → StripeService → Stripe API
    │                                           └─→ Multiple Repositories → DB
    │
    └─→ VIEW USAGE → SubscriptionBillingService → Multiple Repositories → DB

AUTOMATED PROCESSES
    │
    ├─→ HOURLY BILLING → AutomatedBillingBackgroundService
    │                   └─→ SubscriptionBillingService
    │                              └─→ StripeService → Stripe API
    │                              └─→ Multiple Repositories → DB
    │
    ├─→ PRIVILEGE RESET → PrivilegeResetBackgroundService
    │                    └─→ PrivilegeService → UserSubscriptionPrivilegeUsage Repository → DB
    │
    ├─→ PLAN MIGRATION → ScheduledMigrationBackgroundService
    │                  └─→ SubscriptionLifecycleService → Multiple Repositories → DB
    │
    └─→ STRIPE SYNC → StripeSyncJob → StripeService → Stripe API
                                          └─→ Multiple Repositories → DB
```

---

**Visual Documentation Complete**


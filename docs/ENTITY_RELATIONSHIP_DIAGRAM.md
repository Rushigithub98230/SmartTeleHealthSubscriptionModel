## SmartTeleHealth Subscription Management System

Complete visual representation of all entities and their relationships.

---

## CORE SUBSCRIPTION ENTITIES

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         SUBSCRIPTION ECOSYSTEM                               │
└─────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────────┐
                              │      User       │
                              │   (Identity)    │
                              ├─────────────────┤
                              │ Id (int) PK     │
                              │ Email           │
                              │ FirstName       │
                              │ LastName        │
                              │ StripeCustomerId│
                              │ UserRoleId FK   │
                              └────────┬────────┘
                                       │
                                       │ 1:N
                                       │
                              ┌────────▼────────┐
                              │  Subscription   │
                              ├─────────────────┤
                              │ Id (Guid) PK    │
                              │ UserId FK       │───────────────┐
                              │ SubscriptionPlan│               │
                              │   Id FK         │───┐           │
                              │ Status          │   │           │
                              │ CurrentPrice    │   │           │
                              │ StartDate       │   │           │
                              │ EndDate         │   │           │
                              │ NextBillingDate │   │           │
                              │ LastBillingDate │   │           │
                              │ StripeSubscription│  │           │
                              │   Id            │   │           │
                              │ StripePriceId   │   │           │
                              │ PaymentMethodId │   │           │
                              └────────┬────────┘   │           │
                                       │            │           │
                 ┌─────────────────────┼────────────┘           │
                 │                     │                        │
                 │ N:1                 │ 1:N                    │ 1:N
                 │                     │                        │
    ┌────────────▼────────┐   ┌────────▼──────────┐  ┌─────────▼─────────┐
    │ SubscriptionPlan    │   │  BillingRecord    │  │SubscriptionPayment│
    ├─────────────────────┤   ├───────────────────┤  ├───────────────────┤
    │ Id (Guid) PK        │   │ Id (Guid) PK      │  │ Id (Guid) PK      │
    │ Name                │   │ UserId FK         │  │ SubscriptionId FK │
    │ Price               │   │ SubscriptionId FK │  │ BillingRecordId FK│
    │ BillingCycleId FK   │─┐ │ Type (enum)       │  │ Amount            │
    │ CategoryId FK       │ │ │ Status (enum)     │  │ Status (enum)     │
    │ CurrencyId FK       │ │ │ Amount            │  │ Type (enum)       │
    │ StripeProductId     │ │ │ TotalAmount       │  │ BillingPeriodStart│
    │ StripePriceId       │ │ │ BillingDate       │  │ BillingPeriodEnd  │
    │ VersionNumber       │ │ │ DueDate           │  │ DueDate           │
    │ IsLatestVersion     │ │ │ PaidAt            │  │ PaidAt            │
    │ ParentPlanId FK     │ │ │ StripeInvoiceId   │  │ StripePaymentIntent│
    │ PrivilegesTotalCost │ │ │ StripePaymentIntent│ │   Id              │
    │ AdminCommission%    │ │ └────────┬──────────┘  │ StripeInvoiceId   │
    │ IsAutoCalculated    │ │          │              │ AttemptCount      │
    │ PriceChangeNotice   │ │          │              │ NextRetryAt       │
    │   Days              │ │          │ 1:N          │ RefundedAmount    │
    └──────────┬──────────┘ │          │              └───────────────────┘
               │            │          │
               │ 1:N        │          │ 1:N
               │            │          │
    ┌──────────▼────────────▼──┐  ┌───▼─────────────┐
    │SubscriptionPlanPrivilege │  │BillingAdjustment│
    ├──────────────────────────┤  ├─────────────────┤
    │ Id (Guid) PK             │  │ Id (Guid) PK    │
    │ SubscriptionPlanId FK    │  │ BillingRecordId │
    │ PrivilegeId FK           │  │   FK            │
    │ Value (int)              │  │ Type (enum)     │
    │   -1 = Unlimited         │  │ Amount          │
    │    0 = Disabled          │  │ Description     │
    │   >0 = Limited count     │  │ IsPercentage    │
    │ PrivilegeBaseCost        │  │ Percentage      │
    │ UnitCost (overage)       │  │ AppliedAt       │
    │ DurationMonths           │  │ AppliedBy       │
    └──────────┬───────────────┘  │ IsApproved      │
               │                  └─────────────────┘
               │ N:1
               │
    ┌──────────▼───────────┐
    │     Privilege        │
    ├──────────────────────┤
    │ Id (Guid) PK         │
    │ Name                 │
    │ Description          │
    │ PrivilegeTypeId FK   │
    └──────────┬───────────┘
               │
               │ N:1
               │
    ┌──────────▼──────────────┐
    │  MasterPrivilegeType    │
    ├─────────────────────────┤
    │ Id (Guid) PK            │
    │ Name                    │
    │ Description             │
    └─────────────────────────┘


┌────────────────────────────────────────┐
│      BILLING CYCLE REFERENCE           │
│                                        │
│  ┌──────────────────────┐             │
│  │ MasterBillingCycle   │             │
│  ├──────────────────────┤             │
│  │ Id (Guid) PK         │             │
│  │ Name                 │             │
│  │   "monthly" (30d)    │             │
│  │   "quarterly" (90d)  │             │
│  │   "annual" (365d)    │             │
│  │ DurationInDays       │             │
│  └──────────────────────┘             │
│         ▲                              │
│         │                              │
│         └─ Referenced by:              │
│            - SubscriptionPlan          │
│              .BillingCycleId           │
│            - Subscription              │
│              .BillingCycle (computed)  │
└────────────────────────────────────────┘
```

---

## PRIVILEGE USAGE TRACKING

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    PRIVILEGE USAGE SYSTEM                                    │
└─────────────────────────────────────────────────────────────────────────────┘

    Subscription
         │
         │ Has privileges from plan
         │
         ├─────────────────────┐
         │                     │
         │              SubscriptionPlan
         │                     │
         │                     │ Contains
         │                     │
         │              SubscriptionPlanPrivilege
         │                     │
         │                     │ Defines limits:
         │                     │ - Value = TOTAL for cycle
         │                     │ - UnitCost = Overage cost
         │                     │
         │              When subscription created
         │                     │
         │                     ▼
         │              UserSubscriptionPrivilegeUsage
         │                     │
         │              ┌──────┴──────────────────┐
         │              │ UsedValue = 0           │ ← Starts at zero
         │              │ AllowedValue = Value    │ ← From plan privilege
         │              │ UsagePeriodStart        │ ← Billing period start
         │              │ UsagePeriodEnd          │ ← Billing period end
         │              └──────┬──────────────────┘
         │                     │
         │              When user uses privilege
         │                     │
         │              ┌──────▼──────────────────┐
         │              │ UsedValue++ (increment) │
         │              │ LastUsedAt = Now        │
         │              └──────┬──────────────────┘
         │                     │
         │              Check if exceeded
         │                     │
         │         ┌───────────┼────────────┐
         │         │           │            │
         │    Within Limit  Exceeded    Unlimited
         │         │           │            │
         │    ┌────▼────┐ ┌────▼─────┐  ┌──▼──┐
         │    │ Allow   │ │ Allow +  │  │Allow│
         │    │ access  │ │ Track    │  │     │
         │    └─────────┘ │ overage  │  └─────┘
         │                └────┬─────┘
         │                     │
         │              ┌──────▼───────────────────┐
         │              │ Overage Count:           │
         │              │ = UsedValue - AllowedValue│
         │              │ = 7 - 5 = 2              │
         │              │                          │
         │              │ Overage Charge:          │
         │              │ = 2 × UnitCost           │
         │              │ = 2 × $15 = $30          │
         │              └──────┬───────────────────┘
         │                     │
         │              At next billing
         │                     │
         │         ❌ CURRENT: Added to adjustment
         │                     but not billed separately
         │                     │
         │         ✅ SHOULD: Create overage billing
         │                    record and process payment
         │
         └─ When billing period resets
                     │
            ┌────────▼───────────────────┐
            │ Reset ALL fields:          │
            │ - UsedValue = 0            │
            │ - AllowedValue = Value     │
            │ - UsagePeriodStart = NEW   │
            │ - UsagePeriodEnd = NEW     │
            │ - ResetAt = Now            │
            └────────────────────────────┘
```

---

## PAYMENT TRACKING HIERARCHY

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     PAYMENT TRACKING HIERARCHY                               │
└─────────────────────────────────────────────────────────────────────────────┘

    Subscription (sub_xyz)
         │
         │ Generates billing
         │
         ▼
    BillingRecord (billing_abc)
         │
         │ Type: Subscription, Overage, Consultation, etc.
         │ Status: Pending → Paid → Refunded
         │ Amount: $50.00
         │ StripeInvoiceId: in_123456
         │
         ├─────────────────┐
         │                 │
         │ 1:1 (if subscription-related)
         │                 │
         ▼                 ▼
    SubscriptionPayment    BillingAdjustment (optional)
         │                      │
         │                      │ Type: Discount, Credit, LateFee
         │                      │ Amount: $5.00
         │                      │ Description: "Early bird discount"
         │                      │
    ├────┴────────────────┐    │
    │ Links billing to    │    │
    │ subscription        │    │
    │ Tracks:             │    │
    │ - Billing period    │    │
    │ - Payment attempts  │    │
    │ - Retry schedule    │    │
    │ - Refunds           │    │
    └────┬────────────────┘    │
         │                     │
         │ 1:N                 │
         │                     │
         ▼                     │
    PaymentRefund              │
         │                     │
         │ RefundAmount        │
         │ Reason              │
         │ RefundedAt          │
         │ StripeRefundId      │
         └─────────────────────┘

RELATIONSHIP RULES:
─────────────────────────────────────────

1. Every Subscription → Multiple BillingRecords
2. Every BillingRecord (Type: Subscription/Overage/Recurring) 
   → ONE SubscriptionPayment
3. Every BillingRecord → Multiple BillingAdjustments (optional)
4. Every SubscriptionPayment → Multiple PaymentRefunds (optional)

CRITICAL FIELDS FOR BILLING:
─────────────────────────────────────────

Subscription:
├─ LastBillingDate: Start of current billing period
├─ NextBillingDate: End of current period / Start of next
└─ CurrentPrice: Amount to charge per billing

BillingRecord:
├─ Type: Subscription (regular), Overage (extra usage), etc.
├─ Status: Pending → Paid → Refunded
└─ StripeInvoiceId: Link to Stripe invoice (UNIQUE!)

SubscriptionPayment:
├─ BillingPeriodStart: Period this payment covers (start)
├─ BillingPeriodEnd: Period this payment covers (end)
├─ AttemptCount: Number of payment attempts
└─ NextRetryAt: When to retry if failed
```

---

## PRIVILEGE ALLOCATION HIERARCHY

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                  PRIVILEGE ALLOCATION HIERARCHY                              │
└─────────────────────────────────────────────────────────────────────────────┘

Level 1: PRIVILEGE DEFINITION (Master Data)
────────────────────────────────────────────────────
    ┌─────────────────────────────────────┐
    │         Privilege                   │
    │         (Master)                    │
    ├─────────────────────────────────────┤
    │ Id: guid_privilege_telecons         │
    │ Name: "Teleconsultation"            │
    │ Description: "Remote consultation"  │
    │ PrivilegeTypeId: guid_type_consult  │
    └─────────────────────────────────────┘
                    │
                    │ Referenced by plans
                    │
                    ▼

Level 2: PLAN CONFIGURATION (Per Plan Settings)
────────────────────────────────────────────────────
    ┌─────────────────────────────────────┐
    │   SubscriptionPlanPrivilege         │
    │   (Plan: Basic Monthly)             │
    ├─────────────────────────────────────┤
    │ SubscriptionPlanId: guid_basic_mon  │
    │ PrivilegeId: guid_privilege_telecons│
    │ Value: 5                            │ ← Admin sets TOTAL for cycle
    │ PrivilegeBaseCost: $3.00            │ ← For plan pricing
    │ UnitCost: $15.00                    │ ← Overage cost
    └─────────────────────────────────────┘
                    │
                    │ When user subscribes
                    │
                    ▼

Level 3: USER ALLOCATION (Per User Tracking)
────────────────────────────────────────────────────
    ┌─────────────────────────────────────┐
    │ UserSubscriptionPrivilegeUsage      │
    │ (User: john@email.com)              │
    ├─────────────────────────────────────┤
    │ SubscriptionId: guid_sub_123        │
    │ PrivilegeId: guid_privilege_telecons│
    │ AllowedValue: 5                     │ ← Copied from plan
    │ UsedValue: 0                        │ ← Starts at zero
    │ UsagePeriodStart: 2024-10-01        │ ← Billing period
    │ UsagePeriodEnd: 2024-11-01          │
    └─────────────────────────────────────┘
                    │
                    │ User uses privilege
                    │
                    ▼
    ┌─────────────────────────────────────┐
    │ After 3 teleconsultations           │
    ├─────────────────────────────────────┤
    │ UsedValue: 3                        │
    │ RemainingValue: 2                   │ ← Computed: 5 - 3
    │ UsagePercentage: 60%                │ ← Computed: 3/5
    │ IsExhausted: false                  │ ← Not exceeded
    └─────────────────────────────────────┘
                    │
                    │ User continues using
                    │
                    ▼
    ┌─────────────────────────────────────┐
    │ After 7 teleconsultations           │
    ├─────────────────────────────────────┤
    │ UsedValue: 7                        │
    │ RemainingValue: 0                   │ ← Exceeded!
    │ UsagePercentage: 140%               │ ← Over limit
    │ IsExhausted: true                   │ ← Exhausted
    │                                     │
    │ Overage: 7 - 5 = 2                  │
    │ Overage Charge: 2 × $15 = $30       │
    └─────────────────────────────────────┘
                    │
                    │ At next billing
                    │
                    ▼
    ┌─────────────────────────────────────┐
    │ Should create overage billing       │
    │ ❌ ISSUE #2: This doesn't happen!   │
    └─────────────────────────────────────┘
```

---

## STATUS HISTORY TRACKING

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    SUBSCRIPTION STATUS HISTORY                               │
└─────────────────────────────────────────────────────────────────────────────┘

    Subscription (sub_123)
         │
         │ Every status change creates history record
         │
         ▼
    SubscriptionStatusHistory
    ┌─────────────────────────────────────┐
    │ Id: guid_history_001                │
    │ SubscriptionId: sub_123             │
    │ FromStatus: null                    │
    │ ToStatus: "Pending"                 │
    │ Reason: "Subscription created"      │
    │ ChangedAt: 2024-10-01 10:00:00     │
    │ ChangedByUserId: 101                │
    └─────────────────────────────────────┘
         │
         │ User activates subscription
         │
         ▼
    ┌─────────────────────────────────────┐
    │ Id: guid_history_002                │
    │ FromStatus: "Pending"               │
    │ ToStatus: "Active"                  │
    │ Reason: "Payment succeeded"         │
    │ ChangedAt: 2024-10-01 10:05:00     │
    └─────────────────────────────────────┘
         │
         │ Payment fails
         │
         ▼
    ┌─────────────────────────────────────┐
    │ Id: guid_history_003                │
    │ FromStatus: "Active"                │
    │ ToStatus: "PaymentFailed"           │
    │ Reason: "Payment declined"          │
    │ ChangedAt: 2024-11-01 10:00:00     │
    └─────────────────────────────────────┘
         │
         │ Etc...
         │
         └─ Complete audit trail of all status changes
```

---

## MASTER DATA TABLES

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         MASTER DATA TABLES                                   │
└─────────────────────────────────────────────────────────────────────────────┘

MasterBillingCycle          MasterCurrency          MasterPrivilegeType
├─ monthly (30 days)        ├─ USD                  ├─ Teleconsultation
├─ quarterly (90 days)      ├─ EUR                  ├─ In-Person Visit
├─ annual (365 days)        ├─ GBP                  ├─ Messaging
├─ weekly (7 days)          └─ CAD                  ├─ Medication Delivery
└─ daily (1 day)                                    └─ Health Assessment

Category                    Role (UserRole)
├─ Mental Health            ├─ Patient (RoleId: 1)
├─ Physical Health          ├─ Provider (RoleId: 2)
├─ Wellness                 └─ Admin (RoleId: 332)
└─ Specialized Care

All master tables:
├─ Seeded at startup (via SeedData.cs)
├─ Referenced by foreign keys
└─ Rarely changed (configuration data)
```

---

## DATA FLOW: Subscription Creation to Billing

```
┌─────────────────────────────────────────────────────────────────────────────┐
│              COMPLETE DATA FLOW: CREATION TO BILLING                         │
└─────────────────────────────────────────────────────────────────────────────┘

STEP 1: USER SUBSCRIBES
────────────────────────────────────────

User selects: "Premium Monthly" plan
    │
    ├─ Get SubscriptionPlan (guid_premium_monthly)
    │   ├─ Price: $100.00
    │   ├─ BillingCycleId: guid_monthly
    │   └─ Has 5 privileges
    │
    ├─ Create Stripe Customer (if not exists)
    │   └─ Returns: cus_abc123
    │
    ├─ Create Stripe Subscription
    │   ├─ Customer: cus_abc123
    │   ├─ Price: price_premium_monthly
    │   └─ Returns: sub_stripe_xyz
    │
    └─ Create local Subscription
        ├─ UserId: 101
        ├─ SubscriptionPlanId: guid_premium_monthly
        ├─ Status: Active
        ├─ CurrentPrice: $100.00
        ├─ StartDate: 2024-10-01
        ├─ NextBillingDate: 2024-11-01
        ├─ StripeSubscriptionId: sub_stripe_xyz
        └─ StripeCustomerId: cus_abc123


STEP 2: ALLOCATE PRIVILEGES
────────────────────────────────────────

For each PlanPrivilege in Premium plan:
    │
    ├─ Privilege 1: Teleconsultation
    │   ├─ Value: 10
    │   ├─ PrivilegeBaseCost: $5.00
    │   └─ UnitCost: $15.00
    │
    └─ Create UserSubscriptionPrivilegeUsage:
        ├─ SubscriptionId: guid_sub_123
        ├─ PrivilegeId: guid_telecons
        ├─ AllowedValue: 10 (copied from plan)
        ├─ UsedValue: 0
        ├─ UsagePeriodStart: 2024-10-01
        └─ UsagePeriodEnd: 2024-11-01


STEP 3: USER USES SERVICES (During Month)
────────────────────────────────────────

User books teleconsultation:
    │
    ├─ Check privilege usage
    │   ├─ UsedValue: 3
    │   ├─ AllowedValue: 10
    │   └─ Remaining: 7 ✅ OK
    │
    ├─ Allow consultation
    │
    └─ Update usage:
        └─ UsedValue: 4 (incremented)


STEP 4: BILLING TIME (NextBillingDate reached)
────────────────────────────────────────

Date: 2024-11-01
    │
    ├─ AutomatedBillingService runs
    │
    ├─ Create BillingRecord:
    │   ├─ Type: Subscription
    │   ├─ Amount: $100.00
    │   ├─ Status: Pending
    │   └─ DueDate: 2024-11-08
    │
    ├─ Process Payment:
    │   │
    │   ├─ Create SubscriptionPayment:
    │   │   ├─ BillingPeriodStart: 2024-10-01
    │   │   ├─ BillingPeriodEnd: 2024-11-01
    │   │   └─ Status: Pending
    │   │
    │   ├─ Charge via Stripe: $100.00
    │   │
    │   └─ If success, update in TRANSACTION:
    │       ├─ BillingRecord.Status: Paid
    │       ├─ SubscriptionPayment.Status: Succeeded
    │       ├─ Subscription.LastBillingDate: 2024-11-01
    │       └─ Subscription.NextBillingDate: 2024-12-01
    │
    ├─ ❌ SHOULD: Process overage charges
    │   └─ (ISSUE #2: This doesn't happen!)
    │
    └─ Reset Privileges:
        ├─ UsedValue: 4 → 0
        ├─ AllowedValue: 10 (unchanged)
        ├─ UsagePeriodStart: 2024-11-01 (new period)
        └─ UsagePeriodEnd: 2024-12-01


STEP 5: STRIPE WEBHOOK (Confirmation)
────────────────────────────────────────

Stripe sends: invoice.payment_succeeded
    │
    ├─ StripeInvoiceId: in_xyz789
    │
    ├─ ❌ ISSUE #1: Webhook creates duplicate
    │   │
    │   ├─ SHOULD: Check if billing_abc exists
    │   │   └─ GetByStripeInvoiceIdAsync("in_xyz789")
    │   │   └─ If exists: Update, don't create
    │   │
    │   └─ CURRENT: Always creates new billing_def
    │       └─ Result: TWO billing records!
    │
    └─ Calls RecordExternalPaymentAsync ✅
        └─ Creates/updates SubscriptionPayment
```

---

## STRIPE SYNC MAPPING

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                   LOCAL DATABASE ↔ STRIPE MAPPING                           │
└─────────────────────────────────────────────────────────────────────────────┘

LOCAL ENTITY                STRIPE ENTITY               SYNC DIRECTION
──────────────────────────────────────────────────────────────────────────────

User                        Customer                    Local → Stripe
├─ Id: 101                  ├─ Id: cus_abc123         Create customer
├─ Email                    ├─ Email                  when user signs up
├─ FirstName                ├─ Name                   
├─ StripeCustomerId ────────┼─ [LINK]                 Store Stripe ID
└─ UserRoleId               └─ Metadata               locally

SubscriptionPlan            Product + Price             Local → Stripe
├─ Id: guid_basic_mon       ├─ Product: prod_basic    Create product/price
├─ Name                     ├─ Price: price_monthly   when plan created
├─ Price: $50.00            ├─ Amount: 5000 (cents)  
├─ BillingCycle: Monthly    ├─ Recurring: month      
├─ StripeProductId ─────────┼─ [LINK]                
└─ StripePriceId ───────────┼─ [LINK]                 

Subscription                Subscription                Both directions
├─ Id: guid_sub_123         ├─ Id: sub_xyz           ├─ Create: Local → Stripe
├─ SubscriptionPlanId       ├─ Items[0].Price        ├─ Updates: Stripe → Local
├─ CurrentPrice             ├─ Items[0].Price.Amount │   (via webhooks)
├─ Status                   ├─ Status                └─ Cancel: Both
├─ StripeSubscriptionId ────┼─ [LINK]                
├─ StripePriceId            ├─ Items[0].Price.Id     
└─ PaymentMethodId          └─ DefaultPaymentMethod  

BillingRecord               Invoice                     Stripe → Local
├─ Id: guid_billing_abc     ├─ Id: in_123            Created by webhook
├─ Amount                   ├─ Amount (cents)        when invoice 
├─ Status: Paid             ├─ Status: paid          finalized
├─ StripeInvoiceId ─────────┼─ [LINK]                
└─ StripePaymentIntentId ───┼─ PaymentIntent.Id      

SubscriptionPayment         PaymentIntent               Both directions
├─ Id: guid_payment_xyz     ├─ Id: pi_abc           ├─ Create: Local → Stripe
├─ Amount                   ├─ Amount (cents)       └─ Confirm: Stripe → Local
├─ Status                   ├─ Status                   (via webhooks)
├─ StripePaymentIntentId ───┼─ [LINK]
└─ StripeInvoiceId          └─ Invoice.Id

SYNC RULES:
──────────────────────────────────────────────────────────────────────────────

1. Customer: Create in Stripe on user signup, store ID locally
2. Product/Price: Create in Stripe when plan created, store IDs
3. Subscription: Create in Stripe when user subscribes, sync status via webhooks
4. Invoice/Payment: Created by Stripe, synced to local via webhooks
5. Billing adjustments: Local only (not synced to Stripe)
6. Privilege usage: Local only (Stripe doesn't know about this)

CRITICAL SYNC POINTS:
──────────────────────────────────────────────────────────────────────────────

A. Subscription Creation:
   Local → Stripe: CreateSubscriptionAsync(customerId, priceId)
   Returns: StripeSubscriptionId → Store locally

B. Payment Processing:
   Local → Stripe: CreatePaymentIntent(amount, customerId)
   Returns: PaymentIntentId → Store in BillingRecord & SubscriptionPayment

C. Webhook Events:
   Stripe → Local: invoice.payment_succeeded
   Action: Update BillingRecord.Status = Paid, create SubscriptionPayment

D. Status Updates:
   Stripe → Local: subscription.updated
   Action: Update Subscription.Status to match Stripe
```

---

## BILLING CYCLE IMPACT ON ENTITIES

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                  HOW BILLING CYCLE AFFECTS ENTITIES                          │
└─────────────────────────────────────────────────────────────────────────────┘

SubscriptionPlan (Billing Cycle = Monthly)
├─ BillingCycleId: guid_monthly
├─ BillingCycle.DurationInDays: 30
└─ Price: $50.00 (explicit monthly price)

    When user subscribes:
    
Subscription
├─ SubscriptionPlanId: (references plan above)
├─ BillingCycle: [Computed from plan] = Monthly
├─ StartDate: 2024-10-01
├─ NextBillingDate: 2024-10-01 + 30 days = 2024-10-31
└─ CurrentPrice: $50.00 (copied from plan)

    Initial privilege allocation:
    
UserSubscriptionPrivilegeUsage
├─ AllowedValue: 10 (from plan privilege)
├─ UsedValue: 0
├─ UsagePeriodStart: 2024-10-01
└─ UsagePeriodEnd: 2024-10-31 (matches billing cycle!)

    After first billing:
    
BillingRecord
├─ BillingDate: 2024-10-31
├─ Amount: $50.00
└─ Type: Subscription

SubscriptionPayment
├─ BillingPeriodStart: 2024-10-01 (period covered)
├─ BillingPeriodEnd: 2024-10-31
└─ Amount: $50.00

    Subscription updated:
    
Subscription
├─ LastBillingDate: 2024-10-31 (period just billed)
├─ NextBillingDate: 2024-11-30 (next billing)
└─ LastPaymentDate: 2024-10-31

    Privileges reset:
    
UserSubscriptionPrivilegeUsage
├─ UsedValue: 5 → 0 (RESET!)
├─ AllowedValue: 10 (unchanged)
├─ UsagePeriodStart: 2024-10-31 → 2024-10-31 (NEW period)
└─ UsagePeriodEnd: 2024-10-31 → 2024-11-30


COMPARISON: Monthly vs Annual
──────────────────────────────────────────────────────────────────────────────

MONTHLY PLAN:
├─ Billing every 30 days
├─ Price: $50/month
├─ Privileges reset every 30 days
└─ Example: 5 consultations per month = 5 every 30 days

ANNUAL PLAN:
├─ Billing every 365 days
├─ Price: $500/year (explicit annual price, NOT $50 × 12)
├─ Privileges reset every 365 days
└─ Example: 60 consultations per year = 60 every 365 days

CRITICAL: Each plan has EXPLICIT price, not calculated from monthly!
```

---

## TRANSACTION BOUNDARIES

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        TRANSACTION BOUNDARIES                                │
└─────────────────────────────────────────────────────────────────────────────┘

TRANSACTION 1: Subscription Creation
────────────────────────────────────────
BEGIN TRANSACTION
├─ Create Subscription
├─ Create SubscriptionStatusHistory
└─ COMMIT or ROLLBACK

Note: Stripe subscription created BEFORE transaction
      If transaction fails, Stripe subscription is cleaned up


TRANSACTION 2: Payment Processing ⭐ CRITICAL
────────────────────────────────────────
BEGIN TRANSACTION
├─ Update SubscriptionPayment (Status, PaidAt, etc.)
├─ Update BillingRecord (Status, PaidAt, etc.)
├─ Update Subscription (LastBillingDate, NextBillingDate, etc.)
├─ Reset Privileges (via PrivilegeResetHelper)
└─ COMMIT or ROLLBACK (all-or-nothing)

Note: Stripe payment processed BEFORE transaction
      If transaction fails, payment already charged (manual reconciliation needed)


TRANSACTION 3: Subscription Cancellation
────────────────────────────────────────
BEGIN TRANSACTION
├─ Update Subscription (Status = Cancelled, CancelledDate, etc.)
├─ Create SubscriptionStatusHistory
└─ COMMIT or ROLLBACK

Note: Stripe subscription cancelled BEFORE transaction
      If transaction fails, Stripe subscription is recovered (reactivated)


TRANSACTION 4: Privilege Reset (part of Transaction 2)
────────────────────────────────────────
(No separate transaction - part of payment transaction)
├─ For each UserSubscriptionPrivilegeUsage:
│   ├─ Update UsedValue = 0
│   ├─ Update AllowedValue
│   ├─ Update UsagePeriodStart/End
│   └─ Update ResetAt
└─ All updates within parent transaction
```

**✅ VERIFIED:** Transaction management is EXCELLENT throughout the main flows!

**⚠️ ISSUE:** Background service (AutomatedBillingBackgroundService) doesn't use proper transactions.

---

## SUMMARY: Entity Counts and Relationships

```
ENTITIES BY CATEGORY:
────────────────────────────────────────

Core Entities (15):
├─ User (Identity)
├─ Subscription
├─ SubscriptionPlan
├─ SubscriptionPlanPrivilege
├─ Privilege
├─ UserSubscriptionPrivilegeUsage
├─ BillingRecord
├─ SubscriptionPayment
├─ BillingAdjustment
├─ SubscriptionStatusHistory
├─ PaymentRefund
├─ ProcessedWebhookEvent
├─ ScheduledPlanMigration
├─ PrivilegeUsageHistory
└─ Category

Master Data Tables (5):
├─ MasterBillingCycle
├─ MasterCurrency
├─ MasterPrivilegeType
├─ Role (UserRole)
└─ SystemSettings

Healthcare Entities (10+):
├─ Consultation
├─ Appointment
├─ Provider
├─ HealthAssessment
├─ Prescription
├─ MedicationDelivery
├─ Message
├─ ChatRoom
├─ VideoCall
└─ Document

TOTAL: 30+ entities


KEY RELATIONSHIPS:
────────────────────────────────────────

User ──1:N──→ Subscription
Subscription ──N:1──→ SubscriptionPlan
SubscriptionPlan ──1:N──→ SubscriptionPlanPrivilege
SubscriptionPlanPrivilege ──N:1──→ Privilege
Subscription ──1:N──→ UserSubscriptionPrivilegeUsage
Subscription ──1:N──→ BillingRecord
BillingRecord ──1:1──→ SubscriptionPayment (if subscription-related)
BillingRecord ──1:N──→ BillingAdjustment
Subscription ──1:N──→ SubscriptionStatusHistory
SubscriptionPayment ──1:N──→ PaymentRefund
```

---

**Analysis Complete!** ✅

This entity relationship diagram complements the other analysis documents.

For navigation, see: `README_BILLING_ANALYSIS.md`


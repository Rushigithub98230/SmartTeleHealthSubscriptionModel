# 🏗️ COMPLETE SUBSCRIPTION MANAGEMENT ARCHITECTURE GUIDE
## Part 2: Entity Relationships & Database Schema

---

## 📊 COMPLETE ENTITY RELATIONSHIP DIAGRAM

```
┌──────────────────────────────────────────────────────────────────────┐
│                    SUBSCRIPTION MANAGEMENT ENTITIES                   │
└──────────────────────────────────────────────────────────────────────┘

MASTER DATA LAYER:
┌─────────────┐  ┌──────────────────┐  ┌─────────────────┐  ┌──────────┐
│  Category   │  │ MasterBilling    │  │ MasterCurrency  │  │   User   │
│             │  │     Cycle        │  │                 │  │          │
│ Id (Guid)   │  │                  │  │ Id (Guid)       │  │ Id (int) │
│ Name        │  │ Id (Guid)        │  │ Code (USD)      │  │ Email    │
│ Description │  │ Name (Monthly)   │  │ Symbol ($)      │  │ FullName │
└─────┬───────┘  │ DurationInDays   │  └────────┬────────┘  │ Stripe   │
      │          │  (30, 90, 365)   │           │           │ CustomerId
      │          └────────┬─────────┘           │           └────┬─────┘
      │                   │                     │                 │
      │                   │                     │                 │
      │                   │                     │                 │ 1
      │ 1                 │ 1                   │ 1               │
      └───────────────────┴──────────────────┬──┴─────────────────┘
                                             │
PLAN LAYER:                                  │ 1
┌──────────────────────────────────────────────────────────────────┐
│                      SubscriptionPlan                             │
├──────────────────────────────────────────────────────────────────┤
│ PRIMARY KEY:                                                     │
│  • Id (Guid) PK                                                  │
│                                                                  │
│ BASIC INFO:                                                      │
│  • Name: "Standard Health Plan"                                 │
│  • Description: "5 consultations, 3 medications"                 │
│  • Price: $280.00 (decimal18,2)                                  │
│  • PlanType: Standard/Premium/Enterprise                        │
│                                                                  │
│ FOREIGN KEYS:                                                    │
│  • BillingCycleId → MasterBillingCycle                          │
│  • CurrencyId → MasterCurrency                                  │
│  • CategoryId → Category                                        │
│  • ParentPlanId → SubscriptionPlan (for versioning)            │
│                                                                  │
│ PRICING (CLIENT WORKFLOW):                                      │
│  • IsAutoCalculatedPrice: true/false                            │
│  • PrivilegesTotalCost: $250.00                                 │
│  • AdminCommissionPercent: 10% (nullable)                       │
│  • AdminCommissionFixed: $30.00 (nullable)                      │
│  • CalculatedPrice: $280.00 (computed)                          │
│                                                                  │
│ STRIPE INTEGRATION:                                             │
│  • StripeProductId: "prod_xxxxx"                                │
│  • StripeMonthlyPriceId: "price_monthly_xxxxx"                  │
│  • StripeQuarterlyPriceId: "price_quarterly_xxxxx"              │
│  • StripeAnnualPriceId: "price_annual_xxxxx"                    │
│                                                                  │
│ TRIAL:                                                           │
│  • IsTrialAllowed: true/false                                   │
│  • TrialDurationInDays: 7, 14, 30                               │
│                                                                  │
│ VERSIONING (HEALTHCARE RULE):                                   │
│  • VersionNumber: 1, 2, 3...                                    │
│  • IsLatestVersion: true/false                                  │
│  • PriceChangeNoticeDays: 10, 30, 60, 90                        │
│                                                                  │
│ NAVIGATION PROPERTIES:                                          │
│  • PlanPrivileges: ICollection<SubscriptionPlanPrivilege>       │
│  • Subscriptions: ICollection<Subscription>                     │
│  • ParentPlan: SubscriptionPlan                                 │
│  • ChildVersions: ICollection<SubscriptionPlan>                 │
└──────────────────┬───────────────────────┬───────────────────────┘
                   │ 1                   1 │
              ┌────▼────────┐        ┌─────▼──────────────────────┐
              │             │        │                            │
              │             │        │   SubscriptionPlanPrivilege │
              │             │        ├────────────────────────────┤
              │             │        │ JUNCTION TABLE             │
              │             │        │                            │
              │             │        │ PRIMARY KEY:               │
              │             │        │  • Id (Guid) PK            │
              │             │        │                            │
              │             │        │ FOREIGN KEYS:              │
              │             │        │  • SubscriptionPlanId FK   │
              │             │        │  • PrivilegeId FK          │
              │             │        │  • UsagePeriodId FK        │
              │             │        │                            │
              │             │        │ LIMITS & COSTS:            │
              │             │        │  • Value: 5, 3, -1, 0      │
              │             │        │    (5=limit, -1=unlimited, │
              │             │        │     0=disabled)            │
              │             │        │  • UnitCost: $20, $50 ⭐   │
              │             │        │  • PrivilegeBaseCost: $20  │
              │             │        │  • DailyLimit: 2           │
              │             │        │  • WeeklyLimit: 10         │
              │             │        │  • MonthlyLimit: 30        │
              │             │        │                            │
              │             │        │ COMPUTED:                  │
              │             │        │  • IsUnlimited (Value=-1)  │
              │             │        │  • IsDisabled (Value=0)    │
              │             │        │  • HasOverageCharges       │
              │             │        └──────────┬─────────────────┘
              │             │                   │ N
              │             │                   │
              │             │        ┌──────────▼─────────────┐
              │             │        │    Privilege           │
              │             │        ├────────────────────────┤
              │             │        │ Id (Guid) PK           │
              │             │        │ Name: "Teleconsultation"│
              │             │        │ Description            │
              │             │        │ PrivilegeTypeId FK     │
              │             │        └────────────────────────┘
              │             │
              │             │
USER SUBSCRIPTION LAYER:    │
              │             │ 1
         ┌────┴─────────────▼───────────────────────────────────┐
         │                  Subscription                         │
         ├───────────────────────────────────────────────────────┤
         │ PRIMARY KEY:                                          │
         │  • Id (Guid) PK                                       │
         │                                                       │
         │ FOREIGN KEYS:                                         │
         │  • UserId (int) FK → User                            │
         │  • SubscriptionPlanId (Guid) FK → SubscriptionPlan   │
         │  • BillingCycleId (Guid) FK → MasterBillingCycle     │
         │  • ProviderId (int?) FK → Provider (optional)        │
         │                                                       │
         │ STATUS & LIFECYCLE:                                   │
         │  • Status: "Active", "Paused", "Cancelled", etc.     │
         │  • StatusReason: Text explanation                     │
         │  • StartDate: 2025-10-01                             │
         │  • EndDate: 2026-10-01 (nullable)                    │
         │  • NextBillingDate: 2025-11-01                       │
         │  • LastBillingDate: 2025-10-01                       │
         │                                                       │
         │ PRICING:                                              │
         │  • CurrentPrice: $280.00                             │
         │  • AutoRenew: true/false                             │
         │                                                       │
         │ STRIPE INTEGRATION:                                   │
         │  • StripeSubscriptionId: "sub_xxxxx" ⭐              │
         │  • StripeCustomerId: "cus_xxxxx" ⭐                  │
         │  • StripePriceId: "price_xxxxx" ⭐                   │
         │  • PaymentMethodId: "pm_xxxxx"                       │
         │  • LastPaymentDate: 2025-10-01                       │
         │  • LastPaymentFailedDate: null                       │
         │  • LastPaymentError: null                            │
         │  • FailedPaymentAttempts: 0                          │
         │                                                       │
         │ TRIAL:                                                │
         │  • IsTrialSubscription: true/false                   │
         │  • TrialStartDate: 2025-10-01                        │
         │  • TrialEndDate: 2025-10-15                          │
         │  • TrialDurationInDays: 14                           │
         │                                                       │
         │ USAGE TRACKING:                                       │
         │  • LastUsedDate: 2025-10-15                          │
         │  • TotalUsageCount: 23                               │
         │                                                       │
         │ COMPUTED PROPERTIES:                                  │
         │  • IsSubscriptionActive (Status=="Active")           │
         │  • IsPaused, IsCancelled, IsExpired                  │
         │  • HasPaymentIssues                                  │
         │  • IsInTrial                                         │
         │  • DaysUntilNextBilling: 15                          │
         │  • CanPause, CanResume, CanCancel, CanRenew         │
         │                                                       │
         │ NAVIGATION PROPERTIES:                                │
         │  • User: User entity                                 │
         │  • SubscriptionPlan: Plan details                    │
         │  • BillingCycle: Cycle details                       │
         │  • Provider: Assigned provider                       │
         │  • BillingRecords: ICollection<BillingRecord>        │
         │  • PrivilegeUsages: ICollection<UserSubscription     │
         │                     PrivilegeUsage>                  │
         │  • StatusHistory: ICollection<SubscriptionStatus     │
         │                   History>                           │
         │  • Payments: ICollection<SubscriptionPayment>        │
         └─────┬─────────────┬────────────────┬─────────────────┘
               │ 1         1 │              1 │
        ┌──────▼──────┐  ┌──▼────────────────▼──────────────────┐
        │             │  │                                       │
        │             │  │  UserSubscriptionPrivilegeUsage      │
        │             │  ├──────────────────────────────────────┤
        │             │  │ CRITICAL FOR CLIENT WORKFLOW ⭐       │
        │             │  │                                       │
        │             │  │ PRIMARY KEY:                          │
        │             │  │  • Id (Guid) PK                       │
        │             │  │                                       │
        │             │  │ FOREIGN KEYS:                         │
        │             │  │  • SubscriptionId FK                  │
        │             │  │  • SubscriptionPlanPrivilegeId FK     │
        │             │  │  • PrivilegeId FK                     │
        │             │  │                                       │
        │             │  │ USAGE TRACKING:                       │
        │             │  │  • UsedValue: 5 ⭐                    │
        │             │  │    (How many used)                    │
        │             │  │  • AllowedValue: 6 ⭐                 │
        │             │  │    (Current limit, can increase!)     │
        │             │  │  • UsagePeriodStart: 2025-10-01      │
        │             │  │  • UsagePeriodEnd: 2025-10-31        │
        │             │  │  • LastUsedAt: 2025-10-15            │
        │             │  │  • ResetAt: null                      │
        │             │  │                                       │
        │             │  │ COMPUTED PROPERTIES:                  │
        │             │  │  • RemainingValue ⭐                  │
        │             │  │    = AllowedValue - UsedValue         │
        │             │  │    = 6 - 5 = 1                        │
        │             │  │  • IsExhausted                        │
        │             │  │    = UsedValue >= AllowedValue        │
        │             │  │  • UsagePercentage                    │
        │             │  │    = (Used / Allowed) × 100           │
        │             │  │                                       │
        │             │  │ EXAMPLE STATE:                        │
        │             │  │  Teleconsultation:                    │
        │             │  │   - Used: 5 consultations             │
        │             │  │   - Allowed: 6 (originally 5,         │
        │             │  │              +1 purchased)            │
        │             │  │   - Remaining: 1                      │
        │             │  │                                       │
        │             │  │ NAVIGATION:                           │
        │             │  │  • Subscription                       │
        │             │  │  • SubscriptionPlanPrivilege          │
        │             │  │  • Privilege                          │
        │             │  │  • UsageHistory: ICollection<         │
        │             │  │    PrivilegeUsageHistory>             │
        │             │  └───────────────────────────────────────┘
        │             │
        │             │
        │    ┌────────▼──────────────────────────────────────────┐
        │    │          BillingRecord                            │
        │    ├───────────────────────────────────────────────────┤
        │    │ CRITICAL FOR BILLING TRACKING ⭐                  │
        │    │                                                   │
        │    │ PRIMARY KEY:                                      │
        │    │  • Id (Guid) PK                                   │
        │    │                                                   │
        │    │ FOREIGN KEYS:                                     │
        │    │  • UserId (int) FK                                │
        │    │  • SubscriptionId (Guid?) FK                      │
        │    │  • CurrencyId (Guid) FK                           │
        │    │                                                   │
        │    │ BILLING DETAILS:                                  │
        │    │  • Amount: $20.00                                 │
        │    │  • TaxAmount: $0.00                               │
        │    │  • ShippingAmount: $0.00                          │
        │    │  • TotalAmount: $20.00                            │
        │    │  • Status: Pending/Paid/Failed ⭐                 │
        │    │  • Type: Subscription/Overage ⭐                  │
        │    │    (Subscription=$280 base)                       │
        │    │    (Overage=$20 extra credit)                     │
        │    │  • Description: "Purchase 1 additional..."        │
        │    │  • BillingDate: 2025-10-15                        │
        │    │  • DueDate: 2025-10-15 (immediate!)              │
        │    │  • PaidAt: 2025-10-15 10:30:00                    │
        │    │  • IsRecurring: false                             │
        │    │  • PaymentMethod: "pm_xxxxx"                      │
        │    │                                                   │
        │    │ STRIPE:                                           │
        │    │  • StripeInvoiceId: "in_xxxxx"                    │
        │    │  • StripePaymentIntentId: "pi_xxxxx"             │
        │    │                                                   │
        │    │ NAVIGATION:                                       │
        │    │  • User                                           │
        │    │  • Subscription                                   │
        │    │  • Currency                                       │
        │    │  • Adjustments: ICollection<BillingAdjustment>    │
        │    └───────────────┬───────────────────────────────────┘
        │                    │ 1
        │                    │
        │         ┌──────────▼─────────────────────────────────┐
        │         │      SubscriptionPayment                    │
        │         ├─────────────────────────────────────────────┤
        │         │ PAYMENT TRACKING ⭐                         │
        │         │                                             │
        │         │ PRIMARY KEY:                                │
        │         │  • Id (Guid) PK                             │
        │         │                                             │
        │         │ FOREIGN KEYS:                               │
        │         │  • SubscriptionId (Guid) FK                 │
        │         │  • BillingRecordId (Guid) FK                │
        │         │  • CurrencyId (Guid) FK                     │
        │         │                                             │
        │         │ PAYMENT DETAILS:                            │
        │         │  • Amount: $20.00                           │
        │         │  • TaxAmount: $0.00                         │
        │         │  • NetAmount: $20.00                        │
        │         │  • Status: Succeeded/Failed ⭐              │
        │         │  • Type: Overage/Upfront ⭐                 │
        │         │  • DueDate: 2025-10-15                      │
        │         │  • PaidAt: 2025-10-15 10:30:00              │
        │         │                                             │
        │         │ STRIPE:                                     │
        │         │  • StripePaymentIntentId: "pi_xxxxx"       │
        │         │  • StripeInvoiceId: "in_xxxxx"             │
        │         │  • ReceiptUrl: "https://..."                │
        │         │                                             │
        │         │ REFUNDS:                                    │
        │         │  • RefundedAmount: $0.00                    │
        │         │  • Refunds: ICollection<PaymentRefund>      │
        │         └─────────────────────────────────────────────┘
        │
        │
   ┌────▼────────────────────────────────────┐
   │   SubscriptionStatusHistory             │
   ├─────────────────────────────────────────┤
   │ AUDIT TRAIL ⭐                          │
   │                                         │
   │ PRIMARY KEY:                            │
   │  • Id (Guid) PK                         │
   │                                         │
   │ FOREIGN KEY:                            │
   │  • SubscriptionId (Guid) FK             │
   │  • ChangedByUserId (int?) FK            │
   │                                         │
   │ STATUS CHANGE:                          │
   │  • FromStatus: "Pending"                │
   │  • ToStatus: "Active"                   │
   │  • Reason: "Subscription created"       │
   │  • ChangedAt: 2025-10-01 09:00:00       │
   │  • Metadata: JSON data                  │
   │                                         │
   │ TRACKS ALL STATUS CHANGES:              │
   │  • Pending → Active                     │
   │  • Active → Paused                      │
   │  • Paused → Active                      │
   │  • Active → Cancelled                   │
   │  • Complete audit trail                 │
   └─────────────────────────────────────────┘
```

---

## 📋 ENTITY FIELD BREAKDOWN

### **SubscriptionPlan Table:**

| Column | Type | Required | Description | Example |
|--------|------|----------|-------------|---------|
| `Id` | Guid | ✅ PK | Unique identifier | abc-123-def |
| `Name` | string(100) | ✅ | Plan name | "Standard Health Plan" |
| `Description` | string(1000) | ❌ | Full description | "Includes 5 consultations..." |
| `Price` | decimal(18,2) | ✅ | Base price | $280.00 |
| `BillingCycleId` | Guid | ✅ FK | Billing frequency | Monthly/Yearly |
| `CurrencyId` | Guid | ✅ FK | Currency | USD |
| `CategoryId` | Guid | ❌ FK | Plan category | Mental Health |
| `StripeProductId` | string(100) | ❌ | Stripe product | "prod_xxx" |
| `StripeMonthlyPriceId` | string(100) | ❌ | Stripe price | "price_xxx" |
| `IsTrialAllowed` | bool | ✅ | Trial allowed? | true |
| `TrialDurationInDays` | int | ✅ | Trial days | 14 |
| `IsAutoCalculatedPrice` | bool | ✅ | Auto-calc price? | true |
| `PrivilegesTotalCost` | decimal(18,2) | ✅ | Privilege sum | $250.00 |
| `AdminCommissionPercent` | decimal(5,2) | ❌ | Commission % | 10.00 |
| `AdminCommissionFixed` | decimal(18,2) | ❌ | Commission $ | $30.00 |
| `VersionNumber` | int | ✅ | Plan version | 1, 2, 3 |
| `IsLatestVersion` | bool | ✅ | Is latest? | true |
| `ParentPlanId` | Guid | ❌ FK | Parent plan | For versioning |
| `IsActive` | bool | ✅ | Active? | true |
| `CreatedBy` | int | ❌ | Creator | User ID |
| `CreatedDate` | DateTime | ✅ | Created | 2025-01-01 |

---

### **Subscription Table:**

| Column | Type | Required | Description | Example |
|--------|------|----------|-------------|---------|
| `Id` | Guid | ✅ PK | Unique identifier | sub-guid-123 |
| `UserId` | int | ✅ FK | Subscriber | 789 |
| `SubscriptionPlanId` | Guid | ✅ FK | Plan | plan-guid-456 |
| `BillingCycleId` | Guid | ✅ FK | Billing cycle | monthly-guid |
| `Status` | string(50) | ✅ | Current status | "Active" |
| `StartDate` | DateTime | ✅ | Start date | 2025-10-01 |
| `EndDate` | DateTime | ❌ | End date | 2026-10-01 |
| `NextBillingDate` | DateTime | ✅ | Next bill | 2025-11-01 |
| `CurrentPrice` | decimal(18,2) | ✅ | Price | $280.00 |
| `AutoRenew` | bool | ✅ | Auto-renew? | true |
| `StripeSubscriptionId` | string(100) | ❌ | Stripe sub ID | "sub_xxx" |
| `StripeCustomerId` | string(100) | ❌ | Stripe cust ID | "cus_xxx" |
| `StripePriceId` | string(100) | ❌ | Stripe price ID | "price_xxx" |
| `PaymentMethodId` | string(100) | ❌ | Payment method | "pm_xxx" |
| `IsTrialSubscription` | bool | ✅ | Is trial? | false |
| `TrialStartDate` | DateTime | ❌ | Trial start | 2025-09-15 |
| `TrialEndDate` | DateTime | ❌ | Trial end | 2025-09-30 |

---

### **SubscriptionPlanPrivilege Table:**

| Column | Type | Required | Description | Example |
|--------|------|----------|-------------|---------|
| `Id` | Guid | ✅ PK | Unique identifier | priv-guid |
| `SubscriptionPlanId` | Guid | ✅ FK | Plan | plan-guid |
| `PrivilegeId` | Guid | ✅ FK | Privilege | teleconsult-guid |
| `UsagePeriodId` | Guid | ✅ FK | Billing cycle | monthly-guid |
| `Value` | int | ✅ | **Limit** | **5** (consultations) |
| | | | -1=unlimited | |
| | | | 0=disabled | |
| `UnitCost` | decimal(18,2) | ✅ | **Overage cost** | **$20.00** ⭐ |
| `PrivilegeBaseCost` | decimal(18,2) | ✅ | Base price cost | $20.00 |
| `DailyLimit` | int | ❌ | Daily max | 2 |
| `WeeklyLimit` | int | ❌ | Weekly max | 10 |
| `MonthlyLimit` | int | ❌ | Monthly max | 30 |
| `DurationMonths` | int | ✅ | Duration | 1 |

---

### **UserSubscriptionPrivilegeUsage Table:**

| Column | Type | Required | Description | Example |
|--------|------|----------|-------------|---------|
| `Id` | Guid | ✅ PK | Unique identifier | usage-guid |
| `SubscriptionId` | Guid | ✅ FK | Subscription | sub-guid |
| `SubscriptionPlanPrivilegeId` | Guid | ✅ FK | Plan privilege | planpriv-guid |
| `PrivilegeId` | Guid | ✅ FK | Privilege | teleconsult-guid |
| `UsedValue` | int | ✅ | **Used count** | **5** ⭐ |
| `AllowedValue` | int | ✅ | **Limit** | **6** ⭐ |
| `UsagePeriodStart` | DateTime | ✅ | Period start | 2025-10-01 |
| `UsagePeriodEnd` | DateTime | ✅ | Period end | 2025-10-31 |
| `LastUsedAt` | DateTime | ❌ | Last used | 2025-10-15 |
| `ResetAt` | DateTime | ❌ | Last reset | 2025-10-01 |

**RemainingValue (Computed):** `AllowedValue - UsedValue = 6 - 5 = 1`

---

### **BillingRecord Table:**

| Column | Type | Required | Description | Example |
|--------|------|----------|-------------|---------|
| `Id` | Guid | ✅ PK | Unique identifier | bill-guid |
| `UserId` | int | ✅ FK | User | 789 |
| `SubscriptionId` | Guid | ❌ FK | Subscription | sub-guid |
| `CurrencyId` | Guid | ✅ FK | Currency | USD-guid |
| `Amount` | decimal(18,2) | ✅ | Base amount | $20.00 |
| `TaxAmount` | decimal(18,2) | ✅ | Tax | $0.00 |
| `TotalAmount` | decimal(18,2) | ✅ | Total | $20.00 |
| `Status` | enum | ✅ | **Status** | **Paid** ⭐ |
| | | | Pending/Paid/Failed | |
| `Type` | enum | ✅ | **Type** | **Overage** ⭐ |
| | | | Subscription/Overage | |
| `Description` | string | ✅ | Description | "Purchase 1 additional..." |
| `BillingDate` | DateTime | ✅ | Billed on | 2025-10-15 |
| `DueDate` | DateTime | ✅ | Due date | 2025-10-15 |
| `PaidAt` | DateTime | ❌ | Paid on | 2025-10-15 10:30 |
| `StripeInvoiceId` | string(100) | ❌ | Stripe invoice | "in_xxx" |
| `StripePaymentIntentId` | string(100) | ❌ | Stripe payment | "pi_xxx" |

---

## 🔗 RELATIONSHIP TYPES

### **One-to-Many Relationships:**

1. **User → Subscriptions**
   - One user can have multiple subscriptions
   - User.Id ← Subscription.UserId

2. **SubscriptionPlan → Subscriptions**
   - One plan can have many subscribers
   - SubscriptionPlan.Id ← Subscription.SubscriptionPlanId

3. **SubscriptionPlan → PlanPrivileges**
   - One plan has multiple privileges
   - SubscriptionPlan.Id ← SubscriptionPlanPrivilege.SubscriptionPlanId

4. **Subscription → BillingRecords**
   - One subscription has many billing records
   - Subscription.Id ← BillingRecord.SubscriptionId

5. **Subscription → PrivilegeUsages**
   - One subscription tracks multiple privilege usages
   - Subscription.Id ← UserSubscriptionPrivilegeUsage.SubscriptionId

6. **Subscription → StatusHistory**
   - One subscription has complete status history
   - Subscription.Id ← SubscriptionStatusHistory.SubscriptionId

7. **Subscription → Payments**
   - One subscription has multiple payments
   - Subscription.Id ← SubscriptionPayment.SubscriptionId

8. **BillingRecord → Payment**
   - One billing can have one payment
   - BillingRecord.Id ← SubscriptionPayment.BillingRecordId

---

### **Many-to-Many Relationships (via Junction):**

1. **SubscriptionPlan ↔ Privilege**
   - Junction: **SubscriptionPlanPrivilege**
   - Plan.Id ↔ PlanPrivilege ↔ Privilege.Id
   - Stores: limits, costs, time restrictions

---

### **Self-Referencing Relationships:**

1. **SubscriptionPlan → ParentPlan**
   - For plan versioning
   - Plan v2 → points to Plan v1
   - Supports price change migrations

---

## 💡 CRITICAL ENTITY INSIGHTS

### **1. Why Two Cost Fields in SubscriptionPlanPrivilege?**

```csharp
public decimal PrivilegeBaseCost { get; set; }  // For base price calculation
public decimal UnitCost { get; set; }           // For overage charges
```

**Reason:** Flexibility!
- **PrivilegeBaseCost:** Used to calculate plan's base price
  - Example: 5 consultations × $15 base = $75 toward $280 plan
  
- **UnitCost:** Used for overage/extra privilege charges
  - Example: 6th consultation = $20 (charged separately)

**Your client's model:** Both are $20 for simplicity, but system supports different rates!

---

### **2. Why UsedValue and AllowedValue in UserSubscriptionPrivilegeUsage?**

```csharp
public int UsedValue { get; set; }     // Current usage count
public int AllowedValue { get; set; }  // Current limit (can increase!)
```

**Reason:** Dynamic limits!
- **UsedValue:** Tracks how many privileges consumed
- **AllowedValue:** Tracks current limit (originally from plan, but can increase when purchasing credits)

**Example:**
```
Initial State (from plan):
  AllowedValue = 5 (from SubscriptionPlanPrivilege.Value)
  UsedValue = 0
  Remaining = 5

After using 5 consultations:
  AllowedValue = 5
  UsedValue = 5
  Remaining = 0

After purchasing 1 extra credit:
  AllowedValue = 6 (increased!)
  UsedValue = 5
  Remaining = 1

After using 6th consultation:
  AllowedValue = 6
  UsedValue = 6
  Remaining = 0
```

**This is why we can't just use the plan's Value field - it's static!**

---

### **3. Why BillingType Enum?**

```csharp
public enum BillingType
{
    Subscription,  // Base subscription charge ($280)
    Overage,       // Extra privilege charge ($20)
    Consultation,  // Direct consultation charge
    Medication,    // Direct medication charge
    LateFee,       // Late payment fee
    Refund,        // Refund record
    ...
}
```

**Reason:** Revenue reporting and analytics!
- Can query: "Show all overage revenue"
- Can query: "Show subscription revenue vs overage revenue"
- Enables accurate financial reporting

**Your client's model:**
- Type=**Subscription** for $280 monthly charge
- Type=**Overage** for $20 extra consultation

---

## 🔍 COMPUTED PROPERTIES vs STORED FIELDS

### **Why Use Computed Properties?**

**Example from Subscription:**
```csharp
[NotMapped]  // Not stored in database
public int DaysUntilNextBilling => 
    (int)(NextBillingDate - DateTime.UtcNow).TotalDays;
```

**Benefits:**
- ✅ Always current (calculated real-time)
- ✅ No need to update manually
- ✅ Reduces data redundancy
- ✅ Prevents stale data

**Your entities use computed properties for:**
- `RemainingValue` = AllowedValue - UsedValue
- `IsExhausted` = UsedValue >= AllowedValue
- `IsSubscriptionActive` = Status == "Active"
- `DaysUntilNextBilling` = NextBillingDate - Now
- `EffectivePrice` = DiscountedPrice ?? Price
- `HasActiveDiscount` = Check discount validity

---

## 📚 ENTITY INHERITANCE

All entities inherit from **BaseEntity:**

```csharp
public abstract class BaseEntity
{
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
```

**Benefits:**
- ✅ Soft delete support (IsDeleted)
- ✅ Audit trail (CreatedBy, UpdatedBy)
- ✅ Timestamps (CreatedDate, UpdatedDate)
- ✅ Active/Inactive management (IsActive)

---

## 🎯 KEY TAKEAWAYS - PART 2

1. **8 Core Entities** manage subscription system
2. **SubscriptionPlanPrivilege** stores **UnitCost** ($20, $50) - critical!
3. **UserSubscriptionPrivilegeUsage** tracks usage with **dynamic AllowedValue**
4. **BillingRecord.Type** distinguishes subscription vs overage charges
5. **All relationships** properly mapped with foreign keys
6. **Computed properties** for real-time calculations
7. **BaseEntity** provides common audit fields
8. **Stripe integration fields** throughout for synchronization

---

**Continue to Part 3 for Service Layer breakdown...**


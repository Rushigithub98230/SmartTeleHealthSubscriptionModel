# Backend Subscription Management System - Complete Workflow Analysis

## Table of Contents
1. [System Overview](#system-overview)
2. [Architecture Layers](#architecture-layers)
3. [Core Entities & Relationships](#core-entities--relationships)
4. [Service Layer Breakdown](#service-layer-breakdown)
5. [Subscription Lifecycle Workflow](#subscription-lifecycle-workflow)
6. [Billing & Payment Workflow](#billing--payment-workflow)
7. [Privilege Management System](#privilege-management-system)
8. [Stripe Integration](#stripe-integration)
9. [Data Flow Diagrams](#data-flow-diagrams)
10. [Business Logic Summary](#business-logic-summary)

---

## System Overview

The SmartTelehealth backend implements a **comprehensive subscription management system** with the following key capabilities:

### Core Features
- **User Subscription Management**: Create, cancel, pause, resume, upgrade subscriptions
- **Subscription Plans**: Multiple plan types with flexible pricing and billing cycles
- **Billing System**: Automated billing, payment processing, adjustments, refunds
- **Privilege Management**: Fine-grained access control with usage tracking and limits
- **Stripe Integration**: Full Stripe payment processing with webhook synchronization
- **Trial Management**: Trial period handling with automatic conversion
- **Status Tracking**: Complete subscription lifecycle tracking with history
- **Notifications**: Automated email notifications for all subscription events

---

## Architecture Layers

The system follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           API Controllers (Controllers/)             │  │
│  │  - SubscriptionsController                           │  │
│  │  - SubscriptionPlansController                       │  │
│  │  - BillingController                                 │  │
│  │  - PaymentController                                 │  │
│  │  - StripeWebhookController                           │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                     Application Layer                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           Services (Application.Services/)           │  │
│  │  - SubscriptionService                               │  │
│  │  - SubscriptionPlanService                           │  │
│  │  - SubscriptionLifecycleService                      │  │
│  │  - BillingService                                    │  │
│  │  - PaymentService                                    │  │
│  │  - PrivilegeService                                  │  │
│  │  - SubscriptionNotificationService                   │  │
│  │  - SubscriptionAutomationService                     │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                       Domain Layer                           │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Entities (Core.Entities/)               │  │
│  │  - Subscription                                      │  │
│  │  - SubscriptionPlan                                  │  │
│  │  - SubscriptionPlanPrivilege                         │  │
│  │  - BillingRecord                                     │  │
│  │  - SubscriptionPayment                               │  │
│  │  - Privilege                                         │  │
│  │  - UserSubscriptionPrivilegeUsage                    │  │
│  │  - SubscriptionStatusHistory                         │  │
│  │  - User                                              │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      Repositories (Infrastructure.Repositories/)     │  │
│  │  - SubscriptionRepository                            │  │
│  │  - SubscriptionPlanRepository                        │  │
│  │  - BillingRepository                                 │  │
│  │  - PrivilegeRepository                               │  │
│  │  - UserSubscriptionPrivilegeUsageRepository          │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         External Services (Infrastructure/)          │  │
│  │  - StripeService (Stripe API Integration)            │  │
│  │  - EmailService (Notification delivery)              │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## Core Entities & Relationships

### Entity Relationship Diagram

```
┌──────────────────┐         ┌──────────────────────┐
│      User        │         │   SubscriptionPlan   │
│──────────────────│         │──────────────────────│
│ Id (int) PK      │         │ Id (Guid) PK         │
│ FirstName        │         │ Name                 │
│ LastName         │         │ Description          │
│ Email            │         │ Price                │
│ StripeCustomerId │         │ BillingCycleId FK    │
│ UserRoleId FK    │         │ CurrencyId FK        │
│                  │         │ CategoryId FK        │
│                  │         │ StripeProductId      │
│                  │         │ IsTrialAllowed       │
│                  │         │ TrialDurationInDays  │
└────────┬─────────┘         └──────────┬───────────┘
         │                              │
         │ 1                            │
         │                              │ 1
         │                              │
         │         ┌────────────────────┴─────────────────┐
         │         │                                      │
         │         │          Subscription                │
         └─────────┤──────────────────────────────────────│
                   │ Id (Guid) PK                         │
                   │ UserId (int) FK ──────────────────┐  │
                   │ SubscriptionPlanId (Guid) FK ─────┤  │
                   │ BillingCycleId (Guid) FK          │  │
                   │ Status (string)                   │  │
                   │ StartDate                         │  │
                   │ EndDate                           │  │
                   │ NextBillingDate                   │  │
                   │ CurrentPrice                      │  │
                   │ StripeSubscriptionId              │  │
                   │ StripeCustomerId                  │  │
                   │ StripePriceId                     │  │
                   │ IsTrialSubscription               │  │
                   │ TrialStartDate                    │  │
                   │ TrialEndDate                      │  │
                   └──────┬───────────────┬─────────────┘
                          │               │
                          │ 1             │ 1
                          │               │
                   ┌──────┴────────┐   ┌─┴──────────────────────┐
                   │               │   │                        │
              ┌────▼────────┐  ┌──▼───▼──────────────┐  ┌──────▼──────────────┐
              │BillingRecord│  │SubscriptionPayment  │  │SubscriptionStatus   │
              │─────────────│  │─────────────────────│  │History              │
              │Id (Guid) PK │  │Id (Guid) PK         │  │─────────────────────│
              │UserId FK    │  │SubscriptionId FK    │  │Id (Guid) PK         │
              │Subscription │  │CurrencyId FK        │  │SubscriptionId FK    │
              │  Id FK      │  │Amount               │  │FromStatus           │
              │Type         │  │TaxAmount            │  │ToStatus             │
              │Amount       │  │NetAmount            │  │Reason               │
              │Status       │  │Status               │  │ChangedAt            │
              │BillingDate  │  │Type                 │  │ChangedByUserId FK   │
              │StripeInvoice│  │DueDate              │  └─────────────────────┘
              │  Id         │  │PaidAt               │
              └─────────────┘  │StripePaymentIntent  │
                               │  Id                 │
                               │StripeInvoiceId      │
                               └─────────────────────┘

┌──────────────────────┐         ┌──────────────────────────┐
│     Privilege        │         │ SubscriptionPlanPrivilege│
│──────────────────────│         │──────────────────────────│
│ Id (Guid) PK         │         │ Id (Guid) PK             │
│ Name                 │◄────────┤ SubscriptionPlanId FK    │
│ Description          │         │ PrivilegeId FK           │
│ PrivilegeTypeId FK   │         │ Value (-1=unlimited)     │
└──────────┬───────────┘         │ UsagePeriodId FK         │
           │                     │ DurationMonths           │
           │                     │ DailyLimit               │
           │                     │ WeeklyLimit              │
           │                     │ MonthlyLimit             │
           │                     │ UnitCost (overage)       │
           │                     └──────────┬───────────────┘
           │                                │
           │ 1                              │ 1
           │                                │
           │         ┌──────────────────────┴───────────────────┐
           │         │                                          │
           │         │  UserSubscriptionPrivilegeUsage          │
           └─────────┤──────────────────────────────────────────│
                     │ Id (Guid) PK                             │
                     │ SubscriptionId FK                        │
                     │ SubscriptionPlanPrivilegeId FK           │
                     │ PrivilegeId FK                           │
                     │ UsedValue                                │
                     │ AllowedValue (-1=unlimited)              │
                     │ UsagePeriodStart                         │
                     │ UsagePeriodEnd                           │
                     │ LastUsedAt                               │
                     └──────────────────────────────────────────┘
```

### Entity Descriptions

#### **1. User**
- **Purpose**: Represents system users (patients, providers, admins)
- **Key Properties**: 
  - Identity fields (Id, Email, Phone, etc.)
  - Stripe integration (StripeCustomerId)
  - Role-based access (UserRoleId)
- **Relationships**: One-to-Many with Subscriptions

#### **2. SubscriptionPlan**
- **Purpose**: Defines available subscription plan templates
- **Key Properties**:
  - Pricing (Price, DiscountedPrice)
  - Billing configuration (BillingCycleId, CurrencyId)
  - Trial settings (IsTrialAllowed, TrialDurationInDays)
  - Stripe integration (StripeProductId, StripeMonthlyPriceId, etc.)
  - Plan features (MessagingCount, IncludesMedicationDelivery)
- **Relationships**: 
  - One-to-Many with Subscriptions
  - One-to-Many with SubscriptionPlanPrivileges

#### **3. Subscription**
- **Purpose**: Represents a user's active subscription instance
- **Key Properties**:
  - User and plan references (UserId, SubscriptionPlanId)
  - Status management (Status, StatusReason)
  - Dates (StartDate, EndDate, NextBillingDate)
  - Pricing (CurrentPrice)
  - Trial handling (IsTrialSubscription, TrialStartDate, TrialEndDate)
  - Stripe sync (StripeSubscriptionId, StripeCustomerId, StripePriceId)
- **Status Values**: Pending, Active, Paused, Cancelled, Expired, PaymentFailed, TrialActive, TrialExpired, Suspended
- **Relationships**:
  - Many-to-One with User and SubscriptionPlan
  - One-to-Many with BillingRecords, SubscriptionPayments, SubscriptionStatusHistory

#### **4. BillingRecord**
- **Purpose**: Tracks all billing transactions and invoices
- **Key Properties**:
  - Amount details (Amount, TaxAmount, TotalAmount)
  - Type (Subscription, Consultation, Medication, etc.)
  - Status (Pending, Paid, Failed, Cancelled, Refunded, Overdue)
  - Dates (BillingDate, DueDate, PaidAt)
  - Stripe sync (StripePaymentIntentId, StripeInvoiceId)
- **Relationships**: Many-to-One with User and Subscription

#### **5. SubscriptionPayment**
- **Purpose**: Records subscription-specific payments
- **Key Properties**:
  - Amount breakdown (Amount, TaxAmount, NetAmount)
  - Status (Pending, Processing, Succeeded, Failed, Cancelled, Refunded)
  - Type (Subscription, Trial, Setup, Upgrade, Downgrade, Refund)
  - Billing period (BillingPeriodStart, BillingPeriodEnd)
  - Stripe integration (StripePaymentIntentId, StripeInvoiceId)
- **Relationships**: Many-to-One with Subscription

#### **6. Privilege**
- **Purpose**: Defines available service privileges
- **Key Properties**:
  - Name and description
  - PrivilegeTypeId (categorization)
- **Examples**: "Teleconsultation", "Messaging", "Medication Delivery"
- **Relationships**: Many-to-Many with SubscriptionPlans via SubscriptionPlanPrivilege

#### **7. SubscriptionPlanPrivilege**
- **Purpose**: Junction table linking plans to privileges with usage limits
- **Key Properties**:
  - Value (-1 = unlimited, 0 = disabled, >0 = limited)
  - Time-based limits (DailyLimit, WeeklyLimit, MonthlyLimit)
  - Overage pricing (UnitCost)
- **Relationships**: 
  - Many-to-One with SubscriptionPlan and Privilege
  - One-to-Many with UserSubscriptionPrivilegeUsage

#### **8. UserSubscriptionPrivilegeUsage**
- **Purpose**: Tracks user's privilege consumption
- **Key Properties**:
  - UsedValue (current usage)
  - AllowedValue (maximum allowed)
  - Usage period (UsagePeriodStart, UsagePeriodEnd)
  - Tracking (LastUsedAt)
- **Computed Properties**: RemainingValue, IsExhausted, UsagePercentage
- **Relationships**: Many-to-One with Subscription, SubscriptionPlanPrivilege, Privilege

#### **9. SubscriptionStatusHistory**
- **Purpose**: Audit trail for subscription status changes
- **Key Properties**:
  - FromStatus, ToStatus
  - ChangedAt, ChangedByUserId
  - Reason, Metadata
- **Relationships**: Many-to-One with Subscription

---

## Service Layer Breakdown

### Core Services Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Service Layer                             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Subscription Management Services             │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ • SubscriptionService                                │  │
│  │   - Get subscriptions (by ID, user, filters)         │  │
│  │   - Privilege initialization                         │  │
│  │   - Access control validation                        │  │
│  │                                                       │  │
│  │ • SubscriptionLifecycleService                       │  │
│  │   - Create subscription                              │  │
│  │   - Cancel subscription                              │  │
│  │   - Pause/Resume subscription                        │  │
│  │   - Upgrade subscription                             │  │
│  │   - Renew subscription                               │  │
│  │   - Change billing cycle                             │  │
│  │                                                       │  │
│  │ • SubscriptionPlanService                            │  │
│  │   - CRUD operations for plans                        │  │
│  │   - Plan filtering and search                        │  │
│  │   - Privilege assignment to plans                    │  │
│  │   - Stripe product/price management                  │  │
│  │                                                       │  │
│  │ • SubscriptionAutomationService                      │  │
│  │   - Process trial expirations                        │  │
│  │   - Process subscription renewals                    │  │
│  │   - Handle payment failures                          │  │
│  │   - Automated notifications                          │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           Billing & Payment Services                 │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ • BillingService                                     │  │
│  │   - Create billing records                           │  │
│  │   - Get billing history                              │  │
│  │   - Filter and search billing records                │  │
│  │   - Handle billing adjustments                       │  │
│  │                                                       │  │
│  │ • PaymentService                                     │  │
│  │   - Process payments                                 │  │
│  │   - Retry failed payments                            │  │
│  │   - Process refunds                                  │  │
│  │   - Partial payment handling                         │  │
│  │                                                       │  │
│  │ • AutomatedBillingService                            │  │
│  │   - Automated subscription billing                   │  │
│  │   - Recurring payment processing                     │  │
│  │   - Failed payment retry logic                       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          Privilege Management Services               │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ • PrivilegeService                                   │  │
│  │   - Validate privilege usage                         │  │
│  │   - Check usage limits                               │  │
│  │   - Increment usage                                  │  │
│  │   - Time-based limit enforcement                     │  │
│  │   - Get remaining privileges                         │  │
│  │                                                       │  │
│  │ • PrivilegeBasedBillingService                       │  │
│  │   - Calculate overage charges                        │  │
│  │   - Process privilege-based billing                  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            Stripe Integration Services               │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ • StripeService                                      │  │
│  │   - Customer management                              │  │
│  │   - Subscription lifecycle in Stripe                 │  │
│  │   - Payment method management                        │  │
│  │   - Product and price management                     │  │
│  │   - Payment processing                               │  │
│  │                                                       │  │
│  │ • StripeSynchronizationService                       │  │
│  │   - Sync subscriptions from Stripe                   │  │
│  │   - Sync payments from Stripe                        │  │
│  │   - Handle webhook events                            │  │
│  │                                                       │  │
│  │ • WebhookIdempotencyService                          │  │
│  │   - Prevent duplicate webhook processing             │  │
│  │   - Track processed events                           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          Notification & Communication                │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ • SubscriptionNotificationService                    │  │
│  │   - Welcome notifications                            │  │
│  │   - Cancellation confirmations                       │  │
│  │   - Billing reminders                                │  │
│  │   - Trial expiration warnings                        │  │
│  │   - Payment failure alerts                           │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Service Responsibilities

#### **SubscriptionService**
**Primary Responsibilities:**
- Retrieve subscriptions (by ID, user ID, with filtering)
- Access control validation (ensure users can only access their own data)
- Privilege initialization for new subscriptions
- Subscription querying and filtering

**Key Methods:**
```csharp
- GetSubscriptionAsync(subscriptionId, tokenModel)
- GetUserSubscriptionsAsync(userId, tokenModel)
- GetUserSubscriptionsWithFilteringAsync(userId, filters, tokenModel)
- InitializeSubscriptionPrivilegesAsync(subscription, plan, tokenModel)
```

#### **SubscriptionLifecycleService**
**Primary Responsibilities:**
- Complete subscription lifecycle management
- Stripe integration for lifecycle operations
- Status transition validation
- Billing record creation for lifecycle events

**Key Methods:**
```csharp
- CreateSubscriptionAsync(createDto, tokenModel)
- CancelSubscriptionAsync(subscriptionId, reason, tokenModel)
- PauseSubscriptionAsync(subscriptionId, tokenModel)
- ResumeSubscriptionAsync(subscriptionId, tokenModel)
- UpgradeSubscriptionAsync(subscriptionId, newPlanId, tokenModel)
- RenewSubscriptionAsync(subscriptionId, tokenModel)
- ChangeBillingCycleAsync(subscriptionId, newCycleId, tokenModel)
```

**Workflow Example - Create Subscription:**
```
1. Validate subscription plan exists and is active
2. Check for duplicate subscriptions
3. Get/create Stripe customer
4. Validate payment method
5. Create Stripe subscription with correct price ID
6. Create local subscription entity with Stripe IDs
7. Handle trial logic if applicable
8. Calculate billing dates
9. Begin database transaction
10. Create subscription record
11. Create status history entry
12. Commit transaction
13. Create initial billing record
14. Initialize privileges
15. Send welcome notification
```

#### **SubscriptionPlanService**
**Primary Responsibilities:**
- Subscription plan CRUD operations
- Plan privilege management
- Stripe product/price synchronization
- Plan filtering and analytics

**Key Methods:**
```csharp
- GetPlanByIdAsync(planId, tokenModel)
- GetSubscriptionPlansWithFilteringAsync(filters, tokenModel)
- CreatePlanAsync(createDto, tokenModel)
- UpdatePlanAsync(planId, updateDto, tokenModel)
- DeletePlanAsync(planId, tokenModel)
- AddPrivilegeToPlanAsync(planId, privilegeDto, tokenModel)
- UpdatePlanPrivilegeAsync(planPrivilegeId, updateDto, tokenModel)
```

#### **BillingService**
**Primary Responsibilities:**
- Billing record management
- Payment history tracking
- Billing analytics and reporting
- Billing adjustments

**Key Methods:**
```csharp
- CreateBillingRecordAsync(createDto, tokenModel)
- GetBillingRecordAsync(billingRecordId, tokenModel)
- GetUserBillingHistoryAsync(userId, tokenModel)
- GetBillingRecordsWithFilteringAsync(filters, tokenModel)
- CreateBillingAdjustmentAsync(adjustmentDto, tokenModel)
```

#### **PaymentService**
**Primary Responsibilities:**
- Payment processing execution
- Retry logic for failed payments
- Refund processing
- Payment method management

**Key Methods:**
```csharp
- ProcessPaymentAsync(billingRecordId, tokenModel)
- RetryPaymentAsync(billingRecordId, tokenModel)
- ProcessRefundAsync(billingRecordId, amount, tokenModel)
- ProcessPartialPaymentAsync(billingRecordId, amount, tokenModel)
```

#### **PrivilegeService**
**Primary Responsibilities:**
- Privilege usage validation
- Usage limit enforcement (daily, weekly, monthly)
- Usage tracking and increment
- Remaining privilege calculation

**Key Methods:**
```csharp
- GetRemainingPrivilegeAsync(subscriptionId, privilegeName, tokenModel)
- UsePrivilegeAsync(subscriptionId, privilegeName, amount, tokenModel)
- ValidatePrivilegeUsageAsync(subscriptionId, privilegeName, amount, tokenModel)
- GetPrivilegeUsageHistoryAsync(subscriptionId, privilegeName, tokenModel)
```

**Privilege Validation Logic:**
```
1. Get subscription and validate it's active
2. Get plan privilege configuration
3. Check if privilege is disabled (Value = 0) → Deny
4. Check if privilege is unlimited (Value = -1) → Allow
5. Check time-based limits (daily, weekly, monthly)
6. Check quantity-based limits (Value > 0)
7. Get current usage for subscription
8. Calculate remaining = AllowedValue - UsedValue
9. If remaining >= requested amount → Allow, else → Deny
10. If allowed, increment usage and create history record
```

#### **StripeService**
**Primary Responsibilities:**
- All Stripe API interactions
- Customer management
- Subscription management in Stripe
- Payment method management
- Product and price management

**Key Methods:**
```csharp
- CreateCustomerAsync(email, name, tokenModel)
- CreateSubscriptionAsync(customerId, priceId, paymentMethodId, tokenModel)
- CancelSubscriptionAsync(subscriptionId, tokenModel)
- CreateProductAsync(name, description, tokenModel)
- CreatePriceAsync(productId, amount, currency, interval, tokenModel)
- ProcessPaymentAsync(paymentIntentId, tokenModel)
```

---

## Subscription Lifecycle Workflow

### Complete Subscription Creation Flow

```
┌─────────────┐
│   Client    │
│ (Frontend)  │
└──────┬──────┘
       │
       │ POST /api/subscriptions
       │ { userId, planId, billingCycleId, paymentMethodId }
       ↓
┌──────────────────────────────────────────────────────┐
│         SubscriptionsController                       │
│  CreateSubscription(CreateSubscriptionDto)           │
└──────┬───────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────┐
│      SubscriptionLifecycleService                     │
│      CreateSubscriptionAsync()                        │
├──────────────────────────────────────────────────────┤
│  1. Validate Plan Exists & Is Active                 │
│     SubscriptionPlanRepository.GetByIdAsync()        │
│                                                       │
│  2. Check Duplicate Subscription                     │
│     SubscriptionRepository.GetByUserIdAsync()        │
│     → Prevent multiple active subs for same plan     │
│                                                       │
│  3. Get User Details                                 │
│     UserService.GetUserByIdAsync()                   │
│                                                       │
│  4. Ensure Stripe Customer                           │
│     ┌────────────────────────────────────┐          │
│     │ If user.StripeCustomerId is null: │          │
│     │   StripeService.CreateCustomerAsync()         │
│     │   UserService.UpdateUserAsync()    │          │
│     │   (save StripeCustomerId to user)  │          │
│     └────────────────────────────────────┘          │
│                                                       │
│  5. Validate Payment Method                          │
│     StripeService.ValidatePaymentMethodAsync()       │
│                                                       │
│  6. Get Stripe Price ID for Billing Cycle            │
│     GetStripePriceIdForBillingCycleAsync()          │
│     → Returns StripeMonthlyPriceId,                  │
│       StripeQuarterlyPriceId, or                     │
│       StripeAnnualPriceId based on cycle             │
│                                                       │
│  7. Create Stripe Subscription                       │
│     StripeService.CreateSubscriptionAsync(           │
│       customerId,                                    │
│       priceId,                                       │
│       paymentMethodId                                │
│     )                                                │
│     → Returns StripeSubscriptionId                   │
│                                                       │
│  8. Create Local Subscription Entity                 │
│     Subscription entity = new Subscription {         │
│       UserId,                                        │
│       SubscriptionPlanId,                            │
│       BillingCycleId,                                │
│       Status = IsTrialAllowed ?                      │
│                "TrialActive" : "Active",             │
│       StartDate = DateTime.UtcNow,                   │
│       NextBillingDate = Calculated,                  │
│       CurrentPrice = plan.Price,                     │
│       StripeCustomerId,                              │
│       StripeSubscriptionId,                          │
│       StripePriceId,                                 │
│       PaymentMethodId,                               │
│       IsTrialSubscription = plan.IsTrialAllowed,     │
│       TrialStartDate,                                │
│       TrialEndDate                                   │
│     }                                                │
│                                                       │
│  9. BEGIN TRANSACTION                                │
│     UnitOfWork.BeginTransactionAsync()               │
│                                                       │
│ 10. Create Subscription in DB                        │
│     SubscriptionRepository.CreateSubscriptionAsync() │
│                                                       │
│ 11. Create Status History Entry                      │
│     SubscriptionRepository.AddStatusHistoryAsync({   │
│       FromStatus = null,                             │
│       ToStatus = "Active",                           │
│       ChangedAt = Now,                               │
│       ChangedByUserId                                │
│     })                                               │
│                                                       │
│ 12. COMMIT TRANSACTION                               │
│     UnitOfWork.CommitTransactionAsync()              │
│                                                       │
│ 13. Create Initial Billing Record                    │
│     CreateInitialBillingRecordAsync()                │
│                                                       │
│ 14. Initialize Subscription Privileges               │
│     SubscriptionService.                             │
│       InitializeSubscriptionPrivilegesAsync()        │
│     ┌────────────────────────────────────┐          │
│     │ For each plan privilege:           │          │
│     │   Create UserSubscriptionPrivilege │          │
│     │   Usage record with:               │          │
│     │   - AllowedValue = privilege.Value │          │
│     │   - UsedValue = 0                  │          │
│     │   - UsagePeriodStart/End           │          │
│     └────────────────────────────────────┘          │
│                                                       │
│ 15. Send Welcome Notification                        │
│     SubscriptionNotificationService.                 │
│       SendWelcomeEmailAsync()                        │
│                                                       │
│ 16. Return Success                                   │
│     Return SubscriptionDto                           │
└──────────────────────────────────────────────────────┘
```

### Subscription Status Transitions

```
                     ┌──────────┐
                     │ Pending  │
                     └─────┬────┘
                           │
                ┌──────────┴─────────────┐
                │                        │
                ↓                        ↓
         ┌────────────┐          ┌─────────────┐
         │   Active   │          │TrialActive  │
         └─┬──────┬──┬┘          └──────┬──────┘
           │      │  │                  │
           │      │  │                  ↓
           │      │  │          ┌────────────────┐
           │      │  │          │ TrialExpired   │
           │      │  │          └───────┬────────┘
           │      │  │                  │
           │      │  │                  └────────► Active
           │      │  │
           │      │  └─────────┐
           │      │            │
           ↓      ↓            ↓
    ┌─────────┐ ┌──────────┐ ┌──────────┐
    │ Paused  │ │PaymentFailed│Cancelled │
    └────┬────┘ └─────┬─────┘ └──────────┘
         │            │
         └─────► Active
                     │
                     ↓
              ┌──────────┐
              │ Expired  │
              └─────┬────┘
                    │
                    └─────► Active (Renew)
```

**Valid Status Transitions:**
```typescript
Pending → [Active, TrialActive, Cancelled]
Active → [Paused, Cancelled, Expired, PaymentFailed]
Paused → [Active, Cancelled, Expired]
TrialActive → [Active, TrialExpired, Cancelled]
TrialExpired → [Active, Cancelled]
Expired → [Active (renew)]
Cancelled → [None (terminal state)]
PaymentFailed → [Active, Cancelled, Expired]
```

### Cancellation Flow

```
1. Validate subscription exists and can be cancelled
2. Update subscription status to "Cancelled"
3. Set CancelledDate and CancellationReason
4. Cancel Stripe subscription
5. Create status history entry
6. Send cancellation notification
7. Return success
```

### Pause/Resume Flow

```
PAUSE:
1. Validate subscription is active
2. Update status to "Paused"
3. Set PausedDate and PauseReason
4. Pause Stripe subscription
5. Create status history entry
6. Send pause notification

RESUME:
1. Validate subscription is paused
2. Update status to "Active"
3. Set ResumedDate
4. Resume Stripe subscription
5. Recalculate NextBillingDate
6. Create status history entry
7. Send resume notification
```

### Upgrade Flow

```
1. Validate current subscription exists and is active
2. Validate new plan exists and is active
3. Get price difference for prorated billing
4. Update Stripe subscription with new price
5. Update local subscription:
   - SubscriptionPlanId = newPlanId
   - CurrentPrice = newPlan.Price
   - StripePriceId = new price ID
6. Create billing adjustment for proration
7. Update privilege allocations:
   - Remove old plan privileges
   - Add new plan privileges
   - Reset usage counters
8. Create status history entry
9. Send upgrade notification
```

---

## Billing & Payment Workflow

### Billing Record Creation

```
┌─────────────────────────────────────────────────────┐
│            Create Billing Record                     │
├─────────────────────────────────────────────────────┤
│  1. Create BillingRecord entity                     │
│     - Type (Subscription, Consultation, etc.)       │
│     - Amount, TaxAmount, TotalAmount                │
│     - Status = Pending                              │
│     - UserId, SubscriptionId                        │
│     - BillingDate = DateTime.UtcNow                 │
│     - DueDate = calculated                          │
│                                                      │
│  2. Save to database                                │
│     BillingRepository.CreateBillingRecordAsync()    │
│                                                      │
│  3. Create Stripe invoice (if needed)               │
│     StripeService.CreateInvoiceAsync()              │
│     - Save StripeInvoiceId to billing record        │
│                                                      │
│  4. Return billing record DTO                       │
└─────────────────────────────────────────────────────┘
```

### Payment Processing Flow

```
┌──────────────────────────────────────────────────────┐
│            Process Payment                            │
├──────────────────────────────────────────────────────┤
│  1. Get billing record                               │
│     BillingRepository.GetByIdAsync()                 │
│                                                       │
│  2. Validate billing record status is Pending        │
│                                                       │
│  3. Get user and Stripe customer ID                  │
│                                                       │
│  4. Create Stripe payment intent                     │
│     StripeService.CreatePaymentIntentAsync(          │
│       amount,                                        │
│       currency,                                      │
│       customerId,                                    │
│       paymentMethodId                                │
│     )                                                │
│     → Returns PaymentIntentId                        │
│                                                       │
│  5. Confirm payment intent                           │
│     StripeService.ConfirmPaymentIntentAsync()        │
│                                                       │
│  6. Update billing record                            │
│     - Status = Paid                                  │
│     - PaidAt = DateTime.UtcNow                       │
│     - StripePaymentIntentId = paymentIntentId        │
│     - TransactionId = generated                      │
│                                                       │
│  7. If subscription billing:                         │
│     Update subscription:                             │
│     - LastBillingDate = DateTime.UtcNow              │
│     - NextBillingDate = calculated                   │
│     - FailedPaymentAttempts = 0                      │
│                                                       │
│  8. Send payment confirmation notification           │
│     NotificationService.SendPaymentConfirmation()    │
│                                                       │
│  9. Return success                                   │
└──────────────────────────────────────────────────────┘
```

### Failed Payment Handling

```
┌──────────────────────────────────────────────────────┐
│         Handle Payment Failure                        │
├──────────────────────────────────────────────────────┤
│  1. Update billing record                            │
│     - Status = Failed                                │
│     - FailureReason = error message                  │
│     - LastPaymentFailedDate = DateTime.UtcNow        │
│                                                       │
│  2. Update subscription                              │
│     - FailedPaymentAttempts++                        │
│     - LastPaymentError = error message               │
│     - If attempts >= 3:                              │
│       - Status = PaymentFailed                       │
│       - Create status history entry                  │
│                                                       │
│  3. Schedule retry                                   │
│     - Calculate next retry date                      │
│     - Create retry job in AutomatedBillingService    │
│                                                       │
│  4. Send payment failure notification                │
│     NotificationService.SendPaymentFailureAlert()    │
│                                                       │
│  5. If final retry failed:                           │
│     - Send subscription suspension warning           │
│     - Update subscription status to Suspended        │
└──────────────────────────────────────────────────────┘
```

### Automated Subscription Billing

**Handled by**: `SubscriptionAutomationService`

```csharp
public async Task ProcessSubscriptionRenewalsAsync()
{
    // 1. Get all subscriptions due for renewal
    var dueSubscriptions = await _subscriptionRepository
        .GetSubscriptionsDueTodayAsync();
    
    // 2. For each subscription:
    foreach (var subscription in dueSubscriptions)
    {
        try
        {
            // 3. Create billing record
            var billingRecord = await _billingService
                .CreateBillingRecordAsync(new CreateBillingRecordDto
                {
                    UserId = subscription.UserId,
                    SubscriptionId = subscription.Id,
                    Type = BillingType.Subscription,
                    Amount = subscription.CurrentPrice,
                    Description = $"Subscription renewal for {subscription.SubscriptionPlan.Name}"
                }, systemToken);
            
            // 4. Process payment
            var paymentResult = await _paymentService
                .ProcessPaymentAsync(billingRecord.Id, systemToken);
            
            // 5. If payment successful:
            if (paymentResult.StatusCode == 200)
            {
                // Update subscription
                subscription.LastBillingDate = DateTime.UtcNow;
                subscription.NextBillingDate = CalculateNextBillingDate(
                    subscription.BillingCycleId
                );
                subscription.FailedPaymentAttempts = 0;
                
                // Send renewal confirmation
                await _notificationService.SendRenewalConfirmationAsync(
                    subscription
                );
            }
            else
            {
                // Handle failure
                await HandlePaymentFailureAsync(subscription, billingRecord);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing renewal for subscription {SubscriptionId}", subscription.Id);
        }
    }
}
```

---

## Privilege Management System

### Privilege Structure

```
┌─────────────────────────────────────────────────────────┐
│                 Privilege Hierarchy                      │
└─────────────────────────────────────────────────────────┘

MasterPrivilegeType (e.g., "Consultation Services")
    │
    ├─► Privilege: "Teleconsultation"
    │       │
    │       └─► SubscriptionPlanPrivilege (Plan: Basic)
    │               - Value: 5 (5 consultations per month)
    │               - DailyLimit: 1
    │               - WeeklyLimit: 2
    │               - MonthlyLimit: 5
    │               - UnitCost: $50 (overage price)
    │               │
    │               └─► UserSubscriptionPrivilegeUsage
    │                       - AllowedValue: 5
    │                       - UsedValue: 2
    │                       - RemainingValue: 3
    │                       - UsagePeriodStart: 2025-01-01
    │                       - UsagePeriodEnd: 2025-01-31
    │
    ├─► Privilege: "Messaging"
    │       │
    │       └─► SubscriptionPlanPrivilege (Plan: Premium)
    │               - Value: -1 (unlimited)
    │               - DailyLimit: null
    │               - UnitCost: $0
    │
    └─► Privilege: "Medication Delivery"
            │
            └─► SubscriptionPlanPrivilege (Plan: Family)
                    - Value: 12
                    - MonthlyLimit: 4
                    - UnitCost: $15
```

### Privilege Usage Flow

```
┌─────────────────────────────────────────────────────────┐
│              Use Privilege Flow                          │
├─────────────────────────────────────────────────────────┤
│  1. Get Subscription                                    │
│     SubscriptionRepository.GetByIdWithDetailsAsync()    │
│                                                          │
│  2. Validate Subscription Status                        │
│     - Must be Active or TrialActive                     │
│     - Not Deleted, Not Paused, Not Expired              │
│                                                          │
│  3. Get Plan Privilege Configuration                    │
│     SubscriptionPlanPrivilegeRepository                 │
│       .GetByPlanIdAsync()                               │
│     Find privilege by name                              │
│                                                          │
│  4. Check if Privilege is Disabled                      │
│     If Value == 0: DENY                                 │
│                                                          │
│  5. Check if Privilege is Unlimited                     │
│     If Value == -1: ALLOW (skip limit checks)           │
│                                                          │
│  6. Check Time-Based Limits                             │
│     ┌──────────────────────────────────────┐           │
│     │ If DailyLimit exists:                │           │
│     │   Get daily usage count              │           │
│     │   If dailyUsage + amount > limit:    │           │
│     │     DENY                              │           │
│     │                                       │           │
│     │ If WeeklyLimit exists:               │           │
│     │   Get weekly usage count             │           │
│     │   If weeklyUsage + amount > limit:   │           │
│     │     DENY                              │           │
│     │                                       │           │
│     │ If MonthlyLimit exists:              │           │
│     │   Get monthly usage count            │           │
│     │   If monthlyUsage + amount > limit:  │           │
│     │     DENY                              │           │
│     └──────────────────────────────────────┘           │
│                                                          │
│  7. Check Quantity-Based Limits                         │
│     Get current usage:                                  │
│       UserSubscriptionPrivilegeUsage                    │
│         .GetBySubscriptionIdAsync()                     │
│     remaining = AllowedValue - UsedValue                │
│     If remaining < amount: DENY                         │
│                                                          │
│  8. ALLOW - Increment Usage                             │
│     usage.UsedValue += amount                           │
│     usage.LastUsedAt = DateTime.UtcNow                  │
│     UserSubscriptionPrivilegeUsageRepository            │
│       .UpdateAsync(usage)                               │
│                                                          │
│  9. Create Usage History Entry                          │
│     PrivilegeUsageHistory record:                       │
│       - SubscriptionId                                  │
│       - PrivilegeId                                     │
│       - UsedAmount = amount                             │
│       - UsedAt = DateTime.UtcNow                        │
│       - RemainingAfterUse                               │
│                                                          │
│ 10. Check for Overage                                   │
│     If UsedValue > AllowedValue:                        │
│       overage = UsedValue - AllowedValue                │
│       overageCost = overage × UnitCost                  │
│       Create billing record for overage                 │
│                                                          │
│ 11. Return Success                                      │
└─────────────────────────────────────────────────────────┘
```

### Privilege Examples

**Example 1: Limited Teleconsultations**
```json
{
  "planPrivilege": {
    "privilegeName": "Teleconsultation",
    "value": 5,
    "dailyLimit": 1,
    "weeklyLimit": 2,
    "monthlyLimit": 5,
    "unitCost": 50.00
  },
  "userUsage": {
    "allowedValue": 5,
    "usedValue": 3,
    "remainingValue": 2,
    "usagePeriodStart": "2025-01-01",
    "usagePeriodEnd": "2025-01-31"
  }
}
```
- User can use 5 consultations per month
- Max 1 per day, 2 per week
- If user exceeds 5, each additional costs $50
- Currently used 3, has 2 remaining

**Example 2: Unlimited Messaging**
```json
{
  "planPrivilege": {
    "privilegeName": "Messaging",
    "value": -1,
    "dailyLimit": null,
    "weeklyLimit": null,
    "monthlyLimit": null,
    "unitCost": 0
  },
  "userUsage": {
    "allowedValue": -1,
    "usedValue": 127,
    "remainingValue": 2147483647,
    "isUnlimited": true
  }
}
```
- Unlimited messaging privilege
- No time-based limits
- No overage charges

**Example 3: Disabled Privilege**
```json
{
  "planPrivilege": {
    "privilegeName": "Home Visit",
    "value": 0,
    "message": "Not included in your plan"
  }
}
```
- Privilege is disabled for this plan
- User cannot access this feature

### Privilege Reset Logic

**Handled by**: `SubscriptionAutomationService.ProcessPrivilegeResetsAsync()`

```csharp
// Reset privileges based on usage period
public async Task ProcessPrivilegeResetsAsync()
{
    var now = DateTime.UtcNow;
    
    // Get all privilege usages that need reset
    var usagesToReset = await _usageRepo
        .GetUsagesDueForResetAsync(now);
    
    foreach (var usage in usagesToReset)
    {
        // Reset usage counter
        usage.UsedValue = 0;
        usage.ResetAt = now;
        
        // Calculate new usage period
        usage.UsagePeriodStart = now;
        usage.UsagePeriodEnd = CalculateUsagePeriodEnd(
            usage.SubscriptionPlanPrivilege.UsagePeriodId
        );
        
        await _usageRepo.UpdateAsync(usage);
    }
}
```

---

## Stripe Integration

### Stripe Webhook Flow

```
┌────────────┐                    ┌──────────────────┐
│   Stripe   │───── Webhook ─────►│  Application     │
│            │    Event             │  (Webhook        │
└────────────┘                    │   Controller)    │
                                   └────────┬─────────┘
                                            │
                            ┌───────────────┴──────────────┐
                            │                              │
                   ┌────────▼────────┐          ┌─────────▼────────┐
                   │ Verify Signature│          │Check Idempotency │
                   │ (Stripe Secret) │          │(ProcessedWebhook │
                   └────────┬────────┘          │ EventRepository) │
                            │                   └─────────┬────────┘
                            │                             │
                            │         Valid?              │
                            └────────────┬────────────────┘
                                         │
                                         ↓
                            ┌────────────────────────┐
                            │  Process Webhook Event │
                            │  (Switch on event.Type)│
                            └───────────┬────────────┘
                                        │
        ┌───────────────────────────────┼─────────────────────────────┐
        │                               │                             │
        ↓                               ↓                             ↓
┌───────────────────┐     ┌──────────────────────┐     ┌─────────────────────┐
│ Subscription      │     │ Payment Events       │     │ Customer Events     │
│ Events            │     │                      │     │                     │
├───────────────────┤     ├──────────────────────┤     ├─────────────────────┤
│• created          │     │• succeeded           │     │• updated            │
│• updated          │     │• failed              │     │• deleted            │
│• deleted          │     │• refunded            │     │• payment_method     │
│• trial_will_end   │     │• payment_intent      │     │  _attached          │
│• canceled         │     │  .succeeded          │     └─────────────────────┘
│• paused           │     │• invoice.paid        │
│• resumed          │     │• invoice.payment     │
└────────┬──────────┘     │  _failed             │
         │                └──────────┬───────────┘
         │                           │
         └───────────────┬───────────┘
                         │
                         ↓
            ┌────────────────────────┐
            │  Update Local Database │
            │  - Subscription        │
            │  - BillingRecord       │
            │  - SubscriptionPayment │
            │  - User                │
            └────────────┬───────────┘
                         │
                         ↓
            ┌────────────────────────┐
            │ Send Notifications     │
            │ (Email, SMS, Push)     │
            └────────────────────────┘
```

### Webhook Event Handlers

**1. customer.subscription.created**
```csharp
// Webhook creates or updates subscription in local DB
var subscription = await _subscriptionRepository
    .GetByStripeSubscriptionIdAsync(stripeSubscriptionId);

if (subscription == null)
{
    // Create new subscription if webhook arrives before local creation
    subscription = new Subscription
    {
        StripeSubscriptionId = stripeSubscriptionId,
        StripeCustomerId = stripeCustomerId,
        Status = MapStripeStatus(stripeSubscription.Status),
        // ... map other fields
    };
    await _subscriptionRepository.CreateSubscriptionAsync(subscription);
}
```

**2. customer.subscription.updated**
```csharp
// Update local subscription with Stripe data
subscription.Status = MapStripeStatus(stripeSubscription.Status);
subscription.CurrentPrice = stripeSubscription.Items.Data[0].Price.UnitAmount / 100m;
subscription.NextBillingDate = stripeSubscription.CurrentPeriodEnd;
await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
```

**3. invoice.payment_succeeded**
```csharp
// Update billing record to Paid
var billingRecord = await _billingRepository
    .GetByStripeInvoiceIdAsync(stripeInvoiceId);

billingRecord.Status = BillingStatus.Paid;
billingRecord.PaidAt = DateTime.UtcNow;
await _billingRepository.UpdateBillingRecordAsync(billingRecord);

// Update subscription last billing date
subscription.LastBillingDate = DateTime.UtcNow;
subscription.FailedPaymentAttempts = 0;
await _subscriptionRepository.UpdateSubscriptionAsync(subscription);

// Send payment confirmation
await _notificationService.SendPaymentConfirmationAsync(subscription);
```

**4. invoice.payment_failed**
```csharp
// Handle payment failure
billingRecord.Status = BillingStatus.Failed;
billingRecord.FailureReason = invoice.LastPaymentError?.Message;
await _billingRepository.UpdateBillingRecordAsync(billingRecord);

// Update subscription
subscription.FailedPaymentAttempts++;
subscription.LastPaymentError = invoice.LastPaymentError?.Message;

if (subscription.FailedPaymentAttempts >= 3)
{
    subscription.Status = "PaymentFailed";
    await _statusHistoryRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory
    {
        SubscriptionId = subscription.Id,
        FromStatus = "Active",
        ToStatus = "PaymentFailed",
        Reason = "3 consecutive payment failures"
    });
}

await _subscriptionRepository.UpdateSubscriptionAsync(subscription);

// Send payment failure notification
await _notificationService.SendPaymentFailureAlertAsync(subscription);
```

**5. customer.subscription.deleted**
```csharp
// Cancel subscription in local DB
subscription.Status = "Cancelled";
subscription.CancelledDate = DateTime.UtcNow;
subscription.EndDate = DateTime.UtcNow;
await _subscriptionRepository.UpdateSubscriptionAsync(subscription);

// Create status history
await _statusHistoryRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory
{
    SubscriptionId = subscription.Id,
    FromStatus = "Active",
    ToStatus = "Cancelled",
    Reason = "Cancelled via Stripe"
});

// Send cancellation notification
await _notificationService.SendCancellationConfirmationAsync(subscription);
```

### Stripe Idempotency

```csharp
public class WebhookIdempotencyService
{
    public async Task<IdempotencyResult> CheckIdempotencyAsync(string eventId, string eventType)
    {
        var processedEvent = await _repository.GetByEventIdAsync(eventId);
        
        if (processedEvent == null)
        {
            // First time seeing this event - create record
            await _repository.CreateAsync(new ProcessedWebhookEvent
            {
                EventId = eventId,
                EventType = eventType,
                Status = "Processing",
                ReceivedAt = DateTime.UtcNow,
                ProcessingAttempts = 1
            });
            
            return new IdempotencyResult 
            { 
                ShouldProcess = true, 
                IsNewEvent = true 
            };
        }
        
        if (processedEvent.Status == "Processed")
        {
            // Already processed successfully - skip
            return new IdempotencyResult 
            { 
                ShouldProcess = false, 
                Reason = "Already processed",
                IsNewEvent = false
            };
        }
        
        if (processedEvent.ProcessingAttempts >= MaxRetries)
        {
            // Too many retries - skip
            return new IdempotencyResult 
            { 
                ShouldProcess = false, 
                Reason = "Max retries exceeded",
                IsNewEvent = false
            };
        }
        
        // Increment retry counter
        processedEvent.ProcessingAttempts++;
        await _repository.UpdateAsync(processedEvent);
        
        return new IdempotencyResult 
        { 
            ShouldProcess = true, 
            IsNewEvent = false 
        };
    }
    
    public async Task MarkAsProcessedAsync(string eventId, long durationMs)
    {
        var processedEvent = await _repository.GetByEventIdAsync(eventId);
        processedEvent.Status = "Processed";
        processedEvent.ProcessedAt = DateTime.UtcNow;
        processedEvent.ProcessingDurationMs = durationMs;
        await _repository.UpdateAsync(processedEvent);
    }
}
```

---

## Data Flow Diagrams

### Complete Subscription Creation Data Flow

```
┌─────────────┐
│   Client    │
│  (Browser)  │
└──────┬──────┘
       │ 1. POST /api/subscriptions
       │    { userId, planId, billingCycleId, paymentMethodId }
       ↓
┌──────────────────────┐
│ SubscriptionsController│
└──────┬────────────────┘
       │ 2. Call CreateSubscriptionAsync()
       ↓
┌──────────────────────────┐
│ SubscriptionLifecycle    │
│ Service                  │
└──────┬───────────────────┘
       │
       │ 3. Validate Plan
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ SubscriptionPlan         │───────►│  Database   │
│ Repository               │        └─────────────┘
└──────┬───────────────────┘
       │
       │ 4. Get User
       ↓
┌──────────────────────────┐
│ UserService              │
└──────┬───────────────────┘
       │
       │ 5. Ensure Stripe Customer
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ StripeService            │───────►│   Stripe    │
│ CreateCustomerAsync()    │        │     API     │
└──────┬───────────────────┘        └─────────────┘
       │
       │ 6. Create Stripe Subscription
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ StripeService            │───────►│   Stripe    │
│ CreateSubscriptionAsync()│        │     API     │
└──────┬───────────────────┘        └─────────────┘
       │ Returns StripeSubscriptionId
       │
       │ 7. Create Local Subscription
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ Subscription             │───────►│  Database   │
│ Repository               │        └─────────────┘
│ CreateSubscriptionAsync()│
└──────┬───────────────────┘
       │
       │ 8. Create Status History
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ SubscriptionStatus       │───────►│  Database   │
│ HistoryRepository        │        └─────────────┘
└──────┬───────────────────┘
       │
       │ 9. Create Billing Record
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ BillingService           │───────►│  Database   │
│ CreateBillingRecordAsync()│        └─────────────┘
└──────┬───────────────────┘
       │
       │ 10. Initialize Privileges
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ SubscriptionService      │───────►│  Database   │
│ InitializePrivilegesAsync│        └─────────────┘
└──────┬───────────────────┘
       │
       │ 11. Send Welcome Email
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ SubscriptionNotification │───────►│    Email    │
│ Service                  │        │   Service   │
└──────┬───────────────────┘        └─────────────┘
       │
       │ 12. Return SubscriptionDto
       ↓
┌──────────────────────┐
│  Client receives:    │
│  {                   │
│    id,               │
│    status: "Active", │
│    planName,         │
│    nextBillingDate,  │
│    privileges: [...]  │
│  }                   │
└──────────────────────┘
```

### Payment Processing Data Flow

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ POST /api/billing/{id}/process-payment
       ↓
┌──────────────────────┐
│ BillingController    │
└──────┬───────────────┘
       │
       ↓
┌──────────────────────────┐
│ PaymentService           │
│ ProcessPaymentAsync()    │
└──────┬───────────────────┘
       │
       │ 1. Get Billing Record
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ BillingRepository        │───────►│  Database   │
└──────┬───────────────────┘        └─────────────┘
       │
       │ 2. Create Payment Intent
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ StripeService            │───────►│   Stripe    │
│ CreatePaymentIntent()    │        │     API     │
└──────┬───────────────────┘        └─────────────┘
       │ Returns PaymentIntentId
       │
       │ 3. Confirm Payment
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ StripeService            │───────►│   Stripe    │
│ ConfirmPaymentIntent()   │        │     API     │
└──────┬───────────────────┘        └─────────────┘
       │ Payment Success/Failure
       │
       │ 4. Update Billing Record
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ BillingRepository        │───────►│  Database   │
│ UpdateBillingRecord()    │        │ Status=Paid │
└──────┬───────────────────┘        └─────────────┘
       │
       │ 5. Update Subscription (if applicable)
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ SubscriptionRepository   │───────►│  Database   │
│ UpdateSubscription()     │        │             │
└──────┬───────────────────┘        └─────────────┘
       │
       │ 6. Send Notification
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ NotificationService      │───────►│    Email    │
│ SendPaymentConfirmation()│        │   Service   │
└──────┬───────────────────┘        └─────────────┘
       │
       │ 7. Return Success
       ↓
┌──────────────────────┐
│  Client receives:    │
│  {                   │
│    status: "Paid",   │
│    transactionId,    │
│    amount,           │
│    receiptUrl        │
│  }                   │
└──────────────────────┘
```

### Privilege Usage Data Flow

```
┌─────────────┐
│   Client    │
│ (Using      │
│  Service)   │
└──────┬──────┘
       │ Request to use teleconsultation
       ↓
┌──────────────────────────┐
│ ConsultationService      │
│ CreateConsultation()     │
└──────┬───────────────────┘
       │
       │ 1. Check Privilege
       ↓
┌──────────────────────────┐
│ PrivilegeService         │
│ UsePrivilegeAsync(       │
│   subscriptionId,        │
│   "Teleconsultation",    │
│   amount: 1              │
│ )                        │
└──────┬───────────────────┘
       │
       │ 2. Get Subscription
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ SubscriptionRepository   │───────►│  Database   │
└──────┬───────────────────┘        └─────────────┘
       │ Validate Status = Active
       │
       │ 3. Get Plan Privilege
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ SubscriptionPlanPrivilege│───────►│  Database   │
│ Repository               │        └─────────────┘
└──────┬───────────────────┘
       │ Returns: { value: 5, dailyLimit: 1, ... }
       │
       │ 4. Check Time-Based Limits
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ PrivilegeUsageHistory    │───────►│  Database   │
│ Repository               │        └─────────────┘
│ GetDailyUsageAsync()     │
└──────┬───────────────────┘
       │ Daily usage: 0 (< limit of 1) ✓
       │
       │ 5. Get Current Usage
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ UserSubscriptionPrivilege│───────►│  Database   │
│ UsageRepository          │        └─────────────┘
└──────┬───────────────────┘
       │ Returns: { allowedValue: 5, usedValue: 2 }
       │ Remaining: 3 (>= requested: 1) ✓
       │
       │ 6. INCREMENT USAGE
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ UserSubscriptionPrivilege│───────►│  Database   │
│ UsageRepository          │        │ usedValue=3 │
│ UpdateAsync()            │        └─────────────┘
└──────┬───────────────────┘
       │
       │ 7. Create Usage History
       ↓
┌──────────────────────────┐        ┌─────────────┐
│ PrivilegeUsageHistory    │───────►│  Database   │
│ Repository               │        │ New Record  │
│ CreateAsync()            │        └─────────────┘
└──────┬───────────────────┘
       │
       │ 8. Return Success (TRUE)
       ↓
┌──────────────────────────┐
│ ConsultationService      │
│ → Continue creating      │
│   consultation           │
└──────────────────────────┘
```

---

## Business Logic Summary

### Key Business Rules

#### **1. Subscription Creation**
- User can only have ONE active or paused subscription per plan
- Payment method must be valid before subscription creation
- Trial subscriptions start in "TrialActive" status
- Non-trial subscriptions start in "Active" status
- Stripe customer is created if doesn't exist
- Stripe subscription is created with appropriate price ID based on billing cycle
- Initial privileges are allocated with usage counters set to 0

#### **2. Subscription Status Transitions**
- Status transitions are validated before being allowed
- Each transition creates a status history record
- Invalid transitions are rejected (e.g., Cancelled → Active requires renewal)
- Status changes trigger notifications

#### **3. Billing Rules**
- Subscription billing occurs automatically on NextBillingDate
- NextBillingDate is calculated based on billing cycle (monthly, quarterly, annual)
- Failed payments increment FailedPaymentAttempts counter
- After 3 failed payments, subscription status changes to "PaymentFailed"
- Billing adjustments can be applied for credits, discounts, refunds

#### **4. Privilege Management**
- Privileges are tied to subscription plans, not individual subscriptions
- Each subscription gets its own usage counters initialized from plan privileges
- Value = -1 means unlimited usage
- Value = 0 means privilege is disabled
- Value > 0 means limited usage with that maximum
- Time-based limits (daily, weekly, monthly) are checked before quantity limits
- Overage charges apply when usage exceeds allowed value (if UnitCost > 0)
- Usage resets at the end of each usage period

#### **5. Trial Management**
- Trial period is defined at the plan level (IsTrialAllowed, TrialDurationInDays)
- Trial subscriptions have TrialStartDate and TrialEndDate set
- Trial status is "TrialActive" during trial period
- After trial ends, status transitions to "TrialExpired"
- User must convert to paid subscription or subscription is cancelled
- Automated job processes trial expirations daily

#### **6. Payment Processing**
- Payments are processed through Stripe
- Each payment creates a BillingRecord and optionally a SubscriptionPayment
- Payment success updates subscription LastBillingDate and resets FailedPaymentAttempts
- Payment failure records error message and increments FailedPaymentAttempts
- Retry logic attempts payment 3 times with delays
- After final failure, user is notified and subscription may be suspended

#### **7. Stripe Synchronization**
- All subscription operations are synchronized with Stripe
- Local subscription changes trigger Stripe API calls
- Stripe webhook events update local database
- Idempotency ensures webhook events are processed exactly once
- ProcessedWebhookEvent table tracks all processed webhook events

#### **8. Notifications**
- Welcome email on subscription creation
- Billing reminders before NextBillingDate (typically 3 days before)
- Payment confirmation on successful payment
- Payment failure alert on failed payment
- Trial expiration warning (7 days before, 3 days before, 1 day before)
- Cancellation confirmation
- Upgrade/downgrade confirmations

---

## Repository Layer

### Repository Pattern Implementation

All repositories inherit from `GenericRepository<TEntity>` which provides:
- Basic CRUD operations
- Soft delete support
- Audit field management (CreatedBy, CreatedDate, UpdatedBy, etc.)
- Async operations

### Key Repositories

#### **SubscriptionRepository**
```csharp
public interface ISubscriptionRepository
{
    // Basic CRUD
    Task<Subscription> GetByIdAsync(Guid id);
    Task<Subscription> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Subscription>> GetAllAsync();
    Task<Subscription> CreateSubscriptionAsync(Subscription subscription);
    Task<Subscription> UpdateSubscriptionAsync(Subscription subscription);
    Task DeleteSubscriptionAsync(Guid id);
    
    // Queries
    Task<IEnumerable<Subscription>> GetByUserIdAsync(int userId);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
    Task<IEnumerable<Subscription>> GetSubscriptionsDueTodayAsync();
    Task<IEnumerable<Subscription>> GetExpiredTrialsAsync();
    Task<(IEnumerable<Subscription>, int)> GetUserSubscriptionsWithFilteringAsync(
        int userId, 
        SubscriptionFilterDto filter
    );
    
    // Related entities
    Task<SubscriptionPlan> GetSubscriptionPlanByIdAsync(Guid planId);
    Task AddStatusHistoryAsync(SubscriptionStatusHistory history);
}
```

#### **BillingRepository**
```csharp
public interface IBillingRepository
{
    Task<BillingRecord> CreateBillingRecordAsync(BillingRecord billingRecord);
    Task<BillingRecord> GetByIdAsync(Guid id);
    Task<BillingRecord> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<BillingRecord>> GetByUserIdAsync(int userId);
    Task<IEnumerable<BillingRecord>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<BillingRecord?> GetByStripeInvoiceIdAsync(string stripeInvoiceId);
    Task<(IEnumerable<BillingRecord>, int)> GetBillingRecordsWithAdvancedFilteringAsync(
        BillingFilterDto filter
    );
    Task<BillingRecord> UpdateBillingRecordAsync(BillingRecord billingRecord);
}
```

#### **PrivilegeRepository**
```csharp
public interface IPrivilegeRepository
{
    Task<Privilege> GetByIdAsync(Guid id);
    Task<Privilege?> GetByNameAsync(string name);
    Task<IEnumerable<Privilege>> GetAllAsync();
    Task<IEnumerable<Privilege>> GetByTypeIdAsync(Guid privilegeTypeId);
}
```

#### **UserSubscriptionPrivilegeUsageRepository**
```csharp
public interface IUserSubscriptionPrivilegeUsageRepository
{
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<UserSubscriptionPrivilegeUsage> CreateAsync(UserSubscriptionPrivilegeUsage usage);
    Task<UserSubscriptionPrivilegeUsage> UpdateAsync(UserSubscriptionPrivilegeUsage usage);
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetUsagesDueForResetAsync(DateTime now);
}
```

---

## Summary

### System Highlights

**1. Comprehensive Subscription Management**
- Full subscription lifecycle (create, pause, resume, upgrade, cancel, renew)
- Multi-plan support with flexible billing cycles
- Trial period handling
- Status tracking with complete history

**2. Advanced Billing System**
- Automated recurring billing
- Multiple billing types (subscription, consultation, medication, overage)
- Billing adjustments and refunds
- Comprehensive payment retry logic

**3. Privilege-Based Access Control**
- Fine-grained privilege management
- Usage limits (unlimited, quantity-based, time-based)
- Real-time usage tracking
- Overage billing for exceeded limits

**4. Stripe Integration**
- Full Stripe payment processing
- Webhook synchronization
- Idempotent webhook processing
- Customer, subscription, and payment management

**5. Notification System**
- Email notifications for all key events
- Configurable notification preferences
- Template-based email system

**6. Audit & Compliance**
- Complete audit trails for all entities
- Status change history
- Soft delete support
- Comprehensive logging

---

### Technology Stack

**Backend Framework**: ASP.NET Core 6.0+  
**ORM**: Entity Framework Core  
**Database**: SQL Server  
**Payment Processing**: Stripe API  
**Authentication**: ASP.NET Core Identity  
**Dependency Injection**: Built-in .NET DI Container  
**Logging**: Microsoft.Extensions.Logging  
**Mapping**: AutoMapper  
**Architecture Pattern**: Clean Architecture with Repository and Service layers

---

### Key Takeaways

1. **Separation of Concerns**: Clear distinction between subscription lifecycle, billing, payment, and privilege management
2. **Stripe Synchronization**: Bidirectional sync between local DB and Stripe via API calls and webhooks
3. **Idempotency**: Webhook events processed exactly once using ProcessedWebhookEvent tracking
4. **Flexible Privilege System**: Supports unlimited, limited, disabled, and time-restricted privileges
5. **Automated Operations**: Background jobs for billing, trial expirations, payment retries
6. **Comprehensive Audit**: Complete tracking of all subscription status changes and user actions
7. **Transaction Safety**: Database transactions ensure data consistency
8. **Extensible Design**: Easy to add new subscription plans, privileges, and billing types

---

**End of Documentation**



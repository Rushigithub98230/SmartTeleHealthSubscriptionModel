# Subscription and Billing System - Complete Walkthrough

**A Comprehensive Guide to the SmartTeleHealth Subscription Management System**

**Version:** 1.0  
**Last Updated:** October 18, 2025  
**Document Type:** Technical & Business Documentation

---

## Table of Contents

1. [Introduction & System Overview](#1-introduction--system-overview)
2. [Plan Types & Examples](#2-plan-types--examples)
3. [Complete User Journey - Sarah's Story](#3-complete-user-journey---sarahs-story)
4. [Admin Workflow - Plan Creation](#4-admin-workflow---plan-creation)
5. [Technical Implementation Details](#5-technical-implementation-details)

**Note:** This document covers core subscription and billing concepts. For additional details on business rules, error handling, monitoring, verification, testing, and deployment, please refer to the supplementary documentation listed at the end of this document.

---

## 1. Introduction & System Overview

### Executive Summary

The SmartTeleHealth Subscription Management System is a comprehensive, production-ready platform that enables healthcare providers to offer flexible subscription-based telehealth services. The system provides robust billing automation, intelligent privilege management, and seamless payment processing through Stripe integration.

**Key Highlights:**
- **Flexible Billing Cycles:** Support for Monthly, Quarterly, and Annual billing with automatic privilege scaling
- **Revenue Protection:** Prevents under-charging and ensures accurate billing across all billing cycles
- **Automated Operations:** Background services handle recurring billing, payment retries, and privilege resets
- **Usage Tracking:** Real-time monitoring of privilege consumption with overage support
- **Fair Pricing:** Automatic discount application based on billing cycle commitments

### Key Features and Capabilities

#### 1. Subscription Plan Management
- Admin-created plans with customizable pricing and privileges
- Support for multiple billing cycles per plan (Monthly, Quarterly, Annual)
- Billing cycle-specific discounts (e.g., 10% off annual billing)
- Privilege limits that automatically scale to billing cycles
- Plan versioning and pricing history

#### 2. Intelligent Billing System
- **Accurate Billing:** Price = Monthly Price × (Billing Cycle Days ÷ 30) - Discount
- **Privilege Scaling:** Allowed = Monthly Limit × (Billing Cycle Days ÷ 30), rounded up
- **Automated Recurring Billing:** Daily background service processes renewals
- **Overage Support:** Users can exceed limits with automatic billing
- **Payment Retry Logic:** 3 automatic attempts with exponential backoff
- **Migration Support:** Automatically corrects pricing for existing subscriptions

#### 3. Payment Processing
- Stripe integration for secure payment processing
- Multiple payment methods: Cards, ACH, Apple Pay, Google Pay
- Transaction-safe operations with rollback support
- Automatic payment retry with intelligent scheduling
- Subscription suspension after failed attempts

#### 4. Privilege Management
- Real-time usage tracking for all privileges
- Automatic allocation based on billing cycle
- Usage period aligned with subscription billing dates
- Reset only when payment succeeds (not time-based)
- Overage detection and billing

#### 5. Monitoring and Alerts
- Background service monitors expired privileges
- Email notifications for all billing events
- Payment failure alerts with retry schedule
- Subscription suspension notifications
- Admin dashboard for revenue tracking

### Technology Stack

**Backend Framework:**
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- C# 12.0

**Database:**
- SQL Server (LocalDB for development)
- Entity Framework Code-First migrations

**Payment Processing:**
- Stripe API v2023+
- Stripe webhooks for event handling
- PCI DSS compliant (Stripe handles card data)

**Architecture Patterns:**
- Clean Architecture with clear separation of concerns
- Repository Pattern for data access
- Unit of Work for transaction management
- Dependency Injection throughout
- Background Services for automation

**Key Libraries:**
- AutoMapper for DTO mapping
- Serilog for structured logging
- FluentValidation for input validation

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                        │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │    API Controllers (REST Endpoints)                      │  │
│  │    - SubscriptionPlansController                         │  │
│  │    - SubscriptionsController                             │  │
│  │    - BillingController                                   │  │
│  │    - PaymentController                                   │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────────┐
│                       APPLICATION LAYER                          │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │    Business Services                                     │  │
│  │    - SubscriptionPlanService                             │  │
│  │    - SubscriptionLifecycleService                        │  │
│  │    - PaymentService                                      │  │
│  │    - AutomatedBillingService                             │  │
│  │    - PrivilegeService                                    │  │
│  │    - SubscriptionBillingService                          │  │
│  │    - BillingCycleValidator                               │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────────┐
│                          CORE LAYER                              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │    Domain Entities                                       │  │
│  │    - Subscription                                        │  │
│  │    - SubscriptionPlan                                    │  │
│  │    - BillingRecord                                       │  │
│  │    - SubscriptionPayment                                 │  │
│  │    - UserSubscriptionPrivilegeUsage                      │  │
│  │    - MasterBillingCycle                                  │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │    Interfaces (Contracts)                                │  │
│  │    - Repository Interfaces                               │  │
│  │    - Service Interfaces                                  │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                        │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │    Data Access (Repositories)                            │  │
│  │    - SubscriptionRepository                              │  │
│  │    - BillingRepository                                   │  │
│  │    - SubscriptionPaymentRepository                       │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │    External Services                                     │  │
│  │    - StripeBillingService                                │  │
│  │    - NotificationService                                 │  │
│  │    - EmailService                                        │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │    Background Services                                   │  │
│  │    - PrivilegeResetBackgroundService                     │  │
│  │    - AutomatedBillingService (scheduled tasks)           │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────────┐
│                      EXTERNAL SYSTEMS                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │   Stripe     │  │  SQL Server  │  │  SMTP Server │          │
│  │   Payment    │  │   Database   │  │   (Email)    │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
```

### Data Flow Overview

```
User Action → API Controller → Application Service → Repository → Database
                    ↓                    ↓
              Validation          Business Logic
                    ↓                    ↓
           DTO Mapping         External Services (Stripe)
                    ↓                    ↓
             Response          Transaction Commit/Rollback
```

---

## 2. Plan Types & Examples

This section demonstrates the types of subscription plans you can create in the system, showing how pricing, privileges, and billing cycles work together.

### Plan Example 1: Basic Care Plan

**Target Audience:** Individual users seeking basic telehealth services

```
┌────────────────────────────────────────────────────────────────┐
│                       BASIC CARE PLAN                           │
├────────────────────────────────────────────────────────────────┤
│  Base Monthly Price: $50                                       │
│  Plan Type: Individual                                         │
│  Category: Personal Healthcare                                 │
├────────────────────────────────────────────────────────────────┤
│  INCLUDED PRIVILEGES (Per Month):                              │
│  ├─ Video Consultations: 3 per month                          │
│  ├─ Chat Messages: 50 per month                               │
│  ├─ Document Uploads: 5 per month                             │
│  ├─ Prescription Refills: 2 per month                         │
│  └─ Health Reports: 1 per month                               │
├────────────────────────────────────────────────────────────────┤
│  BILLING CYCLE OPTIONS:                                        │
│                                                                │
│  ┌─ MONTHLY BILLING ─────────────────────────────────────┐    │
│  │  Price: $50/month                                     │    │
│  │  Discount: 0%                                         │    │
│  │  Privileges: As listed above                          │    │
│  │  Total Yearly Cost: $600                              │    │
│  └───────────────────────────────────────────────────────┘    │
│                                                                │
│  ┌─ QUARTERLY BILLING ───────────────────────────────────┐    │
│  │  Base Price: $50 × 3 months = $150                    │    │
│  │  Discount: 5% ($7.50)                                 │    │
│  │  Final Price: $142.50 per quarter                     │    │
│  │  Privileges Scaled:                                   │    │
│  │    • Video Consultations: 3 × 3 = 9 per quarter      │    │
│  │    • Chat Messages: 50 × 3 = 150 per quarter         │    │
│  │    • Document Uploads: 5 × 3 = 15 per quarter        │    │
│  │    • Prescription Refills: 2 × 3 = 6 per quarter     │    │
│  │    • Health Reports: 1 × 3 = 3 per quarter           │    │
│  │  Total Yearly Cost: $570 (save $30/year)             │    │
│  └───────────────────────────────────────────────────────┘    │
│                                                                │
│  ┌─ ANNUAL BILLING ──────────────────────────────────────┐    │
│  │  Base Price: $50 × 12 months = $600                   │    │
│  │  Discount: 10% ($60)                                  │    │
│  │  Final Price: $540 per year                           │    │
│  │  Privileges Scaled:                                   │    │
│  │    • Video Consultations: 3 × 12 = 36 per year       │    │
│  │    • Chat Messages: 50 × 12 = 600 per year           │    │
│  │    • Document Uploads: 5 × 12 = 60 per year          │    │
│  │    • Prescription Refills: 2 × 12 = 24 per year      │    │
│  │    • Health Reports: 1 × 12 = 12 per year            │    │
│  │  Total Yearly Cost: $540 (save $60/year)             │    │
│  └───────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────────┘
```

**How Privilege Scaling Works:**
When a user selects **Annual Billing** for the Basic Care Plan:
- System calculates: Billing Cycle Days (365) ÷ 30 = 12.17 months
- Video Consultations: 3/month × 12.17 = 36.5 → **37 consultations for the year**
- The system uses `Math.Ceiling()` to always round up, ensuring users get fair allocation
- These 37 consultations are valid for the **entire year** (not reset monthly)
- Resets only when the user pays for the next year

### Plan Example 2: Family Care Plan

**Target Audience:** Families needing comprehensive healthcare coverage

```
┌────────────────────────────────────────────────────────────────┐
│                      FAMILY CARE PLAN                           │
├────────────────────────────────────────────────────────────────┤
│  Base Monthly Price: $150                                      │
│  Plan Type: Family                                             │
│  Category: Family Healthcare                                   │
├────────────────────────────────────────────────────────────────┤
│  INCLUDED PRIVILEGES (Per Month):                              │
│  ├─ Video Consultations: 10 per month                         │
│  ├─ Chat Messages: Unlimited                                  │
│  ├─ Document Uploads: 20 per month                            │
│  ├─ Prescription Refills: 10 per month                        │
│  ├─ Health Reports: 5 per month                               │
│  └─ Specialist Consultations: 2 per month                     │
├────────────────────────────────────────────────────────────────┤
│  BILLING CYCLE OPTIONS:                                        │
│                                                                │
│  ┌─ MONTHLY BILLING ─────────────────────────────────────┐    │
│  │  Price: $150/month                                    │    │
│  │  Discount: 0%                                         │    │
│  │  Total Yearly Cost: $1,800                            │    │
│  └───────────────────────────────────────────────────────┘    │
│                                                                │
│  ┌─ ANNUAL BILLING ──────────────────────────────────────┐    │
│  │  Base Price: $150 × 12 months = $1,800                │    │
│  │  Discount: 15% ($270)                                 │    │
│  │  Final Price: $1,530 per year                         │    │
│  │  Privileges Scaled:                                   │    │
│  │    • Video Consultations: 10 × 12 = 120 per year     │    │
│  │    • Chat Messages: Unlimited                         │    │
│  │    • Document Uploads: 20 × 12 = 240 per year        │    │
│  │    • Prescription Refills: 10 × 12 = 120 per year    │    │
│  │    • Health Reports: 5 × 12 = 60 per year            │    │
│  │    • Specialist Consultations: 2 × 12 = 24/year      │    │
│  │  Total Yearly Cost: $1,530 (save $270/year)          │    │
│  └───────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────────┘
```

**Special Note on Unlimited Privileges:**
- Chat Messages are marked as "Unlimited" (stored as `-1` in database)
- System does not track or limit usage for unlimited privileges
- No overage charges possible for unlimited privileges
- Ideal for features with low infrastructure cost (like text messages)

### Plan Example 3: Corporate Wellness Plan

**Target Audience:** Organizations providing employee healthcare benefits

```
┌────────────────────────────────────────────────────────────────┐
│                  CORPORATE WELLNESS PLAN                        │
├────────────────────────────────────────────────────────────────┤
│  Base Monthly Price: $1,000                                    │
│  Plan Type: Enterprise                                         │
│  Category: Corporate Healthcare                                │
│  Target: 50 employees per organization                         │
├────────────────────────────────────────────────────────────────┤
│  INCLUDED PRIVILEGES (Per Month):                              │
│  ├─ Video Consultations: 100 per month                        │
│  ├─ Chat Messages: Unlimited                                  │
│  ├─ Document Uploads: Unlimited                               │
│  ├─ Prescription Refills: 50 per month                        │
│  ├─ Health Reports: 50 per month                              │
│  ├─ Specialist Consultations: 20 per month                    │
│  └─ Mental Health Sessions: 30 per month                      │
├────────────────────────────────────────────────────────────────┤
│  BILLING CYCLE OPTIONS:                                        │
│  (Monthly billing NOT available for enterprise plans)          │
│                                                                │
│  ┌─ QUARTERLY BILLING ───────────────────────────────────┐    │
│  │  Base Price: $1,000 × 3 months = $3,000              │    │
│  │  Discount: 8% ($240)                                  │    │
│  │  Final Price: $2,760 per quarter                     │    │
│  │  Privileges Scaled:                                   │    │
│  │    • Video Consultations: 100 × 3 = 300 /quarter    │    │
│  │    • Prescription Refills: 50 × 3 = 150 /quarter    │    │
│  │    • Health Reports: 50 × 3 = 150 /quarter          │    │
│  │    • Specialist: 20 × 3 = 60 /quarter               │    │
│  │    • Mental Health: 30 × 3 = 90 /quarter            │    │
│  │  Total Yearly Cost: $11,040 (save $960/year)        │    │
│  └───────────────────────────────────────────────────────┘    │
│                                                                │
│  ┌─ ANNUAL BILLING ──────────────────────────────────────┐    │
│  │  Base Price: $1,000 × 12 months = $12,000            │    │
│  │  Discount: 20% ($2,400)                               │    │
│  │  Final Price: $9,600 per year                         │    │
│  │  Privileges Scaled:                                   │    │
│  │    • Video Consultations: 100 × 12 = 1,200/year     │    │
│  │    • Prescription Refills: 50 × 12 = 600 /year      │    │
│  │    • Health Reports: 50 × 12 = 600 /year            │    │
│  │    • Specialist: 20 × 12 = 240 /year                │    │
│  │    • Mental Health: 30 × 12 = 360 /year             │    │
│  │  Total Yearly Cost: $9,600 (save $2,400/year)       │    │
│  └───────────────────────────────────────────────────────┘    │
├────────────────────────────────────────────────────────────────┤
│  BILLING CYCLE RESTRICTIONS:                                   │
│  BillingCycleValidator enforces:                               │
│  - No daily/weekly billing for enterprise plans               │
│  - Prevents excessive transaction overhead                     │
│  - Minimum commitment: Quarterly                               │
└────────────────────────────────────────────────────────────────┘
```

**Cost Comparison vs. Individual Plans:**
If 50 employees each had individual Family Care Plans ($1,530/year):
- Total Cost: 50 × $1,530 = **$76,500/year**
- Corporate Plan Cost: **$9,600/year**
- **Savings: $66,900/year (87% reduction)**

### Key Concepts Illustrated

**1. Privilege Scaling Formula:**
```
Allowed Privileges = Monthly Limit × (Billing Cycle Days ÷ 30)
Result = Math.Ceiling(Calculated Value)
```

**Example:**
- Monthly Limit: 10 consultations
- Billing Cycle: Annual (365 days)
- Calculation: 10 × (365 ÷ 30) = 10 × 12.17 = 121.7
- Final: Math.Ceiling(121.7) = **122 consultations for the year**

**2. Billing Amount Formula:**
```
Base Amount = Monthly Price × (Billing Cycle Days ÷ 30)
Discount Amount = Base Amount × (Discount Percentage ÷ 100)
Final Amount = Base Amount - Discount Amount
```

**Example (Exact Calculation - As Used by Backend):**
- Monthly Price: $150
- Billing Cycle: Annual (365 days)
- Months in cycle: 365 ÷ 30 = 12.166...
- Base: $150 × 12.166 = $1,825
- Discount: $1,825 × 0.15 = $273.75
- Final: $1,825 - $273.75 = **$1,551.25**

**Note:** Throughout this document, we use simplified examples with rounded values (e.g., 12 months for annual) for easier understanding. The actual backend system uses precise calculations as shown above.

**3. Why This Approach is Fair:**
- Users paying for 365 days get privileges for 365 days
- No artificial monthly resets that cheat annual subscribers
- Discounts incentivize longer commitments
- System prevents revenue loss from billing cycle mismatches

---

## 3. Complete User Journey - Sarah's Story

This section walks through a complete, real-world example from purchase to renewal, including overage scenarios.

**Meet Sarah:** A 35-year-old mother who needs reliable telehealth services for her family.

### Scene 1: Subscription Purchase (January 1, 2025 - Day 1)

**Frontend: Plan Selection**
- Sarah visits the site and selects "Family Care" plan ($150/month base)
- She chooses **Annual Billing** for the 15% discount
- System shows: $1,530/year (save $270!) *[Simplified example; actual calculation may vary slightly]*
- Privileges: 122 video consultations (scaled: 10/month × 12.17 months), unlimited chat, 244 uploads for the full year

**API Call:**
```
POST /api/Subscriptions
Body: { userId:12345, planId:"family-care-guid", billingCycleId:"annual-guid", paymentMethodId:"pm_..." }
```

**Service Processing** (`SubscriptionLifecycleService.CreateSubscriptionAsync()` - Line 85):
1. **Validate Plan** (Line 90): Check plan exists and is active ✓
2. **Prevent Duplicates** (Line 98): Ensure no active subscription for same plan ✓
3. **Billing Cycle Validation** (Line 161): `BillingCycleValidator.IsValidBillingCycleForPlan()` ✓
4. **Calculate Price** (Line 170-180):
   - Base: $150 × (365÷30) = $150 × 12.17 = $1,825
   - Discount: 15% = $273.75
   - Final: $1,530
5. **Create Subscription** with Status: `PendingPayment`, NextBillingDate: Jan 1, 2026
6. **Calculate Privileges** (`PrivilegeService.CalculatePrivilegeAllocationAsync()` - Line 1207):
   - Video: 10/mo × 12.17 = 122 (rounded up to 122 for year)
   - Uploads: 20/mo × 12.17 = 244 (rounded to 244 for year)
7. **Insert Records**:
   ```sql
   INSERT INTO Subscriptions (...) VALUES (CurrentPrice:1530, NextBillingDate:'2026-01-01', Status:'PendingPayment');
   INSERT INTO UserSubscriptionPrivilegeUsages (...) VALUES (AllowedValue:122, UsedValue:0, UsagePeriodEnd:'2026-01-01');
   ```

**Payment Processing** (`PaymentService.ProcessPaymentAsync()` - Line 78):
1. Create `BillingRecord` (Amount: $1,530, Type: Subscription)
2. Create `SubscriptionPayment` record
3. Call `StripeBillingService.ProcessStripePaymentAsync()`
4. Stripe charges card → Success!
5. **Transaction Commit** (`UpdatePaymentRecordsAsync()` - Line 1125):
   ```csharp
   using var transaction = await _unitOfWork.BeginTransactionAsync();
   billingRecord.Status = Paid;
   subscriptionPayment.Status = Completed;
   subscription.Status = Active;
   subscription.LastBillingDate = Now;
   await _unitOfWork.CommitTransactionAsync();
   ```

**Result**: Subscription active, Sarah has 122 consultations valid until Jan 1, 2026.

---

### Scene 2: Using the Subscription (January 15, 2025 - Day 15)

**Frontend: Usage Dashboard**
```
Video Consultations: █░░░░░░░░░░░░░░ 0/122 (0%)
Chat Messages: Unlimited (45 sent)
Document Uploads: ░░░░░░░░░░░░░░ 0/244 (0%)
```

Sarah books a video consultation for her daughter's fever.

**API Call:**
```
POST /api/Privileges/use
Body: { subscriptionId:"sub-guid", privilegeName:"Video Consultation", amount:1 }
```

**Service** (`PrivilegeService.UsePrivilegeAsync()` - Line 232):
1. **Get Usage Record** (Line 260):
   ```csharp
   var usage = await _usageRepo.GetBySubscriptionAndPrivilegeAsync(subscriptionId, privilegeId);
   // Current: AllowedValue=122, UsedValue=0
   ```
2. **Check Availability** (Line 290):
   ```csharp
   if (usage.UsedValue + amount > usage.AllowedValue) return false; // 0+1 < 122 ✓
   ```
3. **Update Usage** (Line 300):
   ```sql
   UPDATE UserSubscriptionPrivilegeUsages 
   SET UsedValue = 1, LastUsedAt = GETUTCDATE()
   WHERE SubscriptionId = 'sub-guid' AND PrivilegeId = 'video-guid';
   ```
4. **Record History**:
   ```sql
   INSERT INTO PrivilegeUsageHistory (AmountUsed:1, Timestamp:Now, Description:'Video consultation');
   ```

**Result**: Consultation proceeds. Dashboard now shows 1/122 used.

---

### Scene 3: Overage Scenario (December 15, 2025 - Day 350)

Heavy year for Sarah's family - they've used all 122 consultations. Son gets sick, needs one more.

**Frontend: Overage Popup**
```
⚠️ Plan Limit Reached
You've used all 122 consultations.
Additional consultation: $25

[Cancel] [Pay & Continue]
```

Sarah clicks "Pay & Continue"

**Service** (`AutomatedBillingService.ProcessOverageChargesAsync()` - Line 1667):
1. **Calculate Overage**:
   ```csharp
   var overage = usedValue - allowedValue; // 123 - 122 = 1
   var overageAmount = overage * privilegeOveragePrice; // 1 × $25 = $25
   ```
2. **Create Overage Billing** (`CreateOverageBillingRecordAsync()` - Line 1583):
   ```sql
   INSERT INTO BillingRecords (
       Amount:25, Type:Overage, Status:Pending,
       Description:'1 additional video consultation'
   );
   ```
3. **Process Payment** (`PaymentService.ProcessPaymentAsync()`):
   - Charge $25 via Stripe
   - Update records to `Paid`
4. **Allow Privilege**:
   ```sql
   UPDATE UserSubscriptionPrivilegeUsages SET UsedValue = 123;
   ```

**Result**: Consultation proceeds, $25 charged, Sarah's dashboard shows 123/122 (1 over).

---

### Scene 4: Automated Recurring Billing (January 1, 2026 - Day 365)

**Background Service**: `AutomatedBillingService` runs at 2:00 AM daily

**Processing** (`ProcessSubscriptionBillingAsync()`):
1. **Find Due Subscriptions** (Line 618):
   ```csharp
   var dueSubscriptions = await _subscriptionRepo.GetSubscriptionsDueForBilling(DateTime.UtcNow);
   // Found: Sarah's subscription (NextBillingDate = Jan 1, 2026)
   ```

2. **Price Migration Check** (`MigrateSubscriptionPricingIfNeededAsync()` - Line 577):
   ```csharp
   var expectedPrice = CalculateBillingAmountAsync(subscription);
   if (subscription.CurrentPrice != expectedPrice) {
       subscription.CurrentPrice = expectedPrice;
       // Auto-corrects any pricing misalignment
   }
   ```

3. **Calculate Billing Amount** (`CalculateBillingAmountAsync()` - Line 932):
   ```csharp
   var monthlyPrice = plan.Price; // $150
   var monthsInCycle = billingCycleDays / 30.0m; // 12.17
   var basePrice = monthlyPrice * monthsInCycle; // $1,825
   var billingCycleDiscount = CalculateBillingCycleDiscount(plan, billingCycle, basePrice);
   // Annual: 15% of $1,825 = $273.75
   var finalPrice = basePrice - billingCycleDiscount; // $1,551.25 → $1,530
   ```

4. **Create Billing Record**:
   ```sql
   INSERT INTO BillingRecords (Amount:1530, Type:Recurring, DueDate:'2026-01-01');
   ```

5. **Process Payment** (`PaymentService.ProcessPaymentAsync()`):
   - Charge saved card via Stripe

**Two Scenarios:**

**A) Payment Succeeds** ✅

```csharp
// UpdatePaymentRecordsAsync() - Line 1120
using var transaction = await _unitOfWork.BeginTransactionAsync();

// Update billing/payment records
billingRecord.Status = Paid;
subscriptionPayment.Status = Completed;

// Update subscription
subscription.LastBillingDate = DateTime.UtcNow; // Jan 1, 2026
subscription.NextBillingDate = DateTime.UtcNow.AddDays(365); // Jan 1, 2027
subscription.Status = Active;

// RESET PRIVILEGES (Line 1179)
await ResetPrivilegesForNewBillingPeriodAsync(subscription, tokenModel);
```

**Privilege Reset** (`ResetPrivilegesForNewBillingPeriodAsync()` - Line 1197):
```csharp
var usageRecords = await _subscriptionRepo.GetSubscriptionPrivilegeUsagesAsync(subscription.Id);
foreach (var usage in usageRecords) {
    var planPrivilege = subscription.SubscriptionPlan.PlanPrivileges
        .FirstOrDefault(p => p.Id == usage.SubscriptionPlanPrivilegeId);
    
    var monthlyLimit = planPrivilege.MonthlyLimit; // 10
    var monthsInCycle = billingCycleDays / 30.0m; // 12.17
    var allowedForCycle = (int)Math.Ceiling(monthlyLimit * monthsInCycle); // 122
    
    usage.UsedValue = 0; // Reset to 0
    usage.AllowedValue = allowedForCycle; // 122 for next year
    usage.UsagePeriodStart = subscription.LastBillingDate.AddDays(1); // Jan 2, 2026
    usage.UsagePeriodEnd = subscription.NextBillingDate; // Jan 1, 2027
    
    await _subscriptionRepository.UpdatePrivilegeUsageAsync(usage);
}
```

**Email Sent**:
```
Subject: Subscription Renewed!

Hi Sarah,
Your Family Care subscription has been renewed.
Charged: $1,530.00
Valid Until: January 1, 2027

Your privileges have been reset:
• 122 Video Consultations
• 244 Document Uploads
... (all refreshed for the new year)
```

**B) Payment Fails** ❌

```csharp
// UpdatePaymentRecordsAsync()
billingRecord.Status = Failed;
subscriptionPayment.Status = Failed;
subscriptionPayment.NextRetryAt = DateTime.UtcNow.AddDays(1); // Retry tomorrow
subscriptionPayment.AttemptCount = 1;
subscription.Status = PastDue;
```

**Retry Logic** (`ProcessFailedPaymentRetryAsync()`):
- **Day 1 (Jan 2)**: Retry #1 → Fails → Schedule Jan 3
- **Day 2 (Jan 3)**: Retry #2 → Fails → Schedule Jan 4
- **Day 3 (Jan 4)**: Retry #3 → **Final attempt**
  - If succeeds → Activate subscription ✅
  - If fails → `HandleMaxRetriesExceededAsync()`:
    ```csharp
    subscription.Status = Suspended;
    subscriptionPayment.Status = Failed;
    // Email: "Subscription Suspended - Update payment method"
    ```

**Email Sent** (on failure):
```
Subject: ⚠️ Payment Failed

Hi Sarah,
We couldn't process your payment for $1,530.
We'll retry automatically tomorrow.
Please update your payment method to avoid service interruption.
```

---

## 4. Admin Workflow - Plan Creation

This section shows how administrators create and configure subscription plans.

### Step 1: Create Base Plan

**Frontend: Admin Dashboard**
```
Admin Dashboard → Subscription Plans → Create New Plan

┌────────────────────────────────────────────┐
│  CREATE SUBSCRIPTION PLAN                  │
├────────────────────────────────────────────┤
│  Plan Name: Family Care                    │
│  Category: Healthcare Plans                │
│  Description: Comprehensive family health   │
│                                            │
│  Base Monthly Price: $150                  │
│                                            │
│  Billing Cycle Discounts:                  │
│  Monthly: 0%                               │
│  Quarterly: 5%                             │
│  Annual: 15%                               │
│                                            │
│  [Next: Add Privileges →]                  │
└────────────────────────────────────────────┘
```

**API Call:**
```
POST /api/SubscriptionPlans
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "Family Care",
  "description": "Comprehensive family healthcare coverage",
  "price": 150.00,
  "categoryId": "healthcare-category-guid",
  "monthlyBillingDiscount": 0,
  "quarterlyBillingDiscount": 5.00,
  "annualBillingDiscount": 15.00,
  "isActive": false
}
```

**Controller:** `SubscriptionPlansController`  
**File:** `backend/SmartTelehealth.API/Controllers/SubscriptionPlansController.cs`

**Service:** `SubscriptionPlanService`  
**File:** `backend/SmartTelehealth.Application/Services/SubscriptionPlanService.cs`  
**Method:** Creates plan with base configuration

**Database:**
```sql
INSERT INTO SubscriptionPlans (
    Id, Name, Description, Price,
    MonthlyBillingDiscount, QuarterlyBillingDiscount, AnnualBillingDiscount,
    IsActive, CreatedDate, CreatedBy
) VALUES (
    NEWID(), 'Family Care', 'Comprehensive...', 150.00,
    0.00, 5.00, 15.00,
    0, GETUTCDATE(), @adminUserId
);
```

### Step 2: Add Privileges

**Frontend:**
```
┌────────────────────────────────────────────┐
│  CONFIGURE PRIVILEGES - Family Care        │
├────────────────────────────────────────────┤
│  Available Privileges:                     │
│                                            │
│  ☑ Video Consultation                     │
│     Monthly Limit: [10] consultations      │
│     Overage Price: [$25] per additional    │
│                                            │
│  ☑ Chat Messages                          │
│     Monthly Limit: [-1] (Unlimited)        │
│                                            │
│  ☑ Document Upload                        │
│     Monthly Limit: [20] uploads            │
│     Overage Price: [$5] per additional     │
│                                            │
│  ☑ Prescription Refill                    │
│     Monthly Limit: [10] refills            │
│     Overage Price: [$15] per additional    │
│                                            │
│  [Save Privileges]                         │
└────────────────────────────────────────────┘
```

**API Call:**
```
POST /api/SubscriptionPlans/{planId}/privileges
Body: {
  "privilegeId": "video-consult-guid",
  "monthlyLimit": 10,
  "value": 10,
  "overagePrice": 25.00,
  "isUnlimited": false
}
```

**Processing:**
- System creates `SubscriptionPlanPrivilege` records
- Links privileges to the plan
- Sets monthly limits (system will auto-scale based on billing cycle)

**Key Concept:** Admin sets **monthly** limits. System automatically scales:
- Quarterly: 10 × 3 = 30
- Annual: 10 × 12 = 120

### Step 3: Configure Billing Cycles

**Service:** `BillingCycleValidator`  
**File:** `backend/SmartTelehealth.Application/Services/BillingCycleValidator.cs`  
**Method:** `IsValidBillingCycleForPlan()` (Line 17)

```csharp
public static bool IsValidBillingCycleForPlan(
    SubscriptionPlan plan, MasterBillingCycle billingCycle)
{
    var planMonthlyPrice = plan.Price; // $150
    
    // Business rules:
    if (billingCycle.Name == "Daily" && planMonthlyPrice > 50)
        return false; // Too expensive for daily billing
    
    return billingCycle.Name.ToLower() switch {
        "monthly" => true,
        "quarterly" => true,
        "annual" or "yearly" => true,
        "weekly" => planMonthlyPrice <= 100,
        "daily" => planMonthlyPrice <= 50,
        _ => false
    };
}
```

For Family Care ($150/month):
- ✅ Monthly allowed
- ✅ Quarterly allowed  
- ✅ Annual allowed
- ❌ Weekly NOT allowed (price > $100)
- ❌ Daily NOT allowed (price > $50)

### Step 4: Stripe Integration

**API Call:**
```
POST /api/Stripe/products
Body: {
  "planId": "family-care-guid",
  "productName": "Family Care",
  "description": "Comprehensive family healthcare"
}
```

**Service:** `StripeService.CreateProductAsync()`

```csharp
// Create Stripe Product
var product = await _stripeClient.Products.CreateAsync(new ProductCreateOptions {
    Name = "Family Care",
    Description = "Comprehensive family healthcare coverage",
    Metadata = new Dictionary<string, string> {
        { "PlanId", planId.ToString() }
    }
});

// Create Stripe Prices for each billing cycle
var prices = new List<Price>();

// Monthly price
prices.Add(await _stripeClient.Prices.CreateAsync(new PriceCreateOptions {
    Product = product.Id,
    Currency = "usd",
    UnitAmount = 15000, // $150.00
    Recurring = new PriceRecurringOptions { Interval = "month" }
}));

// Annual price (with discount)
prices.Add(await _stripeClient.Prices.CreateAsync(new PriceCreateOptions {
    Product = product.Id,
    Currency = "usd",
    UnitAmount = 153000, // $1,530.00
    Recurring = new PriceRecurringOptions { Interval = "year" }
}));

// Save Stripe IDs back to plan
plan.StripeProductId = product.Id;
plan.StripePriceMonthlyId = prices[0].Id;
plan.StripePriceAnnualId = prices[1].Id;
await _planRepository.UpdateAsync(plan);
```

### Step 5: Activate Plan

**API Call:**
```
PUT /api/SubscriptionPlans/{planId}
Body: { "isActive": true }
```

**Processing:**
1. Validate all privileges configured ✓
2. Validate Stripe integration complete ✓
3. Set `IsActive = true`
4. Plan now visible via `GET /api/SubscriptionPlans/active`

**Result:** Family Care plan is live and available for user subscriptions!

---

## 5. Technical Implementation Details

This section provides deep technical insights into the services, database schema, and API endpoints.

### 5.1 Service Architecture

The system uses 9 core services that work together to manage subscriptions, billing, and payments.

#### **1. SubscriptionPlanService**

**File:** `backend/SmartTelehealth.Application/Services/SubscriptionPlanService.cs`  
**Purpose:** Manage subscription plans (CRUD operations, privilege configuration)

**Key Methods:**
- `GetActivePlansAsync(page, pageSize, filters)` - Fetch plans for public browsing
- `GetPlanByIdAsync(planId)` - Get detailed plan information
- `CreatePlanAsync(createDto)` - Admin creates new plan
- `UpdatePlanAsync(planId, updateDto)` - Modify existing plan
- `AddPrivilegeToPlanAsync(planId, privilegeDto)` - Add privilege to plan
- `RemovePrivilegeFromPlanAsync(planId, privilegeId)` - Remove privilege
- `GetPlanAnalyticsAsync(planId)` - Get plan subscription statistics

**Dependencies:**
- ISubscriptionPlanRepository
- ISubscriptionPlanPrivilegeRepository
- ICategoryService
- IStripeService
- IMapper, ILogger

---

#### **2. SubscriptionLifecycleService**

**File:** `backend/SmartTelehealth.Application/Services/SubscriptionLifecycleService.cs`  
**Purpose:** Manage complete subscription lifecycle from creation to cancellation

**Key Methods:**

**CreateSubscriptionAsync()** - Line 85
- Validates plan and billing cycle
- Ensures Stripe customer exists
- Line 161: Uses `BillingCycleValidator.IsValidBillingCycleForPlan()`
- Line 170-180: Calculates `CurrentPrice` with scaling and discounts
- Creates subscription with calculated NextBillingDate
- Allocates privileges using `PrivilegeService.CalculatePrivilegeAllocationAsync()`

**CancelSubscriptionAsync(subscriptionId)** - Immediate or end-of-period cancellation
**PauseSubscriptionAsync(subscriptionId)** - Temporary suspension
**ResumeSubscriptionAsync(subscriptionId)** - Reactivate paused subscription
**UpgradeSubscriptionAsync(subscriptionId, newPlanId)** - Upgrade with proration
**DowngradeSubscriptionAsync(subscriptionId, newPlanId)** - Downgrade at period end

**Dependencies:**
- ISubscriptionRepository
- ISubscriptionPlanRepository
- IStripeService
- IPrivilegeService
- INotificationService
- ISubscriptionBillingService
- IUnitOfWork

---

#### **3. PaymentService**

**File:** `backend/SmartTelehealth.Application/Services/PaymentService.cs`  
**Purpose:** Process all payments through Stripe with transaction safety

**Key Methods:**

**ProcessPaymentAsync(billingRecordId, tokenModel)** - Line 78
```csharp
// Main payment processing flow
1. Retrieve billing record
2. Create/Get SubscriptionPayment record (Line 95)
3. Call StripeBillingService.ProcessStripePaymentAsync() (Line 110)
4. UpdatePaymentRecordsAsync() - Transaction-safe updates (Line 120)
```

**UpdatePaymentRecordsAsync()** - Line 1120
```csharp
// Transaction-safe record updates
using var transaction = await _unitOfWork.BeginTransactionAsync();
try {
    // Update BillingRecord status
    // Update SubscriptionPayment status
    // Update Subscription (if recurring billing)
    // Reset privileges if payment succeeded (Line 1179)
    await _unitOfWork.CommitTransactionAsync();
}
catch { await _unitOfWork.RollbackTransactionAsync(); throw; }
```

**ResetPrivilegesForNewBillingPeriodAsync(subscription, tokenModel)** - Line 1197
```csharp
// Called when billing succeeds - resets privilege usage
var usageRecords = await _subscriptionRepo.GetSubscriptionPrivilegeUsagesAsync(subscription.Id);
foreach (var usage in usageRecords) {
    var monthlyLimit = planPrivilege.MonthlyLimit;
    var monthsInCycle = billingCycleDays / 30.0m;
    var allowedForCycle = (int)Math.Ceiling(monthlyLimit * monthsInCycle);
    
    usage.UsedValue = 0;  // Reset usage
    usage.AllowedValue = allowedForCycle;  // Recalculate for new period
    usage.UsagePeriodStart = subscription.LastBillingDate.AddDays(1);
    usage.UsagePeriodEnd = subscription.NextBillingDate;
}
```

**Dependencies:**
- ISubscriptionPaymentRepository
- ISubscriptionRepository
- IBillingRepository
- IStripeBillingService
- IUnitOfWork

---

#### **4. AutomatedBillingService**

**File:** `backend/SmartTelehealth.Application/Services/AutomatedBillingService.cs`  
**Purpose:** Automated billing operations (recurring, overage, retries)

**Key Methods:**

**ProcessSubscriptionBillingAsync(subscription, tokenModel)** - Line 618
```csharp
// Called by daily background job for subscriptions due for billing
await MigrateSubscriptionPricingIfNeededAsync(subscription, tokenModel);  // Auto-fix pricing
var billingAmount = await CalculateBillingAmountAsync(subscription, tokenModel);
var billingRecord = await CreateBillingRecordAsync(subscription, billingAmount);
var paymentResult = await _billingService.ProcessPaymentAsync(billingRecord.Id, tokenModel);
```

**CalculateBillingAmountAsync(subscription, tokenModel)** - Line 932
```csharp
// Calculates accurate billing amount with scaling and discounts
var monthlyPrice = plan.Price;
var billingCycleDays = subscription.BillingCycle.DurationInDays;
var monthsInCycle = billingCycleDays / 30.0m;

var basePrice = monthlyPrice * monthsInCycle;
var billingCycleDiscount = CalculateBillingCycleDiscount(plan, billingCycle, basePrice);
var additionalDiscounts = await CalculateDiscountAmountAsync(subscription, tokenModel);
var adjustments = await CalculateAdjustmentAmountAsync(subscription, tokenModel);

return Math.Max(basePrice - billingCycleDiscount - additionalDiscounts + adjustments, 0.01m);
```

**CalculateBillingCycleDiscount(plan, billingCycle, basePrice)** - Line 969
```csharp
// Applies billing cycle-specific discounts
var discountPercent = billingCycle.Name.ToLower() switch {
    "annual" or "yearly" => plan.AnnualBillingDiscount,
    "quarterly" => plan.QuarterlyBillingDiscount,
    "monthly" => plan.MonthlyBillingDiscount,
    _ => 0m
};
return basePrice * (discountPercent / 100);
```

**MigrateSubscriptionPricingIfNeededAsync(subscription, tokenModel)** - Line 577
```csharp
// Auto-corrects pricing for existing subscriptions
var expectedPrice = await CalculateBillingAmountAsync(subscription, tokenModel);
if (Math.Abs(subscription.CurrentPrice - expectedPrice) > 0.01m) {
    _logger.LogInformation("Migrating subscription {Id} pricing from {Old} to {New}",
        subscription.Id, subscription.CurrentPrice, expectedPrice);
    subscription.CurrentPrice = expectedPrice;
    await _subscriptionRepo.UpdateAsync(subscription);
}
```

**ProcessOverageChargesAsync(subscription, tokenModel)** - Line 1667
```csharp
// Handles overage billing when users exceed limits
var usages = await GetPrivilegeUsagesAsync(subscription.Id);
decimal totalOverageAmount = 0;

foreach (var usage in usages.Where(u => u.UsedValue > u.AllowedValue)) {
    var overage = usage.UsedValue - usage.AllowedValue;
    var privilegePrice = usage.SubscriptionPlanPrivilege.OveragePrice;
    totalOverageAmount += overage * privilegePrice;
}

if (totalOverageAmount > 0) {
    var billingRecordId = await CreateOverageBillingRecordAsync(subscription, totalOverageAmount, tokenModel);
    var paymentResult = await _billingService.ProcessPaymentAsync(billingRecordId.Value, tokenModel);
    return paymentResult.StatusCode == 200;
}
```

**ProcessFailedPaymentRetryAsync(tokenModel)**
```csharp
// Retries failed payments with exponential backoff
var paymentsToRetry = await _subscriptionPaymentRepository
    .GetFailedPaymentsDueForRetryAsync(DateTime.UtcNow, 100);

foreach (var payment in paymentsToRetry) {
    if (payment.AttemptCount >= 3) {
        await HandleMaxRetriesExceededAsync(payment, tokenModel);
        continue;
    }
    
    var result = await _billingService.ProcessPaymentAsync(payment.BillingRecordId, tokenModel);
}
```

**Dependencies:**
- ISubscriptionRepository
- IBillingRepository
- ISubscriptionPaymentRepository
- ISubscriptionBillingService (for CreateSubscriptionBillingAsync, ProcessPaymentAsync)
- INotificationService

---

#### **5. PrivilegeService**

**File:** `backend/SmartTelehealth.Application/Services/PrivilegeService.cs`  
**Purpose:** Manage privilege usage and allocation with billing cycle awareness

**Key Methods:**

**UsePrivilegeAsync(subscriptionId, privilegeName, amount, tokenModel)** - Line 232
```csharp
// Consumes privilege with real-time checking
var subscription = await _subscriptionRepo.GetByIdWithDetailsAsync(subscriptionId);
var planPrivilege = subscription.SubscriptionPlan.PlanPrivileges
    .FirstOrDefault(p => p.Privilege.Name == privilegeName);

var usage = await _usageRepo.GetBySubscriptionAndPrivilegeAsync(subscriptionId, planPrivilege.Id);

if (usage == null) {
    // First time using - calculate allocation dynamically (Line 248)
    var (allowedValue, periodStart, periodEnd) = 
        await CalculatePrivilegeAllocationAsync(subscriptionId, planPrivilege);
    
    usage = new UserSubscriptionPrivilegeUsage {
        AllowedValue = allowedValue,
        UsedValue = amount,
        UsagePeriodStart = periodStart,
        UsagePeriodEnd = periodEnd
    };
}
else {
    // Check if exceeds limit (Line 290)
    if (usage.AllowedValue != -1 && usage.UsedValue + amount > usage.AllowedValue) {
        return false;  // Will trigger overage flow
    }
    usage.UsedValue += amount;
}

await _usageRepo.UpdateAsync(usage);
await _historyRepo.AddAsync(new PrivilegeUsageHistory { /* ... */ });
```

**CalculatePrivilegeAllocationAsync(subscriptionId, planPrivilege)** - Line 1207
```csharp
// Dynamically calculates privilege allocation based on billing cycle
private async Task<(int allowedValue, DateTime periodStart, DateTime periodEnd)> 
    CalculatePrivilegeAllocationAsync(Guid subscriptionId, SubscriptionPlanPrivilege planPrivilege)
{
    var subscription = await _subscriptionRepo.GetByIdWithDetailsAsync(subscriptionId);
    var billingCycleDays = subscription.BillingCycle.DurationInDays;
    var monthsInCycle = billingCycleDays / 30.0m;
    
    var monthlyLimit = planPrivilege.MonthlyLimit ?? planPrivilege.Value;
    
    // Calculate allowed for the billing cycle
    var allowedForCycle = monthlyLimit == -1 
        ? -1  // Unlimited
        : (int)Math.Ceiling(monthlyLimit * monthsInCycle);
    
    // Set period aligned with subscription billing dates
    var periodStart = subscription.LastBillingDate?.AddDays(1) ?? subscription.StartDate;
    var periodEnd = subscription.NextBillingDate;
    
    return (allowedForCycle, periodStart, periodEnd);
}
```

**CheckPrivilegeAvailabilityAsync(subscriptionId, privilegeName, amount)**
```csharp
// Checks if user has enough privileges before use
var usage = await _usageRepo.GetBySubscriptionAndPrivilegeAsync(subscriptionId, privilegeId);
if (usage.AllowedValue == -1) return true;  // Unlimited
var remaining = usage.AllowedValue - usage.UsedValue;
return remaining >= amount;
```

**Dependencies:**
- ISubscriptionRepository
- IUserSubscriptionPrivilegeUsageRepository
- IPrivilegeUsageHistoryRepository
- ISubscriptionPlanPrivilegeRepository

---

#### **6. SubscriptionBillingService**

**File:** `backend/SmartTelehealth.Application/Services/SubscriptionBillingService.cs`  
**Purpose:** Create and manage billing records (consolidated billing operations)

**Key Methods:**
- `CreateSubscriptionBillingAsync(subscriptionId, amount, type)` - Create billing record
- `GetBillingHistoryAsync(userId, filters)` - Fetch user's billing history
- `GetBillingRecordByIdAsync(billingRecordId)` - Get specific record
- `CalculateNextBillingDate(currentDate, billingCycle)` - Calculate next billing date
- `UpdateBillingRecordStatusAsync(recordId, status)` - Update billing status

---

#### **7. StripeBillingService**

**File:** `backend/SmartTelehealth.Infrastructure/Services/StripeBillingService.cs`  
**Purpose:** Direct Stripe API integration for payment processing

**Key Methods:**

**ProcessStripePaymentAsync(billingRecordId, tokenModel)**
```csharp
// Charges customer via Stripe
var billingRecord = await _billingRepo.GetByIdAsync(billingRecordId);
var subscription = await _subscriptionRepo.GetByIdAsync(billingRecord.SubscriptionId.Value);

var paymentIntent = await _stripeClient.PaymentIntents.CreateAsync(new PaymentIntentCreateOptions {
    Amount = (long)(billingRecord.Amount * 100),  // Convert to cents
    Currency = "usd",
    Customer = subscription.StripeCustomerId,
    PaymentMethod = subscription.StripePaymentMethodId,
    Confirm = true,
    OffSession = true,  // For recurring payments
    Description = $"{billingRecord.Type} - Subscription {subscription.Id}"
});

return new JsonModel {
    StatusCode = paymentIntent.Status == "succeeded" ? 200 : 400,
    data = new { StripePaymentIntentId = paymentIntent.Id }
};
```

**CreatePaymentIntentAsync(amount, customerId, paymentMethodId)**
**ValidatePaymentMethodAsync(paymentMethodId, tokenModel)**
**CreateProductAsync(productName, description)**
**CreatePriceAsync(productId, amount, interval)**

**Dependencies:**
- Stripe.net SDK
- IBillingRepository
- ISubscriptionRepository

---

#### **8. BillingCycleValidator**

**File:** `backend/SmartTelehealth.Application/Services/BillingCycleValidator.cs`  
**Purpose:** Static validation class for billing cycle selections

**Key Method:**

**IsValidBillingCycleForPlan(plan, billingCycle)** - Line 17
```csharp
public static bool IsValidBillingCycleForPlan(SubscriptionPlan plan, MasterBillingCycle billingCycle)
{
    var planMonthlyPrice = plan.Price;
    
    // Business rules to prevent inappropriate billing cycles
    if (billingCycle.Name.Equals("Daily", StringComparison.OrdinalIgnoreCase) && planMonthlyPrice > 50)
        return false;  // Too many transactions for expensive plans
    
    return billingCycle.Name.ToLower() switch {
        "monthly" => true,
        "quarterly" => true,
        "annual" or "yearly" => true,
        "weekly" => planMonthlyPrice <= 100,   // Only for cheaper plans
        "daily" => planMonthlyPrice <= 50,      // Only for very cheap plans
        _ => false
    };
}
```

**No Dependencies** - Pure business logic validation

---

#### **9. PrivilegeResetBackgroundService**

**File:** `backend/SmartTelehealth.Infrastructure/Services/PrivilegeResetBackgroundService.cs`  
**Purpose:** Background monitoring service for expired privileges (runs every 24 hours)

**Key Method:**

**ExecuteAsync(CancellationToken)** - Line 27
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("Privilege Reset Background Service started");
    
    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);  // Wait for app startup
    
    while (!stoppingToken.IsCancellationRequested) {
        try {
            await CheckExpiredPrivilegeUsagesAsync(stoppingToken);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error in privilege reset background service");
        }
        
        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);  // Run daily
    }
}
```

**CheckExpiredPrivilegeUsagesAsync(stoppingToken)** - Line 52
```csharp
// Finds and logs expired privilege usages for admin review
var now = DateTime.UtcNow;
var expiredUsages = await _context.UserSubscriptionPrivilegeUsages
    .Include(u => u.SubscriptionPlanPrivilege)
        .ThenInclude(p => p.Privilege)
    .Where(u => u.UsagePeriodEnd < now && u.UsedValue > 0)
    .Take(100)
    .ToListAsync(stoppingToken);

if (expiredUsages.Any()) {
    _logger.LogWarning(
        "Found {Count} expired privilege usages. " +
        "These should reset on next successful billing.",
        expiredUsages.Count);
}
```

**Note:** This service does NOT perform resets. Resets happen in `PaymentService.ResetPrivilegesForNewBillingPeriodAsync()` when billing succeeds.

**Dependencies:**
- ApplicationDbContext (direct EF access for monitoring)
- ILogger

---

### 5.2 Database Schema

Key tables and their relationships in the subscription system:

**Subscriptions Table**
```sql
CREATE TABLE Subscriptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId INT NOT NULL,
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    BillingCycleId UNIQUEIDENTIFIER NOT NULL,
    CurrentPrice DECIMAL(18,2) NOT NULL,  -- Scaled price for billing cycle
    StartDate DATETIME2 NOT NULL,
    NextBillingDate DATETIME2 NOT NULL,  -- When next billing occurs
    LastBillingDate DATETIME2 NULL,  -- When last billed
    Status NVARCHAR(50) NOT NULL,  -- Active, PastDue, Suspended, Cancelled, etc.
    IsActive BIT NOT NULL,
    StripeCustomerId NVARCHAR(255),
    StripeSubscriptionId NVARCHAR(255),
    StripePaymentMethodId NVARCHAR(255),
    
    FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id),
    FOREIGN KEY (BillingCycleId) REFERENCES MasterBillingCycles(Id),
    INDEX IX_Subscriptions_UserId (UserId),
    INDEX IX_Subscriptions_NextBillingDate (NextBillingDate)
);
```

**SubscriptionPlans Table**
```sql
CREATE TABLE SubscriptionPlans (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,  -- Base monthly price
    MonthlyBillingDiscount DECIMAL(5,2) DEFAULT 0,  -- % discount for monthly
    QuarterlyBillingDiscount DECIMAL(5,2) DEFAULT 0,  -- % discount for quarterly
    AnnualBillingDiscount DECIMAL(5,2) DEFAULT 0,  -- % discount for annual
    IsActive BIT NOT NULL,
    StripeProductId NVARCHAR(255),
    
    INDEX IX_SubscriptionPlans_IsActive (IsActive)
);
```

**MasterBillingCycles Table**
```sql
CREATE TABLE MasterBillingCycles (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,  -- Monthly, Quarterly, Annual, etc.
    DurationInDays INT NOT NULL,  -- 30, 90, 365, etc.
    IsActive BIT NOT NULL
);
```

**UserSubscriptionPrivilegeUsages Table**
```sql
CREATE TABLE UserSubscriptionPrivilegeUsages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionPlanPrivilegeId UNIQUEIDENTIFIER NOT NULL,
    AllowedValue INT NOT NULL,  -- Scaled to billing cycle, -1 = unlimited
    UsedValue INT NOT NULL DEFAULT 0,  -- Current usage
    UsagePeriodStart DATETIME2 NOT NULL,  -- Aligned with LastBillingDate
    UsagePeriodEnd DATETIME2 NOT NULL,  -- Aligned with NextBillingDate
    LastUsedAt DATETIME2 NULL,
    
    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    FOREIGN KEY (SubscriptionPlanPrivilegeId) REFERENCES SubscriptionPlanPrivileges(Id),
    INDEX IX_UserSubscriptionPrivilegeUsages_Subscription (SubscriptionId),
    INDEX IX_UserSubscriptionPrivilegeUsages_PeriodEnd (UsagePeriodEnd)
);
```

**BillingRecords Table**
```sql
CREATE TABLE BillingRecords (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NULL,
    UserId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Type INT NOT NULL,  -- 0=Subscription, 1=Overage, 2=Recurring, 3=Upfront, etc.
    Status INT NOT NULL,  -- 0=Pending, 1=Paid, 2=Failed, 3=Cancelled
    DueDate DATETIME2 NOT NULL,
    PaidDate DATETIME2 NULL,
    Description NVARCHAR(MAX),
    StripePaymentIntentId NVARCHAR(255),
    
    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    INDEX IX_BillingRecords_Status (Status),
    INDEX IX_BillingRecords_DueDate (DueDate)
);
```

**SubscriptionPayments Table**
```sql
CREATE TABLE SubscriptionPayments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    BillingRecordId UNIQUEIDENTIFIER NOT NULL,  -- Links to billing record
    Amount DECIMAL(18,2) NOT NULL,
    Type INT NOT NULL,  -- Subscription, Overage, Upfront, Recurring, etc.
    Status INT NOT NULL,  -- Pending, Completed, Failed, Refunded
    DueDate DATETIME2 NOT NULL,
    PaidDate DATETIME2 NULL,
    BillingPeriodStart DATETIME2 NOT NULL,  -- Period this payment covers
    BillingPeriodEnd DATETIME2 NOT NULL,
    StripePaymentIntentId NVARCHAR(255),
    NextRetryAt DATETIME2 NULL,  -- For failed payment retries
    AttemptCount INT DEFAULT 0,  -- Retry attempt tracking
    
    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    FOREIGN KEY (BillingRecordId) REFERENCES BillingRecords(Id),
    INDEX IX_SubscriptionPayments_BillingRecordId (BillingRecordId),
    INDEX IX_SubscriptionPayments_NextRetryAt (NextRetryAt, Status)
);
```

**Key Relationships:**
```
Subscriptions
  ├─ Many-to-One → SubscriptionPlans
  ├─ Many-to-One → MasterBillingCycles
  ├─ One-to-Many → UserSubscriptionPrivilegeUsages
  ├─ One-to-Many → BillingRecords
  └─ One-to-Many → SubscriptionPayments

SubscriptionPlans
  └─ One-to-Many → SubscriptionPlanPrivileges
      └─ Many-to-One → Privileges

BillingRecords
  └─ One-to-One → SubscriptionPayments (via BillingRecordId)
```

---

### 5.3 API Endpoints Reference

Complete list of available API endpoints organized by category.

#### Plan Management Endpoints

**Browse Active Plans (Public)**
```
GET /api/SubscriptionPlans/active?page=1&pageSize=10&categoryId=guid&searchTerm=family
Authorization: None (Public endpoint)
Response: 200 OK with paginated plan list
```

**Get Plan Details**
```
GET /api/SubscriptionPlans/{planId}
Authorization: Bearer <token>
Response: 200 OK with plan details including privileges
```

**Create Plan (Admin)**
```
POST /api/SubscriptionPlans
Authorization: Bearer <admin-token>
Body: { name, description, price, categoryId, discounts, ... }
Response: 201 Created with plan ID
```

**Update Plan (Admin)**
```
PUT /api/SubscriptionPlans/{planId}
Authorization: Bearer <admin-token>
Body: { name, price, isActive, discounts, ... }
Response: 200 OK
```

**Add Privilege to Plan (Admin)**
```
POST /api/SubscriptionPlans/{planId}/privileges
Authorization: Bearer <admin-token>
Body: { privilegeId, monthlyLimit, overagePrice, isUnlimited }
Response: 200 OK
```

**Remove Privilege from Plan (Admin)**
```
DELETE /api/SubscriptionPlans/{planId}/privileges/{privilegeId}
Authorization: Bearer <admin-token>
Response: 200 OK
```

#### Subscription Management Endpoints

**Create Subscription (User Purchase)**
```
POST /api/Subscriptions
Authorization: Bearer <user-token>
Body: {
  userId: 12345,
  planId: "plan-guid",
  billingCycleId: "annual-guid",
  paymentMethodId: "pm_123"
}
Response: 201 Created with subscription details
```

**Get Subscription Details**
```
GET /api/Subscriptions/{subscriptionId}
Authorization: Bearer <user-token>
Response: 200 OK with subscription, plan, and usage details
```

**Get User's Subscriptions**
```
GET /api/Subscriptions/user/{userId}
Authorization: Bearer <user-token>
Response: 200 OK with list of user's subscriptions (all statuses)
```

**Cancel Subscription**
```
POST /api/Subscriptions/{subscriptionId}/cancel
Authorization: Bearer <user-token>
Body: { reason: "No longer needed", cancelAtPeriodEnd: true }
Response: 200 OK
```

**Pause Subscription**
```
POST /api/Subscriptions/{subscriptionId}/pause
Authorization: Bearer <user-token>
Body: { reason: "Temporary pause" }
Response: 200 OK
```

**Resume Subscription**
```
POST /api/Subscriptions/{subscriptionId}/resume
Authorization: Bearer <user-token>
Response: 200 OK
```

**Upgrade Subscription**
```
POST /api/Subscriptions/{subscriptionId}/upgrade
Authorization: Bearer <user-token>
Body: { newPlanId: "premium-plan-guid", prorationBehavior: "create_prorations" }
Response: 200 OK with proration details
```

#### Billing & Payment Endpoints

**Get Billing History**
```
GET /api/Billing/records?userId=12345&page=1&pageSize=10&status=Paid&startDate=2025-01-01
Authorization: Bearer <user-token>
Response: 200 OK with paginated billing records
```

**Get Billing Record Details**
```
GET /api/Billing/records/{recordId}
Authorization: Bearer <user-token>
Response: 200 OK with billing record details
```

**Process Payment**
```
POST /api/Payment/process
Authorization: Bearer <user-token>
Body: { billingRecordId: "billing-guid" }
Response: 200 OK if payment succeeds
```

**Get Payment Methods**
```
GET /api/Payment/methods?userId=12345
Authorization: Bearer <user-token>
Response: 200 OK with list of saved payment methods
```

**Add Payment Method**
```
POST /api/Payment/methods
Authorization: Bearer <user-token>
Body: { stripePaymentMethodId: "pm_123", setAsDefault: true }
Response: 200 OK
```

**Update Default Payment Method**
```
PUT /api/Payment/methods/default
Authorization: Bearer <user-token>
Body: { paymentMethodId: "pm_456" }
Response: 200 OK
```

#### Privilege Usage Endpoints

**Use Privilege (Consume)**
```
POST /api/Privileges/use
Authorization: Bearer <user-token>
Body: {
  subscriptionId: "sub-guid",
  privilegeName: "Video Consultation",
  amount: 1
}
Response: 200 OK if successful, 400 if insufficient privileges
```

**Check Privilege Availability**
```
GET /api/Privileges/availability?subscriptionId=sub-guid&privilegeName=Video%20Consultation&amount=1
Authorization: Bearer <user-token>
Response: 200 OK with { available: true/false, remaining: number }
```

**Get Subscription Privilege Usage**
```
GET /api/Privileges/usage/{subscriptionId}
Authorization: Bearer <user-token>
Response: 200 OK with list of all privileges and their usage
Example Response:
{
  "privileges": [
    {
      "name": "Video Consultation",
      "allowedValue": 120,
      "usedValue": 45,
      "remaining": 75,
      "usagePercent": 37.5,
      "periodStart": "2025-01-01",
      "periodEnd": "2026-01-01",
      "lastUsedAt": "2025-10-15"
    }
  ]
}
```

**Get Privilege Usage History**
```
GET /api/Privileges/history?subscriptionId=sub-guid&page=1&pageSize=20
Authorization: Bearer <user-token>
Response: 200 OK with paginated usage history
```

#### Stripe Webhook Endpoints

**Handle Stripe Webhooks**
```
POST /api/StripeWebhook
Authorization: None (Stripe signature verification)
Body: Stripe event payload
Response: 200 OK

Handled Events:
- payment_intent.succeeded
- payment_intent.payment_failed
- customer.subscription.updated
- customer.subscription.deleted
- invoice.payment_succeeded
- invoice.payment_failed
```

---




## Document Status

✅ **Core Documentation Complete** - This comprehensive walkthrough includes all essential sections for understanding the subscription and billing system.

For complete details on business rules, error handling, monitoring, and flow diagrams, refer to:
- IMPLEMENTATION_VERIFICATION_SUMMARY.md
- DEPLOYMENT_COMPLETE.md
- backend/Scripts/VerifyBillingAlignment.sql

---

*Document Version: 1.0 | Last Updated: October 18, 2025 | Status: Production Ready*

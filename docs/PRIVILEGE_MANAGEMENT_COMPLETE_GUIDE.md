# Privilege Management System - Complete Technical Guide

**SmartTeleHealth Subscription Management System**

**Version:** 1.0  
**Last Updated:** October 18, 2025  
**Document Type:** Technical Reference & Implementation Guide

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Core Concepts](#2-core-concepts)
3. [Entity Architecture](#3-entity-architecture)
4. [Service Architecture](#4-service-architecture)
5. [Privilege Lifecycle](#5-privilege-lifecycle)
6. [Usage Tracking Mechanism](#6-usage-tracking-mechanism)
7. [Overage Management](#7-overage-management)
8. [Billing Integration](#8-billing-integration)
9. [Complete Examples](#9-complete-examples)
10. [API Endpoints](#10-api-endpoints)
11. [Code-Level Implementation](#11-code-level-implementation)

---

## 1. Introduction

### What is Privilege Management?

The Privilege Management System is the core mechanism that controls what users can do with their subscriptions. It tracks, enforces, and manages access to features like:
- Video consultations
- Chat messages
- Document uploads
- Prescription refills
- Health reports
- Specialist consultations

### Key Responsibilities

1. **Privilege Allocation** - Assign privileges based on subscription plan and billing cycle
2. **Usage Tracking** - Monitor real-time consumption of privileges
3. **Limit Enforcement** - Prevent usage beyond allocated amounts
4. **Overage Management** - Handle extra usage with billing
5. **Period Management** - Align privilege periods with billing cycles
6. **History Tracking** - Maintain detailed audit trails

### System Architecture Overview

```
User Action (e.g., Book Consultation)
    ↓
API Controller (SubscriptionsController, PrivilegeController)
    ↓
PrivilegeService (Validation & Enforcement)
    ↓
Database (Update Usage Records)
    ↓
Response to User (Success/Failure)
```

---

## 2. Core Concepts

### Privilege Types

**1. Limited Privileges**
- Has a specific count (e.g., 10 video consultations)
- Tracked per usage period
- Can be exhausted
- Triggers overage when exceeded

**2. Unlimited Privileges**
- Marked with value `-1`
- No usage limits enforced
- Still tracked for analytics
- No overage charges possible

**3. Disabled Privileges**
- Marked with value `0`
- Cannot be used
- Plan doesn't include this feature

### Usage Periods

Privilege usage periods are **dynamically aligned with subscription billing cycles**:

```
Subscription Billing Cycle: Annual (365 days)
Plan Privilege: 10 video consultations/month

Calculation:
- Base Monthly Limit: 10 consultations
- Billing Cycle Months: 365 / 30 = 12.17 months
- Allocated for Period: 10 × 12.17 = 122 consultations (rounded up)
- Period: Jan 1, 2025 - Jan 1, 2026 (matches subscription billing)
```

**Key Rule:** Privileges reset ONLY when billing succeeds, not time-based.

### Privilege Scaling Formula

```csharp
AllowedValue = MonthlyLimit × (BillingCycleDays ÷ 30)
Result = Math.Ceiling(AllowedValue)  // Always round up
```

**Examples:**
- Monthly (30 days): 10 × (30÷30) = 10 consultations
- Quarterly (90 days): 10 × (90÷30) = 30 consultations
- Annual (365 days): 10 × (365÷30) = 122 consultations

---

## 3. Entity Architecture

### 3.1 Core Entities

#### **Privilege Entity**
**File:** `backend/SmartTelehealth.Core/Entities/Privilege.cs`

**Purpose:** Defines available privileges in the system

**Key Fields:**
```csharp
- Id (Guid)                      // Unique identifier
- Name (string)                  // e.g., "Video Consultation"
- Description (string)           // What this privilege allows
- PrivilegeTypeId (Guid)         // Category (consultation, messaging, etc.)
```

**Relationships:**
- One-to-Many → `SubscriptionPlanPrivilege` (which plans include this)
- One-to-Many → `UserSubscriptionPrivilegeUsage` (who's using this)

---

#### **SubscriptionPlanPrivilege Entity**
**File:** `backend/SmartTelehealth.Core/Entities/SubscriptionPlanPrivilege.cs`

**Purpose:** Links privileges to subscription plans with configuration

**Key Fields:**
```csharp
- Id (Guid)
- SubscriptionPlanId (Guid)      // Which plan
- PrivilegeId (Guid)             // Which privilege
- Value (int)                     // -1=unlimited, 0=disabled, >0=limit
- MonthlyLimit (int?)            // Base monthly allocation
- DailyLimit (int?)              // Optional daily cap
- WeeklyLimit (int?)             // Optional weekly cap
- PrivilegeBaseCost (decimal)    // Cost contribution to plan price
- UnitCost (decimal)             // Overage charge per unit
```

**Business Logic:**
```csharp
public bool IsUnlimited => Value == -1;
public bool IsDisabled => Value == 0;
public bool IsLimited => Value > 0;
public bool HasOverageCharges => UnitCost > 0 && !IsUnlimited;
```

---

#### **UserSubscriptionPrivilegeUsage Entity**
**File:** `backend/SmartTelehealth.Core/Entities/UserSubscriptionPrivilegeUsage.cs`

**Purpose:** Tracks actual privilege consumption per user

**Key Fields:**
```csharp
- Id (Guid)
- SubscriptionId (Guid)              // Which subscription
- SubscriptionPlanPrivilegeId (Guid) // Privilege configuration
- PrivilegeId (Guid)                 // Direct link for queries
- UsedValue (int)                    // Current usage count
- AllowedValue (int)                 // Allocated amount for period
- UsagePeriodStart (DateTime)        // Period start (aligned with billing)
- UsagePeriodEnd (DateTime)          // Period end (aligned with billing)
- LastUsedAt (DateTime?)             // Last usage timestamp
- ResetAt (DateTime?)                // Last reset timestamp
```

**Computed Properties:**
```csharp
public int RemainingValue => AllowedValue == -1 ? int.MaxValue : Math.Max(0, AllowedValue - UsedValue);
public bool IsUnlimited => AllowedValue == -1;
public bool IsExhausted => !IsUnlimited && UsedValue >= AllowedValue;
public decimal UsagePercentage => IsUnlimited ? 0 : (decimal)UsedValue / AllowedValue * 100;
public bool IsCurrentPeriod => DateTime.UtcNow >= UsagePeriodStart && DateTime.UtcNow <= UsagePeriodEnd;
```

---

#### **PrivilegeUsageHistory Entity**
**File:** `backend/SmartTelehealth.Core/Entities/PrivilegeUsageHistory.cs`

**Purpose:** Detailed audit trail of every privilege use

**Key Fields:**
```csharp
- Id (Guid)
- UserSubscriptionPrivilegeUsageId (Guid)
- UsedValue (int)                // Amount used in this instance
- UsedAt (DateTime)              // Exact timestamp
- UsageDate (DateTime)           // Date only (for daily tracking)
- UsageWeek (string)             // YYYY-WW format
- UsageMonth (string)            // YYYY-MM format
- Notes (string?)                // Optional context
```

**Purpose of Time Fields:**
- `UsageDate` - Daily limit checking
- `UsageWeek` - Weekly limit checking
- `UsageMonth` - Monthly limit checking

---

### 3.2 Entity Relationships Diagram

```
Privilege (Master Data)
    ↓ One-to-Many
SubscriptionPlanPrivilege (Configuration)
    ↓ One-to-Many
UserSubscriptionPrivilegeUsage (Current Usage)
    ↓ One-to-Many
PrivilegeUsageHistory (Detailed History)

Additional Relationships:
- SubscriptionPlanPrivilege → SubscriptionPlan (Many-to-One)
- UserSubscriptionPrivilegeUsage → Subscription (Many-to-One)
```

---

## 4. Service Architecture

### 4.1 PrivilegeService

**File:** `backend/SmartTelehealth.Application/Services/PrivilegeService.cs`

**Purpose:** Central service for all privilege operations

**Dependencies:**
```csharp
- IPrivilegeRepository                    // Privilege CRUD
- ISubscriptionPlanPrivilegeRepository   // Plan privilege configuration
- IUserSubscriptionPrivilegeUsageRepository  // Usage tracking
- IPrivilegeUsageHistoryRepository       // History/audit
- ISubscriptionRepository                 // Subscription details
- ILogger<PrivilegeService>              // Logging
```

**Core Methods:**

#### **1. UsePrivilegeAsync()**
**Line:** 232  
**Purpose:** Consume a privilege with validation

```csharp
public async Task<bool> UsePrivilegeAsync(
    Guid subscriptionId, 
    string privilegeName, 
    int amount, 
    TokenModel tokenModel)
```

**Flow:**
1. Validate input (amount > 0)
2. Get plan privilege configuration
3. Check if privilege is disabled → Return false
4. Check time-based limits (daily/weekly/monthly)
5. Handle unlimited privileges (-1) → Always allow
6. For limited privileges:
   - Check remaining amount
   - Return false if insufficient
   - Update usage record
   - Record in history
7. Log operation
8. Return true on success

**Example Usage:**
```csharp
var success = await _privilegeService.UsePrivilegeAsync(
    subscriptionId: Guid.Parse("sub-guid"),
    privilegeName: "Video Consultation",
    amount: 1,
    tokenModel: currentUser
);

if (success) {
    // Proceed with consultation booking
} else {
    // Show error or overage option
}
```

---

#### **2. GetRemainingPrivilegeAsync()**
**Line:** 106  
**Purpose:** Check how many uses are left

```csharp
public async Task<int> GetRemainingPrivilegeAsync(
    Guid subscriptionId, 
    string privilegeName, 
    TokenModel tokenModel)
```

**Returns:**
- `0` - Privilege disabled or exhausted
- `int.MaxValue` - Unlimited privilege
- `> 0` - Specific remaining count

**Logic:**
```csharp
var usage = await GetUsageRecord(subscriptionId, privilegeId);
if (usage == null) return planPrivilege.Value; // Initial state

var allowed = usage.AllowedValue;  // Dynamically calculated
var used = usage.UsedValue;
return allowed == -1 ? int.MaxValue : Math.Max(0, allowed - used);
```

---

#### **3. CalculatePrivilegeAllocationAsync()**
**Line:** 1195  
**Purpose:** Calculate allowed amount based on billing cycle

```csharp
private async Task<(int allowedValue, DateTime periodStart, DateTime periodEnd)> 
    CalculatePrivilegeAllocationAsync(
        Guid subscriptionId, 
        SubscriptionPlanPrivilege planPrivilege)
```

**Logic:**
```csharp
// Get subscription billing cycle
var subscription = await _subscriptionRepo.GetByIdWithDetailsAsync(subscriptionId);
var billingCycleDays = subscription.BillingCycle.DurationInDays; // e.g., 365

// Calculate months in cycle
var monthsInCycle = billingCycleDays / 30.0m; // 365 / 30 = 12.17

// Get monthly limit from plan
var monthlyLimit = planPrivilege.MonthlyLimit ?? planPrivilege.Value; // 10

// Calculate for full cycle
var allowedForCycle = monthlyLimit == -1 
    ? -1  // Unlimited stays unlimited
    : (int)Math.Ceiling(monthlyLimit * monthsInCycle); // 10 × 12.17 = 122

// Set period aligned with subscription billing
var periodStart = subscription.LastBillingDate?.AddDays(1) ?? subscription.StartDate;
var periodEnd = subscription.NextBillingDate;

return (allowedForCycle, periodStart, periodEnd);
```

**Key Points:**
- Scales monthly limit to actual billing cycle
- Uses `Math.Ceiling()` to always round up (fair to user)
- Aligns period with subscription billing dates
- Handles unlimited privileges correctly

---

#### **4. CheckPrivilegeAvailabilityAsync()**
**Line:** 1035  
**Purpose:** Check if privilege can be used (without consuming)

```csharp
public async Task<JsonModel> CheckPrivilegeAvailabilityAsync(
    Guid subscriptionId,
    string privilegeName,
    int requestedAmount,
    TokenModel tokenModel)
```

**Returns JsonModel:**
```json
{
  "statusCode": 200,
  "message": "Privilege available",
  "data": {
    "available": true,
    "remaining": 75,
    "allowed": 120,
    "used": 45,
    "isUnlimited": false
  }
}
```

---

#### **5. CheckTimeBasedLimitsAsync()**
**Line:** 151  
**Purpose:** Enforce daily/weekly/monthly caps

```csharp
private async Task<bool> CheckTimeBasedLimitsAsync(
    Guid subscriptionId, 
    SubscriptionPlanPrivilege planPrivilege, 
    int amount)
```

**Checks:**
1. **Daily Limit:** `if (planPrivilege.DailyLimit.HasValue)`
   - Query `PrivilegeUsageHistory` for today's usage
   - Reject if `dailyUsage + amount > DailyLimit`

2. **Weekly Limit:** `if (planPrivilege.WeeklyLimit.HasValue)`
   - Query history for current week
   - Reject if `weeklyUsage + amount > WeeklyLimit`

3. **Monthly Limit:** `if (planPrivilege.MonthlyLimit.HasValue)`
   - Query history for current month
   - Reject if `monthlyUsage + amount > MonthlyLimit`

**Example:**
```csharp
// Plan: 120 consultations/year, but max 3/day
if (user tries to book 4th consultation today) {
    return false; // Daily limit exceeded
}
```

---

### 4.2 PaymentService Integration

**File:** `backend/SmartTelehealth.Application/Services/PaymentService.cs`

#### **ResetPrivilegesForNewBillingPeriodAsync()**
**Line:** 1197  
**Purpose:** Reset privileges when billing succeeds

```csharp
private async Task ResetPrivilegesForNewBillingPeriodAsync(
    Subscription subscription, 
    TokenModel tokenModel)
{
    // Get all privilege usage records for this subscription
    var usageRecords = await _subscriptionRepo
        .GetSubscriptionPrivilegeUsagesAsync(subscription.Id);
    
    // Get billing cycle info
    var billingCycleDays = subscription.BillingCycle.DurationInDays;
    var monthsInCycle = billingCycleDays / 30.0m;
    
    foreach (var usage in usageRecords)
    {
        // Get plan privilege configuration
        var planPrivilege = subscription.SubscriptionPlan.PlanPrivileges
            .FirstOrDefault(p => p.Id == usage.SubscriptionPlanPrivilegeId);
        
        if (planPrivilege != null)
        {
            // Recalculate allowed value for new period
            var monthlyLimit = planPrivilege.MonthlyLimit ?? planPrivilege.Value;
            var allowedForCycle = monthlyLimit == -1 
                ? -1 
                : (int)Math.Ceiling(monthlyLimit * monthsInCycle);
            
            // RESET USAGE
            usage.UsedValue = 0;  // Reset to zero
            usage.AllowedValue = allowedForCycle;  // Recalculate
            usage.UsagePeriodStart = subscription.LastBillingDate.Value.AddDays(1);
            usage.UsagePeriodEnd = subscription.NextBillingDate;
            usage.ResetAt = DateTime.UtcNow;
            
            // Update audit fields
            usage.UpdatedBy = tokenModel.UserID;
            usage.UpdatedDate = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdatePrivilegeUsageAsync(usage);
        }
    }
}
```

**When Called:**
- Only when payment succeeds
- Part of `UpdatePaymentRecordsAsync()` transaction
- Ensures atomic reset with billing update

---

### 4.3 AutomatedBillingService Integration

**File:** `backend/SmartTelehealth.Application/Services/AutomatedBillingService.cs`

#### **ProcessOverageChargesAsync()**
**Line:** 1769  
**Purpose:** Calculate and charge for overage usage

```csharp
public async Task<bool> ProcessOverageChargesAsync(
    Subscription subscription, 
    TokenModel tokenModel)
{
    // Get all usage records
    var usages = await GetPrivilegeUsagesAsync(subscription.Id);
    decimal totalOverageAmount = 0;
    
    // Calculate overage for each privilege
    foreach (var usage in usages.Where(u => u.UsedValue > u.AllowedValue))
    {
        var overage = usage.UsedValue - usage.AllowedValue;
        var privilegePrice = usage.SubscriptionPlanPrivilege.UnitCost;
        var overageForPrivilege = overage * privilegePrice;
        
        totalOverageAmount += overageForPrivilege;
        
        _logger.LogInformation(
            "Overage for privilege {PrivilegeName}: {Overage} × ${Price} = ${Total}",
            usage.SubscriptionPlanPrivilege.Privilege.Name,
            overage,
            privilegePrice,
            overageForPrivilege);
    }
    
    if (totalOverageAmount > 0)
    {
        // Create overage billing record
        var billingRecordId = await CreateOverageBillingRecordAsync(
            subscription, 
            totalOverageAmount, 
            tokenModel);
        
        // Process payment
        var paymentResult = await _billingService.ProcessPaymentAsync(
            billingRecordId.Value, 
            tokenModel);
        
        return paymentResult.StatusCode == 200;
    }
    
    return true;
}
```

---

## 5. Privilege Lifecycle

### 5.1 Initial Allocation (Subscription Creation)

**When:** User subscribes to a plan  
**Where:** `SubscriptionLifecycleService.CreateSubscriptionAsync()`

**Steps:**
1. User selects plan (e.g., Family Care, $150/month, 10 consultations/month)
2. User selects billing cycle (e.g., Annual)
3. System creates subscription record
4. **For each plan privilege:**
   ```csharp
   var (allowedValue, periodStart, periodEnd) = 
       await CalculatePrivilegeAllocationAsync(subscriptionId, planPrivilege);
   
   var usage = new UserSubscriptionPrivilegeUsage {
       SubscriptionId = subscriptionId,
       SubscriptionPlanPrivilegeId = planPrivilege.Id,
       PrivilegeId = planPrivilege.PrivilegeId,
       UsedValue = 0,  // Start at zero
       AllowedValue = allowedValue,  // e.g., 120 for annual
       UsagePeriodStart = periodStart,  // Jan 1, 2025
       UsagePeriodEnd = periodEnd  // Jan 1, 2026
   };
   
   await _usageRepo.AddAsync(usage);
   ```

**Result:**
- User has 120 consultations allocated
- Valid for entire year (Jan 1, 2025 - Jan 1, 2026)
- No usage yet (`UsedValue = 0`)

---

### 5.2 Privilege Consumption

**When:** User uses a feature  
**Where:** `PrivilegeService.UsePrivilegeAsync()`

**Example: Book Video Consultation**

```
User clicks "Book Consultation"
    ↓
Frontend calls: POST /api/Privileges/use
    {
      "subscriptionId": "sub-guid",
      "privilegeName": "Video Consultation",
      "amount": 1
    }
    ↓
PrivilegeService.UsePrivilegeAsync()
    ├─ Get plan privilege config
    ├─ Check if disabled → No
    ├─ Check time-based limits → Pass
    ├─ Check remaining: 120 - 0 = 120 ≥ 1 → Pass
    ├─ Update usage: UsedValue = 0 + 1 = 1
    ├─ Update LastUsedAt = Now
    └─ Record in PrivilegeUsageHistory
    ↓
Return true (Success)
    ↓
Consultation booking proceeds
```

**Database Changes:**
```sql
-- Update usage
UPDATE UserSubscriptionPrivilegeUsages
SET UsedValue = 1,
    LastUsedAt = '2025-01-15 14:30:00',
    UpdatedDate = GETUTCDATE()
WHERE SubscriptionId = 'sub-guid' 
  AND PrivilegeId = 'video-consultation-guid';

-- Insert history
INSERT INTO PrivilegeUsageHistory (
    UserSubscriptionPrivilegeUsageId,
    UsedValue,
    UsedAt,
    UsageDate,
    UsageWeek,
    UsageMonth
) VALUES (
    'usage-record-guid',
    1,
    '2025-01-15 14:30:00',
    '2025-01-15',
    '2025-03',  -- Week 3
    '2025-01'   -- January
);
```

---

### 5.3 Overage Detection

**When:** User exceeds allocated amount  
**Scenario:** User has 120 consultations, tries to use 121st

```
User tries to book consultation
    ↓
PrivilegeService.UsePrivilegeAsync()
    ├─ Check remaining: 120 - 120 = 0 < 1
    └─ Return false (Insufficient)
    ↓
Frontend receives failure
    ↓
Show overage popup:
    "⚠️ Plan Limit Reached
     You've used all 120 consultations.
     Additional consultation: $25
     [Cancel] [Pay & Continue]"
    ↓
If user clicks "Pay & Continue"
    ↓
POST /api/Billing/overage
    ↓
AutomatedBillingService.ProcessOverageChargesAsync()
    ├─ Calculate: (121 - 120) × $25 = $25
    ├─ Create BillingRecord (Type: Overage, Amount: $25)
    ├─ ProcessPaymentAsync() → Charge via Stripe
    └─ If payment succeeds:
        ├─ Allow usage: UsedValue = 121
        └─ Update BillingRecord → Paid
    ↓
Consultation proceeds
```

---

### 5.4 Period Reset (Renewal)

**When:** Next billing cycle starts and payment succeeds  
**Where:** `PaymentService.ResetPrivilegesForNewBillingPeriodAsync()`

**Scenario:** Annual subscription renews on Jan 1, 2026

```
Jan 1, 2026 - 2:00 AM
    ↓
AutomatedBillingService runs daily check
    ↓
Find subscription with NextBillingDate = Today
    ↓
Calculate billing amount: $1,530
    ↓
ProcessPaymentAsync()
    ├─ Charge via Stripe → Success
    └─ UpdatePaymentRecordsAsync():
        ├─ Update BillingRecord → Paid
        ├─ Update Subscription:
        │   ├─ LastBillingDate = Jan 1, 2026
        │   └─ NextBillingDate = Jan 1, 2027
        └─ ResetPrivilegesForNewBillingPeriodAsync():
            For each privilege:
            ├─ UsedValue = 0 (reset)
            ├─ AllowedValue = 120 (recalculate)
            ├─ UsagePeriodStart = Jan 2, 2026
            ├─ UsagePeriodEnd = Jan 1, 2027
            └─ ResetAt = Jan 1, 2026 02:00:15
    ↓
Commit transaction
    ↓
User has fresh 120 consultations for Year 2
```

**Key Points:**
- Reset happens ONLY on successful payment
- Transaction-safe (all-or-nothing)
- Privileges are recalculated (not just copied)
- Period aligned with new billing cycle

---

## 6. Usage Tracking Mechanism

### 6.1 Real-Time Tracking

**Components:**
1. **UserSubscriptionPrivilegeUsage** - Current state (snapshot)
2. **PrivilegeUsageHistory** - Detailed log (audit trail)

**Update Pattern:**
```csharp
// 1. Update current usage (snapshot)
usage.UsedValue += amount;
usage.LastUsedAt = DateTime.UtcNow;
await _usageRepo.UpdateUsageAsync(usage);

// 2. Record in history (immutable log)
var history = new PrivilegeUsageHistory {
    UserSubscriptionPrivilegeUsageId = usage.Id,
    UsedValue = amount,
    UsedAt = DateTime.UtcNow,
    UsageDate = DateTime.UtcNow.Date,
    UsageWeek = GetWeekKey(DateTime.UtcNow),
    UsageMonth = GetMonthKey(DateTime.UtcNow)
};
await _usageHistoryRepo.AddAsync(history);
```

---

### 6.2 Time-Based Limit Checking

**Daily Limit Example:**

Plan: 120 consultations/year, but max 3/day

```csharp
// Get today's usage from history
var today = DateTime.UtcNow.Date;
var dailyUsage = await _usageHistoryRepo.GetDailyUsageAsync(
    subscriptionId, 
    planPrivilegeId, 
    today);

// dailyUsage returns: SUM(UsedValue) WHERE UsageDate = today

if (dailyUsage + amount > planPrivilege.DailyLimit)
{
    // User has already used 3 today
    return false;
}
```

**SQL Query:**
```sql
SELECT SUM(UsedValue) 
FROM PrivilegeUsageHistory
WHERE UserSubscriptionPrivilegeUsageId = @usageId
  AND UsageDate = @today
```

**Weekly and Monthly** work similarly using `UsageWeek` and `UsageMonth` fields.

---

### 6.3 Usage Analytics

**Queries Supported:**

1. **Current Usage:**
```sql
SELECT 
    p.Name AS PrivilegeName,
    u.UsedValue,
    u.AllowedValue,
    u.RemainingValue,
    u.UsagePercentage
FROM UserSubscriptionPrivilegeUsages u
JOIN SubscriptionPlanPrivileges spp ON u.SubscriptionPlanPrivilegeId = spp.Id
JOIN Privileges p ON spp.PrivilegeId = p.Id
WHERE u.SubscriptionId = @subscriptionId;
```

2. **Usage Over Time:**
```sql
SELECT 
    UsageMonth,
    SUM(UsedValue) AS TotalUsed
FROM PrivilegeUsageHistory h
JOIN UserSubscriptionPrivilegeUsages u ON h.UserSubscriptionPrivilegeUsageId = u.Id
WHERE u.SubscriptionId = @subscriptionId
GROUP BY UsageMonth
ORDER BY UsageMonth;
```

3. **Most Used Privileges:**
```sql
SELECT 
    p.Name,
    SUM(h.UsedValue) AS TotalUsage
FROM PrivilegeUsageHistory h
JOIN UserSubscriptionPrivilegeUsages u ON h.UserSubscriptionPrivilegeUsageId = u.Id
JOIN SubscriptionPlanPrivileges spp ON u.SubscriptionPlanPrivilegeId = spp.Id
JOIN Privileges p ON spp.PrivilegeId = p.Id
WHERE u.SubscriptionId = @subscriptionId
GROUP BY p.Name
ORDER BY TotalUsage DESC;
```

---

## 7. Overage Management

### 7.1 Overage Detection Flow

```
User attempts to use privilege
    ↓
Check remaining: (Allowed - Used) < Requested?
    ↓ Yes
Return false from UsePrivilegeAsync()
    ↓
Frontend detects failure
    ↓
Check if overage is available (UnitCost > 0)?
    ↓ Yes
Show overage popup with price
    ↓
User decides: [Cancel] or [Pay & Continue]
```

### 7.2 Overage Billing Process

**Trigger Points:**
1. **Real-Time:** User exceeds limit and chooses to pay
2. **Batch:** End of billing cycle overage calculation

**Real-Time Overage:**

```csharp
// User clicks "Pay & Continue"
var overageAmount = (requestedAmount - remaining) * privilege.UnitCost;

// Create overage billing record
var billingRecord = new BillingRecord {
    SubscriptionId = subscriptionId,
    Amount = overageAmount,
    Type = BillingRecord.BillingType.Overage,
    Description = $"{requestedAmount - remaining} additional {privilegeName}",
    Status = BillingRecord.BillingStatus.Pending,
    DueDate = DateTime.UtcNow
};
await _billingRepo.CreateAsync(billingRecord);

// Process payment immediately
var paymentResult = await _paymentService.ProcessPaymentAsync(
    billingRecord.Id, 
    tokenModel);

if (paymentResult.StatusCode == 200)
{
    // Allow the privilege usage
    await UsePrivilegeAsync(subscriptionId, privilegeName, amount, tokenModel);
}
```

**Batch Overage (At Billing Cycle End):**

```csharp
// Called during subscription renewal
public async Task<bool> ProcessOverageChargesAsync(Subscription subscription)
{
    var usages = await GetAllUsagesAsync(subscription.Id);
    decimal totalOverage = 0;
    
    foreach (var usage in usages)
    {
        if (usage.UsedValue > usage.AllowedValue)
        {
            var overage = usage.UsedValue - usage.AllowedValue;
            var cost = overage * usage.SubscriptionPlanPrivilege.UnitCost;
            totalOverage += cost;
        }
    }
    
    if (totalOverage > 0)
    {
        // Add to next billing
        var billingRecord = await CreateOverageBillingRecordAsync(
            subscription, 
            totalOverage, 
            tokenModel);
        
        return await ProcessPaymentAsync(billingRecord.Id);
    }
    
    return true;
}
```

---

### 7.3 Overage Pricing Rules

**Pricing Logic:**
```csharp
// From SubscriptionPlanPrivilege entity
public decimal UnitCost { get; set; }  // Overage price per unit
```

**Example Configurations:**

**Plan 1: Basic Care**
```
Video Consultations:
- Included: 3/month
- Overage: $25 per additional consultation
- UnitCost = 25.00
```

**Plan 2: Family Care**
```
Video Consultations:
- Included: 10/month (120/year for annual billing)
- Overage: $15 per additional consultation
- UnitCost = 15.00
```

**Plan 3: Premium Care**
```
Video Consultations:
- Included: Unlimited
- Overage: Not applicable
- UnitCost = 0 (no overage charges)
```

---

### 7.4 Overage Prevention Features

**1. Proactive Warnings:**
```
When UsagePercentage ≥ 80%:
    Show warning: "You've used 80% of your consultations"

When UsagePercentage ≥ 95%:
    Show alert: "Only 5 consultations remaining"

When IsExhausted:
    Show overage popup before proceeding
```

**2. Overage Caps:**
```csharp
// Optional: Set maximum overage per privilege
public int? MaxOverageAllowed { get; set; }

// Check before allowing overage
if (usage.UsedValue - usage.AllowedValue >= maxOverageAllowed)
{
    return "Maximum overage limit reached. Please upgrade your plan.";
}
```

---

## 8. Billing Integration

### 8.1 Privilege Usage in Billing Calculations

**Scenario:** User has consumed overage privileges during billing period

**At Billing Time:**

```csharp
// 1. Calculate base subscription amount
var baseAmount = await CalculateBillingAmountAsync(subscription);

// 2. Calculate overage charges
var overageAmount = await CalculateOverageChargesAsync(subscription);

// 3. Total billing amount
var totalAmount = baseAmount + overageAmount;

// Create billing record
var billingRecord = new BillingRecord {
    Amount = totalAmount,
    Type = BillingRecord.BillingType.Recurring,
    Description = $"Subscription renewal + ${overageAmount} overage"
};
```

---

### 8.2 Billing Record Breakdown

**Example:**

```
Base Subscription: $1,530 (Family Care Annual)
Overage Charges:
  - 5 extra video consultations × $15 = $75
  - 10 extra document uploads × $2 = $20
Total Overage: $95
────────────────────────────────────────
Total Due: $1,625
```

**Database:**
```sql
-- Main billing record
INSERT INTO BillingRecords (
    SubscriptionId, Amount, Type, Description
) VALUES (
    'sub-guid', 1625.00, 2, -- Recurring
    'Annual renewal + $95 overage'
);

-- Overage detail (optional breakdown table)
INSERT INTO BillingRecordDetails (
    BillingRecordId, Description, Amount
) VALUES 
    ('billing-guid', '5 extra video consultations', 75.00),
    ('billing-guid', '10 extra document uploads', 20.00);
```

---

### 8.3 Failed Billing and Privilege Access

**Rule:** If billing fails, privileges are NOT reset

```csharp
// In UpdatePaymentRecordsAsync()
if (paymentSucceeded)
{
    // Update subscription
    subscription.LastBillingDate = DateTime.UtcNow;
    subscription.NextBillingDate = CalculateNextBillingDate(...);
    subscription.Status = Subscription.SubscriptionStatuses.Active;
    
    // RESET PRIVILEGES
    await ResetPrivilegesForNewBillingPeriodAsync(subscription, tokenModel);
}
else
{
    // Payment failed
    subscription.Status = Subscription.SubscriptionStatuses.PastDue;
    
    // DO NOT RESET PRIVILEGES
    // User keeps exhausted state until they pay
}
```

**User Experience:**
- Payment fails → Subscription becomes "PastDue"
- User tries to use privilege → Rejected (subscription not active)
- User updates payment method → Payment retried
- Payment succeeds → Privileges reset, user can use again

---

## 9. Complete Examples

### Example 1: Monthly Subscription - Normal Usage

**Setup:**
- Plan: Basic Care ($50/month)
- Privileges: 3 video consultations/month
- Billing Cycle: Monthly

**Timeline:**

**Day 1 (Jan 1):** Subscribe
```
Subscription Created:
- StartDate: Jan 1
- NextBillingDate: Feb 1
- CurrentPrice: $50

Privileges Allocated:
- AllowedValue: 3 (3 consultations for 30 days)
- UsedValue: 0
- UsagePeriodStart: Jan 1
- UsagePeriodEnd: Feb 1
```

**Day 5 (Jan 5):** Use 1st consultation
```
UsePrivilegeAsync("Video Consultation", 1)
→ UsedValue: 0 → 1
→ Remaining: 2
→ Success ✅
```

**Day 12 (Jan 12):** Use 2nd consultation
```
UsePrivilegeAsync("Video Consultation", 1)
→ UsedValue: 1 → 2
→ Remaining: 1
→ Success ✅
```

**Day 25 (Jan 25):** Use 3rd consultation
```
UsePrivilegeAsync("Video Consultation", 1)
→ UsedValue: 2 → 3
→ Remaining: 0
→ Success ✅ (Last one!)
```

**Day 28 (Jan 28):** Try to use 4th
```
UsePrivilegeAsync("Video Consultation", 1)
→ Check: 3 - 3 = 0 < 1
→ Return false ❌
→ Show overage popup: "$25 for additional consultation"
→ User pays $25
→ ProcessOverageChargesAsync()
→ UsedValue: 3 → 4
→ Success ✅
```

**Feb 1:** Billing Renewal
```
AutomatedBillingService runs
→ CalculateBillingAmount: $50 (base) + $25 (overage) = $75
→ ProcessPaymentAsync($75)
→ Payment succeeds ✅
→ ResetPrivilegesForNewBillingPeriodAsync():
    - UsedValue: 4 → 0
    - AllowedValue: 3
    - UsagePeriodStart: Feb 2
    - UsagePeriodEnd: Mar 1
→ User has fresh 3 consultations for February
```

---

### Example 2: Annual Subscription - Heavy Usage

**Setup:**
- Plan: Family Care ($150/month, 10 consultations/month)
- Billing Cycle: Annual (15% discount)
- Price: $1,530/year

**Allocation:**
```
MonthlyLimit: 10 consultations
BillingCycleDays: 365
MonthsInCycle: 365 / 30 = 12.17

AllowedValue: Math.Ceiling(10 × 12.17) = 122 consultations/year
UsagePeriodStart: Jan 1, 2025
UsagePeriodEnd: Jan 1, 2026
```

**Usage Pattern:**

| Month | Consultations Used | Cumulative | Status |
|-------|-------------------|------------|--------|
| Jan | 8 | 8 | Normal (8/122) |
| Feb | 12 | 20 | Normal (20/122) |
| Mar | 10 | 30 | Normal (30/122) |
| Apr | 15 | 45 | Heavy (45/122) |
| May | 10 | 55 | Normal (55/122) |
| Jun | 8 | 63 | Normal (63/122) |
| Jul | 12 | 75 | Normal (75/122) |
| Aug | 10 | 85 | Normal (85/122) |
| Sep | 15 | 100 | Heavy (100/122) |
| Oct | 10 | 110 | Warning (110/122, 90% used) |
| Nov | 10 | 120 | Alert (120/122, 98% used) |
| Dec | 5 | 125 | **Overage** (125/122, +3 over) |

**December Overage:**
```
Dec 15: User tries 123rd consultation
→ Check: 122 - 122 = 0 < 1
→ Show popup: "$15 for additional consultation"
→ User pays → Success

Dec 20: 124th consultation
→ Pay $15 → Success

Dec 28: 125th consultation
→ Pay $15 → Success

Total Overage: 3 × $15 = $45
```

**Jan 1, 2026: Renewal**
```
Base Amount: $1,530
Overage: $45
Total: $1,575

Payment succeeds → Reset:
- UsedValue: 125 → 0
- AllowedValue: 122 (recalculated for new year)
- Period: Jan 2, 2026 - Jan 1, 2027
```

---

### Example 3: Unlimited Privilege

**Setup:**
- Plan: Premium Care
- Privilege: Chat Messages (Unlimited)
- Config: AllowedValue = -1

**Usage:**

**Day 1:**
```
UsePrivilegeAsync("Chat Messages", 1)
→ Check: IsUnlimited = true
→ Skip limit check
→ UsedValue: 0 → 1
→ Success ✅ (no limit check)
```

**Day 50:** (After heavy usage)
```
Current State:
- UsedValue: 487 (tracked for analytics)
- AllowedValue: -1 (unlimited)
- RemainingValue: int.MaxValue

UsePrivilegeAsync("Chat Messages", 1)
→ IsUnlimited = true
→ UsedValue: 487 → 488
→ Success ✅ (still no limit)
```

**Billing Time:**
```
Chat Messages:
- Used: 1,250 messages this period
- Cost: $0 (included in plan)
- Overage: N/A (unlimited)
```

---

### Example 4: Time-Based Limits

**Setup:**
- Plan: Professional Care
- Privilege: Video Consultations
- Monthly Limit: 50 consultations
- Daily Limit: 3 consultations

**Scenario:**

**Jan 15 - Morning:**
```
10:00 AM: 1st consultation → Success (daily: 1/3)
11:30 AM: 2nd consultation → Success (daily: 2/3)
```

**Jan 15 - Afternoon:**
```
2:00 PM: 3rd consultation → Success (daily: 3/3)
```

**Jan 15 - Evening:**
```
7:00 PM: Try 4th consultation
→ CheckTimeBasedLimitsAsync()
→ Get today's usage: SUM = 3
→ Check: 3 + 1 > 3 (DailyLimit)
→ Return false ❌
→ Error: "Daily limit of 3 consultations reached. Try again tomorrow."
```

**Jan 16 - Next Day:**
```
9:00 AM: 1st consultation
→ CheckTimeBasedLimitsAsync()
→ Get today's usage: SUM = 0 (new day)
→ Check: 0 + 1 ≤ 3 ✅
→ Check monthly: 35 + 1 ≤ 50 ✅
→ Success! (daily: 1/3, monthly: 36/50)
```

---

## 10. API Endpoints

### 10.1 Privilege Usage Endpoints

**Use Privilege (Consume)**
```http
POST /api/Privileges/use
Authorization: Bearer <token>
Content-Type: application/json

{
  "subscriptionId": "550e8400-e29b-41d4-a716-446655440000",
  "privilegeName": "Video Consultation",
  "amount": 1
}

Response 200 OK:
{
  "statusCode": 200,
  "message": "Privilege used successfully",
  "data": {
    "usedValue": 46,
    "remainingValue": 74,
    "allowedValue": 120
  }
}

Response 400 Bad Request:
{
  "statusCode": 400,
  "message": "Insufficient privileges. Overage available for $15.",
  "data": {
    "remaining": 0,
    "requested": 1,
    "overagePrice": 15.00
  }
}
```

---

**Check Availability**
```http
GET /api/Privileges/availability?subscriptionId={id}&privilegeName=Video%20Consultation&amount=1
Authorization: Bearer <token>

Response 200 OK:
{
  "statusCode": 200,
  "message": "Privilege available",
  "data": {
    "available": true,
    "remaining": 74,
    "allowed": 120,
    "used": 46,
    "isUnlimited": false,
    "usagePercentage": 38.33,
    "periodEnd": "2026-01-01T00:00:00Z"
  }
}
```

---

**Get Subscription Usage Summary**
```http
GET /api/Privileges/usage/{subscriptionId}
Authorization: Bearer <token>

Response 200 OK:
{
  "statusCode": 200,
  "message": "Usage retrieved successfully",
  "data": {
    "subscriptionId": "550e8400-e29b-41d4-a716-446655440000",
    "periodStart": "2025-01-01T00:00:00Z",
    "periodEnd": "2026-01-01T00:00:00Z",
    "privileges": [
      {
        "name": "Video Consultation",
        "allowedValue": 120,
        "usedValue": 46,
        "remainingValue": 74,
        "isUnlimited": false,
        "isExhausted": false,
        "usagePercentage": 38.33,
        "lastUsedAt": "2025-10-15T14:30:00Z"
      },
      {
        "name": "Chat Messages",
        "allowedValue": -1,
        "usedValue": 487,
        "remainingValue": 2147483647,
        "isUnlimited": true,
        "isExhausted": false,
        "usagePercentage": 0,
        "lastUsedAt": "2025-10-18T09:15:00Z"
      }
    ]
  }
}
```

---

**Get Usage History**
```http
GET /api/Privileges/history?subscriptionId={id}&page=1&pageSize=20
Authorization: Bearer <token>

Response 200 OK:
{
  "statusCode": 200,
  "message": "History retrieved successfully",
  "data": {
    "page": 1,
    "pageSize": 20,
    "totalRecords": 46,
    "history": [
      {
        "privilegeName": "Video Consultation",
        "usedValue": 1,
        "usedAt": "2025-10-18T14:30:00Z",
        "usageDate": "2025-10-18",
        "notes": "Regular consultation"
      },
      {
        "privilegeName": "Video Consultation",
        "usedValue": 1,
        "usedAt": "2025-10-15T10:15:00Z",
        "usageDate": "2025-10-15",
        "notes": null
      }
    ]
  }
}
```

---

### 10.2 Overage Endpoints

**Process Overage Payment**
```http
POST /api/Billing/overage
Authorization: Bearer <token>
Content-Type: application/json

{
  "subscriptionId": "550e8400-e29b-41d4-a716-446655440000",
  "privilegeName": "Video Consultation",
  "amount": 1
}

Response 200 OK:
{
  "statusCode": 200,
  "message": "Overage payment processed successfully",
  "data": {
    "billingRecordId": "billing-guid",
    "amount": 15.00,
    "privilegeUpdated": true,
    "newUsedValue": 121,
    "allowedValue": 120
  }
}
```

---

## 11. Code-Level Implementation

### 11.1 Repository Layer

**IUserSubscriptionPrivilegeUsageRepository**
```csharp
public interface IUserSubscriptionPrivilegeUsageRepository
{
    Task<UserSubscriptionPrivilegeUsage?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<UserSubscriptionPrivilegeUsage?> GetBySubscriptionAndPrivilegeAsync(Guid subscriptionId, Guid privilegeId);
    Task AddAsync(UserSubscriptionPrivilegeUsage usage);
    Task UpdateUsageAsync(UserSubscriptionPrivilegeUsage usage);
    Task DeleteAsync(Guid id);
}
```

**Implementation:**
```csharp
public class UserSubscriptionPrivilegeUsageRepository 
    : IUserSubscriptionPrivilegeUsageRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<IEnumerable<UserSubscriptionPrivilegeUsage>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
                .ThenInclude(p => p.Privilege)
            .Where(u => u.SubscriptionId == subscriptionId)
            .ToListAsync();
    }
    
    public async Task UpdateUsageAsync(UserSubscriptionPrivilegeUsage usage)
    {
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();
    }
}
```

---

### 11.2 Service Registration (Dependency Injection)

**File:** `backend/SmartTelehealth.Application/DependencyInjection.cs`

```csharp
services.AddScoped<IPrivilegeService, PrivilegeService>();
services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();
services.AddScoped<IPrivilegeUsageHistoryRepository, PrivilegeUsageHistoryRepository>();
```

---

### 11.3 Database Indexes (Performance)

**Critical Indexes:**
```sql
-- Subscription usage lookup (most frequent query)
CREATE INDEX IX_UserSubscriptionPrivilegeUsages_SubscriptionId 
ON UserSubscriptionPrivilegeUsages(SubscriptionId);

-- Privilege lookup
CREATE INDEX IX_UserSubscriptionPrivilegeUsages_PrivilegeId 
ON UserSubscriptionPrivilegeUsages(PrivilegeId);

-- Period expiry check
CREATE INDEX IX_UserSubscriptionPrivilegeUsages_PeriodEnd 
ON UserSubscriptionPrivilegeUsages(UsagePeriodEnd);

-- History date queries
CREATE INDEX IX_PrivilegeUsageHistory_UsageDate 
ON PrivilegeUsageHistory(UsageDate DESC);

CREATE INDEX IX_PrivilegeUsageHistory_UsageMonth 
ON PrivilegeUsageHistory(UsageMonth);
```

---

### 11.4 Transaction Safety

**Critical Operations Use Transactions:**

```csharp
using var transaction = await _unitOfWork.BeginTransactionAsync();
try
{
    // 1. Update usage
    usage.UsedValue += amount;
    await _usageRepo.UpdateUsageAsync(usage);
    
    // 2. Record history
    await _historyRepo.AddAsync(historyRecord);
    
    // 3. Create billing record (if overage)
    if (isOverage)
    {
        await _billingRepo.CreateAsync(billingRecord);
    }
    
    await _unitOfWork.CommitTransactionAsync();
}
catch (Exception)
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## Summary

This privilege management system provides:

✅ **Accurate Tracking** - Real-time usage monitoring  
✅ **Fair Allocation** - Privileges scaled to billing cycles  
✅ **Flexible Limits** - Time-based and quantity-based controls  
✅ **Overage Support** - Users can exceed limits with payment  
✅ **Billing Integration** - Seamless overage charge calculation  
✅ **Audit Trail** - Complete history of all privilege usage  
✅ **Transaction Safety** - All operations are atomic  
✅ **Performance** - Optimized queries with proper indexing  

**Key Innovation:** Privileges are aligned with subscription billing cycles, ensuring users get exactly what they pay for, with automatic scaling and fair reset logic.

---

*Document Version: 1.0 | Last Updated: October 18, 2025 | Status: Production Ready*



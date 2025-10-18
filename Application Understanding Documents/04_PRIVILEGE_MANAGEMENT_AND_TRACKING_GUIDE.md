# 📘 Privilege Management and Tracking - Developer Guide

## Table of Contents
1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Database Schema](#database-schema)
4. [Service Architecture](#service-architecture)
5. [Privilege Usage Workflow](#privilege-usage-workflow)
6. [Overage Detection](#overage-detection)
7. [Usage History Tracking](#usage-history-tracking)
8. [Code Examples](#code-examples)

---

## 1. Overview

### What is Privilege Management?

Privilege Management is the system that controls what users can do with their subscription and tracks their usage. It enforces limits, detects when users exceed those limits, and maintains a complete audit trail of all privilege consumption.

### Key Responsibilities

- ✅ **Usage Validation**: Check if user has credits before allowing service
- ✅ **Usage Tracking**: Decrement counters when privileges are consumed
- ✅ **Limit Enforcement**: Block usage when limits are exceeded
- ✅ **Overage Detection**: Identify when user needs to pay for extras
- ✅ **History Recording**: Maintain complete audit trail
- ✅ **Reset Management**: Reset counters on subscription renewal

---

## 2. Core Concepts

### 2.1 What is a Privilege?

A **Privilege** is a specific service or feature that a user can access through their subscription.

**Examples:**
- Teleconsultation (video call with doctor)
- Medication Refill (prescription refill)
- Lab Test Request
- Health Records Access
- Specialist Referral

### 2.2 Privilege Configuration Hierarchy

```
┌────────────────────┐
│    Privilege       │  (Master definition)
│    "Teleconsultation"│
└─────────┬──────────┘
          │
          │ Referenced by
          ↓
┌────────────────────┐
│ SubscriptionPlan   │  (Plan configuration)
│ Privilege          │
│  - Value: 5        │  ← Quantity included in plan
│  - BaseCost: $20   │  ← For plan pricing
│  - UnitCost: $25   │  ← For overage
│  - MonthlyLimit: 5 │  ← Usage cap
└─────────┬──────────┘
          │
          │ Copied to
          ↓
┌────────────────────┐
│ User Subscription  │  (User's active tracking)
│ Privilege Usage    │
│  - AllocatedLimit: 5│  ← What they have
│  - UsedValue: 3     │  ← What they've used
│  - AllowedValue: 2  │  ← What's remaining
└────────────────────┘
```

### 2.3 Usage Tracking States

| State | AllocatedLimit | UsedValue | AllowedValue | Meaning |
|-------|----------------|-----------|--------------|---------|
| **Fresh** | 5 | 0 | 5 | New subscription, nothing used |
| **In Use** | 5 | 3 | 2 | Used 3, have 2 left |
| **Depleted** | 5 | 5 | 0 | Used all, none left |
| **Overage** | 6 | 6 | 0 | Purchased 1 extra, used it |

---

## 3. Database Schema

### 3.1 Table: Privileges (Master List)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | telecon-guid |
| Name | NVARCHAR(200) | Privilege name | "Teleconsultation" |
| Description | NVARCHAR(MAX) | What it provides | "Video consultation with doctor" |
| Category | NVARCHAR(100) | Privilege category | "Medical Services" |
| IsActive | BIT | Currently available | 1 (true) |
| CreatedDate | DATETIME2 | When created | 2025-01-01 |

### 3.2 Table: SubscriptionPlanPrivileges (Plan Configuration)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | plan-priv-guid |
| SubscriptionPlanId | UNIQUEIDENTIFIER | FK to Plan | f3a1b2c3-... |
| PrivilegeId | UNIQUEIDENTIFIER | FK to Privilege | telecon-guid |
| Value | INT | Quantity in plan | 5 |
| PrivilegeBaseCost | DECIMAL(18,2) | Cost for plan pricing | 20.00 |
| UnitCost | DECIMAL(18,2) | Cost for overage | 25.00 |
| DailyLimit | INT | Daily cap (optional) | NULL |
| WeeklyLimit | INT | Weekly cap (optional) | NULL |
| MonthlyLimit | INT | Monthly cap | 5 |
| IsActive | BIT | Active configuration | 1 (true) |

### 3.3 Table: UserSubscriptionPrivilegeUsage (Active Tracking)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | usage-123 |
| SubscriptionId | UNIQUEIDENTIFIER | FK to Subscription | sub_111 |
| PrivilegeId | UNIQUEIDENTIFIER | FK to Privilege | telecon-guid |
| SubscriptionPlanPrivilegeId | UNIQUEIDENTIFIER | FK to plan config | plan-priv-guid |
| **AllocatedLimit** | INT | Total allowed | 5 |
| **UsedValue** | INT | How many used | 3 |
| **AllowedValue** | INT | Remaining (calculated) | 2 |
| UsagePeriodStart | DATETIME2 | Period start | 2025-10-17 |
| UsagePeriodEnd | DATETIME2 | Period end | 2025-11-17 |
| LastUsedAt | DATETIME2 | Last usage time | 2025-10-20 |
| ResetAt | DATETIME2 | Last reset time | 2025-10-17 |

**Key Fields Explained:**
- **AllocatedLimit**: Total credits available (e.g., 5 consultations from plan)
- **UsedValue**: How many have been consumed (increments with each use)
- **AllowedValue**: What's left (calculated: AllocatedLimit - UsedValue)

### 3.4 Table: PrivilegeUsageHistory (Audit Trail)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | history-123 |
| UserId | INT | FK to User | 456 |
| SubscriptionId | UNIQUEIDENTIFIER | FK to Subscription | sub_111 |
| PrivilegeId | UNIQUEIDENTIFIER | FK to Privilege | telecon-guid |
| UsageDate | DATETIME2 | When used | 2025-10-20 10:30:00 |
| QuantityUsed | INT | How many used | 1 |
| RemainingAfterUse | INT | Left after this use | 2 |
| **UsageType** | NVARCHAR(50) | "Included" or "Overage" | "Included" |
| **Cost** | DECIMAL(18,2) | $0 for included, $X for overage | 0.00 |
| RelatedEntityId | NVARCHAR(255) | Link to appointment, etc. | appt-123 |
| Notes | NVARCHAR(MAX) | Additional info | "Video consultation booked" |

---

## 4. Service Architecture

### 4.1 Primary Services

#### **PrivilegeService**
**Location:** `SmartTelehealth.Application/Services/PrivilegeService.cs`

**Responsibilities:**
- Check privilege availability
- Use/consume privileges
- Validate usage limits
- Enforce overage requirements

**Key Methods:**
```csharp
Task<JsonModel> CheckPrivilegeAvailabilityAsync(int userId, Guid privilegeId, int requestedQuantity)
Task<JsonModel> UsePrivilegeAsync(int userId, Guid privilegeId, int quantity, string relatedEntityId)
Task<JsonModel> GetUserPrivilegesAsync(int userId)
```

#### **SubscriptionBillingService** (Overage Handling)
**Location:** `SmartTelehealth.Application/Services/SubscriptionBillingService.cs`

**Responsibilities:**
- Calculate overage costs
- Create overage billing records
- Track privilege-based billing

---

## 5. Privilege Usage Workflow

### 5.1 Complete Usage Flow

```
┌─────────────────────────────────────────────────┐
│ USER ACTION: Book Teleconsultation              │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ AppointmentsController                           │
│ Method: BookConsultationAsync()                  │
└─────────────────────────────────────────────────┘
                    ↓
        Must check privilege first
                    ↓
┌─────────────────────────────────────────────────┐
│ PrivilegeService                                 │
│ Method: CheckPrivilegeAvailabilityAsync()       │
│                                                  │
│ [STEP 1] Get User's Active Subscription         │
│   subscription = await _subscriptionRepository  │
│     .GetActiveSubscriptionByUserIdAsync(userId);│
│                                                  │
│   if (subscription == null)                     │
│     return "No active subscription";            │
│                                                  │
│ [STEP 2] Get Current Usage                      │
│   usage = await _privilegeUsageRepository       │
│     .GetByUserAndPrivilegeAsync(                │
│       userId, privilegeId                       │
│     );                                          │
│                                                  │
│   Current State:                                │
│   ┌──────────────────────────────┐             │
│   │ AllocatedLimit: 5            │             │
│   │ UsedValue: 3                 │             │
│   │ AllowedValue: 2              │             │
│   └──────────────────────────────┘             │
│                                                  │
│ [STEP 3] Validate Availability                  │
│   requestedQuantity = 1;  // Booking 1 consult │
│                                                  │
│   if (usage.AllowedValue >= requestedQuantity) {│
│     // ✅ HAS CREDITS                           │
│     return JsonModel {                          │
│       StatusCode = 200,                         │
│       Message = "Privilege available",          │
│       data = new {                              │
│         Available = true,                       │
│         RemainingCredits = 2                    │
│       }                                         │
│     };                                          │
│   }                                             │
│   else {                                        │
│     // ❌ INSUFFICIENT CREDITS                  │
│     // Get overage pricing                     │
│     latestPlan = await GetLatestPlanVersion(); │
│     planPrivilege = latestPlan.PlanPrivileges  │
│       .Find(p => p.PrivilegeId == privilegeId);│
│                                                  │
│     unitCost = planPrivilege.UnitCost;  // $25 │
│                                                  │
│     return JsonModel {                          │
│       StatusCode = 402,  // Payment Required   │
│       Message = "Insufficient credits",         │
│       data = new {                              │
│         Available = false,                      │
│         AvailableCredits = 0,                   │
│         RequiredCredits = 1,                    │
│         CostPerUnit = 25.00,                    │
│         TotalRequired = 25.00                   │
│       }                                         │
│     };                                          │
│   }                                             │
└─────────────────────────────────────────────────┘
                    ↓
        If Available (200): Proceed to use
                    ↓
┌─────────────────────────────────────────────────┐
│ PrivilegeService                                 │
│ Method: UsePrivilegeAsync()                      │
│                                                  │
│ [STEP 1] BEGIN TRANSACTION                      │
│   await _unitOfWork.BeginTransactionAsync();    │
│                                                  │
│ [STEP 2] Get Current Usage (Lock Row)           │
│   usage = await _privilegeUsageRepository       │
│     .GetByUserAndPrivilegeAsync(                │
│       userId, privilegeId                       │
│     );                                          │
│                                                  │
│   Current State:                                │
│   ┌──────────────────────────────┐             │
│   │ AllocatedLimit: 5            │             │
│   │ UsedValue: 3                 │             │
│   │ AllowedValue: 2              │             │
│   └──────────────────────────────┘             │
│                                                  │
│ [STEP 3] Update Usage Counters                  │
│   usage.UsedValue += 1;  // 3 → 4              │
│   usage.AllowedValue = usage.AllocatedLimit     │
│     - usage.UsedValue;  // 5 - 4 = 1           │
│   usage.LastUsedAt = DateTime.UtcNow;          │
│                                                  │
│   New State:                                    │
│   ┌──────────────────────────────┐             │
│   │ AllocatedLimit: 5 (unchanged)│             │
│   │ UsedValue: 4 ✅              │             │
│   │ AllowedValue: 1 ✅           │             │
│   │ LastUsedAt: 2025-10-20       │             │
│   └──────────────────────────────┘             │
│                                                  │
│   await _privilegeUsageRepository.UpdateAsync(  │
│     usage                                       │
│   );                                            │
│                                                  │
│ [STEP 4] Record in Usage History                │
│   var historyRecord = new PrivilegeUsageHistory {│
│     Id = Guid.NewGuid(),                        │
│     UserId = userId,                            │
│     SubscriptionId = subscription.Id,           │
│     PrivilegeId = privilegeId,                  │
│     UsageDate = DateTime.UtcNow,                │
│     QuantityUsed = 1,                           │
│     RemainingAfterUse = usage.AllowedValue,     │
│       // 1                                      │
│     UsageType = "Included",  ← Covered by plan │
│     Cost = 0.00,  ← No charge (included)       │
│     RelatedEntityId = appointmentId,            │
│     Notes = "Teleconsultation booked",          │
│     CreatedDate = DateTime.UtcNow               │
│   };                                            │
│                                                  │
│   await _privilegeUsageHistoryRepository        │
│     .CreateAsync(historyRecord);                │
│                                                  │
│ [STEP 5] COMMIT TRANSACTION                     │
│   await _unitOfWork.CommitTransactionAsync();   │
│                                                  │
│ [STEP 6] Log and Return Success                 │
│   _logger.LogInformation(                       │
│     "User {UserId} used privilege {PrivId}. " + │
│     "Remaining: {Remaining}",                   │
│     userId, privilegeId, usage.AllowedValue     │
│   );                                            │
│                                                  │
│   return JsonModel {                            │
│     StatusCode = 200,                           │
│     Message = "Privilege used successfully",    │
│     data = new {                                │
│       RemainingCredits = 1,                     │
│       UsedTotal = 4,                            │
│       AllocatedTotal = 5                        │
│     }                                           │
│   };                                            │
└─────────────────────────────────────────────────┘
                    ↓
        Booking proceeds successfully
```

### 5.2 Usage Timeline Example

```
SUBSCRIPTION PERIOD: Oct 17 - Nov 17

DAY 1 (Oct 17): Subscription starts
├─ Teleconsultations: 5 remaining
└─ Medications: 3 remaining

DAY 3 (Oct 19): Book consultation
├─ CheckPrivilegeAvailabilityAsync() → 200 OK (has 5)
├─ UsePrivilegeAsync() → Decrement to 4
└─ Teleconsultations: 4 remaining ✅

DAY 7 (Oct 23): Book consultation
├─ CheckPrivilegeAvailabilityAsync() → 200 OK (has 4)
├─ UsePrivilegeAsync() → Decrement to 3
└─ Teleconsultations: 3 remaining ✅

DAY 10 (Oct 26): Book consultation
└─ Teleconsultations: 2 remaining ✅

DAY 15 (Oct 31): Book consultation
└─ Teleconsultations: 1 remaining ✅

DAY 22 (Nov 7): Book consultation
└─ Teleconsultations: 0 remaining ✅

DAY 25 (Nov 10): Try to book 6th consultation
├─ CheckPrivilegeAvailabilityAsync() → 402 Payment Required
│  {
│    "AvailableCredits": 0,
│    "CostPerUnit": 25.00,
│    "TotalRequired": 25.00
│  }
├─ User pays $25 upfront
├─ Credit added: 0 → 1
├─ UsePrivilegeAsync() → Decrement to 0
└─ Teleconsultations: 0 remaining ⚠️ (used overage)

DAY 30 (Nov 17): Subscription renews
├─ Stripe charges $275
├─ Webhook processes renewal
└─ RESET: Teleconsultations: 5 remaining ✅ (fresh cycle)
```

---

## 6. Overage Detection

### 6.1 When Overage is Detected

```
┌─────────────────────────────────────────────────┐
│ OVERAGE DETECTION FLOW                           │
└─────────────────────────────────────────────────┘

User tries to use privilege:
  ↓
Check usage:
  Current: AllowedValue = 0
  Requested: 1
  ↓
Condition: AllowedValue < Requested
  ↓
OVERAGE DETECTED ⚠️
  ↓
Get Latest Plan Version (abuse prevention):
  ↓
  Why latest? If admin raised price from $25 to $30,
  user pays $30 (current price), not $25 (old price)
  ↓
Extract UnitCost from latest plan:
  ↓
  UnitCost = $25 per consultation
  ↓
Return 402 Payment Required:
  {
    "StatusCode": 402,
    "Message": "Insufficient credits",
    "data": {
      "AvailableCredits": 0,
      "RequiredCredits": 1,
      "CostPerUnit": 25.00,
      "TotalRequired": 25.00
    }
  }
  ↓
Frontend displays payment modal:
  "You've used all 5 consultations.
   Additional consultations: $25 each.
   [Cancel] [Pay $25 & Continue]"
  ↓
User clicks "Pay $25 & Continue"
  ↓
System processes upfront payment
  (See Guide 03: Billing & Payment)
  ↓
Only after payment succeeds:
  ↓
Add credit: AllowedValue = 0 → 1
Immediately use it: AllowedValue = 1 → 0
Mark in history: UsageType = "Overage", Cost = $25.00
```

### 6.2 Abuse Prevention Logic

**Problem:** User could exploit old pricing if we don't use latest

```
❌ BAD (Without Abuse Prevention):
  1. User subscribes to "Basic v1" @ $20/consult, $25 overage
  2. Admin updates to "Basic v2" @ $20/consult, $30 overage
  3. User (still on v1) exceeds limit
  4. System charges $25 (user's plan price)
  5. User saves $5 per overage
  6. Admin loses money ❌

✅ GOOD (With Abuse Prevention):
  1. User subscribes to "Basic v1" @ $20/consult, $25 overage
  2. Admin updates to "Basic v2" @ $20/consult, $30 overage
  3. User (still on v1) exceeds limit
  4. System uses LATEST plan version (v2) for overage
  5. System charges $30 (current price)
  6. Fair pricing enforced ✅
```

**Implementation:**
```csharp
// Don't use user's current plan for overage
// var userPlan = subscription.SubscriptionPlan;  ❌

// Get LATEST version of the plan for overage pricing
var parentPlanId = subscription.SubscriptionPlan.ParentPlanId 
    ?? subscription.SubscriptionPlan.Id;

var latestPlan = await _subscriptionPlanRepository
    .GetLatestVersionAsync(parentPlanId);  ✅

var unitCost = latestPlan.PlanPrivileges
    .FirstOrDefault(p => p.PrivilegeId == privilegeId)
    ?.UnitCost ?? 0;
```

---

## 7. Usage History Tracking

### 7.1 Why Track History?

1. **Audit Trail**: Know exactly when/how privileges were used
2. **Billing Verification**: Prove overage charges are correct
3. **Analytics**: Understand user behavior patterns
4. **Support**: Resolve user disputes with evidence
5. **Compliance**: Healthcare regulations require detailed records

### 7.2 History Record Structure

**Every time a privilege is used, we record:**

```csharp
var historyRecord = new PrivilegeUsageHistory
{
    // Who, What, When
    UserId = 456,
    SubscriptionId = sub_111,
    PrivilegeId = teleconsultation-guid,
    UsageDate = DateTime.UtcNow,
    
    // Quantity
    QuantityUsed = 1,  // Used 1 consultation
    RemainingAfterUse = 2,  // 2 left after this use
    
    // Type & Cost
    UsageType = "Included",  // or "Overage"
    Cost = 0.00,  // $0 for included, $25 for overage
    
    // Context
    RelatedEntityId = "appt-123",  // Link to appointment record
    Notes = "Video consultation with Dr. Smith",
    
    // Audit
    CreatedDate = DateTime.UtcNow,
    CreatedBy = 456
};
```

### 7.3 Querying Usage History

**Get all usage for a user:**
```csharp
var history = await _privilegeUsageHistoryRepository
    .GetByUserIdAsync(userId);

// Results:
[
    { Date: "2025-10-19", Privilege: "Teleconsultation", Type: "Included", Cost: $0 },
    { Date: "2025-10-23", Privilege: "Teleconsultation", Type: "Included", Cost: $0 },
    { Date: "2025-10-26", Privilege: "Teleconsultation", Type: "Included", Cost: $0 },
    { Date: "2025-10-31", Privilege: "Teleconsultation", Type: "Included", Cost: $0 },
    { Date: "2025-11-07", Privilege: "Teleconsultation", Type: "Included", Cost: $0 },
    { Date: "2025-11-10", Privilege: "Teleconsultation", Type: "Overage", Cost: $25 } ← Extra
]
```

---

## 8. Code Examples

### 8.1 Checking Privilege Availability (Full Code)

```csharp
public async Task<JsonModel> CheckPrivilegeAvailabilityAsync(
    int userId,
    Guid privilegeId,
    int requestedQuantity = 1)
{
    try
    {
        // 1. Get user's active subscription
        var subscription = await _subscriptionRepository
            .GetActiveSubscriptionByUserIdAsync(userId);
        
        if (subscription == null)
        {
            return new JsonModel
            {
                StatusCode = 404,
                Message = "No active subscription found",
                data = new { Available = false }
            };
        }
        
        // 2. Get current usage
        var usage = await _privilegeUsageRepository
            .GetByUserAndPrivilegeAsync(userId, privilegeId);
        
        if (usage == null)
        {
            return new JsonModel
            {
                StatusCode = 404,
                Message = "Privilege not found in subscription",
                data = new { Available = false }
            };
        }
        
        // 3. Check if user has enough credits
        if (usage.AllowedValue >= requestedQuantity)
        {
            // ✅ User has sufficient credits
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Privilege available",
                data = new
                {
                    Available = true,
                    RemainingCredits = usage.AllowedValue,
                    AllocatedTotal = usage.AllocatedLimit,
                    UsedTotal = usage.UsedValue
                }
            };
        }
        else
        {
            // ❌ Insufficient credits - need to pay for overage
            
            // Get latest plan version for overage pricing (abuse prevention)
            var parentPlanId = subscription.SubscriptionPlan.ParentPlanId 
                ?? subscription.SubscriptionPlan.Id;
            
            var latestPlan = await _subscriptionPlanRepository
                .GetLatestVersionByParentIdAsync(parentPlanId);
            
            var planPrivilege = latestPlan.PlanPrivileges
                .FirstOrDefault(p => p.PrivilegeId == privilegeId);
            
            if (planPrivilege == null)
            {
                return new JsonModel
                {
                    StatusCode = 404,
                    Message = "Privilege configuration not found"
                };
            }
            
            var unitCost = planPrivilege.UnitCost;
            var totalCost = requestedQuantity * unitCost;
            
            _logger.LogWarning(
                "User {UserId} has insufficient credits for privilege {PrivId}. " +
                "Requires: {Required}, Available: {Available}, Overage cost: ${Cost}",
                userId, privilegeId, requestedQuantity, usage.AllowedValue, totalCost
            );
            
            return new JsonModel
            {
                StatusCode = 402,  // Payment Required
                Message = "Insufficient credits. Payment required for additional usage.",
                data = new
                {
                    Available = false,
                    AvailableCredits = usage.AllowedValue,
                    RequiredCredits = requestedQuantity,
                    ShortfallCredits = requestedQuantity - usage.AllowedValue,
                    CostPerUnit = unitCost,
                    TotalRequired = totalCost,
                    PrivilegeName = planPrivilege.Privilege?.Name
                }
            };
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, 
            "Error checking privilege availability for user {UserId}, privilege {PrivId}",
            userId, privilegeId
        );
        
        return new JsonModel
        {
            StatusCode = 500,
            Message = $"Error checking privilege availability: {ex.Message}"
        };
    }
}
```

### 8.2 Using a Privilege (Full Code)

```csharp
public async Task<JsonModel> UsePrivilegeAsync(
    int userId,
    Guid privilegeId,
    int quantity = 1,
    string relatedEntityId = null)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // 1. Get user's active subscription
        var subscription = await _subscriptionRepository
            .GetActiveSubscriptionByUserIdAsync(userId);
        
        if (subscription == null)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return new JsonModel
            {
                StatusCode = 404,
                Message = "No active subscription"
            };
        }
        
        // 2. Get current usage (lock for update)
        var usage = await _privilegeUsageRepository
            .GetByUserAndPrivilegeAsync(userId, privilegeId);
        
        if (usage == null)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return new JsonModel
            {
                StatusCode = 404,
                Message = "Privilege not found"
            };
        }
        
        // 3. Validate sufficient credits
        if (usage.AllowedValue < quantity)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return new JsonModel
            {
                StatusCode = 400,
                Message = $"Insufficient credits. Available: {usage.AllowedValue}, Requested: {quantity}"
            };
        }
        
        // 4. Update usage counters
        usage.UsedValue += quantity;
        usage.AllowedValue = usage.AllocatedLimit - usage.UsedValue;
        usage.LastUsedAt = DateTime.UtcNow;
        usage.UpdatedBy = userId;
        usage.UpdatedDate = DateTime.UtcNow;
        
        await _privilegeUsageRepository.UpdateAsync(usage);
        
        // 5. Record in usage history
        var historyRecord = new PrivilegeUsageHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubscriptionId = subscription.Id,
            PrivilegeId = privilegeId,
            UsageDate = DateTime.UtcNow,
            QuantityUsed = quantity,
            RemainingAfterUse = usage.AllowedValue,
            UsageType = "Included",  // Included in plan (not overage)
            Cost = 0.00,  // No charge for included usage
            RelatedEntityId = relatedEntityId,
            Notes = $"{quantity} privilege(s) used",
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };
        
        await _privilegeUsageHistoryRepository.CreateAsync(historyRecord);
        
        // 6. Commit transaction
        await _unitOfWork.CommitTransactionAsync();
        
        _logger.LogInformation(
            "User {UserId} used {Quantity} of privilege {PrivId}. Remaining: {Remaining}",
            userId, quantity, privilegeId, usage.AllowedValue
        );
        
        return new JsonModel
        {
            StatusCode = 200,
            Message = "Privilege used successfully",
            data = new
            {
                RemainingCredits = usage.AllowedValue,
                UsedTotal = usage.UsedValue,
                AllocatedTotal = usage.AllocatedLimit
            }
        };
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        
        _logger.LogError(ex, 
            "Error using privilege for user {UserId}, privilege {PrivId}",
            userId, privilegeId
        );
        
        return new JsonModel
        {
            StatusCode = 500,
            Message = $"Error using privilege: {ex.Message}"
        };
    }
}
```

### 8.3 Resetting Privileges on Renewal

```csharp
public async Task ResetPrivilegeUsageAsync(Guid subscriptionId)
{
    var usages = await _privilegeUsageRepository
        .GetBySubscriptionIdAsync(subscriptionId);
    
    var subscription = await _subscriptionRepository
        .GetByIdWithDetailsAsync(subscriptionId);
    
    var planPrivileges = subscription.SubscriptionPlan.PlanPrivileges;
    
    foreach (var usage in usages)
    {
        // Get original limit from plan
        var planPrivilege = planPrivileges
            .FirstOrDefault(pp => pp.PrivilegeId == usage.PrivilegeId);
        
        if (planPrivilege == null) continue;
        
        // Reset to original plan limits
        usage.AllocatedLimit = planPrivilege.Value;
        usage.UsedValue = 0;
        usage.AllowedValue = planPrivilege.Value;
        usage.ResetAt = DateTime.UtcNow;
        usage.UsagePeriodStart = DateTime.UtcNow;
        usage.UsagePeriodEnd = DateTime.UtcNow.AddMonths(1);
        usage.UpdatedDate = DateTime.UtcNow;
        
        await _privilegeUsageRepository.UpdateAsync(usage);
    }
    
    _logger.LogInformation(
        "Reset {Count} privilege usages for subscription {SubId}",
        usages.Count(), subscriptionId
    );
}
```

---

## Key Takeaways

### ✅ Critical Concepts

1. **Three-Level Structure**: Privilege (master) → Plan Config → User Usage
2. **Usage Tracking**: AllocatedLimit, UsedValue, AllowedValue
3. **Overage Detection**: AllowedValue < Requested → 402 Payment Required
4. **Abuse Prevention**: Always use latest plan version for overage pricing
5. **Complete Audit**: Every usage recorded in history with cost & type
6. **Transaction Safety**: Always use Unit of Work for usage operations

### 🔍 Common Operations

| Operation | Check First? | Transaction? | History Record? |
|-----------|--------------|--------------|-----------------|
| Check availability | N/A | No | No |
| Use privilege | Yes | Yes | Yes |
| Purchase overage | Yes | Yes | Yes (marked as "Overage") |
| Reset on renewal | No | Yes (bulk) | No |

---

## Next Steps

Continue to:
- **Guide 05**: Stripe Integration Deep Dive
- **Guide 06**: Automated Background Jobs

---

**Document Version:** 1.0  
**Last Updated:** October 17, 2025


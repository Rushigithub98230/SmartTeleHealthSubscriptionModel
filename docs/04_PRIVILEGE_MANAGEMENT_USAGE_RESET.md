# Privilege Management, Usage Tracking & Reset Mechanism

## Table of Contents
1. [Privilege System Overview](#privilege-system-overview)
2. [Privilege Allocation](#privilege-allocation)
3. [Usage Tracking](#usage-tracking)
4. [Overage Detection](#overage-detection)
5. [Privilege Reset on Renewal](#privilege-reset-on-renewal)
6. [Usage History](#usage-history)

---

## Privilege System Overview

### Concept

Privileges represent **services, features, or resources** that users can access based on their subscription plan. Each privilege has:
- **Limit** (how many times it can be used)
- **Cost** (base cost for pricing, unit cost for overages)
- **Usage Period** (aligned with billing cycle)

### Entity Model

```
┌─────────────────┐
│   Privilege     │  (Definition)
│ - Name          │
│ - Description   │
└────────┬────────┘
         │ *
         │
         │ 1
┌────────▼───────────────────┐
│ SubscriptionPlanPrivilege  │  (Plan Configuration)
│ - Value (limit)            │  (-1=unlimited, 0=disabled, >0=limited)
│ - PrivilegeBaseCost        │  (for plan pricing)
│ - UnitCost                 │  (for overage charges)
└────────┬───────────────────┘
         │ *
         │
         │ 1
┌────────▼─────────────────────────┐
│ UserSubscriptionPrivilegeUsage   │  (Usage Tracking)
│ - AllowedValue               │
│ - UsedValue                      │
│ - UsagePeriodStart/End           │
│ - RemainingValue (computed)      │
└────────┬─────────────────────────┘
         │ 1
         │
         │ *
┌────────▼───────────────┐
│ PrivilegeUsageHistory  │  (Detailed History)
│ - UsedValue            │
│ - UsedAt               │
│ - UsageDate            │
└────────────────────────┘
```

### Privilege Types Examples

- **Teleconsultations** - Video consultations with doctors
- **Chat Sessions** - Text-based consultations
- **Home Visits** - In-person home visits
- **Lab Tests** - Laboratory test orders
- **Prescriptions** - Prescription refills
- **Health Records Access** - Medical record downloads

---

## Privilege Allocation

### When Privileges Are Allocated

Privileges are allocated when:
1. **Subscription is created** (initial allocation)
2. **Plan is upgraded** (recalculate allocations)
3. **Billing cycle renews** (reset for new period)

### Initial Allocation Process

**Service**: `PrivilegeService` (called from `SubscriptionLifecycleService`)

```csharp
public async Task AllocatePrivilegesForSubscriptionAsync(
    Guid subscriptionId, 
    TokenModel tokenModel)
{
    // 1. Get subscription with plan
    var subscription = await _subscriptionRepository
        .GetByIdWithDetailsAsync(subscriptionId);
    
    // 2. Get plan privileges
    var planPrivileges = await _planPrivilegeRepo
        .GetByPlanIdAsync(subscription.SubscriptionPlanId);
    
    _logger.LogInformation(
        "Allocating {Count} privileges for subscription {SubscriptionId}",
        planPrivileges.Count(), subscriptionId);
    
    // 3. Create usage tracking for each privilege
    foreach (var planPrivilege in planPrivileges) {
        var usage = new UserSubscriptionPrivilegeUsage {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            SubscriptionPlanPrivilegeId = planPrivilege.Id,
            PrivilegeId = planPrivilege.PrivilegeId,
            
            // Allocate based on plan configuration
            AllowedValue = planPrivilege.Value,
            // -1 = Unlimited
            // 0 = Disabled
            // >0 = Limited quantity
            
            UsedValue = 0, // Start with zero usage
            
            // Set usage period aligned with billing cycle
            UsagePeriodStart = subscription.StartDate,
            UsagePeriodEnd = subscription.NextBillingDate,
            
            LastUsedAt = null,
            ResetAt = null,
            
            CreatedBy = tokenModel.UserID,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };
        
        await _usageRepo.AddAsync(usage);
        
        _logger.LogDebug(
            "Allocated privilege {PrivilegeName} with limit {Limit} for subscription {SubscriptionId}",
            planPrivilege.Privilege.Name, planPrivilege.Value, subscriptionId);
    }
    
    await _usageRepo.SaveChangesAsync();
    
    _logger.LogInformation(
        "Successfully allocated all privileges for subscription {SubscriptionId}",
        subscriptionId);
}
```

### Allocation Example

**Plan**: Premium Healthcare Plan

| Privilege | Value | Meaning |
|-----------|-------|---------|
| Teleconsultations | 10 | 10 consultations allowed |
| Chat Sessions | -1 | Unlimited chats |
| Home Visits | 2 | 2 home visits allowed |
| Lab Tests | 0 | Lab tests disabled |

**Database Records Created**:
```sql
INSERT INTO UserSubscriptionPrivilegeUsage VALUES
('guid1', 'sub-id', 'plan-priv-1', 'priv-teleconsult', 10, 0, '2025-01-01', '2025-02-01'),
('guid2', 'sub-id', 'plan-priv-2', 'priv-chat', -1, 0, '2025-01-01', '2025-02-01'),
('guid3', 'sub-id', 'plan-priv-3', 'priv-homevisit', 2, 0, '2025-01-01', '2025-02-01');
-- Lab Tests not created (Value = 0 means disabled)
```

---

## Usage Tracking

### Checking Available Privileges

**Service**: `PrivilegeService.GetRemainingPrivilegeAsync()`

```csharp
public async Task<int> GetRemainingPrivilegeAsync(
    Guid subscriptionId, 
    string privilegeName, 
    TokenModel tokenModel)
{
    try {
        // 1. Get plan privilege configuration
        var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
        if (planPrivilege == null) return 0;
        
        // 2. Check if privilege is disabled
        if (planPrivilege.Value == 0) return 0;
        
        // 3. Check if privilege is unlimited
        if (planPrivilege.Value == -1) return int.MaxValue;
        
        // 4. Get current usage tracking
        var usage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
            .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
        
        // 5. If no usage record exists, return plan limit
        if (usage == null) {
            return planPrivilege.Value;
        }
        
        // 6. Calculate remaining
        // IMPORTANT: Use DYNAMIC AllowedValue (can increase with upfront credits)
        var remaining = usage.AllowedValue == -1 
            ? int.MaxValue 
            : Math.Max(0, usage.AllowedValue - usage.UsedValue);
        
        _logger.LogInformation(
            "Privilege {PrivilegeName} for subscription {SubscriptionId}: {Remaining} remaining (Allowed: {Allowed}, Used: {Used})",
            privilegeName, subscriptionId, remaining, usage.AllowedValue, usage.UsedValue);
        
        return remaining;
    }
    catch (Exception ex) {
        _logger.LogError(ex, 
            "Error getting remaining privilege {PrivilegeName} for subscription {SubscriptionId}",
            privilegeName, subscriptionId);
        return 0; // Safe default
    }
}
```

### Using a Privilege

**Service**: `PrivilegeService.UsePrivilegeAsync()`

```csharp
public async Task<bool> UsePrivilegeAsync(
    Guid subscriptionId, 
    string privilegeName, 
    int amount, 
    TokenModel tokenModel)
{
    try {
        // 1. Validate amount
        if (amount <= 0) return false;
        
        // 2. Get plan privilege
        var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
        if (planPrivilege == null) return false;
        
        // 3. Check if disabled
        if (planPrivilege.Value == 0) return false;
        
        // 4. Get usage tracking
        var usage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
            .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
        
        if (usage == null) {
            // Create initial usage record
            usage = new UserSubscriptionPrivilegeUsage {
                Id = Guid.NewGuid(),
                SubscriptionId = subscriptionId,
                SubscriptionPlanPrivilegeId = planPrivilege.Id,
                PrivilegeId = planPrivilege.PrivilegeId,
                AllowedValue = planPrivilege.Value,
                UsedValue = 0,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = DateTime.UtcNow.AddMonths(1),
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow
            };
            await _usageRepo.AddAsync(usage);
        }
        
        // 5. For unlimited, always allow
        if (usage.AllowedValue == -1) {
            usage.UsedValue += amount;
            usage.LastUsedAt = DateTime.UtcNow;
            await _usageRepo.UpdateAsync(usage);
            
            await RecordUsageHistoryAsync(usage, amount, tokenModel);
            return true;
        }
        
        // 6. For limited, check remaining
        var remaining = usage.AllowedValue - usage.UsedValue;
        
        if (remaining <= 0) {
            // OVERAGE SCENARIO - will be charged
            _logger.LogWarning(
                "Privilege {PrivilegeName} exhausted for subscription {SubscriptionId}. Usage will incur overage charges.",
                privilegeName, subscriptionId);
        }
        
        // 7. Increment usage (even if overage)
        usage.UsedValue += amount;
        usage.LastUsedAt = DateTime.UtcNow;
        await _usageRepo.UpdateAsync(usage);
        
        // 8. Record history
        await RecordUsageHistoryAsync(usage, amount, tokenModel);
        
        _logger.LogInformation(
            "Used {Amount} of privilege {PrivilegeName} for subscription {SubscriptionId}. Remaining: {Remaining}",
            amount, privilegeName, subscriptionId, Math.Max(0, remaining - amount));
        
        return true;
    }
    catch (Exception ex) {
        _logger.LogError(ex, 
            "Error using privilege {PrivilegeName} for subscription {SubscriptionId}",
            privilegeName, subscriptionId);
        return false;
    }
}
```

---

## Overage Detection

### When Overages Are Detected

Overages occur when `UsedValue > AllowedValue`.

**Detection happens in**: `SubscriptionBillingService.ProcessPrivilegeUsageAsync()`

### Overage Logic

```csharp
// After usage is recorded...

// Check if overage occurred
bool isOverage = false;
decimal overageCharge = 0;

if (planPrivilege.Value != -1) { // Not unlimited
    if (privilegeUsage.UsedValue > privilegeUsage.AllowedValue) {
        isOverage = true;
        
        // CRITICAL: Use LATEST plan version pricing (anti-abuse measure)
        var latestPlanVersion = await GetLatestPlanVersionAsync(
            subscription.SubscriptionPlanId);
        
        var latestPrivilege = await _subscriptionPlanRepository
            .GetPlanPrivilegeAsync(latestPlanVersion.Id, privilegeId);
        
        // Charge at latest version's UnitCost
        overageCharge = latestPrivilege.UnitCost * 
            (privilegeUsage.UsedValue - privilegeUsage.AllowedValue);
        
        _logger.LogWarning(
            "Overage detected for subscription {SubscriptionId}, privilege {PrivilegeName}. " +
            "Exceeded by {Overage}. Charge: {Charge}",
            subscription.Id, privilege.Name, 
            privilegeUsage.UsedValue - privilegeUsage.AllowedValue, 
            overageCharge);
    }
}

// Create overage billing if charge exists
if (isOverage && overageCharge > 0) {
    await CreateOverageBillingRecordAsync(
        subscription, privilegeId, overageCharge, tokenModel);
    
    // Notify user
    await _notificationService.SendOverageNotificationAsync(
        subscription.UserId, privilege.Name, overageCharge);
}
```

### Why Latest Version Pricing?

**Anti-Abuse Measure**: Prevents users on old cheap plans from exploiting lower overage rates.

**Example**:
- **Old Plan v1**: Teleconsultation overage = $5
- **New Plan v2**: Teleconsultation overage = $15
- **User on v1** exceeds limit → Charged at **v2 rate ($15)**

This encourages users to either stay within limits or upgrade to latest plan.

---

## Privilege Reset on Renewal

### When Reset Occurs

Privileges are reset when:
1. **Successful payment processed** (recurring billing)
2. **Manual renewal triggered**
3. **New billing period starts**

### Reset Process

**Called from**: `PaymentService.UpdatePaymentRecordsAsync()` after successful payment

```csharp
private async Task ResetPrivilegesAsync(Guid subscriptionId, TokenModel tokenModel)
{
    _logger.LogInformation(
        "Resetting privileges for subscription {SubscriptionId} (new billing period)",
        subscriptionId);
    
    // 1. Get subscription
    var subscription = await _subscriptionRepository
        .GetByIdWithDetailsAsync(subscriptionId);
    
    // 2. Get all privilege usages
    var usages = await _usageRepo.GetBySubscriptionIdAsync(subscriptionId);
    
    // 3. Reset each privilege
    foreach (var usage in usages) {
        // Get plan privilege (in case limits changed)
        var planPrivilege = await _planPrivilegeRepo
            .GetByIdAsync(usage.SubscriptionPlanPrivilegeId);
        
        // Reset usage
        usage.UsedValue = 0;
        usage.AllowedValue = planPrivilege.Value; // Refresh from plan
        usage.LastUsedAt = null;
        usage.ResetAt = DateTime.UtcNow;
        
        // Update usage period
        usage.UsagePeriodStart = subscription.LastBillingDate.Value;
        usage.UsagePeriodEnd = subscription.NextBillingDate;
        
        usage.UpdatedBy = tokenModel.UserID;
        usage.UpdatedDate = DateTime.UtcNow;
        
        await _usageRepo.UpdateAsync(usage);
        
        _logger.LogDebug(
            "Reset privilege {PrivilegeId} for subscription {SubscriptionId}. New period: {Start} to {End}",
            usage.PrivilegeId, subscriptionId, usage.UsagePeriodStart, usage.UsagePeriodEnd);
    }
    
    await _usageRepo.SaveChangesAsync();
    
    _logger.LogInformation(
        "Successfully reset {Count} privileges for subscription {SubscriptionId}",
        usages.Count(), subscriptionId);
}
```

### Reset Trigger Flow

```
Billing Date Reached
    │
    ▼
Automated Billing Service
    │
    ▼
Create Billing Record
    │
    ▼
Process Payment
    │
    ▼
Payment Success? ──No──> Retry Later
    │ Yes
    ▼
Update Payment Records
    │
    ▼
Update Subscription Dates
  - LastBillingDate = Now
  - NextBillingDate = Now + BillingCycle
    │
    ▼
Reset Privileges ◄────────
  - UsedValue = 0
  - AllowedValue = Plan Value
  - UsagePeriodStart = LastBillingDate
  - UsagePeriodEnd = NextBillingDate
    │
    ▼
Send Renewal Confirmation
```

### Important: Reset Only on Payment Success

**Critical Business Rule**: Privileges are ONLY reset when payment succeeds.

```csharp
if (paymentResult.StatusCode == 200) {
    // Payment succeeded
    
    // 1. Update subscription billing dates
    await UpdateSubscriptionBillingDatesAsync(subscriptionId, tokenModel);
    
    // 2. Reset privileges for new period
    await ResetPrivilegesAsync(subscriptionId, tokenModel);
    
    // 3. Send notifications
    await SendRenewalConfirmationAsync(subscriptionId, tokenModel);
}
else {
    // Payment failed - NO RESET
    // User keeps old usage, no new period starts
    
    await HandlePaymentFailureAsync(subscriptionId, paymentResult);
}
```

---

## Usage History

### Recording Usage Events

**Service**: `PrivilegeService.RecordUsageHistoryAsync()`

```csharp
private async Task RecordUsageHistoryAsync(
    UserSubscriptionPrivilegeUsage usage, 
    int amount, 
    TokenModel tokenModel)
{
    var history = new PrivilegeUsageHistory {
        Id = Guid.NewGuid(),
        UserSubscriptionPrivilegeUsageId = usage.Id,
        UsedValue = amount,
        UsedAt = DateTime.UtcNow,
        UsageDate = DateTime.UtcNow.Date,
        UsageWeek = $"{DateTime.UtcNow:yyyy}-{GetWeekNumber(DateTime.UtcNow):D2}",
        UsageMonth = $"{DateTime.UtcNow:yyyy-MM}",
        Notes = $"Privilege used by user {tokenModel.UserID}",
        CreatedBy = tokenModel.UserID,
        CreatedDate = DateTime.UtcNow
    };
    
    await _usageHistoryRepo.AddAsync(history);
    await _usageHistoryRepo.SaveChangesAsync();
}
```

### Querying Usage History

```csharp
public async Task<JsonModel> GetPrivilegeUsageHistoryAsync(
    Guid subscriptionId, 
    Guid? privilegeId, 
    DateTime? startDate, 
    DateTime? endDate, 
    TokenModel tokenModel)
{
    // Get all usage records for subscription
    var usages = await _usageRepo.GetBySubscriptionIdAsync(subscriptionId);
    
    // Filter by privilege if specified
    if (privilegeId.HasValue) {
        usages = usages.Where(u => u.PrivilegeId == privilegeId.Value);
    }
    
    // Get history for each usage
    var historyList = new List<PrivilegeUsageHistory>();
    foreach (var usage in usages) {
        var history = await _usageHistoryRepo
            .GetByUsageIdAsync(usage.Id);
        
        // Filter by date range
        if (startDate.HasValue) {
            history = history.Where(h => h.UsageDate >= startDate.Value);
        }
        if (endDate.HasValue) {
            history = history.Where(h => h.UsageDate <= endDate.Value);
        }
        
        historyList.AddRange(history);
    }
    
    return Success(historyList.OrderByDescending(h => h.UsedAt));
}
```

### Usage Analytics

```csharp
public async Task<JsonModel> GetUsageAnalyticsAsync(
    Guid subscriptionId, 
    TokenModel tokenModel)
{
    var usages = await _usageRepo.GetBySubscriptionIdAsync(subscriptionId);
    
    var analytics = new {
        TotalPrivileges = usages.Count(),
        UnlimitedPrivileges = usages.Count(u => u.AllowedValue == -1),
        LimitedPrivileges = usages.Count(u => u.AllowedValue > 0),
        DisabledPrivileges = usages.Count(u => u.AllowedValue == 0),
        
        TotalUsage = usages.Sum(u => u.UsedValue),
        TotalAllowed = usages.Where(u => u.AllowedValue > 0).Sum(u => u.AllowedValue),
        
        ExhaustedPrivileges = usages.Count(u => u.IsExhausted),
        NearLimitPrivileges = usages.Count(u => 
            u.AllowedValue > 0 && 
            u.UsagePercentage >= 80 && 
            !u.IsExhausted),
        
        PrivilegeBreakdown = usages.Select(u => new {
            PrivilegeName = u.SubscriptionPlanPrivilege.Privilege.Name,
            Allowed = u.AllowedValue,
            Used = u.UsedValue,
            Remaining = u.RemainingValue,
            UsagePercentage = u.UsagePercentage,
            IsExhausted = u.IsExhausted,
            IsUnlimited = u.IsUnlimited
        })
    };
    
    return Success(analytics);
}
```

---

## Background Monitoring

### Privilege Reset Background Service

**Service**: `PrivilegeResetBackgroundService`

**Purpose**: Monitors for expired usage periods (not reset due to payment issues)

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    // Wait 1 minute before first run
    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    
    while (!stoppingToken.IsCancellationRequested)
    {
        try {
            await CheckExpiredPrivilegeUsagesAsync(stoppingToken);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error in privilege reset background service");
        }
        
        // Run daily
        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
    }
}

private async Task CheckExpiredPrivilegeUsagesAsync(CancellationToken stoppingToken)
{
    var now = DateTime.UtcNow;
    
    // Find usage records where period has expired but not reset
    var expiredUsages = await _context.UserSubscriptionPrivilegeUsages
        .Where(u => u.UsagePeriodEnd < now && u.UsedValue > 0)
        .Take(100)
        .ToListAsync(stoppingToken);
    
    if (expiredUsages.Any()) {
        _logger.LogWarning(
            "Found {Count} expired privilege usages. " +
            "These should reset on next successful billing. " +
            "If billing is delayed, privileges may be locked until payment succeeds.",
            expiredUsages.Count);
        
        // Log details for admin review
        foreach (var usage in expiredUsages.Take(10)) {
            _logger.LogInformation(
                "Expired privilege: SubscriptionId={SubscriptionId}, " +
                "Privilege={PrivilegeName}, Expired={ExpiredDate}, DaysSinceExpiry={Days}",
                usage.SubscriptionId,
                usage.SubscriptionPlanPrivilege?.Privilege?.Name ?? "Unknown",
                usage.UsagePeriodEnd,
                (now - usage.UsagePeriodEnd).Days);
        }
    }
}
```

---

## Summary

The privilege system provides:
- **Flexible limits** (unlimited, disabled, or fixed)
- **Usage tracking** per privilege per subscription
- **Overage detection** with latest version pricing
- **Automatic reset** on successful billing
- **Detailed history** for auditing and analytics
- **Background monitoring** for expired periods
- **Dynamic allocation** (can be increased with credit purchases)

**Key Principle**: Privileges are **tied to billing periods** and **only reset on successful payment**.

**Next**: See [05_PLAN_VERSIONING_MIGRATION.md](./05_PLAN_VERSIONING_MIGRATION.md) for plan versioning details.

---

*Document Version: 1.0*  
*Last Updated: 2025*




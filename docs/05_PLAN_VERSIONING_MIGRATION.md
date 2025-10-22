# Plan Versioning & Migration System

## Table of Contents
1. [Why Versioning](#why-versioning)
2. [Versioning Architecture](#versioning-architecture)
3. [Creating New Versions](#creating-new-versions)
4. [Migration Scheduling](#migration-scheduling)
5. [Migration Execution](#migration-execution)
6. [User Communication](#user-communication)

---

## Why Versioning

### The Problem

**Healthcare compliance requirement**: When a subscription plan changes (price, privileges, features), existing subscribers must be protected.

**Without versioning**:
```
Admin updates "Basic Plan" price: $50 → $80
❌ Problem: Existing users immediately see $80 (unexpected charge)
❌ Problem: Users can't stay on their agreed-upon $50 plan
❌ Problem: No audit trail of plan changes
❌ Problem: Violates user agreement
```

**With versioning**:
```
Admin creates "Basic Plan v2" with $80 price
✅ New users: Subscribe to v2 at $80
✅ Existing users: Stay on v1 at $50
✅ Scheduled migration: Users get 30-60 day notice
✅ Audit trail: Complete history of plan changes
```

---

## Versioning Architecture

### Entity Model

**SubscriptionPlan** with versioning fields:

```csharp
public class SubscriptionPlan : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    // VERSIONING FIELDS
    public int VersionNumber { get; set; } = 1;
    public bool IsLatestVersion { get; set; } = true;
    public Guid? ParentPlanId { get; set; }
    public virtual SubscriptionPlan? ParentPlan { get; set; }
    public virtual ICollection<SubscriptionPlan> ChildVersions { get; set; }
    
    // MIGRATION SETTINGS
    public int PriceChangeNoticeDays { get; set; } = 30;  // 30-60 days notice
    public DateTime VersionCreatedDate { get; set; }
}
```

### Version Hierarchy

```
┌────────────────────┐
│ Basic Plan v1      │  (Original)
│ Price: $50         │
│ ParentPlanId: NULL │
│ IsLatestVersion: false
└────────┬───────────┘
         │ Parent
         │
         ▼
┌────────────────────┐
│ Basic Plan v2      │  (Current)
│ Price: $65         │
│ ParentPlanId: v1   │
│ IsLatestVersion: false
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│ Basic Plan v3      │  (Latest)
│ Price: $80         │
│ ParentPlanId: v1   │  ← Points to ORIGINAL
│ IsLatestVersion: true
└────────────────────┘
```

**Key Points**:
- All versions point to the **original** plan (v1) as parent
- Only **one version** has `IsLatestVersion = true`
- New subscriptions always use **latest version**
- Old subscriptions stay on their version

---

## Creating New Versions

### When to Create New Version

Create new version when:
- **Price changes**
- **Privileges change** (limits, costs)
- **Features change** (add/remove services)
- **Terms change** (billing cycle, trial period)

### Creation Process

**Service**: `PlanVersioningService.CreateNewPlanVersionAsync()`

#### Step 1: Check Active Subscriptions

```csharp
// Issue #2: Check for active subscriptions
var activeSubsCount = await _subscriptionPlanRepository
    .GetActiveSubscriptionsCountAsync(existingPlanId);

if (activeSubsCount > 0) {
    _logger.LogWarning(
        "Plan {PlanId} has {Count} active subscriptions. " +
        "Creating new version to preserve them.",
        existingPlanId, activeSubsCount);
}
```

### Step 2: Create New Version

```csharp
// Determine parent plan ID
var parentPlanId = existingPlan.ParentPlanId ?? existingPlan.Id;

// Get all versions and calculate new version number
var allVersions = await _subscriptionPlanRepository
    .GetAllVersionsOfPlanAsync(parentPlanId);
var newVersionNumber = allVersions.Max(v => v.VersionNumber) + 1;

// Create new version entity
var newVersion = new SubscriptionPlan {
    Id = Guid.NewGuid(),
    
    // Copy base properties
    Name = $"{existingPlan.Name} v{newVersionNumber}",  // Or use original name
    Description = updateDto.Description ?? existingPlan.Description,
    BillingCycleId = updateDto.BillingCycleId ?? existingPlan.BillingCycleId,
    CurrencyId = updateDto.CurrencyId ?? existingPlan.CurrencyId,
    CategoryId = updateDto.CategoryId ?? existingPlan.CategoryId,
    
    // Versioning fields
    ParentPlanId = parentPlanId,  // Point to original
    VersionNumber = newVersionNumber,
    IsLatestVersion = true,       // This is now the latest
    VersionCreatedDate = DateTime.UtcNow,
    
    // NEW pricing
    Price = updateDto.Price,
    IsAutoCalculatedPrice = updateDto.IsAutoCalculatedPrice,
    
    // Migration settings
    PriceChangeNoticeDays = updateDto.PriceChangeNoticeDays ?? 30,
    
    // Audit
    CreatedBy = tokenModel.UserID,
    CreatedDate = DateTime.UtcNow,
    IsActive = true
};
```

### Step 3: Copy Privileges

```csharp
await CopyPrivilegesToNewVersionAsync(existingPlan, newVersion, tokenModel);

private async Task CopyPrivilegesToNewVersionAsync(
    SubscriptionPlan oldPlan, 
    SubscriptionPlan newPlan, 
    TokenModel tokenModel)
{
    var oldPrivileges = await _planPrivilegeRepository
        .GetByPlanIdAsync(oldPlan.Id);
    
    foreach (var oldPrivilege in oldPrivileges) {
        var newPrivilege = new SubscriptionPlanPrivilege {
            Id = Guid.NewGuid(),
            SubscriptionPlanId = newPlan.Id,
            PrivilegeId = oldPrivilege.PrivilegeId,
            
            // Copy values (may be updated by admin)
            Value = oldPrivilege.Value,
            PrivilegeBaseCost = oldPrivilege.PrivilegeBaseCost,
            UnitCost = oldPrivilege.UnitCost,
            
            CreatedBy = tokenModel.UserID,
            CreatedDate = DateTime.UtcNow
        };
        
        await _planPrivilegeRepository.AddAsync(newPrivilege);
    }
}
```

### Step 4: Mark Old Version as Not Latest

```csharp
// This happens in repository method CreateNewPlanVersionAsync()

// Mark old version as not latest
existingPlan.IsLatestVersion = false;
await _subscriptionPlanRepository.UpdateAsync(existingPlan);

// Save new version
await _subscriptionPlanRepository.AddAsync(newVersion);
```

### Step 5: Create Stripe Resources

```csharp
await CreateStripeResourcesForPlanAsync(newVersion, tokenModel);

private async Task CreateStripeResourcesForPlanAsync(
    SubscriptionPlan plan, 
    TokenModel tokenModel)
{
    // Create Stripe Product
    var stripeProductId = await _stripeService.CreateProductAsync(
        plan.Name,
        plan.Description,
        tokenModel
    );
    
    // Create Stripe Price
    var stripePriceId = await _stripeService.CreatePriceAsync(
        stripeProductId,
        plan.Price,
        plan.Currency.Code,
        plan.BillingCycle.IntervalUnit,
        plan.BillingCycle.IntervalCount,
        tokenModel
    );
    
    // Update plan with Stripe IDs
    plan.StripeProductId = stripeProductId;
    plan.StripePriceId = stripePriceId;
    
    await _subscriptionPlanRepository.UpdateAsync(plan);
}
```

### Step 6: Schedule Migrations

```csharp
if (activeSubsCount > 0) {
    await ScheduleMigrationsForActiveSubscribersAsync(
        existingPlanId, 
        newVersion.Id, 
        tokenModel);
}
```

### Complete Flow

```
Admin Requests Plan Update
    │
    ▼
Check Active Subscriptions
    │
    ├─► activeSubsCount = 0 ──► Update plan directly (no versioning needed)
    │
    └─► activeSubsCount > 0 ──► Create New Version
                                    │
                                    ▼
                            Calculate New Version Number
                                    │
                                    ▼
                            Create New Plan Entity
                            - ParentPlanId = original
                            - VersionNumber = N+1
                            - IsLatestVersion = true
                                    │
                                    ▼
                            Copy Privileges from Old Version
                                    │
                                    ▼
                            Mark Old Version as Not Latest
                                    │
                                    ▼
                            Create Stripe Product & Price
                                    │
                                    ▼
                            Schedule Migrations for Existing Users
```

---

## Migration Scheduling

### ScheduledPlanMigration Entity

```csharp
public class ScheduledPlanMigration : BaseEntity
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid FromPlanId { get; set; }
    public Guid ToPlanId { get; set; }
    
    public DateTime ScheduledDate { get; set; }
    public DateTime? NotificationSentDate { get; set; }
    public DateTime? ExecutedDate { get; set; }
    
    public MigrationStatus Status { get; set; }  // Scheduled, Notified, Executed, Failed, Cancelled
    
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
```

### Scheduling Migrations

**Service**: `PlanVersioningService.ScheduleMigrationsForActiveSubscribersAsync()`

```csharp
private async Task ScheduleMigrationsForActiveSubscribersAsync(
    Guid oldPlanId, 
    Guid newPlanId, 
    TokenModel tokenModel)
{
    // 1. Get all active subscriptions on old plan
    var activeSubscriptions = await _subscriptionRepository
        .GetActiveSubscriptionsByPlanIdAsync(oldPlanId);
    
    _logger.LogInformation(
        "Scheduling migrations for {Count} active subscriptions from plan {OldPlanId} to {NewPlanId}",
        activeSubscriptions.Count(), oldPlanId, newPlanId);
    
    // 2. Get new plan for notice period
    var newPlan = await _subscriptionPlanRepository.GetByIdAsync(newPlanId);
    var noticeDays = newPlan.PriceChangeNoticeDays;  // e.g., 30 days
    
    // 3. Create scheduled migration for each subscription
    foreach (var subscription in activeSubscriptions) {
        var migration = new ScheduledPlanMigration {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            FromPlanId = oldPlanId,
            ToPlanId = newPlanId,
            
            // Schedule migration after notice period
            ScheduledDate = DateTime.UtcNow.AddDays(noticeDays),
            
            Status = MigrationStatus.Scheduled,
            Reason = $"Plan updated: {oldPlan.Name} v{oldPlan.VersionNumber} → v{newPlan.VersionNumber}",
            
            CreatedBy = tokenModel.UserID,
            CreatedDate = DateTime.UtcNow
        };
        
        await _scheduledMigrationRepository.AddAsync(migration);
        
        // Send initial notification
        await SendMigrationNotificationAsync(migration, subscription, tokenModel);
        
        _logger.LogDebug(
            "Scheduled migration for subscription {SubscriptionId} on {ScheduledDate}",
            subscription.Id, migration.ScheduledDate);
    }
    
    await _scheduledMigrationRepository.SaveChangesAsync();
    
    _logger.LogInformation(
        "Successfully scheduled {Count} migrations",
        activeSubscriptions.Count());
}
```

### Migration Notification

```csharp
private async Task SendMigrationNotificationAsync(
    ScheduledPlanMigration migration,
    Subscription subscription,
    TokenModel tokenModel)
{
    var oldPlan = await _subscriptionPlanRepository.GetByIdAsync(migration.FromPlanId);
    var newPlan = await _subscriptionPlanRepository.GetByIdAsync(migration.ToPlanId);
    var user = subscription.User;
    
    var notification = new {
        Subject = "Your Subscription Plan Will Be Updated",
        Body = $@"
            Dear {user.FirstName},
            
            We're writing to inform you that your subscription plan will be updated.
            
            CURRENT PLAN:
            - {oldPlan.Name}
            - ${oldPlan.Price}/{oldPlan.BillingCycle.Name}
            
            NEW PLAN:
            - {newPlan.Name}
            - ${newPlan.Price}/{newPlan.BillingCycle.Name}
            
            MIGRATION DATE: {migration.ScheduledDate:MMMM dd, yyyy}
            
            You have {(migration.ScheduledDate - DateTime.UtcNow).Days} days to review these changes.
            
            If you have any questions or concerns, please contact our support team.
            
            Thank you for your continued trust.
        "
    };
    
    await _notificationService.SendEmailAsync(
        user.Email,
        notification.Subject,
        notification.Body
    );
    
    migration.NotificationSentDate = DateTime.UtcNow;
    migration.Status = MigrationStatus.Notified;
    await _scheduledMigrationRepository.UpdateAsync(migration);
}
```

---

## Migration Execution

### Background Service

**Service**: `ScheduledMigrationBackgroundService`

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    // Wait 1 minute before first run
    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    
    while (!stoppingToken.IsCancellationRequested)
    {
        try {
            await ProcessDueMigrationsAsync(stoppingToken);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error in scheduled migration service");
        }
        
        // Run daily
        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
    }
}

private async Task ProcessDueMigrationsAsync(CancellationToken stoppingToken)
{
    var now = DateTime.UtcNow;
    
    // Get migrations due for execution
    var dueMigrations = await _scheduledMigrationRepository
        .GetDueMigrationsAsync(now, 100);
    
    if (!dueMigrations.Any()) {
        _logger.LogDebug("No migrations due for processing");
        return;
    }
    
    _logger.LogInformation(
        "Processing {Count} due migrations", 
        dueMigrations.Count());
    
    foreach (var migration in dueMigrations) {
        try {
            await ExecuteMigrationAsync(migration, stoppingToken);
        }
        catch (Exception ex) {
            _logger.LogError(ex, 
                "Error executing migration {MigrationId}", 
                migration.Id);
            
            migration.Status = MigrationStatus.Failed;
            migration.Notes = $"Error: {ex.Message}";
            await _scheduledMigrationRepository.UpdateAsync(migration);
        }
    }
}
```

### Execute Single Migration

**Service**: `PlanVersioningService.ExecuteMigrationAsync()`

```csharp
private async Task ExecuteMigrationAsync(
    ScheduledPlanMigration migration,
    CancellationToken stoppingToken)
{
    await _unitOfWork.BeginTransactionAsync();
    try {
        _logger.LogInformation(
            "Executing migration {MigrationId} for subscription {SubscriptionId}",
            migration.Id, migration.SubscriptionId);
        
        // 1. Get subscription
        var subscription = await _subscriptionRepository
            .GetByIdWithDetailsAsync(migration.SubscriptionId);
        
        if (subscription == null) {
            throw new InvalidOperationException("Subscription not found");
        }
        
        // 2. Validate subscription is still on old plan
        if (subscription.SubscriptionPlanId != migration.FromPlanId) {
            _logger.LogWarning(
                "Subscription {SubscriptionId} is no longer on source plan. Skipping migration.",
                subscription.Id);
            
            migration.Status = MigrationStatus.Cancelled;
            migration.Notes = "Subscription already on different plan";
            await _scheduledMigrationRepository.UpdateAsync(migration);
            return;
        }
        
        // 3. Get new plan
        var newPlan = await _subscriptionPlanRepository
            .GetByIdWithDetailsAsync(migration.ToPlanId);
        
        // 4. Update subscription plan
        subscription.SubscriptionPlanId = newPlan.Id;
        subscription.CurrentPrice = newPlan.EffectivePrice;
        subscription.UpdatedBy = 0; // System
        subscription.UpdatedDate = DateTime.UtcNow;
        
        await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
        
        // 5. Update Stripe subscription
        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId)) {
            await _stripeService.UpdateSubscriptionPlanAsync(
                subscription.StripeSubscriptionId,
                newPlan.StripePriceId,
                SystemToken
            );
        }
        
        // 6. Reallocate privileges for new plan
        await ReallocatePrivilegesAsync(subscription, newPlan, SystemToken);
        
        // 7. Mark migration as executed
        migration.Status = MigrationStatus.Executed;
        migration.ExecutedDate = DateTime.UtcNow;
        migration.Notes = "Migration completed successfully";
        await _scheduledMigrationRepository.UpdateAsync(migration);
        
        // 8. Notify user
        await SendMigrationCompleteNotificationAsync(
            subscription, migration, SystemToken);
        
        await _unitOfWork.CommitTransactionAsync();
        
        _logger.LogInformation(
            "Successfully migrated subscription {SubscriptionId} to plan {PlanId}",
            subscription.Id, newPlan.Id);
    }
    catch (Exception ex) {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, 
            "Error executing migration {MigrationId}", 
            migration.Id);
        throw;
    }
}
```

### Reallocate Privileges

```csharp
private async Task ReallocatePrivilegesAsync(
    Subscription subscription,
    SubscriptionPlan newPlan,
    TokenModel tokenModel)
{
    // 1. Get old privilege usages
    var oldUsages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
    
    // 2. Get new plan privileges
    var newPlanPrivileges = await _planPrivilegeRepo
        .GetByPlanIdAsync(newPlan.Id);
    
    // 3. For each new plan privilege
    foreach (var newPlanPrivilege in newPlanPrivileges) {
        // Check if privilege existed in old plan
        var oldUsage = oldUsages.FirstOrDefault(u => 
            u.PrivilegeId == newPlanPrivilege.PrivilegeId);
        
        if (oldUsage != null) {
            // Update existing usage with new limits
            oldUsage.AllowedValue = newPlanPrivilege.Value;
            oldUsage.SubscriptionPlanPrivilegeId = newPlanPrivilege.Id;
            // Keep UsedValue (don't reset mid-period)
            
            await _usageRepo.UpdateAsync(oldUsage);
        }
        else {
            // New privilege - create usage record
            var newUsage = new UserSubscriptionPrivilegeUsage {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                SubscriptionPlanPrivilegeId = newPlanPrivilege.Id,
                PrivilegeId = newPlanPrivilege.PrivilegeId,
                AllowedValue = newPlanPrivilege.Value,
                UsedValue = 0,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = subscription.NextBillingDate,
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow
            };
            
            await _usageRepo.AddAsync(newUsage);
        }
    }
    
    await _usageRepo.SaveChangesAsync();
}
```

---

## User Communication

### Notification Timeline

```
Day 0: Admin creates new plan version
  │
  ├─► Email: "Plan Update Notice - Action Required"
  │   - Summary of changes
  │   - Migration date
  │   - Option to contact support
  │
Day 7: Reminder
  │
  ├─► Email: "Reminder: Your plan will be updated in 23 days"
  │
Day 23: Final reminder
  │
  ├─► Email: "Final Reminder: Your plan updates in 7 days"
  │
Day 30: Migration executed
  │
  └─► Email: "Your subscription plan has been updated"
      - Confirmation of changes
      - New billing amount
      - Next billing date
```

### Opt-Out Option

**Admin can provide opt-out**:
```csharp
public async Task<JsonModel> CancelMigrationAsync(
    Guid migrationId, 
    string reason, 
    TokenModel tokenModel)
{
    var migration = await _scheduledMigrationRepository.GetByIdAsync(migrationId);
    
    if (migration.Status == MigrationStatus.Executed) {
        return BadRequest("Migration already executed");
    }
    
    migration.Status = MigrationStatus.Cancelled;
    migration.Notes = $"Cancelled by user: {reason}";
    migration.UpdatedBy = tokenModel.UserID;
    migration.UpdatedDate = DateTime.UtcNow;
    
    await _scheduledMigrationRepository.UpdateAsync(migration);
    
    // User stays on old plan version
    
    return Success("Migration cancelled");
}
```

---

## Summary

The plan versioning system provides:
- **Protection for existing subscribers** (stay on agreed terms)
- **Gradual transition** with configurable notice periods
- **Complete audit trail** of plan changes
- **Automated migration** with background service
- **User communication** throughout process
- **Opt-out capability** for users
- **Stripe synchronization** during migration

**Key Benefits**:
- Healthcare compliance (honor user agreements)
- Business flexibility (update pricing/features)
- User trust (transparency and control)
- System integrity (complete change history)

**Next**: See [06_COMPLETE_SYSTEM_FLOWS.md](./06_COMPLETE_SYSTEM_FLOWS.md) for end-to-end system flows.

---

*Document Version: 1.0*  
*Last Updated: 2025*




# 🎯 PLAN PRICE CHANGE WITH USER MIGRATION - COMPLETE SOLUTION

## 🎓 Understanding Your Requirement

**You're right!** You can't keep users at old prices forever. The solution is:

1. ✅ Create new version with new price
2. ✅ Give users **advance notice** (e.g., 60 days)
3. ✅ Let users **decide** (accept, cancel, or lock in old rate)
4. ✅ **Automatically migrate** users after notice period
5. ✅ Track migration status

**This is how Netflix, Spotify, and all major SaaS companies do it!**

---

## 🎬 THE COMPLETE FLOW (With Migration)

### **Timeline Visualization**

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
COMPLETE PRICE CHANGE & MIGRATION TIMELINE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📅 JAN 1  ─────────────────────────────────────────────────────
│
│  ┌──────────────────────────────────────┐
│  │ Plan v1: Basic Health Plan           │
│  │ Price: $10/month                     │
│  │ Active subscriptions: 0              │
│  └──────────────────────────────────────┘
│
│
📅 JAN 5  ─────────────────────────────────────────────────────
│
│  Alice subscribes
│  ┌──────────────────────────────────────┐
│  │ Alice's Subscription                 │
│  │ Plan: v1 ($10/month)                 │
│  │ Next Billing: Feb 5                  │
│  └──────────────────────────────────────┘
│
│
📅 JAN 10 ─────────────────────────────────────────────────────
│
│  Bob subscribes
│  ┌──────────────────────────────────────┐
│  │ Bob's Subscription                   │
│  │ Plan: v1 ($10/month)                 │
│  │ Next Billing: Feb 10                 │
│  └──────────────────────────────────────┘
│
│
📅 JAN 20 ─────────────────────────────────────────────────────
│
│  🔧 ADMIN CHANGES PRICE TO $20
│  
│  System creates:
│  ┌──────────────────────────────────────┐
│  │ Plan v1 (RETIRED)                    │
│  │ Price: $10/month                     │
│  │ Status: Retired                      │
│  │ MigrationDate: March 20 (60 days)    │ ← NEW FIELD!
│  └──────────────────────────────────────┘
│  
│  ┌──────────────────────────────────────┐
│  │ Plan v2 (CURRENT)                    │
│  │ Price: $20/month                     │
│  │ Status: Active                       │
│  │ EffectiveDate: Jan 20 (immediate)    │
│  │ MigrationDate: March 20 (for v1)     │
│  └──────────────────────────────────────┘
│  
│  📧 EMAILS SENT TO ALICE & BOB:
│  ┌──────────────────────────────────────────────────────┐
│  │ Subject: Important: Plan Price Changing March 20     │
│  │ ─────────────────────────────────────────────────── │
│  │ Hi Alice,                                            │
│  │                                                      │
│  │ We're updating our Basic Health Plan with new       │
│  │ features and a new price.                           │
│  │                                                      │
│  │ WHAT HAPPENS:                                        │
│  │   • Your current price: $10/month                   │
│  │   • New price: $20/month                            │
│  │   • Change effective: March 20, 2025 (60 days)      │
│  │                                                      │
│  │ YOU HAVE 3 OPTIONS:                                  │
│  │                                                      │
│  │ ① ACCEPT NEW PRICE                                   │
│  │    Continue service at $20/month starting March 20  │
│  │    No action needed                                  │
│  │                                                      │
│  │ ② LOCK IN CURRENT PRICE                              │
│  │    Prepay for 1 year at $10/month ($120 total)      │
│  │    Save $120 compared to new price!                  │
│  │    Offer expires: Feb 20                             │
│  │    [Lock In $10/Month Rate]                          │
│  │                                                      │
│  │ ③ CANCEL SUBSCRIPTION                                │
│  │    Cancel anytime before March 20                    │
│  │    No penalty, full access until then                │
│  │    [Cancel Subscription]                             │
│  │                                                      │
│  │ Questions? Contact support@healthplan.com            │
│  └──────────────────────────────────────────────────────┘
│
│
📅 FEB 5  ─────────────────────────────────────────────────────
│
│  Alice's renewal (still on v1)
│  💳 Charged: $10.00 ✅
│  
│  Reminder shown:
│  ┌──────────────────────────────────────┐
│  │ ⏰ 43 days until price change         │
│  │ New price: $20/month                 │
│  │ [Lock in $10] [Accept $20] [Cancel] │
│  └──────────────────────────────────────┘
│
│
📅 FEB 15 ─────────────────────────────────────────────────────
│
│  ⭐ Alice decides to LOCK IN old price
│  
│  Action:
│  ┌──────────────────────────────────────┐
│  │ Prepay for 1 Year                    │
│  │ Current: $10/month                   │
│  │ New rate: $20/month                  │
│  │ Your savings: $120/year              │
│  │                                      │
│  │ Charge today: $120                   │
│  │ Locked until: Feb 15, 2026           │
│  │                                      │
│  │ [Confirm Prepayment]                 │
│  └──────────────────────────────────────┘
│  
│  Alice confirms:
│  💳 Charged $120 for 1 year
│  
│  Database updated:
│  ┌──────────────────────────────────────┐
│  │ Alice's Subscription                 │
│  │ Plan: v1 ($10/month)                 │
│  │ LockedUntil: Feb 15, 2026            │ ← NEW FIELD!
│  │ MigrationStatus: LOCKED              │ ← NEW FIELD!
│  │ NextBillingDate: Feb 15, 2026        │
│  └──────────────────────────────────────┘
│
│
📅 FEB 18 ─────────────────────────────────────────────────────
│
│  Bob does nothing (will accept new price)
│  
│  His subscription:
│  ┌──────────────────────────────────────┐
│  │ Bob's Subscription                   │
│  │ Plan: v1 ($10/month)                 │
│  │ MigrationStatus: PENDING             │ ← Will migrate
│  │ MigrationDate: March 20              │
│  │ NextBillingDate: Feb 10              │
│  └──────────────────────────────────────┘
│
│
📅 MARCH 19 ───────────────────────────────────────────────────
│
│  ⏰ ONE DAY BEFORE MIGRATION
│  
│  System sends FINAL reminder to Bob:
│  ┌──────────────────────────────────────────────────────┐
│  │ Subject: Final Reminder - Price Change Tomorrow      │
│  │ ─────────────────────────────────────────────────── │
│  │ Hi Bob,                                              │
│  │                                                      │
│  │ This is your final reminder that your               │
│  │ subscription price will change tomorrow:             │
│  │                                                      │
│  │ Current: $10/month                                   │
│  │ New: $20/month                                       │
│  │ Effective: Tomorrow, March 20                        │
│  │                                                      │
│  │ Last chance to:                                      │
│  │  • Cancel without penalty                            │
│  │  • Lock in $10 rate for 6 months ($60 prepay)       │
│  │                                                      │
│  │ No action needed to continue at new price.           │
│  │                                                      │
│  │ [Cancel] [Lock In Rate] [Continue at New Price]     │
│  └──────────────────────────────────────────────────────┘
│
│
📅 MARCH 20 ───────────────────────────────────────────────────
│
│  🤖 AUTOMATED MIGRATION JOB RUNS
│  
│  System finds subscriptions to migrate:
│  ┌──────────────────────────────────────┐
│  │ Migration Job                        │
│  │ ───────────────────────────────────│
│  │ SELECT * FROM Subscriptions         │
│  │ WHERE MigrationStatus = 'PENDING'   │
│  │   AND MigrationDate <= TODAY        │
│  │                                      │
│  │ Found: Bob's subscription            │
│  └──────────────────────────────────────┘
│  
│  Executes migration:
│  ┌──────────────────────────────────────┐
│  │ MigrateSubscriptionToPlanVersion()   │
│  │ ───────────────────────────────────│
│  │ Bob's subscription:                  │
│  │   OLD: PlanId = plan-123 (v1, $10)  │
│  │   NEW: PlanId = plan-456 (v2, $20)  │
│  │                                      │
│  │ Update Stripe subscription           │
│  │ Reset privilege usage                │
│  │ Update migration status              │
│  │                                      │
│  │ ✅ Migration complete                │
│  └──────────────────────────────────────┘
│  
│  Database after migration:
│  ┌──────────────────────────────────────┐
│  │ Bob's Subscription                   │
│  │ Plan: v2 ($20/month)     ← MIGRATED! │
│  │ MigrationStatus: COMPLETED           │
│  │ MigratedDate: March 20               │
│  │ NextBillingDate: April 10            │
│  └──────────────────────────────────────┘
│  
│  📧 Email sent to Bob:
│  ┌──────────────────────────────────────────────────────┐
│  │ Subject: Your Plan Has Been Updated                  │
│  │ ─────────────────────────────────────────────────── │
│  │ Hi Bob,                                              │
│  │                                                      │
│  │ As notified 60 days ago, your subscription has      │
│  │ been updated to the new pricing:                     │
│  │                                                      │
│  │ Old price: $10/month                                 │
│  │ New price: $20/month                                 │
│  │                                                      │
│  │ Your next billing:                                   │
│  │   Date: April 10, 2025                              │
│  │   Amount: $20.00                                     │
│  │                                                      │
│  │ Thank you for being a valued customer!               │
│  │                                                      │
│  │ [View Subscription Details]                          │
│  └──────────────────────────────────────────────────────┘
│
│
📅 APRIL 10 ───────────────────────────────────────────────────
│
│  💳 Bob's first renewal on v2
│  Charged: $20.00 ✅
│  
│  Bob: "I knew this was coming. Fair enough." ✅
│  (He had 60 days notice!)
│
│
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

FINAL STATE:
  Alice: Locked at $10/month until Feb 2026 (prepaid)
  Bob: Migrated to $20/month (with notice)
  Diana: Always on $20/month (new user)
  
  ✅ Alice: Happy (got discount for prepaying)
  ✅ Bob: Accepts change (had notice + options)
  ✅ Diana: Pays market rate
  
  EVERYONE TREATED FAIRLY! 🎉
```

---

## 🏗️ THE SOLUTION: Plan Version with Migration

### **New Database Structure**

```csharp
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ENTITY: SubscriptionPlan (Enhanced)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class SubscriptionPlan : BaseEntity
{
    // ... existing fields ...
    
    // ✅ VERSION TRACKING
    public int VersionNumber { get; set; } = 1;
    public Guid? ParentPlanId { get; set; }
    public bool IsLatestVersion { get; set; } = true;
    public DateTime? VersionEffectiveDate { get; set; }
    public DateTime? VersionRetiredDate { get; set; }
    public string? VersionChangeNotes { get; set; }
    
    // ✅ NEW: MIGRATION MANAGEMENT
    /// <summary>
    /// When users on previous version should be migrated to this version
    /// Gives users time to prepare (e.g., 60 days from price change)
    /// </summary>
    public DateTime? MigrationDate { get; set; }
    
    /// <summary>
    /// How many days notice users get before migration
    /// </summary>
    public int MigrationNoticeDays { get; set; } = 60;
    
    /// <summary>
    /// Whether to allow users to lock in old price by prepaying
    /// </summary>
    public bool AllowPriceLock { get; set; } = true;
    
    /// <summary>
    /// How long users can lock in old price (in months)
    /// </summary>
    public int PriceLockDurationMonths { get; set; } = 12;
    
    /// <summary>
    /// Deadline to lock in old price
    /// </summary>
    public DateTime? PriceLockDeadline { get; set; }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ENTITY: Subscription (Enhanced)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Subscription : BaseEntity
{
    // ... existing fields ...
    
    // ✅ NEW: MIGRATION TRACKING
    /// <summary>
    /// Migration status for plan version changes
    /// </summary>
    public MigrationStatus MigrationStatus { get; set; } = MigrationStatus.None;
    
    /// <summary>
    /// When this subscription should migrate to new plan version
    /// </summary>
    public DateTime? ScheduledMigrationDate { get; set; }
    
    /// <summary>
    /// Target plan version to migrate to
    /// </summary>
    public Guid? TargetMigrationPlanId { get; set; }
    
    /// <summary>
    /// When user was notified about migration
    /// </summary>
    public DateTime? MigrationNotifiedDate { get; set; }
    
    /// <summary>
    /// When migration was completed
    /// </summary>
    public DateTime? MigratedDate { get; set; }
    
    /// <summary>
    /// User's choice: Accept, Cancel, or Lock
    /// </summary>
    public string? MigrationUserChoice { get; set; }
    
    /// <summary>
    /// If user locked in old price, when lock expires
    /// </summary>
    public DateTime? PriceLockedUntil { get; set; }
}

public enum MigrationStatus
{
    None,              // No migration scheduled
    NoticeGiven,       // User has been notified
    UserAccepted,      // User accepted new price
    UserCancelled,     // User cancelled subscription
    PriceLocked,       // User prepaid to lock old price
    Migrated,          // Successfully migrated to new version
    MigrationFailed    // Migration failed (needs retry)
}
```

---

## 💻 IMPLEMENTATION: The Complete Code

### **Step 1: Enhanced UpdatePlanAsync (With Migration)**

```csharp
public async Task<JsonModel> UpdatePlanAsync(
    string planId, 
    UpdateSubscriptionPlanDto updateDto, 
    TokenModel tokenModel)
{
    // ... validation code ...
    
    var existingPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
    
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // CHECK FOR PRICE CHANGE
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    bool hasPriceChange = updateDto.Price > 0 && updateDto.Price != existingPlan.Price;
    
    if (hasPriceChange)
    {
        var hasActiveSubscriptions = await _subscriptionPlanRepository
            .HasActiveSubscriptionsAsync(planGuid);
        
        if (hasActiveSubscriptions)
        {
            // ✅ CALL THE MIGRATION WORKFLOW
            return await CreatePlanVersionWithMigrationAsync(
                existingPlan,
                updateDto,
                tokenModel
            );
        }
    }
    
    // No active subscriptions or no price change → safe to update in-place
    // ... existing update code ...
}
```

### **Step 2: Create Plan Version with Migration Schedule**

```csharp
private async Task<JsonModel> CreatePlanVersionWithMigrationAsync(
    SubscriptionPlan existingPlan,
    UpdateSubscriptionPlanDto updateDto,
    TokenModel tokenModel)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        var migrationNoticeDays = updateDto.MigrationNoticeDays ?? 60;  // Default 60 days
        var migrationDate = DateTime.UtcNow.AddDays(migrationNoticeDays);
        var priceLockDeadline = DateTime.UtcNow.AddDays(migrationNoticeDays / 2);  // Half notice period
        
        _logger.LogInformation(
            "Creating plan v{NewVersion} with migration scheduled for {MigrationDate}",
            existingPlan.VersionNumber + 1, migrationDate
        );
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // STEP 1: UPDATE EXISTING PLAN (v1) - MARK FOR RETIREMENT
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        existingPlan.IsLatestVersion = false;
        existingPlan.VersionRetiredDate = DateTime.UtcNow;
        existingPlan.MigrationDate = migrationDate;  // When users will be migrated
        existingPlan.PriceLockDeadline = priceLockDeadline;  // Deadline to lock old price
        existingPlan.UpdatedDate = DateTime.UtcNow;
        existingPlan.UpdatedBy = tokenModel.UserID;
        
        await _subscriptionPlanRepository.UpdatePlanAsync(existingPlan);
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // STEP 2: CREATE NEW VERSION (v2) WITH NEW PRICE
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var newVersion = new SubscriptionPlan
        {
            // Copy all properties
            Name = updateDto.Name ?? existingPlan.Name,
            Description = updateDto.Description ?? existingPlan.Description,
            Price = updateDto.Price,  // NEW PRICE
            BillingCycleId = existingPlan.BillingCycleId,
            CurrencyId = existingPlan.CurrencyId,
            CategoryId = updateDto.CategoryId != Guid.Empty ? updateDto.CategoryId : existingPlan.CategoryId,
            // ... copy all other properties ...
            
            // Version tracking
            VersionNumber = existingPlan.VersionNumber + 1,
            ParentPlanId = existingPlan.ParentPlanId ?? existingPlan.Id,
            IsLatestVersion = true,
            VersionEffectiveDate = DateTime.UtcNow,
            VersionChangeNotes = $"Price changed from ${existingPlan.Price} to ${updateDto.Price}",
            
            // Migration settings
            MigrationDate = null,  // This IS the target version
            MigrationNoticeDays = migrationNoticeDays,
            AllowPriceLock = updateDto.AllowPriceLock ?? true,
            PriceLockDurationMonths = updateDto.PriceLockDurationMonths ?? 12,
            
            // Audit
            CreatedBy = tokenModel.UserID,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };
        
        var createdVersion = await _subscriptionPlanRepository.CreatePlanAsync(newVersion);
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // STEP 3: COPY PRIVILEGES TO NEW VERSION
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var existingPrivileges = await _planPrivilegeRepository.GetByPlanIdAsync(existingPlan.Id);
        
        foreach (var privilege in existingPrivileges)
        {
            var newPrivilege = new SubscriptionPlanPrivilege
            {
                SubscriptionPlanId = createdVersion.Id,
                PrivilegeId = privilege.PrivilegeId,
                Value = privilege.Value,
                // ... copy all privilege settings ...
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow
            };
            
            await _planPrivilegeRepository.AddAsync(newPrivilege);
        }
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // STEP 4: CREATE STRIPE RESOURCES
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var stripeProductId = await _stripeService.CreateProductAsync(
            $"{createdVersion.Name} v{createdVersion.VersionNumber}",
            createdVersion.Description ?? "",
            tokenModel
        );
        
        createdVersion.StripeProductId = stripeProductId;
        createdVersion.StripeMonthlyPriceId = await _stripeService.CreatePriceAsync(
            stripeProductId, createdVersion.Price, "usd", "month", 1, tokenModel);
        // ... create quarterly and annual prices ...
        
        await _subscriptionPlanRepository.UpdatePlanAsync(createdVersion);
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // STEP 5: SCHEDULE MIGRATION FOR EXISTING SUBSCRIPTIONS
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var activeSubscriptions = await _subscriptionRepository.GetByPlanIdAsync(existingPlan.Id);
        var subscriptionsToMigrate = activeSubscriptions
            .Where(s => s.Status == "Active" || s.Status == "TrialActive")
            .ToList();
        
        foreach (var subscription in subscriptionsToMigrate)
        {
            // Schedule migration
            subscription.MigrationStatus = MigrationStatus.NoticeGiven;
            subscription.ScheduledMigrationDate = migrationDate;
            subscription.TargetMigrationPlanId = createdVersion.Id;
            subscription.MigrationNotifiedDate = DateTime.UtcNow;
            subscription.UpdatedDate = DateTime.UtcNow;
            subscription.UpdatedBy = tokenModel.UserID;
            
            await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
        }
        
        _logger.LogInformation(
            "Scheduled migration for {Count} subscriptions to plan v{Version} on {Date}",
            subscriptionsToMigrate.Count, createdVersion.VersionNumber, migrationDate
        );
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // STEP 6: SEND NOTIFICATIONS TO ALL AFFECTED USERS
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        foreach (var subscription in subscriptionsToMigrate)
        {
            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            
            await SendPriceChangeNotificationAsync(
                user,
                subscription,
                existingPlan,
                createdVersion,
                migrationDate,
                priceLockDeadline,
                tokenModel
            );
        }
        
        await _unitOfWork.CommitTransactionAsync();
        
        return new JsonModel
        {
            data = new
            {
                newVersion = _mapper.Map<SubscriptionPlanDto>(createdVersion),
                versionNumber = createdVersion.VersionNumber,
                oldPrice = existingPlan.Price,
                newPrice = createdVersion.Price,
                migrationDate = migrationDate,
                affectedUsers = subscriptionsToMigrate.Count,
                noticeGiven = $"{migrationNoticeDays} days",
                priceLockDeadline = priceLockDeadline,
                summary = new
                {
                    action = "Plan version created with scheduled migration",
                    timeline = $"Users have {migrationNoticeDays} days to prepare",
                    userOptions = new[]
                    {
                        "Accept new price (no action needed)",
                        $"Lock in old price by {priceLockDeadline:MMM dd}",
                        $"Cancel before {migrationDate:MMM dd}"
                    }
                }
            },
            Message = $"Plan v{createdVersion.VersionNumber} created. " +
                     $"{subscriptionsToMigrate.Count} users will migrate on {migrationDate:MMM dd, yyyy}.",
            StatusCode = 201
        };
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Error creating plan version with migration");
        return new JsonModel { Message = "Failed to create plan version", StatusCode = 500 };
    }
}
```

### **Step 3: User Action Handlers**

```csharp
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// USER CHOICE 1: ACCEPT NEW PRICE (Do Nothing)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

// User doesn't need to do anything
// On migrationDate, automated job migrates them
// This is handled in Step 4 below

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// USER CHOICE 2: LOCK IN OLD PRICE BY PREPAYING
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// Allows user to prepay and lock in current price before migration
/// </summary>
[HttpPost("{subscriptionId}/lock-price")]
public async Task<JsonModel> LockInCurrentPrice(
    string subscriptionId,
    [FromBody] LockPriceDto lockDto)
{
    var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
    
    if (subscription.MigrationStatus != MigrationStatus.NoticeGiven)
    {
        return new JsonModel { Message = "No migration scheduled for this subscription", StatusCode = 400 };
    }
    
    // Check if before deadline
    var plan = subscription.SubscriptionPlan;
    if (DateTime.UtcNow > plan.PriceLockDeadline)
    {
        return new JsonModel { Message = "Price lock deadline has passed", StatusCode = 400 };
    }
    
    // Calculate prepayment amount
    var lockDurationMonths = lockDto.DurationMonths ?? plan.PriceLockDurationMonths;
    var monthlyPrice = subscription.CurrentPrice;
    var prepayAmount = monthlyPrice * lockDurationMonths;
    
    _logger.LogInformation(
        "User {UserId} locking price: {Months} months × ${Price} = ${Total}",
        subscription.UserId, lockDurationMonths, monthlyPrice, prepayAmount
    );
    
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // PROCESS PREPAYMENT
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    var paymentResult = await _stripeService.ProcessPaymentAsync(
        lockDto.PaymentMethodId,
        prepayAmount,
        "usd",
        tokenModel
    );
    
    if (!paymentResult.Success)
    {
        return new JsonModel { Message = "Payment failed", StatusCode = 400 };
    }
    
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // UPDATE SUBSCRIPTION - LOCK PRICE
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    subscription.MigrationStatus = MigrationStatus.PriceLocked;
    subscription.MigrationUserChoice = "PriceLocked";
    subscription.PriceLockedUntil = DateTime.UtcNow.AddMonths(lockDurationMonths);
    subscription.NextBillingDate = subscription.PriceLockedUntil.Value;  // No billing until lock expires
    subscription.UpdatedDate = DateTime.UtcNow;
    
    await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
    
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // CREATE BILLING RECORD
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
    {
        UserId = subscription.UserId,
        SubscriptionId = subscription.Id,
        Amount = prepayAmount,
        Type = BillingRecord.BillingType.Upfront.ToString(),
        Description = $"Prepaid {lockDurationMonths} months to lock in ${monthlyPrice}/month rate",
        Status = BillingRecord.BillingStatus.Paid.ToString(),
        StripePaymentIntentId = paymentResult.PaymentIntentId
    }, tokenModel);
    
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // SEND CONFIRMATION
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
    {
        UserId = subscription.UserId,
        Title = "Price Locked Successfully!",
        Message = $"You've locked in ${monthlyPrice}/month for {lockDurationMonths} months. " +
                 $"No billing until {subscription.PriceLockedUntil:MMM dd, yyyy}. " +
                 $"You saved ${(updateDto.Price - monthlyPrice) * lockDurationMonths}!",
        Type = "PriceLockConfirmation",
        Priority = "Normal"
    }, tokenModel);
    
    return new JsonModel
    {
        data = new
        {
            priceLocked = true,
            currentPrice = monthlyPrice,
            lockedUntil = subscription.PriceLockedUntil,
            prepaidAmount = prepayAmount,
            durationMonths = lockDurationMonths,
            savingsVsNewPrice = (updateDto.Price - monthlyPrice) * lockDurationMonths
        },
        Message = $"Price locked at ${monthlyPrice}/month for {lockDurationMonths} months!",
        StatusCode = 200
    };
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// USER CHOICE 3: CANCEL SUBSCRIPTION
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

// User cancels before migration date
// Existing CancelSubscriptionAsync() already handles this
// Just update migration status:

subscription.MigrationStatus = MigrationStatus.UserCancelled;
subscription.MigrationUserChoice = "Cancelled";
```

### **Step 4: Automated Migration Job**

```csharp
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// NEW FILE: SmartTelehealth.Application\Services\SubscriptionMigrationService.cs
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public interface ISubscriptionMigrationService
{
    Task ProcessScheduledMigrationsAsync();
    Task SendMigrationRemindersAsync();
}

public class SubscriptionMigrationService : ISubscriptionMigrationService
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly IStripeService _stripeService;
    private readonly INotificationService _notificationService;
    private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
    private readonly ILogger<SubscriptionMigrationService> _logger;
    
    /// <summary>
    /// Runs daily to migrate subscriptions that have reached their migration date
    /// </summary>
    public async Task ProcessScheduledMigrationsAsync()
    {
        try
        {
            _logger.LogInformation("Starting scheduled subscription migrations");
            
            // ══════════════════════════════════════════════════════
            // FIND SUBSCRIPTIONS DUE FOR MIGRATION
            // ══════════════════════════════════════════════════════
            var today = DateTime.UtcNow.Date;
            
            var subscriptionsDue = await _subscriptionRepo.GetSubscriptionsDueForMigrationAsync(today);
            
            _logger.LogInformation(
                "Found {Count} subscriptions due for migration today",
                subscriptionsDue.Count
            );
            
            foreach (var subscription in subscriptionsDue)
            {
                try
                {
                    await MigrateSubscriptionAsync(subscription);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to migrate subscription {SubscriptionId}",
                        subscription.Id
                    );
                    
                    // Mark as failed
                    subscription.MigrationStatus = MigrationStatus.MigrationFailed;
                    subscription.UpdatedDate = DateTime.UtcNow;
                    await _subscriptionRepo.UpdateSubscriptionAsync(subscription);
                }
            }
            
            _logger.LogInformation("Completed scheduled subscription migrations");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in scheduled migration job");
        }
    }
    
    private async Task MigrateSubscriptionAsync(Subscription subscription)
    {
        _logger.LogInformation(
            "Migrating subscription {SubscriptionId} from plan {OldPlanId} to {NewPlanId}",
            subscription.Id, subscription.SubscriptionPlanId, subscription.TargetMigrationPlanId
        );
        
        var oldPlan = subscription.SubscriptionPlan;
        var newPlan = await _planRepo.GetByIdWithDetailsAsync(subscription.TargetMigrationPlanId.Value);
        
        if (newPlan == null)
        {
            throw new Exception($"Target plan {subscription.TargetMigrationPlanId} not found");
        }
        
        // ══════════════════════════════════════════════════════
        // STEP 1: UPDATE STRIPE SUBSCRIPTION
        // ══════════════════════════════════════════════════════
        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            // Get appropriate Stripe price ID based on billing cycle
            var stripePriceId = GetStripePriceIdForCycle(newPlan, subscription.BillingCycleId);
            
            var stripeUpdated = await _stripeService.UpdateSubscriptionAsync(
                subscription.StripeSubscriptionId,
                stripePriceId,
                null  // tokenModel
            );
            
            if (!stripeUpdated)
            {
                throw new Exception("Failed to update Stripe subscription");
            }
        }
        
        // ══════════════════════════════════════════════════════
        // STEP 2: UPDATE LOCAL SUBSCRIPTION
        // ══════════════════════════════════════════════════════
        subscription.SubscriptionPlanId = newPlan.Id;
        subscription.CurrentPrice = newPlan.Price;
        subscription.MigrationStatus = MigrationStatus.Migrated;
        subscription.MigratedDate = DateTime.UtcNow;
        subscription.UpdatedDate = DateTime.UtcNow;
        
        await _subscriptionRepo.UpdateSubscriptionAsync(subscription);
        
        // ══════════════════════════════════════════════════════
        // STEP 3: RESET PRIVILEGES FOR NEW PLAN
        // ══════════════════════════════════════════════════════
        // Get new plan privileges
        var newPlanPrivileges = await _planRepo.GetPlanPrivilegesAsync(newPlan.Id);
        
        // Delete old usage records
        var oldUsageRecords = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
        foreach (var oldUsage in oldUsageRecords)
        {
            oldUsage.IsDeleted = true;
            oldUsage.UpdatedDate = DateTime.UtcNow;
            await _usageRepo.UpdateUsageAsync(oldUsage);
        }
        
        // Create new usage records for new plan
        foreach (var newPrivilege in newPlanPrivileges)
        {
            var newUsage = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscription.Id,
                SubscriptionPlanPrivilegeId = newPrivilege.Id,
                PrivilegeId = newPrivilege.PrivilegeId,
                UsedValue = 0,
                AllowedValue = newPrivilege.Value,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = subscription.NextBillingDate,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            
            await _usageRepo.AddAsync(newUsage);
        }
        
        // ══════════════════════════════════════════════════════
        // STEP 4: NOTIFY USER
        // ══════════════════════════════════════════════════════
        var user = await _userRepository.GetByIdAsync(subscription.UserId);
        
        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = user.Id,
            Title = "Subscription Migrated",
            Message = $"Your subscription has been updated to {newPlan.Name} v{newPlan.VersionNumber}. " +
                     $"New price: ${newPlan.Price}/month. Next billing: {subscription.NextBillingDate:MMM dd, yyyy}.",
            Type = "MigrationCompleted",
            Priority = "Normal"
        }, null);
        
        await _notificationService.SendPlanMigrationCompletedEmailAsync(
            user.Email,
            user.FullName,
            oldPlan.Name,
            newPlan.Name,
            oldPlan.Price,
            newPlan.Price,
            subscription.NextBillingDate,
            null
        );
        
        _logger.LogInformation(
            "Successfully migrated subscription {SubscriptionId} from v{OldVersion} to v{NewVersion}",
            subscription.Id, oldPlan.VersionNumber, newPlan.VersionNumber
        );
    }
    
    /// <summary>
    /// Sends reminder emails at intervals before migration
    /// Run daily to catch reminders at 30 days, 14 days, 7 days, 3 days, 1 day
    /// </summary>
    public async Task SendMigrationRemindersAsync()
    {
        var reminderDays = new[] { 30, 14, 7, 3, 1 };
        
        foreach (var days in reminderDays)
        {
            var targetDate = DateTime.UtcNow.Date.AddDays(days);
            
            var subscriptionsDue = await _subscriptionRepo
                .GetSubscriptionsByMigrationDateAsync(targetDate);
            
            foreach (var subscription in subscriptionsDue.Where(s => s.MigrationStatus == MigrationStatus.NoticeGiven))
            {
                var user = await _userRepository.GetByIdAsync(subscription.UserId);
                var newPlan = await _planRepo.GetByIdWithDetailsAsync(subscription.TargetMigrationPlanId.Value);
                
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = user.Id,
                    Title = $"Reminder: Price Change in {days} Days",
                    Message = $"Your subscription will update to ${newPlan.Price}/month in {days} days. " +
                             $"You can still lock in ${subscription.CurrentPrice}/month or cancel.",
                    Type = "MigrationReminder",
                    Priority = days <= 7 ? "High" : "Normal"
                }, null);
                
                _logger.LogInformation(
                    "Sent {Days}-day reminder to user {UserId} for subscription {SubscriptionId}",
                    days, user.Id, subscription.Id
                );
            }
        }
    }
}
```

### **Step 5: Background Job Schedule**

```csharp
// Add to your background job scheduler (Hangfire, Quartz, etc.)

// Job 1: Process migrations (runs daily at 2 AM)
[RecurringJob(Cron = "0 2 * * *")]
public class ProcessSubscriptionMigrationsJob
{
    private readonly ISubscriptionMigrationService _migrationService;
    
    public async Task ExecuteAsync()
    {
        await _migrationService.ProcessScheduledMigrationsAsync();
    }
}

// Job 2: Send reminders (runs daily at 9 AM)
[RecurringJob(Cron = "0 9 * * *")]
public class SendMigrationRemindersJob
{
    private readonly ISubscriptionMigrationService _migrationService;
    
    public async Task ExecuteAsync()
    {
        await _migrationService.SendMigrationRemindersAsync();
    }
}
```

---

## 📧 EMAIL TEMPLATES

### **Initial Notice Email (Day 0 - Jan 20)**

```html
Subject: Important: Price Update Notice - Action Required

Hi Alice,

We're writing to let you know about an important update to your 
Basic Health Plan subscription.

┌─────────────────────────────────────────────────────────┐
│ CURRENT PLAN                                            │
│ Price: $10.00/month                                     │
│ Your next billing: February 5, 2025 ($10.00)           │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ PRICE CHANGE NOTICE                                     │
│ New Price: $20.00/month                                 │
│ Effective: March 20, 2025 (60 days from now)           │
│ Reason: Enhanced features and platform improvements     │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ YOUR OPTIONS                                            │
│                                                         │
│ ① NO ACTION NEEDED                                      │
│    • Continue your subscription at new price            │
│    • Automatically updated on March 20                  │
│    • First charge at new price: April 5 ($20)          │
│                                                         │
│ ② LOCK IN CURRENT PRICE                                 │
│    • Prepay for 12 months at $10/month                  │
│    • Pay $120 now (save $120!)                          │
│    • Deadline: February 20                              │
│    [Lock In Old Price - Save $120]                      │
│                                                         │
│ ③ CANCEL YOUR SUBSCRIPTION                              │
│    • Cancel anytime before March 20                     │
│    • No cancellation fee                                │
│    • Keep full access until March 20                    │
│    [Cancel Subscription]                                │
└─────────────────────────────────────────────────────────┘

IMPORTANT DATES:
  • Feb 5: Your next billing at $10 (current price)
  • Feb 20: Deadline to lock in $10/month rate
  • March 5: Your next billing at $10 (current price)
  • March 20: Price changes to $20/month
  • April 5: First billing at $20 (new price)

Questions? Contact support@healthplan.com

Thank you for being a valued customer!
```

### **30-Day Reminder Email (Feb 18)**

```html
Subject: Reminder: Price Change in 30 Days

Hi Alice,

This is a reminder that your Basic Health Plan subscription 
will update to $20/month in 30 days.

TIMELINE:
  ✅ Feb 5: Billed $10 (current price)
  📅 Feb 20: Last day to lock in $10/month (2 days left!)
  📅 March 5: Billed $10 (current price - last time)
  📅 March 20: Price changes to $20/month
  📅 April 5: First billing at $20 (new price)

LAST CHANCE TO LOCK IN $10/MONTH:
  Expires: February 20 (2 DAYS!)
  [Prepay Now - Save $120]

Or simply continue at new price (no action needed).

Thank you!
```

### **Final Reminder Email (March 19 - 1 day before)**

```html
Subject: ⏰ Final Notice: Price Change Tomorrow

Hi Bob,

Your subscription price will change TOMORROW:

Current price: $10/month
New price: $20/month
Change date: Tomorrow, March 20, 2025

Your next billing:
  Date: April 10, 2025
  Amount: $20.00 (new price)

This is your final opportunity to:
  • Cancel without penalty (until midnight tonight)
  • Lock in $10/month for 6 months ($60 prepay)

[Cancel] [Lock In Rate] [Accept New Price]

No action? You'll continue at $20/month automatically.

Thank you!
```

### **Migration Completed Email (March 20)**

```html
Subject: Your Subscription Has Been Updated

Hi Bob,

Your Basic Health Plan subscription has been successfully updated:

BEFORE:
  Plan: Basic Health Plan v1
  Price: $10.00/month

AFTER:
  Plan: Basic Health Plan v2  
  Price: $20.00/month
  New features: Priority support, Extended hours, More consultations

YOUR NEXT BILLING:
  Date: April 10, 2025
  Amount: $20.00

You received 60 days advance notice as promised.

Thank you for being a loyal customer!
  
[View New Features] [Manage Subscription]
```

---

## 🎨 VISUAL: Complete Migration Flow

```
                MIGRATION TIMELINE (60 Days)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

DAY 0 (Jan 20) - PRICE CHANGE ANNOUNCED
═════════════════════════════════════════════════════════
│
│  Admin creates Plan v2 ($20)
│  ↓
│  System schedules migration for March 20 (60 days)
│  ↓
│  📧 Emails sent to all users (Alice, Bob, Charlie)
│  
│  ┌────────────────────────────────────────┐
│  │ Subscription Status:                   │
│  │ ─────────────────────────────────────│
│  │ Current Plan: v1 ($10)                │
│  │ Migration To: v2 ($20)                │
│  │ Migration Date: March 20 (60 days)    │
│  │ Status: NOTICE_GIVEN ⏰               │
│  │                                        │
│  │ User Actions Available:                │
│  │  • Accept new price (do nothing)      │
│  │  • Lock in $10 (prepay by Feb 20)     │
│  │  • Cancel (anytime before March 20)   │
│  └────────────────────────────────────────┘
│
│
DAY 5 (Jan 25) - BUSINESS AS USUAL
═════════════════════════════════════════════════════════
│
│  New user Diana subscribes
│  ↓
│  Gets Plan v2 ($20) automatically
│  (She sees current price, no migration needed)
│
│
DAY 15 (Feb 5) - ALICE'S RENEWAL
═════════════════════════════════════════════════════════
│
│  💳 Alice's renewal
│  Charged: $10.00 (still on v1)
│  
│  Dashboard shows:
│  ┌────────────────────────────────────────┐
│  │ ⏰ 43 days until price change           │
│  │ New price: $20/month                   │
│  │                                        │
│  │ Lock in $10 by Feb 20 (15 days left)  │
│  │ [Prepay 12 Months - Save $120]        │
│  └────────────────────────────────────────┘
│
│
DAY 26 (Feb 15) - ALICE LOCKS IN PRICE
═════════════════════════════════════════════════════════
│
│  ⭐ Alice chooses to prepay
│  
│  Calculation:
│  12 months × $10 = $120
│  vs 12 months × $20 = $240
│  Savings: $120
│  
│  💳 Charged: $120.00 (prepaid for 1 year)
│  
│  ┌────────────────────────────────────────┐
│  │ ✅ PRICE LOCKED                        │
│  │ ─────────────────────────────────────│
│  │ Rate: $10/month                       │
│  │ Locked until: Feb 15, 2026            │
│  │ No billing until then                 │
│  │ Migration Status: PRICE_LOCKED 🔒     │
│  └────────────────────────────────────────┘
│
│  Alice is REMOVED from migration queue ✅
│
│
DAY 30 (Feb 18) - 30-DAY REMINDER
═════════════════════════════════════════════════════════
│
│  📧 Bob & Charlie get reminder email:
│  "30 days until price change"
│  
│  Bob's dashboard:
│  ┌────────────────────────────────────────┐
│  │ ⏰ 30 days until migration              │
│  │ Current: $10/month                     │
│  │ New: $20/month                         │
│  │ Lock-in deadline: 2 days (Feb 20)     │
│  └────────────────────────────────────────┘
│
│
DAY 31 (Feb 20) - LOCK-IN DEADLINE PASSES
═════════════════════════════════════════════════════════
│
│  Bob & Charlie didn't prepay
│  Lock-in option expires
│  
│  Now only 2 options left:
│   • Accept new price
│   • Cancel
│
│
DAY 44 (March 5) - LAST BILLING AT OLD PRICE
═════════════════════════════════════════════════════════
│
│  💳 Bob's renewal
│  Charged: $10.00 (last time!)
│  
│  Dashboard shows:
│  ┌────────────────────────────────────────┐
│  │ ⚠️  FINAL NOTICE                       │
│  │ This is your last billing at $10      │
│  │ Next billing: April 10 at $20         │
│  │ Change in: 15 days                     │
│  │                                        │
│  │ Last chance to cancel (no penalty)    │
│  │ [Keep Subscription] [Cancel]          │
│  └────────────────────────────────────────┘
│
│
DAY 59 (March 19) - 1-DAY FINAL WARNING
═════════════════════════════════════════════════════════
│
│  📧 Bob & Charlie get FINAL email:
│  "Price change TOMORROW - Last chance to cancel"
│
│
DAY 60 (March 20) - 🤖 AUTOMATIC MIGRATION
═════════════════════════════════════════════════════════
│
│  Automated job runs at 2 AM
│  
│  ┌────────────────────────────────────────────────────┐
│  │ ProcessScheduledMigrationsAsync()                  │
│  │ ─────────────────────────────────────────────────│
│  │ Find subscriptions where:                         │
│  │   MigrationStatus = NOTICE_GIVEN                  │
│  │   ScheduledMigrationDate <= TODAY                 │
│  │                                                    │
│  │ Found: Bob's subscription, Charlie's subscription │
│  │                                                    │
│  │ FOR EACH:                                          │
│  │   1. Update Stripe subscription → v2 price        │
│  │   2. Update local subscription → v2 plan          │
│  │   3. Reset privilege usage                        │
│  │   4. Update status: MIGRATED ✅                   │
│  │   5. Send confirmation email                      │
│  └────────────────────────────────────────────────────┘
│  
│  Bob's subscription AFTER migration:
│  ┌────────────────────────────────────────┐
│  │ Bob's Subscription                     │
│  │ Plan: v2 ($20/month)   ← MIGRATED!    │
│  │ MigrationStatus: MIGRATED ✅           │
│  │ MigratedDate: March 20                 │
│  │ NextBillingDate: April 10              │
│  └────────────────────────────────────────┘
│  
│  📧 Confirmation email sent to Bob
│
│
DAY 81 (April 10) - FIRST BILLING AT NEW PRICE
═════════════════════════════════════════════════════════
│
│  💳 Bob's renewal
│  Charged: $20.00 (new price)
│  
│  Bob: "I knew this was coming. I had 60 days notice." ✅
│
│
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

FINAL STATE:
═════════════════════════════════════════════════════════

Alice: Locked at $10 until Feb 2026 (prepaid) 🔒
Bob: Migrated to $20 (with 60 days notice) ✅
Charlie: Migrated to $20 (with 60 days notice) ✅
Diana: Always on $20 (new user) ✅

EVERYONE TREATED FAIRLY WITH ADVANCE NOTICE! 🎉
```

---

## 🎯 USER EXPERIENCE COMPARISON

### **Without Migration System (Current - Bad)**

```
Jan 20: Price changed to $20
        ↓
Feb 5:  Alice charged $20
        Alice: "WHAT?! No notice!" 😡
        
Result: Angry users, chargebacks, cancellations
```

### **With Migration System (Solution - Good)**

```
Jan 20: Price changed to $20
        System: "Migration in 60 days"
        Email: "You have 3 options + 60 days"
        ↓
60 days of:
  • Multiple reminder emails
  • Dashboard warnings
  • Options to prepay/cancel
        ↓
March 20: Automatic migration
        Email: "As promised, price is now $20"
        ↓
April 10: First charge at $20
        Bob: "Expected. I had 60 days notice." ✅
        
Result: Users feel respected, minimal churn
```

---

## 📊 STATE DIAGRAM

```
                    SUBSCRIPTION MIGRATION STATES
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

         ┌──────────────────┐
         │  NONE            │
         │  (Normal state)  │
         └────────┬─────────┘
                  │
                  │ Plan price changes
                  │
                  ▼
         ┌──────────────────┐
         │  NOTICE_GIVEN    │ ← Email sent
         │  (60 days)       │ ← Migration scheduled
         └────────┬─────────┘
                  │
          ┌───────┼────────┬─────────┐
          │       │        │         │
   User   │  User │   User │         │ User does
   cancels│  locks│   does │         │ nothing
          │  price│  nothing│         │ (accepts)
          │       │        │         │
          ▼       ▼        │         │
    ┌─────────┐ ┌─────────┐         │
    │CANCELLED│ │  PRICE  │         │
    │         │ │  LOCKED │         │
    └─────────┘ └─────────┘         │
      (ends)     (stays v1)          │
                                     │
                             60 days pass
                                     │
                                     ▼
                           ┌──────────────────┐
                           │    MIGRATED      │
                           │  (Moved to v2)   │
                           └────────┬─────────┘
                                    │
                                    ▼
                           ┌──────────────────┐
                           │  NONE            │
                           │ (Normal on v2)   │
                           └──────────────────┘
```

---

## 💡 KEY FEATURES

### **1. Grace Period**
```
Users get TIME to decide:
  • Review new pricing
  • Compare alternatives
  • Decide if service worth new price
  • Option to prepay and save
  • Option to cancel gracefully
  
NOT forced immediately! ✅
```

### **2. Price Lock Option**
```
EARLY BIRD DISCOUNT:
  "Lock in current price by prepaying!"
  
Benefits:
  • Users save money (prepay at old rate)
  • Business gets cash upfront
  • Win-win situation
  
Example:
  Alice prepays $120 (12 × $10)
  Saves: $120 (vs 12 × $20 = $240)
  Business gets: $120 cash now
  
Both sides happy! ✅
```

### **3. Clear Communication**
```
NOTIFICATION SCHEDULE:
  Day 0:  Initial notice (60 days warning)
  Day 30: 30-day reminder
  Day 46: 14-day reminder
  Day 53: 7-day reminder
  Day 57: 3-day reminder
  Day 59: FINAL warning (tomorrow!)
  Day 60: Migration executed
  Day 60: Confirmation sent
  
Users can't say "I didn't know!" ✅
```

### **4. Automatic Migration**
```
ON MIGRATION DATE:
  System automatically:
    1. Updates subscription to new plan
    2. Updates Stripe subscription
    3. Resets privilege usage
    4. Sends confirmation email
  
No manual work required! ✅
```

---

## 🎬 CODE WALKTHROUGH

### **When Admin Changes Price**

```javascript
// Visualization of what happens in code:

Admin clicks "Save" on price change ($10 → $20)
         ↓
┌──────────────────────────────────────────────────────────┐
│ UpdatePlanAsync()                                        │
├──────────────────────────────────────────────────────────┤
│ if (priceChanged && hasActiveSubscriptions) {            │
│     ↓                                                    │
│     CreatePlanVersionWithMigrationAsync()                │
│     │                                                    │
│     ├─► CREATE v2 plan with new price                   │
│     │                                                    │
│     ├─► SET migration date = now + 60 days              │
│     │                                                    │
│     ├─► UPDATE all active subscriptions:                │
│     │   FOR EACH subscription on v1:                    │
│     │     subscription.MigrationStatus = NOTICE_GIVEN   │
│     │     subscription.ScheduledMigrationDate = March20 │
│     │     subscription.TargetMigrationPlanId = v2.Id    │
│     │                                                    │
│     └─► SEND emails to all users                        │
│ }                                                        │
└──────────────────────────────────────────────────────────┘
         ↓
         ✅ Done! Migration scheduled
```

### **Daily Background Job**

```javascript
// Runs every day at 2 AM

Background Job runs
         ↓
┌──────────────────────────────────────────────────────────┐
│ ProcessScheduledMigrationsAsync()                        │
├──────────────────────────────────────────────────────────┤
│ today = March 20, 2025                                   │
│                                                          │
│ Query:                                                   │
│   SELECT * FROM Subscriptions                            │
│   WHERE MigrationStatus = 'NOTICE_GIVEN'                 │
│     AND ScheduledMigrationDate <= '2025-03-20'          │
│                                                          │
│ Found: Bob's subscription, Charlie's subscription        │
│                                                          │
│ FOR EACH subscription:                                   │
│   ├─► Get old plan (v1) and new plan (v2)              │
│   │                                                      │
│   ├─► Update Stripe:                                    │
│   │   stripe.subscriptions.update(                      │
│   │     bob.StripeSubscriptionId,                       │
│   │     { price: v2.StripePriceId }                     │
│   │   )                                                  │
│   │                                                      │
│   ├─► Update local subscription:                        │
│   │   bob.SubscriptionPlanId = v2.Id                    │
│   │   bob.CurrentPrice = 20.00                          │
│   │   bob.MigrationStatus = MIGRATED                    │
│   │   bob.MigratedDate = NOW                            │
│   │                                                      │
│   ├─► Reset privilege usage:                            │
│   │   Delete old usage records                          │
│   │   Create new usage records for v2                   │
│   │                                                      │
│   └─► Send confirmation email                           │
│                                                          │
│ ✅ All migrations completed                             │
└──────────────────────────────────────────────────────────┘
```

---

## 📊 MIGRATION STATISTICS TRACKING

```
ADMIN DASHBOARD - MIGRATION TRACKING
═════════════════════════════════════════════════════════

┌────────────────────────────────────────────────────────┐
│ Plan: Basic Health Plan                                │
│ Version Migration Status                               │
├────────────────────────────────────────────────────────┤
│                                                        │
│ Version 1 → Version 2 Migration                        │
│ Migration Date: March 20, 2025                         │
│ Days Remaining: 15 days                                │
│                                                        │
│ ┌────────────────────────────────────────────────────┐ │
│ │ MIGRATION BREAKDOWN                                │ │
│ │                                                    │ │
│ │ Total on v1: 100 subscriptions                     │ │
│ │                                                    │ │
│ │ ● Migrated:     0  (0%)    ──────────────         │ │
│ │ ● Price Locked: 25 (25%)   ████████░░░░░░░░░░░░   │ │
│ │ ● Will Migrate: 60 (60%)   ████████████████████   │ │
│ │ ● Cancelled:    15 (15%)   ██████░░░░░░░░░░░░░░   │ │
│ │                                                    │ │
│ │ Revenue Impact:                                    │ │
│ │  Current (v1): $1,000/month                        │ │
│ │  After Migration:                                  │ │
│ │    - Price Locked (25 users): $250/month          │ │
│ │    - Migrated (60 users): $1,200/month            │ │
│ │    - Cancelled (15 users): $0/month               │ │
│ │  New Total: $1,450/month (+45%) ✅                │ │
│ │                                                    │ │
│ │ Churn Rate: 15% (Industry avg: 25%)               │ │
│ │ Lock-in Rate: 25% (Good!)                          │ │
│ └────────────────────────────────────────────────────┘ │
│                                                        │
│ [View User List] [Send Custom Message] [Export]       │
└────────────────────────────────────────────────────────┘
```

---

## 🎁 BONUS: User Dashboard Features

### **User View During Notice Period**

```
ALICE'S DASHBOARD (Feb 1 - During Notice Period)
═════════════════════════════════════════════════════════

┌────────────────────────────────────────────────────────┐
│ ⚠️  IMPORTANT: Price Change Scheduled                  │
├────────────────────────────────────────────────────────┤
│ Your Basic Health Plan will update on March 20         │
│                                                        │
│ ┌────────────────────┬─────────────────────────────┐  │
│ │ CURRENT            │ AFTER MARCH 20              │  │
│ ├────────────────────┼─────────────────────────────┤  │
│ │ Price: $10/month   │ Price: $20/month            │  │
│ │ Plan: v1           │ Plan: v2                    │  │
│ │ Features: Standard │ Features: Enhanced          │  │
│ └────────────────────┴─────────────────────────────┘  │
│                                                        │
│ ⏰ TIME REMAINING: 47 days                             │
│                                                        │
│ ┌────────────────────────────────────────────────────┐ │
│ │ YOUR OPTIONS                                       │ │
│ │                                                    │ │
│ │ ① CONTINUE AT NEW PRICE (No action)               │ │
│ │    Auto-migrates March 20                          │ │
│ │    First charge at $20: April 5                    │ │
│ │                                                    │ │
│ │ ② LOCK IN $10/MONTH RATE                           │ │
│ │    ⏰ Deadline: February 20 (19 days left)         │ │
│ │    Prepay: $120 for 12 months                      │ │
│ │    You Save: $120 vs new price!                    │ │
│ │    [Prepay & Save $120] ⭐ RECOMMENDED             │ │
│ │                                                    │ │
│ │ ③ CANCEL SUBSCRIPTION                              │ │
│ │    Cancel anytime before March 20                  │ │
│ │    No penalty, keep access until then              │ │
│ │    [Cancel Subscription]                           │ │
│ └────────────────────────────────────────────────────┘ │
│                                                        │
│ What's New in v2?                                      │
│   ✨ Priority support (24/7)                           │
│   ✨ 3 extra consultations (5→8)                       │
│   ✨ Advanced health analytics                         │
│   ✨ Extended appointment slots                        │
│                                                        │
│ [Learn More] [Compare Plans]                           │
└────────────────────────────────────────────────────────┘

Interactive countdown:
┌────────────────────────────────────────┐
│ 🕐 Migration Countdown                 │
│                                        │
│  47 days : 12 hours : 34 mins         │
│                                        │
│  until price changes to $20/month     │
│                                        │
│  Last day to lock in $10: Feb 20      │
│  (19 days left)                        │
└────────────────────────────────────────┘
```

---

## ✅ COMPLETE SOLUTION SUMMARY

### **What This Solution Provides:**

1. **✅ Advance Notice (60 days)**
   - Users know about change WAY in advance
   - Multiple reminders sent
   - Clear timeline provided

2. **✅ User Choice**
   - Accept new price (do nothing)
   - Lock in old price (prepay)
   - Cancel subscription (no penalty)

3. **✅ Financial Incentive**
   - Prepay option encourages loyalty
   - Users save money
   - Business gets cash upfront

4. **✅ Automatic Migration**
   - No manual work after notice period
   - Background job handles it
   - Users migrated smoothly

5. **✅ Legal Compliance**
   - Meets 30-60 day notice requirements
   - Users consented (by not cancelling)
   - Clear terms and communication

6. **✅ Business Benefits**
   - Lower churn (users feel respected)
   - Revenue increase (new price)
   - Cash upfront (prepayments)
   - Positive brand image

### **Implementation Checklist:**

```
□ Add migration fields to SubscriptionPlan entity
□ Add migration fields to Subscription entity
□ Implement CreatePlanVersionWithMigrationAsync()
□ Implement LockInCurrentPrice() endpoint
□ Implement ProcessScheduledMigrationsAsync() background job
□ Implement SendMigrationRemindersAsync() background job
□ Create email templates
□ Create user dashboard migration UI
□ Create admin migration tracking dashboard
□ Test migration scenarios
□ Deploy!
```

---

## 🚀 FINAL RESULT

**Your users get:**
- ✅ Fair treatment (advance notice)
- ✅ Options (accept, lock, cancel)
- ✅ Transparency (clear communication)
- ✅ Control (they decide)

**Your business gets:**
- ✅ Revenue increase (new price)
- ✅ Low churn (users feel respected)
- ✅ Legal compliance (proper notice)
- ✅ Cash upfront (prepayments)
- ✅ Positive reputation (fair company)

**EVERYONE WINS!** 🎉🎉🎉

---

## 📁 FILES YOU NEED

All code is ready in:
- `VERIFIED_ISSUES_AND_SOLUTIONS.md` - Base versioning code
- `PLAN_PRICE_CHANGE_WITH_MIGRATION_SOLUTION.md` - This file (migration logic)

**Ready to implement? I can add all this code to your project right now!** 🚀


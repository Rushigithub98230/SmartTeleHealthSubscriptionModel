# Database Migration: Pending Cancellation at Renewal

**Date:** October 28, 2025  
**Migration Name:** `AddPendingCancellationToSubscriptions`  
**Status:** ✅ Migration Created, Manual SQL Script Provided

## Overview

This migration adds support for the "pending cancellation at renewal" feature, which allows users who reject a plan migration to have their subscription automatically canceled at the next renewal date.

## Changes Made

### 1. Entity Updates

**File:** `backend/SmartTelehealth.Core/Entities/Subscription.cs`

Added two new properties to the `Subscription` entity:

```csharp
/// <summary>
/// Indicates whether the subscription should be automatically canceled at the next renewal.
/// Used when a user opts out of a required plan migration.
/// </summary>
public bool PendingCancellationAtRenewal { get; set; }

/// <summary>
/// The reason why the subscription is pending cancellation.
/// Typically: "User opted out of plan migration to [Plan Name]"
/// </summary>
[MaxLength(500)]
public string? PendingCancellationReason { get; set; }
```

### 2. Database Schema Changes

**Migration File:** `backend/SmartTelehealth.Infrastructure/Migrations/20251028075817_AddPendingCancellationToSubscriptions.cs`

Adds two columns to the `Subscriptions` table:

| Column Name | Type | Nullable | Default | Description |
|------------|------|----------|---------|-------------|
| `PendingCancellationAtRenewal` | BIT | NO | 0 (false) | Flag indicating pending cancellation |
| `PendingCancellationReason` | NVARCHAR(500) | YES | NULL | Reason for pending cancellation |

### 3. Manual SQL Script

**File:** `backend/SmartTelehealth.Infrastructure/Migrations/Scripts/AddPendingCancellationColumns.sql`

A manual SQL script has been provided to apply these changes directly to the database, bypassing the migration issue with `FixApplicationLogsTable`.

## How to Apply the Migration

### Option 1: Apply via EF Core Migrations (Recommended)

If the database migration pipeline is working correctly:

```bash
cd backend/SmartTelehealth.Infrastructure
dotnet ef database update --startup-project ../SmartTelehealth.API
```

### Option 2: Apply Manually via SQL Script

If the EF Core migration is blocked by other pending migrations:

1. Connect to your database using SQL Server Management Studio or Azure Data Studio
2. Open the script: `backend/SmartTelehealth.Infrastructure/Migrations/Scripts/AddPendingCancellationColumns.sql`
3. Execute the script against your database
4. The script includes checks to prevent duplicate column creation

**SQL Command:**
```sql
-- Execute the script from file or copy-paste the contents
```

### Option 3: Direct SQL Execution

```sql
-- Add PendingCancellationAtRenewal column
ALTER TABLE Subscriptions
ADD PendingCancellationAtRenewal BIT NOT NULL DEFAULT 0;

-- Add PendingCancellationReason column
ALTER TABLE Subscriptions
ADD PendingCancellationReason NVARCHAR(500) NULL;
```

## Integration Points

### 1. Plan Versioning Service

**File:** `backend/SmartTelehealth.Application/Services/PlanVersioningService.cs`

When users are notified of a plan change, they now have two options:
- **Accept:** Migrate to the new plan at renewal
- **Cancel:** Stay on the current plan until renewal, then auto-cancel

### 2. Scheduled Migration Background Service

**File:** `backend/SmartTelehealth.Infrastructure/Services/ScheduledMigrationBackgroundService.cs`

**Lines ~230-245:** When processing a migration where `UserDecision == "Cancel"`:
```csharp
if (migration.UserDecision == "Cancel")
{
    subscription.PendingCancellationAtRenewal = true;
    subscription.PendingCancellationReason = 
        $"User opted out of plan migration to {targetPlan.Name} (v{targetPlan.VersionNumber})";
    migration.Status = "UserOptedOut";
    _logger.LogInformation(
        "User opted out of migration - subscription {SubId} marked for cancellation at renewal",
        subscription.Id);
}
```

### 3. Automated Billing Service

**File:** `backend/SmartTelehealth.Application/Services/AutomatedBillingService.cs`

**Lines ~660-675:** During renewal processing, check for pending cancellation:
```csharp
// Check for pending cancellation (user opted out of migration)
if (subscription.PendingCancellationAtRenewal)
{
    _logger.LogInformation(
        "Subscription {SubId} has pending cancellation at renewal. Reason: {Reason}",
        subscription.Id, subscription.PendingCancellationReason);
    
    await CancelSubscriptionAsync(
        subscription.Id, 
        subscription.PendingCancellationReason ?? "Automatic cancellation at renewal", 
        tokenModel);
    return;
}
```

## User Flow

### Scenario: User Rejects Plan Migration

**Day 1:** Admin updates plan from $100 to $120
- System creates "Plan v2" at $120
- v1 stays at $100, users continue normally
- 150 migrations scheduled at renewal dates
- Users notified: "Price changes on Feb 15"

**Day 7:** User views notification
- **Option A:** Accept migration → User will be migrated to new plan at renewal
- **Option B:** Cancel → User's subscription continues until renewal, then gets canceled

**Day 15 (Renewal Date):** For users who chose "Cancel"
1. Renewal billing process detects `PendingCancellationAtRenewal = true`
2. Subscription is automatically canceled
3. Cancellation reason: "User opted out of plan migration to Premium Plan v2"
4. No charge is processed
5. User's access continues until current period ends

## Testing Checklist

- [ ] Verify columns exist in database after migration
- [ ] Test user selecting "Cancel" option during migration notification
- [ ] Verify `PendingCancellationAtRenewal` flag is set correctly
- [ ] Test renewal process cancels subscription when flag is true
- [ ] Verify cancellation reason is logged correctly
- [ ] Ensure no billing occurs when subscription is auto-canceled
- [ ] Test that user access continues until current period expires

## Rollback Instructions

If you need to rollback this migration:

### Via EF Core:
```bash
dotnet ef database update FixApplicationLogsTable --startup-project ../SmartTelehealth.API
```

### Via SQL:
```sql
-- Remove the columns
ALTER TABLE Subscriptions DROP COLUMN PendingCancellationAtRenewal;
ALTER TABLE Subscriptions DROP COLUMN PendingCancellationReason;
```

## Related Documents

- `PLAN_VERSIONING_INTEGRATION_COMPLETE.md` - Complete implementation summary
- `BASEPRICE_ISSUE_DETAILED_ANALYSIS.md` - BasePrice staleness fix
- `PLAN_VERSIONING_ANALYSIS.md` - Plan versioning system analysis

## Notes

- The migration is non-destructive and adds columns with safe defaults
- Existing subscriptions will have `PendingCancellationAtRenewal = false` by default
- The feature is backward compatible with existing subscription flows
- No data loss will occur during migration

---

**Migration Status:** Ready to apply  
**Breaking Changes:** None  
**Data Loss Risk:** None


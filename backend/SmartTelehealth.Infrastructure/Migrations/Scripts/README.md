# Data Migration Scripts

## Purpose
These SQL scripts handle data migration for the healthcare-specific subscription plan management system.

## Scripts

### 1. `VersionExistingPlans.sql`

**When to Run:** After deploying the `AddHealthcarePlanVersioningAndPricing` EF Core migration

**What It Does:**
- Auto-versions all existing subscription plans as **version 1.0** (Choice 3a)
- Sets all existing plans to use **manual pricing mode** (preserves current prices)
- Sets default **price change notice period to 10 days**
- Configures plans to use **global commission default** (20%)

**How to Run:**

#### Option 1: SQL Server Management Studio (SSMS)
```sql
1. Open SSMS
2. Connect to your database
3. Open VersionExistingPlans.sql
4. Execute (F5)
```

#### Option 2: Command Line (sqlcmd)
```bash
sqlcmd -S YourServerName -d SmartTelehealthDb -i VersionExistingPlans.sql
```

#### Option 3: Visual Studio / Azure Data Studio
```sql
1. Open the SQL file
2. Connect to database
3. Execute script
```

**Expected Output:**
```
═══════════════════════════════════════════════════════════════════
Starting Auto-Versioning of Existing Subscription Plans...
═══════════════════════════════════════════════════════════════════
✅ Updated 15 existing plans to version 1.0
✅ Total plans at version 1: 15

Plan Versioning Summary:
─────────────────────────────────────────────────────────────────
[Table showing all plans with their version info]

═══════════════════════════════════════════════════════════════════
Auto-Versioning Completed Successfully!
═══════════════════════════════════════════════════════════════════

Next Steps:
1. All existing plans are now version 1.0 with manual pricing
2. When you update a plan, a new version will be created automatically
3. Users on old versions will be scheduled for migration at their renewal dates
4. Overage charges will use the latest plan version pricing (abuse prevention)

✅ Transaction committed successfully.
```

**Rollback:**
If something goes wrong, the transaction will automatically rollback. No manual intervention needed.

## Before Running

1. ✅ Ensure the EF Core migration `AddHealthcarePlanVersioningAndPricing` has been applied
2. ✅ Backup your database (recommended)
3. ✅ Test on a staging environment first

## After Running

1. ✅ Verify all plans have `VersionNumber = 1`
2. ✅ Verify all plans have `IsLatestVersion = true`
3. ✅ Check that existing subscriptions are still linked correctly
4. ✅ Test creating a new plan version to ensure the workflow works

## Support

If you encounter issues:
1. Check the error message in the output
2. Verify the migration was applied: `SELECT * FROM SystemSettings`
3. Check plan versioning fields: `SELECT Id, Name, VersionNumber, IsLatestVersion FROM SubscriptionPlans`


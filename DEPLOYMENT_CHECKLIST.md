# 🚀 Healthcare Subscription Plan System - Deployment Checklist

## Pre-Deployment

### 1. Code Review ✅
- [ ] All code changes reviewed
- [ ] No linting errors
- [ ] No compilation errors
- [ ] All dependencies resolved

### 2. Database Preparation
- [ ] Backup production database
- [ ] Test migration on staging database first
- [ ] Verify migration rollback plan

### 3. Configuration
- [ ] Update `appsettings.Production.json` with correct connection string
- [ ] Verify Stripe API keys are set
- [ ] Set up logging configuration
- [ ] Configure CORS policies

---

## Deployment Steps

### Step 1: Stop Application (if running)

```bash
# Stop IIS/Kestrel or Docker container
# Ensure no active database connections
```

### Step 2: Apply Database Migration

```bash
cd backend/SmartTelehealth.Infrastructure

# Apply EF Core migration
dotnet ef database update --context ApplicationDbContext --startup-project ../SmartTelehealth.API --configuration Release
```

**Verify:**
```sql
-- Check migration applied
SELECT * FROM __EFMigrationsHistory 
WHERE MigrationId LIKE '%AddHealthcarePlanVersioningAndPricing%';

-- Should return 1 row
```

### Step 3: Run Data Migration Script

**Execute:** `Migrations/Scripts/VersionExistingPlans.sql`

```sql
-- In SSMS or Azure Data Studio
-- Open and execute: VersionExistingPlans.sql
```

**Verify:**
```sql
-- All plans should be version 1
SELECT COUNT(*) FROM SubscriptionPlans WHERE VersionNumber = 1;

-- Should match total plan count
SELECT COUNT(*) FROM SubscriptionPlans;

-- Check system settings exist
SELECT * FROM SystemSettings;

-- Should return 1 row with:
-- DefaultAdminCommissionPercent = 20
-- DefaultPriceChangeNoticeDays = 10
```

### Step 4: Deploy Application

```bash
# Build in Release mode
dotnet build --configuration Release

# Publish
dotnet publish --configuration Release --output ./publish

# Deploy to server (IIS/Docker/Azure/AWS)
# Update web.config or docker-compose.yml
```

### Step 5: Start Application

```bash
# Start IIS/Kestrel or Docker container
# Verify application starts without errors
```

### Step 6: Verify Background Service

```bash
# Check logs for:
# "Scheduled Migration Background Service started"
# "Next run at 2025-XX-XX 02:00:00"
```

---

## Post-Deployment Verification

### Test 1: Verify Plan Creation

```bash
curl -X POST "https://your-api.com/api/SubscriptionPlans" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -d '{
  "name": "Deployment Test Plan",
  "billingCycleId": "MONTHLY_ID",
  "currencyId": "USD_ID",
  "categoryId": "TEST_CATEGORY_ID",
  "isAutoCalculatedPrice": true,
  "price": 100,
  "isActive": true,
  "privileges": [
    {
      "privilegeId": "TEST_PRIVILEGE_ID",
      "value": 5,
      "usagePeriodId": "MONTHLY_ID",
      "privilegeBaseCost": 10.00,
      "unitCost": 20.00
    }
  ]
}'
```

**Expected:**
- Status 201
- Plan created with v1
- Price auto-calculated

### Test 2: Verify Version Creation

```bash
curl -X POST "https://your-api.com/api/SubscriptionPlans/PLAN_ID/versions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -d '{
  "name": "Deployment Test Plan",
  "price": 120,
  "billingCycleId": "MONTHLY_ID",
  "currencyId": "USD_ID",
  "categoryId": "TEST_CATEGORY_ID",
  "isActive": true,
  "isAutoCalculatedPrice": false
}'
```

**Expected:**
- Status 201
- Version 2 created
- If subscriptions exist, migrations scheduled

### Test 3: Verify Pricing Breakdown

```bash
curl "https://your-api.com/api/SubscriptionPlans/PLAN_ID/pricing-breakdown"
```

**Expected:**
- Detailed breakdown showing privilege costs
- Commission calculated correctly
- Final price matches

### Test 4: Verify Background Service

**Wait until 2 AM or manually trigger:**

```sql
-- Check background service logs
SELECT TOP 10 * FROM AuditLogs 
WHERE TableName = 'ScheduledPlanMigrations' 
ORDER BY CreatedDate DESC;

-- Verify migrations processed
SELECT COUNT(*) FROM ScheduledPlanMigrations 
WHERE Status = 'Completed' 
AND CompletedDate >= CAST(GETDATE() AS DATE);
```

---

## Monitoring Setup

### 1. Application Logs

**Monitor for:**
- `"Scheduled Migration Background Service started"`
- `"Processing scheduled migrations for {Date}"`
- `"Completed migration {MigrationId}"`
- `"Failed migration {MigrationId}"` ⚠️

**Alert on:**
- Migration failures > 3 per day
- Background service not starting
- Stripe API errors

### 2. Database Monitoring

**Track:**
```sql
-- Daily migration stats
SELECT 
    CAST(CompletedDate AS DATE) AS MigrationDate,
    COUNT(*) AS TotalMigrations,
    SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS Successful,
    SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) AS Failed
FROM ScheduledPlanMigrations
WHERE CompletedDate >= DATEADD(day, -7, GETUTCDATE())
GROUP BY CAST(CompletedDate AS DATE)
ORDER BY CAST(CompletedDate AS DATE) DESC;
```

### 3. Business Metrics

**Track:**
- Number of plan versions created per month
- User responses to migrations (Accept/Downgrade/Cancel %)
- Revenue impact of price changes
- Overage billing trends

---

## Rollback Plan

### If Critical Issues Arise:

#### Option 1: Rollback Migration (Immediate)

```bash
cd backend/SmartTelehealth.Infrastructure

# Rollback to previous migration
dotnet ef database update PreviousMigrationName --context ApplicationDbContext --startup-project ../SmartTelehealth.API
```

#### Option 2: Keep Migration, Deactivate Features

```sql
-- Deactivate new plan versions
UPDATE SubscriptionPlans 
SET IsActive = 0 
WHERE VersionNumber > 1;

-- Mark old versions as latest again
UPDATE SubscriptionPlans 
SET IsLatestVersion = 1 
WHERE VersionNumber = 1;

-- Cancel pending migrations
UPDATE ScheduledPlanMigrations 
SET Status = 'Cancelled' 
WHERE Status = 'Pending';
```

#### Option 3: Stop Background Service

```csharp
// In Program.cs or DependencyInjection.cs
// Comment out:
// services.AddHostedService<ScheduledMigrationBackgroundService>();
```

---

## Health Checks

### API Health Check Endpoint

```bash
curl "https://your-api.com/health"
```

### Database Health Check

```sql
-- Verify critical tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('SubscriptionPlans', 'SystemSettings', 'ScheduledPlanMigrations');

-- Should return 3 rows
```

### Background Service Health

```sql
-- Check recent migration processing
SELECT TOP 1 
    CompletedDate,
    COUNT(*) AS ProcessedCount
FROM ScheduledPlanMigrations
WHERE Status IN ('Completed', 'Failed')
GROUP BY CAST(CompletedDate AS DATE)
ORDER BY CompletedDate DESC;

-- Should show activity within last 24 hours (if migrations exist)
```

---

## Performance Considerations

### Database Indexes

**Verify these indexes exist:**
```sql
-- Plan versioning indexes
CREATE INDEX IX_SubscriptionPlans_IsLatestVersion ON SubscriptionPlans(IsLatestVersion);
CREATE INDEX IX_SubscriptionPlans_ParentPlanId ON SubscriptionPlans(ParentPlanId);
CREATE INDEX IX_SubscriptionPlans_ParentPlanId_VersionNumber ON SubscriptionPlans(ParentPlanId, VersionNumber);

-- Migration indexes
CREATE INDEX IX_ScheduledPlanMigrations_Status ON ScheduledPlanMigrations(Status);
CREATE INDEX IX_ScheduledPlanMigrations_ScheduledMigrationDate ON ScheduledPlanMigrations(ScheduledMigrationDate);
CREATE INDEX IX_ScheduledPlanMigrations_Status_ScheduledMigrationDate ON ScheduledPlanMigrations(Status, ScheduledMigrationDate);
```

### Query Optimization

**Monitor slow queries:**
```sql
-- Find plans with many versions
SELECT 
    ISNULL(ParentPlanId, Id) AS PlanFamily,
    COUNT(*) AS VersionCount
FROM SubscriptionPlans
GROUP BY ISNULL(ParentPlanId, Id)
HAVING COUNT(*) > 5
ORDER BY COUNT(*) DESC;

-- Find large migration batches
SELECT 
    ToPlanId,
    COUNT(*) AS PendingMigrations
FROM ScheduledPlanMigrations
WHERE Status = 'Pending'
GROUP BY ToPlanId
HAVING COUNT(*) > 100
ORDER BY COUNT(*) DESC;
```

---

## Security Considerations

### 1. Authorization

**Verify:**
- ✅ Plan versioning requires Admin role
- ✅ Users can only view their own migrations
- ✅ Users can only respond to their own migrations
- ✅ Pricing breakdown is public (transparency)

### 2. Data Validation

**Verify:**
- ✅ Notice period: 7-365 days range
- ✅ Commission: 0-100% range
- ✅ Privilege base cost: >= 0
- ✅ Overage cost: >= 0

### 3. Rate Limiting

**Consider adding:**
```csharp
// Prevent abuse of version creation
[RateLimit(Requests = 10, Period = "1h")]
[HttpPost("{planId}/versions")]
```

---

## Monitoring Alerts

### Critical Alerts (Immediate)

1. **Migration Service Down**
   - No log entry "Scheduled Migration Background Service started"
   - No migrations processed in 48 hours

2. **Database Connection Failed**
   - Any database timeout errors
   - Failed transactions > 10 per hour

3. **Stripe Sync Failures**
   - Failed Stripe subscription updates during migration
   - Orphaned Stripe resources

### Warning Alerts (Next Business Day)

1. **High Migration Failure Rate**
   - Failed migrations > 5% of total

2. **User Cancellations**
   - Cancel rate > 10% for price changes

3. **Pricing Calculation Errors**
   - Auto-calculation failures > 3 per day

---

## Success Criteria

### Technical Success ✅

- [ ] All database migrations applied
- [ ] All existing plans versioned as v1.0
- [ ] SystemSettings row exists
- [ ] No compilation errors
- [ ] No runtime exceptions
- [ ] Background service running
- [ ] All API endpoints responding

### Business Success ✅

- [ ] Plan creation works (auto and manual pricing)
- [ ] Version creation preserves existing subscriptions
- [ ] Users migrate at individual renewal dates
- [ ] Overage uses latest plan pricing
- [ ] Users receive migration notifications
- [ ] Users can respond to migrations
- [ ] Migrations execute automatically
- [ ] Pricing is transparent

---

## Final Deployment Command Sequence

```bash
# 1. Backup database
# 2. Apply EF Core migration
cd backend/SmartTelehealth.Infrastructure
dotnet ef database update --context ApplicationDbContext --startup-project ../SmartTelehealth.API

# 3. Run SQL script (in SSMS)
# Execute: Migrations/Scripts/VersionExistingPlans.sql

# 4. Build and publish
cd ../..
dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish

# 5. Deploy to server
# Copy publish folder to server

# 6. Start application
# Start IIS/Docker/etc.

# 7. Verify health
curl https://your-api.com/api/SubscriptionPlans/active

# 8. Monitor logs
tail -f logs/application.log
```

---

## Post-Deployment Monitoring (First Week)

### Daily Checks:

**Day 1:**
- [ ] Verify background service ran at 2 AM
- [ ] Check for any migration failures
- [ ] Monitor application logs
- [ ] Verify new plan creations work

**Day 2-7:**
- [ ] Daily background service check
- [ ] Monitor user responses to migrations
- [ ] Track overage billing patterns
- [ ] Review pricing calculation accuracy

**Week 1 Report:**
- [ ] Total plans created
- [ ] Total versions created
- [ ] Total migrations scheduled
- [ ] Total migrations completed
- [ ] User response distribution (Accept/Downgrade/Cancel)
- [ ] Any errors or issues encountered

---

## 🎉 Deployment Complete!

Your healthcare subscription plan management system is now:

✅ **Production-ready**  
✅ **Healthcare-compliant**  
✅ **Abuse-resistant**  
✅ **User-friendly**  
✅ **Transparent**  
✅ **Fully automated**  
✅ **Scalable**  
✅ **Maintainable**  

**Congratulations! 🏥✨**


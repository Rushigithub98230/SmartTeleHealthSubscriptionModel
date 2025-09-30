# 🗄️ **DATABASE RESET GUIDE FOR SUBSCRIPTION PLANS**

## 🎯 **OVERVIEW**

This guide provides SQL scripts to completely reset the subscription plan related database tables. Use these scripts when you need to:

- **Clean up test data** from subscription plan tables
- **Reset the database structure** for subscription plans
- **Start fresh** with subscription plan development
- **Fix database schema issues** related to subscription plans

---

## 📋 **AVAILABLE SCRIPTS**

### **1. `DROP_SUBSCRIPTION_PLAN_TABLES.sql`**
- **Purpose**: Drops all subscription plan related tables
- **Use Case**: When you only want to remove tables without recreating them
- **Tables Dropped**:
  - `UserSubscriptionPrivilegeUsage`
  - `PrivilegeUsageHistory`
  - `SubscriptionPlanPrivilege`
  - `Subscription`
  - `SubscriptionPlan`
  - `ServiceConstraint`

### **2. `CREATE_SUBSCRIPTION_PLAN_TABLES.sql`**
- **Purpose**: Creates all subscription plan related tables with proper structure
- **Use Case**: When you want to create fresh tables (after dropping or for new setup)
- **Tables Created**:
  - `SubscriptionPlan` (main table)
  - `SubscriptionPlanPrivilege` (junction table)
  - `Subscription` (user subscriptions)
  - `UserSubscriptionPrivilegeUsage` (usage tracking)
  - `PrivilegeUsageHistory` (historical usage)
  - `ServiceConstraint` (service limitations)

### **3. `RESET_SUBSCRIPTION_PLAN_TABLES.sql`** ⭐ **RECOMMENDED**
- **Purpose**: Complete reset - drops and recreates all tables
- **Use Case**: **Most common scenario** - complete fresh start
- **What it does**:
  1. Drops all existing tables
  2. Creates new tables with proper structure
  3. Creates indexes for performance
  4. Verifies successful creation

---

## 🚀 **HOW TO USE**

### **Option 1: Complete Reset (Recommended)**
```sql
-- Run this single script for complete reset
-- File: RESET_SUBSCRIPTION_PLAN_TABLES.sql
```

**Steps:**
1. Open your database management tool (SSMS, Azure Data Studio, etc.)
2. Connect to your database
3. Open `RESET_SUBSCRIPTION_PLAN_TABLES.sql`
4. Execute the script
5. Verify the completion message

### **Option 2: Drop Only**
```sql
-- Run this if you only want to drop tables
-- File: DROP_SUBSCRIPTION_PLAN_TABLES.sql
```

### **Option 3: Create Only**
```sql
-- Run this if you only want to create tables
-- File: CREATE_SUBSCRIPTION_PLAN_TABLES.sql
```

---

## ⚠️ **IMPORTANT CONSIDERATIONS**

### **🔒 Data Loss Warning**
- **ALL DATA** in subscription plan tables will be **PERMANENTLY DELETED**
- This includes:
  - All subscription plans
  - All user subscriptions
  - All privilege configurations
  - All usage history
  - All service constraints

### **🔗 Foreign Key Dependencies**
The scripts handle foreign key constraints properly by:
- **Dropping in correct order** (child tables first)
- **Temporarily disabling** foreign key checks
- **Re-enabling** foreign key checks after operations

### **📊 Required Master Tables**
Before running the create scripts, ensure these tables exist:
- `Users` (for user references)
- `Privilege` (for privilege references)
- `MasterBillingCycles` (for billing cycle references)
- `MasterCurrencies` (for currency references)
- `Categories` (for category references)
- `Providers` (for provider references)

---

## 🎯 **TABLE STRUCTURE OVERVIEW**

### **📋 SubscriptionPlan Table**
- **Primary Key**: `Id` (UNIQUEIDENTIFIER)
- **Key Fields**: Name, Price, BillingCycleId, CurrencyId, CategoryId
- **Features**: Trial support, Stripe integration, marketing flags
- **Constraints**: Price > 0, valid dates, proper limits

### **🔗 SubscriptionPlanPrivilege Table**
- **Primary Key**: `Id` (UNIQUEIDENTIFIER)
- **Foreign Keys**: SubscriptionPlanId, PrivilegeId, UsagePeriodId
- **Features**: Usage limits, time-based restrictions, overage costs
- **Constraints**: Value >= -1, proper date ranges

### **👤 Subscription Table**
- **Primary Key**: `Id` (UNIQUEIDENTIFIER)
- **Foreign Keys**: UserId, SubscriptionPlanId, BillingCycleId, ProviderId
- **Features**: Status management, trial handling, Stripe integration
- **Constraints**: Valid dates, positive amounts

### **📊 UserSubscriptionPrivilegeUsage Table**
- **Primary Key**: `Id` (UNIQUEIDENTIFIER)
- **Foreign Keys**: SubscriptionId, SubscriptionPlanPrivilegeId, PrivilegeId, UserId
- **Features**: Usage tracking, time-based analytics
- **Constraints**: Valid time periods, positive usage counts

### **📈 PrivilegeUsageHistory Table**
- **Primary Key**: `Id` (UNIQUEIDENTIFIER)
- **Foreign Keys**: SubscriptionId, SubscriptionPlanPrivilegeId, PrivilegeId, UserId
- **Features**: Historical usage data, period tracking, overage calculations
- **Constraints**: Valid periods, proper usage counts

### **⚙️ ServiceConstraint Table**
- **Primary Key**: `Id` (UNIQUEIDENTIFIER)
- **Foreign Keys**: SubscriptionPlanId
- **Features**: Service limitations, constraint types, configuration
- **Constraints**: Valid constraint values, proper date ranges

---

## 🔍 **VERIFICATION QUERIES**

After running the scripts, use these queries to verify:

### **Check Table Creation**
```sql
SELECT 
    TABLE_NAME,
    'CREATED' as Status
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME IN (
        'SubscriptionPlan',
        'SubscriptionPlanPrivilege',
        'Subscription',
        'UserSubscriptionPrivilegeUsage',
        'PrivilegeUsageHistory',
        'ServiceConstraint'
    )
ORDER BY TABLE_NAME;
```

### **Check Foreign Key Constraints**
```sql
SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
WHERE tp.name IN ('SubscriptionPlan', 'SubscriptionPlanPrivilege', 'Subscription', 'UserSubscriptionPrivilegeUsage', 'PrivilegeUsageHistory', 'ServiceConstraint')
ORDER BY tp.name, fk.name;
```

### **Check Indexes**
```sql
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('SubscriptionPlan', 'SubscriptionPlanPrivilege', 'Subscription', 'UserSubscriptionPrivilegeUsage', 'PrivilegeUsageHistory', 'ServiceConstraint')
    AND i.name IS NOT NULL
ORDER BY t.name, i.name;
```

---

## 🚨 **TROUBLESHOOTING**

### **Error: Foreign Key Constraint Violation**
- **Cause**: Master tables don't exist or have different structure
- **Solution**: Ensure all required master tables exist before running create scripts

### **Error: Permission Denied**
- **Cause**: Insufficient database permissions
- **Solution**: Run with database owner or admin permissions

### **Error: Table Already Exists**
- **Cause**: Tables weren't properly dropped
- **Solution**: Use the drop script first, then create script

### **Error: Invalid Column Type**
- **Cause**: Database version doesn't support certain data types
- **Solution**: Check SQL Server version compatibility

---

## ✅ **SUCCESS INDICATORS**

After successful execution, you should see:

1. **Completion Message**: "SUBSCRIPTION PLAN TABLES RESET COMPLETE!"
2. **Verification Results**: All 6 tables listed as "CREATED"
3. **No Error Messages**: Script completes without errors
4. **Foreign Keys**: All relationships properly established
5. **Indexes**: Performance indexes created successfully

---

## 🎯 **NEXT STEPS**

After running the reset script:

1. **Test API Endpoints**: Use the test payloads to verify functionality
2. **Create Master Data**: Ensure currencies, billing cycles, categories, and privileges exist
3. **Seed Data**: Add sample subscription plans for testing
4. **Verify Integration**: Test Stripe integration and webhook handling

---

**🎯 Use the `RESET_SUBSCRIPTION_PLAN_TABLES.sql` script for a complete fresh start with your subscription plan system!**

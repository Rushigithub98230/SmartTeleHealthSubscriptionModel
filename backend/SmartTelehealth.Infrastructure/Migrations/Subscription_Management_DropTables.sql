-- SQL Script to systematically drop Subscription & Billing Tables
-- Tables are dropped in REVERSE dependency order to handle foreign key constraints
-- This ensures child tables are dropped before parent tables

-- ════════════════════════════════════════════════════════════════════════════
-- DROP ORDER: Drop dependent tables first, then their dependencies
-- ════════════════════════════════════════════════════════════════════════════

-- NOTE: This script drops ONLY subscription management tables
-- PaymentStatuses and RefundStatuses are MASTER tables and NOT included here
-- as they are referenced by Appointment tables (not subscription tables)

-- ════════════════════════════════════════════════════════════════════════════
-- TABLE MAPPING (Singular in queries → Plural in database)
-- ════════════════════════════════════════════════════════════════════════════
-- BillingRecords              ✅ Covered
-- SubscriptionPlan          → SubscriptionPlans ✅ Covered
-- Subscription              → Subscriptions ✅ Covered
-- Privilege                 → Privileges ✅ Covered
-- SubscriptionPlanPrivilege → SubscriptionPlanPrivileges ✅ Covered
-- UserSubscriptionPrivilegeUsage → UserSubscriptionPrivilegeUsages ✅ Covered
-- PrivilegeUsageHistory     → PrivilegeUsageHistories ✅ Covered
-- SubscriptionStatusHistory → SubscriptionStatusHistories ✅ Covered
-- SubscriptionPayment       → SubscriptionPayments ✅ Covered
-- PaymentStatuses           ❌ Master table (NOT part of subscriptions)
-- PaymentRefunds            ✅ Covered
-- ScheduledPlanMigrations   ✅ Covered
-- ════════════════════════════════════════════════════════════════════════════

-- 1. PrivilegeUsageHistories (depends on UserSubscriptionPrivilegeUsages)
IF OBJECT_ID('PrivilegeUsageHistories', 'U') IS NOT NULL
BEGIN
    DROP TABLE PrivilegeUsageHistories;
    PRINT 'Table PrivilegeUsageHistories dropped successfully.';
END
ELSE
    PRINT 'Table PrivilegeUsageHistories does not exist.';
GO

-- 2. PaymentRefunds (depends on SubscriptionPayments)
IF OBJECT_ID('PaymentRefunds', 'U') IS NOT NULL
BEGIN
    DROP TABLE PaymentRefunds;
    PRINT 'Table PaymentRefunds dropped successfully.';
END
ELSE
    PRINT 'Table PaymentRefunds does not exist.';
GO

-- 3. BillingAdjustments (depends on BillingRecords)
IF OBJECT_ID('BillingAdjustments', 'U') IS NOT NULL
BEGIN
    DROP TABLE BillingAdjustments;
    PRINT 'Table BillingAdjustments dropped successfully.';
END
ELSE
    PRINT 'Table BillingAdjustments does not exist.';
GO

-- 4. ScheduledPlanMigrations (depends on Subscriptions and SubscriptionPlans)
IF OBJECT_ID('ScheduledPlanMigrations', 'U') IS NOT NULL
BEGIN
    DROP TABLE ScheduledPlanMigrations;
    PRINT 'Table ScheduledPlanMigrations dropped successfully.';
END
ELSE
    PRINT 'Table ScheduledPlanMigrations does not exist.';
GO

-- 5. SubscriptionStatusHistories (depends on Subscriptions)
IF OBJECT_ID('SubscriptionStatusHistories', 'U') IS NOT NULL
BEGIN
    DROP TABLE SubscriptionStatusHistories;
    PRINT 'Table SubscriptionStatusHistories dropped successfully.';
END
ELSE
    PRINT 'Table SubscriptionStatusHistories does not exist.';
GO

-- 6. UserSubscriptionPrivilegeUsages (depends on SubscriptionPlanPrivileges, Privileges, and indirectly on Subscriptions)
IF OBJECT_ID('UserSubscriptionPrivilegeUsages', 'U') IS NOT NULL
BEGIN
    DROP TABLE UserSubscriptionPrivilegeUsages;
    PRINT 'Table UserSubscriptionPrivilegeUsages dropped successfully.';
END
ELSE
    PRINT 'Table UserSubscriptionPrivilegeUsages does not exist.';
GO

-- 7. SubscriptionPayments (depends on Subscriptions and BillingRecords)
IF OBJECT_ID('SubscriptionPayments', 'U') IS NOT NULL
BEGIN
    DROP TABLE SubscriptionPayments;
    PRINT 'Table SubscriptionPayments dropped successfully.';
END
ELSE
    PRINT 'Table SubscriptionPayments does not exist.';
GO

-- 8. SubscriptionPlanPrivileges (depends on SubscriptionPlans and Privileges)
IF OBJECT_ID('SubscriptionPlanPrivileges', 'U') IS NOT NULL
BEGIN
    DROP TABLE SubscriptionPlanPrivileges;
    PRINT 'Table SubscriptionPlanPrivileges dropped successfully.';
END
ELSE
    PRINT 'Table SubscriptionPlanPrivileges does not exist.';
GO

-- 9. Subscriptions (depends on SubscriptionPlans)
IF OBJECT_ID('Subscriptions', 'U') IS NOT NULL
BEGIN
    DROP TABLE Subscriptions;
    PRINT 'Table Subscriptions dropped successfully.';
END
ELSE
    PRINT 'Table Subscriptions does not exist.';
GO

-- 10. BillingRecords (FK to Subscriptions is commented, but data integrity suggests dropping after Subscriptions)
IF OBJECT_ID('BillingRecords', 'U') IS NOT NULL
BEGIN
    DROP TABLE BillingRecords;
    PRINT 'Table BillingRecords dropped successfully.';
END
ELSE
    PRINT 'Table BillingRecords does not exist.';
GO

-- 11. Privileges (no dependencies on other subscription tables)
IF OBJECT_ID('Privileges', 'U') IS NOT NULL
BEGIN
    DROP TABLE Privileges;
    PRINT 'Table Privileges dropped successfully.';
END
ELSE
    PRINT 'Table Privileges does not exist.';
GO

-- 12. SubscriptionPlans (self-referencing FK for ParentPlanId, but safe to drop here as last table)
IF OBJECT_ID('SubscriptionPlans', 'U') IS NOT NULL
BEGIN
    DROP TABLE SubscriptionPlans;
    PRINT 'Table SubscriptionPlans dropped successfully.';
END
ELSE
    PRINT 'Table SubscriptionPlans does not exist.';
GO

-- ════════════════════════════════════════════════════════════════════════════
-- COMPLETION MESSAGE
-- ════════════════════════════════════════════════════════════════════════════
PRINT '==============================================================';
PRINT 'All subscription management tables have been processed.';
PRINT '==============================================================';
GO


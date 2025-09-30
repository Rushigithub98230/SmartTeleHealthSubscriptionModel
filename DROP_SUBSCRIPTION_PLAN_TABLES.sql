-- =====================================================
-- DROP SUBSCRIPTION PLAN RELATED TABLES
-- =====================================================
-- This script drops all subscription plan related tables
-- in the correct order to handle foreign key constraints
-- =====================================================

-- Disable foreign key checks temporarily
SET FOREIGN_KEY_CHECKS = 0;

-- Drop tables in reverse dependency order
-- (Child tables first, then parent tables)

-- 1. Drop UserSubscriptionPrivilegeUsage table (references SubscriptionPlanPrivilege)
DROP TABLE IF EXISTS UserSubscriptionPrivilegeUsage;

-- 2. Drop PrivilegeUsageHistory table (references SubscriptionPlanPrivilege)
DROP TABLE IF EXISTS PrivilegeUsageHistory;

-- 3. Drop SubscriptionPlanPrivilege table (junction table)
DROP TABLE IF EXISTS SubscriptionPlanPrivilege;

-- 4. Drop Subscription table (references SubscriptionPlan)
DROP TABLE IF EXISTS Subscription;

-- 5. Drop SubscriptionPlan table (main table)
DROP TABLE IF EXISTS SubscriptionPlan;

-- 6. Drop ServiceConstraint table (references SubscriptionPlan)
DROP TABLE IF EXISTS ServiceConstraint;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================
-- Run these queries to verify tables are dropped

SELECT 'Verifying tables are dropped...' as Status;

SELECT 
    TABLE_NAME,
    'DROPPED' as Status
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME IN (
        'UserSubscriptionPrivilegeUsage',
        'PrivilegeUsageHistory', 
        'SubscriptionPlanPrivilege',
        'Subscription',
        'SubscriptionPlan',
        'ServiceConstraint'
    );

-- If no rows returned, all tables are successfully dropped
SELECT 'All subscription plan related tables have been dropped successfully!' as Result;

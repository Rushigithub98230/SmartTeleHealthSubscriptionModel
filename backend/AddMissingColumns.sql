-- Add missing columns to fix database schema mismatch
-- This script adds the missing columns that Entity Framework expects

-- Add PlanType column to SubscriptionPlans table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'PlanType')
BEGIN
    ALTER TABLE [dbo].[SubscriptionPlans] 
    ADD [PlanType] nvarchar(50) NOT NULL DEFAULT 'Standard';
    PRINT 'Added PlanType column to SubscriptionPlans table';
END
ELSE
BEGIN
    PRINT 'PlanType column already exists in SubscriptionPlans table';
END

-- Add UnitCost column to SubscriptionPlanPrivileges table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlanPrivileges]') AND name = 'UnitCost')
BEGIN
    ALTER TABLE [dbo].[SubscriptionPlanPrivileges] 
    ADD [UnitCost] decimal(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added UnitCost column to SubscriptionPlanPrivileges table';
END
ELSE
BEGIN
    PRINT 'UnitCost column already exists in SubscriptionPlanPrivileges table';
END

-- Check if Privileges table has PrivilegeTypeId column (it should be PrivilegeTypeId, not MasterPrivilegeTypeId)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Privileges]') AND name = 'PrivilegeTypeId')
BEGIN
    -- Check if MasterPrivilegeTypeId exists and rename it
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Privileges]') AND name = 'MasterPrivilegeTypeId')
    BEGIN
        EXEC sp_rename 'Privileges.MasterPrivilegeTypeId', 'PrivilegeTypeId', 'COLUMN';
        PRINT 'Renamed MasterPrivilegeTypeId to PrivilegeTypeId in Privileges table';
    END
    ELSE
    BEGIN
        ALTER TABLE [dbo].[Privileges] 
        ADD [PrivilegeTypeId] uniqueidentifier NOT NULL;
        PRINT 'Added PrivilegeTypeId column to Privileges table';
    END
END
ELSE
BEGIN
    PRINT 'PrivilegeTypeId column already exists in Privileges table';
END

-- Create index on PlanType for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'IX_SubscriptionPlans_PlanType')
BEGIN
    CREATE INDEX [IX_SubscriptionPlans_PlanType] ON [dbo].[SubscriptionPlans] ([PlanType]);
    PRINT 'Created index on PlanType column';
END
ELSE
BEGIN
    PRINT 'Index on PlanType already exists';
END

PRINT 'Database schema update completed successfully!';

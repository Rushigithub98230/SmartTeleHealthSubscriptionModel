-- =====================================================
-- MIGRATION SCRIPT: Add PrivilegeId to UserSubscriptionPrivilegeUsages
-- =====================================================
-- This script adds the missing PrivilegeId column and foreign key constraint
-- to the UserSubscriptionPrivilegeUsages table for direct privilege access.
-- 
-- Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
-- Purpose: Enable direct privilege access for performance optimization
-- =====================================================

-- Step 1: Add the PrivilegeId column
-- This column will store the direct reference to the Privilege entity
ALTER TABLE [UserSubscriptionPrivilegeUsages] 
ADD [PrivilegeId] uniqueidentifier NOT NULL;

-- Step 2: Add the foreign key constraint to Privileges table
-- This ensures referential integrity between usage records and privileges
ALTER TABLE [UserSubscriptionPrivilegeUsages]
ADD CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId] 
FOREIGN KEY ([PrivilegeId]) 
REFERENCES [Privileges] ([Id]) 
ON DELETE RESTRICT;

-- Step 3: Add performance index for the new foreign key
-- This optimizes queries that filter by PrivilegeId
CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_PrivilegeId] 
ON [UserSubscriptionPrivilegeUsages] ([PrivilegeId]);

-- Step 4: Add additional performance indexes for common query patterns
-- These indexes optimize privilege-based analytics and reporting
CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_UsagePeriodStart] 
ON [UserSubscriptionPrivilegeUsages] ([UsagePeriodStart]);

CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_UsagePeriodEnd] 
ON [UserSubscriptionPrivilegeUsages] ([UsagePeriodEnd]);

CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_LastUsedAt] 
ON [UserSubscriptionPrivilegeUsages] ([LastUsedAt]);

-- Step 5: Add composite indexes for common query patterns
-- These indexes optimize queries that combine multiple columns
CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_SubscriptionId_PrivilegeId] 
ON [UserSubscriptionPrivilegeUsages] ([SubscriptionId], [PrivilegeId]);

CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_PrivilegeId_UsagePeriodStart] 
ON [UserSubscriptionPrivilegeUsages] ([PrivilegeId], [UsagePeriodStart]);

-- Step 6: Add the missing Notes column (if it doesn't exist)
-- This column was referenced in the entity but might be missing from the table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserSubscriptionPrivilegeUsages') AND name = 'Notes')
BEGIN
    ALTER TABLE [UserSubscriptionPrivilegeUsages] 
    ADD [Notes] nvarchar(500) NULL;
END

-- Step 7: Add the missing ResetAt column (if it doesn't exist)
-- This column was referenced in the entity but might be missing from the table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserSubscriptionPrivilegeUsages') AND name = 'ResetAt')
BEGIN
    ALTER TABLE [UserSubscriptionPrivilegeUsages] 
    ADD [ResetAt] datetime2 NULL;
END

-- Step 8: Populate the PrivilegeId column with data from existing records
-- This step populates the new column using the existing SubscriptionPlanPrivilegeId relationship
UPDATE uspu 
SET uspu.[PrivilegeId] = spp.[PrivilegeId]
FROM [UserSubscriptionPrivilegeUsages] uspu
INNER JOIN [SubscriptionPlanPrivileges] spp 
    ON uspu.[SubscriptionPlanPrivilegeId] = spp.[Id];

-- Step 9: Verify the data population
-- This query shows the count of records that were updated
SELECT 
    'Records Updated' as Status,
    COUNT(*) as Count
FROM [UserSubscriptionPrivilegeUsages] 
WHERE [PrivilegeId] IS NOT NULL;

-- Step 10: Verify foreign key constraint
-- This query shows any records that might have invalid PrivilegeId references
SELECT 
    'Invalid PrivilegeId References' as Status,
    COUNT(*) as Count
FROM [UserSubscriptionPrivilegeUsages] uspu
LEFT JOIN [Privileges] p ON uspu.[PrivilegeId] = p.[Id]
WHERE p.[Id] IS NULL;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Query 1: Verify table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UserSubscriptionPrivilegeUsages'
ORDER BY ORDINAL_POSITION;

-- Query 2: Verify foreign key constraints
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
WHERE tp.name = 'UserSubscriptionPrivilegeUsages'
ORDER BY fk.name;

-- Query 3: Verify indexes
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    c.name AS ColumnName,
    ic.key_ordinal AS KeyOrdinal
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('UserSubscriptionPrivilegeUsages')
ORDER BY i.name, ic.key_ordinal;

-- =====================================================
-- ROLLBACK SCRIPT (if needed)
-- =====================================================
/*
-- To rollback this migration, run the following script:

-- Drop indexes
DROP INDEX [IX_UserSubscriptionPrivilegeUsages_PrivilegeId] ON [UserSubscriptionPrivilegeUsages];
DROP INDEX [IX_UserSubscriptionPrivilegeUsages_UsagePeriodStart] ON [UserSubscriptionPrivilegeUsages];
DROP INDEX [IX_UserSubscriptionPrivilegeUsages_UsagePeriodEnd] ON [UserSubscriptionPrivilegeUsages];
DROP INDEX [IX_UserSubscriptionPrivilegeUsages_LastUsedAt] ON [UserSubscriptionPrivilegeUsages];
DROP INDEX [IX_UserSubscriptionPrivilegeUsages_SubscriptionId_PrivilegeId] ON [UserSubscriptionPrivilegeUsages];
DROP INDEX [IX_UserSubscriptionPrivilegeUsages_PrivilegeId_UsagePeriodStart] ON [UserSubscriptionPrivilegeUsages];

-- Drop foreign key constraint
ALTER TABLE [UserSubscriptionPrivilegeUsages] 
DROP CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId];

-- Drop the PrivilegeId column
ALTER TABLE [UserSubscriptionPrivilegeUsages] 
DROP COLUMN [PrivilegeId];
*/

-- =====================================================
-- MIGRATION COMPLETE
-- =====================================================
PRINT 'Migration completed successfully!';
PRINT 'Added PrivilegeId column and foreign key constraint to UserSubscriptionPrivilegeUsages table.';
PRINT 'Added performance indexes for optimized queries.';
PRINT 'Populated PrivilegeId column with existing data.';

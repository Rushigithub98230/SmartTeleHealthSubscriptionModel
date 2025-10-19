-- ============================================
-- Fix Admin Role ID to 332
-- This script safely updates the UserRoles table and assigns admin users
-- ============================================

USE [SmartTelehealthSubscriptionDB]  -- Change to your database name
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '=== Starting Admin Role Fix ==='
    
    -- Step 1: Check if role with ID 332 exists
    IF EXISTS (SELECT 1 FROM UserRoles WHERE Id = 332)
    BEGIN
        PRINT 'Role with ID 332 already exists'
        
        -- Update it to be Admin if it's not
        UPDATE UserRoles 
        SET Name = 'Admin',
            Description = 'Administrator with full system access',
            SortOrder = 3
        WHERE Id = 332
        
        PRINT '✓ Updated role ID 332 to Admin'
    END
    ELSE
    BEGIN
        PRINT 'Role ID 332 does not exist, checking for existing Admin role...'
        
        -- Step 2: Check if an Admin role exists with different ID
        IF EXISTS (SELECT 1 FROM UserRoles WHERE Name = 'Admin')
        BEGIN
            DECLARE @OldAdminId INT
            SELECT @OldAdminId = Id FROM UserRoles WHERE Name = 'Admin'
            
            PRINT 'Found existing Admin role with ID: ' + CAST(@OldAdminId AS VARCHAR)
            
            -- Step 3: Update users who have the old Admin role ID
            UPDATE Users 
            SET UserRoleId = 332
            WHERE UserRoleId = @OldAdminId
            
            PRINT '✓ Updated users from old Admin role ID to 332'
            
            -- Step 4: Delete the old Admin role
            DELETE FROM UserRoles WHERE Id = @OldAdminId
            
            PRINT '✓ Deleted old Admin role with ID: ' + CAST(@OldAdminId AS VARCHAR)
        END
        
        -- Step 5: Insert Admin role with ID 332
        SET IDENTITY_INSERT UserRoles ON
        
        INSERT INTO UserRoles (Id, Name, Description, SortOrder, IsActive, CreatedDate, UpdatedDate)
        VALUES (332, 'Admin', 'Administrator with full system access', 3, 1, GETUTCDATE(), GETUTCDATE())
        
        SET IDENTITY_INSERT UserRoles OFF
        
        PRINT '✓ Created new Admin role with ID 332'
    END
    
    -- Step 6: Ensure Client role exists (ID = 1)
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Id = 1)
    BEGIN
        SET IDENTITY_INSERT UserRoles ON
        
        INSERT INTO UserRoles (Id, Name, Description, SortOrder, IsActive, CreatedDate, UpdatedDate)
        VALUES (1, 'Client', 'Standard client user', 1, 1, GETUTCDATE(), GETUTCDATE())
        
        SET IDENTITY_INSERT UserRoles OFF
        
        PRINT '✓ Created Client role with ID 1'
    END
    
    -- Step 7: Ensure Provider role exists (ID = 2)
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Id = 2)
    BEGIN
        SET IDENTITY_INSERT UserRoles ON
        
        INSERT INTO UserRoles (Id, Name, Description, SortOrder, IsActive, CreatedDate, UpdatedDate)
        VALUES (2, 'Provider', 'Healthcare provider', 2, 1, GETUTCDATE(), GETUTCDATE())
        
        SET IDENTITY_INSERT UserRoles OFF
        
        PRINT '✓ Created Provider role with ID 2'
    END
    
    -- Step 8: Update specific user to Admin (change email as needed)
    UPDATE Users 
    SET UserRoleId = 332,
        UserType = 'Admin'
    WHERE Email = 'admin@test.com'
    
    IF @@ROWCOUNT > 0
        PRINT '✓ Updated admin@test.com to Admin role (ID 332)'
    ELSE
        PRINT '⚠ User admin@test.com not found - you may need to create this user first'
    
    -- Step 9: Verify the setup
    PRINT ''
    PRINT '=== Verification ==='
    SELECT Id, Name, Description, SortOrder, IsActive 
    FROM UserRoles 
    ORDER BY Id
    
    PRINT ''
    PRINT '=== Admin Users ==='
    SELECT Id, Email, FirstName, LastName, UserRoleId, UserType
    FROM Users
    WHERE UserRoleId = 332
    
    COMMIT TRANSACTION;
    
    PRINT ''
    PRINT '✅ SUCCESS: Admin role setup complete!'
    PRINT 'Admin users must log out and log back in to get new JWT token with RoleId=332'
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT '❌ ERROR: ' + ERROR_MESSAGE()
    PRINT 'Transaction rolled back - no changes made'
    
    -- Show error details
    SELECT 
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_SEVERITY() AS ErrorSeverity,
        ERROR_STATE() AS ErrorState,
        ERROR_PROCEDURE() AS ErrorProcedure,
        ERROR_LINE() AS ErrorLine,
        ERROR_MESSAGE() AS ErrorMessage
END CATCH

GO

-- ============================================
-- OPTIONAL: Create additional admin users
-- ============================================

-- Uncomment and modify as needed:

/*
UPDATE Users 
SET UserRoleId = 332, UserType = 'Admin'
WHERE Email IN (
    'admin@smarttelehealth.com',
    'superadmin@smarttelehealth.com'
)
*/

-- ============================================
-- Final Verification Query
-- ============================================
PRINT ''
PRINT '=== Final Status ==='
SELECT 
    'Roles' AS TableName,
    COUNT(*) AS TotalCount,
    SUM(CASE WHEN Name = 'Admin' THEN 1 ELSE 0 END) AS AdminRoles,
    SUM(CASE WHEN Id = 332 THEN 1 ELSE 0 END) AS Role332Count
FROM UserRoles

UNION ALL

SELECT 
    'Admin Users' AS TableName,
    COUNT(*) AS TotalCount,
    SUM(CASE WHEN UserType = 'Admin' THEN 1 ELSE 0 END) AS AdminTypeCount,
    SUM(CASE WHEN UserRoleId = 332 THEN 1 ELSE 0 END) AS RoleId332Count
FROM Users
WHERE UserRoleId = 332 OR UserType = 'Admin'

GO



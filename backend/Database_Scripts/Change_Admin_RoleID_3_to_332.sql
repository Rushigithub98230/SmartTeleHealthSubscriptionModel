-- ============================================
-- Change Admin Role ID from 3 to 332
-- Safe migration script with rollback support
-- ============================================

USE [SmartTelehealthSubscriptionDB]  -- Change to your database name
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '=== Starting Admin Role ID Migration (3 → 332) ==='
    PRINT ''
    
    -- Step 1: Verify current state
    PRINT 'Step 1: Verifying current Admin role...'
    
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Id = 3)
    BEGIN
        PRINT '❌ ERROR: Role ID 3 does not exist!'
        PRINT 'Please verify your database. Current roles:'
        SELECT Id, Name, Description FROM UserRoles ORDER BY Id
        ROLLBACK TRANSACTION
        RETURN
    END
    
    DECLARE @CurrentRoleName NVARCHAR(50)
    SELECT @CurrentRoleName = Name FROM UserRoles WHERE Id = 3
    
    PRINT '   Current Role ID 3: ' + @CurrentRoleName
    
    -- Step 2: Check if ID 332 already exists
    IF EXISTS (SELECT 1 FROM UserRoles WHERE Id = 332)
    BEGIN
        PRINT '⚠ WARNING: Role ID 332 already exists!'
        SELECT Id, Name, Description FROM UserRoles WHERE Id = 332
        PRINT 'Please manually resolve this conflict before proceeding'
        ROLLBACK TRANSACTION
        RETURN
    END
    
    -- Step 3: Count affected users
    DECLARE @AffectedUsers INT
    SELECT @AffectedUsers = COUNT(*) FROM Users WHERE UserRoleId = 3
    
    PRINT ''
    PRINT 'Step 2: Found ' + CAST(@AffectedUsers AS VARCHAR) + ' users with RoleId = 3'
    
    IF @AffectedUsers > 0
    BEGIN
        PRINT '   Users to be updated:'
        SELECT Id, Email, FirstName, LastName, UserType, UserRoleId 
        FROM Users 
        WHERE UserRoleId = 3
    END
    
    -- Step 4: Create new Admin role with ID 332
    PRINT ''
    PRINT 'Step 3: Creating new Admin role with ID 332...'
    
    SET IDENTITY_INSERT UserRoles ON
    
    INSERT INTO UserRoles (Id, Name, Description, SortOrder, IsActive, CreatedDate, UpdatedDate)
    SELECT 
        332,                    -- New ID
        Name,                   -- Keep name from old role
        'Administrator with full system access',  -- Updated description
        3,                      -- Sort order
        IsActive,
        CreatedDate,
        GETUTCDATE()           -- New update date
    FROM UserRoles 
    WHERE Id = 3
    
    SET IDENTITY_INSERT UserRoles OFF
    
    PRINT '   ✓ Created UserRoles with ID 332'
    
    -- Step 5: Update all users to point to new role ID
    PRINT ''
    PRINT 'Step 4: Updating users to new role ID 332...'
    
    UPDATE Users 
    SET UserRoleId = 332,
        UserType = 'Admin',      -- Ensure UserType is also set
        UpdatedDate = GETUTCDATE()
    WHERE UserRoleId = 3
    
    PRINT '   ✓ Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' users to RoleId 332'
    
    -- Step 6: Delete old role ID 3
    PRINT ''
    PRINT 'Step 5: Removing old role ID 3...'
    
    DELETE FROM UserRoles WHERE Id = 3
    
    PRINT '   ✓ Deleted old role ID 3'
    
    -- Step 7: Verify the migration
    PRINT ''
    PRINT '=== Migration Verification ==='
    
    PRINT 'UserRoles Table:'
    SELECT Id, Name, Description, SortOrder, IsActive 
    FROM UserRoles 
    ORDER BY Id
    
    PRINT ''
    PRINT 'Admin Users (RoleId = 332):'
    SELECT Id, Email, FirstName, LastName, UserRoleId, UserType, IsActive
    FROM Users
    WHERE UserRoleId = 332
    
    PRINT ''
    PRINT '=== Checking for orphaned users ==='
    SELECT COUNT(*) AS OrphanedUsers
    FROM Users
    WHERE UserRoleId NOT IN (SELECT Id FROM UserRoles)
    
    -- Step 8: Commit if everything looks good
    COMMIT TRANSACTION;
    
    PRINT ''
    PRINT '✅ SUCCESS: Admin Role ID migrated from 3 to 332'
    PRINT ''
    PRINT '⚠ IMPORTANT NEXT STEPS:'
    PRINT '1. Admin users MUST log out from frontend'
    PRINT '2. Clear browser localStorage (F12 → Application → Clear)'
    PRINT '3. Log back in to get new JWT token with RoleId=332'
    PRINT '4. Admin portal will now work without 403 errors!'
    PRINT ''
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT ''
    PRINT '❌ ERROR OCCURRED - Transaction rolled back'
    PRINT 'No changes were made to the database'
    PRINT ''
    
    -- Show detailed error
    SELECT 
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_SEVERITY() AS ErrorSeverity,
        ERROR_STATE() AS ErrorState,
        ERROR_PROCEDURE() AS ErrorProcedure,
        ERROR_LINE() AS ErrorLine,
        ERROR_MESSAGE() AS ErrorMessage
        
    PRINT ''
    PRINT 'Please review the error and try again'
END CATCH

GO

-- ============================================
-- Post-Migration Verification Queries
-- ============================================

PRINT ''
PRINT '=== Post-Migration Checks ==='

-- Check 1: Verify Admin role exists with ID 332
IF EXISTS (SELECT 1 FROM UserRoles WHERE Id = 332 AND Name = 'Admin')
    PRINT '✓ Check 1: Admin role exists with ID 332'
ELSE
    PRINT '❌ Check 1 FAILED: Admin role with ID 332 not found'

-- Check 2: Verify no users have old role ID 3
IF NOT EXISTS (SELECT 1 FROM Users WHERE UserRoleId = 3)
    PRINT '✓ Check 2: No users have old RoleId 3'
ELSE
    PRINT '❌ Check 2 FAILED: Some users still have RoleId 3'

-- Check 3: Verify admin users exist
DECLARE @AdminCount INT
SELECT @AdminCount = COUNT(*) FROM Users WHERE UserRoleId = 332

IF @AdminCount > 0
    PRINT '✓ Check 3: ' + CAST(@AdminCount AS VARCHAR) + ' admin users found with RoleId 332'
ELSE
    PRINT '⚠ Check 3: No admin users found - you may need to assign admin role to users'

GO

-- ============================================
-- QUICK REFERENCE: Assign Admin Role to User
-- ============================================

/*
-- To make any user an admin, run this:

UPDATE Users 
SET UserRoleId = 332,
    UserType = 'Admin',
    UpdatedDate = GETUTCDATE()
WHERE Email = 'youremail@example.com'

-- Verify:
SELECT Id, Email, UserRoleId, UserType FROM Users WHERE Email = 'youremail@example.com'
*/



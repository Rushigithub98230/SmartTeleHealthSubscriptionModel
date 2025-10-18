-- ═══════════════════════════════════════════════════════════════════════════
-- AUTO-VERSION EXISTING PLANS (Choice 3a)
-- ═══════════════════════════════════════════════════════════════════════════
-- Purpose: Migrate all existing subscription plans to the new versioning system
-- Run this AFTER deploying the AddHealthcarePlanVersioningAndPricing migration
-- ═══════════════════════════════════════════════════════════════════════════

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '═══════════════════════════════════════════════════════════════════';
    PRINT 'Starting Auto-Versioning of Existing Subscription Plans...';
    PRINT '═══════════════════════════════════════════════════════════════════';
    
    -- Step 1: Update all existing plans to be version 1
    UPDATE SubscriptionPlans
    SET 
        VersionNumber = 1,
        IsLatestVersion = 1,
        ParentPlanId = NULL,
        VersionCreatedDate = GETUTCDATE(),
        IsAutoCalculatedPrice = 0,  -- Existing plans keep manual pricing
        PriceChangeNoticeDays = 10,  -- Healthcare default (10 days notice)
        PrivilegesTotalCost = 0,     -- Will be calculated if they switch to auto mode
        AdminCommissionPercent = NULL, -- Use global default (20%)
        AdminCommissionFixed = NULL
    WHERE VersionNumber IS NULL OR VersionNumber = 0;
    
    DECLARE @UpdatedCount INT = @@ROWCOUNT;
    PRINT CONCAT('✅ Updated ', @UpdatedCount, ' existing plans to version 1.0');
    
    -- Step 2: Verify the update
    DECLARE @Version1Count INT = (SELECT COUNT(*) FROM SubscriptionPlans WHERE VersionNumber = 1);
    PRINT CONCAT('✅ Total plans at version 1: ', @Version1Count);
    
    -- Step 3: Show summary of versioned plans
    PRINT '';
    PRINT 'Plan Versioning Summary:';
    PRINT '─────────────────────────────────────────────────────────────────';
    
    SELECT 
        Id,
        Name,
        VersionNumber,
        IsLatestVersion,
        IsAutoCalculatedPrice,
        PriceChangeNoticeDays,
        Price AS CurrentPrice,
        (SELECT COUNT(*) FROM Subscriptions WHERE SubscriptionPlanId = SubscriptionPlans.Id AND Status = 'Active') AS ActiveSubscriptions
    FROM SubscriptionPlans
    WHERE VersionNumber = 1
    ORDER BY Name;
    
    PRINT '';
    PRINT '═══════════════════════════════════════════════════════════════════';
    PRINT 'Auto-Versioning Completed Successfully!';
    PRINT '═══════════════════════════════════════════════════════════════════';
    PRINT '';
    PRINT 'Next Steps:';
    PRINT '1. All existing plans are now version 1.0 with manual pricing';
    PRINT '2. When you update a plan, a new version will be created automatically';
    PRINT '3. Users on old versions will be scheduled for migration at their renewal dates';
    PRINT '4. Overage charges will use the latest plan version pricing (abuse prevention)';
    PRINT '';
    
    COMMIT TRANSACTION;
    PRINT '✅ Transaction committed successfully.';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT '❌ ERROR: Auto-versioning failed!';
    PRINT CONCAT('Error Message: ', ERROR_MESSAGE());
    PRINT CONCAT('Error Line: ', ERROR_LINE());
    PRINT CONCAT('Error Severity: ', ERROR_SEVERITY());
    
    -- Re-throw the error
    THROW;
END CATCH;

GO

-- ═══════════════════════════════════════════════════════════════════════════
-- VERIFICATION QUERIES (Optional - Run separately to verify)
-- ═══════════════════════════════════════════════════════════════════════════

-- Verify all plans are versioned
/*
SELECT 
    COUNT(*) AS TotalPlans,
    SUM(CASE WHEN VersionNumber = 1 THEN 1 ELSE 0 END) AS Version1Plans,
    SUM(CASE WHEN IsLatestVersion = 1 THEN 1 ELSE 0 END) AS LatestVersionPlans,
    SUM(CASE WHEN IsAutoCalculatedPrice = 1 THEN 1 ELSE 0 END) AS AutoCalculatedPlans,
    SUM(CASE WHEN IsAutoCalculatedPrice = 0 THEN 1 ELSE 0 END) AS ManualPricePlans
FROM SubscriptionPlans;

-- Show plans with active subscriptions
SELECT 
    sp.Id,
    sp.Name,
    sp.VersionNumber,
    sp.Price,
    sp.IsAutoCalculatedPrice,
    sp.PriceChangeNoticeDays,
    COUNT(s.Id) AS ActiveSubscriptions
FROM SubscriptionPlans sp
LEFT JOIN Subscriptions s ON sp.Id = s.SubscriptionPlanId AND s.Status = 'Active'
GROUP BY sp.Id, sp.Name, sp.VersionNumber, sp.Price, sp.IsAutoCalculatedPrice, sp.PriceChangeNoticeDays
HAVING COUNT(s.Id) > 0
ORDER BY COUNT(s.Id) DESC;
*/


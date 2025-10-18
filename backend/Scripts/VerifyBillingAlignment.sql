-- ================================================================
-- BILLING CYCLE & PRIVILEGE ALIGNMENT VERIFICATION QUERIES
-- Solution A Implementation Verification
-- ================================================================

-- Query 1: Check subscriptions with mismatched pricing
-- This identifies subscriptions where CurrentPrice doesn't match expected scaled price
SELECT 
    s.Id as SubscriptionId,
    u.Email as UserEmail,
    s.CurrentPrice as ActualPrice,
    sp.Price as PlanMonthlyPrice,
    bc.Name as BillingCycle,
    bc.DurationInDays,
    (sp.Price * bc.DurationInDays / 30.0) as ExpectedBasePrice,
    sp.AnnualBillingDiscount as AnnualDiscount,
    sp.QuarterlyBillingDiscount as QuarterlyDiscount,
    CASE 
        WHEN ABS(s.CurrentPrice - (sp.Price * bc.DurationInDays / 30.0)) > 0.01 
        THEN 'MISMATCH - NEEDS MIGRATION' 
        ELSE 'OK' 
    END as PriceStatus,
    s.CreatedDate,
    s.NextBillingDate
FROM Subscriptions s
JOIN SubscriptionPlans sp ON s.SubscriptionPlanId = sp.Id
JOIN MasterBillingCycles bc ON s.BillingCycleId = bc.Id
JOIN Users u ON s.UserId = u.Id
WHERE s.IsActive = 1
ORDER BY 
    CASE WHEN ABS(s.CurrentPrice - (sp.Price * bc.DurationInDays / 30.0)) > 0.01 THEN 0 ELSE 1 END,
    s.CreatedDate DESC;

-- Query 2: Check privilege allocations
-- This verifies that privilege limits are correctly scaled to billing cycles
SELECT 
    u.SubscriptionId,
    sub.UserId,
    usr.Email as UserEmail,
    p.Name as PrivilegeName,
    u.AllowedValue as ActualAllocation,
    u.UsedValue,
    spp.MonthlyLimit as PlanMonthlyLimit,
    bc.Name as BillingCycle,
    bc.DurationInDays,
    (spp.MonthlyLimit * bc.DurationInDays / 30.0) as ExpectedAllocation,
    DATEDIFF(day, u.UsagePeriodStart, u.UsagePeriodEnd) as ActualPeriodDays,
    bc.DurationInDays as ExpectedPeriodDays,
    CASE 
        WHEN u.AllowedValue != CEILING(spp.MonthlyLimit * bc.DurationInDays / 30.0) 
        THEN 'MISMATCH - NEEDS UPDATE' 
        ELSE 'OK' 
    END as AllocationStatus,
    u.UsagePeriodStart,
    u.UsagePeriodEnd,
    sub.NextBillingDate
FROM UserSubscriptionPrivilegeUsages u
JOIN Subscriptions sub ON u.SubscriptionId = sub.Id
JOIN Users usr ON sub.UserId = usr.Id
JOIN SubscriptionPlanPrivileges spp ON u.SubscriptionPlanPrivilegeId = spp.Id
JOIN Privileges p ON spp.PrivilegeId = p.Id
JOIN MasterBillingCycles bc ON sub.BillingCycleId = bc.Id
WHERE sub.IsActive = 1
  AND spp.MonthlyLimit IS NOT NULL
ORDER BY 
    CASE WHEN u.AllowedValue != CEILING(spp.MonthlyLimit * bc.DurationInDays / 30.0) THEN 0 ELSE 1 END,
    sub.UserId;

-- Query 3: Check for expired privilege periods
-- This identifies privileges where the usage period has ended but hasn't been reset
SELECT 
    u.SubscriptionId,
    usr.Email as UserEmail,
    p.Name as PrivilegeName,
    u.UsedValue,
    u.AllowedValue,
    u.UsagePeriodEnd as ExpiredDate,
    DATEDIFF(day, u.UsagePeriodEnd, GETUTCDATE()) as DaysSinceExpiry,
    sub.Status as SubscriptionStatus,
    sub.NextBillingDate,
    CASE 
        WHEN sub.NextBillingDate < GETUTCDATE() THEN 'BILLING OVERDUE - CHECK PAYMENT'
        WHEN u.UsagePeriodEnd < GETUTCDATE() THEN 'EXPIRED - WILL RESET ON NEXT BILLING'
        ELSE 'ACTIVE'
    END as PrivilegeStatus
FROM UserSubscriptionPrivilegeUsages u
JOIN Subscriptions sub ON u.SubscriptionId = sub.Id
JOIN Users usr ON sub.UserId = usr.Id
JOIN SubscriptionPlanPrivileges spp ON u.SubscriptionPlanPrivilegeId = spp.Id
JOIN Privileges p ON spp.PrivilegeId = p.Id
WHERE u.UsagePeriodEnd < GETUTCDATE()
  AND sub.IsActive = 1
ORDER BY DaysSinceExpiry DESC;

-- Query 4: Billing cycle distribution and revenue analysis
-- Shows how many subscriptions are on each billing cycle and potential revenue
SELECT 
    bc.Name as BillingCycle,
    bc.DurationInDays,
    COUNT(s.Id) as SubscriptionCount,
    SUM(s.CurrentPrice) as TotalRevenue,
    AVG(s.CurrentPrice) as AvgPrice,
    AVG(sp.Price) as AvgMonthlyBasePrice,
    AVG(s.CurrentPrice / (bc.DurationInDays / 30.0)) as AvgMonthlyRevenue
FROM Subscriptions s
JOIN SubscriptionPlans sp ON s.SubscriptionPlanId = sp.Id
JOIN MasterBillingCycles bc ON s.BillingCycleId = bc.Id
WHERE s.IsActive = 1
GROUP BY bc.Name, bc.DurationInDays
ORDER BY bc.DurationInDays;

-- Query 5: Discount effectiveness analysis
-- Shows which discounts are being applied and their impact
SELECT 
    sp.Name as PlanName,
    bc.Name as BillingCycle,
    COUNT(s.Id) as Subscriptions,
    sp.Price as MonthlyPrice,
    (sp.Price * bc.DurationInDays / 30.0) as BasePrice,
    CASE bc.Name
        WHEN 'Annual' THEN sp.AnnualBillingDiscount
        WHEN 'Quarterly' THEN sp.QuarterlyBillingDiscount
        WHEN 'Monthly' THEN sp.MonthlyBillingDiscount
        ELSE 0
    END as DiscountPercent,
    AVG(s.CurrentPrice) as AvgActualPrice,
    AVG((sp.Price * bc.DurationInDays / 30.0) - s.CurrentPrice) as AvgDiscountAmount
FROM Subscriptions s
JOIN SubscriptionPlans sp ON s.SubscriptionPlanId = sp.Id
JOIN MasterBillingCycles bc ON s.BillingCycleId = bc.Id
WHERE s.IsActive = 1
GROUP BY sp.Name, sp.Price, bc.Name, bc.DurationInDays, 
         sp.AnnualBillingDiscount, sp.QuarterlyBillingDiscount, sp.MonthlyBillingDiscount
ORDER BY sp.Name, bc.DurationInDays;

-- Query 6: Revenue protection check
-- Identifies potential revenue loss from misaligned billing
SELECT 
    'BEFORE FIX' as Scenario,
    SUM(sp.Price) as MonthlyRevenueLost,
    SUM(sp.Price * 12) as AnnualRevenueLost
FROM Subscriptions s
JOIN SubscriptionPlans sp ON s.SubscriptionPlanId = sp.Id
JOIN MasterBillingCycles bc ON s.BillingCycleId = bc.Id
WHERE s.IsActive = 1
  AND bc.Name IN ('Annual', 'Yearly')
  AND ABS(s.CurrentPrice - (sp.Price * 12)) > 0.01

UNION ALL

SELECT 
    'AFTER FIX' as Scenario,
    0 as MonthlyRevenueLost,
    0 as AnnualRevenueLost;

-- Query 7: Privilege usage summary by billing cycle
-- Shows how usage patterns differ across billing cycles
SELECT 
    bc.Name as BillingCycle,
    p.Name as PrivilegeName,
    COUNT(DISTINCT u.SubscriptionId) as Subscriptions,
    AVG(u.AllowedValue) as AvgAllowed,
    AVG(u.UsedValue) as AvgUsed,
    AVG(CAST(u.UsedValue as FLOAT) / NULLIF(u.AllowedValue, 0) * 100) as AvgUsagePercent,
    SUM(CASE WHEN u.UsedValue >= u.AllowedValue THEN 1 ELSE 0 END) as FullyUsedCount
FROM UserSubscriptionPrivilegeUsages u
JOIN Subscriptions sub ON u.SubscriptionId = sub.Id
JOIN MasterBillingCycles bc ON sub.BillingCycleId = bc.Id
JOIN SubscriptionPlanPrivileges spp ON u.SubscriptionPlanPrivilegeId = spp.Id
JOIN Privileges p ON spp.PrivilegeId = p.Id
WHERE sub.IsActive = 1
  AND u.AllowedValue > 0
GROUP BY bc.Name, p.Name
ORDER BY bc.Name, p.Name;


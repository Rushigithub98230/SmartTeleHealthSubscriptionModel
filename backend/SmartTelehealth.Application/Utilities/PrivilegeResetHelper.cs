using Microsoft.Extensions.Logging;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Utilities;

/// <summary>
/// Centralized utility for resetting privileges at billing period boundaries.
/// SINGLE SOURCE OF TRUTH for privilege reset logic.
/// Ensures consistent privilege allocation and reset behavior across all services.
/// 
/// Usage:
/// - Called during subscription renewal (after payment success)
/// - Called during billing period transitions
/// - Uses PrivilegeAllocationCalculator for consistent allocation
/// - Updates ALL privilege fields including usage periods
/// 
/// Critical Timing:
/// - MUST be called AFTER subscription billing dates are updated
/// - subscription.LastBillingDate should be set to start of new period
/// - subscription.NextBillingDate should be set to end of new period
/// </summary>
public static class PrivilegeResetHelper
{
    /// <summary>
    /// Resets all privileges for a subscription at the start of a new billing period.
    /// Uses PrivilegeAllocationCalculator to ensure consistent allocation logic.
    /// SINGLE SOURCE OF TRUTH for privilege resets.
    /// 
    /// CRITICAL REQUIREMENTS:
    /// 1. Subscription MUST have updated LastBillingDate and NextBillingDate
    /// 2. Subscription MUST include SubscriptionPlan with PlanPrivileges loaded
    /// 3. UpdateUsageAsync delegate MUST persist changes to database
    /// 
    /// Reset Process:
    /// 1. For each privilege usage record
    /// 2. Get corresponding plan privilege configuration
    /// 3. Calculate new allocation using PrivilegeAllocationCalculator
    /// 4. Reset UsedValue to 0
    /// 5. Update AllowedValue to new allocation
    /// 6. Update UsagePeriodStart/End to new billing period
    /// 7. Set ResetAt timestamp
    /// 8. Update audit fields
    /// 9. Persist changes via delegate
    /// </summary>
    /// <param name="subscription">Subscription with updated billing dates and loaded plan/privileges</param>
    /// <param name="usageRecords">All privilege usage records for this subscription</param>
    /// <param name="updateUsageAsync">Async delegate to persist each updated usage record</param>
    /// <param name="updatedByUserId">User ID performing the reset (for audit)</param>
    /// <param name="logger">Logger for tracking reset operations</param>
    /// <returns>Task completing when all privileges are reset</returns>
    public static async Task ResetPrivilegesForBillingPeriodAsync(
        Subscription subscription,
        IEnumerable<UserSubscriptionPrivilegeUsage> usageRecords,
        Func<UserSubscriptionPrivilegeUsage, Task> updateUsageAsync,
        int updatedByUserId,
        ILogger logger)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));
            
        if (subscription.SubscriptionPlan == null)
            throw new InvalidOperationException("Subscription must include SubscriptionPlan for privilege reset");
            
        if (usageRecords == null)
            throw new ArgumentNullException(nameof(usageRecords));
            
        if (updateUsageAsync == null)
            throw new ArgumentNullException(nameof(updateUsageAsync));
            
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));
        
        logger.LogInformation(
            "Starting privilege reset for subscription {SubscriptionId}: BillingCycle={Cycle}, " +
            "PeriodStart={PeriodStart:yyyy-MM-dd}, PeriodEnd={PeriodEnd:yyyy-MM-dd}",
            subscription.Id, 
            subscription.BillingCycle?.Name ?? "Unknown",
            subscription.LastBillingDate ?? subscription.StartDate,
            subscription.NextBillingDate);
        
        int resetCount = 0;
        int skippedCount = 0;
        
        foreach (var usage in usageRecords)
        {
            try
            {
                // Find corresponding plan privilege configuration
                var planPrivilege = subscription.SubscriptionPlan.PlanPrivileges
                    .FirstOrDefault(p => p.Id == usage.SubscriptionPlanPrivilegeId);
                
                if (planPrivilege == null)
                {
                    logger.LogWarning(
                        "Plan privilege not found for usage record {UsageId} (PlanPrivilegeId: {PlanPrivilegeId}). Skipping reset.",
                        usage.Id, usage.SubscriptionPlanPrivilegeId);
                    skippedCount++;
                    continue;
                }
                
                // Use centralized calculator for ALL allocation logic
                // This ensures consistent allocation across the entire system
                var (allowedValue, periodStart, periodEnd) = 
                    PrivilegeAllocationCalculator.CalculatePrivilegeAllocation(subscription, planPrivilege);
                
                // Store old values for logging
                var oldUsedValue = usage.UsedValue;
                var oldAllowedValue = usage.AllowedValue;
                
                // Reset ALL fields to ensure complete state refresh
                usage.UsedValue = 0;
                usage.AllowedValue = allowedValue;
                usage.UsagePeriodStart = periodStart;
                usage.UsagePeriodEnd = periodEnd;
                usage.ResetAt = DateTime.UtcNow;
                usage.UpdatedBy = updatedByUserId;
                usage.UpdatedDate = DateTime.UtcNow;
                
                // Persist changes via delegate
                await updateUsageAsync(usage);
                resetCount++;
                
                // Log detailed reset information
                var privilegeName = planPrivilege.Privilege?.Name ?? "Unknown";
                var isUnlimited = allowedValue == -1;
                
                logger.LogInformation(
                    "✓ Reset privilege '{PrivilegeName}' (ID: {PrivilegeId}): " +
                    "Used={OldUsed}→0, Allowed={OldAllowed}→{NewAllowed}{Unlimited}, " +
                    "Period={Start:yyyy-MM-dd} to {End:yyyy-MM-dd}",
                    privilegeName, 
                    planPrivilege.PrivilegeId, 
                    oldUsedValue,
                    oldAllowedValue, 
                    allowedValue,
                    isUnlimited ? " (Unlimited)" : "",
                    periodStart, 
                    periodEnd);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, 
                    "Error resetting privilege usage {UsageId} for subscription {SubscriptionId}. Continuing with other privileges.",
                    usage.Id, subscription.Id);
                skippedCount++;
                // Continue with other privileges - don't fail entire reset
            }
        }
        
        logger.LogInformation(
            "Completed privilege reset for subscription {SubscriptionId}: " +
            "Reset={ResetCount}, Skipped={SkippedCount}, Total={TotalCount}",
            subscription.Id, resetCount, skippedCount, usageRecords.Count());
    }
    
    /// <summary>
    /// Validates that a subscription is ready for privilege reset.
    /// Checks that all required data is loaded and billing dates are set.
    /// </summary>
    /// <param name="subscription">Subscription to validate</param>
    /// <returns>Tuple of (isValid, errorMessage)</returns>
    public static (bool isValid, string errorMessage) ValidateSubscriptionForReset(Subscription subscription)
    {
        if (subscription == null)
            return (false, "Subscription is null");
            
        if (subscription.SubscriptionPlan == null)
            return (false, "SubscriptionPlan is not loaded");
            
        if (subscription.BillingCycle == null)
            return (false, "BillingCycle is not loaded");
            
        if (!subscription.LastBillingDate.HasValue && subscription.StartDate == default(DateTime))
            return (false, "LastBillingDate and StartDate are both empty");
            
        if (subscription.NextBillingDate == default(DateTime))
            return (false, "NextBillingDate is not set");
            
        if (subscription.SubscriptionPlan.PlanPrivileges == null || !subscription.SubscriptionPlan.PlanPrivileges.Any())
            return (false, "SubscriptionPlan.PlanPrivileges is not loaded or empty");
            
        return (true, string.Empty);
    }
}

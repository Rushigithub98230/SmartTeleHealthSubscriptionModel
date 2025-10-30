using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service for handling synchronization between local database and Stripe
/// </summary>
public interface IStripeSynchronizationService
{
    /// <summary>
    /// Synchronize a subscription plan with Stripe (create/update product and prices)
    /// </summary>
    Task<bool> SynchronizeSubscriptionPlanAsync(Guid planId, TokenModel tokenModel);
    
    /// <summary>
    /// Synchronize subscription plan deletion with Stripe cleanup
    /// </summary>
    Task<bool> SynchronizeSubscriptionPlanDeletionAsync(Guid planId, TokenModel tokenModel);
    
    /// <summary>
    /// Synchronize subscription status changes with Stripe
    /// </summary>
    Task<bool> SynchronizeSubscriptionStatusAsync(Guid subscriptionId, string newStatus, TokenModel tokenModel);
    
    /// <summary>
    /// Synchronize customer information with Stripe
    /// </summary>
    Task<bool> SynchronizeCustomerAsync(int userId, TokenModel tokenModel);
    
    /// <summary>
    /// Validate Stripe synchronization status for a subscription plan
    /// </summary>
    Task<StripeSyncValidationResult> ValidatePlanSynchronizationAsync(Guid planId, TokenModel tokenModel);
    
    /// <summary>
    /// Validate Stripe synchronization status for a subscription
    /// </summary>
    Task<StripeSyncValidationResult> ValidateSubscriptionSynchronizationAsync(Guid subscriptionId, TokenModel tokenModel);
    
    /// <summary>
    /// Repair Stripe synchronization for a subscription plan
    /// </summary>
    Task<bool> RepairPlanSynchronizationAsync(Guid planId, TokenModel tokenModel);
    
    /// <summary>
    /// Repair Stripe synchronization for a subscription
    /// </summary>
    Task<bool> RepairSubscriptionSynchronizationAsync(Guid subscriptionId, TokenModel tokenModel);
    
    // ===== PHASE 3: BACKGROUND SYNC & RECONCILIATION =====
    
    /// <summary>
    /// Synchronizes all subscriptions from Stripe to local database
    /// Compares Stripe subscription data with local records and updates statuses, billing dates
    /// Phase 3: Background sync job support
    /// </summary>
    Task<JsonModel> SyncAllSubscriptionsFromStripeAsync(TokenModel tokenModel);
    
    /// <summary>
    /// Checks consistency of Stripe customer IDs across all users
    /// Validates that each user's Stripe customer ID exists in Stripe and emails match
    /// Phase 3: Customer ID integrity validation
    /// </summary>
    Task<JsonModel> CheckCustomerIdConsistencyAsync(TokenModel tokenModel);
    
    // ===== PHASE 5: STRIPE SYNC DASHBOARD ENHANCEMENTS =====
    
    /// <summary>
    /// Get all sync discrepancies across plans, subscriptions, and customers
    /// Phase 5: Comprehensive discrepancy detection
    /// </summary>
    Task<JsonModel> GetAllDiscrepanciesAsync(TokenModel tokenModel);
    
    /// <summary>
    /// Bulk synchronize multiple entities
    /// Phase 5: Batch sync operations
    /// </summary>
    Task<JsonModel> BulkSyncAsync(BulkSyncRequestDto request, TokenModel tokenModel);
    
    /// <summary>
    /// Get synchronization history log
    /// Phase 5: Audit trail for sync operations
    /// </summary>
    Task<JsonModel> GetSyncHistoryAsync(int page, int pageSize, TokenModel tokenModel);
    
    /// <summary>
    /// Get webhook health status
    /// Phase 5: Monitor webhook processing
    /// </summary>
    Task<JsonModel> GetWebhookStatusAsync(TokenModel tokenModel);
}

/// <summary>
/// Result of Stripe synchronization validation
/// </summary>
public class StripeSyncValidationResult
{
    public bool IsSynchronized { get; set; }
    public List<string> Issues { get; set; } = new List<string>();
    public List<string> Recommendations { get; set; } = new List<string>();
    public DateTime LastSyncCheck { get; set; } = DateTime.UtcNow;
}

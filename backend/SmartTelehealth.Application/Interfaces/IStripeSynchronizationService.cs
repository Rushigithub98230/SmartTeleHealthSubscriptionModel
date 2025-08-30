using SmartTelehealth.Core.DTOs;

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

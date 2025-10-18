using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Core.Interfaces;

/// <summary>
/// Repository interface for subscription plan data access operations.
/// This interface defines all database operations related to subscription plans,
/// providing a clean abstraction for data access layer.
/// </summary>
public interface ISubscriptionPlanRepository : IRepositoryBase<SubscriptionPlan>
{
    #region Basic CRUD Operations
    
    
    
    // Custom methods with different names to avoid overriding base methods
    Task<SubscriptionPlan?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<SubscriptionPlan>> GetAllWithDetailsAsync();
    Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan);
    Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan plan);
    Task<bool> DeletePlanAsync(Guid id);
    Task<bool> ExistsPlanAsync(Guid id);
    
    #endregion
    
    #region Status Management Operations
    
    /// <summary>
    /// Activates a subscription plan
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to activate</param>
    /// <returns>True if activation was successful, false otherwise</returns>
    Task<bool> ActivateAsync(Guid id);
    
    /// <summary>
    /// Deactivates a subscription plan
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to deactivate</param>
    /// <returns>True if deactivation was successful, false otherwise</returns>
    Task<bool> DeactivateAsync(Guid id);
    
    #endregion
    
    #region Query Operations
    
    /// <summary>
    /// Retrieves subscription plans with comprehensive filtering using filter DTO
    /// This is the main method that consolidates all filtering capabilities
    /// </summary>
    /// <param name="filter">Comprehensive filter DTO containing all filter parameters</param>
    /// <returns>Tuple containing filtered and paginated subscription plans and total count</returns>
    Task<(IEnumerable<SubscriptionPlan> Plans, int TotalCount)> GetPlansWithAdvancedFilteringAsync(SubscriptionPlanFilterDto filter);
    
    #endregion
    
    #region Analytics and Reporting Operations
    
    /// <summary>
    /// Retrieves subscription plan statistics
    /// </summary>
    /// <returns>Object containing plan statistics</returns>
    Task<object> GetPlanStatisticsAsync();
    
    #endregion
    
    #region Validation Operations
    
    /// <summary>
    /// Checks if a subscription plan exists
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan</param>
    /// <returns>True if the plan exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Checks if a subscription plan name is unique
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if the name is unique, false otherwise</returns>
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    
    /// <summary>
    /// Checks if a subscription plan has active subscriptions
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan</param>
    /// <returns>True if the plan has active subscriptions, false otherwise</returns>
    Task<bool> HasActiveSubscriptionsAsync(Guid id);
    
    /// <summary>
    /// Gets all privileges associated with a subscription plan
    /// </summary>
    /// <param name="planId">The subscription plan ID</param>
    /// <returns>Collection of plan privileges</returns>
    Task<IEnumerable<SubscriptionPlanPrivilege>> GetPlanPrivilegesAsync(Guid planId);
    
    /// <summary>
    /// Gets a specific plan privilege configuration
    /// </summary>
    /// <param name="planId">The subscription plan ID</param>
    /// <param name="privilegeId">The privilege ID</param>
    /// <returns>Plan privilege configuration or null if not found</returns>
    Task<SubscriptionPlanPrivilege?> GetPlanPrivilegeAsync(Guid planId, Guid privilegeId);
    
    #endregion
    
    #region Plan Versioning Operations (Healthcare-Specific)
    
    /// <summary>
    /// Gets the latest version of a plan by its parent/original plan ID.
    /// Healthcare Feature: Enables plan versioning for price changes.
    /// </summary>
    /// <param name="planIdOrParentId">Plan ID or parent plan ID</param>
    /// <returns>Latest version of the plan or null if not found</returns>
    Task<SubscriptionPlan?> GetLatestVersionOfPlanAsync(Guid planIdOrParentId);
    
    /// <summary>
    /// Gets all versions of a plan (including parent).
    /// Healthcare Feature: View complete plan version history.
    /// </summary>
    /// <param name="planIdOrParentId">Plan ID or parent plan ID</param>
    /// <returns>Collection of all plan versions ordered by version number</returns>
    Task<IEnumerable<SubscriptionPlan>> GetAllVersionsOfPlanAsync(Guid planIdOrParentId);
    
    /// <summary>
    /// Creates a new version of an existing plan.
    /// Healthcare Feature: Marks previous versions as not latest and sets up new version.
    /// </summary>
    /// <param name="newVersion">The new plan version to create</param>
    /// <returns>Created plan version</returns>
    Task<SubscriptionPlan> CreateNewPlanVersionAsync(SubscriptionPlan newVersion);
    
    /// <summary>
    /// Gets count of active subscriptions for a plan.
    /// Healthcare Feature: Determine if plan version migration is needed.
    /// </summary>
    /// <param name="planId">The subscription plan ID</param>
    /// <returns>Number of active subscriptions</returns>
    Task<int> GetActiveSubscriptionsCountAsync(Guid planId);
    
    #endregion
} 
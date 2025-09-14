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
    
    // Basic CRUD methods are inherited from IRepositoryBase<SubscriptionPlan>
    
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
    
    #endregion
} 
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

/// <summary>
/// Repository interface for subscription plan data access operations.
/// This interface defines all database operations related to subscription plans,
/// providing a clean abstraction for data access layer.
/// </summary>
public interface ISubscriptionPlanRepository : IRepositoryBase<SubscriptionPlan>
{
    #region Basic CRUD Operations
    
    /// <summary>
    /// Retrieves a subscription plan by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan</param>
    /// <returns>SubscriptionPlan entity if found, null otherwise</returns>
    Task<SubscriptionPlan?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves all subscription plans
    /// </summary>
    /// <returns>Collection of all subscription plans</returns>
    Task<IEnumerable<SubscriptionPlan>> GetAllAsync();
    
    /// <summary>
    /// Creates a new subscription plan
    /// </summary>
    /// <param name="plan">The subscription plan entity to create</param>
    /// <returns>The created subscription plan entity</returns>
    Task<SubscriptionPlan> CreateAsync(SubscriptionPlan plan);
    
    /// <summary>
    /// Updates an existing subscription plan
    /// </summary>
    /// <param name="plan">The subscription plan entity to update</param>
    /// <returns>The updated subscription plan entity</returns>
    Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan plan);
    
    /// <summary>
    /// Deletes a subscription plan by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to delete</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteAsync(Guid id);
    
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
    /// Retrieves all active subscription plans
    /// </summary>
    /// <returns>Collection of active subscription plans</returns>
    Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync();
    
    /// <summary>
    /// Retrieves subscription plans by category
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category</param>
    /// <returns>Collection of subscription plans for the specified category</returns>
    Task<IEnumerable<SubscriptionPlan>> GetPlansByCategoryAsync(Guid categoryId);
    
    /// <summary>
    /// Searches subscription plans by name or description
    /// </summary>
    /// <param name="searchTerm">The search term to match against plan name or description</param>
    /// <returns>Collection of matching subscription plans</returns>
    Task<IEnumerable<SubscriptionPlan>> SearchPlansAsync(string searchTerm);
    
    /// <summary>
    /// Retrieves subscription plans with comprehensive filtering, pagination, and sorting
    /// </summary>
    /// <param name="page">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Search term for filtering plans</param>
    /// <param name="categoryId">Category ID for filtering plans</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="sortColumn">Column name for sorting</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>Tuple containing filtered and paginated subscription plans and total count</returns>
    Task<(IEnumerable<SubscriptionPlan> Plans, int TotalCount)> GetPlansWithPaginationAsync(
        int page = 1, 
        int pageSize = 50, 
        string? searchTerm = null, 
        string? categoryId = null, 
        bool? isActive = null,
        string? sortColumn = "DisplayOrder",
        string? sortOrder = "asc");
    
    #endregion
    
    #region Analytics and Reporting Operations
    
    /// <summary>
    /// Retrieves subscription plan statistics
    /// </summary>
    /// <returns>Object containing plan statistics</returns>
    Task<object> GetPlanStatisticsAsync();
    
    /// <summary>
    /// Retrieves subscription plans created within a date range
    /// </summary>
    /// <param name="startDate">Start date of the range</param>
    /// <param name="endDate">End date of the range</param>
    /// <returns>Collection of subscription plans created within the date range</returns>
    Task<IEnumerable<SubscriptionPlan>> GetPlansByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Retrieves subscription plans by billing cycle
    /// </summary>
    /// <param name="billingCycleId">The unique identifier of the billing cycle</param>
    /// <returns>Collection of subscription plans for the specified billing cycle</returns>
    Task<IEnumerable<SubscriptionPlan>> GetPlansByBillingCycleAsync(Guid billingCycleId);
    
    /// <summary>
    /// Retrieves subscription plans by currency
    /// </summary>
    /// <param name="currencyId">The unique identifier of the currency</param>
    /// <returns>Collection of subscription plans for the specified currency</returns>
    Task<IEnumerable<SubscriptionPlan>> GetPlansByCurrencyAsync(Guid currencyId);
    
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
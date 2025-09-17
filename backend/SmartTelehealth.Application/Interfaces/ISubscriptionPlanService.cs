using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service interface for managing subscription plans including CRUD operations,
/// plan administration, analytics, and privilege management.
/// This service handles all subscription plan-related functionality that was
/// previously managed by the SubscriptionService, following the Single Responsibility Principle.
/// </summary>
public interface ISubscriptionPlanService
{
    #region Core Plan Management
    
    /// <summary>
    /// Retrieves a specific subscription plan by its unique identifier
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the subscription plan data or error information</returns>
    Task<JsonModel> GetPlanByIdAsync(string planId, TokenModel tokenModel);
    
    /// <summary>
    /// Retrieves subscription plans with comprehensive filtering using filter DTO
    /// This is the main method that consolidates all filtering capabilities
    /// </summary>
    /// <param name="filter">Comprehensive filter DTO containing all filter parameters</param>
    /// <param name="tokenModel">Token model for authentication (optional for public access)</param>
    /// <param name="adminOnly">Whether to require admin access</param>
    /// <returns>JsonModel containing filtered, paginated, and sorted subscription plans with metadata</returns>
    Task<JsonModel> GetSubscriptionPlansWithFilteringAsync(SubscriptionPlanFilterDto filter, TokenModel? tokenModel = null, bool adminOnly = false);
    
    
    /// <summary>
    /// Creates a new subscription plan
    /// </summary>
    /// <param name="createDto">DTO containing subscription plan creation details</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the created subscription plan or error information</returns>
    Task<JsonModel> CreatePlanAsync(CreateSubscriptionPlanDto createDto, TokenModel tokenModel);
    
    
    /// <summary>
    /// Activates a subscription plan
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to activate</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the activation result</returns>
    Task<JsonModel> ActivatePlanAsync(string planId, TokenModel tokenModel);
    
    
    #endregion
    
    #region Plan Search and Filtering
    
    // All search and filtering functionality is now consolidated into GetSubscriptionPlansWithFilteringAsync
    
    #endregion
    
    #region Plan Analytics and Reporting
    
    
    /// <summary>
    /// Exports subscription plans to specified format
    /// </summary>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <param name="searchTerm">Search term for filtering export data</param>
    /// <param name="categoryId">Category ID for filtering export data</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="format">Export format (csv, json, excel)</param>
    /// <returns>JsonModel containing export result or file data</returns>
    Task<JsonModel> ExportSubscriptionPlansAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null, string format = "csv");
    
    #endregion
    
    #region Plan Privilege Management
    
    /// <summary>
    /// Assigns privileges to a subscription plan
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="privileges">List of privileges to assign to the plan</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the assignment result</returns>
    Task<JsonModel> AssignPrivilegesToPlanAsync(Guid planId, List<PlanPrivilegeDto> privileges, TokenModel tokenModel);
    
    /// <summary>
    /// Removes a privilege from a subscription plan
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="privilegeId">The unique identifier of the privilege to remove</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the removal result</returns>
    Task<JsonModel> RemovePrivilegeFromPlanAsync(Guid planId, Guid privilegeId, TokenModel tokenModel);
    
    /// <summary>
    /// Updates a privilege assignment for a subscription plan
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="privilegeId">The unique identifier of the privilege to update</param>
    /// <param name="privilegeDto">DTO containing updated privilege information</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the update result</returns>
    Task<JsonModel> UpdatePlanPrivilegeAsync(Guid planId, Guid privilegeId, PlanPrivilegeDto privilegeDto, TokenModel tokenModel);
    
    /// <summary>
    /// Retrieves all privileges assigned to a subscription plan
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the plan privileges</returns>
    Task<JsonModel> GetPlanPrivilegesAsync(Guid planId, TokenModel tokenModel);
    
    #endregion
    
    #region Additional Plan Methods (for backward compatibility)
    
    
    /// <summary>
    /// Updates a subscription plan with comprehensive validation (for backward compatibility)
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to update</param>
    /// <param name="updateDto">DTO containing subscription plan update details</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the updated subscription plan or error information</returns>
    Task<JsonModel> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updateDto, TokenModel tokenModel);
    
    /// <summary>
    /// Deactivates a subscription plan (soft delete) - RECOMMENDED APPROACH
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to deactivate</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the deactivation result</returns>
    Task<JsonModel> DeactivatePlanAsync(string planId, TokenModel tokenModel);
    
    /// <summary>
    /// Reactivates a deactivated subscription plan
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to reactivate</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the reactivation result</returns>
    Task<JsonModel> ReactivatePlanAsync(string planId, TokenModel tokenModel);
    
    /// <summary>
    /// Deletes a subscription plan (DEPRECATED - Use DeactivatePlanAsync instead)
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to delete</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the deletion result</returns>
    [Obsolete("Use DeactivatePlanAsync instead for better data integrity and business continuity")]
    Task<JsonModel> DeletePlanAsync(string planId, TokenModel tokenModel);
    
    
    #endregion
}

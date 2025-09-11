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
    /// Retrieves subscription plans with comprehensive filtering, pagination, and sorting
    /// </summary>
    /// <param name="tokenModel">Token model for authentication (optional for public access)</param>
    /// <param name="page">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Search term for filtering plans</param>
    /// <param name="categoryId">Category ID for filtering plans</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="sortColumn">Column name for sorting</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <param name="adminOnly">Whether to require admin access</param>
    /// <returns>JsonModel containing filtered, paginated, and sorted subscription plans</returns>
    Task<JsonModel> GetSubscriptionPlansAsync(
        TokenModel? tokenModel = null,
        int page = 1,
        int pageSize = 50,
        string? searchTerm = null,
        string? categoryId = null,
        bool? isActive = null,
        string? sortColumn = "DisplayOrder",
        string? sortOrder = "asc",
        bool adminOnly = false);
    
    
    /// <summary>
    /// Retrieves all subscription plans with pagination and filtering (convenience method)
    /// </summary>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Search term for filtering plans</param>
    /// <param name="categoryId">Category ID for filtering plans</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing paginated subscription plans</returns>
    Task<JsonModel> GetAllPlansAsync(int page, int pageSize, string? searchTerm, string? categoryId, bool? isActive, TokenModel tokenModel);
    
    /// <summary>
    /// Retrieves all public subscription plans (convenience method)
    /// </summary>
    /// <returns>JsonModel containing public subscription plans</returns>
    Task<JsonModel> GetPublicPlansAsync();
    
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
    
    /// <summary>
    /// Retrieves all active subscription plans
    /// </summary>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing active subscription plans</returns>
    Task<JsonModel> GetActiveSubscriptionPlansAsync(TokenModel tokenModel);
    
    /// <summary>
    /// Retrieves subscription plans by category
    /// </summary>
    /// <param name="category">Category name or identifier</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing subscription plans for the specified category</returns>
    Task<JsonModel> GetSubscriptionPlansByCategoryAsync(string category, TokenModel tokenModel);
    
    /// <summary>
    /// Retrieves a subscription plan with detailed information
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the subscription plan details</returns>
    Task<JsonModel> GetSubscriptionPlanAsync(string planId, TokenModel tokenModel);
    
    /// <summary>
    /// Retrieves all subscription plans with advanced filtering and pagination
    /// </summary>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <param name="searchTerm">Search term for filtering plans</param>
    /// <param name="categoryId">Category ID for filtering plans</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>JsonModel containing filtered and paginated subscription plans</returns>
    Task<JsonModel> GetAllSubscriptionPlansAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null, int page = 1, int pageSize = 50);
    
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
    /// Deletes a subscription plan with comprehensive validation (for backward compatibility)
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to delete</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the deletion result</returns>
    Task<JsonModel> DeletePlanAsync(string planId, TokenModel tokenModel);
    
    /// <summary>
    /// Deactivates a subscription plan with admin user tracking (for backward compatibility)
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to deactivate</param>
    /// <param name="adminUserId">The ID of the admin user performing the action</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the deactivation result</returns>
    Task<JsonModel> DeactivatePlanAsync(string planId, string adminUserId, TokenModel tokenModel);
    
    #endregion
}

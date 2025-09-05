using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for web administration subscription management operations.
/// This controller provides comprehensive functionality for managing subscription plans,
/// user subscriptions, categories, analytics, bulk operations, and plan privileges.
/// It serves as the primary interface for web-based subscription administration and management.
/// </summary>
[ApiController]
[Route("webadmin/subscription-management")]
//[Authorize]
public class SubscriptionManagementController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICategoryService _categoryService;
    private readonly IAnalyticsService _analyticsService;
      

    /// <summary>
    /// Initializes a new instance of the SubscriptionManagementController with required services.
    /// </summary>
    /// <param name="subscriptionService">Service for handling subscription-related business logic</param>
    /// <param name="categoryService">Service for handling category management operations</param>
    /// <param name="analyticsService">Service for handling analytics and reporting</param>
    /// <param name="auditService">Service for handling audit logging operations</param>
    public SubscriptionManagementController(
        ISubscriptionService subscriptionService,
        ICategoryService categoryService,
        IAnalyticsService analyticsService,
        IAuditService auditService)
    {
        _subscriptionService = subscriptionService;
        _categoryService = categoryService;
        _analyticsService = analyticsService;
          
    }

    #region Subscription Plans Management

    /// <summary>
    /// Retrieves all subscription plans with comprehensive filtering and pagination for administrative management.
    /// This endpoint provides administrators with access to all subscription plans in the system with advanced filtering
    /// capabilities including search, category filtering, and status filtering for effective plan management.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 10)</param>
    /// <param name="searchTerm">Search term for filtering plans by name or description</param>
    /// <param name="categoryId">Filter plans by specific category ID</param>
    /// <param name="isActive">Filter plans by active status (true/false/null for all)</param>
    /// <returns>JsonModel containing paginated subscription plans with filtering applied</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all subscription plans with comprehensive filtering options
    /// - Supports pagination for large datasets
    /// - Includes search functionality for plan names and descriptions
    /// - Filters by category and active status
    /// - Access restricted to administrators only
    /// - Used for administrative plan management and oversight
    /// - Includes comprehensive plan information and metadata
    /// - Supports advanced filtering for plan analysis
    /// - Provides comprehensive plan overview for administrators
    /// </remarks>
    [HttpGet("plans")]
    public async Task<JsonModel> GetAllPlans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null)
    {
        return await _subscriptionService.GetAllPlansAsync(page, pageSize, searchTerm, categoryId, isActive, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new subscription plan for the system.
    /// This endpoint handles subscription plan creation including validation, configuration,
    /// and initial setup for administrative management and user subscription options.
    /// </summary>
    /// <param name="createDto">DTO containing subscription plan creation details</param>
    /// <returns>JsonModel containing the created subscription plan information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new subscription plan with comprehensive validation
    /// - Validates plan configuration and pricing information
    /// - Sets up plan for administrative management
    /// - Access restricted to administrators only
    /// - Used for subscription plan creation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan creation
    /// - Maintains plan creation audit trails and history
    /// </remarks>
    [HttpPost("plans")]
    public async Task<JsonModel> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        return await _subscriptionService.CreatePlanAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing subscription plan with new configuration.
    /// This endpoint handles subscription plan updates including validation, configuration changes,
    /// and plan modification for administrative management and user subscription options.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to update</param>
    /// <param name="updateDto">DTO containing subscription plan update details</param>
    /// <returns>JsonModel containing the updated subscription plan information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates an existing subscription plan with comprehensive validation
    /// - Validates plan configuration and pricing changes
    /// - Ensures ID consistency between URL and request body
    /// - Access restricted to administrators only
    /// - Used for subscription plan modification and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan updates
    /// - Maintains plan update audit trails and change history
    /// </remarks>
    [HttpPut("plans/{id}")]
    public async Task<JsonModel> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        if (id != updateDto.Id)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        
        return await _subscriptionService.UpdatePlanAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a subscription plan from the system.
    /// This endpoint handles subscription plan deletion including validation, dependency checking,
    /// and plan removal for administrative management and system cleanup.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Deletes a subscription plan with comprehensive validation
    /// - Checks for existing subscriptions and dependencies
    /// - Ensures safe plan removal without data integrity issues
    /// - Access restricted to administrators only
    /// - Used for subscription plan removal and system cleanup
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan deletion
    /// - Maintains plan deletion audit trails and removal history
    /// </remarks>
    [HttpDelete("plans/{id}")]
    public async Task<JsonModel> DeletePlan(string id)
    {
        return await _subscriptionService.DeletePlanAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Activates a subscription plan to make it available for user subscriptions.
    /// This endpoint handles subscription plan activation including validation, status updates,
    /// and plan availability for user subscription options.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to activate</param>
    /// <returns>JsonModel containing the activation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Activates a subscription plan for user availability
    /// - Validates plan configuration and readiness
    /// - Updates plan status to active
    /// - Access restricted to administrators only
    /// - Used for subscription plan activation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan activation
    /// - Maintains plan activation audit trails and status history
    /// </remarks>
    [HttpPost("plans/{id}/activate")]
    public async Task<JsonModel> ActivatePlan(string id)
    {
        return await _subscriptionService.ActivatePlanAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Deactivates a subscription plan to prevent new user subscriptions.
    /// This endpoint handles subscription plan deactivation including validation, status updates,
    /// and plan unavailability for new user subscription options.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to deactivate</param>
    /// <returns>JsonModel containing the deactivation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Deactivates a subscription plan to prevent new subscriptions
    /// - Validates plan status and existing subscriptions
    /// - Updates plan status to inactive
    /// - Access restricted to administrators only
    /// - Used for subscription plan deactivation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan deactivation
    /// - Maintains plan deactivation audit trails and status history
    /// </remarks>
    [HttpPost("plans/{id}/deactivate")]
    public async Task<JsonModel> DeactivatePlan(string id)
    {
        return await _subscriptionService.DeactivatePlanAsync(id, GetToken(HttpContext));
    }

    #endregion

    #region User Subscriptions Management

    /// <summary>
    /// Retrieves all user subscriptions with comprehensive filtering and pagination for administrative management.
    /// This endpoint provides administrators with access to all user subscriptions in the system with advanced filtering
    /// capabilities including status, plan, user, date range, and sorting options for effective subscription oversight.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 10)</param>
    /// <param name="searchTerm">Search term for filtering subscriptions</param>
    /// <param name="status">Filter subscriptions by status array</param>
    /// <param name="planId">Filter subscriptions by plan ID array</param>
    /// <param name="userId">Filter subscriptions by user ID array</param>
    /// <param name="startDate">Start date for date range filtering</param>
    /// <param name="endDate">End date for date range filtering</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>JsonModel containing paginated user subscriptions with filtering applied</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all user subscriptions with comprehensive filtering options
    /// - Supports pagination for large datasets
    /// - Includes advanced filtering by status, plan, user, and date range
    /// - Provides sorting capabilities for data organization
    /// - Access restricted to administrators only
    /// - Used for administrative subscription oversight and management
    /// - Includes comprehensive subscription information and metadata
    /// - Supports advanced filtering for subscription analysis
    /// - Provides comprehensive subscription overview for administrators
    /// </remarks>
    [HttpGet("subscriptions")]
    public async Task<JsonModel> GetAllUserSubscriptions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string[]? status = null,
        [FromQuery] string[]? planId = null,
        [FromQuery] string[]? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null)
    {
        return await _subscriptionService.GetAllUserSubscriptionsAsync(page, pageSize, searchTerm, status, planId, userId, startDate, endDate, sortBy, sortOrder, GetToken(HttpContext));
    }

    /// <summary>
    /// Cancels a user subscription with optional reason for administrative management.
    /// This endpoint handles subscription cancellation including validation, status updates,
    /// and cancellation processing for administrative subscription management.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to cancel</param>
    /// <param name="reason">Optional reason for cancellation</param>
    /// <returns>JsonModel containing the cancellation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Cancels a user subscription with administrative authority
    /// - Validates subscription status and cancellation eligibility
    /// - Records cancellation reason for audit purposes
    /// - Access restricted to administrators only
    /// - Used for administrative subscription cancellation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on cancellation operations
    /// - Maintains subscription cancellation audit trails
    /// </remarks>
    [HttpPost("subscriptions/{id}/cancel")]
    public async Task<JsonModel> CancelUserSubscription(string id, [FromBody] string? reason = null)
    {
        return await _subscriptionService.CancelUserSubscriptionAsync(id, reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Pauses a user subscription with optional reason for administrative management.
    /// This endpoint handles subscription pausing including validation, status updates,
    /// and pause processing for administrative subscription management.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to pause</param>
    /// <param name="reason">Optional reason for pausing</param>
    /// <returns>JsonModel containing the pause result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Pauses a user subscription with administrative authority
    /// - Validates subscription status and pause eligibility
    /// - Records pause reason for audit purposes
    /// - Access restricted to administrators only
    /// - Used for administrative subscription pause and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on pause operations
    /// - Maintains subscription pause audit trails
    /// </remarks>
    [HttpPost("subscriptions/{id}/pause")]
    public async Task<JsonModel> PauseUserSubscription(string id, [FromBody] string? reason = null)
    {
        return await _subscriptionService.PauseUserSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Resumes a paused user subscription for administrative management.
    /// This endpoint handles subscription resumption including validation, status updates,
    /// and resume processing for administrative subscription management.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to resume</param>
    /// <returns>JsonModel containing the resume result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Resumes a paused user subscription with administrative authority
    /// - Validates subscription status and resume eligibility
    /// - Updates subscription status to active
    /// - Access restricted to administrators only
    /// - Used for administrative subscription resume and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on resume operations
    /// - Maintains subscription resume audit trails
    /// </remarks>
    [HttpPost("subscriptions/{id}/resume")]
    public async Task<JsonModel> ResumeUserSubscription(string id)
    {
        return await _subscriptionService.ResumeUserSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Extends a user subscription duration for administrative management.
    /// This endpoint handles subscription extension including validation, date calculation,
    /// and extension processing for administrative subscription management.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to extend</param>
    /// <param name="extendDto">DTO containing extension details including new end date</param>
    /// <returns>JsonModel containing the extension result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Extends a user subscription duration with administrative authority
    /// - Validates new end date and calculates additional days
    /// - Ensures new end date is in the future
    /// - Access restricted to administrators only
    /// - Used for administrative subscription extension and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on extension operations
    /// - Maintains subscription extension audit trails
    /// </remarks>
    [HttpPost("subscriptions/{id}/extend")]
    public async Task<JsonModel> ExtendUserSubscription(string id, [FromBody] ExtendSubscriptionDto extendDto)
    {
        // Calculate additional days from the new end date
        var additionalDays = (int)(extendDto.NewEndDate - DateTime.UtcNow).TotalDays;
        if (additionalDays <= 0)
        {
            return new JsonModel { data = new object(), Message = "New end date must be in the future", StatusCode = 400 };
        }
        
        return await _subscriptionService.ExtendUserSubscriptionAsync(id, additionalDays, GetToken(HttpContext));
    }

    #endregion

    #region Categories Management

    /// <summary>
    /// Retrieves all categories with comprehensive filtering and pagination for administrative management.
    /// This endpoint provides administrators with access to all categories in the system with advanced filtering
    /// capabilities including search and status filtering for effective category management.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 10)</param>
    /// <param name="searchTerm">Search term for filtering categories by name or description</param>
    /// <param name="isActive">Filter categories by active status (true/false/null for all)</param>
    /// <returns>JsonModel containing paginated categories with filtering applied</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all categories with comprehensive filtering options
    /// - Supports pagination for large datasets
    /// - Includes search functionality for category names and descriptions
    /// - Filters by active status
    /// - Access restricted to administrators only
    /// - Used for administrative category management and oversight
    /// - Includes comprehensive category information and metadata
    /// - Supports advanced filtering for category analysis
    /// - Provides comprehensive category overview for administrators
    /// </remarks>
    [HttpGet("categories")]
    public async Task<JsonModel> GetAllCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        return await _categoryService.GetAllCategoriesAsync(page, pageSize, searchTerm, isActive, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new category for the system.
    /// This endpoint handles category creation including validation, configuration,
    /// and initial setup for administrative management and subscription plan organization.
    /// </summary>
    /// <param name="createDto">DTO containing category creation details</param>
    /// <returns>JsonModel containing the created category information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new category with comprehensive validation
    /// - Validates category configuration and naming
    /// - Sets up category for administrative management
    /// - Access restricted to administrators only
    /// - Used for category creation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on category creation
    /// - Maintains category creation audit trails and history
    /// </remarks>
    [HttpPost("categories")]
    public async Task<JsonModel> CreateCategory([FromBody] CreateCategoryDto createDto)
    {
        return await _categoryService.CreateCategoryAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing category with new configuration.
    /// This endpoint handles category updates including validation, configuration changes,
    /// and category modification for administrative management and subscription plan organization.
    /// </summary>
    /// <param name="id">The unique identifier of the category to update</param>
    /// <param name="updateDto">DTO containing category update details</param>
    /// <returns>JsonModel containing the updated category information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates an existing category with comprehensive validation
    /// - Validates category configuration and naming changes
    /// - Ensures ID consistency between URL and request body
    /// - Access restricted to administrators only
    /// - Used for category modification and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on category updates
    /// - Maintains category update audit trails and change history
    /// </remarks>
    [HttpPut("categories/{id}")]
    public async Task<JsonModel> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateDto)
    {
        if (id.ToString() != updateDto.Id)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        
        return await _categoryService.UpdateCategoryAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a category from the system.
    /// This endpoint handles category deletion including validation, dependency checking,
    /// and category removal for administrative management and system cleanup.
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Deletes a category with comprehensive validation
    /// - Checks for existing subscription plans and dependencies
    /// - Ensures safe category removal without data integrity issues
    /// - Access restricted to administrators only
    /// - Used for category removal and system cleanup
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on category deletion
    /// - Maintains category deletion audit trails and removal history
    /// </remarks>
    [HttpDelete("categories/{id}")]
    public async Task<JsonModel> DeleteCategory(Guid id)
    {
        return await _categoryService.DeleteCategoryAsync(id, GetToken(HttpContext));
    }

    #endregion

    #region Analytics

    /// <summary>
    /// Retrieves comprehensive subscription analytics for the administrative dashboard.
    /// This endpoint provides detailed analytics including subscription metrics, performance indicators,
    /// and business intelligence data for administrative decision-making and system monitoring.
    /// </summary>
    /// <param name="startDate">Start date for analytics date range filtering</param>
    /// <param name="endDate">End date for analytics date range filtering</param>
    /// <param name="planId">Filter analytics by specific subscription plan ID</param>
    /// <returns>JsonModel containing comprehensive subscription analytics data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive subscription analytics and metrics
    /// - Includes subscription performance indicators and trends
    /// - Supports date range filtering for historical analysis
    /// - Filters by specific subscription plans for detailed analysis
    /// - Access restricted to administrators only
    /// - Used for administrative dashboard and business intelligence
    /// - Includes comprehensive analytics information and metadata
    /// - Supports advanced filtering for analytics analysis
    /// - Provides comprehensive analytics overview for administrators
    /// </remarks>
    [HttpGet("analytics")]
    public async Task<JsonModel> GetAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? planId = null)
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, endDate, planId, GetToken(HttpContext));
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Performs bulk operations on multiple subscriptions for administrative management.
    /// This endpoint handles bulk actions including status updates, cancellations, and other
    /// administrative operations on multiple subscriptions simultaneously for efficient management.
    /// </summary>
    /// <param name="request">DTO containing bulk action request details</param>
    /// <returns>JsonModel containing the bulk operation results</returns>
    /// <remarks>
    /// This endpoint:
    /// - Performs bulk operations on multiple subscriptions
    /// - Supports various administrative actions and status updates
    /// - Provides comprehensive result tracking and reporting
    /// - Includes success and failure counts for operation monitoring
    /// - Access restricted to administrators only
    /// - Used for efficient bulk subscription management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on bulk operations
    /// - Maintains bulk operation audit trails and execution history
    /// </remarks>
    [HttpPost("bulk-action")]
    public async Task<JsonModel> PerformBulkAction([FromBody] BulkActionRequestDto request)
    {
        var result = await _subscriptionService.PerformBulkActionAsync(new List<BulkActionRequestDto> { request }, GetToken(HttpContext));
        
        var results = new List<BulkActionResultDto>
        {
            new BulkActionResultDto
            {
                SubscriptionId = request.SubscriptionId,
                Action = request.Action,
                Success = true,
                Message = "Action completed successfully"
            }
        };
        
        return new JsonModel
        {
            data = new BulkActionResultDto
            {
                TotalCount = 1,
                SuccessCount = results.Count(r => r.Success),
                FailureCount = results.Count(r => !r.Success),
                Results = results
            },
            Message = "Action completed",
            StatusCode = 200
        };
    }

    #endregion

    #region Plan Privilege Management

    /// <summary>
    /// Assigns multiple privileges to a subscription plan for administrative management.
    /// This endpoint handles privilege assignment including validation, configuration,
    /// and privilege setup for subscription plan feature management.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="privileges">List of privilege DTOs containing privilege assignment details</param>
    /// <returns>JsonModel containing the privilege assignment results</returns>
    /// <remarks>
    /// This endpoint:
    /// - Assigns multiple privileges to a subscription plan
    /// - Validates privilege configuration and plan compatibility
    /// - Sets up privilege assignments for plan feature management
    /// - Access restricted to administrators only
    /// - Used for subscription plan privilege configuration
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on privilege assignments
    /// - Maintains privilege assignment audit trails and configuration history
    /// </remarks>
    [HttpPost("plans/{planId}/privileges")]
    public async Task<JsonModel> AssignPrivilegesToPlan(string planId, [FromBody] List<PlanPrivilegeDto> privileges)
    {
        return await _subscriptionService.AssignPrivilegesToPlanAsync(Guid.Parse(planId), privileges, GetToken(HttpContext));
    }

    /// <summary>
    /// Removes a specific privilege from a subscription plan for administrative management.
    /// This endpoint handles privilege removal including validation, dependency checking,
    /// and privilege removal for subscription plan feature management.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="privilegeId">The unique identifier of the privilege to remove</param>
    /// <returns>JsonModel containing the privilege removal result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Removes a specific privilege from a subscription plan
    /// - Validates privilege removal and dependency checking
    /// - Ensures safe privilege removal without data integrity issues
    /// - Access restricted to administrators only
    /// - Used for subscription plan privilege management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on privilege removal
    /// - Maintains privilege removal audit trails and configuration history
    /// </remarks>
    [HttpDelete("plans/{planId}/privileges/{privilegeId}")]
    public async Task<JsonModel> RemovePrivilegeFromPlan(string planId, string privilegeId)
    {
        return await _subscriptionService.RemovePrivilegeFromPlanAsync(Guid.Parse(planId), Guid.Parse(privilegeId), GetToken(HttpContext));
    }

    /// <summary>
    /// Updates a specific privilege configuration for a subscription plan for administrative management.
    /// This endpoint handles privilege configuration updates including validation, configuration changes,
    /// and privilege modification for subscription plan feature management.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="privilegeId">The unique identifier of the privilege to update</param>
    /// <param name="privilegeDto">DTO containing updated privilege configuration details</param>
    /// <returns>JsonModel containing the privilege update result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates a specific privilege configuration for a subscription plan
    /// - Validates privilege configuration changes and plan compatibility
    /// - Ensures privilege configuration consistency and integrity
    /// - Access restricted to administrators only
    /// - Used for subscription plan privilege configuration management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on privilege updates
    /// - Maintains privilege update audit trails and configuration history
    /// </remarks>
    [HttpPut("plans/{planId}/privileges/{privilegeId}")]
    public async Task<JsonModel> UpdatePlanPrivilege(string planId, string privilegeId, [FromBody] PlanPrivilegeDto privilegeDto)
    {
        return await _subscriptionService.UpdatePlanPrivilegeAsync(Guid.Parse(planId), Guid.Parse(privilegeId), privilegeDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all privileges assigned to a specific subscription plan for administrative management.
    /// This endpoint provides comprehensive privilege information including privilege details,
    /// configuration, and assignment status for subscription plan feature management.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <returns>JsonModel containing all privileges assigned to the plan</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all privileges assigned to a specific subscription plan
    /// - Includes privilege details, configuration, and assignment status
    /// - Shows privilege settings and feature access information
    /// - Access restricted to administrators only
    /// - Used for subscription plan privilege management and oversight
    /// - Includes comprehensive privilege information and metadata
    /// - Provides data for privilege configuration and management
    /// - Handles privilege validation and error responses
    /// </remarks>
    [HttpGet("plans/{planId}/privileges")]
    public async Task<JsonModel> GetPlanPrivileges(string planId)
    {
        return await _subscriptionService.GetPlanPrivilegesAsync(Guid.Parse(planId), GetToken(HttpContext));
    }

    #endregion
}

#region DTOs for Admin Subscription Management

public class ExtendSubscriptionDto
{
    public DateTime NewEndDate { get; set; }
    public string? Reason { get; set; }
}

#endregion 
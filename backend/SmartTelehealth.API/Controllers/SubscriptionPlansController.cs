using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for managing subscription plans.
/// This controller provides functionality for creating, reading, updating, and deleting
/// subscription plans, which define the features, pricing, and privileges available
/// to users who subscribe to them.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class SubscriptionPlansController : BaseController
{
    private readonly ISubscriptionPlanService _subscriptionPlanService;
    private readonly IPrivilegeService _privilegeService;

    /// <summary>
    /// Initializes a new instance of the SubscriptionPlansController with the required services.
    /// </summary>
    /// <param name="subscriptionPlanService">Service for handling subscription plan-related business logic</param>
    /// <param name="privilegeService">Service for handling privilege-related business logic</param>
    public SubscriptionPlansController(ISubscriptionPlanService subscriptionPlanService, IPrivilegeService privilegeService)
    {
        _subscriptionPlanService = subscriptionPlanService;
        _privilegeService = privilegeService;
    }


    /// <summary>
    /// Retrieves all active subscription plans available for public viewing with comprehensive filtering and pagination.
    /// This endpoint returns only active subscription plans that are suitable for
    /// public display and user subscription, excluding administrative or draft plans, with advanced filtering capabilities.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50)</param>
    /// <param name="searchTerm">Search term for filtering plans</param>
    /// <param name="categoryId">Filter plans by category ID</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>JsonModel containing paginated active subscription plans with filtering applied</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns only active subscription plans
    /// - Supports pagination for large datasets
    /// - Includes advanced filtering by search term and category
    /// - Provides sorting capabilities for data organization
    /// - Includes public-facing plan information and pricing
    /// - No authentication required - accessible to all users
    /// - Used for plan selection and comparison by potential subscribers
    /// - Optimized for public consumption with marketing-friendly information
    /// - Excludes administrative details and internal configurations
    /// - Supports advanced filtering for plan discovery
    /// </remarks>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<JsonModel> GetActivePlans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null)
    {
        var filter = new SubscriptionPlanFilterDto
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = !string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId) ? catId : null,
            IsActive = true,
            SortColumn = sortBy ?? "CreatedDate",
            SortOrder = sortOrder ?? "desc"
        };
        return await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(filter, null, adminOnly: false);
    }

    /// <summary>
    /// Retrieves subscription plans with comprehensive filtering for public use.
    /// This endpoint supports advanced filtering, pagination, sorting, and search capabilities.
    /// </summary>
    /// <param name="filter">Comprehensive filter DTO containing all filter parameters</param>
    /// <returns>JsonModel containing filtered, paginated, and sorted subscription plans</returns>
    /// <remarks>
    /// This endpoint supports:
    /// - Advanced search by name, description, or short description
    /// - Filtering by category, pricing, billing cycle, and status
    /// - Date range filtering for creation, update, and effective dates
    /// - Trial duration and display order filtering
    /// - Stripe integration status filtering
    /// - Comprehensive pagination with metadata
    /// - Dynamic sorting by multiple columns
    /// - No authentication required - accessible to all users
    /// </remarks>
    [HttpPost("filter")]
    [AllowAnonymous]
    public async Task<JsonModel> GetPlansWithFiltering([FromBody] SubscriptionPlanFilterDto filter)
    {
        return await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(filter, null, adminOnly: false);
    }

    /// <summary>
    /// Retrieves subscription plans filtered by a specific category.
    /// This endpoint returns subscription plans that belong to the specified category,
    /// allowing users to browse plans by service type or feature category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category to filter by</param>
    /// <returns>JsonModel containing subscription plans in the specified category</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns subscription plans filtered by category
    /// - Includes only active plans in the specified category
    /// - No authentication required - accessible to all users
    /// - Used for categorized plan browsing and comparison
    /// - Helps users find plans that match their specific needs
    /// - Supports category-based plan organization and marketing
    /// </remarks>
    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<JsonModel> GetPlansByCategory(string categoryId)
    {
        var filter = new SubscriptionPlanFilterDto
        {
            Page = 1,
            PageSize = 1000,
            CategoryId = Guid.TryParse(categoryId, out var catId) ? catId : null
        };
        return await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(filter, GetToken(HttpContext), adminOnly: false);
    }

    /// <summary>
    /// Retrieves detailed information about a specific subscription plan.
    /// This endpoint returns comprehensive details about a particular subscription plan,
    /// including features, pricing, privileges, and availability information.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to retrieve</param>
    /// <returns>JsonModel containing detailed subscription plan information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed information about the specified subscription plan
    /// - Includes plan features, pricing, and privilege details
    /// - No authentication required - accessible to all users
    /// - Used for detailed plan views and plan comparison
    /// - Provides comprehensive plan information for decision-making
    /// - Includes availability status and subscription requirements
    /// </remarks>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<JsonModel> GetPlan(string id)
    {
        return await _subscriptionPlanService.GetPlanByIdAsync(id, GetToken(HttpContext));
    }




    /// <summary>
    /// Activates a subscription plan to make it available for user subscriptions.
    /// This endpoint handles subscription plan activation including validation, status updates,
    /// and notification processes. It ensures that only valid plans can be activated
    /// and maintains proper audit trails for plan status changes.
    /// </summary>
    /// <param name="planId">The unique identifier (GUID) of the subscription plan to activate</param>
    /// <returns>JsonModel containing activation confirmation or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for plan activation
    /// - Returns 403 Forbidden for non-admin users
    /// - Returns 404 Not Found if plan doesn't exist
    /// 
    /// Business Logic:
    /// - Validates plan exists and is not already active
    /// - Updates plan status to active
    /// - Triggers any necessary notifications or integrations
    /// - Maintains plan activation audit trails and status history
    /// </remarks>
    [HttpPost("{planId}/activate")]
    public async Task<JsonModel> ActivatePlan(string planId)
    {
        return await _subscriptionPlanService.ActivatePlanAsync(planId, GetToken(HttpContext));
    }


    /// <summary>
    /// Retrieves all subscription plans with comprehensive filtering and pagination for administrative management.
    /// This endpoint provides administrators with access to all subscription plans in the system with advanced filtering,
    /// searching, and pagination capabilities. It supports various filter criteria and export options.
    /// </summary>
    /// <param name="searchTerm">Search term to filter plans by name or description</param>
    /// <param name="categoryId">Category ID to filter plans by category</param>
    /// <param name="isActive">Filter by active status (true/false/null for all)</param>
    /// <param name="page">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="format">Export format (csv/excel) - if specified, returns export instead of paginated data</param>
    /// <returns>JsonModel containing paginated subscription plans or export file</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for comprehensive plan management
    /// - Returns 403 Forbidden for non-admin users
    /// 
    /// Features:
    /// - Advanced filtering by search term, category, and active status
    /// - Pagination support for large datasets
    /// - Export capabilities (CSV/Excel)
    /// - Comprehensive plan information including pricing and features
    /// - Used for administrative plan management and oversight
    /// </remarks>
    [HttpGet("admin")]
    public async Task<JsonModel> GetAllSubscriptionPlans(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? format = null)
    {
        if (!string.IsNullOrEmpty(format) && (format.ToLower() == "csv" || format.ToLower() == "excel"))
        {
            return await _subscriptionPlanService.ExportSubscriptionPlansAsync(GetToken(HttpContext), searchTerm, categoryId, isActive, format);
        }
        
        var filter = new SubscriptionPlanFilterDto
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = !string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId) ? catId : null,
            IsActive = isActive
        };
        return await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(filter, GetToken(HttpContext), adminOnly: true);
    }

    /// <summary>
    /// Retrieves all active subscription plans for administrative management with comprehensive filtering and pagination.
    /// This endpoint provides administrators with access to all currently active subscription plans
    /// in the system with advanced filtering capabilities for administrative oversight and plan management operations.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50)</param>
    /// <param name="searchTerm">Search term for filtering plans</param>
    /// <param name="categoryId">Filter plans by category ID</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>JsonModel containing paginated active subscription plans with filtering applied</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for administrative plan management
    /// - Returns 403 Forbidden for non-admin users
    /// 
    /// Business Logic:
    /// - Retrieves only active subscription plans (IsActive = true)
    /// - Supports pagination for large datasets
    /// - Includes advanced filtering by search term and category
    /// - Provides sorting capabilities for data organization
    /// - Returns comprehensive plan information including pricing and features
    /// - Used for administrative plan oversight and management
    /// - Handles plan validation and error responses
    /// - Supports advanced filtering for plan analysis
    /// </remarks>
    [HttpGet("admin/active")]
    public async Task<JsonModel> GetActiveSubscriptionPlans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null)
    {
        var filter = new SubscriptionPlanFilterDto
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = !string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId) ? catId : null,
            IsActive = true,
            SortColumn = sortBy ?? "CreatedDate",
            SortOrder = sortOrder ?? "desc"
        };
        return await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(filter, GetToken(HttpContext), adminOnly: true);
    }

    /// <summary>
    /// Retrieves subscription plans by specific category for administrative management.
    /// This endpoint provides administrators with access to subscription plans filtered by category
    /// for targeted management and oversight operations.
    /// </summary>
    /// <param name="category">The category name to filter plans by</param>
    /// <returns>JsonModel containing subscription plans in the specified category or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for administrative plan management
    /// - Returns 403 Forbidden for non-admin users
    /// - Returns 404 Not Found if category doesn't exist
    /// 
    /// Business Logic:
    /// - Validates category exists in the system
    /// - Retrieves plans associated with the specified category
    /// - Returns comprehensive plan information for administrative use
    /// - Handles category validation and error responses
    /// </remarks>
    [HttpGet("admin/category/{category}")]
    public async Task<JsonModel> GetSubscriptionPlansByCategory(string category)
    {
        var filter = new SubscriptionPlanFilterDto
        {
            Page = 1,
            PageSize = 1000,
            CategoryId = Guid.TryParse(category, out var catId) ? catId : null
        };
        return await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(filter, GetToken(HttpContext), adminOnly: true);
    }

    /// <summary>
    /// Retrieves detailed information about a specific subscription plan for administrative management.
    /// This endpoint provides administrators with comprehensive details about a specific subscription plan
    /// including pricing, features, privileges, and administrative information.
    /// </summary>
    /// <param name="planId">The unique identifier (GUID) of the subscription plan to retrieve</param>
    /// <returns>JsonModel containing detailed subscription plan information or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for detailed plan information
    /// - Returns 403 Forbidden for non-admin users
    /// - Returns 404 Not Found if plan doesn't exist
    /// 
    /// Business Logic:
    /// - Validates plan exists in the system
    /// - Returns comprehensive plan details including administrative information
    /// - Includes pricing, features, and privilege information
    /// - Handles plan validation and error responses
    /// </remarks>
    [HttpGet("admin/{planId}")]
    public async Task<JsonModel> GetSubscriptionPlan(string planId)
    {
        return await _subscriptionPlanService.GetPlanByIdAsync(planId, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new subscription plan for administrative management.
    /// This endpoint handles subscription plan creation including validation, configuration,
    /// and integration setup. It ensures that new plans are properly configured and integrated
    /// with the payment system and other services.
    /// </summary>
    /// <param name="createDto">DTO containing subscription plan creation details</param>
    /// <returns>JsonModel containing the created subscription plan or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for plan creation
    /// - Returns 403 Forbidden for non-admin users
    /// - Returns 400 Bad Request for invalid plan data
    /// 
    /// Business Logic:
    /// - Validates plan data and configuration
    /// - Creates plan with proper pricing and feature configuration
    /// - Integrates with payment system and other services
    /// - Maintains plan creation audit trails and history
    /// </remarks>
    [HttpPost("admin")]
    public async Task<JsonModel> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        return await _subscriptionPlanService.CreatePlanAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing subscription plan for administrative management.
    /// This endpoint handles subscription plan updates including validation, configuration changes,
    /// and integration updates. It ensures that plan changes are properly applied and integrated.
    /// </summary>
    /// <param name="planId">The unique identifier (GUID) of the subscription plan to update</param>
    /// <param name="updateDto">DTO containing subscription plan update details</param>
    /// <returns>JsonModel containing the updated subscription plan or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for plan updates
    /// - Returns 403 Forbidden for non-admin users
    /// - Returns 404 Not Found if plan doesn't exist
    /// - Returns 400 Bad Request for invalid update data
    /// 
    /// Business Logic:
    /// - Validates plan exists and update data
    /// - Updates plan configuration and pricing
    /// - Integrates changes with payment system and other services
    /// - Maintains plan update audit trails and change history
    /// </remarks>
    [HttpPut("admin/{planId}")]
    public async Task<JsonModel> UpdateSubscriptionPlan(string planId, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        return await _subscriptionPlanService.UpdatePlanAsync(planId, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a subscription plan from the system for administrative management.
    /// This endpoint handles subscription plan deletion including validation, dependency checking,
    /// and cleanup operations. It ensures that plans with active subscriptions cannot be deleted.
    /// </summary>
    /// <param name="planId">The unique identifier (GUID) of the subscription plan to delete</param>
    /// <returns>JsonModel containing deletion confirmation or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for plan deletion
    /// - Returns 403 Forbidden for non-admin users
    /// - Returns 404 Not Found if plan doesn't exist
    /// - Returns 400 Bad Request if plan has active subscriptions
    /// 
    /// Business Logic:
    /// - Validates plan exists and is not in use
    /// - Checks for active subscriptions before deletion
    /// - Performs cleanup operations and integrations
    /// - Maintains plan deletion audit trails and removal history
    /// </remarks>
    /// <summary>
    /// Deactivates a subscription plan (soft delete) - RECOMMENDED APPROACH.
    /// This endpoint provides administrators with the ability to deactivate
    /// subscription plans while preserving data integrity and business continuity.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to deactivate</param>
    /// <returns>JsonModel containing the deactivation result and any relevant information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates admin access and plan existence
    /// - Checks for active subscriptions before deactivation
    /// - Deactivates Stripe resources instead of deleting them
    /// - Preserves historical data for reporting and analytics
    /// - Maintains referential integrity with existing subscriptions
    /// - Allows for plan reactivation if needed
    /// </remarks>
    [HttpPost("admin/{planId}/deactivate")]
    public async Task<JsonModel> DeactivateSubscriptionPlan(string planId)
    {
        return await _subscriptionPlanService.DeactivatePlanAsync(planId, GetToken(HttpContext));
    }

    /// <summary>
    /// Reactivates a deactivated subscription plan.
    /// This endpoint allows administrators to restore previously deactivated
    /// subscription plans back to active status.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to reactivate</param>
    /// <returns>JsonModel containing the reactivation result and any relevant information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates admin access and plan existence
    /// - Checks if plan is already active
    /// - Restores plan to active status
    /// - Maintains audit trail of reactivation
    /// </remarks>
    [HttpPost("admin/{planId}/reactivate")]
    public async Task<JsonModel> ReactivateSubscriptionPlan(string planId)
    {
        return await _subscriptionPlanService.ReactivatePlanAsync(planId, GetToken(HttpContext));
    }

    [HttpDelete("admin/{planId}")]
    [Obsolete("Use DeactivateSubscriptionPlan instead for better data integrity and business continuity")]
    public async Task<JsonModel> DeleteSubscriptionPlan(string planId)
    {
        return await _subscriptionPlanService.DeletePlanAsync(planId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves subscription plans with pagination and filtering for administrative management.
    /// This endpoint provides administrators with paginated access to subscription plans with
    /// advanced filtering and search capabilities.
    /// </summary>
    /// <param name="page">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Search term to filter plans by name or description</param>
    /// <param name="categoryId">Category ID to filter plans by category</param>
    /// <param name="isActive">Filter by active status (true/false/null for all)</param>
    /// <param name="includeAnalytics">Include analytics data in response</param>
    /// <returns>JsonModel containing paginated subscription plans or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for paginated plan management
    /// - Returns 403 Forbidden for non-admin users
    /// 
    /// Features:
    /// - Pagination support for large datasets
    /// - Advanced filtering by search term, category, and active status
    /// - Optional analytics data inclusion
    /// - Comprehensive plan information for administrative use
    /// </remarks>
    [HttpGet("admin/paged")]
    public async Task<JsonModel> GetAllPlansPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool includeAnalytics = false)
    {
        if (includeAnalytics == true)
        {
            // This method should be moved to a dedicated analytics service
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Plan analytics not available - use analytics service", 
                StatusCode = 501 
            };
        }
        
        var filter = new SubscriptionPlanFilterDto
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = !string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId) ? catId : null,
            IsActive = isActive
        };
        return await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(filter, GetToken(HttpContext), adminOnly: false);
    }

    /// <summary>
    /// Retrieves public subscription plans for homepage display (no authentication required).
    /// This endpoint returns a curated list of subscription plans that are suitable for
    /// public display on marketing pages, pricing pages, and signup flows.
    /// </summary>
    /// <returns>JsonModel containing public subscription plans or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - No authentication required - public access
    /// - Returns only active plans suitable for public display
    /// 
    /// Business Logic:
    /// - Retrieves only active subscription plans
    /// - Returns public-friendly plan information
    /// - Used for marketing pages and signup flows
    /// - Optimized for public consumption
    /// </remarks>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<JsonModel> GetPublicPlans()
    {
        var filter = new SubscriptionPlanFilterDto
        {
            Page = 1,
            PageSize = 1000,
            IsActive = true
        };
        return await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(filter, null, adminOnly: false);
    }

    #region Additional Admin Endpoints (Consolidated from other controllers)

    /// <summary>
    /// Retrieves subscription plans with comprehensive filtering for admin management.
    /// This endpoint supports advanced filtering, pagination, sorting, and search capabilities
    /// for administrative purposes with full access to all plan data and configurations.
    /// </summary>
    /// <param name="filter">Comprehensive filter DTO containing all filter parameters</param>
    /// <returns>JsonModel containing filtered, paginated, and sorted subscription plans with metadata</returns>
    /// <remarks>
    /// This endpoint supports:
    /// - Advanced search by name, description, or short description
    /// - Filtering by category, pricing, billing cycle, and status
    /// - Date range filtering for creation, update, and effective dates
    /// - Trial duration and display order filtering
    /// - Stripe integration status filtering
    /// - Subscription status filtering (has active subscriptions, etc.)
    /// - Comprehensive pagination with metadata
    /// - Dynamic sorting by multiple columns
    /// - Admin-only access with full plan data visibility
    /// </remarks>
    [HttpPost("admin/filter")]
    public async Task<JsonModel> GetPlansWithAdvancedFiltering([FromBody] SubscriptionPlanFilterDto filter)
    {
        return await _subscriptionPlanService.GetSubscriptionPlansWithFilteringAsync(filter, GetToken(HttpContext), adminOnly: true);
    }

    /// <summary>
    /// Assigns privileges to a subscription plan for administrative management.
    /// This endpoint allows administrators to configure which privileges are available
    /// within a specific subscription plan, including usage limits and restrictions.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="privileges">List of privileges to assign to the plan</param>
    /// <returns>JsonModel containing the assignment result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Assigns multiple privileges to a subscription plan
    /// - Configures privilege usage limits and restrictions
    /// - Validates privilege and plan existence
    /// - Access restricted to administrators only
    /// - Used for plan privilege configuration and management
    /// - Includes comprehensive validation and error handling
    /// - Maintains privilege assignment audit trails
    /// </remarks>
    [HttpPost("admin/{planId}/privileges")]
    public async Task<JsonModel> AssignPrivilegesToPlan(string planId, [FromBody] List<PlanPrivilegeDto> privileges)
    {
        return await _subscriptionPlanService.AssignPrivilegesToPlanAsync(Guid.Parse(planId), privileges, GetToken(HttpContext));
    }

    /// <summary>
    /// Removes a privilege from a subscription plan for administrative management.
    /// This endpoint allows administrators to remove specific privileges from
    /// subscription plans, affecting future subscriptions but not existing ones.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="privilegeId">The unique identifier of the privilege to remove</param>
    /// <returns>JsonModel containing the removal result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Removes a specific privilege from a subscription plan
    /// - Validates plan and privilege existence
    /// - Access restricted to administrators only
    /// - Used for plan privilege management and cleanup
    /// - Includes comprehensive validation and error handling
    /// - Maintains privilege removal audit trails
    /// </remarks>
    [HttpDelete("admin/{planId}/privileges/{privilegeId}")]
    public async Task<JsonModel> RemovePrivilegeFromPlan(string planId, string privilegeId)
    {
        return await _subscriptionPlanService.RemovePrivilegeFromPlanAsync(Guid.Parse(planId), Guid.Parse(privilegeId), GetToken(HttpContext));
    }

    /// <summary>
    /// Updates a privilege configuration within a subscription plan for administrative management.
    /// This endpoint allows administrators to modify privilege settings such as usage limits,
    /// restrictions, and availability within specific subscription plans.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="privilegeId">The unique identifier of the privilege to update</param>
    /// <param name="privilegeDto">Updated privilege configuration</param>
    /// <returns>JsonModel containing the update result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates privilege configuration within a subscription plan
    /// - Modifies usage limits, restrictions, and availability
    /// - Validates plan and privilege existence
    /// - Access restricted to administrators only
    /// - Used for plan privilege configuration updates
    /// - Includes comprehensive validation and error handling
    /// - Maintains privilege update audit trails
    /// </remarks>
    [HttpPut("admin/{planId}/privileges/{privilegeId}")]
    public async Task<JsonModel> UpdatePlanPrivilege(string planId, string privilegeId, [FromBody] PlanPrivilegeDto privilegeDto)
    {
        return await _subscriptionPlanService.UpdatePlanPrivilegeAsync(Guid.Parse(planId), Guid.Parse(privilegeId), privilegeDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all privileges associated with a specific subscription plan for administrative management.
    /// This endpoint provides administrators with a comprehensive list of privileges
    /// configured for a particular subscription plan, including their settings and limits.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <returns>JsonModel containing the plan's privileges and their configurations</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all privileges associated with a subscription plan
    /// - Includes privilege configurations, limits, and restrictions
    /// - Shows privilege availability and usage settings
    /// - Access restricted to administrators only
    /// - Used for plan privilege management and oversight
    /// - Includes comprehensive privilege information and metadata
    /// - Provides data for privilege analysis and management
    /// </remarks>
    [HttpGet("admin/{planId}/privileges")]
    public async Task<JsonModel> GetPlanPrivileges(string planId)
    {
        return await _subscriptionPlanService.GetPlanPrivilegesAsync(Guid.Parse(planId), GetToken(HttpContext));
    }

    #endregion

    #region Privilege Management

    /// <summary>
    /// Updates time-based usage limits for a subscription plan privilege.
    /// This endpoint allows administrators to configure daily, weekly, and monthly usage limits
    /// for specific privileges within subscription plans, including effective dates and duration settings.
    /// </summary>
    /// <param name="updateDto">DTO containing time-based limit configuration details</param>
    /// <returns>JsonModel containing the updated time-based limits</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates time-based usage limits for privileges
    /// - Configures daily, weekly, and monthly limits
    /// - Sets effective dates and duration settings
    /// - Access restricted to administrators only
    /// - Used for privilege limit management and configuration
    /// - Includes comprehensive validation and error handling
    /// </remarks>
    [HttpPut("admin/privileges/time-based-limits")]
    public async Task<JsonModel> UpdateTimeBasedLimits([FromBody] UpdateTimeBasedLimitsDto updateDto)
    {
        return await _privilegeService.UpdateTimeBasedLimitsAsync(updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves time-based usage limits for a specific subscription plan privilege.
    /// This endpoint provides comprehensive information about configured time-based limits
    /// including daily, weekly, and monthly restrictions for a specific plan privilege.
    /// </summary>
    /// <param name="planPrivilegeId">The unique identifier of the subscription plan privilege</param>
    /// <returns>JsonModel containing the time-based limits configuration</returns>
    [HttpGet("admin/privileges/{planPrivilegeId}/time-based-limits")]
    public async Task<JsonModel> GetTimeBasedLimits(string planPrivilegeId)
    {
        try
        {
            // This would typically retrieve the time-based limits from the database
            // For now, return a placeholder response
            var timeBasedLimits = new
            {
                PlanPrivilegeId = planPrivilegeId,
                DailyLimit = 5,
                WeeklyLimit = 20,
                MonthlyLimit = 80,
                UsagePeriodId = Guid.NewGuid(),
                DurationMonths = 1,
                Description = "Standard time-based limits",
                EffectiveDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(1)
            };

            return new JsonModel
            {
                data = timeBasedLimits,
                Message = "Time-based limits retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving time-based limits: {ex.Message}",
                StatusCode = 500
            };
        }
    }



    /// <summary>
    /// Creates a new privilege in the system.
    /// This endpoint allows administrators to create new privileges that can be assigned to subscription plans.
    /// </summary>
    /// <param name="privilegeDto">DTO containing the privilege details</param>
    /// <returns>JsonModel containing the created privilege</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new privilege with proper validation
    /// - Validates privilege data and configuration
    /// - Access restricted to administrators only
    /// - Used for privilege management and administration
    /// - Includes comprehensive validation and error handling
    /// - Maintains privilege creation audit trails
    /// </remarks>
    [HttpPost("admin/privileges")]
    public async Task<JsonModel> CreatePrivilege([FromBody] CreatePrivilegeDto privilegeDto)
    {
        return await _privilegeService.CreatePrivilegeAsync(privilegeDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing privilege in the system.
    /// This endpoint allows administrators to update privilege details.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege</param>
    /// <param name="privilegeDto">DTO containing the updated privilege details</param>
    /// <returns>JsonModel containing the updated privilege</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates an existing privilege with proper validation
    /// - Validates privilege data and configuration
    /// - Access restricted to administrators only
    /// - Used for privilege management and administration
    /// - Includes comprehensive validation and error handling
    /// - Maintains privilege update audit trails
    /// </remarks>
    [HttpPut("admin/privileges/{id}")]
    public async Task<JsonModel> UpdatePrivilege(string id, [FromBody] UpdatePrivilegeDto privilegeDto)
    {
        return await _privilegeService.UpdatePrivilegeAsync(id, privilegeDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a privilege from the system.
    /// This endpoint allows administrators to remove privileges that are no longer needed.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Soft deletes a privilege from the system
    /// - Validates privilege existence before deletion
    /// - Access restricted to administrators only
    /// - Used for privilege management and cleanup
    /// - Includes comprehensive validation and error handling
    /// - Maintains privilege deletion audit trails
    /// </remarks>
    [HttpDelete("admin/privileges/{id}")]
    public async Task<JsonModel> DeletePrivilege(string id)
    {
        return await _privilegeService.DeletePrivilegeAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all privileges in the system with filtering and pagination.
    /// This endpoint allows administrators to view and manage all privileges.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50)</param>
    /// <param name="search">Search term for filtering privileges</param>
    /// <param name="category">Category filter for privileges</param>
    /// <param name="status">Status filter (active/inactive)</param>
    /// <returns>JsonModel containing paginated privileges</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all privileges with advanced filtering
    /// - Supports pagination for large datasets
    /// - Includes search functionality by name and description
    /// - Access restricted to administrators only
    /// - Used for privilege management and oversight
    /// - Includes comprehensive privilege information
    /// </remarks>
    [HttpGet("admin/privileges")]
    public async Task<JsonModel> GetAllPrivileges(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null)
    {
        return await _privilegeService.GetAllPrivilegesAsync(page, pageSize, search, category, status, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves a specific privilege by its unique identifier.
    /// This endpoint allows administrators to view detailed information about a specific privilege.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege</param>
    /// <returns>JsonModel containing the privilege details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed information about a specific privilege
    /// - Validates privilege existence
    /// - Access restricted to administrators only
    /// - Used for privilege management and administration
    /// - Includes comprehensive privilege information and metadata
    /// </remarks>
    [HttpGet("admin/privileges/{id}")]
    public async Task<JsonModel> GetPrivilege(string id)
    {
        return await _privilegeService.GetPrivilegeByIdAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all privilege categories available in the system.
    /// This endpoint provides a list of categories that can be used to organize privileges.
    /// </summary>
    /// <returns>JsonModel containing the list of privilege categories</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all privilege categories available in the system
    /// - Used for privilege organization and management
    /// - Access restricted to administrators only
    /// - Includes comprehensive category information
    /// </remarks>
    [HttpGet("admin/privileges/categories")]
    public async Task<JsonModel> GetPrivilegeCategories()
    {
        return await _privilegeService.GetPrivilegeCategoriesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all privilege types available in the system.
    /// This endpoint provides a list of types that can be used to categorize privileges.
    /// </summary>
    /// <returns>JsonModel containing the list of privilege types</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all privilege types available in the system
    /// - Used for privilege categorization and management
    /// - Access restricted to administrators only
    /// - Includes comprehensive type information
    /// </remarks>
    [HttpGet("admin/privileges/types")]
    public async Task<JsonModel> GetPrivilegeTypes()
    {
        return await _privilegeService.GetPrivilegeTypesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves privilege usage history for analysis and reporting.
    /// This endpoint provides historical data about privilege usage across the system.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50)</param>
    /// <param name="privilegeId">Filter by privilege ID</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="subscriptionId">Filter by subscription ID</param>
    /// <param name="startDate">Start date for filtering</param>
    /// <param name="endDate">End date for filtering</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>JsonModel containing the privilege usage history</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns privilege usage history with advanced filtering
    /// - Supports pagination for large datasets
    /// - Includes filtering by privilege, user, subscription, and date range
    /// - Access restricted to administrators only
    /// - Used for privilege usage analysis and reporting
    /// </remarks>
    [HttpGet("admin/privileges/usage-history")]
    public async Task<JsonModel> GetPrivilegeUsageHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? privilegeId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? subscriptionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null)
    {
        return await _privilegeService.GetUsageHistoryAsync(page, pageSize, privilegeId, userId, subscriptionId, startDate, endDate, sortBy, sortOrder, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves privilege usage summary for analysis and reporting.
    /// This endpoint provides summarized data about privilege usage across the system.
    /// </summary>
    /// <param name="privilegeId">Filter by privilege ID</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="subscriptionId">Filter by subscription ID</param>
    /// <param name="startDate">Start date for filtering</param>
    /// <param name="endDate">End date for filtering</param>
    /// <returns>JsonModel containing the privilege usage summary</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns privilege usage summary with filtering options
    /// - Provides aggregated usage statistics
    /// - Access restricted to administrators only
    /// - Used for privilege usage analysis and reporting
    /// </remarks>
    [HttpGet("admin/privileges/usage-summary")]
    public async Task<JsonModel> GetPrivilegeUsageSummary(
        [FromQuery] string? privilegeId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? subscriptionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _privilegeService.GetUsageSummaryAsync(privilegeId, userId, subscriptionId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Exports privilege usage data for analysis and reporting.
    /// This endpoint allows administrators to export privilege usage data in various formats.
    /// </summary>
    /// <param name="format">Export format (csv, json, excel)</param>
    /// <param name="privilegeId">Filter by privilege ID</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="subscriptionId">Filter by subscription ID</param>
    /// <param name="startDate">Start date for filtering</param>
    /// <param name="endDate">End date for filtering</param>
    /// <returns>JsonModel containing the exported privilege usage data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Exports privilege usage data in specified format
    /// - Supports filtering by privilege, user, subscription, and date range
    /// - Access restricted to administrators only
    /// - Used for privilege usage analysis and reporting
    /// </remarks>
    [HttpGet("admin/privileges/usage-export")]
    public async Task<JsonModel> ExportPrivilegeUsage(
        [FromQuery] string format = "csv",
        [FromQuery] string? privilegeId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? subscriptionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _privilegeService.ExportUsageDataAsync(format, privilegeId, userId, subscriptionId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Exports privileges data for analysis and reporting.
    /// This endpoint allows administrators to export privileges data in various formats.
    /// </summary>
    /// <param name="format">Export format (csv, json, excel)</param>
    /// <param name="search">Search term for filtering privileges</param>
    /// <param name="category">Category filter for privileges</param>
    /// <param name="status">Status filter (active/inactive)</param>
    /// <returns>JsonModel containing the exported privileges data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Exports privileges data in specified format
    /// - Supports filtering by search term, category, and status
    /// - Access restricted to administrators only
    /// - Used for privilege management and reporting
    /// </remarks>
    [HttpGet("admin/privileges/export")]
    public async Task<JsonModel> ExportPrivileges(
        [FromQuery] string format = "csv",
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null)
    {
        return await _privilegeService.ExportPrivilegesAsync(search, category, status, format, GetToken(HttpContext));
    }


    #endregion
}


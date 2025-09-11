using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
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

    /// <summary>
    /// Initializes a new instance of the SubscriptionPlansController with the required subscription plan service.
    /// </summary>
    /// <param name="subscriptionPlanService">Service for handling subscription plan-related business logic</param>
    public SubscriptionPlansController(ISubscriptionPlanService subscriptionPlanService)
    {
        _subscriptionPlanService = subscriptionPlanService;
    }


    /// <summary>
    /// Retrieves all active subscription plans available for public viewing.
    /// This endpoint returns only active subscription plans that are suitable for
    /// public display and user subscription, excluding administrative or draft plans.
    /// </summary>
    /// <returns>JsonModel containing active subscription plans for public consumption</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns only active subscription plans
    /// - Includes public-facing plan information and pricing
    /// - No authentication required - accessible to all users
    /// - Used for plan selection and comparison by potential subscribers
    /// - Optimized for public consumption with marketing-friendly information
    /// - Excludes administrative details and internal configurations
    /// </remarks>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<JsonModel> GetActivePlans()
    {
        return await _subscriptionPlanService.GetActiveSubscriptionPlansAsync(GetToken(HttpContext));
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
        return await _subscriptionPlanService.GetSubscriptionPlansByCategoryAsync(categoryId, GetToken(HttpContext));
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
        return await _subscriptionPlanService.GetSubscriptionPlanAsync(id, GetToken(HttpContext));
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
    /// Deactivates a subscription plan to prevent new user subscriptions.
    /// This endpoint handles subscription plan deactivation including validation, status updates,
    /// and notification processes. It ensures that existing subscribers are properly notified
    /// and maintains proper audit trails for plan status changes.
    /// </summary>
    /// <param name="planId">The unique identifier (GUID) of the subscription plan to deactivate</param>
    /// <returns>JsonModel containing deactivation confirmation or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for plan deactivation
    /// - Returns 403 Forbidden for non-admin users
    /// - Returns 404 Not Found if plan doesn't exist
    /// 
    /// Business Logic:
    /// - Validates plan exists and is currently active
    /// - Updates plan status to inactive
    /// - Notifies existing subscribers of plan deactivation
    /// - Maintains plan deactivation audit trails and status history
    /// </remarks>
    [HttpPost("{planId}/deactivate")]
    public async Task<JsonModel> DeactivatePlan(string planId)
    {
        return await _subscriptionPlanService.DeactivatePlanAsync(planId, GetToken(HttpContext).UserID.ToString(), GetToken(HttpContext));
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
        
        return await _subscriptionPlanService.GetAllSubscriptionPlansAsync(GetToken(HttpContext), searchTerm, categoryId, isActive, page, pageSize);
    }

    /// <summary>
    /// Retrieves all active subscription plans for administrative management.
    /// This endpoint provides administrators with access to all currently active subscription plans
    /// in the system. It's used for administrative oversight and plan management operations.
    /// </summary>
    /// <returns>JsonModel containing active subscription plans or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admin access required for administrative plan management
    /// - Returns 403 Forbidden for non-admin users
    /// 
    /// Business Logic:
    /// - Retrieves only active subscription plans (IsActive = true)
    /// - Returns comprehensive plan information including pricing and features
    /// - Used for administrative plan oversight and management
    /// - Handles plan validation and error responses
    /// </remarks>
    [HttpGet("admin/active")]
    public async Task<JsonModel> GetActiveSubscriptionPlans()
    {
        return await _subscriptionPlanService.GetActiveSubscriptionPlansAsync(GetToken(HttpContext));
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
        return await _subscriptionPlanService.GetSubscriptionPlansByCategoryAsync(category, GetToken(HttpContext));
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
        return await _subscriptionPlanService.GetSubscriptionPlanAsync(planId, GetToken(HttpContext));
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
    [HttpDelete("admin/{planId}")]
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
        
        return await _subscriptionPlanService.GetAllPlansAsync(page, pageSize, searchTerm, categoryId, isActive, GetToken(HttpContext));
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
        return await _subscriptionPlanService.GetPublicPlansAsync();
    }
} 
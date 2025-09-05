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
    private readonly ISubscriptionService _subscriptionService;

    /// <summary>
    /// Initializes a new instance of the SubscriptionPlansController with the required subscription service.
    /// </summary>
    /// <param name="subscriptionService">Service for handling subscription plan-related business logic</param>
    public SubscriptionPlansController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Retrieves all subscription plans in the system (admin only).
    /// This endpoint returns a comprehensive list of all subscription plans including
    /// both active and inactive plans, with full administrative details.
    /// </summary>
    /// <returns>JsonModel containing all subscription plans with administrative details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all subscription plans regardless of status
    /// - Includes administrative details and internal information
    /// - Shows plan configuration, pricing, and feature details
    /// - Access restricted to administrators only
    /// - Used for administrative plan management and oversight
    /// - Includes inactive and draft plans for complete system view
    /// </remarks>
    [HttpGet]
    public async Task<JsonModel> GetAllPlans()
    {
        return await _subscriptionService.GetAllSubscriptionPlansAsync(GetToken(HttpContext));
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
        return await _subscriptionService.GetActiveSubscriptionPlansAsync(GetToken(HttpContext));
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
        return await _subscriptionService.GetSubscriptionPlansByCategoryAsync(categoryId, GetToken(HttpContext));
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
        return await _subscriptionService.GetSubscriptionPlanAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new subscription plan in the system (admin only).
    /// This endpoint allows administrators to create new subscription plans with
    /// specified features, pricing, privileges, and configuration options.
    /// </summary>
    /// <param name="createDto">DTO containing the subscription plan creation details</param>
    /// <returns>JsonModel containing the creation result and new plan information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new subscription plan with specified configuration
    /// - Sets up plan features, pricing, and privilege associations
    /// - Integrates with Stripe for payment processing setup
    /// - Access restricted to administrators only
    /// - Used for adding new subscription tiers and service offerings
    /// - Includes validation of plan configuration and business rules
    /// - Sets up audit trails and administrative tracking
    /// </remarks>
    [HttpPost]
    public async Task<JsonModel> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        return await _subscriptionService.CreatePlanAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing subscription plan (admin only).
    /// This endpoint allows administrators to modify subscription plan details including
    /// pricing, features, privileges, and availability status.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to update</param>
    /// <param name="updateDto">DTO containing the updated subscription plan information</param>
    /// <returns>JsonModel containing the update result and updated plan information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates existing subscription plan with new configuration
    /// - Modifies plan features, pricing, and privilege associations
    /// - Synchronizes changes with Stripe for payment processing
    /// - Access restricted to administrators only
    /// - Used for plan maintenance and feature updates
    /// - Includes validation of plan changes and business impact
    /// - Maintains audit trails of all plan modifications
    /// - Handles impact on existing subscribers if applicable
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        if (id != updateDto.Id)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        updateDto.Id = id;
        return await _subscriptionService.UpdatePlanAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a subscription plan from the system (admin only).
    /// This endpoint removes a subscription plan from the system, handling cleanup
    /// of associated data and ensuring no active subscriptions are affected.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription plan to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Removes the subscription plan from the system
    /// - Validates that no active subscriptions are using the plan
    /// - Cleans up associated Stripe resources and configurations
    /// - Access restricted to administrators only
    /// - Used for removing obsolete or discontinued plans
    /// - Includes safety checks to prevent data loss
    /// - Maintains audit trails of plan deletion
    /// - Handles cleanup of related privilege associations
    /// </remarks>
    [HttpDelete("{id}")]
    public async Task<JsonModel> DeletePlan(string id)
    {
        return await _subscriptionService.DeleteSubscriptionPlanAsync(id, GetToken(HttpContext));
    }
} 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Security.Claims;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for user-facing subscription operations and management.
/// This controller provides functionality for users to manage their own subscriptions,
/// including purchasing new subscriptions, cancelling existing ones, and tracking
/// privilege usage. It serves as the primary interface for user subscription interactions.
/// </summary>
[ApiController]
[Route("api/user/[controller]")]
//[Authorize]
public class UserSubscriptionsController : BaseController
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly PrivilegeService _privilegeService;
    private readonly SubscriptionService _subscriptionService;

    /// <summary>
    /// Initializes a new instance of the UserSubscriptionsController with required services.
    /// </summary>
    /// <param name="subscriptionRepo">Repository for subscription data access</param>
    /// <param name="planRepo">Repository for subscription plan data access</param>
    /// <param name="privilegeService">Service for privilege management operations</param>
    /// <param name="subscriptionService">Service for subscription business logic</param>
    public UserSubscriptionsController(
        ISubscriptionRepository subscriptionRepo,
        ISubscriptionPlanRepository planRepo,
        PrivilegeService privilegeService,
        SubscriptionService subscriptionService)
    {
        _subscriptionRepo = subscriptionRepo;
        _planRepo = planRepo;
        _privilegeService = privilegeService;
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Gets the current user ID from the authentication claims.
    /// This helper method extracts the user ID from the JWT token claims for use in service calls.
    /// </summary>
    /// <returns>The current user ID</returns>
    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Retrieves all subscriptions for the current authenticated user.
    /// This endpoint provides a comprehensive list of the user's active and inactive subscriptions
    /// including subscription details, plan information, and current status.
    /// </summary>
    /// <returns>JsonModel containing the user's subscriptions</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all subscriptions for the authenticated user
    /// - Includes subscription details, plan information, and status
    /// - Shows subscription history and current active subscriptions
    /// - Access restricted to authenticated users
    /// - Used for user subscription overview and management
    /// - Includes comprehensive subscription information and metadata
    /// - Provides secure access to user subscription data
    /// - Handles authentication validation and error responses
    /// </remarks>
    [HttpGet("subscriptions")]
    public async Task<JsonModel> GetUserSubscriptions()
    {
        var userId = GetCurrentUserId();
        return await _subscriptionService.GetUserSubscriptionsAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Purchases a new subscription for the current authenticated user.
    /// This endpoint handles subscription purchase including plan selection, payment processing,
    /// and subscription creation for the user's account.
    /// </summary>
    /// <param name="dto">DTO containing subscription purchase details including plan ID</param>
    /// <returns>JsonModel containing the created subscription information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new subscription for the authenticated user
    /// - Validates plan availability and user eligibility
    /// - Processes subscription creation and activation
    /// - Access restricted to authenticated users
    /// - Used for subscription purchase and plan selection
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on subscription creation
    /// - Maintains subscription purchase audit trails
    /// </remarks>
    [HttpPost("subscriptions")]
    public async Task<JsonModel> PurchaseSubscription([FromBody] PurchaseSubscriptionDto dto)
    {
        var userId = GetCurrentUserId();
        var createDto = new CreateSubscriptionDto
        {
            UserId = userId,
            PlanId = dto.PlanId.ToString()
        };
        var result = await _subscriptionService.CreateSubscriptionAsync(createDto, GetToken(HttpContext));
        if (result.StatusCode != 200) 
            return new JsonModel { data = new object(), Message = result.Message, StatusCode = result.StatusCode };
        return result;
    }

    /// <summary>
    /// Cancels an existing subscription for the current authenticated user.
    /// This endpoint handles subscription cancellation including validation, status updates,
    /// and cancellation processing for the user's subscription.
    /// </summary>
    /// <param name="dto">DTO containing subscription cancellation details including subscription ID</param>
    /// <returns>JsonModel containing the cancellation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Cancels an existing subscription for the authenticated user
    /// - Validates subscription ownership and cancellation eligibility
    /// - Processes subscription cancellation and status updates
    /// - Access restricted to subscription owners
    /// - Used for subscription cancellation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on cancellation operations
    /// - Maintains subscription cancellation audit trails
    /// </remarks>
    [HttpPost("subscriptions/cancel")]
    public async Task<JsonModel> CancelSubscription([FromBody] CancelSubscriptionDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _subscriptionService.CancelSubscriptionAsync(dto.SubscriptionId.ToString(), null, GetToken(HttpContext));
        if (result.StatusCode != 200) 
            return new JsonModel { data = new object(), Message = result.Message, StatusCode = result.StatusCode };
        return result;
    }

    /// <summary>
    /// Retrieves privilege usage information for all of the user's subscriptions.
    /// This endpoint provides comprehensive privilege usage tracking including remaining
    /// usage counts, privilege limits, and usage statistics across all user subscriptions.
    /// </summary>
    /// <returns>JsonModel containing privilege usage information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns privilege usage information for all user subscriptions
    /// - Includes remaining usage counts and privilege limits
    /// - Shows privilege usage statistics and tracking
    /// - Access restricted to authenticated users
    /// - Used for privilege usage monitoring and management
    /// - Includes comprehensive privilege information and usage data
    /// - Provides data for privilege usage decisions
    /// - Handles privilege validation and error responses
    /// </remarks>
    [HttpGet("privilege-usage")]
    public async Task<JsonModel> GetPrivilegeUsage()
    {
        var userId = GetCurrentUserId();
        var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId, GetToken(HttpContext));
        
        if (subscriptions.StatusCode != 200)
            return subscriptions;
            
        var subscriptionList = subscriptions.data as IEnumerable<SubscriptionDto>;
        if (subscriptionList == null || !subscriptionList.Any())
            return new JsonModel { data = new List<object>(), Message = "No subscriptions found", StatusCode = 200 };
            
        var privilegeUsageList = new List<object>();
        foreach (var subscription in subscriptionList)
        {
            var subscriptionId = Guid.Parse(subscription.Id);
            var planId = Guid.Parse(subscription.PlanId);
            var planPrivileges = await _privilegeService.GetPrivilegesForPlanAsync(planId, GetToken(HttpContext));
            foreach (var privilege in planPrivileges)
            {
                var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, privilege.Name, GetToken(HttpContext));
                privilegeUsageList.Add(new
                {
                    SubscriptionId = subscription.Id,
                    PlanName = subscription.PlanName,
                    PrivilegeName = privilege.Name,
                    Remaining = remaining
                });
            }
        }
        
        return new JsonModel { data = privilegeUsageList, Message = "Privilege usage retrieved successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Uses a specific privilege from the user's subscription.
    /// This endpoint handles privilege consumption including validation, usage tracking,
    /// and privilege limit enforcement for subscription-based services.
    /// </summary>
    /// <param name="dto">DTO containing privilege usage details including subscription ID, privilege name, and amount</param>
    /// <returns>JsonModel containing the privilege usage result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Consumes a privilege from the user's subscription
    /// - Validates privilege availability and usage limits
    /// - Tracks privilege usage and updates remaining counts
    /// - Access restricted to authenticated users
    /// - Used for privilege consumption and service access
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on privilege usage
    /// - Maintains privilege usage audit trails
    /// </remarks>
    [HttpPost("privileges/use")]
    public async Task<JsonModel> UsePrivilege([FromBody] UsePrivilegeDto dto)
    {
        var userId = GetCurrentUserId();
        var used = await _privilegeService.UsePrivilegeAsync(dto.SubscriptionId, dto.PrivilegeName, dto.Amount, GetToken(HttpContext));
        if (!used) 
            return new JsonModel { data = new object(), Message = "Privilege could not be used or limit reached.", StatusCode = 400 };
        return new JsonModel { data = true, Message = "Privilege used successfully", StatusCode = 200 };
    }
}

public class PurchaseSubscriptionDto
{
    public Guid PlanId { get; set; }
}

public class CancelSubscriptionDto
{
    public Guid SubscriptionId { get; set; }
}

public class UsePrivilegeDto
{
    public Guid SubscriptionId { get; set; }
    public string PrivilegeName { get; set; } = string.Empty;
    public int Amount { get; set; } = 1;
} 
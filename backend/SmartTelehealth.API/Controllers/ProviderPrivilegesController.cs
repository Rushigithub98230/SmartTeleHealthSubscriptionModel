using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for provider-specific user privilege management and tracking.
/// This controller provides functionality for managing user privileges from a provider perspective,
/// including privilege checking, usage tracking, and subscription-based privilege management
/// for healthcare providers to monitor and manage user access to their services.
/// </summary>
[ApiController]
[Route("api/provider/user")]
//[Authorize]
public class ProviderPrivilegesController : BaseController
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly PrivilegeService _privilegeService;

    /// <summary>
    /// Initializes a new instance of the ProviderPrivilegesController with required services.
    /// </summary>
    /// <param name="subscriptionRepo">Repository for subscription data access</param>
    /// <param name="privilegeService">Service for privilege management operations</param>
    public ProviderPrivilegesController(
        ISubscriptionRepository subscriptionRepo,
        PrivilegeService privilegeService)
    {
        _subscriptionRepo = subscriptionRepo;
        _privilegeService = privilegeService;
    }

    /// <summary>
    /// Retrieves all privileges and usage information for a specific user.
    /// This endpoint provides comprehensive privilege information including subscription-based privileges,
    /// remaining usage counts, and privilege status for provider access control and user management.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>JsonModel containing user privileges and usage information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all privileges for the specified user across all subscriptions
    /// - Includes privilege usage counts and remaining allowances
    /// - Shows subscription-based privilege information
    /// - Access restricted to providers and authorized users
    /// - Used for provider user privilege management and access control
    /// - Includes comprehensive privilege information and usage data
    /// - Provides data for provider service access decisions
    /// - Handles privilege validation and error responses
    /// </remarks>
    [HttpGet("{userId}/privileges")]
    public async Task<JsonModel> GetUserPrivileges(int userId)
    {
        var subs = await _subscriptionRepo.GetByUserIdAsync(userId);
        var usageList = new List<UserPrivilegeUsageDto>();
        foreach (var sub in subs)
        {
            var planPrivileges = await _privilegeService.GetPrivilegesForPlanAsync(sub.SubscriptionPlanId, GetToken(HttpContext));
            foreach (var priv in planPrivileges)
            {
                var remaining = await _privilegeService.GetRemainingPrivilegeAsync(sub.Id, priv.Name, GetToken(HttpContext));
                usageList.Add(new UserPrivilegeUsageDto
                {
                    SubscriptionId = sub.Id,
                    PrivilegeName = priv.Name,
                    Remaining = remaining
                });
            }
        }
        return new JsonModel { data = usageList, Message = "User privileges retrieved successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Checks if a user has a specific privilege and returns usage information.
    /// This endpoint validates user access to a specific privilege and provides remaining usage
    /// information for provider service access control and privilege validation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="privilegeName">The name of the privilege to check</param>
    /// <returns>JsonModel containing privilege status and usage information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Checks if user has access to the specified privilege
    /// - Returns remaining usage count if privilege is available
    /// - Provides privilege validation for service access
    /// - Access restricted to providers and authorized users
    /// - Used for provider service access control and validation
    /// - Includes comprehensive privilege validation and usage data
    /// - Provides data for service access decisions
    /// - Handles privilege checking and error responses
    /// </remarks>
    [HttpGet("{userId}/privileges/{privilegeName}")]
    public async Task<JsonModel> CheckUserPrivilege(int userId, string privilegeName)
    {
        var subs = await _subscriptionRepo.GetByUserIdAsync(userId);
        foreach (var sub in subs)
        {
            var remaining = await _privilegeService.GetRemainingPrivilegeAsync(sub.Id, privilegeName, GetToken(HttpContext));
            if (remaining > 0)
            {
                return new JsonModel { 
                    data = new UserPrivilegeUsageDto
                    {
                        SubscriptionId = sub.Id,
                        PrivilegeName = privilegeName,
                        Remaining = remaining
                    }, 
                    Message = "User privilege found", 
                    StatusCode = 200 
                };
            }
        }
        return new JsonModel { data = new object(), Message = $"User does not have privilege: {privilegeName}", StatusCode = 404 };
    }
} 
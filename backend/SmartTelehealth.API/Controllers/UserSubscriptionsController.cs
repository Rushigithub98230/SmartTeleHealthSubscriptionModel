using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Security.Claims;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/user/[controller]")]
[Authorize]
public class UserSubscriptionsController : BaseController
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly PrivilegeService _privilegeService;
    private readonly SubscriptionService _subscriptionService;

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

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("subscriptions")]
    public async Task<JsonModel> GetUserSubscriptions()
    {
        var userId = GetCurrentUserId();
        return await _subscriptionService.GetUserSubscriptionsAsync(userId, GetToken(HttpContext));
    }

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

    [HttpPost("subscriptions/cancel")]
    public async Task<JsonModel> CancelSubscription([FromBody] CancelSubscriptionDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _subscriptionService.CancelSubscriptionAsync(dto.SubscriptionId.ToString(), null, GetToken(HttpContext));
        if (result.StatusCode != 200) 
            return new JsonModel { data = new object(), Message = result.Message, StatusCode = result.StatusCode };
        return result;
    }

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
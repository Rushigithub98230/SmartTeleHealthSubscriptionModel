using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/provider/user")]
[Authorize]
public class ProviderPrivilegesController : BaseController
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly PrivilegeService _privilegeService;

    public ProviderPrivilegesController(
        ISubscriptionRepository subscriptionRepo,
        PrivilegeService privilegeService)
    {
        _subscriptionRepo = subscriptionRepo;
        _privilegeService = privilegeService;
    }

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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class SubscriptionPlansController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionPlansController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Get all subscription plans (admin only)
    /// </summary>
    [HttpGet]
    public async Task<JsonModel> GetAllPlans()
    {
        return await _subscriptionService.GetAllSubscriptionPlansAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get active subscription plans (public)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<JsonModel> GetActivePlans()
    {
        return await _subscriptionService.GetActiveSubscriptionPlansAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription plans by category (public)
    /// </summary>
    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<JsonModel> GetPlansByCategory(string categoryId)
    {
        return await _subscriptionService.GetSubscriptionPlansByCategoryAsync(categoryId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get a specific subscription plan by ID (public)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<JsonModel> GetPlan(string id)
    {
        return await _subscriptionService.GetSubscriptionPlanAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Create a new subscription plan (admin only)
    /// </summary>
    [HttpPost]
    public async Task<JsonModel> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        return await _subscriptionService.CreatePlanAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Update a subscription plan (admin only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        if (id != updateDto.Id)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        updateDto.Id = id;
        return await _subscriptionService.UpdatePlanAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete a subscription plan (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<JsonModel> DeletePlan(string id)
    {
        return await _subscriptionService.DeleteSubscriptionPlanAsync(id, GetToken(HttpContext));
    }
} 
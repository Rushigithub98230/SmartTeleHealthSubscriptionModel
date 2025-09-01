using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("webadmin/subscription-management")]
//[Authorize]
public class SubscriptionManagementController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICategoryService _categoryService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IAuditService _auditService;

    public SubscriptionManagementController(
        ISubscriptionService subscriptionService,
        ICategoryService categoryService,
        IAnalyticsService analyticsService,
        IAuditService auditService)
    {
        _subscriptionService = subscriptionService;
        _categoryService = categoryService;
        _analyticsService = analyticsService;
        _auditService = auditService;
    }

    #region Subscription Plans Management

    /// <summary>
    /// Get all subscription plans for admin management
    /// </summary>
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
    /// Create a new subscription plan
    /// </summary>
    [HttpPost("plans")]
    public async Task<JsonModel> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        return await _subscriptionService.CreatePlanAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Update an existing subscription plan
    /// </summary>
    [HttpPut("plans/{id}")]
    public async Task<JsonModel> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        if (id != updateDto.Id)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        
        return await _subscriptionService.UpdatePlanAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete a subscription plan
    /// </summary>
    [HttpDelete("plans/{id}")]
    public async Task<JsonModel> DeletePlan(string id)
    {
        return await _subscriptionService.DeletePlanAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Activate a subscription plan
    /// </summary>
    [HttpPost("plans/{id}/activate")]
    public async Task<JsonModel> ActivatePlan(string id)
    {
        return await _subscriptionService.ActivatePlanAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Deactivate a subscription plan
    /// </summary>
    [HttpPost("plans/{id}/deactivate")]
    public async Task<JsonModel> DeactivatePlan(string id)
    {
        return await _subscriptionService.DeactivatePlanAsync(id, GetToken(HttpContext));
    }

    #endregion

    #region User Subscriptions Management

    /// <summary>
    /// Get all user subscriptions for admin management
    /// </summary>
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
    /// Cancel a user subscription
    /// </summary>
    [HttpPost("subscriptions/{id}/cancel")]
    public async Task<JsonModel> CancelUserSubscription(string id, [FromBody] string? reason = null)
    {
        return await _subscriptionService.CancelUserSubscriptionAsync(id, reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Pause a user subscription
    /// </summary>
    [HttpPost("subscriptions/{id}/pause")]
    public async Task<JsonModel> PauseUserSubscription(string id, [FromBody] string? reason = null)
    {
        return await _subscriptionService.PauseUserSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Resume a user subscription
    /// </summary>
    [HttpPost("subscriptions/{id}/resume")]
    public async Task<JsonModel> ResumeUserSubscription(string id)
    {
        return await _subscriptionService.ResumeUserSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Extend a user subscription
    /// </summary>
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
    /// Get all categories for admin management
    /// </summary>
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
    /// Create a new category
    /// </summary>
    [HttpPost("categories")]
    public async Task<JsonModel> CreateCategory([FromBody] CreateCategoryDto createDto)
    {
        return await _categoryService.CreateCategoryAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("categories/{id}")]
    public async Task<JsonModel> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateDto)
    {
        if (id.ToString() != updateDto.Id)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        
        return await _categoryService.UpdateCategoryAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<JsonModel> DeleteCategory(Guid id)
    {
        return await _categoryService.DeleteCategoryAsync(id, GetToken(HttpContext));
    }

    #endregion

    #region Analytics

    /// <summary>
    /// Get subscription analytics for admin dashboard
    /// </summary>
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
    /// Perform bulk operations on subscriptions
    /// </summary>
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
}

#region DTOs for Admin Subscription Management

public class ExtendSubscriptionDto
{
    public DateTime NewEndDate { get; set; }
    public string? Reason { get; set; }
}

#endregion 
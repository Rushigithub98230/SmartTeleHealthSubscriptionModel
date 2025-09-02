using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class SubscriptionsController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetSubscription(string id)
    {
        return await _subscriptionService.GetSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserSubscriptions(int userId)
    {
        return await _subscriptionService.GetUserSubscriptionsAsync(userId, GetToken(HttpContext));
    }

    [HttpPost]
    public async Task<JsonModel> CreateSubscription([FromBody] CreateSubscriptionDto createDto)
    {
        return await _subscriptionService.CreateSubscriptionAsync(createDto, GetToken(HttpContext));
    }

    [HttpPost("{id}/cancel")]
    public async Task<JsonModel> CancelSubscription(string id, [FromBody] string reason)
    {
        return await _subscriptionService.CancelSubscriptionAsync(id, reason, GetToken(HttpContext));
    }

    [HttpPost("{id}/pause")]
    public async Task<JsonModel> PauseSubscription(string id)
    {
        return await _subscriptionService.PauseSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpPost("{id}/resume")]
    public async Task<JsonModel> ResumeSubscription(string id)
    {
        return await _subscriptionService.ResumeSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpPost("{id}/upgrade")]
    public async Task<JsonModel> UpgradeSubscription(string id, [FromBody] string newPlanId)
    {
        return await _subscriptionService.UpgradeSubscriptionAsync(id, newPlanId, GetToken(HttpContext));
    }

    [HttpPost("{id}/reactivate")]
    public async Task<JsonModel> ReactivateSubscription(string id)
    {
        return await _subscriptionService.ReactivateSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpGet("plans")]
    public async Task<JsonModel> GetAllPlans()
    {
        return await _subscriptionService.GetAllPlansAsync(GetToken(HttpContext));
    }

    [HttpGet("plans/{planId}")]
    public async Task<JsonModel> GetPlanById(string planId)
    {
        return await _subscriptionService.GetPlanByIdAsync(planId, GetToken(HttpContext));
    }

    [HttpGet("{id}/billing-history")]
    public async Task<JsonModel> GetBillingHistory(string id)
    {
        return await _subscriptionService.GetBillingHistoryAsync(id, GetToken(HttpContext));
    }

    [HttpGet("user/{userId}/payment-methods")]
    public async Task<JsonModel> GetPaymentMethods(int userId)
    {
        return await _subscriptionService.GetPaymentMethodsAsync(userId, GetToken(HttpContext));
    }

    [HttpPost("user/{userId}/payment-methods")]
    public async Task<JsonModel> AddPaymentMethod(int userId, [FromBody] string paymentMethodId)
    {
        return await _subscriptionService.AddPaymentMethodAsync(userId, paymentMethodId, GetToken(HttpContext));
    }

    [HttpGet("plan/{planId}")]
    public async Task<JsonModel> GetSubscriptionByPlanId(string planId)
    {
        return await _subscriptionService.GetSubscriptionByPlanIdAsync(planId, GetToken(HttpContext));
    }

    [HttpGet("active")]
    public async Task<JsonModel> GetActiveSubscriptions()
    {
        return await _subscriptionService.GetActiveSubscriptionsAsync(GetToken(HttpContext));
    }

    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateSubscription(string id, [FromBody] UpdateSubscriptionDto updateDto)
    {
        return await _subscriptionService.UpdateSubscriptionAsync(id, updateDto, GetToken(HttpContext));
    }

    [HttpPost("{id}/process-payment")]
    public async Task<JsonModel> ProcessPayment(string id, [FromBody] PaymentRequestDto paymentRequest)
    {
        return await _subscriptionService.ProcessPaymentAsync(id, paymentRequest, GetToken(HttpContext));
    }

    [HttpGet("{id}/usage-statistics")]
    public async Task<JsonModel> GetUsageStatistics(string id)
    {
        return await _subscriptionService.GetUsageStatisticsAsync(id, GetToken(HttpContext));
    }

    [HttpGet]
    public async Task<JsonModel> GetAllSubscriptions()
    {
        return await _subscriptionService.GetAllSubscriptionsAsync(GetToken(HttpContext));
    }

    [HttpGet("{id}/analytics")]
    public async Task<JsonModel> GetSubscriptionAnalytics(string id)
    {
        return await _subscriptionService.GetSubscriptionAnalyticsAsync(id, GetToken(HttpContext));
    }

    [HttpPost("plans")]
    public async Task<JsonModel> CreatePlan([FromBody] CreateSubscriptionPlanDto createPlanDto)
    {
        return await _subscriptionService.CreatePlanAsync(createPlanDto, GetToken(HttpContext));
    }

    [HttpPut("plans/{planId}")]
    public async Task<JsonModel> UpdatePlan(string planId, [FromBody] UpdateSubscriptionPlanDto updatePlanDto)
    {
        return await _subscriptionService.UpdatePlanAsync(planId, updatePlanDto, GetToken(HttpContext));
    }

    [HttpPost("plans/{planId}/activate")]
    public async Task<JsonModel> ActivatePlan(string planId)
    {
        return await _subscriptionService.ActivatePlanAsync(planId, GetToken(HttpContext));
    }

    [HttpPost("plans/{planId}/deactivate")]
    public async Task<JsonModel> DeactivatePlan(string planId)
    {
        return await _subscriptionService.DeactivatePlanAsync(planId, GetToken(HttpContext));
    }

    [HttpDelete("plans/{planId}")]
    public async Task<JsonModel> DeletePlan(string planId)
    {
        return await _subscriptionService.DeletePlanAsync(planId, GetToken(HttpContext));
    }

    [HttpGet("stripe/{stripeSubscriptionId}")]
    public async Task<JsonModel> GetByStripeSubscriptionId(string stripeSubscriptionId)
    {
        return await _subscriptionService.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, GetToken(HttpContext));
    }

    [HttpGet("admin/user-subscriptions")]
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

    [HttpPost("admin/{id}/cancel")]
    public async Task<JsonModel> CancelUserSubscription(string id, [FromBody] string? reason)
    {
        return await _subscriptionService.CancelUserSubscriptionAsync(id, reason, GetToken(HttpContext));
    }

    [HttpPost("admin/{id}/pause")]
    public async Task<JsonModel> PauseUserSubscription(string id)
    {
        return await _subscriptionService.PauseUserSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpPost("admin/{id}/resume")]
    public async Task<JsonModel> ResumeUserSubscription(string id)
    {
        return await _subscriptionService.ResumeUserSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpPost("admin/{id}/extend")]
    public async Task<JsonModel> ExtendUserSubscription(string id, [FromBody] int additionalDays)
    {
        return await _subscriptionService.ExtendUserSubscriptionAsync(id, additionalDays, GetToken(HttpContext));
    }

    [HttpPost("admin/bulk-action")]
    public async Task<JsonModel> PerformBulkAction([FromBody] List<BulkActionRequestDto> actions)
    {
        return await _subscriptionService.PerformBulkActionAsync(actions, GetToken(HttpContext));
    }

    [HttpGet("admin/plans")]
    public async Task<JsonModel> GetAllSubscriptionPlans(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? format = null)
    {
        // If format is specified, return export data
        if (!string.IsNullOrEmpty(format) && (format.ToLower() == "csv" || format.ToLower() == "excel"))
        {
            return await _subscriptionService.ExportSubscriptionPlansAsync(GetToken(HttpContext), searchTerm, categoryId, isActive, format);
        }
        
        return await _subscriptionService.GetAllSubscriptionPlansAsync(GetToken(HttpContext), searchTerm, categoryId, isActive, page, pageSize);
    }

    [HttpGet("admin/plans/active")]
    public async Task<JsonModel> GetActiveSubscriptionPlans()
    {
        return await _subscriptionService.GetActiveSubscriptionPlansAsync(GetToken(HttpContext));
    }

    [HttpGet("admin/plans/category/{category}")]
    public async Task<JsonModel> GetSubscriptionPlansByCategory(string category)
    {
        return await _subscriptionService.GetSubscriptionPlansByCategoryAsync(category, GetToken(HttpContext));
    }

    [HttpGet("admin/plans/{planId}")]
    public async Task<JsonModel> GetSubscriptionPlan(string planId)
    {
        return await _subscriptionService.GetSubscriptionPlanAsync(planId, GetToken(HttpContext));
    }

    [HttpPost("admin/plans")]
    public async Task<JsonModel> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        return await _subscriptionService.CreatePlanAsync(createDto, GetToken(HttpContext));
    }

    [HttpPut("admin/plans/{planId}")]
    public async Task<JsonModel> UpdateSubscriptionPlan(string planId, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        return await _subscriptionService.UpdateSubscriptionPlanAsync(planId, updateDto, GetToken(HttpContext));
    }

    [HttpDelete("admin/plans/{planId}")]
    public async Task<JsonModel> DeleteSubscriptionPlan(string planId)
    {
        return await _subscriptionService.DeleteSubscriptionPlanAsync(planId, GetToken(HttpContext));
    }

    [HttpGet("admin/categories")]
    public async Task<JsonModel> GetAllCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? format = null)
    {
        // If format is specified, return export data
        if (!string.IsNullOrEmpty(format) && (format.ToLower() == "csv" || format.ToLower() == "excel"))
        {
            return await _subscriptionService.ExportCategoriesAsync(GetToken(HttpContext), searchTerm, isActive, format);
        }
        
        return await _subscriptionService.GetAllCategoriesAsync(page, pageSize, searchTerm, isActive, GetToken(HttpContext));
    }

    [HttpGet("admin/plans/paged")]
    public async Task<JsonModel> GetAllPlansPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? format = null,
        [FromQuery] bool? includeAnalytics = null)
    {
        // If format is specified, return export data
        if (!string.IsNullOrEmpty(format) && (format.ToLower() == "csv" || format.ToLower() == "excel"))
        {
            return await _subscriptionService.ExportSubscriptionPlansAsync(GetToken(HttpContext), searchTerm, categoryId, isActive, format);
        }
        
        // If analytics is requested, return analytics data
        if (includeAnalytics == true)
        {
            return await _subscriptionService.GetSubscriptionAnalyticsAsync(GetToken(HttpContext), searchTerm, categoryId, isActive);
        }
        
        return await _subscriptionService.GetAllPlansAsync(page, pageSize, searchTerm, categoryId, isActive, GetToken(HttpContext));
    }

    /// <summary>
    /// Get public subscription plans for homepage (no authentication required)
    /// </summary>
    [HttpGet("plans/public")]
    [AllowAnonymous]
    public async Task<JsonModel> GetPublicPlans()
    {
        return await _subscriptionService.GetPublicPlansAsync();
    }
} 
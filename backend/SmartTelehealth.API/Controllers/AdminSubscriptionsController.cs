using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive administrative subscription management operations.
/// This controller consolidates all admin subscription functionality including user subscription management,
/// subscription plan management, automation controls, analytics access, and bulk operations.
/// It serves as the central hub for administrators to manage and monitor all subscription operations.
/// </summary>
[ApiController]
[Route("api/admin/subscriptions")]
[Authorize(Roles = "Admin")]
public class AdminSubscriptionsController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionLifecycleService _subscriptionLifecycleService;
    private readonly ISubscriptionPlanService _subscriptionPlanService;
    private readonly ICategoryService _categoryService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IAuditService _auditService;

    /// <summary>
    /// Initializes a new instance of the AdminSubscriptionsController with required services.
    /// </summary>
    public AdminSubscriptionsController(
        ISubscriptionService subscriptionService,
        ISubscriptionLifecycleService subscriptionLifecycleService,
        ISubscriptionPlanService subscriptionPlanService,
        ICategoryService categoryService,
        IAnalyticsService analyticsService,
        IAuditService auditService)
    {
        _subscriptionService = subscriptionService;
        _subscriptionLifecycleService = subscriptionLifecycleService;
        _subscriptionPlanService = subscriptionPlanService;
        _categoryService = categoryService;
        _analyticsService = analyticsService;
        _auditService = auditService;
    }

    #region User Subscriptions Management

    /// <summary>
    /// Retrieves all user subscriptions with comprehensive filtering and pagination for administrative management.
    /// </summary>
    [HttpGet]
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
    /// Retrieves detailed information about a specific subscription for administrative management.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetSubscriptionDetails(string id)
    {
        return await _subscriptionService.GetSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Cancels a user subscription with optional reason for administrative management.
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<JsonModel> CancelUserSubscription(string id, [FromBody] string? reason = null)
    {
        return await _subscriptionLifecycleService.CancelSubscriptionAsync(id, reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Pauses a user subscription with optional reason for administrative management.
    /// </summary>
    [HttpPost("{id}/pause")]
    public async Task<JsonModel> PauseUserSubscription(string id, [FromBody] string? reason = null)
    {
        return await _subscriptionLifecycleService.PauseSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Resumes a paused user subscription for administrative management.
    /// </summary>
    [HttpPost("{id}/resume")]
    public async Task<JsonModel> ResumeUserSubscription(string id)
    {
        return await _subscriptionLifecycleService.ResumeSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Extends a user subscription duration for administrative management.
    /// </summary>
    [HttpPost("{id}/extend")]
    public async Task<JsonModel> ExtendUserSubscription(string id, [FromBody] int additionalDays)
    {
        return await _subscriptionLifecycleService.ExtendUserSubscriptionAsync(id, additionalDays, GetToken(HttpContext));
    }

    /// <summary>
    /// Upgrades a user subscription to a different plan for administrative management.
    /// </summary>
    [HttpPost("{id}/upgrade")]
    public async Task<JsonModel> UpgradeUserSubscription(string id, [FromBody] string newPlanId)
    {
        return await _subscriptionLifecycleService.UpgradeSubscriptionAsync(id, newPlanId, GetToken(HttpContext));
    }

    /// <summary>
    /// Downgrades a user subscription to a different plan for administrative management.
    /// </summary>
    [HttpPost("{id}/downgrade")]
    public async Task<JsonModel> DowngradeUserSubscription(string id, [FromBody] string newPlanId)
    {
        // Using upgrade method for downgrade - the service will handle the logic
        return await _subscriptionLifecycleService.UpgradeSubscriptionAsync(id, newPlanId, GetToken(HttpContext));
    }

    /// <summary>
    /// Reactivates a cancelled or expired subscription for administrative management.
    /// </summary>
    [HttpPost("{id}/reactivate")]
    public async Task<JsonModel> ReactivateUserSubscription(string id)
    {
        return await _subscriptionLifecycleService.ReactivateSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing subscription for administrative management.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateUserSubscription(string id, [FromBody] UpdateSubscriptionDto updateDto)
    {
        return await _subscriptionLifecycleService.UpdateSubscriptionAsync(id, updateDto, GetToken(HttpContext));
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Performs bulk operations on multiple subscriptions for administrative management.
    /// </summary>
    [HttpPost("bulk-action")]
    public async Task<JsonModel> PerformBulkAction([FromBody] List<BulkActionRequestDto> actions)
    {
        return await _subscriptionLifecycleService.PerformBulkActionAsync(actions, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates status for multiple subscriptions in bulk.
    /// </summary>
    [HttpPost("bulk/status")]
    public async Task<JsonModel> BulkUpdateStatus([FromBody] BulkStatusUpdateDto bulkUpdateDto)
    {
        // For now, return a placeholder - this would need to be implemented
        return new JsonModel 
        { 
            data = new { message = "Bulk status update feature not yet implemented" }, 
            Message = "Bulk status update not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Cancels multiple subscriptions in bulk.
    /// </summary>
    [HttpPost("bulk/cancel")]
    public async Task<JsonModel> BulkCancelSubscriptions([FromBody] BulkCancelDto bulkCancelDto)
    {
        // For now, return a placeholder - this would need to be implemented
        return new JsonModel 
        { 
            data = new { message = "Bulk cancel feature not yet implemented" }, 
            Message = "Bulk cancel not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Sends notifications to multiple subscription users in bulk.
    /// </summary>
    [HttpPost("bulk/notifications")]
    public async Task<JsonModel> BulkSendNotifications([FromBody] BulkNotificationDto bulkNotificationDto)
    {
        // For now, return a placeholder - this would need to be implemented in the notification service
        return new JsonModel 
        { 
            data = new { message = "Bulk notification feature not yet implemented" }, 
            Message = "Bulk notifications not implemented", 
            StatusCode = 501 
        };
    }

    #endregion

    #region Analytics and Reporting

    /// <summary>
    /// Redirects to the dedicated SubscriptionAnalyticsController for comprehensive analytics.
    /// This endpoint provides a redirect to the specialized analytics controller for better organization.
    /// </summary>
    [HttpGet("analytics")]
    public IActionResult RedirectToAnalytics()
    {
        return Redirect("/api/SubscriptionAnalytics");
    }

    /// <summary>
    /// Redirects to the dedicated SubscriptionAnalyticsController for revenue analytics.
    /// This endpoint provides a redirect to the specialized analytics controller for better organization.
    /// </summary>
    [HttpGet("analytics/revenue")]
    public IActionResult RedirectToRevenueAnalytics()
    {
        return Redirect("/api/SubscriptionAnalytics/revenue");
    }

    /// <summary>
    /// Redirects to the dedicated SubscriptionAnalyticsController for churn analytics.
    /// This endpoint provides a redirect to the specialized analytics controller for better organization.
    /// </summary>
    [HttpGet("analytics/churn")]
    public IActionResult RedirectToChurnAnalytics()
    {
        return Redirect("/api/SubscriptionAnalytics/churn");
    }

    /// <summary>
    /// Redirects to the dedicated SubscriptionAnalyticsController for analytics export.
    /// This endpoint provides a redirect to the specialized analytics controller for better organization.
    /// </summary>
    [HttpGet("analytics/export")]
    public IActionResult RedirectToAnalyticsExport()
    {
        return Redirect("/api/SubscriptionAnalytics/export");
    }

    /// <summary>
    /// Redirects to the dedicated SubscriptionAnalyticsController for report generation.
    /// This endpoint provides a redirect to the specialized analytics controller for better organization.
    /// </summary>
    [HttpGet("reports")]
    public IActionResult RedirectToReports()
    {
        return Redirect("/api/SubscriptionAnalytics/reports");
    }

    #endregion

    #region Automation Operations

    /// <summary>
    /// Manually triggers the automated billing process for all eligible subscriptions.
    /// </summary>
    [HttpPost("automation/billing/trigger")]
    public async Task<JsonModel> TriggerAutomatedBilling()
    {
        // For now, return a placeholder - this would need to be implemented
        return new JsonModel 
        { 
            data = new { message = "Automated billing trigger feature not yet implemented" }, 
            Message = "Automated billing trigger not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Manually triggers subscription renewal for a specific subscription.
    /// </summary>
    [HttpPost("automation/renew/{subscriptionId}")]
    public async Task<JsonModel> TriggerSubscriptionRenewal(string subscriptionId)
    {
        // For now, return a placeholder - this would need to be implemented
        return new JsonModel 
        { 
            data = new { message = "Subscription renewal trigger feature not yet implemented" }, 
            Message = "Subscription renewal trigger not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Manually triggers plan change for a specific subscription.
    /// </summary>
    [HttpPost("automation/change-plan/{subscriptionId}")]
    public async Task<JsonModel> TriggerPlanChange(string subscriptionId, [FromBody] string newPlanId)
    {
        // Use the existing upgrade method for plan changes
        return await _subscriptionLifecycleService.UpgradeSubscriptionAsync(subscriptionId, newPlanId, GetToken(HttpContext));
    }

    /// <summary>
    /// Manually triggers state transition for a specific subscription.
    /// </summary>
    [HttpPost("automation/state-transition/{subscriptionId}")]
    public async Task<JsonModel> TriggerStateTransition(string subscriptionId, [FromBody] string newStatus)
    {
        // For now, return a placeholder - this would need to be implemented
        return new JsonModel 
        { 
            data = new { message = "State transition trigger feature not yet implemented" }, 
            Message = "State transition trigger not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Manually triggers subscription expiration for a specific subscription.
    /// </summary>
    [HttpPost("automation/expire/{subscriptionId}")]
    public async Task<JsonModel> TriggerSubscriptionExpiration(string subscriptionId)
    {
        // For now, return a placeholder - this would need to be implemented
        return new JsonModel 
        { 
            data = new { message = "Subscription expiration trigger feature not yet implemented" }, 
            Message = "Subscription expiration trigger not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Manually triggers subscription suspension for a specific subscription.
    /// </summary>
    [HttpPost("automation/suspend/{subscriptionId}")]
    public async Task<JsonModel> TriggerSubscriptionSuspension(string subscriptionId, [FromBody] string reason)
    {
        // Use the existing pause method for suspension
        return await _subscriptionLifecycleService.PauseSubscriptionAsync(subscriptionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves automation status and configuration.
    /// </summary>
    [HttpGet("automation/status")]
    public async Task<JsonModel> GetAutomationStatus()
    {
        // For now, return a placeholder - this would need to be implemented
        return new JsonModel 
        { 
            data = new { message = "Automation status feature not yet implemented" }, 
            Message = "Automation status not implemented", 
            StatusCode = 501 
        };
    }

    /// <summary>
    /// Retrieves automation logs for monitoring and debugging.
    /// </summary>
    [HttpGet("automation/logs")]
    public async Task<JsonModel> GetAutomationLogs(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? level = null)
    {
        // For now, return a placeholder - this would need to be implemented
        return new JsonModel 
        { 
            data = new { message = "Automation logs feature not yet implemented" }, 
            Message = "Automation logs not implemented", 
            StatusCode = 501 
        };
    }

    #endregion

    #region Categories Management

    /// <summary>
    /// Retrieves all categories with comprehensive filtering and export capabilities for administrative management.
    /// </summary>
    [HttpGet("categories")]
    public async Task<JsonModel> GetAllCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? format = null)
    {
        if (!string.IsNullOrEmpty(format) && (format.ToLower() == "csv" || format.ToLower() == "excel"))
        {
            // For now, return a placeholder - this would need to be implemented in the category service
            return new JsonModel 
            { 
                data = new { message = "Category export feature not yet implemented" }, 
                Message = "Category export not implemented", 
                StatusCode = 501 
            };
        }
        
        return await _categoryService.GetAllCategoriesAsync(page, pageSize, searchTerm, isActive, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new category for administrative management.
    /// </summary>
    [HttpPost("categories")]
    public async Task<JsonModel> CreateCategory([FromBody] CreateCategoryDto createDto)
    {
        return await _categoryService.CreateCategoryAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing category for administrative management.
    /// </summary>
    [HttpPut("categories/{id}")]
    public async Task<JsonModel> UpdateCategory(string id, [FromBody] UpdateCategoryDto updateDto)
    {
        return await _categoryService.UpdateCategoryAsync(Guid.Parse(id), updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a category for administrative management.
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<JsonModel> DeleteCategory(string id)
    {
        return await _categoryService.DeleteCategoryAsync(Guid.Parse(id), GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves active categories for administrative management.
    /// </summary>
    [HttpGet("categories/active")]
    public async Task<JsonModel> GetActiveCategories()
    {
        return await _categoryService.GetActiveCategoriesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Searches categories for administrative management.
    /// </summary>
    [HttpGet("categories/search")]
    public async Task<JsonModel> SearchCategories([FromQuery] string searchTerm)
    {
        return await _categoryService.SearchCategoriesAsync(searchTerm, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves subscription plans for a specific category for administrative management.
    /// </summary>
    [HttpGet("categories/{id}/plans")]
    public async Task<JsonModel> GetCategoryPlans(string id)
    {
        // For now, return a placeholder - this would need to be implemented
        return new JsonModel 
        { 
            data = new { message = "Category plans feature not yet implemented" }, 
            Message = "Category plans not implemented", 
            StatusCode = 501 
        };
    }

    #endregion
}

// DTOs for bulk operations
public class BulkStatusUpdateDto
{
    public List<string> SubscriptionIds { get; set; } = new();
    public string NewStatus { get; set; } = string.Empty;
}

public class BulkCancelDto
{
    public List<string> SubscriptionIds { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
}

public class BulkNotificationDto
{
    public List<string> SubscriptionIds { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
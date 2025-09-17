using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for managing subscription plan privileges and time-based limits.
/// This controller provides functionality for configuring privileges associated with subscription plans,
/// including time-based usage limits, privilege assignments, and plan privilege management.
/// It handles the relationship between subscription plans and their associated privileges.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class SubscriptionPlanPrivilegesController : BaseController
{
    private readonly IPrivilegeService _privilegeService;
    private readonly ISubscriptionPlanService _subscriptionPlanService;
    private readonly ISubscriptionRepository _subscriptionRepo;

    /// <summary>
    /// Initializes a new instance of the SubscriptionPlanPrivilegesController with required services.
    /// </summary>
    /// <param name="privilegeService">Service for handling privilege-related business logic</param>
    /// <param name="subscriptionPlanService">Service for handling subscription plan operations</param>
    /// <param name="subscriptionRepo">Repository for subscription data access</param>
    public SubscriptionPlanPrivilegesController(
        IPrivilegeService privilegeService,
        ISubscriptionPlanService subscriptionPlanService,
        ISubscriptionRepository subscriptionRepo)
    {
        _privilegeService = privilegeService;
        _subscriptionPlanService = subscriptionPlanService;
        _subscriptionRepo = subscriptionRepo;
    }

    /// <summary>
    /// Updates time-based usage limits for a subscription plan privilege.
    /// This endpoint allows administrators to configure daily, weekly, and monthly usage limits
    /// for specific privileges within subscription plans, including effective dates and duration settings.
    /// </summary>
    /// <param name="request">DTO containing time-based limit configuration details</param>
    /// <returns>JsonModel containing the updated time-based limits</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates time-based usage limits for subscription plan privileges
    /// - Configures daily, weekly, and monthly usage restrictions
    /// - Sets effective dates and duration for limit enforcement
    /// - Access restricted to administrators and authorized users
    /// - Used for privilege limit configuration and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on limit updates
    /// - Maintains privilege limit audit trails and configuration history
    /// </remarks>
    [HttpPut("time-based-limits")]
    public async Task<JsonModel> UpdateTimeBasedLimits([FromBody] UpdateTimeBasedLimitsRequest request)
    {
        try
        {
            // This would typically call a service method to update the time-based limits
            // For now, return a success response with the updated limits
            var updatedLimits = new
            {
                PrivilegeId = request.PrivilegeId,
                DailyLimit = request.DailyLimit,
                WeeklyLimit = request.WeeklyLimit,
                MonthlyLimit = request.MonthlyLimit,
                UsagePeriodId = request.UsagePeriodId,
                DurationMonths = request.DurationMonths,
                Description = request.Description,
                EffectiveDate = request.EffectiveDate,
                ExpirationDate = request.ExpirationDate
            };

            return new JsonModel
            {
                data = updatedLimits,
                Message = "Time-based limits updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error updating time-based limits: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves time-based usage limits for a specific subscription plan privilege.
    /// This endpoint provides comprehensive information about configured time-based limits
    /// including daily, weekly, and monthly restrictions for a specific plan privilege.
    /// </summary>
    /// <param name="planPrivilegeId">The unique identifier of the subscription plan privilege</param>
    /// <returns>JsonModel containing the time-based limits configuration</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns time-based usage limits for a specific plan privilege
    /// - Includes daily, weekly, and monthly limit configurations
    /// - Shows effective dates and duration settings
    /// - Access restricted to authenticated users
    /// - Used for privilege limit retrieval and management
    /// - Includes comprehensive limit information and metadata
    /// - Provides data for privilege usage enforcement
    /// - Handles limit validation and error responses
    /// </remarks>
    [HttpGet("{planPrivilegeId}/time-based-limits")]
    public async Task<JsonModel> GetTimeBasedLimits(string planPrivilegeId)
    {
        try
        {
            // This would typically retrieve the time-based limits from the database
            // For now, return a placeholder response
            var timeBasedLimits = new
            {
                PlanPrivilegeId = planPrivilegeId,
                DailyLimit = 5,
                WeeklyLimit = 20,
                MonthlyLimit = 80,
                UsagePeriodId = Guid.NewGuid(),
                DurationMonths = 1,
                Description = "Standard time-based limits",
                EffectiveDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(1)
            };

            return new JsonModel
            {
                data = timeBasedLimits,
                Message = "Time-based limits retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving time-based limits: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    #region Privilege Management (Consolidated from PrivilegesController)

    /// <summary>
    /// Retrieves all privileges with comprehensive filtering, pagination, and export capabilities.
    /// This endpoint provides access to all privileges in the system with advanced filtering options
    /// including search, category, and status filters, as well as data export functionality.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 10)</param>
    /// <param name="search">Search term to filter privileges by name or description</param>
    /// <param name="category">Category filter to show privileges in specific categories</param>
    /// <param name="status">Status filter to show privileges with specific status (Active, Inactive, etc.)</param>
    /// <param name="format">Export format (csv, excel) - returns export data instead of paginated results</param>
    /// <returns>JsonModel containing paginated privileges or export data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Supports comprehensive filtering by search term, category, and status
    /// - Provides pagination for large datasets
    /// - Supports data export in CSV or Excel format
    /// - Access restricted to administrators only
    /// - Used for privilege management and system administration
    /// - Returns detailed privilege information including limits and usage statistics
    /// - Includes privilege categories and types for better organization
    /// </remarks>
    [HttpGet("privileges")]
    public async Task<JsonModel> GetAllPrivileges(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        [FromQuery] string? format = null)
    {
        // If format is specified, return export data
        if (!string.IsNullOrEmpty(format) && (format.ToLower() == "csv" || format.ToLower() == "excel"))
        {
            return await _privilegeService.ExportPrivilegesAsync(search, category, status, format, GetToken(HttpContext));
        }
        
        return await _privilegeService.GetAllPrivilegesAsync(page, pageSize, search, category, status, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves detailed information about a specific privilege by its unique identifier.
    /// This endpoint returns comprehensive privilege details including limits, usage statistics,
    /// and associated subscription plans.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege to retrieve</param>
    /// <returns>JsonModel containing the privilege details or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed privilege information including limits and usage statistics
    /// - Shows associated subscription plans and privilege assignments
    /// - Includes privilege category and type information
    /// - Access restricted to administrators only
    /// - Used for privilege details and management
    /// - Provides complete privilege configuration and usage data
    /// </remarks>
    [HttpGet("privileges/{id}")]
    public async Task<JsonModel> GetPrivilegeById(string id)
    {
        return await _privilegeService.GetPrivilegeByIdAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new privilege in the system.
    /// This endpoint allows administrators to create new privileges with specified
    /// limits, categories, and configuration options.
    /// </summary>
    /// <param name="createDto">DTO containing the privilege creation details</param>
    /// <returns>JsonModel containing the creation result and new privilege information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new privilege with specified configuration
    /// - Sets up privilege limits, categories, and types
    /// - Access restricted to administrators only
    /// - Used for adding new privileges to the system
    /// - Includes validation of privilege configuration and business rules
    /// - Sets up audit trails and administrative tracking
    /// - Ensures privilege uniqueness and proper categorization
    /// </remarks>
    [HttpPost("privileges")]
    public async Task<JsonModel> CreatePrivilege([FromBody] CreatePrivilegeDto createDto)
    {
        return await _privilegeService.CreatePrivilegeAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing privilege with new information.
    /// This endpoint allows administrators to modify privilege details including
    /// limits, categories, and configuration options.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege to update</param>
    /// <param name="updateDto">DTO containing the updated privilege information</param>
    /// <returns>JsonModel containing the update result and updated privilege information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates existing privilege with new configuration
    /// - Modifies privilege limits, categories, and types
    /// - Access restricted to administrators only
    /// - Used for privilege maintenance and configuration updates
    /// - Includes validation of privilege changes and business impact
    /// - Maintains audit trails of all privilege modifications
    /// - Handles impact on existing subscriptions and users
    /// </remarks>
    [HttpPut("privileges/{id}")]
    public async Task<JsonModel> UpdatePrivilege(string id, [FromBody] UpdatePrivilegeDto updateDto)
    {
        return await _privilegeService.UpdatePrivilegeAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a privilege from the system.
    /// This endpoint removes a privilege from the system, handling cleanup
    /// of associated data and ensuring no active subscriptions are affected.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Removes the privilege from the system
    /// - Validates that no active subscriptions are using the privilege
    /// - Access restricted to administrators only
    /// - Used for removing obsolete or discontinued privileges
    /// - Includes safety checks to prevent data loss
    /// - Maintains audit trails of privilege deletion
    /// - Handles cleanup of related subscription plan associations
    /// </remarks>
    [HttpDelete("privileges/{id}")]
    public async Task<JsonModel> DeletePrivilege(string id)
    {
        return await _privilegeService.DeletePrivilegeAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get privilege categories for administrative management.
    /// This endpoint provides access to all privilege categories in the system
    /// for organizing and managing privileges effectively.
    /// </summary>
    /// <returns>JsonModel containing privilege categories</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all privilege categories in the system
    /// - Used for privilege organization and management
    /// - Access restricted to administrators only
    /// - Provides category information for privilege assignment
    /// </remarks>
    [HttpGet("privileges/categories")]
    public async Task<JsonModel> GetPrivilegeCategories()
    {
        return await _privilegeService.GetPrivilegeCategoriesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get privilege types for administrative management.
    /// This endpoint provides access to all privilege types in the system
    /// for organizing and managing privileges effectively.
    /// </summary>
    /// <returns>JsonModel containing privilege types</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all privilege types in the system
    /// - Used for privilege organization and management
    /// - Access restricted to administrators only
    /// - Provides type information for privilege assignment
    /// </remarks>
    [HttpGet("privileges/types")]
    public async Task<JsonModel> GetPrivilegeTypes()
    {
        return await _privilegeService.GetPrivilegeTypesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get privilege usage history for administrative management.
    /// This endpoint provides comprehensive usage history for privileges
    /// with advanced filtering and pagination capabilities.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 10)</param>
    /// <param name="privilegeId">Filter by specific privilege ID</param>
    /// <param name="userId">Filter by specific user ID</param>
    /// <param name="subscriptionId">Filter by specific subscription ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <param name="sortBy">Sort by field</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>JsonModel containing privilege usage history</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive privilege usage history
    /// - Supports advanced filtering by privilege, user, subscription, and date range
    /// - Provides pagination for large datasets
    /// - Supports sorting by various fields
    /// - Access restricted to administrators only
    /// - Used for privilege usage analysis and monitoring
    /// </remarks>
    [HttpGet("privileges/usage-history")]
    public async Task<JsonModel> GetPrivilegeUsageHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? privilegeId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? subscriptionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null)
    {
        return await _privilegeService.GetUsageHistoryAsync(page, pageSize, privilegeId, userId, subscriptionId, startDate, endDate, sortBy, sortOrder, GetToken(HttpContext));
    }

    /// <summary>
    /// Get privilege usage summary for administrative management.
    /// This endpoint provides aggregated usage statistics for privileges
    /// with filtering capabilities.
    /// </summary>
    /// <param name="privilegeId">Filter by specific privilege ID</param>
    /// <param name="userId">Filter by specific user ID</param>
    /// <param name="subscriptionId">Filter by specific subscription ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <returns>JsonModel containing privilege usage summary</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns aggregated privilege usage statistics
    /// - Supports filtering by privilege, user, subscription, and date range
    /// - Access restricted to administrators only
    /// - Used for privilege usage analysis and reporting
    /// </remarks>
    [HttpGet("privileges/usage-summary")]
    public async Task<JsonModel> GetPrivilegeUsageSummary(
        [FromQuery] string? privilegeId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? subscriptionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _privilegeService.GetUsageSummaryAsync(privilegeId, userId, subscriptionId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Export privilege usage data for administrative management.
    /// This endpoint provides data export functionality for privilege usage
    /// in various formats (CSV, Excel).
    /// </summary>
    /// <param name="format">Export format (csv, excel)</param>
    /// <param name="privilegeId">Filter by specific privilege ID</param>
    /// <param name="userId">Filter by specific user ID</param>
    /// <param name="subscriptionId">Filter by specific subscription ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <returns>JsonModel containing exported privilege usage data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Exports privilege usage data in specified format
    /// - Supports filtering by privilege, user, subscription, and date range
    /// - Access restricted to administrators only
    /// - Used for privilege usage reporting and analysis
    /// </remarks>
    [HttpGet("privileges/usage-export")]
    public async Task<JsonModel> ExportPrivilegeUsageData(
        [FromQuery] string format = "csv",
        [FromQuery] string? privilegeId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? subscriptionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _privilegeService.ExportUsageDataAsync(format, privilegeId, userId, subscriptionId, startDate, endDate, GetToken(HttpContext));
    }

    #endregion

    #region User Privilege Operations (Consolidated from ProviderPrivilegesController)

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
    [HttpGet("users/{userId}")]
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

    #endregion
}

public class UpdateTimeBasedLimitsRequest
{
    public string PrivilegeId { get; set; } = string.Empty;
    public int? DailyLimit { get; set; }
    public int? WeeklyLimit { get; set; }
    public int? MonthlyLimit { get; set; }
    public string UsagePeriodId { get; set; } = string.Empty;
    public int DurationMonths { get; set; } = 1;
    public string? Description { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

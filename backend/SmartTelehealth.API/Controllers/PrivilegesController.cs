using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for managing privileges and privilege usage tracking.
/// This controller provides comprehensive privilege management functionality including
/// creating, updating, and deleting privileges, as well as tracking privilege usage
/// across users and subscriptions. It supports advanced filtering, analytics, and export capabilities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class PrivilegesController : BaseController
{
    private readonly IPrivilegeService _privilegeService;

    /// <summary>
    /// Initializes a new instance of the PrivilegesController with the required privilege service.
    /// </summary>
    /// <param name="privilegeService">Service for handling privilege-related business logic</param>
    public PrivilegesController(IPrivilegeService privilegeService)
    {
        _privilegeService = privilegeService;
    }

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
    [HttpGet]
    public async Task<JsonModel> GetAll(
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
    [HttpGet("{id}")]
    public async Task<JsonModel> GetById(string id)
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
    [HttpPost]
    public async Task<JsonModel> Create([FromBody] CreatePrivilegeDto createDto)
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
    [HttpPut("{id}")]
    public async Task<JsonModel> Update(string id, [FromBody] UpdatePrivilegeDto updateDto)
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
    [HttpDelete("{id}")]
    public async Task<JsonModel> Delete(string id)
    {
        return await _privilegeService.DeletePrivilegeAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get privilege categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<JsonModel> GetCategories()
    {
        return await _privilegeService.GetPrivilegeCategoriesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get privilege types
    /// </summary>
    [HttpGet("types")]
    public async Task<JsonModel> GetTypes()
    {
        return await _privilegeService.GetPrivilegeTypesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get privilege usage history
    /// </summary>
    [HttpGet("usage-history")]
    public async Task<JsonModel> GetUsageHistory(
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
    /// Get privilege usage summary
    /// </summary>
    [HttpGet("usage-summary")]
    public async Task<JsonModel> GetUsageSummary(
        [FromQuery] string? privilegeId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? subscriptionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _privilegeService.GetUsageSummaryAsync(privilegeId, userId, subscriptionId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Export privilege usage data
    /// </summary>
    [HttpGet("usage-export")]
    public async Task<JsonModel> ExportUsageData(
        [FromQuery] string format = "csv",
        [FromQuery] string? privilegeId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? subscriptionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _privilegeService.ExportUsageDataAsync(format, privilegeId, userId, subscriptionId, startDate, endDate, GetToken(HttpContext));
    }
} 
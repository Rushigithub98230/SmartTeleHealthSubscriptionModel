using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrivilegesController : BaseController
{
    private readonly IPrivilegeService _privilegeService;

    public PrivilegesController(IPrivilegeService privilegeService)
    {
        _privilegeService = privilegeService;
    }

    /// <summary>
    /// Get all privileges with pagination and filtering
    /// </summary>
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
    /// Get privilege by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetById(string id)
    {
        return await _privilegeService.GetPrivilegeByIdAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Create new privilege
    /// </summary>
    [HttpPost]
    public async Task<JsonModel> Create([FromBody] CreatePrivilegeDto createDto)
    {
        return await _privilegeService.CreatePrivilegeAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Update privilege
    /// </summary>
    [HttpPut("{id}")]
    public async Task<JsonModel> Update(string id, [FromBody] UpdatePrivilegeDto updateDto)
    {
        return await _privilegeService.UpdatePrivilegeAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete privilege
    /// </summary>
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
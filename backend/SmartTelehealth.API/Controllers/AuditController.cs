using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class AuditController : BaseController
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<JsonModel> GetAllAuditLogs(
        [FromQuery] string? action = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var tokenModel = GetToken(HttpContext);
        int? parsedUserId = null;
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int parsedId))
        {
            parsedUserId = parsedId;
        }
        var response = await _auditService.GetAuditLogsByDateRangeAsync(startDate ?? DateTime.MinValue, endDate ?? DateTime.MaxValue, tokenModel);
        return response;
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetAuditLog(int id)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetAuditLogByIdAsync(id, tokenModel);
        return response;
    }

    [HttpGet("database/{tableName}")]
    public async Task<JsonModel> GetDatabaseAuditTrail(string tableName, [FromQuery] string? entityId = null)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetDatabaseAuditTrailAsync(tableName, entityId, tokenModel);
        return response;
    }

    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserAuditTrail(int userId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetUserDatabaseAuditTrailAsync(userId, fromDate, toDate, tokenModel);
        return response;
    }

    [HttpGet("entity/{tableName}/{entityId}")]
    public async Task<JsonModel> GetEntityChangeHistory(string tableName, string entityId)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetEntityChangeHistoryAsync(tableName, entityId, tokenModel);
        return response;
    }

    [HttpGet("statistics")]
    public async Task<JsonModel> GetAuditStatistics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetAuditStatisticsAsync(fromDate, toDate, tokenModel);
        return response;
    }

    [HttpGet("recent")]
    public async Task<JsonModel> GetRecentChanges([FromQuery] int count = 50)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetRecentDatabaseChangesAsync(count, tokenModel);
        return response;
    }
} 
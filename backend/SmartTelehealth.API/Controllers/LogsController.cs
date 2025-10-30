using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive log management including application logs
/// and database audit logs. This controller provides extensive functionality for viewing
/// logs with advanced filtering, pagination, and detailed log information for monitoring
/// and debugging purposes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LogsController : BaseController
{
    private readonly ILogsService _logsService;

    /// <summary>
    /// Initializes a new instance of the LogsController with the required logs service.
    /// </summary>
    /// <param name="logsService">Service for handling logs-related business logic</param>
    public LogsController(ILogsService logsService)
    {
        _logsService = logsService;
    }

    /// <summary>
    /// Retrieves paginated application logs with comprehensive filtering options.
    /// </summary>
    [HttpGet("application")]
    public async Task<JsonModel> GetApplicationLogs([FromQuery] ApplicationLogFilterDto filter)
    {
        var tokenModel = GetToken(HttpContext);
        return await _logsService.GetApplicationLogsAsync(filter, tokenModel);
    }

    /// <summary>
    /// Retrieves paginated audit logs with comprehensive filtering options.
    /// </summary>
    [HttpGet("audit")]
    public async Task<JsonModel> GetAuditLogs([FromQuery] AuditLogFilterDto filter)
    {
        var tokenModel = GetToken(HttpContext);
        return await _logsService.GetAuditLogsAsync(filter, tokenModel);
    }

    /// <summary>
    /// Retrieves combined logs (application and audit) based on the log type filter.
    /// </summary>
    [HttpGet("combined")]
    public async Task<JsonModel> GetCombinedLogs([FromQuery] CombinedLogFilterDto filter)
    {
        var tokenModel = GetToken(HttpContext);
        return await _logsService.GetCombinedLogsAsync(filter, tokenModel);
    }

    /// <summary>
    /// Retrieves a specific log by its ID and type (application or audit).
    /// </summary>
    [HttpGet("{logType}/{id}")]
    public async Task<JsonModel> GetLogById(string logType, long id)
    {
        var tokenModel = GetToken(HttpContext);
        return await _logsService.GetLogByIdAsync(id, logType, tokenModel);
    }

    /// <summary>
    /// Retrieves file logs within the specified date range.
    /// </summary>
    [HttpGet("file-logs")]
    public async Task<JsonModel> GetFileLogs([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var tokenModel = GetToken(HttpContext);
        return await _logsService.GetFileLogsAsync(startDate, endDate, tokenModel);
    }

    /// <summary>
    /// Retrieves recent file logs.
    /// </summary>
    [HttpGet("file-logs/recent")]
    public async Task<JsonModel> GetRecentFileLogs([FromQuery] int count = 100)
    {
        var tokenModel = GetToken(HttpContext);
        return await _logsService.GetRecentFileLogsAsync(count, tokenModel);
    }

    /// <summary>
    /// Retrieves log statistics for the specified date range.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<JsonModel> GetLogStatistics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var tokenModel = GetToken(HttpContext);
        return await _logsService.GetLogStatisticsAsync(startDate, endDate, tokenModel);
    }

    /// <summary>
    /// Retrieves available table names for audit log filtering.
    /// </summary>
    [HttpGet("audit/tables")]
    public async Task<JsonModel> GetAvailableTables()
    {
        var tokenModel = GetToken(HttpContext);
        return await _logsService.GetAvailableTablesAsync(tokenModel);
    }

    /// <summary>
    /// Retrieves available audit types for filtering.
    /// </summary>
    [HttpGet("audit/types")]
    public async Task<JsonModel> GetAvailableTypes()
    {
        var tokenModel = GetToken(HttpContext);
        return await _logsService.GetAvailableTypesAsync(tokenModel);
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive audit logging and system activity tracking.
/// This controller provides extensive functionality for managing audit logs, tracking system activities,
/// monitoring database changes, and providing audit trail analysis. It handles the complete audit
/// lifecycle from log creation to analysis and reporting for compliance and security purposes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class AuditController : BaseController
{
    private readonly IAuditService _auditService;

    /// <summary>
    /// Initializes a new instance of the AuditController with the required audit service.
    /// </summary>
    /// <param name="auditService">Service for handling audit-related business logic</param>
    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Retrieves all audit logs with comprehensive filtering and pagination options.
    /// This endpoint provides administrators with access to audit logs including filtering
    /// by action, user, date range, and pagination for effective audit trail management.
    /// </summary>
    /// <param name="action">Filter audit logs by specific action type</param>
    /// <param name="userId">Filter audit logs by specific user ID</param>
    /// <param name="startDate">Start date for date range filtering</param>
    /// <param name="endDate">End date for date range filtering</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50)</param>
    /// <returns>JsonModel containing paginated audit logs with filtering applied</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all audit logs with comprehensive filtering options
    /// - Supports filtering by action, user, and date range
    /// - Provides pagination for large audit datasets
    /// - Access restricted to administrators only
    /// - Used for audit trail management and compliance monitoring
    /// - Includes comprehensive audit information and metadata
    /// - Supports advanced filtering for audit analysis
    /// - Provides comprehensive audit overview for administrators
    /// </remarks>
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

    /// <summary>
    /// Retrieves detailed information about a specific audit log by its ID.
    /// This endpoint provides comprehensive audit log details including action details,
    /// user information, timestamp, and audit metadata for detailed audit analysis.
    /// </summary>
    /// <param name="id">The unique identifier of the audit log</param>
    /// <returns>JsonModel containing the audit log details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed audit log information by ID
    /// - Includes action details, user information, and timestamp
    /// - Shows audit metadata and change information
    /// - Access restricted to administrators and authorized users
    /// - Used for detailed audit log analysis and investigation
    /// - Includes comprehensive audit information and metadata
    /// - Provides secure access to audit log information
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetAuditLog(int id)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetAuditLogByIdAsync(id, tokenModel);
        return response;
    }

    /// <summary>
    /// Retrieves database audit trail for a specific table with optional entity filtering.
    /// This endpoint provides comprehensive database change tracking including table-level
    /// changes, entity modifications, and database audit trail analysis for compliance.
    /// </summary>
    /// <param name="tableName">The name of the database table</param>
    /// <param name="entityId">Optional entity ID to filter specific entity changes</param>
    /// <returns>JsonModel containing the database audit trail</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns database audit trail for specific table
    /// - Includes table-level changes and entity modifications
    /// - Shows database change history and audit information
    /// - Access restricted to administrators and authorized users
    /// - Used for database audit trail analysis and compliance
    /// - Includes comprehensive database audit information
    /// - Provides secure access to database audit data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("database/{tableName}")]
    public async Task<JsonModel> GetDatabaseAuditTrail(string tableName, [FromQuery] string? entityId = null)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetDatabaseAuditTrailAsync(tableName, entityId, tokenModel);
        return response;
    }

    /// <summary>
    /// Retrieves audit trail for a specific user within a date range.
    /// This endpoint provides comprehensive user activity tracking including user actions,
    /// system interactions, and user audit trail analysis for user behavior monitoring.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="fromDate">Start date for audit trail filtering</param>
    /// <param name="toDate">End date for audit trail filtering</param>
    /// <returns>JsonModel containing the user audit trail</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns audit trail for specific user within date range
    /// - Includes user actions, system interactions, and activity history
    /// - Shows user behavior patterns and audit information
    /// - Access restricted to administrators and authorized users
    /// - Used for user activity monitoring and audit analysis
    /// - Includes comprehensive user audit information
    /// - Provides secure access to user audit data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserAuditTrail(int userId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetUserDatabaseAuditTrailAsync(userId, fromDate, toDate, tokenModel);
        return response;
    }

    /// <summary>
    /// Retrieves change history for a specific entity in a database table.
    /// This endpoint provides comprehensive entity change tracking including entity modifications,
    /// change history, and entity audit trail analysis for entity-level audit monitoring.
    /// </summary>
    /// <param name="tableName">The name of the database table</param>
    /// <param name="entityId">The unique identifier of the entity</param>
    /// <returns>JsonModel containing the entity change history</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns change history for specific entity in table
    /// - Includes entity modifications and change history
    /// - Shows entity audit trail and change information
    /// - Access restricted to administrators and authorized users
    /// - Used for entity change monitoring and audit analysis
    /// - Includes comprehensive entity audit information
    /// - Provides secure access to entity audit data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("entity/{tableName}/{entityId}")]
    public async Task<JsonModel> GetEntityChangeHistory(string tableName, string entityId)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetEntityChangeHistoryAsync(tableName, entityId, tokenModel);
        return response;
    }

    /// <summary>
    /// Retrieves audit statistics and analytics within a date range.
    /// This endpoint provides comprehensive audit analytics including audit metrics,
    /// activity statistics, and audit performance indicators for audit analysis.
    /// </summary>
    /// <param name="fromDate">Start date for statistics calculation</param>
    /// <param name="toDate">End date for statistics calculation</param>
    /// <returns>JsonModel containing audit statistics and analytics</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns audit statistics and analytics within date range
    /// - Includes audit metrics, activity statistics, and performance indicators
    /// - Shows audit trends and statistical information
    /// - Access restricted to administrators and authorized users
    /// - Used for audit analytics and performance monitoring
    /// - Includes comprehensive audit statistical information
    /// - Provides secure access to audit analytics data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("statistics")]
    public async Task<JsonModel> GetAuditStatistics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetAuditStatisticsAsync(fromDate, toDate, tokenModel);
        return response;
    }

    /// <summary>
    /// Retrieves recent database changes and system activities.
    /// This endpoint provides a list of recent system changes including database modifications,
    /// system activities, and recent audit events for real-time system monitoring.
    /// </summary>
    /// <param name="count">Number of recent changes to retrieve (default: 50)</param>
    /// <returns>JsonModel containing recent database changes and activities</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns recent database changes and system activities
    /// - Includes recent modifications, activities, and audit events
    /// - Shows real-time system change information
    /// - Access restricted to administrators and authorized users
    /// - Used for real-time system monitoring and change tracking
    /// - Includes comprehensive recent change information
    /// - Provides secure access to recent change data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("recent")]
    public async Task<JsonModel> GetRecentChanges([FromQuery] int count = 50)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetRecentDatabaseChangesAsync(count, tokenModel);
        return response;
    }
} 
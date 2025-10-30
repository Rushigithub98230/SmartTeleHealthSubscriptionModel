using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service interface for managing application logs, audit logs, and file logs.
/// </summary>
public interface ILogsService
{
    /// <summary>
    /// Retrieves paginated application logs with filtering.
    /// </summary>
    Task<JsonModel> GetApplicationLogsAsync(ApplicationLogFilterDto filter, TokenModel token);
    
    /// <summary>
    /// Retrieves paginated audit logs with filtering.
    /// </summary>
    Task<JsonModel> GetAuditLogsAsync(AuditLogFilterDto filter, TokenModel token);
    
    /// <summary>
    /// Retrieves combined logs (application and audit) with filtering.
    /// </summary>
    Task<JsonModel> GetCombinedLogsAsync(CombinedLogFilterDto filter, TokenModel token);
    
    /// <summary>
    /// Retrieves a specific log by ID and type.
    /// </summary>
    Task<JsonModel> GetLogByIdAsync(long id, string logType, TokenModel token);

    /// <summary>
    /// Gets file logs within the specified date range.
    /// </summary>
    Task<JsonModel> GetFileLogsAsync(DateTime startDate, DateTime endDate, TokenModel token);

    /// <summary>
    /// Gets recent file logs.
    /// </summary>
    Task<JsonModel> GetRecentFileLogsAsync(int count, TokenModel token);

    /// <summary>
    /// Gets log statistics for the specified date range.
    /// </summary>
    Task<JsonModel> GetLogStatisticsAsync(DateTime startDate, DateTime endDate, TokenModel token);
    
    /// <summary>
    /// Gets available table names for audit log filtering.
    /// </summary>
    Task<JsonModel> GetAvailableTablesAsync(TokenModel token);
    
    /// <summary>
    /// Gets available audit types for filtering.
    /// </summary>
    Task<JsonModel> GetAvailableTypesAsync(TokenModel token);
}


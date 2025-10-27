using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service interface for managing application logs and audit logs.
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
}


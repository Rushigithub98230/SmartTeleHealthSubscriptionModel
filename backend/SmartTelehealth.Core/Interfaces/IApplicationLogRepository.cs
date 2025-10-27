using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

/// <summary>
/// Repository interface for ApplicationLog entity with specialized query methods for log management.
/// </summary>
public interface IApplicationLogRepository : IGenericRepository<ApplicationLog>
{
    /// <summary>
    /// Retrieves paginated application logs with filtering options.
    /// </summary>
    /// <param name="startDate">Start date for filtering logs</param>
    /// <param name="endDate">End date for filtering logs</param>
    /// <param name="logLevel">Filter by log level (Information, Warning, Error, Critical)</param>
    /// <param name="source">Filter by source (service or controller name)</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="searchText">Search text to filter by message content</param>
    /// <param name="page">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <returns>Tuple containing list of logs and total count</returns>
    Task<(List<ApplicationLog> logs, int totalCount)> GetLogsAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? logLevel,
        string? source,
        int? userId,
        string? searchText,
        int page,
        int pageSize);

    /// <summary>
    /// Retrieves recent application logs ordered by timestamp descending.
    /// </summary>
    /// <param name="count">Number of recent logs to retrieve</param>
    /// <returns>List of recent application logs</returns>
    Task<List<ApplicationLog>> GetRecentLogsAsync(int count);

    /// <summary>
    /// Retrieves a specific application log by its ID with related user information.
    /// </summary>
    /// <param name="id">The unique identifier of the application log</param>
    /// <returns>The application log if found, null otherwise</returns>
    Task<ApplicationLog?> GetLogByIdAsync(long id);
}


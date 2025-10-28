namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service interface for reading and parsing Serilog files for admin viewing.
/// This service enables administrators to view logs from Serilog files
/// alongside database logs for comprehensive log monitoring.
/// </summary>
public interface IFileLogReaderService
{
    /// <summary>
    /// Reads log files within the specified date range.
    /// </summary>
    /// <param name="startDate">Start date for filtering logs</param>
    /// <param name="endDate">End date for filtering logs</param>
    /// <returns>List of log entries</returns>
    Task<List<LogEntry>> ReadLogFilesAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Reads recent log entries from the latest log files.
    /// </summary>
    /// <param name="count">Number of recent entries to retrieve</param>
    /// <returns>List of recent log entries</returns>
    Task<List<LogEntry>> ReadRecentLogsAsync(int count);

    /// <summary>
    /// Reads a specific log entry from a file by line number.
    /// </summary>
    /// <param name="filePath">Path to the log file</param>
    /// <param name="lineNumber">Line number of the log entry</param>
    /// <returns>The log entry if found, null otherwise</returns>
    Task<LogEntry?> ReadLogEntryAsync(string filePath, int lineNumber);

    /// <summary>
    /// Gets log statistics for the specified date range.
    /// </summary>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <returns>Log statistics</returns>
    Task<LogStatistics> GetLogStatisticsAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Represents a log entry from a Serilog file.
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Properties { get; set; }
}

/// <summary>
/// Represents log statistics for a date range.
/// </summary>
public class LogStatistics
{
    public int TotalEntries { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, int> LevelCounts { get; set; } = new();
    public Dictionary<string, int> SourceCounts { get; set; } = new();
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
}



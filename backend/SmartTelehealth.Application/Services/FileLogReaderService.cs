using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for reading and parsing Serilog files for admin viewing.
/// This service enables administrators to view logs from Serilog files
/// alongside database logs for comprehensive log monitoring.
/// </summary>
public class FileLogReaderService : IFileLogReaderService
{
    private readonly ILogger<FileLogReaderService> _logger;
    private readonly string _logsDirectory;
    private readonly Regex _logEntryRegex;

    public FileLogReaderService(ILogger<FileLogReaderService> logger)
    {
        _logger = logger;
        _logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        
        // Regex pattern to match Serilog file output format
        // Format: yyyy-MM-dd HH:mm:ss.fff +TZ [LEVEL] message
        _logEntryRegex = new Regex(
            @"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}) \[(\w+)\] (.+)$",
            RegexOptions.Compiled
        );
    }

    /// <summary>
    /// Reads log files within the specified date range.
    /// </summary>
    /// <param name="startDate">Start date for filtering logs</param>
    /// <param name="endDate">End date for filtering logs</param>
    /// <returns>List of log entries</returns>
    public async Task<List<LogEntry>> ReadLogFilesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var logEntries = new List<LogEntry>();
            var logFiles = GetLogFilesInDateRange(startDate, endDate);
            
            _logger.LogInformation("Reading {Count} log files from {StartDate} to {EndDate}", 
                logFiles.Count, startDate, endDate);

            foreach (var file in logFiles)
            {
                var entries = await ReadLogFileAsync(file);
                logEntries.AddRange(entries);
            }

            return logEntries.OrderByDescending(e => e.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading log files from {StartDate} to {EndDate}", startDate, endDate);
            return new List<LogEntry>();
        }
    }

    /// <summary>
    /// Reads recent log entries from the latest log files.
    /// </summary>
    /// <param name="count">Number of recent entries to retrieve</param>
    /// <returns>List of recent log entries</returns>
    public async Task<List<LogEntry>> ReadRecentLogsAsync(int count)
    {
        try
        {
            var logEntries = new List<LogEntry>();
            var logFiles = GetRecentLogFiles(5); // Get last 5 log files

            foreach (var file in logFiles.OrderByDescending(f => f.CreationTime))
            {
                var entries = await ReadLogFileAsync(file);
                logEntries.AddRange(entries);

                if (logEntries.Count >= count)
                    break;
            }

            return logEntries.OrderByDescending(e => e.Timestamp).Take(count).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading recent logs");
            return new List<LogEntry>();
        }
    }

    /// <summary>
    /// Reads a specific log entry from a file by line number.
    /// </summary>
    /// <param name="filePath">Path to the log file</param>
    /// <param name="lineNumber">Line number of the log entry</param>
    /// <returns>The log entry if found, null otherwise</returns>
    public async Task<LogEntry?> ReadLogEntryAsync(string filePath, int lineNumber)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Log file {FilePath} does not exist", filePath);
                return null;
            }

            var lines = await File.ReadAllLinesAsync(filePath);
            if (lineNumber < 1 || lineNumber > lines.Length)
            {
                _logger.LogWarning("Line number {LineNumber} is out of range for file {FilePath}", lineNumber, filePath);
                return null;
            }

            var line = lines[lineNumber - 1];
            if (TryParseLogLine(line, out var logEntry))
            {
                return logEntry;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading log entry from {FilePath} at line {LineNumber}", filePath, lineNumber);
            return null;
        }
    }

    /// <summary>
    /// Gets log statistics for the specified date range.
    /// </summary>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <returns>Log statistics</returns>
    public async Task<LogStatistics> GetLogStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var logEntries = await ReadLogFilesAsync(startDate, endDate);
            
            var stats = new LogStatistics
            {
                TotalEntries = logEntries.Count,
                StartDate = startDate,
                EndDate = endDate,
                LevelCounts = logEntries.GroupBy(e => e.Level)
                    .ToDictionary(g => g.Key, g => g.Count()),
                SourceCounts = logEntries.GroupBy(e => e.Source)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ErrorCount = logEntries.Count(e => e.Level == "Error" || e.Level == "Fatal"),
                WarningCount = logEntries.Count(e => e.Level == "Warning"),
                InfoCount = logEntries.Count(e => e.Level == "Information")
            };
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting log statistics from {StartDate} to {EndDate}", startDate, endDate);
            return new LogStatistics();
        }
    }

    /// <summary>
    /// Gets log files within the specified date range.
    /// </summary>
    private List<FileInfo> GetLogFilesInDateRange(DateTime startDate, DateTime endDate)
    {
        if (!Directory.Exists(_logsDirectory))
        {
            _logger.LogWarning("Logs directory {LogsDirectory} does not exist", _logsDirectory);
            return new List<FileInfo>();
        }

        var logFiles = new List<FileInfo>();
        var directory = new DirectoryInfo(_logsDirectory);

        // Get all log files
        var files = directory.GetFiles("*.log", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            // Parse date from filename (format: audit-yyyyMMdd.log)
            var dateMatch = Regex.Match(file.Name, @"(\d{8})\.log$");
            if (dateMatch.Success)
            {
                if (DateTime.TryParseExact(dateMatch.Groups[1].Value, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var fileDate))
                {
                    // Check if file date is within range (compare dates only, not time)
                    if (fileDate.Date >= startDate.Date && fileDate.Date <= endDate.Date)
                    {
                        logFiles.Add(file);
                    }
                }
            }
            else
            {
                // If filename doesn't match pattern, check LastWriteTime as fallback
                if (file.LastWriteTime.Date >= startDate.Date && file.LastWriteTime.Date <= endDate.Date)
                {
                    logFiles.Add(file);
                }
            }
        }

        return logFiles;
    }

    /// <summary>
    /// Gets recent log files.
    /// </summary>
    private List<FileInfo> GetRecentLogFiles(int count)
    {
        if (!Directory.Exists(_logsDirectory))
        {
            return new List<FileInfo>();
        }

        var directory = new DirectoryInfo(_logsDirectory);
        return directory.GetFiles("*.log", SearchOption.TopDirectoryOnly)
            .OrderByDescending(f => f.CreationTime)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Reads and parses a log file.
    /// </summary>
    private async Task<List<LogEntry>> ReadLogFileAsync(FileInfo file)
    {
        try
        {
            var logEntries = new List<LogEntry>();
            
            // Use FileStream with FileShare.ReadWrite to allow reading while Serilog is writing
            using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (TryParseLogLine(line, out var logEntry))
                    {
                        logEntries.Add(logEntry);
                    }
                }
            }

            return logEntries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading log file {FilePath}", file.FullName);
            return new List<LogEntry>();
        }
    }

    /// <summary>
    /// Attempts to parse a log line into a LogEntry.
    /// </summary>
    private bool TryParseLogLine(string line, out LogEntry logEntry)
    {
        logEntry = new LogEntry();

        try
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var match = _logEntryRegex.Match(line);
            if (!match.Success)
            {
                return false;
            }

            // Parse timestamp (format: 2025-10-28 14:42:19.123 +05:30)
            var timestampStr = match.Groups[1].Value;
            if (DateTime.TryParse(timestampStr, out var timestamp))
            {
                logEntry.Timestamp = timestamp;
            }
            else
            {
                return false;
            }

            // Parse log level
            logEntry.Level = match.Groups[2].Value;

            // Parse message (everything after [LEVEL])
            logEntry.Message = match.Groups[3].Value.Trim();

            // Extract source from message if it contains common patterns
            // e.g., "SmartTelehealth.Application.Services.SubscriptionService: Message"
            var sourceMatch = Regex.Match(logEntry.Message, @"^([\w\.]+):\s*(.+)$");
            if (sourceMatch.Success)
            {
                logEntry.Source = sourceMatch.Groups[1].Value;
                logEntry.Message = sourceMatch.Groups[2].Value;
            }
            else
            {
                logEntry.Source = "Application"; // Default source
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error parsing log line: {Line}", line);
            return false;
        }
    }
}

using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using SmartTelehealth.Infrastructure.Hubs;

namespace SmartTelehealth.Infrastructure.Services;

/// <summary>
/// Service for broadcasting logs to connected admin users in real-time via SignalR.
/// This service enables real-time log monitoring for administrators, allowing them
/// to see system activities as they happen without needing to refresh the page.
/// </summary>
public class RealTimeLogsService : IRealTimeLogsService
{
    private readonly IHubContext<LogsHub> _hubContext;
    private readonly ILogger<RealTimeLogsService> _logger;

    public RealTimeLogsService(
        IHubContext<LogsHub> hubContext,
        ILogger<RealTimeLogsService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Broadcasts an application log to all connected admin users.
    /// </summary>
    /// <param name="log">The application log to broadcast</param>
    public async Task BroadcastApplicationLogAsync(ApplicationLog log)
    {
        try
        {
            await _hubContext.Clients.Group("AdminLogs").SendAsync("ReceiveApplicationLog", log);
            _logger.LogDebug("Application log broadcasted: {LogId}", log.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting application log {LogId}", log.Id);
        }
    }

    /// <summary>
    /// Broadcasts an audit log to all connected admin users.
    /// </summary>
    /// <param name="log">The audit log to broadcast</param>
    public async Task BroadcastAuditLogAsync(AuditLog log)
    {
        try
        {
            await _hubContext.Clients.Group("AdminLogs").SendAsync("ReceiveAuditLog", log);
            _logger.LogDebug("Audit log broadcasted: {LogId}", log.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting audit log {LogId}", log.Id);
        }
    }

    /// <summary>
    /// Broadcasts a system event to all connected admin users.
    /// </summary>
    /// <param name="eventType">The type of system event</param>
    /// <param name="data">The event data</param>
    public async Task BroadcastSystemEventAsync(string eventType, object data)
    {
        try
        {
            await _hubContext.Clients.Group("AdminLogs").SendAsync("ReceiveSystemEvent", eventType, data);
            _logger.LogDebug("System event broadcasted: {EventType}", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting system event {EventType}", eventType);
        }
    }

    /// <summary>
    /// Broadcasts log statistics to all connected admin users.
    /// </summary>
    /// <param name="stats">The log statistics</param>
    public async Task BroadcastLogStatsAsync(object stats)
    {
        try
        {
            await _hubContext.Clients.Group("AdminLogs").SendAsync("ReceiveLogStats", stats);
            _logger.LogDebug("Log statistics broadcasted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting log statistics");
        }
    }

    /// <summary>
    /// Sends a notification to all connected admin users.
    /// </summary>
    /// <param name="title">The notification title</param>
    /// <param name="message">The notification message</param>
    /// <param name="type">The notification type (info, warning, error, success)</param>
    public async Task BroadcastNotificationAsync(string title, string message, string type = "info")
    {
        try
        {
            await _hubContext.Clients.Group("AdminLogs").SendAsync("ReceiveNotification", title, message, type);
            _logger.LogDebug("Notification broadcasted: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification {Title}", title);
        }
    }
}

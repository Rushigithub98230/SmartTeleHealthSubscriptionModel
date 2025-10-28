using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service interface for broadcasting logs to connected admin users in real-time via SignalR.
/// This service enables real-time log monitoring for administrators, allowing them
/// to see system activities as they happen without needing to refresh the page.
/// </summary>
public interface IRealTimeLogsService
{
    /// <summary>
    /// Broadcasts an application log to all connected admin users.
    /// </summary>
    /// <param name="log">The application log to broadcast</param>
    Task BroadcastApplicationLogAsync(ApplicationLog log);

    /// <summary>
    /// Broadcasts an audit log to all connected admin users.
    /// </summary>
    /// <param name="log">The audit log to broadcast</param>
    Task BroadcastAuditLogAsync(AuditLog log);

    /// <summary>
    /// Broadcasts a system event to all connected admin users.
    /// </summary>
    /// <param name="eventType">The type of system event</param>
    /// <param name="data">The event data</param>
    Task BroadcastSystemEventAsync(string eventType, object data);

    /// <summary>
    /// Broadcasts log statistics to all connected admin users.
    /// </summary>
    /// <param name="stats">The log statistics</param>
    Task BroadcastLogStatsAsync(object stats);

    /// <summary>
    /// Sends a notification to all connected admin users.
    /// </summary>
    /// <param name="title">The notification title</param>
    /// <param name="message">The notification message</param>
    /// <param name="type">The notification type (info, warning, error, success)</param>
    Task BroadcastNotificationAsync(string title, string message, string type = "info");
}



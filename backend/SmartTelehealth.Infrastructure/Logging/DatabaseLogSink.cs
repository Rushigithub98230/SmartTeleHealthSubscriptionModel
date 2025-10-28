using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.Interfaces;
using System.Text.Json;

namespace SmartTelehealth.Infrastructure.Logging;

/// <summary>
/// Custom Serilog sink that writes log events to the ApplicationLogs database table
/// and broadcasts them to connected admin users via SignalR.
/// This sink captures all ILogger calls throughout the application and stores them
/// in the database for admin viewing and real-time monitoring.
/// </summary>
public class DatabaseLogSink : ILogEventSink
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseLogSink(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Emits a log event to the database and broadcasts it to admin users.
    /// This method is called by Serilog for every log event in the application.
    /// </summary>
    /// <param name="logEvent">The log event from Serilog</param>
    public void Emit(LogEvent logEvent)
    {
        try
        {
            // Create ApplicationLog entity from Serilog LogEvent
            var applicationLog = new ApplicationLog
            {
                Timestamp = logEvent.Timestamp.DateTime,
                LogLevel = logEvent.Level.ToString(),
                Source = GetSourceFromLogEvent(logEvent),
                Message = logEvent.RenderMessage(),
                Exception = logEvent.Exception?.ToString(),
                UserId = GetUserIdFromLogEvent(logEvent),
                Operation = GetOperationFromLogEvent(logEvent),
                AdditionalData = GetAdditionalDataFromLogEvent(logEvent),
                CorrelationId = GetCorrelationIdFromLogEvent(logEvent)
            };

            // Store in database asynchronously (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    // Create a scope to resolve scoped services
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<IApplicationLogRepository>();
                        var realTimeLogsService = scope.ServiceProvider.GetRequiredService<IRealTimeLogsService>();
                        
                        await repository.AddAsync(applicationLog);
                        await repository.SaveChangesAsync();

                        // Broadcast to admin users via SignalR
                        await realTimeLogsService.BroadcastApplicationLogAsync(applicationLog);
                    }
                }
                catch (Exception ex)
                {
                    // Use console logging to avoid infinite recursion
                    Console.WriteLine($"Error in DatabaseLogSink: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            // Use console logging to avoid infinite recursion
            Console.WriteLine($"Critical error in DatabaseLogSink: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts the source (service/controller name) from the log event.
    /// </summary>
    private string GetSourceFromLogEvent(LogEvent logEvent)
    {
        // Try to get source from properties
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            return sourceContext.ToString().Trim('"');
        }

        // Try to get source from logger name
        if (logEvent.Properties.TryGetValue("SourceContext", out var loggerName))
        {
            var loggerNameStr = loggerName.ToString().Trim('"');
            if (!string.IsNullOrEmpty(loggerNameStr))
            {
                // Extract the last part of the namespace (e.g., "SubscriptionService" from "SmartTelehealth.Application.Services.SubscriptionService")
                var parts = loggerNameStr.Split('.');
                return parts.LastOrDefault() ?? loggerNameStr;
            }
        }

        return "Unknown";
    }

    /// <summary>
    /// Extracts the user ID from the log event properties.
    /// </summary>
    private int? GetUserIdFromLogEvent(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("UserId", out var userId))
        {
            if (int.TryParse(userId.ToString().Trim('"'), out var id))
            {
                return id;
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts the operation type from the log event properties.
    /// </summary>
    private string? GetOperationFromLogEvent(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("Operation", out var operation))
        {
            return operation.ToString().Trim('"');
        }

        // Try to infer operation from message content
        var message = logEvent.RenderMessage();
        if (message.Contains("created", StringComparison.OrdinalIgnoreCase))
            return "Create";
        if (message.Contains("updated", StringComparison.OrdinalIgnoreCase))
            return "Update";
        if (message.Contains("deleted", StringComparison.OrdinalIgnoreCase))
            return "Delete";
        if (message.Contains("retrieved", StringComparison.OrdinalIgnoreCase))
            return "Read";

        return null;
    }

    /// <summary>
    /// Extracts additional contextual data from the log event properties.
    /// </summary>
    private string? GetAdditionalDataFromLogEvent(LogEvent logEvent)
    {
        var additionalData = new Dictionary<string, object>();

        // Extract relevant properties
        foreach (var property in logEvent.Properties)
        {
            var key = property.Key;
            var value = property.Value.ToString().Trim('"');

            // Skip common properties that are already stored separately
            if (key == "SourceContext" || key == "UserId" || key == "Operation" || key == "CorrelationId")
                continue;

            // Skip empty values
            if (string.IsNullOrEmpty(value) || value == "null")
                continue;

            additionalData[key] = value;
        }

        if (additionalData.Count > 0)
        {
            try
            {
                return JsonSerializer.Serialize(additionalData);
            }
            catch
            {
                return additionalData.ToString();
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts the correlation ID from the log event properties.
    /// </summary>
    private string? GetCorrelationIdFromLogEvent(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("CorrelationId", out var correlationId))
        {
            return correlationId.ToString().Trim('"');
        }

        return null;
    }
}

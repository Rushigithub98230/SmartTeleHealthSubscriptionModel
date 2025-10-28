using Microsoft.AspNetCore.SignalR;

namespace SmartTelehealth.Infrastructure.Hubs;

/// <summary>
/// Simple SignalR Hub for broadcasting logs to admin users.
/// This is a minimal implementation to avoid circular dependencies.
/// </summary>
public class LogsHub : Hub
{
    // This hub doesn't need any specific methods since we're only broadcasting
    // The actual hub logic is handled by the API layer's LogsHub
}



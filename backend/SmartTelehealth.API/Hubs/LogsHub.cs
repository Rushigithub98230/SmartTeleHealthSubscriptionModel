using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartTelehealth.Application.DTOs;
using System.Security.Claims;

namespace SmartTelehealth.API.Hubs;

/// <summary>
/// SignalR Hub for broadcasting application logs to admin users in real-time.
/// </summary>
[Authorize(Roles = "Admin")]
public class LogsHub : Hub
{
    private readonly ILogger<LogsHub> _logger;
    private static readonly HashSet<string> _connectedAdmins = new();

    public LogsHub(ILogger<LogsHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            _connectedAdmins.Add(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminLogs");
            _logger.LogInformation("Admin user {UserId} connected to logs hub", userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            _connectedAdmins.Remove(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminLogs");
            _logger.LogInformation("Admin user {UserId} disconnected from logs hub", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to application logs updates.
    /// </summary>
    public async Task SubscribeToApplicationLogs()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AdminLogs");
        _logger.LogInformation("Client {ConnectionId} subscribed to application logs", Context.ConnectionId);
    }

    /// <summary>
    /// Unsubscribe from application logs updates.
    /// </summary>
    public async Task UnsubscribeFromApplicationLogs()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminLogs");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from application logs", Context.ConnectionId);
    }

    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

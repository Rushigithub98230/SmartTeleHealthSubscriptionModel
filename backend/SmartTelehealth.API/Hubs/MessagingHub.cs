using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;
using System.Security.Claims;

namespace SmartTelehealth.API.Hubs;

//[Authorize]
public class MessagingHub : Hub
{
    private readonly IMessagingService _messagingService;
    private readonly ChatService _chatService;
    private readonly ILogger<MessagingHub> _logger;
    private static readonly Dictionary<string, string> _userConnections = new();
    private static readonly Dictionary<string, HashSet<string>> _notificationGroups = new();

    private TokenModel GetTokenModel()
    {
        var userId = GetUserId();
        return new TokenModel
        {
            UserID = userId,
            RoleID = 0 // Default role
        };
    }

    public MessagingHub(
        IMessagingService messagingService,
        ChatService chatService,
        ILogger<MessagingHub> logger)
    {
        _messagingService = messagingService;
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != 0)
        {
            _userConnections[userId.ToString()] = Context.ConnectionId;
            _logger.LogInformation("User {UserId} connected to messaging hub", userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
            if (userId != 0)
        {
            _userConnections.Remove(userId.ToString());
            _logger.LogInformation("User {UserId} disconnected from messaging hub", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Subscribe to notifications
    public async Task SubscribeToNotifications()
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"notifications_{userId}");
            
            if (!_notificationGroups.ContainsKey(userId.ToString()))
                _notificationGroups[userId.ToString()] = new HashSet<string>();
            
            _notificationGroups[userId.ToString()].Add(Context.ConnectionId);

            await Clients.Caller.SendAsync("SubscribedToNotifications", userId);
            _logger.LogInformation("User {UserId} subscribed to notifications", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing user {UserId} to notifications", userId);
        }
    }

    // Unsubscribe from notifications
    public async Task UnsubscribeFromNotifications()
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notifications_{userId}");
            
            if (_notificationGroups.ContainsKey(userId.ToString()))
                _notificationGroups[userId.ToString()].Remove(Context.ConnectionId);

            await Clients.Caller.SendAsync("UnsubscribedFromNotifications", userId);
            _logger.LogInformation("User {UserId} unsubscribed from notifications", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing user {UserId} from notifications", userId);
        }
    }

    // Send notification to specific user
    public async Task SendNotificationToUser(string targetUserId, string title, string message, string? chatRoomId = null)
    {
        var senderId = GetUserId();
        if (senderId == 0) return;

        try
        {
            var notification = new ChatNotificationDto
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                ChatRoomId = chatRoomId,
                UserId = int.TryParse(targetUserId, out var userId) ? userId : 0,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            // Send via messaging service
            if (!int.TryParse(targetUserId, out int targetUserIdInt))
            {
                await Clients.Caller.SendAsync("NotificationFailed", "Invalid user ID format");
                return;
            }
            var result = await _messagingService.SendNotificationToUserAsync(targetUserIdInt, title, message, chatRoomId, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                // Send real-time notification if user is online
                if (_userConnections.ContainsKey(targetUserId))
                {
                    await Clients.Client(_userConnections[targetUserId]).SendAsync("NotificationReceived", notification);
                }
                
                await Clients.Caller.SendAsync("NotificationSent", notification);
            }
            else
            {
                await Clients.Caller.SendAsync("NotificationFailed", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {TargetUserId} from {SenderId}", targetUserId, senderId);
            await Clients.Caller.SendAsync("NotificationFailed", "Failed to send notification");
        }
    }

    // Send notification to chat room
    public async Task SendNotificationToChatRoom(string chatRoomId, string title, string message)
    {
        var senderId = GetUserId();
        if (senderId == 0) return;

        try
        {
            var notification = new ChatNotificationDto
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                ChatRoomId = chatRoomId,
                UserId = senderId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            // Send via messaging service
            var result = await _messagingService.SendNotificationToChatRoomAsync(chatRoomId, title, message, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                // Send real-time notification to all users in the chat room
                await Clients.Group(chatRoomId).SendAsync("ChatRoomNotificationReceived", notification);
                await Clients.Caller.SendAsync("NotificationSent", notification);
            }
            else
            {
                await Clients.Caller.SendAsync("NotificationFailed", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to chat room {ChatRoomId}", chatRoomId);
            await Clients.Caller.SendAsync("NotificationFailed", "Failed to send notification");
        }
    }

    // Mark notification as read
    public async Task MarkNotificationAsRead(string notificationId)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            // This would typically update the notification in the database
            // For now, we'll just acknowledge the read status
            await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
            _logger.LogInformation("User {UserId} marked notification {NotificationId} as read", userId, notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read by user {UserId}", notificationId, userId);
        }
    }

    // Get unread notifications count
    public async Task GetUnreadNotificationsCount()
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            // This would typically query the database for unread notifications
            // For now, we'll return a placeholder count
            var unreadCount = 0; // TODO: Implement actual count from database
            await Clients.Caller.SendAsync("UnreadNotificationsCount", unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications count for user {UserId}", userId);
        }
    }

    // Send typing indicator
    public async Task SendTypingIndicator(string chatRoomId, bool isTyping)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var typingIndicator = new TypingIndicatorDto
            {
                ChatRoomId = Guid.Parse(chatRoomId),
                UserId = userId,
                UserName = GetUserName(),
                IsTyping = isTyping
            };

            await Clients.OthersInGroup(chatRoomId).SendAsync("TypingIndicator", typingIndicator);
            
            // Also send to messaging service for persistence if needed
            await _messagingService.SendTypingIndicatorAsync(chatRoomId, userId, isTyping, GetTokenModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing indicator in chat room {ChatRoomId}", chatRoomId);
        }
    }

    // Send message status update
    public async Task SendMessageStatusUpdate(string messageId, string status)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var message = await _messagingService.GetMessageAsync(messageId, GetTokenModel()); // was Guid.Parse(messageId)
                    if (message.StatusCode == 200 && message.data != null)
        {
            await Clients.Group(((dynamic)message.data).ChatRoomId.ToString()).SendAsync("MessageStatusUpdated", messageId, status);
        }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message status update for message {MessageId}", messageId);
        }
    }

    // Get online users in chat room
    public async Task GetOnlineUsers(string chatRoomId)
    {
        try
        {
            if (_notificationGroups.ContainsKey(chatRoomId))
            {
                var onlineUserIds = _notificationGroups[chatRoomId]
                    .Select(connectionId => _userConnections.FirstOrDefault(x => x.Value == connectionId).Key)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

                await Clients.Caller.SendAsync("OnlineUsers", chatRoomId, onlineUserIds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online users for chat room {ChatRoomId}", chatRoomId);
        }
    }

    // Send system notification
    public async Task SendSystemNotification(string title, string message, string? targetUserId = null)
    {
        var senderId = GetUserId();
        if (senderId == 0) return;

        try
        {
            var notification = new ChatNotificationDto
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                UserId = !string.IsNullOrEmpty(targetUserId) && int.TryParse(targetUserId, out var userId) ? userId : 0,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            if (!string.IsNullOrEmpty(targetUserId))
            {
                // Send to specific user
                if (_userConnections.ContainsKey(targetUserId))
                {
                    await Clients.Client(_userConnections[targetUserId]).SendAsync("SystemNotificationReceived", notification);
                }
            }
            else
            {
                // Send to all connected users
                await Clients.All.SendAsync("SystemNotificationReceived", notification);
            }

            await Clients.Caller.SendAsync("SystemNotificationSent", notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system notification");
            await Clients.Caller.SendAsync("NotificationFailed", "Failed to send system notification");
        }
    }

    // Private helper methods
    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetUserName()
    {
        var nameClaim = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        return nameClaim ?? "Unknown User";
    }
} 
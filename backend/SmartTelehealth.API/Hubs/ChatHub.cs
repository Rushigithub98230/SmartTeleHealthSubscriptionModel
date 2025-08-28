using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;
using System.Security.Claims;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessagingService _messagingService;
    private readonly ChatService _chatService;
    private readonly ILogger<ChatHub> _logger;
    private static readonly Dictionary<string, string> _userConnections = new();
    private static readonly Dictionary<string, HashSet<string>> _chatRoomGroups = new();
    private static readonly Dictionary<string, DateTime> _userLastSeen = new();

    private TokenModel GetTokenModel()
    {
        var userId = GetUserId();
        return new TokenModel
        {
            UserID = userId,
            RoleID = 0 // Default role
        };
    }

    public ChatHub(
        IMessagingService messagingService,
        ChatService chatService,
        ILogger<ChatHub> logger)
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
            _userLastSeen[userId.ToString()] = DateTime.UtcNow;
            _logger.LogInformation("User {UserId} connected to chat hub", userId);
            
            // Notify other users about online status
            await NotifyUserOnlineStatus(userId, true);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != 0)
        {
            _userConnections.Remove(userId.ToString());
            _userLastSeen[userId.ToString()] = DateTime.UtcNow;
            _logger.LogInformation("User {UserId} disconnected from chat hub", userId);
            
            // Notify other users about offline status
            await NotifyUserOnlineStatus(userId, false);
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Join a chat room
    public async Task JoinChatRoom(string chatRoomId)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            // Validate access to chat room
            var accessValidation = await _messagingService.ValidateChatRoomAccessAsync(chatRoomId, userId, GetTokenModel());
            if (accessValidation.StatusCode != 200)
            {
                await Clients.Caller.SendAsync("AccessDenied", "You don't have access to this chat room");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
            
            if (!_chatRoomGroups.ContainsKey(chatRoomId))
                _chatRoomGroups[chatRoomId] = new HashSet<string>();
            
            _chatRoomGroups[chatRoomId].Add(Context.ConnectionId);

            await Clients.Caller.SendAsync("JoinedChatRoom", chatRoomId);
            await Clients.OthersInGroup(chatRoomId).SendAsync("UserJoined", userId, GetUserName());
            
            _logger.LogInformation("User {UserId} joined chat room {ChatRoomId}", userId, chatRoomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining chat room {ChatRoomId} by user {UserId}", chatRoomId, userId);
            await Clients.Caller.SendAsync("Error", "Failed to join chat room");
        }
    }

    // Leave a chat room
    public async Task LeaveChatRoom(string chatRoomId)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
            
            if (_chatRoomGroups.ContainsKey(chatRoomId))
                _chatRoomGroups[chatRoomId].Remove(Context.ConnectionId);

            await Clients.OthersInGroup(chatRoomId).SendAsync("UserLeft", userId, GetUserName());
            
            _logger.LogInformation("User {UserId} left chat room {ChatRoomId}", userId, chatRoomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving chat room {ChatRoomId} by user {UserId}", chatRoomId, userId);
        }
    }

    // Send a message
    public async Task SendMessage(string chatRoomId, string content, string? replyToMessageId = null, string? filePath = null)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var createMessageDto = new CreateMessageDto
            {
                ChatRoomId = chatRoomId,
                Content = content,
                Type = string.IsNullOrEmpty(filePath) ? "Text" : GetMessageTypeFromFilePath(filePath).ToString(),
                ReplyToMessageId = replyToMessageId,
                FilePath = filePath
            };

            var result = await _messagingService.SendMessageAsync(createMessageDto, userId, GetTokenModel());
            
            if (result.StatusCode == 200)
            {
                // Get the actual message data
                var messages = await _messagingService.GetChatRoomMessagesAsync(chatRoomId, 0, 1, GetTokenModel());
                if (messages.StatusCode == 200 && ((IEnumerable<object>)messages.data).Any())
                {
                    var message = ((IEnumerable<object>)messages.data).First();
                    await Clients.Group(chatRoomId).SendAsync("MessageReceived", message);
                    var messageId = message.GetType().GetProperty("Id")?.GetValue(message)?.ToString() ?? "";
                    await Clients.Caller.SendAsync("MessageSent", messageId);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("MessageFailed", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message in chat room {ChatRoomId}", chatRoomId);
            await Clients.Caller.SendAsync("MessageFailed", "Failed to send message");
        }
    }

    // Typing indicator
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

    // Mark message as read
    public async Task MarkMessageAsRead(string messageId)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _messagingService.MarkMessageAsReadAsync(messageId, userId, GetTokenModel());
            if (result.StatusCode == 200)
            {
                var message = await _messagingService.GetMessageAsync(messageId, GetTokenModel());
                if (message.StatusCode == 200 && message.data != null)
                {
                    var chatRoomId = message.data.GetType().GetProperty("ChatRoomId")?.GetValue(message.data)?.ToString() ?? "";
                    await Clients.Group(chatRoomId).SendAsync("MessageRead", messageId, userId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
        }
    }

    // Add reaction to message
    public async Task AddReaction(string messageId, string emoji)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var addReactionDto = new AddReactionDto { Emoji = emoji };
            var result = await _messagingService.AddReactionAsync(messageId, userId, emoji, GetTokenModel());
            
            if (result.StatusCode == 200 && result.data != null)
            {
                var message = await _messagingService.GetMessageAsync(messageId, GetTokenModel());
                if (message.StatusCode == 200 && message.data != null)
                {
                    var chatRoomId = message.data.GetType().GetProperty("ChatRoomId")?.GetValue(message.data)?.ToString() ?? "";
                    await Clients.Group(chatRoomId).SendAsync("ReactionAdded", messageId, result.data);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message {MessageId}", messageId);
        }
    }

    // Remove reaction from message
    public async Task RemoveReaction(string messageId, string emoji)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            var result = await _messagingService.RemoveReactionAsync(messageId, userId, emoji, GetTokenModel());
            if (result.StatusCode == 200)
            {
                var message = await _messagingService.GetMessageAsync(messageId, GetTokenModel());
                if (message.StatusCode == 200 && message.data != null)
                {
                    await Clients.Group(((dynamic)message.data).ChatRoomId.ToString()).SendAsync("ReactionRemoved", messageId, userId, emoji);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from message {MessageId}", messageId);
        }
    }

    // Get online users in chat room
    public async Task GetOnlineUsers(string chatRoomId)
    {
        try
        {
            if (_chatRoomGroups.ContainsKey(chatRoomId))
            {
                var onlineUserIds = _chatRoomGroups[chatRoomId]
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

    // Send notification to specific user
    public async Task SendNotification(string userId, string title, string message, string? chatRoomId = null)
    {
        try
        {
            var notification = new
            {
                Title = title,
                Message = message,
                ChatRoomId = chatRoomId,
                Timestamp = DateTime.UtcNow
            };

            if (_userConnections.ContainsKey(userId))
            {
                await Clients.Client(_userConnections[userId]).SendAsync("NotificationReceived", notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
        }
    }

    // Send notification to all users in a chat room
    public async Task SendNotificationToChatRoom(string chatRoomId, string title, string message)
    {
        try
        {
            var notification = new
            {
                Title = title,
                Message = message,
                ChatRoomId = chatRoomId,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(chatRoomId).SendAsync("NotificationReceived", notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to chat room {ChatRoomId}", chatRoomId);
        }
    }

    // Update user status
    public async Task UpdateUserStatus(string status)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        try
        {
            await Clients.All.SendAsync("UserStatusChanged", userId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status for user {UserId}", userId);
        }
    }

    // Get user's last seen time
    public async Task GetUserLastSeen(string userId)
    {
        try
        {
            if (_userLastSeen.ContainsKey(userId))
            {
                await Clients.Caller.SendAsync("UserLastSeen", userId, _userLastSeen[userId]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last seen time for user {UserId}", userId);
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

    private Message.MessageType GetMessageTypeFromFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return Message.MessageType.Text;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => Message.MessageType.Image,
            ".mp4" or ".avi" or ".mov" or ".wmv" => Message.MessageType.Video,
            ".mp3" or ".wav" or ".ogg" => Message.MessageType.Audio,
            ".pdf" or ".doc" or ".docx" or ".txt" => Message.MessageType.Document,
            _ => Message.MessageType.Document
        };
    }

    private async Task NotifyUserOnlineStatus(int userId, bool isOnline)
    {
        try
        {
            // Get all chat rooms where user is a participant
            var chatRooms = await _messagingService.GetUserChatRoomsAsync(userId, GetTokenModel());
            if (chatRooms != null && chatRooms.data != null)
            {
                var chatRoomsList = chatRooms.data as IEnumerable<object>;
                if (chatRoomsList != null)
                {
                    foreach (var chatRoom in chatRoomsList)
                    {
                        var idProperty = chatRoom.GetType().GetProperty("Id");
                        if (idProperty != null)
                        {
                            var id = idProperty.GetValue(chatRoom);
                            if (id != null)
                            {
                                await Clients.Group(id.ToString()).SendAsync("UserOnlineStatusChanged", userId, isOnline);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user online status for user {UserId}", userId);
        }
    }
}

// DTO for typing indicator
public class TypingIndicatorDto
{
    public Guid ChatRoomId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsTyping { get; set; }
} 
using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using AutoMapper;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.Infrastructure.Services;

public class CloudChatStorageService : ICloudChatStorageService, IChatStorageService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CloudChatStorageService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public CloudChatStorageService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<CloudChatStorageService> logger,
        IUserRepository userRepository,
        ISubscriptionRepository subscriptionRepository,
        BlobServiceClient blobServiceClient,
        IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _blobServiceClient = blobServiceClient;
        _containerName = configuration["AzureStorage:ChatContainerName"] ?? "chat-storage";
    }

    // ICloudChatStorageService methods (Guid-based with JsonModel)
    public Task<JsonModel> UpdateChatRoomAsync(Guid chatRoomId, UpdateChatRoomDto updateDto) => throw new NotImplementedException();
    public Task<JsonModel> DeleteChatRoomAsync(Guid chatRoomId) => throw new NotImplementedException();
    public Task<JsonModel> CreateMessageAsync(CreateMessageDto createDto) => throw new NotImplementedException();
    public Task<JsonModel> UpdateMessageAsync(Guid messageId, UpdateMessageDto updateDto) => throw new NotImplementedException();
    public Task<JsonModel> DeleteMessageAsync(Guid messageId) => throw new NotImplementedException();
    public Task<JsonModel> GetMessageByIdAsync(Guid messageId) => throw new NotImplementedException();
    public Task<JsonModel> GetMessagesByChatRoomAsync(Guid chatRoomId) => throw new NotImplementedException();
    public Task<JsonModel> CreateChatRoomAsync(CreateChatRoomDto createDto) => throw new NotImplementedException();

    // IChatStorageService methods (string-based with Task<T>)
    Task<ChatRoomDto> IChatStorageService.CreateChatRoomAsync(CreateChatRoomDto createDto) => throw new NotImplementedException();
    public Task<ChatRoomDto?> GetChatRoomAsync(string chatRoomId) => throw new NotImplementedException();
    public Task<ChatRoomDto?> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto) => throw new NotImplementedException();
    public Task<bool> DeleteChatRoomAsync(string chatRoomId) => throw new NotImplementedException();
    public async Task<IEnumerable<ChatRoomDto>> GetUserChatRoomsAsync(int userId)
    {
        try
        {
            var chatRooms = await _context.ChatRooms
                .Where(cr => cr.Participants.Any(p => p.UserId == userId && p.Status == ChatRoomParticipant.ParticipantStatus.Active))
                .ToListAsync();
            return _mapper.Map<IEnumerable<ChatRoomDto>>(chatRooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user chat rooms for user {UserId}", userId);
            return Enumerable.Empty<ChatRoomDto>();
        }
    }
    public async Task<bool> AddParticipantAsync(string chatRoomId, int userId, string role = "Member")
    {
        try
        {
            var participant = new ChatRoomParticipant
            {
                ChatRoomId = Guid.Parse(chatRoomId),
                UserId = userId,
                Role = role,
                JoinedAt = DateTime.UtcNow,
                Status = ChatRoomParticipant.ParticipantStatus.Active
            };
            _context.ChatRoomParticipants.Add(participant);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant {UserId} to chat room {ChatRoomId}", userId, chatRoomId);
            return false;
        }
    }
    public async Task<bool> RemoveParticipantAsync(string chatRoomId, int userId)
    {
        try
        {
            var participant = await _context.ChatRoomParticipants
                .FirstOrDefaultAsync(p => p.ChatRoomId == Guid.Parse(chatRoomId) && p.UserId == userId);
            
            if (participant != null)
            {
                participant.Status = ChatRoomParticipant.ParticipantStatus.Left;
                participant.LeftAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant {UserId} from chat room {ChatRoomId}", userId, chatRoomId);
            return false;
        }
    }
    public Task<IEnumerable<ChatRoomParticipantDto>> GetChatRoomParticipantsAsync(string chatRoomId) => throw new NotImplementedException();
    public async Task<bool> UpdateParticipantRoleAsync(string chatRoomId, int userId, string newRole)
    {
        try
        {
            var participant = await _context.ChatRoomParticipants
                .FirstOrDefaultAsync(p => p.ChatRoomId == Guid.Parse(chatRoomId) && p.UserId == userId);
            
            if (participant != null)
            {
                participant.Role = newRole;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant role for user {UserId} in chat room {ChatRoomId}", userId, chatRoomId);
            return false;
        }
    }
    public Task<MessageDto> StoreMessageAsync(CreateMessageDto createDto) => throw new NotImplementedException();
    public Task<MessageDto?> GetMessageAsync(string messageId) => throw new NotImplementedException();
    public Task<IEnumerable<MessageDto>> GetChatRoomMessagesAsync(string chatRoomId, int page = 1, int pageSize = 50) => throw new NotImplementedException();
    public Task<bool> UpdateMessageAsync(string messageId, UpdateMessageDto updateDto, int userId) => throw new NotImplementedException();
    public Task<bool> DeleteMessageAsync(string messageId, int userId) => throw new NotImplementedException();
    public async Task<IEnumerable<MessageDto>> GetUnreadMessagesAsync(int userId, string chatRoomId)
    {
        try
        {
            var messages = await _context.Messages
                .Where(m => m.ChatRoomId == Guid.Parse(chatRoomId) && 
                           m.Status == Message.MessageStatus.Sent &&
                           m.SenderId != userId)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread messages for user {UserId} in chat room {ChatRoomId}", userId, chatRoomId);
            return Enumerable.Empty<MessageDto>();
        }
    }
    public async Task<bool> MarkMessageAsReadAsync(string messageId, int userId)
    {
        try
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == Guid.Parse(messageId));
            
            if (message != null)
            {
                message.Status = Message.MessageStatus.Read;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read for user {UserId}", messageId, userId);
            return false;
        }
    }
    public async Task<bool> AddReactionAsync(string messageId, int userId, string reactionType)
    {
        try
        {
            var reaction = new MessageReaction
            {
                MessageId = Guid.Parse(messageId),
                UserId = userId,
                Emoji = reactionType,
                CreatedDate = DateTime.UtcNow
            };
            
            _context.MessageReactions.Add(reaction);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message {MessageId} by user {UserId}", messageId, userId);
            return false;
        }
    }
    public async Task<bool> RemoveReactionAsync(string messageId, int userId, string reactionType)
    {
        try
        {
            var reaction = await _context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == Guid.Parse(messageId) && 
                                        r.UserId == userId && 
                                        r.Emoji == reactionType);
            
            if (reaction != null)
            {
                _context.MessageReactions.Remove(reaction);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from message {MessageId} by user {UserId}", messageId, userId);
            return false;
        }
    }
    public Task<IEnumerable<MessageReactionDto>> GetMessageReactionsAsync(string messageId) => throw new NotImplementedException();
    public Task<string> UploadMessageAttachmentAsync(string messageId, Stream fileStream, string fileName, string contentType) => throw new NotImplementedException();
    public Task<Stream> DownloadMessageAttachmentAsync(string attachmentId) => throw new NotImplementedException();
    public Task<bool> DeleteMessageAttachmentAsync(string attachmentId) => throw new NotImplementedException();
    public Task<IEnumerable<MessageDto>> SearchMessagesAsync(string chatRoomId, string searchTerm) => throw new NotImplementedException();
    public async Task<bool> ValidateChatAccessAsync(int userId, string chatRoomId)
    {
        try
        {
            var participant = await _context.ChatRoomParticipants
                .FirstOrDefaultAsync(p => p.ChatRoomId == Guid.Parse(chatRoomId) && 
                                        p.UserId == userId && 
                                        p.Status == ChatRoomParticipant.ParticipantStatus.Active);
            
            return participant != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating chat access for user {UserId} in chat room {ChatRoomId}", userId, chatRoomId);
            return false;
        }
    }
    public Task<ChatStatisticsDto> GetChatStatisticsAsync(string chatRoomId) => throw new NotImplementedException();

    // Private helper methods for blob operations
    private async Task StoreBlobAsync(string blobName, string content)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(blobName);
        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }
    }

    private async Task<string?> GetBlobAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadAsync();
        using (var reader = new StreamReader(response.Value.Content))
        {
            return await reader.ReadToEndAsync();
        }
    }

    private async Task DeleteBlobAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync())
        {
            await blobClient.DeleteAsync();
        }
    }
} 
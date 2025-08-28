using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using AutoMapper;

namespace SmartTelehealth.Application.Services;

public class ChatRoomService : IChatRoomService
{
    private readonly IChatStorageService _chatStorageService;
    private readonly ILogger<ChatRoomService> _logger;

    public ChatRoomService(IChatStorageService chatStorageService, ILogger<ChatRoomService> logger)
    {
        _chatStorageService = chatStorageService;
        _logger = logger;
    }

    public async Task<JsonModel> CreateChatRoomAsync(CreateChatRoomDto createDto)
    {
        try
        {
            var chatRoom = await _chatStorageService.CreateChatRoomAsync(createDto);
            return new JsonModel { data = chatRoom, Message = "Chat room created successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat room");
            return new JsonModel { data = new object(), Message = "Failed to create chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreatePatientProviderChatRoomAsync(string patientId, string providerId, string? subscriptionId = null)
    {
        try
        {
            var createDto = new CreateChatRoomDto
            {
                Name = $"Patient-Provider Chat",
                Description = "Direct consultation chat between patient and provider",
                Type = "Direct",
                CreatedBy = patientId,
                ParticipantIds = new List<string> { patientId, providerId }
            };

            var chatRoom = await _chatStorageService.CreateChatRoomAsync(createDto);
            return new JsonModel { data = chatRoom, Message = "Patient-provider chat room created successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient-provider chat room");
            return new JsonModel { data = new object(), Message = "Failed to create patient-provider chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreateGroupChatRoomAsync(string name, string? description, List<string> participantIds, string creatorId)
    {
        try
        {
            var createDto = new CreateChatRoomDto
            {
                Name = name,
                Description = description ?? "Group chat room",
                Type = "Group",
                CreatedBy = creatorId,
                ParticipantIds = participantIds
            };

            var chatRoom = await _chatStorageService.CreateChatRoomAsync(createDto);
            return new JsonModel { data = chatRoom, Message = "Group chat room created successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group chat room");
            return new JsonModel { data = new object(), Message = "Failed to create group chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetChatRoomAsync(string chatRoomId)
    {
        try
        {
            var chatRoom = await _chatStorageService.GetChatRoomAsync(chatRoomId);
            if (chatRoom == null)
                return new JsonModel { data = new object(), Message = "Chat room not found", StatusCode = 404 };

            return new JsonModel { data = chatRoom, Message = "Chat room retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room {ChatRoomId}", chatRoomId);
            return new JsonModel { data = new object(), Message = "Failed to get chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserChatRoomsAsync(int userId)
    {
        try
        {
            var chatRooms = await _chatStorageService.GetUserChatRoomsAsync(userId);
            return new JsonModel { data = chatRooms, Message = "User chat rooms retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user chat rooms {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Failed to get user chat rooms", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUnreadMessagesAsync(int userId, string chatRoomId)
    {
        try
        {
            var messages = await _chatStorageService.GetUnreadMessagesAsync(userId, chatRoomId);
            return new JsonModel { data = messages, Message = "Unread messages retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread messages");
            return new JsonModel { data = new object(), Message = "Failed to get unread messages", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto)
    {
        try
        {
            var chatRoom = await _chatStorageService.UpdateChatRoomAsync(chatRoomId, updateDto);
            if (chatRoom == null)
                return new JsonModel { data = new object(), Message = "Chat room not found", StatusCode = 404 };

            return new JsonModel { data = chatRoom, Message = "Chat room updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chat room {ChatRoomId}", chatRoomId);
            return new JsonModel { data = new object(), Message = "Failed to update chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> RemoveParticipantAsync(string chatRoomId, int userId)
    {
        try
        {
            var result = await _chatStorageService.RemoveParticipantAsync(chatRoomId, userId);
            return new JsonModel { data = result, Message = "Participant removed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant from chat room");
            return new JsonModel { data = new object(), Message = "Failed to remove participant", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetChatRoomParticipantsAsync(string chatRoomId)
    {
        try
        {
            var participants = await _chatStorageService.GetChatRoomParticipantsAsync(chatRoomId);
            return new JsonModel { data = participants, Message = "Chat room participants retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room participants {ChatRoomId}", chatRoomId);
            return new JsonModel { data = new object(), Message = "Failed to get chat room participants", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> AddParticipantAsync(string chatRoomId, int userId, string role = "Member")
    {
        try
        {
            var result = await _chatStorageService.AddParticipantAsync(chatRoomId, userId, role);
            return new JsonModel { data = result, Message = "Participant added successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to chat room");
            return new JsonModel { data = new object(), Message = "Failed to add participant", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateParticipantRoleAsync(string chatRoomId, int userId, string newRole)
    {
        try
        {
            var result = await _chatStorageService.UpdateParticipantRoleAsync(chatRoomId, userId, newRole);
            return new JsonModel { data = result, Message = "Participant role updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant role");
            return new JsonModel { data = new object(), Message = "Failed to update participant role", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ValidateChatAccessAsync(int userId, string chatRoomId)
    {
        try
        {
            var result = await _chatStorageService.ValidateChatAccessAsync(userId, chatRoomId);
            return new JsonModel { data = result, Message = "Chat access validated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating chat access");
            return new JsonModel { data = new object(), Message = "Failed to validate chat access", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetChatRoomAsync(string chatRoomId, int userId)
    {
        try
        {
            // Validate access first
            var accessResult = await _chatStorageService.ValidateChatAccessAsync(userId, chatRoomId);
            if (!accessResult)
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };

            var chatRoom = await _chatStorageService.GetChatRoomAsync(chatRoomId);
            if (chatRoom == null)
                return new JsonModel { data = new object(), Message = "Chat room not found", StatusCode = 404 };

            return new JsonModel { data = chatRoom, Message = "Chat room retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room {ChatRoomId} for user {UserId}", chatRoomId, userId);
            return new JsonModel { data = new object(), Message = "Failed to get chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetChatRoomMessagesAsync(string chatRoomId, int userId)
    {
        try
        {
            // Validate access first
            var accessResult = await _chatStorageService.ValidateChatAccessAsync(userId, chatRoomId);
            if (!accessResult)
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };

            var messages = await _chatStorageService.GetChatRoomMessagesAsync(chatRoomId);
            return new JsonModel { data = messages, Message = "Chat room messages retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room messages");
            return new JsonModel { data = new object(), Message = "Failed to get chat room messages", StatusCode = 500 };
        }
    }
} 
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IMessageService
{
    Task<JsonModel> GetByIdAsync(Guid id);
    Task<JsonModel> GetByUserIdAsync(Guid userId);
    Task<JsonModel> CreateAsync(CreateMessageDto createDto);
    Task<JsonModel> UpdateAsync(Guid id, UpdateMessageDto updateDto);
    Task<JsonModel> DeleteAsync(Guid id);
    Task<JsonModel> GetAllAsync();
} 
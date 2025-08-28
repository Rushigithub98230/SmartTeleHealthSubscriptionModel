using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IFileService
{
    Task<JsonModel> GetByIdAsync(Guid id);
    Task<JsonModel> GetByUserIdAsync(Guid userId);
    Task<JsonModel> CreateAsync(CreateFileDto createDto);
    Task<JsonModel> UpdateAsync(Guid id, UpdateFileDto updateDto);
    Task<JsonModel> DeleteAsync(Guid id);
    Task<JsonModel> GetAllAsync();
} 
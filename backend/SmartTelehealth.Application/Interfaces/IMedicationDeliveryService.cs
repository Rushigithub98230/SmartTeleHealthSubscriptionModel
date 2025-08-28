using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IMedicationDeliveryService
{
    Task<JsonModel> GetByIdAsync(Guid id);
    Task<JsonModel> GetByUserIdAsync(int userId);
    Task<JsonModel> CreateAsync(CreateMedicationDeliveryDto createDto);
    Task<JsonModel> UpdateAsync(Guid id, UpdateMedicationDeliveryDto updateDto);
    Task<JsonModel> DeleteAsync(Guid id);
    Task<JsonModel> GetAllAsync();
} 
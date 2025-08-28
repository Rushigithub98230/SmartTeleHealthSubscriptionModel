using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Infrastructure.Services;

public class MedicationDeliveryService : IMedicationDeliveryService
{
    private readonly IMedicationDeliveryRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<MedicationDeliveryService> _logger;

    public MedicationDeliveryService(
        IMedicationDeliveryRepository repository,
        IMapper mapper,
        ILogger<MedicationDeliveryService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<JsonModel> GetByIdAsync(Guid id)
    {
        try
        {
            var delivery = await _repository.GetByIdAsync(id);
            if (delivery == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Medication delivery not found",
                    StatusCode = 404
                };

            var dto = _mapper.Map<MedicationDeliveryDto>(delivery);
            return new JsonModel
            {
                data = dto,
                Message = "Medication delivery retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medication delivery by ID {Id}", id);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get medication delivery",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetByUserIdAsync(int userId)
    {
        try
        {
            var deliveries = await _repository.GetByUserIdAsync(userId);
            var dtos = _mapper.Map<IEnumerable<MedicationDeliveryDto>>(deliveries);
            return new JsonModel
            {
                data = dtos,
                Message = "Medication deliveries retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medication deliveries for user {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get medication deliveries",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CreateAsync(CreateMedicationDeliveryDto createDto)
    {
        try
        {
            var delivery = _mapper.Map<MedicationDelivery>(createDto);
            var created = await _repository.CreateAsync(delivery);
            var dto = _mapper.Map<MedicationDeliveryDto>(created);
            return new JsonModel
            {
                data = dto,
                Message = "Medication delivery created successfully",
                StatusCode = 201
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medication delivery");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to create medication delivery",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdateAsync(Guid id, UpdateMedicationDeliveryDto updateDto)
    {
        try
        {
            var delivery = await _repository.GetByIdAsync(id);
            if (delivery == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Medication delivery not found",
                    StatusCode = 404
                };

            _mapper.Map(updateDto, delivery);
            var updated = await _repository.UpdateAsync(delivery);
            var dto = _mapper.Map<MedicationDeliveryDto>(updated);
            return new JsonModel
            {
                data = dto,
                Message = "Medication delivery updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medication delivery {Id}", id);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to update medication delivery",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteAsync(Guid id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            return new JsonModel
            {
                data = result,
                Message = "Medication delivery deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting medication delivery {Id}", id);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to delete medication delivery",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAllAsync()
    {
        try
        {
            var deliveries = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<MedicationDeliveryDto>>(deliveries);
            return new JsonModel
            {
                data = dtos,
                Message = "All medication deliveries retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all medication deliveries");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get medication deliveries",
                StatusCode = 500
            };
        }
    }
} 
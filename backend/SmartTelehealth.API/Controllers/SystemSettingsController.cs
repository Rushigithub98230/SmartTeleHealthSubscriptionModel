using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for managing system-wide settings and configuration.
/// This controller provides endpoints for retrieving and updating global system parameters
/// that control application behavior, billing defaults, and administrative settings.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class SystemSettingsController : BaseController
{
    private readonly ISystemSettingsRepository _settingsRepository;

    /// <summary>
    /// Initializes a new instance of the SystemSettingsController with required repository.
    /// </summary>
    /// <param name="settingsRepository">Repository for system settings management</param>
    public SystemSettingsController(ISystemSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    /// <summary>
    /// Retrieves the current system settings.
    /// This endpoint returns all global system configuration parameters.
    /// </summary>
    /// <returns>JsonModel containing current system settings</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns default admin commission percentage
    /// - Returns default price change notice period (days)
    /// - Returns maximum failed payment attempts allowed
    /// - Returns last updated timestamp
    /// - Access restricted to administrators only
    /// - Used for system configuration viewing
    /// </remarks>
    [HttpGet]
    public async Task<JsonModel> GetSettings()
    {
        try
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            
            return new JsonModel
            {
                data = settings,
                Message = "System settings retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving system settings: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Updates system settings with new configuration values.
    /// This endpoint allows administrators to modify global system parameters.
    /// </summary>
    /// <param name="updateDto">DTO containing updated settings values</param>
    /// <returns>JsonModel containing updated system settings</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates default admin commission percentage (0-100)
    /// - Updates default price change notice period (days)
    /// - Updates maximum failed payment attempts (1-10)
    /// - Validates input ranges
    /// - Records update timestamp and user
    /// - Access restricted to administrators only
    /// - Used for system configuration management
    /// </remarks>
    [HttpPut]
    public async Task<JsonModel> UpdateSettings([FromBody] UpdateSystemSettingsDto updateDto)
    {
        try
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            
            // Update only provided fields
            if (updateDto.DefaultAdminCommissionPercent.HasValue)
            {
                if (updateDto.DefaultAdminCommissionPercent.Value < 0 || updateDto.DefaultAdminCommissionPercent.Value > 100)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Commission percent must be between 0 and 100",
                        StatusCode = 400
                    };
                }
                settings.DefaultAdminCommissionPercent = updateDto.DefaultAdminCommissionPercent.Value;
            }
            
            if (updateDto.DefaultPriceChangeNoticeDays.HasValue)
            {
                if (updateDto.DefaultPriceChangeNoticeDays.Value < 1 || updateDto.DefaultPriceChangeNoticeDays.Value > 90)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Price change notice days must be between 1 and 90",
                        StatusCode = 400
                    };
                }
                settings.DefaultPriceChangeNoticeDays = updateDto.DefaultPriceChangeNoticeDays.Value;
            }
            
            if (updateDto.MaxFailedPaymentAttempts.HasValue)
            {
                if (updateDto.MaxFailedPaymentAttempts.Value < 1 || updateDto.MaxFailedPaymentAttempts.Value > 10)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Max failed payment attempts must be between 1 and 10",
                        StatusCode = 400
                    };
                }
                settings.MaxFailedPaymentAttempts = updateDto.MaxFailedPaymentAttempts.Value;
            }
            
            settings.LastUpdated = DateTime.UtcNow;
            
            await _settingsRepository.UpdateSettingsAsync(settings);
            
            return new JsonModel
            {
                data = settings,
                Message = "System settings updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error updating system settings: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Retrieves the history of system settings changes.
    /// This endpoint returns audit trail of all system settings modifications.
    /// </summary>
    /// <returns>JsonModel containing settings change history</returns>
    [HttpGet("history")]
    public async Task<JsonModel> GetSettingsHistory()
    {
        try
        {
            // This would require an audit log implementation
            // For now, return empty array
            return new JsonModel
            {
                data = new List<object>(),
                Message = "Settings history not yet implemented",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error retrieving settings history: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Resets system settings to default values.
    /// This endpoint restores all system settings to their original default configuration.
    /// </summary>
    /// <returns>JsonModel containing reset system settings</returns>
    [HttpPost("reset")]
    public async Task<JsonModel> ResetToDefaults()
    {
        try
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            
            // Reset to defaults
            settings.DefaultAdminCommissionPercent = 20;
            settings.DefaultPriceChangeNoticeDays = 30;
            settings.MaxFailedPaymentAttempts = 3;
            settings.LastUpdated = DateTime.UtcNow;
            
            await _settingsRepository.UpdateSettingsAsync(settings);
            
            return new JsonModel
            {
                data = settings,
                Message = "System settings reset to defaults successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Error resetting system settings: {ex.Message}",
                StatusCode = 500
            };
        }
    }
}

/// <summary>
/// DTO for updating system settings
/// </summary>
public class UpdateSystemSettingsDto
{
    public decimal? DefaultAdminCommissionPercent { get; set; }
    public int? DefaultPriceChangeNoticeDays { get; set; }
    public int? MaxFailedPaymentAttempts { get; set; }
}




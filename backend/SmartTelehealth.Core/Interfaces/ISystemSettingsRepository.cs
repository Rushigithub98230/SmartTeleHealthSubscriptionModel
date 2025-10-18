using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

/// <summary>
/// Repository interface for system-wide settings management.
/// Healthcare Feature: Global defaults for commission rates and notice periods.
/// </summary>
public interface ISystemSettingsRepository : IRepositoryBase<SystemSettings>
{
    /// <summary>
    /// Gets the system settings (singleton pattern).
    /// </summary>
    /// <returns>System settings entity</returns>
    Task<SystemSettings> GetSettingsAsync();
    
    /// <summary>
    /// Updates the system settings.
    /// </summary>
    /// <param name="settings">Updated settings</param>
    /// <returns>Updated system settings entity</returns>
    Task<SystemSettings> UpdateSettingsAsync(SystemSettings settings);
}


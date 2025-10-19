using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

/// <summary>
/// Repository for system-wide settings management.
/// Healthcare Feature: Global defaults for commission rates and notice periods.
/// Implements singleton pattern for system settings.
/// </summary>
public class SystemSettingsRepository : RepositoryBase<SystemSettings>, ISystemSettingsRepository
{
    private readonly ApplicationDbContext _context;

    public SystemSettingsRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the system settings (singleton pattern).
    /// Returns the first (and only) settings record.
    /// </summary>
    public async Task<SystemSettings> GetSettingsAsync()
    {
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        
        // If no settings exist, create default settings
        if (settings == null)
        {
            settings = new SystemSettings
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DefaultAdminCommissionPercent = 20,
                DefaultPriceChangeNoticeDays = 10,
                MaxFailedPaymentAttempts = 3,
                LastUpdated = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = null,  // System-generated, no user created this
                CreatedDate = DateTime.UtcNow
            };
            
            await _context.SystemSettings.AddAsync(settings);
            await _context.SaveChangesAsync();
        }
        
        return settings;
    }

    /// <summary>
    /// Updates the system settings.
    /// </summary>
    public async Task<SystemSettings> UpdateSettingsAsync(SystemSettings settings)
    {
        settings.LastUpdated = DateTime.UtcNow;
        _context.SystemSettings.Update(settings);
        await _context.SaveChangesAsync();
        
        return settings;
    }
}


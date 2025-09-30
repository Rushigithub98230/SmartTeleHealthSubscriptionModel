using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ProviderRepository : RepositoryBase<Provider>, IProviderRepository
{
    private readonly ApplicationDbContext _context;

    public ProviderRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a provider by its unique identifier with related entities
    /// </summary>
    public async Task<Provider?> GetByIdWithDetailsAsync(int providerId)
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Id == providerId && !p.IsDeleted);
    }

    /// <summary>
    /// Retrieves all providers with related entities
    /// </summary>
    public async Task<IEnumerable<Provider>> GetAllWithDetailsAsync()
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new provider
    /// </summary>
    public async Task<Provider> CreateProviderAsync(Provider provider)
    {
        return await base.CreateAsync(provider);
    }

    /// <summary>
    /// Updates an existing provider
    /// </summary>
    public async Task<Provider> UpdateProviderAsync(Provider provider)
    {
        return await base.UpdateAsync(provider);
    }

    /// <summary>
    /// Deletes a provider by its unique identifier (soft delete)
    /// </summary>
    public async Task<bool> DeleteProviderAsync(int providerId)
    {
        var provider = await _context.Providers.FindAsync(providerId);
        if (provider == null) return false;

        provider.IsActive = false;
        provider.IsDeleted = true;
        Update(provider);
        await SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a provider exists
    /// </summary>
    public async Task<bool> ExistsProviderAsync(int providerId)
    {
        return await _context.Providers
            .AnyAsync(p => p.Id == providerId && !p.IsDeleted);
    }

    public async Task<IEnumerable<Provider>> GetActiveProvidersAsync()
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Provider>> GetAvailableProvidersAsync()
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories)
            .Where(p => p.IsAvailable && p.AvailableFrom.HasValue && p.AvailableTo.HasValue)
            .ToListAsync();
    }

    public async Task<IEnumerable<Provider>> GetProvidersByCategoryAsync(Guid categoryId)
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.CategoryId == categoryId && pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => p.ProviderCategories.Any(pc => pc.CategoryId == categoryId && pc.IsAvailable) &&
                       p.IsActive && p.IsAvailable && !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Provider>> GetProvidersBySpecialtyAsync(string specialty)
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => p.Specialty.ToLower().Contains(specialty.ToLower()) &&
                       p.IsActive && p.IsAvailable && !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }


    public async Task<bool> ExistsByLicenseNumberAsync(string licenseNumber)
    {
        return await _context.Providers
            .AnyAsync(p => p.LicenseNumber == licenseNumber && !p.IsDeleted);
    }

    public async Task<int> GetActiveProviderCountAsync()
    {
        return await _context.Providers
            .CountAsync(p => p.IsActive && !p.IsDeleted);
    }

    public async Task<IEnumerable<Provider>> SearchProvidersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Provider>();

        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => !p.IsDeleted && 
                       (p.FirstName.ToLower().Contains(searchTerm.ToLower()) ||
                        p.LastName.ToLower().Contains(searchTerm.ToLower()) ||
                        p.Specialty.ToLower().Contains(searchTerm.ToLower()) ||
                        p.LicenseNumber.ToLower().Contains(searchTerm.ToLower())))
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Provider>> GetProvidersByAvailabilityAsync(TimeSpan time)
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive && p.IsAvailable && !p.IsDeleted &&
                       p.AvailableFrom <= time && p.AvailableTo >= time)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }
} 
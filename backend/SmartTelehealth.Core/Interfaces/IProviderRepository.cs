using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IProviderRepository : IRepositoryBase<Provider>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Provider>
    
    Task<IEnumerable<Provider>> GetActiveProvidersAsync();
    Task<IEnumerable<Provider>> GetAvailableProvidersAsync();
    Task<IEnumerable<Provider>> GetProvidersByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Provider>> GetProvidersBySpecialtyAsync(string specialty);
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber);
    Task<int> GetActiveProviderCountAsync();
    Task<IEnumerable<Provider>> SearchProvidersAsync(string searchTerm);
    Task<IEnumerable<Provider>> GetProvidersByAvailabilityAsync(TimeSpan time);
} 
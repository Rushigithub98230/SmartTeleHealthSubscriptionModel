using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IProviderRepository : IRepositoryBase<Provider>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Provider>
    
    // Custom methods with business logic
    Task<Provider?> GetByIdWithDetailsAsync(int providerId);
    Task<IEnumerable<Provider>> GetAllWithDetailsAsync();
    Task<Provider> CreateProviderAsync(Provider provider);
    Task<Provider> UpdateProviderAsync(Provider provider);
    Task<bool> DeleteProviderAsync(int providerId);
    Task<bool> ExistsProviderAsync(int providerId);
    
    Task<IEnumerable<Provider>> GetActiveProvidersAsync();
    Task<IEnumerable<Provider>> GetAvailableProvidersAsync();
    Task<IEnumerable<Provider>> GetProvidersByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Provider>> GetProvidersBySpecialtyAsync(string specialty);
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber);
    Task<int> GetActiveProviderCountAsync();
    Task<IEnumerable<Provider>> SearchProvidersAsync(string searchTerm);
    Task<IEnumerable<Provider>> GetProvidersByAvailabilityAsync(TimeSpan time);
} 
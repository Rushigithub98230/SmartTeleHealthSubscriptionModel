using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IProviderRepository : IRepositoryBase<Provider>
{
    Task<Provider?> GetByIdAsync(int id);
    Task<IEnumerable<Provider>> GetAllAsync();
    Task<IEnumerable<Provider>> GetActiveProvidersAsync();
    Task<IEnumerable<Provider>> GetAvailableProvidersAsync();
    Task<IEnumerable<Provider>> GetProvidersByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Provider>> GetProvidersBySpecialtyAsync(string specialty);
    Task<Provider> CreateAsync(Provider provider);
    Task<Provider> UpdateAsync(Provider provider);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber);
    Task<int> GetActiveProviderCountAsync();
    Task<IEnumerable<Provider>> SearchProvidersAsync(string searchTerm);
    Task<IEnumerable<Provider>> GetProvidersByAvailabilityAsync(TimeSpan time);
} 
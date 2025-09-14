using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IProviderOnboardingRepository : IRepositoryBase<ProviderOnboarding>
{
    // Basic CRUD methods are inherited from IRepositoryBase<ProviderOnboarding>
    
    Task<ProviderOnboarding?> GetByUserIdAsync(int userId);
    Task<IEnumerable<ProviderOnboarding>> GetByStatusAsync(string status);
    Task<IEnumerable<ProviderOnboarding>> GetPendingAsync();
    Task<IEnumerable<ProviderOnboarding>> GetByStatusWithPaginationAsync(string status, int page, int pageSize);
    Task<ProviderOnboarding> AddAsync(ProviderOnboarding onboarding); // Legacy method
    Task<int> GetCountByStatusAsync(string status);
    Task<int> GetTotalCountAsync();
} 
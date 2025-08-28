using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IPharmacyIntegrationRepository
{
    Task<PharmacyIntegration> GetByIdAsync(Guid id);
    Task<PharmacyIntegration> GetActiveIntegrationAsync();
    Task<IEnumerable<PharmacyIntegration>> GetAllAsync();
    Task<PharmacyIntegration> CreateAsync(PharmacyIntegration integration);
    Task<PharmacyIntegration> UpdateAsync(PharmacyIntegration integration);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> TestConnectionAsync(Guid integrationId);
    Task<DateTime> GetLastSyncTimeAsync(Guid integrationId);
    Task<bool> UpdateLastSyncTimeAsync(Guid integrationId, DateTime syncTime);
} 
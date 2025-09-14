using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IPrescriptionRepository : IRepositoryBase<Prescription>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Prescription>
    
    Task<IEnumerable<Prescription>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Prescription>> GetByProviderIdAsync(int providerId);
    Task<IEnumerable<Prescription>> GetByStatusAsync(string status);
    Task<IEnumerable<Prescription>> GetOverduePrescriptionsAsync();
    Task<IEnumerable<Prescription>> GetRefillRequestsAsync(int userId);
    Task<int> GetPrescriptionCountAsync(int userId);
    Task<decimal> GetPrescriptionTotalAsync(int userId);
} 
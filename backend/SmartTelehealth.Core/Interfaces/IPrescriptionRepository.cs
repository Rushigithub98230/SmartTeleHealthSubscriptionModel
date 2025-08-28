using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IPrescriptionRepository
{
    Task<Prescription> GetByIdAsync(Guid id);
    Task<IEnumerable<Prescription>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Prescription>> GetByProviderIdAsync(int providerId);
    Task<IEnumerable<Prescription>> GetByStatusAsync(string status);
    Task<Prescription> CreateAsync(Prescription prescription);
    Task<Prescription> UpdateAsync(Prescription prescription);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Prescription>> GetOverduePrescriptionsAsync();
    Task<IEnumerable<Prescription>> GetRefillRequestsAsync(int userId);
    Task<int> GetPrescriptionCountAsync(int userId);
    Task<decimal> GetPrescriptionTotalAsync(int userId);
} 
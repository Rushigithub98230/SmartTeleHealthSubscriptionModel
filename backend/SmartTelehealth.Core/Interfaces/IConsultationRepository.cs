using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IConsultationRepository
{
    Task<Consultation> GetByIdAsync(Guid id);
    Task<IEnumerable<Consultation>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Consultation>> GetByProviderIdAsync(int providerId);
    Task<IEnumerable<Consultation>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<Consultation> CreateAsync(Consultation consultation);
    Task<Consultation> UpdateAsync(Consultation consultation);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Consultation>> GetUpcomingConsultationsAsync();
    Task<IEnumerable<Consultation>> GetUpcomingAsync();
    Task<IEnumerable<Consultation>> GetCompletedConsultationsAsync(int userId);
} 
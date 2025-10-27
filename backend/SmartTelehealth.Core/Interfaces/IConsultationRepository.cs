using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IConsultationRepository : IRepositoryBase<Consultation>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Consultation>
    
    // Custom methods with business logic
    Task<Consultation?> GetByIdWithDetailsAsync(Guid consultationId);
    Task<IEnumerable<Consultation>> GetAllWithDetailsAsync();
    Task<Consultation> CreateConsultationAsync(Consultation consultation);
    Task<Consultation> UpdateConsultationAsync(Consultation consultation);
    Task<bool> DeleteConsultationAsync(Guid consultationId);
    Task<bool> ExistsConsultationAsync(Guid consultationId);
    
    Task<IEnumerable<Consultation>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Consultation>> GetByProviderIdAsync(int providerId);
    Task<IEnumerable<Consultation>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<Consultation>> GetUpcomingConsultationsAsync();
    Task<IEnumerable<Consultation>> GetUpcomingAsync();
    Task<IEnumerable<Consultation>> GetScheduledConsultationsAsync();
    Task<IEnumerable<Consultation>> GetCompletedConsultationsAsync(int userId);
    
    // Analytics methods
    Task<int> GetTotalConsultationsCountAsync(DateTime startDate, DateTime endDate);
    Task<int> GetCompletedConsultationsCountAsync(DateTime startDate, DateTime endDate);
    Task<int> GetCancelledConsultationsCountAsync(DateTime startDate, DateTime endDate);
    Task<int> GetPendingConsultationsCountAsync(DateTime startDate, DateTime endDate);
    Task<double> GetAverageConsultationDurationAsync(DateTime startDate, DateTime endDate);
} 
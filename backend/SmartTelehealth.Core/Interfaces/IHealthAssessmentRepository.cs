using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IHealthAssessmentRepository : IRepositoryBase<HealthAssessment>
{
    // Basic CRUD methods are inherited from IRepositoryBase<HealthAssessment>
    
    Task<IEnumerable<HealthAssessment>> GetByUserIdAsync(int userId);
    Task<IEnumerable<HealthAssessment>> GetByCategoryIdAsync(Guid categoryId);
    Task<IEnumerable<HealthAssessment>> GetPendingAssessmentsAsync();
    Task<IEnumerable<HealthAssessment>> GetUserAssessmentsAsync(int userId);
    Task<IEnumerable<HealthAssessment>> GetProviderPendingAssessmentsAsync(int providerId);
    Task<IEnumerable<HealthAssessment>> GetProviderReviewedAssessmentsAsync(int providerId);
} 
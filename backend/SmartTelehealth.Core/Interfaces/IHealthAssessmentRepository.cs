using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IHealthAssessmentRepository
{
    Task<HealthAssessment> GetByIdAsync(Guid id);
    Task<IEnumerable<HealthAssessment>> GetByUserIdAsync(int userId);
    Task<IEnumerable<HealthAssessment>> GetByCategoryIdAsync(Guid categoryId);
    Task<IEnumerable<HealthAssessment>> GetPendingAssessmentsAsync();
    Task<HealthAssessment> CreateAsync(HealthAssessment assessment);
    Task<HealthAssessment> UpdateAsync(HealthAssessment assessment);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<HealthAssessment>> GetUserAssessmentsAsync(int userId);
    Task<IEnumerable<HealthAssessment>> GetProviderPendingAssessmentsAsync(int providerId);
    Task<IEnumerable<HealthAssessment>> GetProviderReviewedAssessmentsAsync(int providerId);
} 
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IProviderOnboardingService
{
    Task<JsonModel> CreateOnboardingAsync(CreateProviderOnboardingDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetOnboardingAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetOnboardingByUserIdAsync(int userId, TokenModel tokenModel);
    Task<JsonModel> UpdateOnboardingAsync(Guid id, UpdateProviderOnboardingDto updateDto, TokenModel tokenModel);
    Task<JsonModel> SubmitOnboardingAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> ReviewOnboardingAsync(Guid id, ReviewProviderOnboardingDto reviewDto, TokenModel tokenModel);
    Task<JsonModel> GetAllOnboardingsAsync(string? status, int page, int pageSize, TokenModel tokenModel);
    Task<JsonModel> GetPendingOnboardingsAsync(TokenModel tokenModel);
    Task<JsonModel> GetOnboardingsByStatusAsync(string status, TokenModel tokenModel);
    Task<JsonModel> DeleteOnboardingAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetOnboardingStatisticsAsync(TokenModel tokenModel);
}

public class OnboardingStatisticsDto
{
    public int TotalOnboardings { get; set; }
    public int PendingOnboardings { get; set; }
    public int UnderReviewOnboardings { get; set; }
    public int ApprovedOnboardings { get; set; }
    public int RejectedOnboardings { get; set; }
    public int RequiresMoreInfoOnboardings { get; set; }
    public decimal ApprovalRate { get; set; }
    public int AverageProcessingTimeDays { get; set; }
} 
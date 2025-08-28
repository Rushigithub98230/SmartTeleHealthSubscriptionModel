using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IProviderFeeService
{
    Task<JsonModel> CreateFeeAsync(CreateProviderFeeDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetFeeAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetFeeByProviderAndCategoryAsync(int providerId, Guid categoryId, TokenModel tokenModel);
    Task<JsonModel> UpdateFeeAsync(Guid id, UpdateProviderFeeDto updateDto, TokenModel tokenModel);
    Task<JsonModel> ProposeFeeAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> ReviewFeeAsync(Guid id, ReviewProviderFeeDto reviewDto, TokenModel tokenModel);
    Task<JsonModel> GetFeesByProviderAsync(int providerId, TokenModel tokenModel);
    Task<JsonModel> GetFeesByCategoryAsync(Guid categoryId, TokenModel tokenModel);
    Task<JsonModel> GetAllFeesAsync(string? status = null, int page = 1, int pageSize = 50, TokenModel tokenModel = null);
    Task<JsonModel> GetPendingFeesAsync(TokenModel tokenModel);
    Task<JsonModel> GetFeesByStatusAsync(string status, TokenModel tokenModel);
    Task<JsonModel> DeleteFeeAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetFeeStatisticsAsync(TokenModel tokenModel);
}

public interface ICategoryFeeRangeService
{
    Task<JsonModel> CreateFeeRangeAsync(CreateCategoryFeeRangeDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetFeeRangeAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetFeeRangeByCategoryAsync(Guid categoryId, TokenModel tokenModel);
    Task<JsonModel> UpdateFeeRangeAsync(Guid id, UpdateCategoryFeeRangeDto updateDto, TokenModel tokenModel);
    Task<JsonModel> GetAllFeeRangesAsync(TokenModel tokenModel);
    Task<JsonModel> DeleteFeeRangeAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetFeeRangeStatisticsAsync(TokenModel tokenModel);
}

public class FeeStatisticsDto
{
    public int TotalFees { get; set; }
    public int PendingFees { get; set; }
    public int UnderReviewFees { get; set; }
    public int ApprovedFees { get; set; }
    public int RejectedFees { get; set; }
    public decimal AverageProposedFee { get; set; }
    public decimal AverageApprovedFee { get; set; }
    public decimal ApprovalRate { get; set; }
}

public class FeeRangeStatisticsDto
{
    public int TotalFeeRanges { get; set; }
    public int ActiveFeeRanges { get; set; }
    public decimal AverageMinimumFee { get; set; }
    public decimal AverageMaximumFee { get; set; }
    public decimal AveragePlatformCommission { get; set; }
} 
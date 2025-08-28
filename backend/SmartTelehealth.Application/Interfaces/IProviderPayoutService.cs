using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IProviderPayoutService
{
    Task<JsonModel> GetPayoutAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> ProcessPayoutAsync(Guid id, ProcessPayoutDto processDto, TokenModel tokenModel);
    Task<JsonModel> GetPayoutsByProviderAsync(int providerId, TokenModel tokenModel);
    Task<JsonModel> GetPayoutsByPeriodAsync(Guid periodId, TokenModel tokenModel);
    Task<JsonModel> GetAllPayoutsAsync(string? status, int page, int pageSize, TokenModel tokenModel);
    Task<JsonModel> GetPendingPayoutsAsync(TokenModel tokenModel);
    Task<JsonModel> GetPayoutsByStatusAsync(string status, TokenModel tokenModel);
    Task<JsonModel> GetProviderEarningsAsync(int providerId, TokenModel tokenModel);
    Task<JsonModel> GetPayoutStatisticsAsync(TokenModel tokenModel);
    Task<JsonModel> GeneratePayoutsForPeriodAsync(Guid periodId, TokenModel tokenModel);
    Task<JsonModel> ProcessAllPendingPayoutsAsync(TokenModel tokenModel);
}

public interface IPayoutPeriodService
{
    Task<JsonModel> CreatePeriodAsync(CreatePayoutPeriodDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetPeriodAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> UpdatePeriodAsync(Guid id, CreatePayoutPeriodDto updateDto, TokenModel tokenModel);
    Task<JsonModel> GetAllPeriodsAsync(TokenModel tokenModel);
    Task<JsonModel> GetActivePeriodsAsync(TokenModel tokenModel);
    Task<JsonModel> DeletePeriodAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> ProcessPeriodAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetPeriodStatisticsAsync(TokenModel tokenModel);
}

public class PayoutStatisticsDto
{
    public int TotalPayouts { get; set; }
    public int PendingPayouts { get; set; }
    public int ProcessedPayouts { get; set; }
    public int OnHoldPayouts { get; set; }
    public decimal TotalPayoutAmount { get; set; }
    public decimal PendingPayoutAmount { get; set; }
    public decimal ProcessedPayoutAmount { get; set; }
    public decimal AveragePayoutAmount { get; set; }
    public int TotalProviders { get; set; }
    public int ProvidersWithPendingPayouts { get; set; }
}

public class PayoutPeriodStatisticsDto
{
    public int TotalPeriods { get; set; }
    public int OpenPeriods { get; set; }
    public int ProcessingPeriods { get; set; }
    public int CompletedPeriods { get; set; }
    public decimal TotalAmountProcessed { get; set; }
    public int TotalPayoutsProcessed { get; set; }
    public decimal AveragePeriodAmount { get; set; }
} 
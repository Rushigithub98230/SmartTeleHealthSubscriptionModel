using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IProviderPayoutRepository : IRepositoryBase<ProviderPayout>
{
    // Basic CRUD methods are inherited from IRepositoryBase<ProviderPayout>
    
    Task<IEnumerable<ProviderPayout>> GetByProviderAsync(int providerId);
    Task<IEnumerable<ProviderPayout>> GetByPeriodAsync(Guid periodId);
    Task<IEnumerable<ProviderPayout>> GetByStatusAsync(string status);
    Task<IEnumerable<ProviderPayout>> GetPendingAsync();
    Task<IEnumerable<ProviderPayout>> GetByStatusWithPaginationAsync(string status, int page, int pageSize);
    Task<int> GetCountByStatusAsync(string status);
    Task<int> GetTotalCountAsync();
    Task<decimal> GetTotalEarningsByProviderAsync(int providerId);
    Task<decimal> GetPendingEarningsByProviderAsync(int providerId);
    Task<IEnumerable<ProviderPayout>> GetPendingPayoutsAsync();
    Task<decimal> GetTotalPayoutAmountByProviderAsync(int providerId);
    Task<decimal> GetPendingPayoutAmountByProviderAsync(int providerId);
    Task<int> GetPayoutCountByProviderAsync(int providerId);
    Task<object> GetPayoutStatisticsAsync();
    Task<object> AddPeriodAsync();
    Task<object> GetAllPeriodsAsync();
    Task<ProviderPayout> AddAsync(ProviderPayout payout); // Legacy method
}

public interface IPayoutPeriodRepository : IRepositoryBase<PayoutPeriod>
{
    Task<PayoutPeriod?> GetByIdAsync(Guid id);
    Task<IEnumerable<PayoutPeriod>> GetAllAsync();
    Task<IEnumerable<PayoutPeriod>> GetActiveAsync();
    Task<PayoutPeriod> AddAsync(PayoutPeriod period);
    Task<PayoutPeriod> UpdateAsync(PayoutPeriod period);
    Task<bool> DeleteAsync(Guid id);
}

public interface IPayoutDetailRepository : IRepositoryBase<PayoutDetail>
{
    Task<IEnumerable<PayoutDetail>> GetByPayoutAsync(Guid payoutId);
    Task<PayoutDetail> AddAsync(PayoutDetail detail);
    Task<PayoutDetail> UpdateAsync(PayoutDetail detail);
    Task<bool> DeleteAsync(Guid id);
} 
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IProviderFeeRepository
{
    Task<ProviderFee?> GetByIdAsync(Guid id);
    Task<ProviderFee?> GetByProviderAndCategoryAsync(int providerId, Guid categoryId);
    Task<IEnumerable<ProviderFee>> GetByProviderAsync(int providerId);
    Task<IEnumerable<ProviderFee>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<ProviderFee>> GetAllAsync();
    Task<IEnumerable<ProviderFee>> GetByStatusAsync(string status);
    Task<IEnumerable<ProviderFee>> GetPendingAsync();
    Task<IEnumerable<ProviderFee>> GetByStatusWithPaginationAsync(string status, int page, int pageSize);
    Task<ProviderFee> AddAsync(ProviderFee fee);
    Task<ProviderFee> UpdateAsync(ProviderFee fee);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetCountByStatusAsync(string status);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<ProviderFee>> GetByProviderIdAsync(int providerId);
    Task<IEnumerable<ProviderFee>> GetByCategoryIdAsync(Guid categoryId);
    Task<IEnumerable<ProviderFee>> GetAllAsync(string status, int page, int pageSize);
    Task<IEnumerable<ProviderFee>> GetPendingFeesAsync();
    Task<object> GetFeeStatisticsAsync();
}

public interface ICategoryFeeRangeRepository
{
    Task<CategoryFeeRange?> GetByIdAsync(Guid id);
    Task<CategoryFeeRange?> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<CategoryFeeRange>> GetAllAsync();
    Task<IEnumerable<CategoryFeeRange>> GetActiveAsync();
    Task<CategoryFeeRange> AddAsync(CategoryFeeRange feeRange);
    Task<CategoryFeeRange> UpdateAsync(CategoryFeeRange feeRange);
    Task<bool> DeleteAsync(Guid id);
} 
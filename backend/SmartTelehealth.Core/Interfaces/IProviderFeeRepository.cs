using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IProviderFeeRepository : IRepositoryBase<ProviderFee>
{
    // Basic CRUD methods are inherited from IRepositoryBase<ProviderFee>
    
    Task<ProviderFee?> GetByProviderAndCategoryAsync(int providerId, Guid categoryId);
    Task<IEnumerable<ProviderFee>> GetByProviderAsync(int providerId);
    Task<IEnumerable<ProviderFee>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<ProviderFee>> GetByStatusAsync(string status);
    Task<IEnumerable<ProviderFee>> GetPendingAsync();
    Task<IEnumerable<ProviderFee>> GetByStatusWithPaginationAsync(string status, int page, int pageSize);
    Task<ProviderFee> AddAsync(ProviderFee fee); // Legacy method
    Task<int> GetCountByStatusAsync(string status);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<ProviderFee>> GetByProviderIdAsync(int providerId);
    Task<IEnumerable<ProviderFee>> GetByCategoryIdAsync(Guid categoryId);
    Task<IEnumerable<ProviderFee>> GetAllAsync(string status, int page, int pageSize);
    Task<IEnumerable<ProviderFee>> GetPendingFeesAsync();
    Task<object> GetFeeStatisticsAsync();
}

public interface ICategoryFeeRangeRepository : IRepositoryBase<CategoryFeeRange>
{
    // Basic CRUD methods are inherited from IRepositoryBase<CategoryFeeRange>
    
    Task<CategoryFeeRange?> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<CategoryFeeRange>> GetActiveAsync();
    Task<CategoryFeeRange> AddAsync(CategoryFeeRange feeRange); // Legacy method
} 
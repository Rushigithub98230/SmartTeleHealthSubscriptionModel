using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ProviderFeeRepository : IProviderFeeRepository
{
    private readonly ApplicationDbContext _context;

    public ProviderFeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProviderFee?> GetByIdAsync(Guid id)
    {
        return await _context.ProviderFees
            .Include(f => f.Provider)
            .Include(f => f.Category)
            .Include(f => f.ReviewedByUser)
            .FirstOrDefaultAsync(f => f.Id == id && f.IsActive);
    }

    public async Task<ProviderFee?> GetByProviderAndCategoryAsync(int providerId, Guid categoryId)
    {
        return await _context.ProviderFees
            .Include(f => f.Provider)
            .Include(f => f.Category)
            .Include(f => f.ReviewedByUser)
            .FirstOrDefaultAsync(f => f.ProviderId == providerId && f.CategoryId == categoryId && f.IsActive);
    }

    public async Task<IEnumerable<ProviderFee>> GetByProviderAsync(int providerId)
    {
        return await _context.ProviderFees
            .Include(f => f.Provider)
            .Include(f => f.Category)
            .Include(f => f.ReviewedByUser)
            .Where(f => f.ProviderId == providerId && f.IsActive)
            .OrderByDescending(f => f.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProviderFee>> GetByCategoryAsync(Guid categoryId)
    {
        return await _context.ProviderFees
            .Include(f => f.Provider)
            .Include(f => f.Category)
            .Include(f => f.ReviewedByUser)
            .Where(f => f.CategoryId == categoryId && f.IsActive)
            .OrderByDescending(f => f.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProviderFee>> GetAllAsync()
    {
        return await _context.ProviderFees
            .Include(f => f.Provider)
            .Include(f => f.Category)
            .Include(f => f.ReviewedByUser)
            .Where(f => f.IsActive)
            .OrderByDescending(f => f.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProviderFee>> GetByStatusAsync(string status)
    {
        if (Enum.TryParse<FeeStatus>(status, out var statusEnum))
        {
            return await _context.ProviderFees
                .Include(f => f.Provider)
                .Include(f => f.Category)
                .Include(f => f.ReviewedByUser)
                .Where(f => f.Status == statusEnum && f.IsActive)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }
        return new List<ProviderFee>();
    }

    public async Task<IEnumerable<ProviderFee>> GetPendingAsync()
    {
        return await _context.ProviderFees
            .Include(f => f.Provider)
            .Include(f => f.Category)
            .Include(f => f.ReviewedByUser)
            .Where(f => f.Status == FeeStatus.Pending && f.IsActive)
            .OrderByDescending(f => f.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProviderFee>> GetByStatusWithPaginationAsync(string status, int page, int pageSize)
    {
        var query = _context.ProviderFees
            .Include(f => f.Provider)
            .Include(f => f.Category)
            .Include(f => f.ReviewedByUser)
            .Where(f => f.IsActive);

        if (Enum.TryParse<FeeStatus>(status, out var statusEnum))
        {
            query = query.Where(f => f.Status == statusEnum);
        }

        return await query
            .OrderByDescending(f => f.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<ProviderFee> AddAsync(ProviderFee fee)
    {
        _context.ProviderFees.Add(fee);
        await _context.SaveChangesAsync();
        return fee;
    }

    public async Task<ProviderFee> UpdateAsync(ProviderFee fee)
    {
        _context.ProviderFees.Update(fee);
        await _context.SaveChangesAsync();
        return fee;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var fee = await _context.ProviderFees.FindAsync(id);
        if (fee == null)
            return false;

        fee.IsActive = false;
        fee.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetCountByStatusAsync(string status)
    {
        if (Enum.TryParse<FeeStatus>(status, out var statusEnum))
        {
            return await _context.ProviderFees
                .CountAsync(f => f.Status == statusEnum && f.IsActive);
        }
        return 0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.ProviderFees
            .CountAsync(f => f.IsActive);
    }

    public Task<IEnumerable<ProviderFee>> GetPendingFeesAsync() => Task.FromResult<IEnumerable<ProviderFee>>(new List<ProviderFee>());
    public Task<object> GetFeeStatisticsAsync() => Task.FromResult<object>(new { });
    public Task<IEnumerable<ProviderFee>> GetAllAsync(string status, int page, int pageSize) => Task.FromResult<IEnumerable<ProviderFee>>(new List<ProviderFee>());
            public Task<IEnumerable<ProviderFee>> GetByProviderIdAsync(int providerId) => Task.FromResult<IEnumerable<ProviderFee>>(new List<ProviderFee>());
    public Task<IEnumerable<ProviderFee>> GetByCategoryIdAsync(Guid categoryId) => Task.FromResult<IEnumerable<ProviderFee>>(new List<ProviderFee>());
}

public class CategoryFeeRangeRepository : ICategoryFeeRangeRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryFeeRangeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryFeeRange?> GetByIdAsync(Guid id)
    {
        return await _context.CategoryFeeRanges
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<CategoryFeeRange?> GetByCategoryAsync(Guid categoryId)
    {
        return await _context.CategoryFeeRanges
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.CategoryId == categoryId && f.IsActive);
    }

    public async Task<IEnumerable<CategoryFeeRange>> GetAllAsync()
    {
        return await _context.CategoryFeeRanges
            .Include(f => f.Category)
            .Where(f => f.IsActive)
            .OrderBy(f => f.Category.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoryFeeRange>> GetActiveAsync()
    {
        return await _context.CategoryFeeRanges
            .Include(f => f.Category)
            .Where(f => f.IsActive)
            .OrderBy(f => f.Category.Name)
            .ToListAsync();
    }

    public async Task<CategoryFeeRange> AddAsync(CategoryFeeRange feeRange)
    {
        _context.CategoryFeeRanges.Add(feeRange);
        await _context.SaveChangesAsync();
        return feeRange;
    }

    public async Task<CategoryFeeRange> UpdateAsync(CategoryFeeRange feeRange)
    {
        _context.CategoryFeeRanges.Update(feeRange);
        await _context.SaveChangesAsync();
        return feeRange;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var feeRange = await _context.CategoryFeeRanges.FindAsync(id);
        if (feeRange == null)
            return false;

        feeRange.IsActive = false;
        feeRange.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
} 
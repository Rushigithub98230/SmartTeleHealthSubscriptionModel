using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ProviderOnboardingRepository : RepositoryBase<ProviderOnboarding>, IProviderOnboardingRepository
{
    private readonly ApplicationDbContext _context;

    public ProviderOnboardingRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a provider onboarding by its unique identifier with related entities
    /// </summary>
    public override async Task<ProviderOnboarding?> GetByIdAsync(object id)
    {
        if (id is not Guid onboardingId)
            return null;

        return await _context.ProviderOnboardings
            .Include(o => o.User)
            .Include(o => o.ReviewedByUser)
            .FirstOrDefaultAsync(o => o.Id == onboardingId && o.IsActive);
    }

    public async Task<ProviderOnboarding?> GetByUserIdAsync(int userId)
    {
        return await _context.ProviderOnboardings
            .Include(o => o.User)
            .Include(o => o.ReviewedByUser)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.IsActive);
    }

    /// <summary>
    /// Retrieves all provider onboardings with related entities
    /// </summary>
    public override async Task<IEnumerable<ProviderOnboarding>> GetAllAsync()
    {
        return await _context.ProviderOnboardings
            .Include(o => o.User)
            .Include(o => o.ReviewedByUser)
            .Where(o => o.IsActive)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProviderOnboarding>> GetByStatusAsync(string status)
    {
        if (Enum.TryParse<OnboardingStatus>(status, out var statusEnum))
        {
            return await _context.ProviderOnboardings
                .Include(o => o.User)
                .Include(o => o.ReviewedByUser)
                .Where(o => o.Status == statusEnum && o.IsActive)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }
        return new List<ProviderOnboarding>();
    }

    public async Task<IEnumerable<ProviderOnboarding>> GetPendingAsync()
    {
        return await _context.ProviderOnboardings
            .Include(o => o.User)
            .Include(o => o.ReviewedByUser)
            .Where(o => o.Status == OnboardingStatus.Pending && o.IsActive)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProviderOnboarding>> GetByStatusWithPaginationAsync(string status, int page, int pageSize)
    {
        var query = _context.ProviderOnboardings
            .Include(o => o.User)
            .Include(o => o.ReviewedByUser)
            .Where(o => o.IsActive);

        if (Enum.TryParse<OnboardingStatus>(status, out var statusEnum))
        {
            query = query.Where(o => o.Status == statusEnum);
        }

        return await query
            .OrderByDescending(o => o.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new provider onboarding
    /// </summary>
    public override async Task<ProviderOnboarding> CreateAsync(ProviderOnboarding onboarding)
    {
        onboarding.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(onboarding);
    }

    /// <summary>
    /// Updates an existing provider onboarding
    /// </summary>
    public override async Task<ProviderOnboarding> UpdateAsync(ProviderOnboarding onboarding)
    {
        onboarding.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(onboarding);
    }

    // Note: DeleteAsync is inherited from RepositoryBase<ProviderOnboarding>
    // Service layer should handle audit properties and use UpdateAsync for soft deletes

    /// <summary>
    /// Checks if a provider onboarding exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid onboardingId)
            return false;

        return await _context.ProviderOnboardings
            .AnyAsync(o => o.Id == onboardingId && o.IsActive);
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public async Task<ProviderOnboarding> AddAsync(ProviderOnboarding onboarding)
    {
        return await CreateAsync(onboarding);
    }

    public async Task<int> GetCountByStatusAsync(string status)
    {
        if (Enum.TryParse<OnboardingStatus>(status, out var statusEnum))
        {
            return await _context.ProviderOnboardings
                .CountAsync(o => o.Status == statusEnum && o.IsActive);
        }
        return 0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.ProviderOnboardings
            .CountAsync(o => o.IsActive);
    }
} 
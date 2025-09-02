using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ProviderOnboardingRepository : IProviderOnboardingRepository
{
    private readonly ApplicationDbContext _context;

    public ProviderOnboardingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProviderOnboarding?> GetByIdAsync(Guid id)
    {
        return await _context.ProviderOnboardings
            .Include(o => o.User)
            .Include(o => o.ReviewedByUser)
            .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);
    }

    public async Task<ProviderOnboarding?> GetByUserIdAsync(int userId)
    {
        return await _context.ProviderOnboardings
            .Include(o => o.User)
            .Include(o => o.ReviewedByUser)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.IsActive);
    }

    public async Task<IEnumerable<ProviderOnboarding>> GetAllAsync()
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

    public async Task<ProviderOnboarding> AddAsync(ProviderOnboarding onboarding)
    {
        _context.ProviderOnboardings.Add(onboarding);
        await _context.SaveChangesAsync();
        return onboarding;
    }

    public async Task<ProviderOnboarding> UpdateAsync(ProviderOnboarding onboarding)
    {
        _context.ProviderOnboardings.Update(onboarding);
        await _context.SaveChangesAsync();
        return onboarding;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var onboarding = await _context.ProviderOnboardings.FindAsync(id);
        if (onboarding == null)
            return false;

        onboarding.IsActive = false;
        onboarding.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
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
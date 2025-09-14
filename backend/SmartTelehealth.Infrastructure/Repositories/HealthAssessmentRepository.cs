using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class HealthAssessmentRepository : RepositoryBase<HealthAssessment>, IHealthAssessmentRepository
{
    private readonly ApplicationDbContext _context;
    
    public HealthAssessmentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    
    public override async Task<HealthAssessment?> GetByIdAsync(object id)
    {
        if (id is not Guid assessmentId)
            return null;

        return await _context.HealthAssessments
            .Include(h => h.User)
            .Include(h => h.Category)
            .Include(h => h.Provider)
            .FirstOrDefaultAsync(h => h.Id == assessmentId);
    }

    public override async Task<IEnumerable<HealthAssessment>> GetAllAsync()
    {
        return await _context.HealthAssessments
            .Include(h => h.User)
            .Include(h => h.Category)
            .Include(h => h.Provider)
            .OrderByDescending(h => h.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<HealthAssessment>> GetByUserIdAsync(int userId)
    {
        return await _context.HealthAssessments
            .Include(h => h.Category)
            .Include(h => h.Provider)
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<HealthAssessment>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _context.HealthAssessments
            .Include(h => h.User)
            .Include(h => h.Provider)
            .Where(h => h.CategoryId == categoryId)
            .OrderByDescending(h => h.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<HealthAssessment>> GetPendingAssessmentsAsync()
    {
        return await _context.HealthAssessments
            .Include(h => h.Category)
            .Include(h => h.Provider)
            .Where(h => h.Status == HealthAssessment.AssessmentStatus.Pending)
            .OrderBy(h => h.CreatedDate)
            .ToListAsync();
    }
    
    public override async Task<HealthAssessment> CreateAsync(HealthAssessment assessment)
    {
        assessment.CreatedDate = DateTime.UtcNow;
        assessment.UpdatedDate = DateTime.UtcNow;
        return await base.CreateAsync(assessment);
    }
    
    public override async Task<HealthAssessment> UpdateAsync(HealthAssessment assessment)
    {
        assessment.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(assessment);
    }
    
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid assessmentId)
            return false;

        var assessment = await _context.HealthAssessments.FindAsync(assessmentId);
        if (assessment == null)
            return false;
            
        _context.HealthAssessments.Remove(assessment);
        await _context.SaveChangesAsync();
        return true;
    }

    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid assessmentId)
            return false;

        return await _context.HealthAssessments.AnyAsync(h => h.Id == assessmentId);
    }

    public async Task<IEnumerable<HealthAssessment>> GetUserAssessmentsAsync(int userId)
    {
        return await _context.HealthAssessments
            .Where(ha => ha.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<HealthAssessment>> GetProviderPendingAssessmentsAsync(int providerId)
    {
        return await _context.HealthAssessments
            .Where(ha => ha.ProviderId == providerId && ha.Status == HealthAssessment.AssessmentStatus.Pending)
            .ToListAsync();
    }

    public async Task<IEnumerable<HealthAssessment>> GetProviderReviewedAssessmentsAsync(int providerId)
    {
        return await _context.HealthAssessments
            .Where(ha => ha.ProviderId == providerId && ha.Status == HealthAssessment.AssessmentStatus.Completed)
            .ToListAsync();
    }
} 
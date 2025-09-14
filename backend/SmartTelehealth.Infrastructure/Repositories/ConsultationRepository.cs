using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ConsultationRepository : RepositoryBase<Consultation>, IConsultationRepository
{
    private readonly ApplicationDbContext _context;
    
    public ConsultationRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Retrieves a consultation by its unique identifier with related entities
    /// </summary>
    public override async Task<Consultation?> GetByIdAsync(object id)
    {
        if (id is not Guid consultationId)
            return null;

        return await _context.Consultations
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Category)
            .Include(c => c.Subscription)
            .Include(c => c.HealthAssessment)
            .FirstOrDefaultAsync(c => c.Id == consultationId);
    }
    
    public async Task<IEnumerable<Consultation>> GetByUserIdAsync(int userId)
    {
        return await _context.Consultations
            .Include(c => c.Provider)
            .Include(c => c.Category)
            .Include(c => c.Subscription)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ScheduledAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Consultation>> GetByProviderIdAsync(int providerId)
    {
        return await _context.Consultations
            .Include(c => c.User)
            .Include(c => c.Category)
            .Include(c => c.Subscription)
            .Where(c => c.ProviderId == providerId)
            .OrderByDescending(c => c.ScheduledAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Consultation>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.Consultations
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Category)
            .Where(c => c.SubscriptionId == subscriptionId)
            .OrderByDescending(c => c.ScheduledAt)
            .ToListAsync();
    }
    
    /// <summary>
    /// Retrieves all consultations with related entities
    /// </summary>
    public override async Task<IEnumerable<Consultation>> GetAllAsync()
    {
        return await _context.Consultations
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Category)
            .Include(c => c.Subscription)
            .Include(c => c.HealthAssessment)
            .OrderByDescending(c => c.ScheduledAt)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new consultation
    /// </summary>
    public override async Task<Consultation> CreateAsync(Consultation consultation)
    {
        consultation.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(consultation);
    }
    
    /// <summary>
    /// Updates an existing consultation
    /// </summary>
    public override async Task<Consultation> UpdateAsync(Consultation consultation)
    {
        consultation.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(consultation);
    }
    
    /// <summary>
    /// Deletes a consultation by its unique identifier (hard delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid consultationId)
            return false;

        var consultation = await _context.Consultations.FindAsync(consultationId);
        if (consultation == null)
            return false;
            
        _context.Consultations.Remove(consultation);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a consultation exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid consultationId)
            return false;

        return await _context.Consultations.AnyAsync(c => c.Id == consultationId);
    }
    
    public async Task<IEnumerable<Consultation>> GetUpcomingConsultationsAsync()
    {
        return await _context.Consultations
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Category)
            .Where(c => c.ScheduledAt > DateTime.UtcNow && c.Status == Consultation.ConsultationStatus.Scheduled)
            .OrderBy(c => c.ScheduledAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Consultation>> GetUpcomingAsync()
    {
        return await _context.Consultations
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Category)
            .Where(c => c.ScheduledAt > DateTime.UtcNow && c.Status == Consultation.ConsultationStatus.Scheduled)
            .OrderBy(c => c.ScheduledAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Consultation>> GetScheduledConsultationsAsync()
    {
        return await _context.Consultations
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Category)
            .Where(c => c.Status == Consultation.ConsultationStatus.Scheduled)
            .OrderBy(c => c.ScheduledAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Consultation>> GetCompletedConsultationsAsync(int userId)
    {
        return await _context.Consultations
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Category)
            .Where(c => c.UserId == userId && c.Status == Consultation.ConsultationStatus.Completed)
            .OrderByDescending(c => c.EndedAt)
            .ToListAsync();
    }
} 
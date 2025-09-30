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
    public async Task<Consultation?> GetByIdWithDetailsAsync(Guid consultationId)
    {
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
    public async Task<IEnumerable<Consultation>> GetAllWithDetailsAsync()
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
    public async Task<Consultation> CreateConsultationAsync(Consultation consultation)
    {
        return await base.CreateAsync(consultation);
    }
    
    /// <summary>
    /// Updates an existing consultation
    /// </summary>
    public async Task<Consultation> UpdateConsultationAsync(Consultation consultation)
    {
        return await base.UpdateAsync(consultation);
    }
    
    /// <summary>
    /// Deletes a consultation by its unique identifier (soft delete)
    /// </summary>
    public async Task<bool> DeleteConsultationAsync(Guid consultationId)
    {
        var consultation = await _context.Consultations.FindAsync(consultationId);
        if (consultation == null)
            return false;

        consultation.IsActive = false;
        consultation.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a consultation exists
    /// </summary>
    public async Task<bool> ExistsConsultationAsync(Guid consultationId)
    {
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
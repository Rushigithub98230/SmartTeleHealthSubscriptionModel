using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ConsultationRepository : IConsultationRepository
{
    private readonly ApplicationDbContext _context;
    
    public ConsultationRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Consultation> GetByIdAsync(Guid id)
    {
        return await _context.Consultations
            .Include(c => c.User)
            .Include(c => c.Provider)
            .Include(c => c.Category)
            .Include(c => c.Subscription)
            .Include(c => c.HealthAssessment)
            .FirstOrDefaultAsync(c => c.Id == id);
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
    
    public async Task<Consultation> CreateAsync(Consultation consultation)
    {
        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();
        return consultation;
    }
    
    public async Task<Consultation> UpdateAsync(Consultation consultation)
    {
        _context.Consultations.Update(consultation);
        await _context.SaveChangesAsync();
        return consultation;
    }
    
    public async Task<bool> DeleteAsync(Guid id)
    {
        var consultation = await _context.Consultations.FindAsync(id);
        if (consultation == null)
            return false;
            
        _context.Consultations.Remove(consultation);
        await _context.SaveChangesAsync();
        return true;
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
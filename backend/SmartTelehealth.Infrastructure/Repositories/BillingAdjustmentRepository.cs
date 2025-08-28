using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class BillingAdjustmentRepository : IBillingAdjustmentRepository
{
    private readonly ApplicationDbContext _context;

    public BillingAdjustmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BillingAdjustment> GetByIdAsync(Guid id)
    {
        return await _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
            .FirstOrDefaultAsync(ba => ba.Id == id);
    }

    public async Task<IEnumerable<BillingAdjustment>> GetByBillingRecordIdAsync(Guid billingRecordId)
    {
        return await _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
            .Where(ba => ba.BillingRecordId == billingRecordId)
            .ToListAsync();
    }

    public async Task<BillingAdjustment> CreateAsync(BillingAdjustment billingAdjustment)
    {
        _context.BillingAdjustments.Add(billingAdjustment);
        await _context.SaveChangesAsync();
        return billingAdjustment;
    }

    public async Task<BillingAdjustment> UpdateAsync(BillingAdjustment billingAdjustment)
    {
        _context.BillingAdjustments.Update(billingAdjustment);
        await _context.SaveChangesAsync();
        return billingAdjustment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var billingAdjustment = await GetByIdAsync(id);
        if (billingAdjustment == null)
            return false;

        _context.BillingAdjustments.Remove(billingAdjustment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<BillingAdjustment>> GetAllAsync()
    {
        return await _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
            .ToListAsync();
    }
} 
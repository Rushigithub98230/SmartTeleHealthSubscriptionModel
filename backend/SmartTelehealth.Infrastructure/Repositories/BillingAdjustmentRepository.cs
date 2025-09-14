using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class BillingAdjustmentRepository : RepositoryBase<BillingAdjustment>, IBillingAdjustmentRepository
{
    private readonly ApplicationDbContext _context;

    public BillingAdjustmentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a billing adjustment by its unique identifier with related entities
    /// </summary>
    public override async Task<BillingAdjustment?> GetByIdAsync(object id)
    {
        if (id is not Guid adjustmentId)
            return null;

        return await _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
            .FirstOrDefaultAsync(ba => ba.Id == adjustmentId);
    }

    public async Task<IEnumerable<BillingAdjustment>> GetByBillingRecordIdAsync(Guid billingRecordId)
    {
        return await _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
            .Where(ba => ba.BillingRecordId == billingRecordId)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all billing adjustments with related entities
    /// </summary>
    public override async Task<IEnumerable<BillingAdjustment>> GetAllAsync()
    {
        return await _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new billing adjustment
    /// </summary>
    public override async Task<BillingAdjustment> CreateAsync(BillingAdjustment billingAdjustment)
    {
        billingAdjustment.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(billingAdjustment);
    }

    /// <summary>
    /// Updates an existing billing adjustment
    /// </summary>
    public override async Task<BillingAdjustment> UpdateAsync(BillingAdjustment billingAdjustment)
    {
        billingAdjustment.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(billingAdjustment);
    }

    /// <summary>
    /// Deletes a billing adjustment by its unique identifier (hard delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid adjustmentId)
            return false;

        var billingAdjustment = await GetByIdAsync(adjustmentId);
        if (billingAdjustment == null)
            return false;

        _context.BillingAdjustments.Remove(billingAdjustment);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a billing adjustment exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid adjustmentId)
            return false;

        return await _context.BillingAdjustments.AnyAsync(ba => ba.Id == adjustmentId);
    }
} 
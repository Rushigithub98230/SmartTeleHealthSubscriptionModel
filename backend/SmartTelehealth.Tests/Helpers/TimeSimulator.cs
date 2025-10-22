using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartTelehealth.Tests.Helpers;

/// <summary>
/// Utility for simulating time passage in subscription tests
/// </summary>
public class TimeSimulator
{
    private readonly ApplicationDbContext _context;

    public TimeSimulator(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Advances subscription to a target date, updating all related dates
    /// </summary>
    public async Task AdvanceTimeAsync(Subscription subscription, DateTime targetDate)
    {
        // Calculate how many billing cycles have passed
        var billingCycle = await _context.MasterBillingCycles
            .FirstOrDefaultAsync(bc => bc.Id == subscription.BillingCycleId);

        if (billingCycle == null)
            throw new InvalidOperationException($"Billing cycle {subscription.BillingCycleId} not found");

        var currentDate = subscription.NextBillingDate;
        
        while (currentDate <= targetDate)
        {
            // Move to next billing date
            subscription.LastBillingDate = subscription.NextBillingDate;
            subscription.NextBillingDate = CalculateNextBillingDate(subscription.NextBillingDate, billingCycle);
            currentDate = subscription.NextBillingDate;
        }

        subscription.UpdatedDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Simulates a complete billing cycle for a subscription
    /// </summary>
    public async Task SimulateBillingCycleAsync(Subscription subscription)
    {
        var billingCycle = await _context.MasterBillingCycles
            .FirstOrDefaultAsync(bc => bc.Id == subscription.BillingCycleId);

        if (billingCycle == null)
            throw new InvalidOperationException($"Billing cycle {subscription.BillingCycleId} not found");

        // Move to next billing date
        subscription.LastBillingDate = subscription.NextBillingDate;
        subscription.NextBillingDate = CalculateNextBillingDate(subscription.NextBillingDate, billingCycle);
        subscription.UpdatedDate = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Advances subscription to its next billing date
    /// </summary>
    public async Task AdvanceToNextBillingDateAsync(Subscription subscription)
    {
        var billingCycle = await _context.MasterBillingCycles
            .FirstOrDefaultAsync(bc => bc.Id == subscription.BillingCycleId);

        if (billingCycle == null)
            throw new InvalidOperationException($"Billing cycle {subscription.BillingCycleId} not found");

        subscription.LastBillingDate = subscription.NextBillingDate;
        subscription.NextBillingDate = CalculateNextBillingDate(subscription.NextBillingDate, billingCycle);
        subscription.UpdatedDate = DateTime.UtcNow;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Simulates multiple billing cycles
    /// </summary>
    public async Task SimulateMultipleBillingCyclesAsync(Subscription subscription, int cycleCount)
    {
        for (int i = 0; i < cycleCount; i++)
        {
            await SimulateBillingCycleAsync(subscription);
        }
    }

    /// <summary>
    /// Calculates the next billing date based on billing cycle
    /// </summary>
    private DateTime CalculateNextBillingDate(DateTime baseDate, MasterBillingCycle billingCycle)
    {
        return billingCycle.Name.ToLower() switch
        {
            "monthly" => baseDate.AddMonths(1),
            "quarterly" => baseDate.AddMonths(3),
            "yearly" or "annual" => baseDate.AddYears(1),
            "weekly" => baseDate.AddDays(7),
            "daily" => baseDate.AddDays(1),
            _ => baseDate.AddDays(billingCycle.DurationInDays)
        };
    }

    /// <summary>
    /// Sets the current time for testing (manipulates subscription dates to simulate time passage)
    /// </summary>
    public async Task SetCurrentTimeAsync(Subscription subscription, DateTime currentTime)
    {
        var daysPassed = (currentTime - subscription.StartDate).Days;
        var billingCycle = await _context.MasterBillingCycles
            .FirstOrDefaultAsync(bc => bc.Id == subscription.BillingCycleId);

        if (billingCycle == null)
            throw new InvalidOperationException($"Billing cycle {subscription.BillingCycleId} not found");

        // Calculate how many full cycles have passed
        var cyclesPassed = daysPassed / billingCycle.DurationInDays;
        
        if (cyclesPassed > 0)
        {
            var lastCycleDate = subscription.StartDate.AddDays(cyclesPassed * billingCycle.DurationInDays);
            subscription.LastBillingDate = lastCycleDate;
            subscription.NextBillingDate = CalculateNextBillingDate(lastCycleDate, billingCycle);
        }

        subscription.UpdatedDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }
}


# Billing Mechanism - Complete Guide

## Table of Contents
1. [Overview](#overview)
2. [Billing Types](#billing-types)
3. [Billing Record Creation](#billing-record-creation)
4. [Privilege-Based Billing](#privilege-based-billing)
5. [Overage Billing](#overage-billing)
6. [Recurring Billing](#recurring-billing)
7. [Billing Adjustments](#billing-adjustments)
8. [Automated Billing Process](#automated-billing-process)

---

## Overview

The billing mechanism manages all financial transactions related to subscriptions, including plan pricing, usage tracking, overage charges, and automated recurring billing.

###Key Services
- **SubscriptionBillingService** - Core billing operations
- **AutomatedBillingService** - Automated billing workflows
- **PlanPricingService** - Healthcare-specific pricing
- **PaymentService** - Payment execution

### Billing Flow Architecture

```
┌──────────────────┐
│ Subscription     │
│ Activated        │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Create Billing   │
│ Record           │
│ (Type: Subscription)
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Process Payment  │
│ via Stripe       │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Update Records   │
│ - BillingRecord  │
│ - SubscriptionPayment
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Reset Privileges │
│ (New Billing     │
│  Period)         │
└──────────────────┘
```

---

## Billing Types

### Enum Definition

```csharp
public enum BillingType
{
    Subscription,    // Plan subscription billing
    Consultation,    // One-time consultation
    Medication,      // Medication delivery
    LateFee,        // Late payment penalty
    Refund,         // Refund credits
    Recurring,      // Recurring subscription billing
    Upfront,        // Upfront payment
    Bundle,         // Bundled services
    Invoice,        // Invoice-based billing
    Overage,        // Usage overage charges
    Cycle           // Billing cycle charges
}
```

### Type Usage

| Type | When Created | Processed By |
|------|-------------|--------------|
| Subscription | Initial subscription | PaymentService |
| Recurring | Each billing cycle | AutomatedBillingService |
| Overage | Usage exceeds limit | SubscriptionBillingService |
| Refund | Cancellation/adjustment | PaymentService |

---

## Billing Record Creation

### BillingRecord Entity

```csharp
public class BillingRecord : BaseEntity
{
    public Guid Id { get; set; }
    
    // Foreign Keys
    public int UserId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Guid CurrencyId { get; set; }
    
    // Amounts
    public decimal Amount { get; set; }           // Base amount
    public decimal TaxAmount { get; set; }        // Tax
    public decimal ShippingAmount { get; set; }   // Shipping
    public decimal TotalAmount { get; set; }      // Final amount
    
    // Status
    public BillingStatus Status { get; set; }     // Pending, Paid, Failed, etc.
    public BillingType Type { get; set; }         // Subscription, Overage, etc.
    
    // Dates
    public DateTime BillingDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Stripe Integration
    public string? StripePaymentIntentId { get; set; }
    public string? StripeInvoiceId { get; set; }
    
    // Description
    public string? Description { get; set; }
    public string? InvoiceNumber { get; set; }
    
    // Relationships
    public virtual Subscription? Subscription { get; set; }
    public virtual ICollection<BillingAdjustment> Adjustments { get; set; }
}
```

### Creation Process

**Service**: `SubscriptionBillingService.CreateBillingRecordAsync()`

```csharp
public async Task<JsonModel> CreateBillingRecordAsync(
    CreateBillingRecordDto billingDto, 
    TokenModel tokenModel)
{
    await _unitOfWork.BeginTransactionAsync();
    try {
        // 1. Get subscription
        var subscription = await _subscriptionRepository
            .GetByIdWithDetailsAsync(billingDto.SubscriptionId);
        
        if (subscription == null) {
            return NotFound("Subscription not found");
        }
        
        // 2. Get plan for pricing
        var plan = subscription.SubscriptionPlan;
        
        // 3. Create billing record
        var billingRecord = new BillingRecord {
            Id = Guid.NewGuid(),
            UserId = subscription.UserId,
            SubscriptionId = subscription.Id,
            CurrencyId = plan.CurrencyId,
            
            // Calculate amounts
            Amount = billingDto.Amount ?? plan.EffectivePrice,
            TaxAmount = 0, // Calculate tax if needed
            ShippingAmount = 0,
            TotalAmount = billingDto.Amount ?? plan.EffectivePrice,
            
            // Set status and type
            Status = BillingRecord.BillingStatus.Pending,
            Type = billingDto.Type ?? BillingRecord.BillingType.Subscription,
            
            // Set dates
            BillingDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            
            // Description
            Description = billingDto.Description ?? 
                $"Billing for {plan.Name} subscription",
            
            // Audit
            CreatedBy = tokenModel.UserID,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };
        
        // 4. Save billing record
        await _billingRepository.AddAsync(billingRecord);
        await _billingRepository.SaveChangesAsync();
        
        await _unitOfWork.CommitTransactionAsync();
        
        return new JsonModel {
            data = billingRecord,
            Message = "Billing record created successfully",
            StatusCode = 200
        };
    }
    catch (Exception ex) {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Error creating billing record");
        throw;
    }
}
```

---

## Privilege-Based Billing

### Healthcare Pricing Model

The system uses a **privilege-based pricing model** where plan prices are calculated from constituent privileges:

**Formula**:
```
Plan Price = Σ(Privilege Value × Privilege Base Cost) + Admin Commission
```

### Plan Price Calculation

**Service**: `SubscriptionBillingService.CalculatePlanBasePriceAsync()`

```csharp
public async Task<JsonModel> CalculatePlanBasePriceAsync(
    CalculatePlanPriceDto calculateDto, 
    TokenModel tokenModel)
{
    // 1. Get plan
    var plan = await _subscriptionPlanRepository
        .GetByIdWithDetailsAsync(calculateDto.PlanId);
    
    // 2. Get plan privileges
    var planPrivileges = await _subscriptionPlanRepository
        .GetPlanPrivilegesAsync(calculateDto.PlanId);
    
    // 3. Calculate base price from privileges
    decimal totalBasePrice = 0;
    var privilegeBreakdown = new List<object>();
    
    foreach (var planPrivilege in planPrivileges) {
        var privilege = planPrivilege.Privilege;
        
        // Calculate privilege cost
        var privilegeLimit = planPrivilege.Value > 0 ? planPrivilege.Value : 0;
        var privilegeCost = privilegeLimit * planPrivilege.PrivilegeBaseCost;
        
        totalBasePrice += privilegeCost;
        
        privilegeBreakdown.Add(new {
            PrivilegeId = privilege.Id,
            PrivilegeName = privilege.Name,
            PrivilegeLimit = planPrivilege.Value,
            BaseCost = planPrivilege.PrivilegeBaseCost,
            TotalCost = privilegeCost
        });
    }
    
    // 4. Calculate admin commission
    var adminCommission = calculateDto.AdminCommissionPercentage > 0 
        ? totalBasePrice * (calculateDto.AdminCommissionPercentage / 100)
        : calculateDto.AdminCommissionFixed;
    
    // 5. Calculate final price
    var finalPrice = totalBasePrice + adminCommission;
    
    return new JsonModel {
        data = new {
            PlanId = calculateDto.PlanId,
            BasePrice = totalBasePrice,
            AdminCommission = adminCommission,
            FinalPrice = finalPrice,
            PrivilegeBreakdown = privilegeBreakdown
        },
        Message = "Plan price calculated successfully",
        StatusCode = 200
    };
}
```

### Example Calculation

**Plan**: Basic Healthcare Plan
- 5 Teleconsultations @ $3.00/consultation = $15.00
- 10 Chat Sessions @ $1.50/session = $15.00
- 2 Home Visits @ $10.00/visit = $20.00

**Subtotal**: $50.00  
**Admin Commission (20%)**: $10.00  
**Final Plan Price**: $60.00

---

## Overage Billing

### When Overages Occur

Overages occur when users exceed their privilege limits:

```csharp
// User has 5 consultations in plan
// User uses 6th consultation
// System creates overage charge
```

### Overage Calculation

**Service**: `SubscriptionBillingService.ProcessPrivilegeUsageAsync()`

```csharp
public async Task<JsonModel> ProcessPrivilegeUsageAsync(
    ProcessPrivilegeUsageDto usageDto, 
    TokenModel tokenModel)
{
    // 1. Get subscription
    var subscription = await _subscriptionRepository
        .GetActiveSubscriptionByUserIdAsync(usageDto.UserId);
    
    // 2. Get privilege usage tracking
    var privilegeUsage = await GetOrCreatePrivilegeUsageAsync(
        usageDto.UserId, 
        usageDto.PrivilegeId, 
        subscription.Id);
    
    // 3. Get plan privilege configuration
    var planPrivilege = await _subscriptionPlanRepository
        .GetPlanPrivilegeAsync(
            subscription.SubscriptionPlanId, 
            usageDto.PrivilegeId);
    
    // 4. Check if usage exceeds limit
    bool isOverage = false;
    decimal overageCharge = 0;
    
    if (planPrivilege.Value != -1) { // Not unlimited
        if (privilegeUsage.UsedValue >= privilegeUsage.AllowedValue) {
            isOverage = true;
            
            // Calculate overage charge
            // IMPORTANT: Use LATEST plan version pricing (anti-abuse)
            var latestPlanVersion = await GetLatestPlanVersionAsync(subscription.SubscriptionPlanId);
            var latestPrivilege = await _subscriptionPlanRepository
                .GetPlanPrivilegeAsync(latestPlanVersion.Id, usageDto.PrivilegeId);
            
            overageCharge = latestPrivilege.UnitCost;
        }
    }
    
    // 5. Increment usage
    privilegeUsage.UsedValue += usageDto.UsageValue;
    privilegeUsage.LastUsedAt = DateTime.UtcNow;
    await _privilegeUsageRepository.UpdateAsync(privilegeUsage);
    
    // 6. Create usage history
    var usageHistory = new PrivilegeUsageHistory {
        Id = Guid.NewGuid(),
        UserSubscriptionPrivilegeUsageId = privilegeUsage.Id,
        UsedValue = usageDto.UsageValue,
        UsedAt = DateTime.UtcNow,
        UsageDate = DateTime.UtcNow.Date,
        Notes = usageDto.Notes
    };
    await _usageHistoryRepo.AddAsync(usageHistory);
    
    // 7. If overage, create billing record
    if (isOverage && overageCharge > 0) {
        await CreateOverageBillingRecordAsync(
            subscription, 
            usageDto.PrivilegeId, 
            overageCharge, 
            tokenModel);
    }
    
    return new JsonModel {
        data = new {
            UsageRecorded = true,
            IsOverage = isOverage,
            OverageCharge = overageCharge,
            RemainingPrivileges = privilegeUsage.RemainingValue
        },
        Message = isOverage 
            ? $"Usage recorded. Overage charge: {overageCharge:C}"
            : "Usage recorded successfully",
        StatusCode = 200
    };
}
```

### Overage Billing Record Creation

```csharp
private async Task CreateOverageBillingRecordAsync(
    Subscription subscription, 
    Guid privilegeId, 
    decimal overageCharge, 
    TokenModel tokenModel)
{
    var privilege = await _privilegeRepository.GetByIdAsync(privilegeId);
    var plan = await _subscriptionPlanRepository
        .GetByIdWithDetailsAsync(subscription.SubscriptionPlanId);
    
    var billingRecord = new BillingRecord {
        Id = Guid.NewGuid(),
        UserId = subscription.UserId,
        SubscriptionId = subscription.Id,
        CurrencyId = plan.CurrencyId,
        
        Amount = overageCharge,
        TotalAmount = overageCharge,
        TaxAmount = 0,
        ShippingAmount = 0,
        
        Status = BillingRecord.BillingStatus.Pending,
        Type = BillingRecord.BillingType.Overage,
        
        Description = $"Overage charge for {privilege.Name} - {overageCharge:C}",
        BillingDate = DateTime.UtcNow,
        DueDate = DateTime.UtcNow.AddDays(7),
        
        CreatedBy = tokenModel.UserID,
        CreatedDate = DateTime.UtcNow,
        IsActive = true
    };
    
    await _billingRepository.AddAsync(billingRecord);
    await _billingRepository.SaveChangesAsync();
    
    // Notify user
    await _notificationService.SendOverageNotificationAsync(
        subscription.UserId, 
        privilege.Name, 
        overageCharge);
}
```

---

## Recurring Billing

### Automated Recurring Billing Process

**Service**: `AutomatedBillingService.ProcessRecurringBillingAsync()`

**Trigger**: Background service runs daily

```csharp
public async Task ProcessRecurringBillingAsync(TokenModel tokenModel)
{
    _logger.LogInformation("Starting recurring billing process");
    
    // 1. Get subscriptions due for billing
    var dueSubscriptions = await _subscriptionRepository
        .GetSubscriptionsDueForBillingAsync(DateTime.UtcNow);
    
    _logger.LogInformation(
        "Found {Count} subscriptions due for billing", 
        dueSubscriptions.Count());
    
    // 2. Process each subscription
    foreach (var subscription in dueSubscriptions) {
        try {
            await ProcessSubscriptionBillingAsync(subscription, tokenModel);
        }
        catch (Exception ex) {
            _logger.LogError(ex, 
                "Error processing billing for subscription {SubscriptionId}", 
                subscription.Id);
        }
    }
    
    _logger.LogInformation("Completed recurring billing process");
}
```

### Individual Subscription Billing

```csharp
private async Task ProcessSubscriptionBillingAsync(
    Subscription subscription, 
    TokenModel tokenModel)
{
    await _unitOfWork.BeginTransactionAsync();
    try {
        _logger.LogInformation(
            "Processing billing for subscription {SubscriptionId}", 
            subscription.Id);
        
        // 1. Create recurring billing record
        var billingRecord = await CreateRecurringBillingRecordAsync(
            subscription, tokenModel);
        
        // 2. Process payment
        var paymentResult = await _billingService.ProcessPaymentAsync(
            billingRecord.Id, tokenModel);
        
        if (paymentResult.StatusCode == 200) {
            // 3. Update subscription
            subscription.LastBillingDate = DateTime.UtcNow;
            subscription.NextBillingDate = BillingCycleCalculator
                .CalculateNextBillingDate(
                    DateTime.UtcNow, 
                    subscription.BillingCycle);
            
            await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
            
            // 4. Reset privileges for new billing period
            await ResetPrivilegesForNewPeriodAsync(subscription, tokenModel);
            
            _logger.LogInformation(
                "Successfully processed billing for subscription {SubscriptionId}. Next billing: {NextBillingDate}",
                subscription.Id, subscription.NextBillingDate);
        }
        else {
            // Handle payment failure
            subscription.FailedPaymentAttempts++;
            subscription.LastPaymentFailedDate = DateTime.UtcNow;
            subscription.LastPaymentError = paymentResult.Message;
            
            if (subscription.FailedPaymentAttempts >= 3) {
                subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
                
                // Notify user
                await _subscriptionNotificationService
                    .SendPaymentFailedNotificationAsync(subscription.Id, tokenModel);
            }
            
            await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
            
            _logger.LogWarning(
                "Payment failed for subscription {SubscriptionId}. Attempt: {AttemptCount}",
                subscription.Id, subscription.FailedPaymentAttempts);
        }
        
        await _unitOfWork.CommitTransactionAsync();
    }
    catch (Exception ex) {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, 
            "Error in recurring billing for subscription {SubscriptionId}", 
            subscription.Id);
        throw;
    }
}
```

### Creating Recurring Billing Record

```csharp
private async Task<BillingRecord> CreateRecurringBillingRecordAsync(
    Subscription subscription, 
    TokenModel tokenModel)
{
    var plan = subscription.SubscriptionPlan;
    
    var billingRecord = new BillingRecord {
        Id = Guid.NewGuid(),
        UserId = subscription.UserId,
        SubscriptionId = subscription.Id,
        CurrencyId = plan.CurrencyId,
        
        // Amount calculation
        Amount = subscription.CurrentPrice,
        TaxAmount = 0,
        ShippingAmount = 0,
        TotalAmount = subscription.CurrentPrice,
        
        // Status and type
        Status = BillingRecord.BillingStatus.Pending,
        Type = BillingRecord.BillingType.Recurring,
        
        // Dates
        BillingDate = DateTime.UtcNow,
        DueDate = DateTime.UtcNow.AddDays(7),
        
        // Description
        Description = $"Recurring billing for {plan.Name} subscription",
        
        // Recurring flag
        IsRecurring = true,
        NextBillingDate = BillingCycleCalculator.CalculateNextBillingDate(
            DateTime.UtcNow, subscription.BillingCycle),
        
        // Audit
        CreatedBy = tokenModel.UserID,
        CreatedDate = DateTime.UtcNow,
        IsActive = true
    };
    
    await _billingRepository.AddAsync(billingRecord);
    await _billingRepository.SaveChangesAsync();
    
    return billingRecord;
}
```

---

## Billing Adjustments

### Adjustment Types

```csharp
public enum AdjustmentType
{
    Credit,      // Add credit to user
    Debit,       // Add charge to user
    Discount,    // Apply discount
    Refund,      // Refund amount
    ProRated,    // Prorated adjustment
    Tax,         // Tax adjustment
    Fee          // Additional fee
}
```

### Creating Adjustments

**Service**: `SubscriptionBillingService.CreateBillingAdjustmentAsync()`

```csharp
public async Task<JsonModel> CreateBillingAdjustmentAsync(
    CreateBillingAdjustmentDto adjustmentDto, 
    TokenModel tokenModel)
{
    // 1. Get billing record
    var billingRecord = await _billingRepository
        .GetByIdAsync(adjustmentDto.BillingRecordId);
    
    if (billingRecord == null) {
        return NotFound("Billing record not found");
    }
    
    // 2. Calculate adjustment amount
    decimal adjustmentAmount = adjustmentDto.Amount;
    
    if (adjustmentDto.IsPercentage && adjustmentDto.Percentage.HasValue) {
        adjustmentAmount = billingRecord.Amount * 
            (adjustmentDto.Percentage.Value / 100);
    }
    
    // 3. Create adjustment record
    var adjustment = new BillingAdjustment {
        Id = Guid.NewGuid(),
        BillingRecordId = billingRecord.Id,
        Type = adjustmentDto.Type,
        
        Amount = adjustmentAmount,
        Description = adjustmentDto.Description,
        Reason = adjustmentDto.Reason,
        
        AppliedAt = DateTime.UtcNow,
        AppliedBy = tokenModel.UserID,
        IsApproved = adjustmentDto.RequiresApproval ? false : true,
        
        CreatedBy = tokenModel.UserID,
        CreatedDate = DateTime.UtcNow
    };
    
    // 4. Update billing record total
    if (adjustment.IsApproved) {
        billingRecord.TotalAmount += adjustmentAmount;
        await _billingRepository.UpdateAsync(billingRecord);
    }
    
    // 5. Save adjustment
    await _billingRepository.AddAdjustmentAsync(adjustment);
    await _billingRepository.SaveChangesAsync();
    
    return new JsonModel {
        data = adjustment,
        Message = "Billing adjustment created successfully",
        StatusCode = 200
    };
}
```

---

## Automated Billing Process

### Background Service

**Service**: `AutomatedBillingBackgroundService`

```csharp
public class AutomatedBillingBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait 1 minute before first run
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try {
                await ProcessDailyBillingAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error in automated billing");
            }
            
            // Run daily at midnight
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
    
    private async Task ProcessDailyBillingAsync()
    {
        var tokenModel = new TokenModel { UserID = 0, RoleID = (int)RoleId.System };
        
        // 1. Process recurring billing
        await _automatedBillingService.ProcessRecurringBillingAsync(tokenModel);
        
        // 2. Process failed payment retries
        await _automatedBillingService.ProcessFailedPaymentRetryAsync(tokenModel);
        
        // 3. Process subscription renewals
        await _automatedBillingService.ProcessSubscriptionRenewalAsync(tokenModel);
    }
}
```

### Billing Due Date Calculation

```csharp
public static class BillingCycleCalculator
{
    public static DateTime CalculateNextBillingDate(
        DateTime currentDate, 
        MasterBillingCycle billingCycle)
    {
        return billingCycle.IntervalUnit switch
        {
            "day" => currentDate.AddDays(billingCycle.IntervalCount),
            "week" => currentDate.AddDays(billingCycle.IntervalCount * 7),
            "month" => currentDate.AddMonths(billingCycle.IntervalCount),
            "year" => currentDate.AddYears(billingCycle.IntervalCount),
            _ => currentDate.AddMonths(1) // Default monthly
        };
    }
}
```

---

## Summary

The billing mechanism provides:
- **Multiple billing types** (subscription, overage, recurring)
- **Privilege-based pricing** with healthcare model
- **Automated recurring billing** via background service
- **Overage detection and charging** with latest version pricing
- **Flexible adjustments** (credits, debits, discounts)
- **Transaction-safe operations** with rollback support
- **Comprehensive auditing** of all billing events

**Next**: See [03_PAYMENT_PROCESSING.md](./03_PAYMENT_PROCESSING.md) for payment details.

---

*Document Version: 1.0*  
*Last Updated: 2025*




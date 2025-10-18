# 🔧 SUBSCRIPTION PLANS - CRITICAL FIXES IMPLEMENTATION GUIDE

## 📋 Overview

This document provides **actionable code fixes** for the critical issues identified in the Subscription Plans Management Analysis. Each fix includes:
- Problem statement
- Code to add/modify
- Testing scenarios
- Migration considerations

---

## 🚨 FIX #1: Add Active Subscription Protection in UpdatePlanAsync

### **Current Issue:**
`UpdatePlanAsync` doesn't check if price/privilege changes affect active subscriptions.

### **Fix Implementation:**

#### **Step 1: Add Helper Method to SubscriptionPlanService**

```csharp
/// <summary>
/// Validates if plan changes will affect active subscriptions and require confirmation
/// </summary>
private async Task<PlanUpdateValidationResult> ValidatePlanUpdateImpactAsync(
    SubscriptionPlan existingPlan,
    UpdateSubscriptionPlanDto updateDto,
    TokenModel tokenModel)
{
    var result = new PlanUpdateValidationResult
    {
        CanProceed = true,
        RequiresConfirmation = false,
        AffectedSubscriptions = new List<Guid>(),
        Warnings = new List<string>()
    };
    
    // Check if there are active subscriptions
    var hasActiveSubscriptions = await _subscriptionPlanRepository.HasActiveSubscriptionsAsync(existingPlan.Id);
    
    if (!hasActiveSubscriptions)
    {
        return result; // No active subscriptions, safe to update
    }
    
    // Get count of active subscriptions
    var activeSubscriptions = await _subscriptionRepository.GetByPlanIdAsync(existingPlan.Id);
    var activeCount = activeSubscriptions.Count(s => s.Status == "Active" || s.Status == "TrialActive");
    
    // Check for price changes
    if (updateDto.Price > 0 && updateDto.Price != existingPlan.Price)
    {
        var priceChange = updateDto.Price - existingPlan.Price;
        var priceChangePercent = (priceChange / existingPlan.Price) * 100;
        
        result.RequiresConfirmation = true;
        result.AffectedSubscriptions = activeSubscriptions.Select(s => s.Id).ToList();
        result.Warnings.Add(
            $"Price change from ${existingPlan.Price} to ${updateDto.Price} " +
            $"({priceChangePercent:F1}% {(priceChange > 0 ? "increase" : "decrease")}) " +
            $"will affect {activeCount} active subscriptions at their next renewal."
        );
        
        // Large price increase requires special handling
        if (priceChangePercent > 20)
        {
            result.Warnings.Add(
                $"⚠️ WARNING: Price increase exceeds 20%. " +
                $"Consider grandfathering existing users or providing advance notice."
            );
        }
    }
    
    // Check for plan deactivation
    if (updateDto.IsActive == false && existingPlan.IsActive == true)
    {
        result.RequiresConfirmation = true;
        result.AffectedSubscriptions = activeSubscriptions.Select(s => s.Id).ToList();
        result.Warnings.Add(
            $"Deactivating plan will prevent new subscriptions. " +
            $"{activeCount} existing subscriptions will continue until renewal."
        );
    }
    
    return result;
}

public class PlanUpdateValidationResult
{
    public bool CanProceed { get; set; }
    public bool RequiresConfirmation { get; set; }
    public List<Guid> AffectedSubscriptions { get; set; }
    public List<string> Warnings { get; set; }
}
```

#### **Step 2: Modify UpdatePlanAsync to Use Validation**

```csharp
public async Task<JsonModel> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updateDto, TokenModel tokenModel)
{
    try
    {
        // ... existing validation ...
        
        var existingPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
        if (existingPlan == null)
        {
            return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
        }
        
        // ✅ NEW: Validate impact on active subscriptions
        var validationResult = await ValidatePlanUpdateImpactAsync(existingPlan, updateDto, tokenModel);
        
        if (validationResult.RequiresConfirmation && !updateDto.ConfirmImpact)
        {
            return new JsonModel
            {
                data = new
                {
                    requiresConfirmation = true,
                    affectedSubscriptionsCount = validationResult.AffectedSubscriptions.Count,
                    warnings = validationResult.Warnings,
                    impact = new
                    {
                        priceChange = updateDto.Price - existingPlan.Price,
                        priceChangePercent = ((updateDto.Price - existingPlan.Price) / existingPlan.Price) * 100,
                        affectedUsers = validationResult.AffectedSubscriptions.Count
                    }
                },
                Message = "Plan update will affect active subscriptions. Please review and confirm.",
                StatusCode = 409  // Conflict - requires user decision
            };
        }
        
        // ... continue with update ...
    }
    catch (Exception ex)
    {
        // ... error handling ...
    }
}
```

#### **Step 3: Add ConfirmImpact Property to UpdateDto**

```csharp
public class UpdateSubscriptionPlanDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; }
    public int? DisplayOrder { get; set; }
    
    // ✅ NEW: Confirmation flag for impactful changes
    public bool ConfirmImpact { get; set; } = false;
    
    // ✅ NEW: Options for handling existing subscriptions
    public PlanUpdateStrategy Strategy { get; set; } = PlanUpdateStrategy.ApplyAtRenewal;
}

public enum PlanUpdateStrategy
{
    ApplyImmediately,      // Apply changes to all subscriptions now
    ApplyAtRenewal,        // Apply changes at next renewal (default)
    GrandfatherExisting    // Keep existing subscriptions on old terms
}
```

---

## 🚨 FIX #2: Implement Plan Versioning

### **Problem:**
No versioning system for tracking plan changes over time.

### **Fix Implementation:**

#### **Step 1: Add Version Fields to SubscriptionPlan Entity**

```csharp
// Add to SmartTelehealth.Core\Entities\SubscriptionPlan.cs

public class SubscriptionPlan : BaseEntity
{
    // ... existing fields ...
    
    // ✅ NEW: Versioning Support
    public int VersionNumber { get; set; } = 1;
    public Guid? ParentPlanId { get; set; }  // Link to original plan
    public bool IsLatestVersion { get; set; } = true;
    public DateTime? VersionEffectiveDate { get; set; }
    public DateTime? VersionRetiredDate { get; set; }
    public string? VersionChangeNotes { get; set; }
    
    // Navigation to parent plan
    public virtual SubscriptionPlan? ParentPlan { get; set; }
    public virtual ICollection<SubscriptionPlan> VersionHistory { get; set; } = new List<SubscriptionPlan>();
}
```

#### **Step 2: Modify Database Migration**

```csharp
// Add migration: dotnet ef migrations add AddPlanVersioning

public partial class AddPlanVersioning : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "VersionNumber",
            table: "SubscriptionPlans",
            type: "int",
            nullable: false,
            defaultValue: 1);
            
        migrationBuilder.AddColumn<Guid>(
            name: "ParentPlanId",
            table: "SubscriptionPlans",
            type: "uniqueidentifier",
            nullable: true);
            
        migrationBuilder.AddColumn<bool>(
            name: "IsLatestVersion",
            table: "SubscriptionPlans",
            type: "bit",
            nullable: false,
            defaultValue: true);
            
        migrationBuilder.AddColumn<DateTime>(
            name: "VersionEffectiveDate",
            table: "SubscriptionPlans",
            type: "datetime2",
            nullable: true);
            
        migrationBuilder.AddColumn<DateTime>(
            name: "VersionRetiredDate",
            table: "SubscriptionPlans",
            type: "datetime2",
            nullable: true);
            
        migrationBuilder.AddColumn<string>(
            name: "VersionChangeNotes",
            table: "SubscriptionPlans",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true);
            
        // Add foreign key constraint
        migrationBuilder.CreateIndex(
            name: "IX_SubscriptionPlans_ParentPlanId",
            table: "SubscriptionPlans",
            column: "ParentPlanId");
            
        migrationBuilder.AddForeignKey(
            name: "FK_SubscriptionPlans_SubscriptionPlans_ParentPlanId",
            table: "SubscriptionPlans",
            column: "ParentPlanId",
            principalTable: "SubscriptionPlans",
            principalColumn: "Id",
            onDelete: ReferentialAction.NoAction);
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_SubscriptionPlans_SubscriptionPlans_ParentPlanId",
            table: "SubscriptionPlans");
            
        migrationBuilder.DropIndex(
            name: "IX_SubscriptionPlans_ParentPlanId",
            table: "SubscriptionPlans");
            
        migrationBuilder.DropColumn(name: "VersionNumber", table: "SubscriptionPlans");
        migrationBuilder.DropColumn(name: "ParentPlanId", table: "SubscriptionPlans");
        migrationBuilder.DropColumn(name: "IsLatestVersion", table: "SubscriptionPlans");
        migrationBuilder.DropColumn(name: "VersionEffectiveDate", table: "SubscriptionPlans");
        migrationBuilder.DropColumn(name: "VersionRetiredDate", table: "SubscriptionPlans");
        migrationBuilder.DropColumn(name: "VersionChangeNotes", table: "SubscriptionPlans");
    }
}
```

#### **Step 3: Create Plan Versioning Service**

```csharp
// Add new file: SmartTelehealth.Application\Services\PlanVersioningService.cs

public interface IPlanVersioningService
{
    Task<JsonModel> CreatePlanVersionAsync(Guid planId, UpdateSubscriptionPlanDto updateDto, string changeNotes, TokenModel tokenModel);
    Task<JsonModel> GetPlanVersionHistoryAsync(Guid planId, TokenModel tokenModel);
    Task<JsonModel> GetPlanVersionAsync(Guid planId, int version, TokenModel tokenModel);
    Task<JsonModel> MigrateSubscriptionsToPlanVersionAsync(Guid oldPlanId, Guid newPlanId, TokenModel tokenModel);
}

public class PlanVersioningService : IPlanVersioningService
{
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ILogger<PlanVersioningService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<JsonModel> CreatePlanVersionAsync(
        Guid planId, 
        UpdateSubscriptionPlanDto updateDto, 
        string changeNotes, 
        TokenModel tokenModel)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // 1. Get existing plan
            var existingPlan = await _planRepo.GetByIdWithDetailsAsync(planId);
            if (existingPlan == null)
            {
                return new JsonModel { Message = "Plan not found", StatusCode = 404 };
            }
            
            // 2. Mark existing plan as not latest
            existingPlan.IsLatestVersion = false;
            existingPlan.VersionRetiredDate = DateTime.UtcNow;
            existingPlan.UpdatedDate = DateTime.UtcNow;
            existingPlan.UpdatedBy = tokenModel.UserID;
            await _planRepo.UpdatePlanAsync(existingPlan);
            
            // 3. Create new version
            var newVersion = new SubscriptionPlan
            {
                // Copy all properties from existing plan
                Name = updateDto.Name ?? existingPlan.Name,
                Description = updateDto.Description ?? existingPlan.Description,
                Price = updateDto.Price > 0 ? updateDto.Price : existingPlan.Price,
                BillingCycleId = existingPlan.BillingCycleId,
                CurrencyId = existingPlan.CurrencyId,
                CategoryId = updateDto.CategoryId != Guid.Empty ? updateDto.CategoryId : existingPlan.CategoryId,
                IsActive = updateDto.IsActive,
                DisplayOrder = updateDto.DisplayOrder ?? existingPlan.DisplayOrder,
                
                // Version tracking
                VersionNumber = existingPlan.VersionNumber + 1,
                ParentPlanId = existingPlan.ParentPlanId ?? existingPlan.Id,
                IsLatestVersion = true,
                VersionEffectiveDate = DateTime.UtcNow,
                VersionChangeNotes = changeNotes,
                
                // Stripe IDs will be created separately
                StripeProductId = null,
                StripeMonthlyPriceId = null,
                StripeQuarterlyPriceId = null,
                StripeAnnualPriceId = null,
                
                // Audit fields
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            
            var createdVersion = await _planRepo.CreatePlanAsync(newVersion);
            
            // 4. Copy privileges from old plan to new plan
            var existingPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(existingPlan.Id);
            foreach (var privilege in existingPrivileges)
            {
                var newPrivilege = new SubscriptionPlanPrivilege
                {
                    SubscriptionPlanId = createdVersion.Id,
                    PrivilegeId = privilege.PrivilegeId,
                    Value = privilege.Value,
                    UsagePeriodId = privilege.UsagePeriodId,
                    DurationMonths = privilege.DurationMonths,
                    DailyLimit = privilege.DailyLimit,
                    WeeklyLimit = privilege.WeeklyLimit,
                    MonthlyLimit = privilege.MonthlyLimit,
                    UnitCost = privilege.UnitCost,
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow
                };
                
                await _planPrivilegeRepo.AddAsync(newPrivilege);
            }
            
            // 5. Create Stripe resources for new version
            var stripeProductId = await _stripeService.CreateProductAsync(
                $"{createdVersion.Name} (v{createdVersion.VersionNumber})",
                createdVersion.Description ?? "",
                tokenModel
            );
            
            createdVersion.StripeProductId = stripeProductId;
            createdVersion.StripeMonthlyPriceId = await _stripeService.CreatePriceAsync(
                stripeProductId, createdVersion.Price, "usd", "month", 1, tokenModel);
            createdVersion.StripeQuarterlyPriceId = await _stripeService.CreatePriceAsync(
                stripeProductId, createdVersion.Price * 3, "usd", "month", 3, tokenModel);
            createdVersion.StripeAnnualPriceId = await _stripeService.CreatePriceAsync(
                stripeProductId, createdVersion.Price * 12, "usd", "month", 12, tokenModel);
            
            await _planRepo.UpdatePlanAsync(createdVersion);
            
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation(
                "Created plan version {Version} for plan {PlanName}. {AffectedCount} subscriptions remain on v{OldVersion}",
                createdVersion.VersionNumber,
                createdVersion.Name,
                existingPrivileges.Count(),
                existingPlan.VersionNumber
            );
            
            return new JsonModel
            {
                data = new
                {
                    newVersion = createdVersion,
                    versionNumber = createdVersion.VersionNumber,
                    affectedSubscriptions = existingPrivileges.Count(),
                    changeNotes = changeNotes
                },
                Message = $"Plan version {createdVersion.VersionNumber} created successfully",
                StatusCode = 201
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating plan version for plan {PlanId}", planId);
            return new JsonModel { Message = "Failed to create plan version", StatusCode = 500 };
        }
    }
}
```

#### **Step 4: Add Controller Endpoint**

```csharp
// Add to SubscriptionPlansController.cs

/// <summary>
/// Creates a new version of an existing plan (recommended for major changes)
/// </summary>
[HttpPost("admin/{planId}/create-version")]
public async Task<JsonModel> CreatePlanVersion(
    string planId,
    [FromBody] CreatePlanVersionDto dto)
{
    return await _planVersioningService.CreatePlanVersionAsync(
        Guid.Parse(planId),
        dto.UpdateDto,
        dto.ChangeNotes,
        GetToken(HttpContext)
    );
}

/// <summary>
/// Gets version history for a plan
/// </summary>
[HttpGet("admin/{planId}/versions")]
public async Task<JsonModel> GetPlanVersionHistory(string planId)
{
    return await _planVersioningService.GetPlanVersionHistoryAsync(
        Guid.Parse(planId),
        GetToken(HttpContext)
    );
}
```

---

## 🚨 FIX #3: Add Privilege Validation Logic

### **Fix Implementation:**

```csharp
// Add to SubscriptionPlanService.cs

/// <summary>
/// Validates privilege configuration for logical consistency
/// </summary>
private async Task<PrivilegeValidationResult> ValidatePrivilegeConfigurationAsync(
    List<PlanPrivilegeDto> privileges)
{
    var result = new PrivilegeValidationResult
    {
        IsValid = true,
        Errors = new List<string>(),
        Warnings = new List<string>()
    };
    
    // ══════════════════════════════════════════════════════
    // 1. CHECK FOR DUPLICATE PRIVILEGES
    // ══════════════════════════════════════════════════════
    var duplicates = privileges
        .GroupBy(p => p.PrivilegeId)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key)
        .ToList();
    
    if (duplicates.Any())
    {
        foreach (var duplicateId in duplicates)
        {
            var privilege = await _privilegeRepository.GetByIdAsync(duplicateId);
            result.Errors.Add($"Privilege '{privilege?.Name}' is assigned multiple times");
        }
        result.IsValid = false;
    }
    
    // ══════════════════════════════════════════════════════
    // 2. VALIDATE TIME-BASED LIMITS CONSISTENCY
    // ══════════════════════════════════════════════════════
    foreach (var privilege in privileges)
    {
        var privilegeEntity = await _privilegeRepository.GetByIdAsync(privilege.PrivilegeId);
        var privilegeName = privilegeEntity?.Name ?? "Unknown";
        
        // Daily limit should not exceed weekly limit
        if (privilege.DailyLimit.HasValue && privilege.WeeklyLimit.HasValue)
        {
            if (privilege.DailyLimit.Value * 7 > privilege.WeeklyLimit.Value)
            {
                result.Errors.Add(
                    $"Privilege '{privilegeName}': Daily limit ({privilege.DailyLimit}) × 7 " +
                    $"exceeds weekly limit ({privilege.WeeklyLimit})"
                );
                result.IsValid = false;
            }
        }
        
        // Weekly limit should not exceed monthly limit
        if (privilege.WeeklyLimit.HasValue && privilege.MonthlyLimit.HasValue)
        {
            if (privilege.WeeklyLimit.Value * 4 > privilege.MonthlyLimit.Value)
            {
                result.Errors.Add(
                    $"Privilege '{privilegeName}': Weekly limit ({privilege.WeeklyLimit}) × 4 " +
                    $"exceeds monthly limit ({privilege.MonthlyLimit})"
                );
                result.IsValid = false;
            }
        }
        
        // Monthly limit should not exceed total value (if limited)
        if (privilege.MonthlyLimit.HasValue && privilege.Value > 0)
        {
            if (privilege.MonthlyLimit.Value > privilege.Value)
            {
                result.Errors.Add(
                    $"Privilege '{privilegeName}': Monthly limit ({privilege.MonthlyLimit}) " +
                    $"exceeds total allowed value ({privilege.Value})"
                );
                result.IsValid = false;
            }
        }
        
        // Time-based limits without total value should have warning
        if ((privilege.DailyLimit.HasValue || privilege.WeeklyLimit.HasValue || privilege.MonthlyLimit.HasValue) 
            && privilege.Value <= 0)
        {
            result.Warnings.Add(
                $"Privilege '{privilegeName}': Time-based limits set but total value is " +
                $"{(privilege.Value == -1 ? "unlimited" : "disabled")}"
            );
        }
    }
    
    // ══════════════════════════════════════════════════════
    // 3. VALIDATE UNIT COST FOR LIMITED PRIVILEGES
    // ══════════════════════════════════════════════════════
    foreach (var privilege in privileges)
    {
        if (privilege.Value > 0 && privilege.UnitCost <= 0)
        {
            var privilegeEntity = await _privilegeRepository.GetByIdAsync(privilege.PrivilegeId);
            result.Warnings.Add(
                $"Privilege '{privilegeEntity?.Name}': Limited privilege with no overage cost. " +
                $"Users cannot purchase additional usage."
            );
        }
    }
    
    // ══════════════════════════════════════════════════════
    // 4. VALIDATE AT LEAST ONE PRIVILEGE IS ENABLED
    // ══════════════════════════════════════════════════════
    if (privileges.All(p => p.Value == 0))
    {
        result.Errors.Add("Plan must have at least one enabled privilege");
        result.IsValid = false;
    }
    
    // ══════════════════════════════════════════════════════
    // 5. VALIDATE USAGE PERIOD EXISTS
    // ══════════════════════════════════════════════════════
    var usagePeriodIds = privileges.Select(p => p.UsagePeriodId).Distinct();
    foreach (var usagePeriodId in usagePeriodIds)
    {
        var exists = await _billingCycleRepo.ExistsAsync(usagePeriodId);
        if (!exists)
        {
            result.Errors.Add($"Usage period {usagePeriodId} does not exist");
            result.IsValid = false;
        }
    }
    
    return result;
}

public class PrivilegeValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; }
    public List<string> Warnings { get; set; }
}
```

#### **Step 5: Use Validation in CreatePlanAsync**

```csharp
public async Task<JsonModel> CreatePlanAsync(CreateSubscriptionPlanDto createDto, TokenModel tokenModel)
{
    // ... existing validation ...
    
    // ✅ NEW: Validate privilege configuration
    if (createDto.Privileges != null && createDto.Privileges.Any())
    {
        var privilegeValidation = await ValidatePrivilegeConfigurationAsync(createDto.Privileges);
        
        if (!privilegeValidation.IsValid)
        {
            return new JsonModel
            {
                data = new
                {
                    errors = privilegeValidation.Errors,
                    warnings = privilegeValidation.Warnings
                },
                Message = "Privilege configuration validation failed",
                StatusCode = 400
            };
        }
        
        // Log warnings even if validation passes
        if (privilegeValidation.Warnings.Any())
        {
            _logger.LogWarning(
                "Plan '{PlanName}' created with warnings: {Warnings}",
                createDto.Name,
                string.Join("; ", privilegeValidation.Warnings)
            );
        }
    }
    
    // ... continue with creation ...
}
```

---

## 🚨 FIX #4: Add Stripe Orphaned Resource Tracking

### **Fix Implementation:**

#### **Step 1: Create OrphanedStripeResource Entity**

```csharp
// Add new file: SmartTelehealth.Core\Entities\StripeOrphanedResource.cs

public class StripeOrphanedResource : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Stripe resource ID (product, price, subscription, etc.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string StripeResourceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of Stripe resource (Product, Price, Subscription, Customer)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ResourceType { get; set; } = string.Empty;
    
    /// <summary>
    /// Operation that failed (Create, Update, Delete)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string FailedOperation { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity ID in local database (Plan, Subscription, etc.)
    /// </summary>
    public Guid? LocalEntityId { get; set; }
    
    /// <summary>
    /// Local entity type (SubscriptionPlan, Subscription, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? LocalEntityType { get; set; }
    
    /// <summary>
    /// Reason for cleanup failure
    /// </summary>
    [MaxLength(1000)]
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// Number of cleanup retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Maximum retry attempts before giving up
    /// </summary>
    public int MaxRetries { get; set; } = 5;
    
    /// <summary>
    /// Date and time of last retry attempt
    /// </summary>
    public DateTime? LastRetryAt { get; set; }
    
    /// <summary>
    /// Date and time when the resource will be retried next
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
    
    /// <summary>
    /// Whether the cleanup has been successfully completed
    /// </summary>
    public bool IsResolved { get; set; } = false;
    
    /// <summary>
    /// Date and time when the resource was resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
    
    /// <summary>
    /// Whether admin has been notified about this orphaned resource
    /// </summary>
    public bool AdminNotified { get; set; } = false;
    
    /// <summary>
    /// Whether to skip this resource (manual resolution required)
    /// </summary>
    public bool SkipCleanup { get; set; } = false;
    
    /// <summary>
    /// Additional context or metadata about the orphaned resource
    /// </summary>
    [MaxLength(2000)]
    public string? Metadata { get; set; }
}
```

#### **Step 2: Create Repository**

```csharp
// Add new file: SmartTelehealth.Core\Interfaces\IStripeOrphanedResourceRepository.cs

public interface IStripeOrphanedResourceRepository : IRepositoryBase<StripeOrphanedResource>
{
    Task<List<StripeOrphanedResource>> GetUnresolvedResourcesAsync();
    Task<List<StripeOrphanedResource>> GetResourcesDueForRetryAsync();
    Task<StripeOrphanedResource?> GetByStripeResourceIdAsync(string stripeResourceId);
    Task<int> GetUnresolvedCountAsync();
}

// Add implementation: SmartTelehealth.Infrastructure\Repositories\StripeOrphanedResourceRepository.cs

public class StripeOrphanedResourceRepository : RepositoryBase<StripeOrphanedResource>, IStripeOrphanedResourceRepository
{
    private readonly ApplicationDbContext _context;
    
    public StripeOrphanedResourceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<List<StripeOrphanedResource>> GetUnresolvedResourcesAsync()
    {
        return await _context.StripeOrphanedResources
            .Where(r => !r.IsResolved && !r.SkipCleanup && !r.IsDeleted)
            .OrderBy(r => r.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<List<StripeOrphanedResource>> GetResourcesDueForRetryAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.StripeOrphanedResources
            .Where(r => !r.IsResolved && 
                       !r.SkipCleanup && 
                       !r.IsDeleted &&
                       r.RetryCount < r.MaxRetries &&
                       (r.NextRetryAt == null || r.NextRetryAt <= now))
            .OrderBy(r => r.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<StripeOrphanedResource?> GetByStripeResourceIdAsync(string stripeResourceId)
    {
        return await _context.StripeOrphanedResources
            .FirstOrDefaultAsync(r => r.StripeResourceId == stripeResourceId && !r.IsDeleted);
    }
    
    public async Task<int> GetUnresolvedCountAsync()
    {
        return await _context.StripeOrphanedResources
            .CountAsync(r => !r.IsResolved && !r.SkipCleanup && !r.IsDeleted);
    }
}
```

#### **Step 3: Create Cleanup Service**

```csharp
// Add new file: SmartTelehealth.Application\Services\StripeCleanupService.cs

public interface IStripeCleanupService
{
    Task TrackOrphanedResourceAsync(string stripeResourceId, string resourceType, string operation, 
        Guid? localEntityId = null, string? localEntityType = null, string? failureReason = null);
    Task<JsonModel> RetryOrphanedResourceCleanupAsync(Guid orphanedResourceId, TokenModel tokenModel);
    Task<JsonModel> GetOrphanedResourcesAsync(TokenModel tokenModel);
    Task RunBackgroundCleanupAsync();
}

public class StripeCleanupService : IStripeCleanupService
{
    private readonly IStripeOrphanedResourceRepository _orphanedRepo;
    private readonly IStripeService _stripeService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<StripeCleanupService> _logger;
    
    public async Task TrackOrphanedResourceAsync(
        string stripeResourceId,
        string resourceType,
        string operation,
        Guid? localEntityId = null,
        string? localEntityType = null,
        string? failureReason = null)
    {
        try
        {
            // Check if already tracked
            var existing = await _orphanedRepo.GetByStripeResourceIdAsync(stripeResourceId);
            if (existing != null && !existing.IsResolved)
            {
                // Update existing record
                existing.RetryCount++;
                existing.LastRetryAt = DateTime.UtcNow;
                existing.NextRetryAt = CalculateNextRetryTime(existing.RetryCount);
                existing.FailureReason = failureReason ?? existing.FailureReason;
                existing.UpdatedDate = DateTime.UtcNow;
                
                await _orphanedRepo.UpdateAsync(existing);
                
                _logger.LogWarning(
                    "Updated orphaned resource tracking: {ResourceId} (Retry {Count}/{Max})",
                    stripeResourceId, existing.RetryCount, existing.MaxRetries
                );
                
                return;
            }
            
            // Create new tracking record
            var orphanedResource = new StripeOrphanedResource
            {
                StripeResourceId = stripeResourceId,
                ResourceType = resourceType,
                FailedOperation = operation,
                LocalEntityId = localEntityId,
                LocalEntityType = localEntityType,
                FailureReason = failureReason,
                RetryCount = 0,
                MaxRetries = 5,
                NextRetryAt = CalculateNextRetryTime(0),
                IsResolved = false,
                AdminNotified = false,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            
            await _orphanedRepo.AddAsync(orphanedResource);
            
            _logger.LogWarning(
                "Tracked orphaned Stripe resource: {ResourceType} {ResourceId} from {Operation}",
                resourceType, stripeResourceId, operation
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to track orphaned Stripe resource {ResourceId}",
                stripeResourceId
            );
        }
    }
    
    private DateTime CalculateNextRetryTime(int retryCount)
    {
        // Exponential backoff: 5min, 15min, 1hr, 6hr, 24hr
        var delays = new[] { 5, 15, 60, 360, 1440 };
        var delayMinutes = retryCount < delays.Length ? delays[retryCount] : 1440;
        return DateTime.UtcNow.AddMinutes(delayMinutes);
    }
    
    public async Task RunBackgroundCleanupAsync()
    {
        try
        {
            var resourcesDue = await _orphanedRepo.GetResourcesDueForRetryAsync();
            
            _logger.LogInformation(
                "Starting background Stripe cleanup. {Count} resources due for retry.",
                resourcesDue.Count
            );
            
            foreach (var resource in resourcesDue)
            {
                try
                {
                    await RetryResourceCleanupAsync(resource);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error during background cleanup of resource {ResourceId}",
                        resource.StripeResourceId
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background Stripe cleanup job");
        }
    }
    
    private async Task RetryResourceCleanupAsync(StripeOrphanedResource resource)
    {
        try
        {
            bool cleanupSuccess = false;
            
            // Attempt cleanup based on resource type
            switch (resource.ResourceType.ToLower())
            {
                case "product":
                    cleanupSuccess = await _stripeService.DeleteProductAsync(
                        resource.StripeResourceId, 
                        null
                    );
                    break;
                    
                case "price":
                    cleanupSuccess = await _stripeService.DeactivatePriceAsync(
                        resource.StripeResourceId, 
                        null
                    );
                    break;
                    
                case "subscription":
                    cleanupSuccess = await _stripeService.CancelSubscriptionAsync(
                        resource.StripeResourceId, 
                        null
                    );
                    break;
                    
                default:
                    _logger.LogWarning(
                        "Unknown resource type for cleanup: {Type}",
                        resource.ResourceType
                    );
                    break;
            }
            
            if (cleanupSuccess)
            {
                // Mark as resolved
                resource.IsResolved = true;
                resource.ResolvedAt = DateTime.UtcNow;
                resource.UpdatedDate = DateTime.UtcNow;
                
                await _orphanedRepo.UpdateAsync(resource);
                
                _logger.LogInformation(
                    "Successfully cleaned up orphaned resource: {ResourceType} {ResourceId}",
                    resource.ResourceType, resource.StripeResourceId
                );
            }
            else
            {
                // Increment retry count
                resource.RetryCount++;
                resource.LastRetryAt = DateTime.UtcNow;
                resource.NextRetryAt = CalculateNextRetryTime(resource.RetryCount);
                resource.UpdatedDate = DateTime.UtcNow;
                
                await _orphanedRepo.UpdateAsync(resource);
                
                // Notify admin if max retries reached
                if (resource.RetryCount >= resource.MaxRetries && !resource.AdminNotified)
                {
                    await _notificationService.SendAdminAlertAsync(
                        "Stripe Cleanup Failed",
                        $"Failed to cleanup {resource.ResourceType} {resource.StripeResourceId} " +
                        $"after {resource.MaxRetries} attempts. Manual intervention required."
                    );
                    
                    resource.AdminNotified = true;
                    await _orphanedRepo.UpdateAsync(resource);
                }
                
                _logger.LogWarning(
                    "Cleanup retry {Count}/{Max} failed for resource {ResourceId}",
                    resource.RetryCount, resource.MaxRetries, resource.StripeResourceId
                );
            }
        }
        catch (Exception ex)
        {
            resource.RetryCount++;
            resource.LastRetryAt = DateTime.UtcNow;
            resource.NextRetryAt = CalculateNextRetryTime(resource.RetryCount);
            resource.FailureReason = ex.Message;
            resource.UpdatedDate = DateTime.UtcNow;
            
            await _orphanedRepo.UpdateAsync(resource);
            
            _logger.LogError(ex,
                "Error retrying cleanup for resource {ResourceId}",
                resource.StripeResourceId
            );
        }
    }
}
```

#### **Step 4: Update Plan Creation/Deletion to Use Tracking**

```csharp
// Modify SubscriptionPlanService.cs CreatePlanAsync

catch (Exception ex)
{
    await _unitOfWork.RollbackTransactionAsync();
    
    // ✅ IMPROVED: Track orphaned resources instead of just logging
    if (!string.IsNullOrEmpty(stripeProductId))
    {
        try
        {
            _logger.LogWarning("Attempting cleanup of Stripe resources for failed plan");
            
            // Try immediate cleanup
            if (!string.IsNullOrEmpty(monthlyPriceId))
                await _stripeService.DeactivatePriceAsync(monthlyPriceId, tokenModel);
            if (!string.IsNullOrEmpty(quarterlyPriceId))
                await _stripeService.DeactivatePriceAsync(quarterlyPriceId, tokenModel);
            if (!string.IsNullOrEmpty(annualPriceId))
                await _stripeService.DeactivatePriceAsync(annualPriceId, tokenModel);
            
            await _stripeService.DeleteProductAsync(stripeProductId, tokenModel);
            
            _logger.LogInformation("Successfully cleaned up Stripe resources");
        }
        catch (Exception cleanupEx)
        {
            _logger.LogError(cleanupEx, "Failed to cleanup Stripe resources");
            
            // ✅ NEW: Track orphaned resources for background cleanup
            await _stripeCleanupService.TrackOrphanedResourceAsync(
                stripeProductId,
                "Product",
                "Delete",
                null,  // No local entity ID since creation failed
                "SubscriptionPlan",
                cleanupEx.Message
            );
            
            // Track orphaned prices too
            if (!string.IsNullOrEmpty(monthlyPriceId))
            {
                await _stripeCleanupService.TrackOrphanedResourceAsync(
                    monthlyPriceId, "Price", "Delete", null, "SubscriptionPlan", "Orphaned during plan creation failure"
                );
            }
            // ... same for quarterly and annual
        }
    }
    
    _logger.LogError(ex, "Failed to create subscription plan");
    return new JsonModel { Message = "Failed to create plan", StatusCode = 500 };
}
```

#### **Step 5: Add Admin Controller Endpoints**

```csharp
// Add to AdminController.cs or create StripeManagementController.cs

/// <summary>
/// Gets all orphaned Stripe resources that need cleanup
/// </summary>
[HttpGet("admin/stripe/orphaned-resources")]
public async Task<JsonModel> GetOrphanedStripeResources()
{
    return await _stripeCleanupService.GetOrphanedResourcesAsync(GetToken(HttpContext));
}

/// <summary>
/// Manually retries cleanup for a specific orphaned resource
/// </summary>
[HttpPost("admin/stripe/orphaned-resources/{id}/retry")]
public async Task<JsonModel> RetryOrphanedResourceCleanup(Guid id)
{
    return await _stripeCleanupService.RetryOrphanedResourceCleanupAsync(id, GetToken(HttpContext));
}

/// <summary>
/// Marks an orphaned resource to skip cleanup (manual resolution)
/// </summary>
[HttpPost("admin/stripe/orphaned-resources/{id}/skip")]
public async Task<JsonModel> SkipOrphanedResourceCleanup(Guid id)
{
    var resource = await _orphanedRepo.GetByIdAsync(id);
    if (resource == null)
    {
        return new JsonModel { Message = "Resource not found", StatusCode = 404 };
    }
    
    resource.SkipCleanup = true;
    resource.UpdatedDate = DateTime.UtcNow;
    await _orphanedRepo.UpdateAsync(resource);
    
    return new JsonModel 
    { 
        data = resource,
        Message = "Resource marked for manual cleanup",
        StatusCode = 200
    };
}
```

#### **Step 6: Add Background Job**

```csharp
// Add to your background job scheduler (Hangfire, Quartz, etc.)

// Run every hour
[RecurringJob(Cron = "0 * * * *")]
public class StripeOrphanedResourceCleanupJob
{
    private readonly IStripeCleanupService _cleanupService;
    private readonly ILogger<StripeOrphanedResourceCleanupJob> _logger;
    
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting scheduled Stripe orphaned resource cleanup");
        
        await _cleanupService.RunBackgroundCleanupAsync();
        
        _logger.LogInformation("Completed scheduled Stripe orphaned resource cleanup");
    }
}
```

---

## 🎯 TESTING CHECKLIST

### **Test #1: Plan Update with Active Subscriptions**
```
1. Create a plan with Price = $10
2. Create 5 active subscriptions to that plan
3. Attempt to update plan price to $20 without confirmation
   → Expected: 409 Conflict with warning about 5 affected subscriptions
4. Update plan with ConfirmImpact = true
   → Expected: 200 OK, new version created
5. Verify old subscriptions still reference old plan version
6. Verify new subscriptions use new plan version
```

### **Test #2: Privilege Validation**
```
1. Create plan with DailyLimit=10, MonthlyLimit=5
   → Expected: 400 Bad Request - "Daily limit × 30 exceeds monthly limit"
2. Create plan with duplicate privilege
   → Expected: 400 Bad Request - "Privilege assigned multiple times"
3. Create plan with all privileges disabled (Value=0)
   → Expected: 400 Bad Request - "Plan must have at least one enabled privilege"
```

### **Test #3: Stripe Cleanup Tracking**
```
1. Create plan, simulate Stripe product creation success but DB failure
2. Check orphaned resources table
   → Expected: Product ID tracked for cleanup
3. Run background cleanup job
   → Expected: Resource cleaned up successfully
4. Simulate cleanup failure 5 times
   → Expected: Admin notification sent after 5th failure
```

---

## 📊 DEPLOYMENT CHECKLIST

- [ ] Run database migrations for new entities
- [ ] Register new services in DI container
- [ ] Configure background job scheduler
- [ ] Update API documentation
- [ ] Create admin UI for orphaned resources
- [ ] Set up monitoring/alerts for orphaned resources
- [ ] Test all endpoints with integration tests
- [ ] Update client applications to handle new response codes (409)

---

## 🎯 SUMMARY

These fixes address the **most critical** issues in subscription plans management:

1. ✅ **Active Subscription Protection** - Prevents unexpected changes
2. ✅ **Plan Versioning** - Maintains pricing integrity
3. ✅ **Privilege Validation** - Ensures logical consistency
4. ✅ **Stripe Cleanup Tracking** - Prevents orphaned resources

**Implementation Time Estimate:**
- Fix #1 (Active Subscription Protection): 4 hours
- Fix #2 (Plan Versioning): 8 hours
- Fix #3 (Privilege Validation): 3 hours
- Fix #4 (Stripe Cleanup Tracking): 6 hours
- **Total: ~21 hours (3 work days)**

**Priority:** 🔴 CRITICAL - Implement before production launch


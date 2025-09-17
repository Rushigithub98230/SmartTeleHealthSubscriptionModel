# 🎫 **DISCOUNT & COUPON IMPLEMENTATION PLAN**
## **COMPREHENSIVE DISCOUNT & COUPON SYSTEM FOR SUBSCRIPTION MANAGEMENT**

---

## **📊 EXECUTIVE SUMMARY**

**IMPLEMENTATION PLAN: 4-PHASE APPROACH**

Based on the current state analysis, I recommend implementing a **comprehensive discount and coupon system** in **4 phases** to support modern subscription management requirements. The current system has only **25% of the required functionality** with basic plan-level discounts.

---

## **🎯 IMPLEMENTATION PHASES**

### **PHASE 1: CORE ENTITIES & DATABASE (Week 1-2)**
**Priority: CRITICAL** | **Effort: 40 hours**

#### **A. Database Schema Design**
```sql
-- 1. Discounts Table
CREATE TABLE [Discounts] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Code] nvarchar(50) NOT NULL UNIQUE,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Type] nvarchar(20) NOT NULL, -- Percentage, FixedAmount, FreeTrial
    [Value] decimal(18,2) NOT NULL,
    [MinOrderAmount] decimal(18,2) NULL,
    [MaxDiscountAmount] decimal(18,2) NULL,
    [UsageLimit] int NULL,
    [UsedCount] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [ValidFrom] datetime2 NOT NULL,
    [ValidUntil] datetime2 NULL,
    [ApplicablePlans] nvarchar(max) NULL, -- JSON array of plan IDs
    [ApplicableUsers] nvarchar(max) NULL, -- JSON array of user IDs
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] int NULL,
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Discounts] PRIMARY KEY ([Id])
);

-- 2. Coupons Table
CREATE TABLE [Coupons] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Code] nvarchar(50) NOT NULL UNIQUE,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Type] nvarchar(20) NOT NULL, -- Percentage, FixedAmount, FreeTrial
    [Value] decimal(18,2) NOT NULL,
    [MinOrderAmount] decimal(18,2) NULL,
    [MaxDiscountAmount] decimal(18,2) NULL,
    [UsageLimit] int NULL,
    [UsedCount] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [ValidFrom] datetime2 NOT NULL,
    [ValidUntil] datetime2 NULL,
    [ApplicablePlans] nvarchar(max) NULL, -- JSON array of plan IDs
    [ApplicableUsers] nvarchar(max) NULL, -- JSON array of user IDs
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] int NOT NULL,
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Coupons] PRIMARY KEY ([Id])
);

-- 3. Discount Usages Table
CREATE TABLE [DiscountUsages] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [DiscountId] uniqueidentifier NOT NULL,
    [UserId] int NOT NULL,
    [SubscriptionId] uniqueidentifier NULL,
    [BillingRecordId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NOT NULL,
    [UsedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_DiscountUsages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DiscountUsages_Discounts_DiscountId] FOREIGN KEY ([DiscountId]) REFERENCES [Discounts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DiscountUsages_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DiscountUsages_BillingRecords_BillingRecordId] FOREIGN KEY ([BillingRecordId]) REFERENCES [BillingRecords] ([Id]) ON DELETE NO ACTION
);

-- 4. Coupon Usages Table
CREATE TABLE [CouponUsages] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [CouponId] uniqueidentifier NOT NULL,
    [UserId] int NOT NULL,
    [SubscriptionId] uniqueidentifier NULL,
    [BillingRecordId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NOT NULL,
    [UsedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_CouponUsages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CouponUsages_Coupons_CouponId] FOREIGN KEY ([CouponId]) REFERENCES [Coupons] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CouponUsages_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CouponUsages_BillingRecords_BillingRecordId] FOREIGN KEY ([BillingRecordId]) REFERENCES [BillingRecords] ([Id]) ON DELETE NO ACTION
);

-- 5. Discount Rules Table
CREATE TABLE [DiscountRules] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [RuleType] nvarchar(20) NOT NULL, -- EarlyBird, Volume, Loyalty, Referral, Seasonal
    [Conditions] nvarchar(max) NULL, -- JSON conditions
    [DiscountType] nvarchar(20) NOT NULL, -- Percentage, FixedAmount
    [DiscountValue] decimal(18,2) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [Priority] int NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] int NULL,
    [UpdatedDate] datetime2 NULL,
    CONSTRAINT [PK_DiscountRules] PRIMARY KEY ([Id])
);
```

#### **B. Entity Classes**
```csharp
// 1. Discount Entity
public class Discount : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DiscountType Type { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinOrderAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }
    
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicablePlans { get; set; } // JSON array
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicableUsers { get; set; } // JSON array
    
    // Navigation properties
    public virtual ICollection<DiscountUsage> Usages { get; set; } = new List<DiscountUsage>();
    
    // Computed properties
    [NotMapped]
    public bool IsValid => IsActive && 
        DateTime.UtcNow >= ValidFrom && 
        (!ValidUntil.HasValue || DateTime.UtcNow <= ValidUntil.Value) &&
        (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);
    
    [NotMapped]
    public bool HasUsageLimit => UsageLimit.HasValue;
    
    [NotMapped]
    public int RemainingUsage => UsageLimit.HasValue ? UsageLimit.Value - UsedCount : int.MaxValue;
}

// 2. Coupon Entity
public class Coupon : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public CouponType Type { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinOrderAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }
    
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicablePlans { get; set; } // JSON array
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicableUsers { get; set; } // JSON array
    
    // Navigation properties
    public virtual ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
    
    // Computed properties
    [NotMapped]
    public bool IsValid => IsActive && 
        DateTime.UtcNow >= ValidFrom && 
        (!ValidUntil.HasValue || DateTime.UtcNow <= ValidUntil.Value) &&
        (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);
    
    [NotMapped]
    public bool HasUsageLimit => UsageLimit.HasValue;
    
    [NotMapped]
    public int RemainingUsage => UsageLimit.HasValue ? UsageLimit.Value - UsedCount : int.MaxValue;
}

// 3. Discount Usage Entity
public class DiscountUsage : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid DiscountId { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public Guid? SubscriptionId { get; set; }
    public Guid? BillingRecordId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    public DateTime UsedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Discount Discount { get; set; } = null!;
    public virtual Subscription? Subscription { get; set; }
    public virtual BillingRecord? BillingRecord { get; set; }
}

// 4. Coupon Usage Entity
public class CouponUsage : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid CouponId { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public Guid? SubscriptionId { get; set; }
    public Guid? BillingRecordId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    public DateTime UsedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Coupon Coupon { get; set; } = null!;
    public virtual Subscription? Subscription { get; set; }
    public virtual BillingRecord? BillingRecord { get; set; }
}

// 5. Discount Rule Entity
public class DiscountRule : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DiscountRuleType RuleType { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Conditions { get; set; } // JSON conditions
    
    [Required]
    public DiscountType DiscountType { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }
    
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;
    
    // Computed properties
    [NotMapped]
    public bool IsValid => IsActive;
}

// 6. Enums
public enum DiscountType
{
    Percentage,
    FixedAmount,
    FreeTrial
}

public enum CouponType
{
    Percentage,
    FixedAmount,
    FreeTrial
}

public enum DiscountRuleType
{
    EarlyBird,
    Volume,
    Loyalty,
    Referral,
    Seasonal,
    Custom
}
```

### **PHASE 2: SERVICES & REPOSITORIES (Week 3-4)**
**Priority: CRITICAL** | **Effort: 60 hours**

#### **A. Repository Interfaces**
```csharp
// 1. IDiscountRepository
public interface IDiscountRepository : IRepositoryBase<Discount>
{
    Task<Discount?> GetByCodeAsync(string code);
    Task<IEnumerable<Discount>> GetActiveDiscountsAsync();
    Task<IEnumerable<Discount>> GetApplicableDiscountsAsync(Guid planId, int userId);
    Task<IEnumerable<Discount>> GetDiscountsByUserAsync(int userId);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<int> GetUsageCountAsync(Guid discountId);
    Task<IEnumerable<Discount>> GetExpiredDiscountsAsync();
    Task<IEnumerable<Discount>> GetDiscountsByDateRangeAsync(DateTime startDate, DateTime endDate);
}

// 2. ICouponRepository
public interface ICouponRepository : IRepositoryBase<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code);
    Task<IEnumerable<Coupon>> GetActiveCouponsAsync();
    Task<IEnumerable<Coupon>> GetApplicableCouponsAsync(Guid planId, int userId);
    Task<IEnumerable<Coupon>> GetCouponsByUserAsync(int userId);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<int> GetUsageCountAsync(Guid couponId);
    Task<IEnumerable<Coupon>> GetExpiredCouponsAsync();
    Task<IEnumerable<Coupon>> GetCouponsByDateRangeAsync(DateTime startDate, DateTime endDate);
}

// 3. IDiscountUsageRepository
public interface IDiscountUsageRepository : IRepositoryBase<DiscountUsage>
{
    Task<IEnumerable<DiscountUsage>> GetByDiscountIdAsync(Guid discountId);
    Task<IEnumerable<DiscountUsage>> GetByUserIdAsync(int userId);
    Task<IEnumerable<DiscountUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<bool> HasUserUsedDiscountAsync(Guid discountId, int userId);
    Task<int> GetUsageCountByUserAsync(Guid discountId, int userId);
    Task<decimal> GetTotalDiscountAmountAsync(Guid discountId);
    Task<decimal> GetTotalDiscountAmountByUserAsync(int userId);
}

// 4. ICouponUsageRepository
public interface ICouponUsageRepository : IRepositoryBase<CouponUsage>
{
    Task<IEnumerable<CouponUsage>> GetByCouponIdAsync(Guid couponId);
    Task<IEnumerable<CouponUsage>> GetByUserIdAsync(int userId);
    Task<IEnumerable<CouponUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<bool> HasUserUsedCouponAsync(Guid couponId, int userId);
    Task<int> GetUsageCountByUserAsync(Guid couponId, int userId);
    Task<decimal> GetTotalCouponAmountAsync(Guid couponId);
    Task<decimal> GetTotalCouponAmountByUserAsync(int userId);
}

// 5. IDiscountRuleRepository
public interface IDiscountRuleRepository : IRepositoryBase<DiscountRule>
{
    Task<IEnumerable<DiscountRule>> GetActiveRulesAsync();
    Task<IEnumerable<DiscountRule>> GetRulesByTypeAsync(DiscountRuleType ruleType);
    Task<IEnumerable<DiscountRule>> GetApplicableRulesAsync(Guid planId, int userId);
    Task<DiscountRule?> GetByPriorityAsync(int priority);
}
```

#### **B. Service Interfaces**
```csharp
// 1. IDiscountService
public interface IDiscountService
{
    Task<JsonModel> CreateDiscountAsync(CreateDiscountDto createDto, TokenModel tokenModel);
    Task<JsonModel> UpdateDiscountAsync(Guid id, UpdateDiscountDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteDiscountAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetDiscountAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetDiscountsAsync(DiscountFilterDto filter, TokenModel tokenModel);
    Task<JsonModel> GetDiscountByCodeAsync(string code, TokenModel tokenModel);
    Task<JsonModel> ValidateDiscountAsync(string code, Guid planId, int userId, decimal orderAmount, TokenModel tokenModel);
    Task<JsonModel> ApplyDiscountAsync(string code, Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> GetDiscountUsageAsync(Guid discountId, TokenModel tokenModel);
    Task<JsonModel> GetDiscountAnalyticsAsync(DiscountAnalyticsFilterDto filter, TokenModel tokenModel);
}

// 2. ICouponService
public interface ICouponService
{
    Task<JsonModel> CreateCouponAsync(CreateCouponDto createDto, TokenModel tokenModel);
    Task<JsonModel> UpdateCouponAsync(Guid id, UpdateCouponDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteCouponAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetCouponAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetCouponsAsync(CouponFilterDto filter, TokenModel tokenModel);
    Task<JsonModel> GetCouponByCodeAsync(string code, TokenModel tokenModel);
    Task<JsonModel> ValidateCouponAsync(string code, Guid planId, int userId, decimal orderAmount, TokenModel tokenModel);
    Task<JsonModel> ApplyCouponAsync(string code, Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> GetCouponUsageAsync(Guid couponId, TokenModel tokenModel);
    Task<JsonModel> GetCouponAnalyticsAsync(CouponAnalyticsFilterDto filter, TokenModel tokenModel);
}

// 3. IDiscountValidationService
public interface IDiscountValidationService
{
    Task<DiscountValidationResult> ValidateDiscountAsync(string code, Guid planId, int userId, decimal orderAmount);
    Task<CouponValidationResult> ValidateCouponAsync(string code, Guid planId, int userId, decimal orderAmount);
    Task<decimal> CalculateDiscountAmountAsync(Discount discount, decimal orderAmount);
    Task<decimal> CalculateCouponAmountAsync(Coupon coupon, decimal orderAmount);
    Task<bool> IsEligibleForDiscountAsync(Discount discount, Guid planId, int userId);
    Task<bool> IsEligibleForCouponAsync(Coupon coupon, Guid planId, int userId);
}

// 4. IDiscountRuleService
public interface IDiscountRuleService
{
    Task<JsonModel> CreateRuleAsync(CreateDiscountRuleDto createDto, TokenModel tokenModel);
    Task<JsonModel> UpdateRuleAsync(Guid id, UpdateDiscountRuleDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteRuleAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetRuleAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetRulesAsync(DiscountRuleFilterDto filter, TokenModel tokenModel);
    Task<JsonModel> ApplyRulesAsync(Guid planId, int userId, decimal orderAmount, TokenModel tokenModel);
    Task<JsonModel> GetRuleAnalyticsAsync(DiscountRuleAnalyticsFilterDto filter, TokenModel tokenModel);
}
```

### **PHASE 3: DTOs & CONTROLLERS (Week 5-6)**
**Priority: HIGH** | **Effort: 40 hours**

#### **A. DTOs**
```csharp
// 1. Discount DTOs
public class DiscountDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public List<Guid> ApplicablePlans { get; set; } = new();
    public List<int> ApplicableUsers { get; set; } = new();
    public bool IsValid { get; set; }
    public bool HasUsageLimit { get; set; }
    public int RemainingUsage { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class CreateDiscountDto
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DiscountType Type { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Value { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MinOrderAmount { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [Required]
    public DateTime ValidFrom { get; set; }
    
    public DateTime? ValidUntil { get; set; }
    
    public List<Guid> ApplicablePlans { get; set; } = new();
    public List<int> ApplicableUsers { get; set; } = new();
}

public class UpdateDiscountDto
{
    [Required]
    public Guid Id { get; set; }
    
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? Value { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MinOrderAmount { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }
    
    public bool? IsActive { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    
    public List<Guid>? ApplicablePlans { get; set; }
    public List<int>? ApplicableUsers { get; set; }
}

public class DiscountFilterDto
{
    public string? SearchTerm { get; set; }
    public DiscountType? Type { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public Guid? PlanId { get; set; }
    public int? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedDate";
    public string SortDirection { get; set; } = "desc";
}

// 2. Coupon DTOs (Similar structure to Discount DTOs)
public class CouponDto { /* Similar to DiscountDto */ }
public class CreateCouponDto { /* Similar to CreateDiscountDto */ }
public class UpdateCouponDto { /* Similar to UpdateDiscountDto */ }
public class CouponFilterDto { /* Similar to DiscountFilterDto */ }

// 3. Validation DTOs
public class DiscountValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; }
    public Discount? Discount { get; set; }
}

public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal CouponAmount { get; set; }
    public Coupon? Coupon { get; set; }
}

// 4. Analytics DTOs
public class DiscountAnalyticsDto
{
    public int TotalDiscounts { get; set; }
    public int ActiveDiscounts { get; set; }
    public int ExpiredDiscounts { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal AverageDiscountAmount { get; set; }
    public int TotalUsages { get; set; }
    public decimal UsageRate { get; set; }
    public List<DiscountUsageDto> TopDiscounts { get; set; } = new();
    public List<MonthlyDiscountData> MonthlyData { get; set; } = new();
}

public class CouponAnalyticsDto
{
    public int TotalCoupons { get; set; }
    public int ActiveCoupons { get; set; }
    public int ExpiredCoupons { get; set; }
    public decimal TotalCouponAmount { get; set; }
    public decimal AverageCouponAmount { get; set; }
    public int TotalUsages { get; set; }
    public decimal UsageRate { get; set; }
    public List<CouponUsageDto> TopCoupons { get; set; } = new();
    public List<MonthlyCouponData> MonthlyData { get; set; } = new();
}
```

#### **B. Controllers**
```csharp
// 1. DiscountController
[ApiController]
[Route("api/discounts")]
[Authorize]
public class DiscountController : BaseController
{
    private readonly IDiscountService _discountService;
    private readonly IDiscountValidationService _validationService;

    [HttpPost]
    public async Task<JsonModel> CreateDiscount([FromBody] CreateDiscountDto createDto)
    {
        return await _discountService.CreateDiscountAsync(createDto, GetToken(HttpContext));
    }

    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateDiscount(Guid id, [FromBody] UpdateDiscountDto updateDto)
    {
        return await _discountService.UpdateDiscountAsync(id, updateDto, GetToken(HttpContext));
    }

    [HttpDelete("{id}")]
    public async Task<JsonModel> DeleteDiscount(Guid id)
    {
        return await _discountService.DeleteDiscountAsync(id, GetToken(HttpContext));
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetDiscount(Guid id)
    {
        return await _discountService.GetDiscountAsync(id, GetToken(HttpContext));
    }

    [HttpGet]
    public async Task<JsonModel> GetDiscounts([FromQuery] DiscountFilterDto filter)
    {
        return await _discountService.GetDiscountsAsync(filter, GetToken(HttpContext));
    }

    [HttpGet("code/{code}")]
    public async Task<JsonModel> GetDiscountByCode(string code)
    {
        return await _discountService.GetDiscountByCodeAsync(code, GetToken(HttpContext));
    }

    [HttpPost("validate")]
    public async Task<JsonModel> ValidateDiscount([FromBody] ValidateDiscountRequestDto request)
    {
        return await _discountService.ValidateDiscountAsync(
            request.Code, request.PlanId, request.UserId, request.OrderAmount, GetToken(HttpContext));
    }

    [HttpPost("apply")]
    public async Task<JsonModel> ApplyDiscount([FromBody] ApplyDiscountRequestDto request)
    {
        return await _discountService.ApplyDiscountAsync(
            request.Code, request.SubscriptionId, GetToken(HttpContext));
    }

    [HttpGet("{id}/usage")]
    public async Task<JsonModel> GetDiscountUsage(Guid id)
    {
        return await _discountService.GetDiscountUsageAsync(id, GetToken(HttpContext));
    }

    [HttpGet("analytics")]
    public async Task<JsonModel> GetDiscountAnalytics([FromQuery] DiscountAnalyticsFilterDto filter)
    {
        return await _discountService.GetDiscountAnalyticsAsync(filter, GetToken(HttpContext));
    }
}

// 2. CouponController (Similar structure to DiscountController)
[ApiController]
[Route("api/coupons")]
[Authorize]
public class CouponController : BaseController
{
    // Similar endpoints to DiscountController
}
```

### **PHASE 4: INTEGRATION & TESTING (Week 7-8)**
**Priority: HIGH** | **Effort: 50 hours**

#### **A. Integration with Existing Services**
```csharp
// 1. Update AutomatedBillingService
public class AutomatedBillingService : IAutomatedBillingService
{
    private readonly IDiscountService _discountService;
    private readonly ICouponService _couponService;
    private readonly IDiscountValidationService _validationService;

    // Update CalculateDiscountAmountAsync to use new discount system
    private async Task<decimal> CalculateDiscountAmountAsync(Subscription subscription, TokenModel tokenModel)
    {
        decimal totalDiscount = 0;
        
        // Get applicable discounts for the subscription
        var applicableDiscounts = await _discountService.GetApplicableDiscountsAsync(
            subscription.SubscriptionPlanId, subscription.UserId, tokenModel);
        
        // Apply each applicable discount
        foreach (var discount in applicableDiscounts)
        {
            var discountAmount = await _validationService.CalculateDiscountAmountAsync(
                discount, subscription.CurrentPrice);
            totalDiscount += discountAmount;
        }
        
        // Apply discount rules
        var rules = await _discountRuleService.GetApplicableRulesAsync(
            subscription.SubscriptionPlanId, subscription.UserId, tokenModel);
        
        foreach (var rule in rules)
        {
            var ruleDiscount = await CalculateRuleDiscountAsync(rule, subscription);
            totalDiscount += ruleDiscount;
        }
        
        return Math.Min(totalDiscount, subscription.CurrentPrice);
    }
}

// 2. Update SubscriptionService
public class SubscriptionService : ISubscriptionService
{
    private readonly IDiscountService _discountService;
    private readonly ICouponService _couponService;

    // Add discount/coupon application methods
    public async Task<JsonModel> ApplyDiscountToSubscriptionAsync(
        Guid subscriptionId, string discountCode, TokenModel tokenModel)
    {
        // Implementation for applying discount to subscription
    }

    public async Task<JsonModel> ApplyCouponToSubscriptionAsync(
        Guid subscriptionId, string couponCode, TokenModel tokenModel)
    {
        // Implementation for applying coupon to subscription
    }
}

// 3. Update BillingService
public class BillingService : IBillingService
{
    private readonly IDiscountService _discountService;
    private readonly ICouponService _couponService;

    // Update billing record creation to include discounts/coupons
    public async Task<JsonModel> CreateBillingRecordAsync(
        CreateBillingRecordDto createDto, TokenModel tokenModel)
    {
        // Apply discounts and coupons to billing record
        var discountAmount = await CalculateTotalDiscountAsync(createDto, tokenModel);
        var couponAmount = await CalculateTotalCouponAsync(createDto, tokenModel);
        
        // Update billing record with discount/coupon amounts
        createDto.Amount -= discountAmount + couponAmount;
        
        // Continue with existing billing record creation
    }
}
```

#### **B. Frontend Integration**
```typescript
// 1. Discount Models
export interface Discount {
  id: string;
  code: string;
  name: string;
  description?: string;
  type: 'Percentage' | 'FixedAmount' | 'FreeTrial';
  value: number;
  minOrderAmount?: number;
  maxDiscountAmount?: number;
  usageLimit?: number;
  usedCount: number;
  isActive: boolean;
  validFrom: Date;
  validUntil?: Date;
  applicablePlans: string[];
  applicableUsers: number[];
  isValid: boolean;
  hasUsageLimit: boolean;
  remainingUsage: number;
  createdDate: Date;
  updatedDate?: Date;
}

export interface CreateDiscountDto {
  code: string;
  name: string;
  description?: string;
  type: 'Percentage' | 'FixedAmount' | 'FreeTrial';
  value: number;
  minOrderAmount?: number;
  maxDiscountAmount?: number;
  usageLimit?: number;
  isActive: boolean;
  validFrom: Date;
  validUntil?: Date;
  applicablePlans: string[];
  applicableUsers: number[];
}

// 2. Discount Service
@Injectable({
  providedIn: 'root'
})
export class DiscountService {
  private apiUrl = environment.apiUrl + '/discounts';

  constructor(private http: HttpClient) {}

  createDiscount(discount: CreateDiscountDto): Observable<ApiResponse<Discount>> {
    return this.http.post<ApiResponse<Discount>>(this.apiUrl, discount);
  }

  getDiscounts(filter: DiscountFilter): Observable<ApiResponse<Discount[]>> {
    return this.http.get<ApiResponse<Discount[]>>(this.apiUrl, { params: filter });
  }

  validateDiscount(code: string, planId: string, userId: number, orderAmount: number): Observable<ApiResponse<DiscountValidationResult>> {
    return this.http.post<ApiResponse<DiscountValidationResult>>(`${this.apiUrl}/validate`, {
      code, planId, userId, orderAmount
    });
  }

  applyDiscount(code: string, subscriptionId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/apply`, {
      code, subscriptionId
    });
  }
}

// 3. Coupon Service (Similar structure)
@Injectable({
  providedIn: 'root'
})
export class CouponService {
  // Similar methods to DiscountService
}
```

---

## **🎯 IMPLEMENTATION TIMELINE**

### **Week 1-2: Core Entities & Database**
- ✅ Create database tables
- ✅ Create entity classes
- ✅ Create enums
- ✅ Update DbContext
- ✅ Create migrations

### **Week 3-4: Services & Repositories**
- ✅ Create repository interfaces
- ✅ Implement repositories
- ✅ Create service interfaces
- ✅ Implement services
- ✅ Create validation services

### **Week 5-6: DTOs & Controllers**
- ✅ Create DTOs
- ✅ Create controllers
- ✅ Create validation DTOs
- ✅ Create analytics DTOs
- ✅ Update mapping profiles

### **Week 7-8: Integration & Testing**
- ✅ Integrate with existing services
- ✅ Update billing logic
- ✅ Create frontend services
- ✅ Create frontend components
- ✅ Integration testing
- ✅ End-to-end testing

---

## **📊 EFFORT ESTIMATION**

### **Total Effort: 190 hours (5 weeks)**

#### **Backend Development: 140 hours**
- Database & Entities: 40 hours
- Services & Repositories: 60 hours
- DTOs & Controllers: 40 hours

#### **Frontend Development: 30 hours**
- Models & Services: 15 hours
- Components & UI: 15 hours

#### **Integration & Testing: 20 hours**
- Backend Integration: 10 hours
- Frontend Integration: 5 hours
- Testing: 5 hours

---

## **🚀 EXPECTED OUTCOMES**

### **After Implementation:**
- ✅ **Complete discount management** - Create, update, delete discounts
- ✅ **Complete coupon management** - Create, update, delete coupons
- ✅ **Advanced discount types** - Percentage, fixed amount, free trial
- ✅ **Usage tracking** - Track discount and coupon usage
- ✅ **Validation system** - Comprehensive validation logic
- ✅ **Analytics & reporting** - Discount and coupon analytics
- ✅ **API endpoints** - Complete REST API
- ✅ **Frontend integration** - Complete UI integration
- ✅ **Billing integration** - Seamless billing integration
- ✅ **Stripe integration** - Stripe discount/coupon support

### **Business Benefits:**
- 🎯 **Increased conversions** - Attractive discount offers
- 🎯 **Customer retention** - Loyalty discounts and coupons
- 🎯 **Revenue growth** - Strategic discount campaigns
- 🎯 **Marketing flexibility** - Multiple discount types
- 🎯 **Analytics insights** - Discount performance tracking
- 🎯 **Operational efficiency** - Automated discount management

---

## **🎯 RECOMMENDATION**

**Implement this comprehensive discount and coupon system in 4 phases over 8 weeks to transform your subscription management application into a modern, feature-rich platform with advanced discount and coupon capabilities.**
## **COMPREHENSIVE DISCOUNT & COUPON SYSTEM FOR SUBSCRIPTION MANAGEMENT**

---

## **📊 EXECUTIVE SUMMARY**

**IMPLEMENTATION PLAN: 4-PHASE APPROACH**

Based on the current state analysis, I recommend implementing a **comprehensive discount and coupon system** in **4 phases** to support modern subscription management requirements. The current system has only **25% of the required functionality** with basic plan-level discounts.

---

## **🎯 IMPLEMENTATION PHASES**

### **PHASE 1: CORE ENTITIES & DATABASE (Week 1-2)**
**Priority: CRITICAL** | **Effort: 40 hours**

#### **A. Database Schema Design**
```sql
-- 1. Discounts Table
CREATE TABLE [Discounts] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Code] nvarchar(50) NOT NULL UNIQUE,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Type] nvarchar(20) NOT NULL, -- Percentage, FixedAmount, FreeTrial
    [Value] decimal(18,2) NOT NULL,
    [MinOrderAmount] decimal(18,2) NULL,
    [MaxDiscountAmount] decimal(18,2) NULL,
    [UsageLimit] int NULL,
    [UsedCount] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [ValidFrom] datetime2 NOT NULL,
    [ValidUntil] datetime2 NULL,
    [ApplicablePlans] nvarchar(max) NULL, -- JSON array of plan IDs
    [ApplicableUsers] nvarchar(max) NULL, -- JSON array of user IDs
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] int NULL,
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Discounts] PRIMARY KEY ([Id])
);

-- 2. Coupons Table
CREATE TABLE [Coupons] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Code] nvarchar(50) NOT NULL UNIQUE,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Type] nvarchar(20) NOT NULL, -- Percentage, FixedAmount, FreeTrial
    [Value] decimal(18,2) NOT NULL,
    [MinOrderAmount] decimal(18,2) NULL,
    [MaxDiscountAmount] decimal(18,2) NULL,
    [UsageLimit] int NULL,
    [UsedCount] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [ValidFrom] datetime2 NOT NULL,
    [ValidUntil] datetime2 NULL,
    [ApplicablePlans] nvarchar(max) NULL, -- JSON array of plan IDs
    [ApplicableUsers] nvarchar(max) NULL, -- JSON array of user IDs
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] int NOT NULL,
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Coupons] PRIMARY KEY ([Id])
);

-- 3. Discount Usages Table
CREATE TABLE [DiscountUsages] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [DiscountId] uniqueidentifier NOT NULL,
    [UserId] int NOT NULL,
    [SubscriptionId] uniqueidentifier NULL,
    [BillingRecordId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NOT NULL,
    [UsedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_DiscountUsages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DiscountUsages_Discounts_DiscountId] FOREIGN KEY ([DiscountId]) REFERENCES [Discounts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DiscountUsages_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DiscountUsages_BillingRecords_BillingRecordId] FOREIGN KEY ([BillingRecordId]) REFERENCES [BillingRecords] ([Id]) ON DELETE NO ACTION
);

-- 4. Coupon Usages Table
CREATE TABLE [CouponUsages] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [CouponId] uniqueidentifier NOT NULL,
    [UserId] int NOT NULL,
    [SubscriptionId] uniqueidentifier NULL,
    [BillingRecordId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NOT NULL,
    [UsedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_CouponUsages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CouponUsages_Coupons_CouponId] FOREIGN KEY ([CouponId]) REFERENCES [Coupons] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CouponUsages_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CouponUsages_BillingRecords_BillingRecordId] FOREIGN KEY ([BillingRecordId]) REFERENCES [BillingRecords] ([Id]) ON DELETE NO ACTION
);

-- 5. Discount Rules Table
CREATE TABLE [DiscountRules] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [RuleType] nvarchar(20) NOT NULL, -- EarlyBird, Volume, Loyalty, Referral, Seasonal
    [Conditions] nvarchar(max) NULL, -- JSON conditions
    [DiscountType] nvarchar(20) NOT NULL, -- Percentage, FixedAmount
    [DiscountValue] decimal(18,2) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [Priority] int NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] int NULL,
    [UpdatedDate] datetime2 NULL,
    CONSTRAINT [PK_DiscountRules] PRIMARY KEY ([Id])
);
```

#### **B. Entity Classes**
```csharp
// 1. Discount Entity
public class Discount : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DiscountType Type { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinOrderAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }
    
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicablePlans { get; set; } // JSON array
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicableUsers { get; set; } // JSON array
    
    // Navigation properties
    public virtual ICollection<DiscountUsage> Usages { get; set; } = new List<DiscountUsage>();
    
    // Computed properties
    [NotMapped]
    public bool IsValid => IsActive && 
        DateTime.UtcNow >= ValidFrom && 
        (!ValidUntil.HasValue || DateTime.UtcNow <= ValidUntil.Value) &&
        (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);
    
    [NotMapped]
    public bool HasUsageLimit => UsageLimit.HasValue;
    
    [NotMapped]
    public int RemainingUsage => UsageLimit.HasValue ? UsageLimit.Value - UsedCount : int.MaxValue;
}

// 2. Coupon Entity
public class Coupon : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public CouponType Type { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinOrderAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }
    
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicablePlans { get; set; } // JSON array
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicableUsers { get; set; } // JSON array
    
    // Navigation properties
    public virtual ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
    
    // Computed properties
    [NotMapped]
    public bool IsValid => IsActive && 
        DateTime.UtcNow >= ValidFrom && 
        (!ValidUntil.HasValue || DateTime.UtcNow <= ValidUntil.Value) &&
        (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);
    
    [NotMapped]
    public bool HasUsageLimit => UsageLimit.HasValue;
    
    [NotMapped]
    public int RemainingUsage => UsageLimit.HasValue ? UsageLimit.Value - UsedCount : int.MaxValue;
}

// 3. Discount Usage Entity
public class DiscountUsage : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid DiscountId { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public Guid? SubscriptionId { get; set; }
    public Guid? BillingRecordId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    public DateTime UsedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Discount Discount { get; set; } = null!;
    public virtual Subscription? Subscription { get; set; }
    public virtual BillingRecord? BillingRecord { get; set; }
}

// 4. Coupon Usage Entity
public class CouponUsage : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid CouponId { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public Guid? SubscriptionId { get; set; }
    public Guid? BillingRecordId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    public DateTime UsedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Coupon Coupon { get; set; } = null!;
    public virtual Subscription? Subscription { get; set; }
    public virtual BillingRecord? BillingRecord { get; set; }
}

// 5. Discount Rule Entity
public class DiscountRule : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DiscountRuleType RuleType { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Conditions { get; set; } // JSON conditions
    
    [Required]
    public DiscountType DiscountType { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }
    
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;
    
    // Computed properties
    [NotMapped]
    public bool IsValid => IsActive;
}

// 6. Enums
public enum DiscountType
{
    Percentage,
    FixedAmount,
    FreeTrial
}

public enum CouponType
{
    Percentage,
    FixedAmount,
    FreeTrial
}

public enum DiscountRuleType
{
    EarlyBird,
    Volume,
    Loyalty,
    Referral,
    Seasonal,
    Custom
}
```

### **PHASE 2: SERVICES & REPOSITORIES (Week 3-4)**
**Priority: CRITICAL** | **Effort: 60 hours**

#### **A. Repository Interfaces**
```csharp
// 1. IDiscountRepository
public interface IDiscountRepository : IRepositoryBase<Discount>
{
    Task<Discount?> GetByCodeAsync(string code);
    Task<IEnumerable<Discount>> GetActiveDiscountsAsync();
    Task<IEnumerable<Discount>> GetApplicableDiscountsAsync(Guid planId, int userId);
    Task<IEnumerable<Discount>> GetDiscountsByUserAsync(int userId);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<int> GetUsageCountAsync(Guid discountId);
    Task<IEnumerable<Discount>> GetExpiredDiscountsAsync();
    Task<IEnumerable<Discount>> GetDiscountsByDateRangeAsync(DateTime startDate, DateTime endDate);
}

// 2. ICouponRepository
public interface ICouponRepository : IRepositoryBase<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code);
    Task<IEnumerable<Coupon>> GetActiveCouponsAsync();
    Task<IEnumerable<Coupon>> GetApplicableCouponsAsync(Guid planId, int userId);
    Task<IEnumerable<Coupon>> GetCouponsByUserAsync(int userId);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<int> GetUsageCountAsync(Guid couponId);
    Task<IEnumerable<Coupon>> GetExpiredCouponsAsync();
    Task<IEnumerable<Coupon>> GetCouponsByDateRangeAsync(DateTime startDate, DateTime endDate);
}

// 3. IDiscountUsageRepository
public interface IDiscountUsageRepository : IRepositoryBase<DiscountUsage>
{
    Task<IEnumerable<DiscountUsage>> GetByDiscountIdAsync(Guid discountId);
    Task<IEnumerable<DiscountUsage>> GetByUserIdAsync(int userId);
    Task<IEnumerable<DiscountUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<bool> HasUserUsedDiscountAsync(Guid discountId, int userId);
    Task<int> GetUsageCountByUserAsync(Guid discountId, int userId);
    Task<decimal> GetTotalDiscountAmountAsync(Guid discountId);
    Task<decimal> GetTotalDiscountAmountByUserAsync(int userId);
}

// 4. ICouponUsageRepository
public interface ICouponUsageRepository : IRepositoryBase<CouponUsage>
{
    Task<IEnumerable<CouponUsage>> GetByCouponIdAsync(Guid couponId);
    Task<IEnumerable<CouponUsage>> GetByUserIdAsync(int userId);
    Task<IEnumerable<CouponUsage>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<bool> HasUserUsedCouponAsync(Guid couponId, int userId);
    Task<int> GetUsageCountByUserAsync(Guid couponId, int userId);
    Task<decimal> GetTotalCouponAmountAsync(Guid couponId);
    Task<decimal> GetTotalCouponAmountByUserAsync(int userId);
}

// 5. IDiscountRuleRepository
public interface IDiscountRuleRepository : IRepositoryBase<DiscountRule>
{
    Task<IEnumerable<DiscountRule>> GetActiveRulesAsync();
    Task<IEnumerable<DiscountRule>> GetRulesByTypeAsync(DiscountRuleType ruleType);
    Task<IEnumerable<DiscountRule>> GetApplicableRulesAsync(Guid planId, int userId);
    Task<DiscountRule?> GetByPriorityAsync(int priority);
}
```

#### **B. Service Interfaces**
```csharp
// 1. IDiscountService
public interface IDiscountService
{
    Task<JsonModel> CreateDiscountAsync(CreateDiscountDto createDto, TokenModel tokenModel);
    Task<JsonModel> UpdateDiscountAsync(Guid id, UpdateDiscountDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteDiscountAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetDiscountAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetDiscountsAsync(DiscountFilterDto filter, TokenModel tokenModel);
    Task<JsonModel> GetDiscountByCodeAsync(string code, TokenModel tokenModel);
    Task<JsonModel> ValidateDiscountAsync(string code, Guid planId, int userId, decimal orderAmount, TokenModel tokenModel);
    Task<JsonModel> ApplyDiscountAsync(string code, Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> GetDiscountUsageAsync(Guid discountId, TokenModel tokenModel);
    Task<JsonModel> GetDiscountAnalyticsAsync(DiscountAnalyticsFilterDto filter, TokenModel tokenModel);
}

// 2. ICouponService
public interface ICouponService
{
    Task<JsonModel> CreateCouponAsync(CreateCouponDto createDto, TokenModel tokenModel);
    Task<JsonModel> UpdateCouponAsync(Guid id, UpdateCouponDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteCouponAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetCouponAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetCouponsAsync(CouponFilterDto filter, TokenModel tokenModel);
    Task<JsonModel> GetCouponByCodeAsync(string code, TokenModel tokenModel);
    Task<JsonModel> ValidateCouponAsync(string code, Guid planId, int userId, decimal orderAmount, TokenModel tokenModel);
    Task<JsonModel> ApplyCouponAsync(string code, Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> GetCouponUsageAsync(Guid couponId, TokenModel tokenModel);
    Task<JsonModel> GetCouponAnalyticsAsync(CouponAnalyticsFilterDto filter, TokenModel tokenModel);
}

// 3. IDiscountValidationService
public interface IDiscountValidationService
{
    Task<DiscountValidationResult> ValidateDiscountAsync(string code, Guid planId, int userId, decimal orderAmount);
    Task<CouponValidationResult> ValidateCouponAsync(string code, Guid planId, int userId, decimal orderAmount);
    Task<decimal> CalculateDiscountAmountAsync(Discount discount, decimal orderAmount);
    Task<decimal> CalculateCouponAmountAsync(Coupon coupon, decimal orderAmount);
    Task<bool> IsEligibleForDiscountAsync(Discount discount, Guid planId, int userId);
    Task<bool> IsEligibleForCouponAsync(Coupon coupon, Guid planId, int userId);
}

// 4. IDiscountRuleService
public interface IDiscountRuleService
{
    Task<JsonModel> CreateRuleAsync(CreateDiscountRuleDto createDto, TokenModel tokenModel);
    Task<JsonModel> UpdateRuleAsync(Guid id, UpdateDiscountRuleDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteRuleAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetRuleAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetRulesAsync(DiscountRuleFilterDto filter, TokenModel tokenModel);
    Task<JsonModel> ApplyRulesAsync(Guid planId, int userId, decimal orderAmount, TokenModel tokenModel);
    Task<JsonModel> GetRuleAnalyticsAsync(DiscountRuleAnalyticsFilterDto filter, TokenModel tokenModel);
}
```

### **PHASE 3: DTOs & CONTROLLERS (Week 5-6)**
**Priority: HIGH** | **Effort: 40 hours**

#### **A. DTOs**
```csharp
// 1. Discount DTOs
public class DiscountDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public List<Guid> ApplicablePlans { get; set; } = new();
    public List<int> ApplicableUsers { get; set; } = new();
    public bool IsValid { get; set; }
    public bool HasUsageLimit { get; set; }
    public int RemainingUsage { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class CreateDiscountDto
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DiscountType Type { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Value { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MinOrderAmount { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [Required]
    public DateTime ValidFrom { get; set; }
    
    public DateTime? ValidUntil { get; set; }
    
    public List<Guid> ApplicablePlans { get; set; } = new();
    public List<int> ApplicableUsers { get; set; } = new();
}

public class UpdateDiscountDto
{
    [Required]
    public Guid Id { get; set; }
    
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? Value { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MinOrderAmount { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }
    
    public bool? IsActive { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    
    public List<Guid>? ApplicablePlans { get; set; }
    public List<int>? ApplicableUsers { get; set; }
}

public class DiscountFilterDto
{
    public string? SearchTerm { get; set; }
    public DiscountType? Type { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public Guid? PlanId { get; set; }
    public int? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedDate";
    public string SortDirection { get; set; } = "desc";
}

// 2. Coupon DTOs (Similar structure to Discount DTOs)
public class CouponDto { /* Similar to DiscountDto */ }
public class CreateCouponDto { /* Similar to CreateDiscountDto */ }
public class UpdateCouponDto { /* Similar to UpdateDiscountDto */ }
public class CouponFilterDto { /* Similar to DiscountFilterDto */ }

// 3. Validation DTOs
public class DiscountValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; }
    public Discount? Discount { get; set; }
}

public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal CouponAmount { get; set; }
    public Coupon? Coupon { get; set; }
}

// 4. Analytics DTOs
public class DiscountAnalyticsDto
{
    public int TotalDiscounts { get; set; }
    public int ActiveDiscounts { get; set; }
    public int ExpiredDiscounts { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal AverageDiscountAmount { get; set; }
    public int TotalUsages { get; set; }
    public decimal UsageRate { get; set; }
    public List<DiscountUsageDto> TopDiscounts { get; set; } = new();
    public List<MonthlyDiscountData> MonthlyData { get; set; } = new();
}

public class CouponAnalyticsDto
{
    public int TotalCoupons { get; set; }
    public int ActiveCoupons { get; set; }
    public int ExpiredCoupons { get; set; }
    public decimal TotalCouponAmount { get; set; }
    public decimal AverageCouponAmount { get; set; }
    public int TotalUsages { get; set; }
    public decimal UsageRate { get; set; }
    public List<CouponUsageDto> TopCoupons { get; set; } = new();
    public List<MonthlyCouponData> MonthlyData { get; set; } = new();
}
```

#### **B. Controllers**
```csharp
// 1. DiscountController
[ApiController]
[Route("api/discounts")]
[Authorize]
public class DiscountController : BaseController
{
    private readonly IDiscountService _discountService;
    private readonly IDiscountValidationService _validationService;

    [HttpPost]
    public async Task<JsonModel> CreateDiscount([FromBody] CreateDiscountDto createDto)
    {
        return await _discountService.CreateDiscountAsync(createDto, GetToken(HttpContext));
    }

    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateDiscount(Guid id, [FromBody] UpdateDiscountDto updateDto)
    {
        return await _discountService.UpdateDiscountAsync(id, updateDto, GetToken(HttpContext));
    }

    [HttpDelete("{id}")]
    public async Task<JsonModel> DeleteDiscount(Guid id)
    {
        return await _discountService.DeleteDiscountAsync(id, GetToken(HttpContext));
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetDiscount(Guid id)
    {
        return await _discountService.GetDiscountAsync(id, GetToken(HttpContext));
    }

    [HttpGet]
    public async Task<JsonModel> GetDiscounts([FromQuery] DiscountFilterDto filter)
    {
        return await _discountService.GetDiscountsAsync(filter, GetToken(HttpContext));
    }

    [HttpGet("code/{code}")]
    public async Task<JsonModel> GetDiscountByCode(string code)
    {
        return await _discountService.GetDiscountByCodeAsync(code, GetToken(HttpContext));
    }

    [HttpPost("validate")]
    public async Task<JsonModel> ValidateDiscount([FromBody] ValidateDiscountRequestDto request)
    {
        return await _discountService.ValidateDiscountAsync(
            request.Code, request.PlanId, request.UserId, request.OrderAmount, GetToken(HttpContext));
    }

    [HttpPost("apply")]
    public async Task<JsonModel> ApplyDiscount([FromBody] ApplyDiscountRequestDto request)
    {
        return await _discountService.ApplyDiscountAsync(
            request.Code, request.SubscriptionId, GetToken(HttpContext));
    }

    [HttpGet("{id}/usage")]
    public async Task<JsonModel> GetDiscountUsage(Guid id)
    {
        return await _discountService.GetDiscountUsageAsync(id, GetToken(HttpContext));
    }

    [HttpGet("analytics")]
    public async Task<JsonModel> GetDiscountAnalytics([FromQuery] DiscountAnalyticsFilterDto filter)
    {
        return await _discountService.GetDiscountAnalyticsAsync(filter, GetToken(HttpContext));
    }
}

// 2. CouponController (Similar structure to DiscountController)
[ApiController]
[Route("api/coupons")]
[Authorize]
public class CouponController : BaseController
{
    // Similar endpoints to DiscountController
}
```

### **PHASE 4: INTEGRATION & TESTING (Week 7-8)**
**Priority: HIGH** | **Effort: 50 hours**

#### **A. Integration with Existing Services**
```csharp
// 1. Update AutomatedBillingService
public class AutomatedBillingService : IAutomatedBillingService
{
    private readonly IDiscountService _discountService;
    private readonly ICouponService _couponService;
    private readonly IDiscountValidationService _validationService;

    // Update CalculateDiscountAmountAsync to use new discount system
    private async Task<decimal> CalculateDiscountAmountAsync(Subscription subscription, TokenModel tokenModel)
    {
        decimal totalDiscount = 0;
        
        // Get applicable discounts for the subscription
        var applicableDiscounts = await _discountService.GetApplicableDiscountsAsync(
            subscription.SubscriptionPlanId, subscription.UserId, tokenModel);
        
        // Apply each applicable discount
        foreach (var discount in applicableDiscounts)
        {
            var discountAmount = await _validationService.CalculateDiscountAmountAsync(
                discount, subscription.CurrentPrice);
            totalDiscount += discountAmount;
        }
        
        // Apply discount rules
        var rules = await _discountRuleService.GetApplicableRulesAsync(
            subscription.SubscriptionPlanId, subscription.UserId, tokenModel);
        
        foreach (var rule in rules)
        {
            var ruleDiscount = await CalculateRuleDiscountAsync(rule, subscription);
            totalDiscount += ruleDiscount;
        }
        
        return Math.Min(totalDiscount, subscription.CurrentPrice);
    }
}

// 2. Update SubscriptionService
public class SubscriptionService : ISubscriptionService
{
    private readonly IDiscountService _discountService;
    private readonly ICouponService _couponService;

    // Add discount/coupon application methods
    public async Task<JsonModel> ApplyDiscountToSubscriptionAsync(
        Guid subscriptionId, string discountCode, TokenModel tokenModel)
    {
        // Implementation for applying discount to subscription
    }

    public async Task<JsonModel> ApplyCouponToSubscriptionAsync(
        Guid subscriptionId, string couponCode, TokenModel tokenModel)
    {
        // Implementation for applying coupon to subscription
    }
}

// 3. Update BillingService
public class BillingService : IBillingService
{
    private readonly IDiscountService _discountService;
    private readonly ICouponService _couponService;

    // Update billing record creation to include discounts/coupons
    public async Task<JsonModel> CreateBillingRecordAsync(
        CreateBillingRecordDto createDto, TokenModel tokenModel)
    {
        // Apply discounts and coupons to billing record
        var discountAmount = await CalculateTotalDiscountAsync(createDto, tokenModel);
        var couponAmount = await CalculateTotalCouponAsync(createDto, tokenModel);
        
        // Update billing record with discount/coupon amounts
        createDto.Amount -= discountAmount + couponAmount;
        
        // Continue with existing billing record creation
    }
}
```

#### **B. Frontend Integration**
```typescript
// 1. Discount Models
export interface Discount {
  id: string;
  code: string;
  name: string;
  description?: string;
  type: 'Percentage' | 'FixedAmount' | 'FreeTrial';
  value: number;
  minOrderAmount?: number;
  maxDiscountAmount?: number;
  usageLimit?: number;
  usedCount: number;
  isActive: boolean;
  validFrom: Date;
  validUntil?: Date;
  applicablePlans: string[];
  applicableUsers: number[];
  isValid: boolean;
  hasUsageLimit: boolean;
  remainingUsage: number;
  createdDate: Date;
  updatedDate?: Date;
}

export interface CreateDiscountDto {
  code: string;
  name: string;
  description?: string;
  type: 'Percentage' | 'FixedAmount' | 'FreeTrial';
  value: number;
  minOrderAmount?: number;
  maxDiscountAmount?: number;
  usageLimit?: number;
  isActive: boolean;
  validFrom: Date;
  validUntil?: Date;
  applicablePlans: string[];
  applicableUsers: number[];
}

// 2. Discount Service
@Injectable({
  providedIn: 'root'
})
export class DiscountService {
  private apiUrl = environment.apiUrl + '/discounts';

  constructor(private http: HttpClient) {}

  createDiscount(discount: CreateDiscountDto): Observable<ApiResponse<Discount>> {
    return this.http.post<ApiResponse<Discount>>(this.apiUrl, discount);
  }

  getDiscounts(filter: DiscountFilter): Observable<ApiResponse<Discount[]>> {
    return this.http.get<ApiResponse<Discount[]>>(this.apiUrl, { params: filter });
  }

  validateDiscount(code: string, planId: string, userId: number, orderAmount: number): Observable<ApiResponse<DiscountValidationResult>> {
    return this.http.post<ApiResponse<DiscountValidationResult>>(`${this.apiUrl}/validate`, {
      code, planId, userId, orderAmount
    });
  }

  applyDiscount(code: string, subscriptionId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/apply`, {
      code, subscriptionId
    });
  }
}

// 3. Coupon Service (Similar structure)
@Injectable({
  providedIn: 'root'
})
export class CouponService {
  // Similar methods to DiscountService
}
```

---

## **🎯 IMPLEMENTATION TIMELINE**

### **Week 1-2: Core Entities & Database**
- ✅ Create database tables
- ✅ Create entity classes
- ✅ Create enums
- ✅ Update DbContext
- ✅ Create migrations

### **Week 3-4: Services & Repositories**
- ✅ Create repository interfaces
- ✅ Implement repositories
- ✅ Create service interfaces
- ✅ Implement services
- ✅ Create validation services

### **Week 5-6: DTOs & Controllers**
- ✅ Create DTOs
- ✅ Create controllers
- ✅ Create validation DTOs
- ✅ Create analytics DTOs
- ✅ Update mapping profiles

### **Week 7-8: Integration & Testing**
- ✅ Integrate with existing services
- ✅ Update billing logic
- ✅ Create frontend services
- ✅ Create frontend components
- ✅ Integration testing
- ✅ End-to-end testing

---

## **📊 EFFORT ESTIMATION**

### **Total Effort: 190 hours (5 weeks)**

#### **Backend Development: 140 hours**
- Database & Entities: 40 hours
- Services & Repositories: 60 hours
- DTOs & Controllers: 40 hours

#### **Frontend Development: 30 hours**
- Models & Services: 15 hours
- Components & UI: 15 hours

#### **Integration & Testing: 20 hours**
- Backend Integration: 10 hours
- Frontend Integration: 5 hours
- Testing: 5 hours

---

## **🚀 EXPECTED OUTCOMES**

### **After Implementation:**
- ✅ **Complete discount management** - Create, update, delete discounts
- ✅ **Complete coupon management** - Create, update, delete coupons
- ✅ **Advanced discount types** - Percentage, fixed amount, free trial
- ✅ **Usage tracking** - Track discount and coupon usage
- ✅ **Validation system** - Comprehensive validation logic
- ✅ **Analytics & reporting** - Discount and coupon analytics
- ✅ **API endpoints** - Complete REST API
- ✅ **Frontend integration** - Complete UI integration
- ✅ **Billing integration** - Seamless billing integration
- ✅ **Stripe integration** - Stripe discount/coupon support

### **Business Benefits:**
- 🎯 **Increased conversions** - Attractive discount offers
- 🎯 **Customer retention** - Loyalty discounts and coupons
- 🎯 **Revenue growth** - Strategic discount campaigns
- 🎯 **Marketing flexibility** - Multiple discount types
- 🎯 **Analytics insights** - Discount performance tracking
- 🎯 **Operational efficiency** - Automated discount management

---

## **🎯 RECOMMENDATION**

**Implement this comprehensive discount and coupon system in 4 phases over 8 weeks to transform your subscription management application into a modern, feature-rich platform with advanced discount and coupon capabilities.**

# 📘 Subscription Plan Management - Developer Guide

> **✨ CURRENT IMPLEMENTATION** | Updated October 18, 2025
> 
> **System Version:** Billing Cycle-Based System with Dynamic Scaling
> 
> **Key Updates in This Guide:**
> - ✅ Billing cycle discount fields (Monthly/Quarterly/Annual)
> - ✅ BillingCycleValidator for billing cycle restrictions
> - ✅ Dynamic price and privilege calculations based on selected billing cycle

---

## Table of Contents
1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Database Schema](#database-schema)
4. [Service Architecture](#service-architecture)
5. [Complete Workflows](#complete-workflows)
6. [Code Examples](#code-examples)
7. [Integration Points](#integration-points)

---

## 1. Overview

### What is Subscription Plan Management?

Subscription Plan Management is the foundation of our subscription system. It handles the creation, configuration, and maintenance of subscription plans that users can purchase. Think of it as the "product catalog" for our healthcare subscription service.

### Key Responsibilities

- ✅ **Plan Creation**: Admins define subscription plans with pricing and features
- ✅ **Privilege Assignment**: Each plan includes specific privileges (e.g., consultations, medications)
- ✅ **Pricing Configuration**: Both base pricing (for plan purchase) and overage pricing
- ✅ **Stripe Integration**: Synchronizes plans as products in Stripe
- ✅ **Versioning**: Supports plan updates without affecting existing subscribers

---

## 2. Core Concepts

### 2.1 What is a Subscription Plan?

A **Subscription Plan** is a predefined package that includes:
- A name and description (e.g., "Basic Health Plan")
- A monthly/quarterly/annual price
- A set of privileges (what the user gets)
- Limits for each privilege (how much they can use)
- Billing cycle configuration

**Example:**
```
Plan: "Basic Health Plan"
Price: $275/month
Includes:
  - 5 Teleconsultations (base cost: $20 each, overage: $25 each)
  - 3 Medication Refills (base cost: $50 each, overage: $60 each)
Commission: 10% ($25)
```

### 2.2 Key Entities

#### **SubscriptionPlan** Entity
```csharp
public class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; }  // "Basic Health Plan"
    public string Description { get; set; }
    public decimal Price { get; set; }  // 275.00
    
    // Stripe Integration
    public string StripeProductId { get; set; }  // "prod_ABC123"
    public string StripeMonthlyPriceId { get; set; }  // "price_1Month_XYZ"
    public string StripeQuarterlyPriceId { get; set; }
    public string StripeAnnualPriceId { get; set; }
    
    // Healthcare Pricing Model
    public bool IsAutoCalculatedPrice { get; set; }  // true = auto-calculate from privileges
    public decimal? AdminCommissionPercent { get; set; }  // 10.00
    public decimal? AdminCommissionFixed { get; set; }  // 25.00
    public decimal PrivilegesTotalCost { get; set; }  // 250.00
    
    // Plan Versioning
    public int VersionNumber { get; set; }  // 1, 2, 3...
    public bool IsLatestVersion { get; set; }  // true for current version
    public Guid? ParentPlanId { get; set; }  // Links to previous version
    
    // Relationships
    public ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
}
```

#### **SubscriptionPlanPrivilege** Entity (Junction Table)
```csharp
public class SubscriptionPlanPrivilege
{
    public Guid Id { get; set; }
    public Guid SubscriptionPlanId { get; set; }  // Links to plan
    public Guid PrivilegeId { get; set; }  // Links to privilege (e.g., "Teleconsultation")
    
    // Quantity Configuration
    public int Value { get; set; }  // 5 consultations included in plan
    
    // Pricing Configuration
    public decimal PrivilegeBaseCost { get; set; }  // $20 (used for plan price calculation)
    public decimal UnitCost { get; set; }  // $25 (used for overage billing)
    
    // Usage Limits
    public int? DailyLimit { get; set; }  // Optional daily cap
    public int? WeeklyLimit { get; set; }  // Optional weekly cap
    public int? MonthlyLimit { get; set; }  // 5 per month
    
    // Relationships
    public SubscriptionPlan SubscriptionPlan { get; set; }
    public Privilege Privilege { get; set; }
}
```

#### **Privilege** Entity (Master List)
```csharp
public class Privilege
{
    public Guid Id { get; set; }
    public string Name { get; set; }  // "Teleconsultation", "Medication Refill"
    public string Description { get; set; }
    public string Category { get; set; }  // "Medical Services", "Prescriptions"
    public bool IsActive { get; set; }
    
    // Relationships
    public ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; }
}
```

### 2.3 Pricing Model Explained

#### **Two Types of Costs:**

1. **PrivilegeBaseCost** - Used for **calculating plan price**
   ```
   Example: Teleconsultation base cost = $20
   If plan includes 5 consultations:
   Contribution to plan price = 5 × $20 = $100
   ```

2. **UnitCost** - Used for **overage billing**
   ```
   Example: Teleconsultation overage cost = $25
   If user exceeds 5 and wants 6th:
   Charge for extra = 1 × $25 = $25
   ```

#### **Auto-Calculated Price Formula:**
```
Plan Price = Σ(Privilege Value × PrivilegeBaseCost) + Admin Commission

Example:
  Teleconsultations: 5 × $20 = $100
  Medications: 3 × $50 = $150
  ─────────────────────────────
  Privileges Total: $250
  
  Commission (10%): $250 × 0.10 = $25
  ─────────────────────────────
  FINAL PLAN PRICE: $275/month
```

---

## 3. Database Schema

### 3.1 Table: SubscriptionPlans

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | f3a1b2c3-... |
| Name | NVARCHAR(200) | Plan name | "Basic Health Plan" |
| Description | NVARCHAR(MAX) | Plan details | "Essential healthcare..." |
| Price | DECIMAL(18,2) | Monthly price | 275.00 |
| BillingCycleId | UNIQUEIDENTIFIER | FK to billing cycles | monthly-guid |
| IsActive | BIT | Plan availability | 1 (true) |
| IsAutoCalculatedPrice | BIT | Auto vs manual pricing | 1 (true) |
| AdminCommissionPercent | DECIMAL(5,2) | Commission % | 10.00 |
| AdminCommissionFixed | DECIMAL(18,2) | Fixed commission $ | NULL or 25.00 |
| PrivilegesTotalCost | DECIMAL(18,2) | Sum of privilege costs | 250.00 |
| StripeProductId | NVARCHAR(255) | Stripe product ID | "prod_ABC123" |
| StripeMonthlyPriceId | NVARCHAR(255) | Stripe price ID (monthly) | "price_1Month_XYZ" |
| VersionNumber | INT | Plan version | 1 |
| IsLatestVersion | BIT | Current version flag | 1 (true) |
| ParentPlanId | UNIQUEIDENTIFIER | Previous version link | NULL or parent-guid |

### 3.2 Table: SubscriptionPlanPrivileges

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | privilege-123 |
| SubscriptionPlanId | UNIQUEIDENTIFIER | FK to plan | f3a1b2c3-... |
| PrivilegeId | UNIQUEIDENTIFIER | FK to privilege | telecon-guid |
| Value | INT | Quantity in plan | 5 |
| PrivilegeBaseCost | DECIMAL(18,2) | Base cost (for pricing) | 20.00 |
| UnitCost | DECIMAL(18,2) | Overage cost | 25.00 |
| DailyLimit | INT | Daily cap (optional) | NULL |
| WeeklyLimit | INT | Weekly cap (optional) | NULL |
| MonthlyLimit | INT | Monthly cap | 5 |
| IsActive | BIT | Active status | 1 (true) |

### 3.3 Table: Privileges (Master Reference)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | telecon-guid |
| Name | NVARCHAR(200) | Privilege name | "Teleconsultation" |
| Description | NVARCHAR(MAX) | Details | "Video consultation with doctor" |
| Category | NVARCHAR(100) | Privilege category | "Medical Services" |
| IsActive | BIT | Availability | 1 (true) |

### 3.4 Relationships Diagram

```
Privileges (Master List)
    ↓
    │ One-to-Many
    ↓
SubscriptionPlanPrivileges (Junction)
    ↓
    │ Many-to-One
    ↓
SubscriptionPlans
    ↓
    │ One-to-Many
    ↓
Subscriptions (User subscriptions)
```

---

## 4. Service Architecture

### 4.1 Service Responsibilities

#### **SubscriptionPlanService** (Primary Service)
**Location:** `SmartTelehealth.Application/Services/SubscriptionPlanService.cs`

**Responsibilities:**
- Create/Update/Delete subscription plans
- Manage plan privileges
- Calculate pricing
- Sync with Stripe
- Plan versioning

**Dependencies:**
```csharp
ISubscriptionPlanRepository _subscriptionPlanRepository
ISubscriptionPlanPrivilegeRepository _planPrivilegeRepository
IStripeService _stripeService
IPrivilegeRepository _privilegeRepository
IPlanPricingService _pricingService
IUnitOfWork _unitOfWork
IMapper _mapper
ILogger<SubscriptionPlanService> _logger
```

#### **IPlanPricingService** (Pricing Logic)
**Location:** `SmartTelehealth.Application/Services/PlanPricingService.cs`

**Responsibilities:**
- Calculate plan base price from privileges
- Calculate pricing breakdown
- Validate pricing configurations

#### **IStripeService** (Stripe Integration)
**Location:** `SmartTelehealth.Infrastructure/Services/StripeService.cs`

**Responsibilities:**
- Create/update Stripe products
- Create/update Stripe prices
- Manage Stripe resources

### 4.2 Repository Layer

#### **ISubscriptionPlanRepository**
**Location:** `SmartTelehealth.Infrastructure/Repositories/SubscriptionPlanRepository.cs`

**Key Methods:**
```csharp
Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan)
Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan plan)
Task<SubscriptionPlan> GetByIdWithDetailsAsync(Guid planId)
Task<IEnumerable<SubscriptionPlan>> GetAllActivePlansAsync()
Task<IEnumerable<SubscriptionPlanPrivilege>> GetPlanPrivilegesAsync(Guid planId)
```

#### **ISubscriptionPlanPrivilegeRepository**
**Key Methods:**
```csharp
Task<SubscriptionPlanPrivilege> CreateAsync(SubscriptionPlanPrivilege privilege)
Task<SubscriptionPlanPrivilege> UpdateAsync(SubscriptionPlanPrivilege privilege)
Task<bool> DeleteAsync(Guid id)
Task<IEnumerable<SubscriptionPlanPrivilege>> GetByPlanIdAsync(Guid planId)
```

---

## 5. Complete Workflows

### 5.1 Workflow: Admin Creates a Subscription Plan

#### **Step-by-Step Process**

```
┌─────────────────────────────────────────────────┐
│ ADMIN ACTION: Create Plan                       │
│ Endpoint: POST /api/subscription-plans/admin    │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionPlansController                      │
│ Method: CreatePlan(CreateSubscriptionPlanDto)   │
└─────────────────────────────────────────────────┘
                    ↓
        Calls SubscriptionPlanService
                    ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionPlanService.CreatePlanAsync()       │
│                                                  │
│ [STEP 1] Validate Admin Authorization           │
│   if (tokenModel.RoleID != Admin) → 403         │
│                                                  │
│ [STEP 2] BEGIN TRANSACTION                      │
│   _unitOfWork.BeginTransactionAsync()           │
│                                                  │
│ [STEP 3] Create Plan Entity in Database         │
│   var plan = new SubscriptionPlan {             │
│     Name = "Basic Health",                      │
│     Price = 0,  // Will be calculated           │
│     IsAutoCalculatedPrice = true,               │
│     AdminCommissionPercent = 10,                │
│     VersionNumber = 1,                          │
│     IsLatestVersion = true                      │
│   };                                            │
│   createdPlan = await _repo.CreatePlanAsync()   │
│                                                  │
│ [STEP 4] Create Stripe Product                  │
│   stripeProductId = await _stripeService        │
│     .CreateProductAsync(                        │
│       name: "Basic Health",                     │
│       description: "..."                        │
│     )                                           │
│   → Returns: "prod_ABC123"                      │
│                                                  │
│ [STEP 5] Create Stripe Prices (3 tiers)         │
│   monthlyId = await _stripeService              │
│     .CreatePriceAsync(productId, $275, "month") │
│   quarterlyId = await _stripeService            │
│     .CreatePriceAsync(productId, $825, "month") │
│   annualId = await _stripeService               │
│     .CreatePriceAsync(productId, $3300, "month")│
│                                                  │
│ [STEP 6] Update Plan with Stripe IDs            │
│   plan.StripeProductId = "prod_ABC123"          │
│   plan.StripeMonthlyPriceId = monthlyId         │
│   await _repo.UpdatePlanAsync(plan)             │
│                                                  │
│ [STEP 7] Assign Privileges to Plan              │
│   foreach (privilege in createDto.Privileges) { │
│     var planPrivilege = new                     │
│       SubscriptionPlanPrivilege {               │
│       PrivilegeId = privilege.Id,               │
│       Value = 5,  // Quantity                   │
│       PrivilegeBaseCost = 20.00,                │
│       UnitCost = 25.00,                         │
│       MonthlyLimit = 5                          │
│     };                                          │
│     await _privilegeRepo.CreateAsync()          │
│   }                                             │
│                                                  │
│ [STEP 8] Auto-Calculate Price                   │
│   if (IsAutoCalculatedPrice) {                  │
│     breakdown = await _pricingService           │
│       .CalculatePricingBreakdownAsync(planId)   │
│                                                  │
│     plan.Price = breakdown.FinalPrice  // $275  │
│     plan.PrivilegesTotalCost = $250             │
│     await _repo.UpdatePlanAsync(plan)           │
│   }                                             │
│                                                  │
│ [STEP 9] COMMIT TRANSACTION                     │
│   _unitOfWork.CommitTransactionAsync()          │
│                                                  │
│ [STEP 10] Return Success                        │
│   return JsonModel {                            │
│     data = planDto,                             │
│     Message = "Plan created",                   │
│     StatusCode = 201                            │
│   }                                             │
└─────────────────────────────────────────────────┘
```

#### **Error Handling**

If ANY step fails (e.g., Stripe API error):
```csharp
catch (Exception ex)
{
    // ROLLBACK DATABASE TRANSACTION
    await _unitOfWork.RollbackTransactionAsync();
    
    // CLEANUP STRIPE RESOURCES
    if (!string.IsNullOrEmpty(stripeProductId))
    {
        await _stripeService.DeleteProductAsync(stripeProductId);
        await _stripeService.DeactivatePriceAsync(monthlyId);
        // ... cleanup other resources
    }
    
    _logger.LogError(ex, "Plan creation failed");
    return new JsonModel { 
        data = new object(), 
        Message = $"Failed: {ex.Message}", 
        StatusCode = 500 
    };
}
```

---

## 6. Code Examples

### 6.1 Creating a Plan (Full Code)

**Controller Method:**
```csharp
[HttpPost("admin")]
[Authorize(Roles = "Admin")]
public async Task<JsonModel> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
{
    return await _subscriptionPlanService.CreatePlanAsync(createDto, GetToken(HttpContext));
}
```

**DTO Structure:**
```csharp
public class CreateSubscriptionPlanDto
{
    public string Name { get; set; }  // "Basic Health Plan"
    public string Description { get; set; }
    public decimal Price { get; set; }  // 0 if auto-calculated
    public Guid BillingCycleId { get; set; }  // monthly/quarterly/yearly
    public bool IsAutoCalculatedPrice { get; set; }  // true
    public decimal? AdminCommissionPercent { get; set; }  // 10.00
    public decimal? AdminCommissionFixed { get; set; }  // null or fixed amount
    
    public List<PlanPrivilegeDto> Privileges { get; set; }
}

public class PlanPrivilegeDto
{
    public Guid PrivilegeId { get; set; }  // teleconsultation-guid
    public int Value { get; set; }  // 5
    public decimal PrivilegeBaseCost { get; set; }  // 20.00
    public decimal UnitCost { get; set; }  // 25.00
    public int? DailyLimit { get; set; }
    public int? WeeklyLimit { get; set; }
    public int? MonthlyLimit { get; set; }  // 5
}
```

**Service Implementation (Simplified):**
```csharp
public async Task<JsonModel> CreatePlanAsync(
    CreateSubscriptionPlanDto createDto, 
    TokenModel tokenModel)
{
    // 1. Validate Authorization
    if (tokenModel.RoleID != (int)RoleId.Admin)
        return new JsonModel { StatusCode = 403, Message = "Admin only" };
    
    // 2. Begin Transaction
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // 3. Create Plan Entity
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Price = createDto.Price,
            IsAutoCalculatedPrice = createDto.IsAutoCalculatedPrice,
            AdminCommissionPercent = createDto.AdminCommissionPercent,
            VersionNumber = 1,
            IsLatestVersion = true,
            CreatedBy = tokenModel.UserID,
            CreatedDate = DateTime.UtcNow
        };
        
        var createdPlan = await _subscriptionPlanRepository.CreatePlanAsync(plan);
        
        // 4. Create Stripe Product
        var stripeProductId = await _stripeService.CreateProductAsync(
            createdPlan.Name, 
            createdPlan.Description ?? "", 
            tokenModel
        );
        createdPlan.StripeProductId = stripeProductId;
        
        // 5. Create Stripe Prices
        var monthlyPriceId = await _stripeService.CreatePriceAsync(
            stripeProductId, 
            createdPlan.Price, 
            "usd", 
            "month", 
            1, 
            tokenModel
        );
        createdPlan.StripeMonthlyPriceId = monthlyPriceId;
        
        // 6. Update Plan with Stripe IDs
        await _subscriptionPlanRepository.UpdatePlanAsync(createdPlan);
        
        // 7. Assign Privileges
        foreach (var privilege in createDto.Privileges)
        {
            var planPrivilege = new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = createdPlan.Id,
                PrivilegeId = privilege.PrivilegeId,
                Value = privilege.Value,
                PrivilegeBaseCost = privilege.PrivilegeBaseCost,
                UnitCost = privilege.UnitCost,
                MonthlyLimit = privilege.MonthlyLimit,
                IsActive = true
            };
            
            await _planPrivilegeRepository.CreateAsync(planPrivilege);
        }
        
        // 8. Auto-Calculate Price
        if (createdPlan.IsAutoCalculatedPrice)
        {
            var breakdown = await _pricingService
                .CalculatePricingBreakdownAsync(createdPlan.Id);
            
            createdPlan.Price = breakdown.FinalPrice;
            createdPlan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
            
            await _subscriptionPlanRepository.UpdatePlanAsync(createdPlan);
        }
        
        // 9. Commit Transaction
        await _unitOfWork.CommitTransactionAsync();
        
        // 10. Return Success
        var planDto = _mapper.Map<SubscriptionPlanDto>(createdPlan);
        return new JsonModel { 
            data = planDto, 
            Message = "Plan created successfully", 
            StatusCode = 201 
        };
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Failed to create plan");
        return new JsonModel { 
            StatusCode = 500, 
            Message = $"Failed: {ex.Message}" 
        };
    }
}
```

### 6.2 Querying Plans

**Get All Active Plans:**
```csharp
public async Task<JsonModel> GetAllActivePlansAsync(TokenModel tokenModel)
{
    var plans = await _subscriptionPlanRepository.GetAllActivePlansAsync();
    var planDtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
    
    return new JsonModel { 
        data = planDtos, 
        Message = "Plans retrieved", 
        StatusCode = 200 
    };
}
```

**Get Plan by ID with Privileges:**
```csharp
public async Task<JsonModel> GetPlanByIdAsync(string planId, TokenModel tokenModel)
{
    if (!Guid.TryParse(planId, out var planGuid))
        return new JsonModel { StatusCode = 400, Message = "Invalid ID" };
    
    var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
    
    if (plan == null)
        return new JsonModel { StatusCode = 404, Message = "Plan not found" };
    
    var planDto = _mapper.Map<SubscriptionPlanDto>(plan);
    
    return new JsonModel { 
        data = planDto, 
        Message = "Plan retrieved", 
        StatusCode = 200 
    };
}
```

---

## 7. Integration Points

### 7.1 Stripe Integration

**When Plan is Created:**
```
YOUR DATABASE             →             STRIPE
SubscriptionPlan created  →  Create Product (prod_ABC123)
                         →  Create Prices (price_1Month_XYZ, etc.)
                         ←  Return Stripe IDs
Update Plan with IDs     ←
```

**Stripe Product Structure:**
```json
{
  "id": "prod_ABC123",
  "object": "product",
  "name": "Basic Health Plan",
  "description": "Essential healthcare services...",
  "active": true,
  "metadata": {
    "planId": "f3a1b2c3-...",
    "source": "SmartTelehealth"
  }
}
```

**Stripe Price Structure:**
```json
{
  "id": "price_1Month_XYZ",
  "object": "price",
  "product": "prod_ABC123",
  "unit_amount": 27500,  // $275.00 in cents
  "currency": "usd",
  "recurring": {
    "interval": "month",
    "interval_count": 1
  },
  "active": true
}
```

### 7.2 How Plans Connect to Subscriptions

```
┌──────────────────┐
│ SubscriptionPlan │  (Plan definition - what's offered)
│  Id, Name, Price │
└────────┬─────────┘
         │ One-to-Many
         ↓
┌──────────────────┐
│  Subscription    │  (User's active subscription)
│  UserId, PlanId  │
└──────────────────┘
```

**When user subscribes:**
1. User selects a plan (e.g., "Basic Health")
2. System creates a `Subscription` record linking `UserId` to `PlanId`
3. System copies plan privileges to `UserSubscriptionPrivilegeUsage`
4. User gets access to services defined in the plan

---

## 8. Key Takeaways for New Developers

### ✅ Do's

1. **Always use transactions** when creating/updating plans
2. **Always sync with Stripe** - never update DB without updating Stripe
3. **Use AutoMapper** for entity-to-DTO conversions
4. **Log everything** - helps with debugging
5. **Validate admin role** before allowing plan modifications

### ❌ Don'ts

1. **Don't modify plans** with active subscriptions directly
2. **Don't skip Stripe cleanup** on errors
3. **Don't hardcode prices** - always use privilege-based calculation
4. **Don't forget versioning** when updating plans
5. **Don't allow price changes** without creating new version

### 🔍 Common Pitfalls

1. **Forgetting to rollback transactions** on errors
2. **Creating Stripe resources without DB transaction**
3. **Not handling Stripe API errors properly**
4. **Mixing up PrivilegeBaseCost vs UnitCost**
5. **Not invalidating old plan versions**

---

## Next Steps

Continue to:
- **Guide 02**: User Subscription Lifecycle Management
- **Guide 03**: Billing and Payment Processing
- **Guide 04**: Privilege Usage and Tracking
- **Guide 05**: Stripe Integration Deep Dive

---

**Document Version:** 1.0  
**Last Updated:** October 17, 2025  
**Author:** Development Team




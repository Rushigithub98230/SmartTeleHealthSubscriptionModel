# 🚀 Base Price Calculation - Implementation Guide

**Priority:** CRITICAL - Required for client workflow  
**Estimated Time:** 3-4 hours  
**Impact:** Completes 95% → 100% readiness

---

## 🎯 WHAT NEEDS TO BE IMPLEMENTED

Your client's workflow requires:
```
Base Price = (consultationFee × limitConsultations) + 
             (medicationFee × limitMedications) + 
             adminCommission
```

**Current Gap:** Manual price entry instead of automated calculation

---

## 📋 STEP-BY-STEP IMPLEMENTATION

### **Step 1: Add DTOs (15 minutes)**

Create `CalculatePlanBasePriceDto.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class CalculatePlanBasePriceDto
{
    [Required]
    public Guid PlanId { get; set; }
    
    [Range(0, 100, ErrorMessage = "Commission percentage must be between 0 and 100")]
    public decimal? AdminCommissionPercentage { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Commission amount must be positive")]
    public decimal? AdminCommissionFixed { get; set; }
}
```

### **Step 2: Update CreateSubscriptionPlanDto (10 minutes)**

Add to existing `CreateSubscriptionPlanDto.cs`:
```csharp
// Add these fields to the existing DTO
[Range(0, 100, ErrorMessage = "Commission percentage must be between 0 and 100")]
public decimal? AdminCommissionPercentage { get; set; }

[Range(0, double.MaxValue, ErrorMessage = "Commission amount must be positive")]
public decimal? AdminCommissionFixed { get; set; }

public bool AutoCalculatePrice { get; set; } = true;
```

### **Step 3: Update SubscriptionPlan Entity (10 minutes)**

Add to `SubscriptionPlan.cs`:
```csharp
// Add these properties
[Column(TypeName = "decimal(18,2)")]
public decimal? AdminCommissionPercentage { get; set; }

[Column(TypeName = "decimal(18,2)")]
public decimal? AdminCommissionFixed { get; set; }

[Column(TypeName = "decimal(18,2)")]
public decimal CalculatedBasePrice { get; set; }

[Column(TypeName = "decimal(18,2)")]
public decimal AdminCommissionAmount { get; set; }
```

### **Step 4: Add Interface Method (5 minutes)**

Add to `ISubscriptionPlanService.cs`:
```csharp
Task<JsonModel> CalculatePlanBasePriceAsync(CalculatePlanBasePriceDto dto, TokenModel tokenModel);
```

### **Step 5: Implement Calculation Method (45 minutes)**

Add to `SubscriptionPlanService.cs`:
```csharp
public async Task<JsonModel> CalculatePlanBasePriceAsync(
    CalculatePlanBasePriceDto dto, 
    TokenModel tokenModel)
{
    try
    {
        // Validate admin access
        if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
        {
            return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
        }

        _logger.LogInformation("Calculating base price for plan {PlanId} by user {UserId}", dto.PlanId, tokenModel.UserID);

        // Get plan privileges
        var privileges = await _subscriptionPlanRepository.GetPlanPrivilegesAsync(dto.PlanId);
        if (!privileges.Any())
        {
            return new JsonModel 
            { 
                data = new object(), 
                Message = "No privileges found for this plan", 
                StatusCode = 404 
            };
        }

        decimal basePrice = 0;
        var breakdown = new List<object>();

        // Calculate: (unitCost × limit) for each privilege
        foreach (var privilege in privileges)
        {
            if (privilege.Value > 0) // Only limited privileges (not unlimited or disabled)
            {
                var subtotal = privilege.UnitCost * privilege.Value;
                basePrice += subtotal;
                
                breakdown.Add(new
                {
                    privilegeName = privilege.Privilege?.Name ?? "Unknown",
                    unitCost = privilege.UnitCost,
                    limit = privilege.Value,
                    subtotal = subtotal
                });
            }
        }

        // Add admin commission
        decimal commission = 0;
        if (dto.AdminCommissionPercentage.HasValue)
        {
            commission = basePrice * (dto.AdminCommissionPercentage.Value / 100);
        }
        else if (dto.AdminCommissionFixed.HasValue)
        {
            commission = dto.AdminCommissionFixed.Value;
        }

        decimal totalPrice = basePrice + commission;

        _logger.LogInformation(
            "Calculated base price for plan {PlanId}: Base=${BasePrice}, Commission=${Commission}, Total=${TotalPrice}",
            dto.PlanId, basePrice, commission, totalPrice
        );

        return new JsonModel
        {
            data = new
            {
                basePrice,
                commission,
                totalPrice,
                breakdown,
                calculation = new
                {
                    adminCommissionPercentage = dto.AdminCommissionPercentage,
                    adminCommissionFixed = dto.AdminCommissionFixed,
                    commissionType = dto.AdminCommissionPercentage.HasValue ? "percentage" : "fixed"
                }
            },
            Message = "Base price calculated successfully",
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error calculating base price for plan {PlanId}", dto.PlanId);
        return new JsonModel { data = new object(), Message = "Error calculating base price", StatusCode = 500 };
    }
}
```

### **Step 6: Update Plan Creation Method (20 minutes)**

Modify `CreateSubscriptionPlanAsync` in `SubscriptionPlanService.cs`:
```csharp
// Add this logic after privilege creation, before final save
if (createDto.AutoCalculatePrice)
{
    // Calculate base price if auto-calculation is enabled
    var calculateDto = new CalculatePlanBasePriceDto
    {
        PlanId = createdPlan.Id,
        AdminCommissionPercentage = createDto.AdminCommissionPercentage,
        AdminCommissionFixed = createDto.AdminCommissionFixed
    };
    
    var priceResult = await CalculatePlanBasePriceAsync(calculateDto, tokenModel);
    if (priceResult.StatusCode == 200 && priceResult.data != null)
    {
        // Update plan with calculated price
        var priceData = priceResult.data;
        createdPlan.CalculatedBasePrice = (decimal)priceData.GetType().GetProperty("basePrice").GetValue(priceData);
        createdPlan.AdminCommissionAmount = (decimal)priceData.GetType().GetProperty("commission").GetValue(priceData);
        createdPlan.Price = (decimal)priceData.GetType().GetProperty("totalPrice").GetValue(priceData);
        
        _logger.LogInformation("Updated plan {PlanId} with calculated price: ${Price}", createdPlan.Id, createdPlan.Price);
    }
}
```

### **Step 7: Add API Endpoint (10 minutes)**

Add to `SubscriptionPlansController.cs`:
```csharp
[HttpPost("calculate-base-price")]
public async Task<JsonModel> CalculateBasePrice([FromBody] CalculatePlanBasePriceDto dto)
{
    return await _subscriptionPlanService.CalculatePlanBasePriceAsync(dto, GetToken(HttpContext));
}
```

### **Step 8: Add Database Migration (15 minutes)**

Create migration for new fields:
```bash
# In Package Manager Console
Add-Migration AddCommissionFieldsToSubscriptionPlan
Update-Database
```

---

## 🧪 TESTING THE IMPLEMENTATION

### **Test Case 1: Client's "Standard Plan"**

```csharp
// Test data matching client's example
var calculateDto = new CalculatePlanBasePriceDto
{
    PlanId = planId,
    AdminCommissionFixed = 30.00m
};

// Expected privileges:
// - 5 consultations @ $20 each = $100
// - 3 medications @ $50 each = $150
// - Admin commission = $30
// - Total = $280
```

### **Test Case 2: Percentage Commission**

```csharp
var calculateDto = new CalculatePlanBasePriceDto
{
    PlanId = planId,
    AdminCommissionPercentage = 10m // 10%
};

// Expected: Base price × 1.10
```

### **Test API Call:**
```bash
POST /api/subscription-plans/calculate-base-price
{
  "planId": "guid-here",
  "adminCommissionFixed": 30.00
}
```

---

## ✅ VALIDATION CHECKLIST

After implementation, verify:

- [ ] ✅ `CalculatePlanBasePriceAsync` method works
- [ ] ✅ Commission calculation is accurate
- [ ] ✅ Plan creation uses calculated price
- [ ] ✅ API endpoint responds correctly
- [ ] ✅ Database fields are created
- [ ] ✅ Client's "Standard Plan" scenario works
- [ ] ✅ Error handling is robust
- [ ] ✅ Logging is comprehensive

---

## 🎯 EXPECTED RESULTS

### **Before Implementation:**
```json
{
  "message": "Manual price entry required",
  "status": "❌ Gap exists"
}
```

### **After Implementation:**
```json
{
  "data": {
    "basePrice": 250.00,
    "commission": 30.00,
    "totalPrice": 280.00,
    "breakdown": [
      {
        "privilegeName": "Teleconsultation",
        "unitCost": 20.00,
        "limit": 5,
        "subtotal": 100.00
      },
      {
        "privilegeName": "Medication",
        "unitCost": 50.00,
        "limit": 3,
        "subtotal": 150.00
      }
    ]
  },
  "message": "Base price calculated successfully",
  "statusCode": 200
}
```

---

## 🚀 DEPLOYMENT NOTES

1. **Database Migration Required** - New fields need to be added
2. **Backward Compatibility** - Existing plans will have null commission fields
3. **Frontend Updates** - Plan creation form should show calculated price
4. **Testing Priority** - Test with client's exact scenario first

---

## 💡 SUCCESS METRICS

**Implementation is successful when:**
- ✅ Client's "Standard Plan" calculates to exactly $280
- ✅ Commission tracking works for both percentage and fixed
- ✅ Plan creation automatically uses calculated price
- ✅ API responds with detailed breakdown
- ✅ Error handling prevents invalid calculations

---

**Total Implementation Time: 3-4 hours**  
**Impact: Completes client workflow requirements**  
**Priority: CRITICAL - Deploy immediately after implementation**

---

**End of Implementation Guide**


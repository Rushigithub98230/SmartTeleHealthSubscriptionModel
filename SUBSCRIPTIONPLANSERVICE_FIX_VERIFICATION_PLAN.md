# SubscriptionPlanService - Fix Verification Plan
**Date**: October 21, 2025  
**Status**: PRE-IMPLEMENTATION REVIEW

---

## 🎯 VERIFICATION CHECKLIST

### **1. Audit Properties Verification**

#### **Current Audit Property Usage in Service**:

| Operation | Should Set | Current Implementation |
|-----------|-----------|----------------------|
| **CREATE** | CreatedBy, CreatedDate, IsActive | ✅ Line 284-285 |
| **UPDATE** | UpdatedBy, UpdatedDate | ✅ Line 484, 1034-1035 |
| **DELETE** | DeletedBy, DeletedDate, IsDeleted | ✅ Line 726-730 (soft delete), Line 1378-1381 |
| **ACTIVATE** | UpdatedBy, UpdatedDate | ✅ Line 483-484 |
| **DEACTIVATE** | UpdatedBy, UpdatedDate | ✅ Line 1167-1169 |

#### **Audit Properties in Plan Privilege Operations**:

| Operation | Should Set | Current Implementation |
|-----------|-----------|----------------------|
| **Create PlanPrivilege** | CreatedBy, CreatedDate, IsActive | ✅ Line 366-368, Line 630-631 |
| **Update PlanPrivilege** | UpdatedBy, UpdatedDate | ✅ Line 809-810 |
| **Delete PlanPrivilege** | DeletedBy, DeletedDate, IsDeleted, UpdatedBy, UpdatedDate | ✅ Line 726-730 |

---

## 🔍 DETAILED FIX VERIFICATION

### **FIX #1: CreatePlanAsync - Name Uniqueness Check**

**Location**: Line 212-216  
**Type**: Critical - Repository optimization

**Current Code**:
```csharp
// Check if plan with same name already exists
var existingPlans = await _subscriptionPlanRepository.GetAllWithDetailsAsync();
if (existingPlans.Any(p => p.Name.Equals(createDto.Name, StringComparison.OrdinalIgnoreCase)))
{
    return new JsonModel { data = new object(), Message = "A plan with this name already exists", StatusCode = 400 };
}
```

**Proposed Fix**:
```csharp
// Check if plan with same name already exists (database-level check)
if (!await _subscriptionPlanRepository.IsNameUniqueAsync(createDto.Name))
{
    return new JsonModel { data = new object(), Message = "A plan with this name already exists", StatusCode = 400 };
}
```

**Verification**:
- ✅ Repository method exists: `IsNameUniqueAsync(string name, Guid? excludeId = null)` in ISubscriptionPlanRepository
- ✅ No audit properties involved (read-only operation)
- ✅ Logic equivalent: Both check name uniqueness
- ✅ No side effects
- ✅ Error message unchanged (user experience maintained)

**Risk**: 🟢 **LOW** - Simple read operation replacement

---

### **FIX #2: DeactivatePlanAsync - Active Subscriptions Check**

**Location**: Line 1130-1134  
**Type**: Critical - Repository optimization

**Current Code**:
```csharp
// Check if plan has active subscriptions
var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
if (activeSubscriptions.Any(s => s.SubscriptionPlanId == existingPlan.Id))
{
    return new JsonModel { data = new object(), Message = "Cannot deactivate plan with active subscriptions...", StatusCode = 400 };
}
```

**Proposed Fix**:
```csharp
// Check if plan has active subscriptions (database-level check)
if (await _subscriptionPlanRepository.HasActiveSubscriptionsAsync(existingPlan.Id))
{
    return new JsonModel { data = new object(), Message = "Cannot deactivate plan with active subscriptions. Please wait for all subscriptions to end or cancel them first.", StatusCode = 400 };
}
```

**Verification**:
- ✅ Repository method exists: `HasActiveSubscriptionsAsync(Guid id)` in ISubscriptionPlanRepository
- ✅ No audit properties involved (read-only operation)
- ✅ Logic equivalent: Both check for active subscriptions
- ✅ No side effects
- ✅ Error message unchanged

**Context Check**: Deactivation still properly sets audit properties (Line 1167-1169):
```csharp
existingPlan.IsActive = false;
existingPlan.UpdatedDate = DateTime.UtcNow;
existingPlan.UpdatedBy = tokenModel?.UserID ?? 0;
```

**Risk**: 🟢 **LOW** - Simple read operation replacement

---

### **FIX #3: DeletePlanAsync - Active Subscriptions Check**

**Location**: Line 1313-1317  
**Type**: Critical - Repository optimization

**Current Code**: Same as Fix #2

**Proposed Fix**: Same as Fix #2

**Verification**:
- ✅ Repository method exists
- ✅ No audit properties involved (read-only operation)
- ✅ Logic equivalent
- ✅ No side effects

**Context Check**: Soft delete still properly sets audit properties (Line 1378-1381):
```csharp
existingPlan.DeletedBy = tokenModel.UserID;
existingPlan.DeletedDate = DateTime.UtcNow;
existingPlan.UpdatedBy = tokenModel.UserID;
existingPlan.UpdatedDate = DateTime.UtcNow;
```

**Risk**: 🟢 **LOW** - Simple read operation replacement

---

### **FIX #4: ActivatePlanAsync - Use Repository Method**

**Location**: Line 472-486  
**Type**: Medium - Optimization + simplification

**Current Code**:
```csharp
var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(Guid.Parse(planId));
if (plan == null)
    return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };

if (plan.IsActive)
    return new JsonModel { data = new object(), Message = "Plan is already active", StatusCode = 400 };

plan.IsActive = true;
plan.UpdatedBy = tokenModel.UserID;
plan.UpdatedDate = DateTime.UtcNow;
await _subscriptionPlanRepository.UpdatePlanAsync(plan);
return new JsonModel { data = true, Message = "Plan activated", StatusCode = 200 };
```

**Proposed Fix Option A** (Use repository method):
```csharp
var plan = await _subscriptionPlanRepository.GetByIdAsync(Guid.Parse(planId));
if (plan == null)
    return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };

if (plan.IsActive)
    return new JsonModel { data = new object(), Message = "Plan is already active", StatusCode = 400 };

plan.IsActive = true;
plan.UpdatedBy = tokenModel.UserID;
plan.UpdatedDate = DateTime.UtcNow;
await _subscriptionPlanRepository.UpdatePlanAsync(plan);
return new JsonModel { data = true, Message = "Plan activated", StatusCode = 200 };
```

**Verification**:
- ✅ Repository method exists: `GetByIdAsync(Guid id)` in IRepositoryBase<SubscriptionPlan>
- ✅ Audit properties MAINTAINED: UpdatedBy, UpdatedDate still set
- ✅ Logic unchanged: Same validation flow
- ✅ No side effects
- ⚠️ **IMPORTANT**: We need the full plan entity for update, so we CANNOT use repository's `ActivateAsync()` if we need to set audit properties in the service

**Decision**: Use `GetByIdAsync()` instead of `GetByIdWithDetailsAsync()` - maintains audit property control in service layer

**Risk**: 🟢 **LOW** - Simple optimization, audit properties maintained

---

### **FIX #5: AssignPrivilegesToPlanAsync - Plan Existence Check**

**Location**: Line 595-600  
**Type**: Medium - Optimization

**Current Code**:
```csharp
// Check if plan exists
var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
if (plan == null)
{
    await _unitOfWork.RollbackTransactionAsync();
    return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
}
```

**Proposed Fix**:
```csharp
// Check if plan exists (lightweight query - we need the plan object for auto-pricing later)
var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
if (plan == null)
{
    await _unitOfWork.RollbackTransactionAsync();
    return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
}
```

**Verification**:
- ✅ Repository method exists
- ✅ Plan object needed later (Line 651: `if (plan.IsAutoCalculatedPrice && assignedCount > 0)`)
- ✅ Audit properties set correctly (Line 630-631 for privileges, Line 658-659 for plan update)
- ✅ No audit property regression

**Context Check**: Privilege creation sets audit properties correctly (Line 630-631):
```csharp
IsActive = true,
CreatedBy = tokenModel.UserID,
CreatedDate = DateTime.UtcNow
```

**Risk**: 🟢 **LOW** - Simple optimization, plan object still loaded

---

### **FIX #6: AssignPrivilegesToPlanAsync - Privilege Existence Check**

**Location**: Line 609-615  
**Type**: Medium - Optimization (in loop!)

**Current Code**:
```csharp
// Validate privilege exists
var privilegeEntity = await _privilegeRepository.GetByIdAsync(privilege.PrivilegeId);
if (privilegeEntity == null)
{
    _logger.LogWarning("Privilege {PrivilegeId} not found, skipping", privilege.PrivilegeId);
    invalidPrivileges.Add(privilege.PrivilegeId);
    continue;
}
```

**Proposed Fix**:
```csharp
// Validate privilege exists (use ExistsAsync for efficiency)
if (!await _privilegeRepository.ExistsAsync(privilege.PrivilegeId))
{
    _logger.LogWarning("Privilege {PrivilegeId} not found, skipping", privilege.PrivilegeId);
    invalidPrivileges.Add(privilege.PrivilegeId);
    continue;
}
```

**Verification**:
- ✅ Repository method exists: `ExistsAsync(Guid id)` in IRepositoryBase<Privilege>
- ✅ No audit properties involved (read-only check)
- ✅ Logic equivalent: Both check privilege exists
- ✅ No side effects
- ✅ In a loop - performance impact significant

**Risk**: 🟢 **LOW** - Simple read operation, no functional change

---

### **FIX #7: RemovePrivilegeFromPlanAsync - Plan Existence Check**

**Location**: Line 708-713  
**Type**: Medium - Optimization

**Current Code**:
```csharp
// Check if plan exists
var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
if (plan == null)
{
    await _unitOfWork.RollbackTransactionAsync();
    return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
}
```

**Proposed Fix**:
```csharp
// Check if plan exists (lightweight query - we need the plan object for auto-pricing later)
var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
if (plan == null)
{
    await _unitOfWork.RollbackTransactionAsync();
    return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
}
```

**Verification**:
- ✅ Repository method exists
- ✅ Plan object needed later (Line 735: `if (plan.IsAutoCalculatedPrice)`)
- ✅ Soft delete audit properties maintained (Line 726-730)
- ✅ No audit property regression

**Context Check**: Soft delete sets audit properties correctly (Line 726-730):
```csharp
planPrivilege.IsDeleted = true;
planPrivilege.DeletedBy = tokenModel.UserID;
planPrivilege.DeletedDate = DateTime.UtcNow;
planPrivilege.UpdatedBy = tokenModel.UserID;
planPrivilege.UpdatedDate = DateTime.UtcNow;
```

**Risk**: 🟢 **LOW** - Simple optimization, audit properties maintained

---

### **FIX #8: UpdatePlanPrivilegeAsync - Plan Existence Check**

**Location**: Line 784-789  
**Type**: Medium - Optimization

**Current Code**:
```csharp
// Check if plan exists
var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
if (plan == null)
{
    await _unitOfWork.RollbackTransactionAsync();
    return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
}
```

**Proposed Fix**:
```csharp
// Check if plan exists (lightweight query - we need the plan object for auto-pricing later)
var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
if (plan == null)
{
    await _unitOfWork.RollbackTransactionAsync();
    return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
}
```

**Verification**:
- ✅ Repository method exists
- ✅ Plan object needed later (Line 815: `if (plan.IsAutoCalculatedPrice)`)
- ✅ Update audit properties maintained (Line 809-810)
- ✅ No audit property regression

**Context Check**: Update sets audit properties correctly (Line 809-810):
```csharp
planPrivilege.UpdatedBy = tokenModel.UserID;
planPrivilege.UpdatedDate = DateTime.UtcNow;
```

**Risk**: 🟢 **LOW** - Simple optimization, audit properties maintained

---

### **FIX #9: GetPlanPrivilegesAsync - Plan Existence Check**

**Location**: Line 853-855  
**Type**: Medium - Optimization

**Current Code**:
```csharp
// Check if plan exists
var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
if (plan == null)
    return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
```

**Proposed Fix**:
```csharp
// Check if plan exists (existence check only - we don't use the plan object)
if (!await _subscriptionPlanRepository.ExistsAsync(planId))
    return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
```

**Verification**:
- ✅ Repository method exists: `ExistsAsync(Guid id)` in ISubscriptionPlanRepository
- ✅ No audit properties involved (read-only operation)
- ✅ Plan object NOT used after check (only privileges are retrieved)
- ✅ Logic equivalent
- ✅ No side effects

**Risk**: 🟢 **LOW** - Simple read operation

---

### **FIX #10: CreatePlanAsync - Privilege Existence Check**

**Location**: Line 342-348  
**Type**: Medium - Optimization (in loop!)

**Current Code**:
```csharp
// Validate privilege exists
var privilegeEntity = await _privilegeRepository.GetByIdAsync(privilege.PrivilegeId);
if (privilegeEntity == null)
{
    _logger.LogWarning("Privilege {PrivilegeId} not found, skipping privilege assignment", privilege.PrivilegeId);
    invalidPrivileges.Add(privilege.PrivilegeId);
    continue;
}
```

**Proposed Fix**:
```csharp
// Validate privilege exists (use ExistsAsync for efficiency)
if (!await _privilegeRepository.ExistsAsync(privilege.PrivilegeId))
{
    _logger.LogWarning("Privilege {PrivilegeId} not found, skipping privilege assignment", privilege.PrivilegeId);
    invalidPrivileges.Add(privilege.PrivilegeId);
    continue;
}
```

**Verification**:
- ✅ Repository method exists: `ExistsAsync(Guid id)` in IRepositoryBase<Privilege>
- ✅ No audit properties involved (read-only check)
- ✅ Logic equivalent
- ✅ No side effects
- ✅ In a loop - performance impact significant

**Context Check**: Privilege creation still sets audit properties correctly (Line 366-368):
```csharp
IsActive = true,
CreatedBy = tokenModel.UserID,
CreatedDate = DateTime.UtcNow
```

**Risk**: 🟢 **LOW** - Simple read operation, audit properties maintained

---

## ✅ **FINAL VERIFICATION SUMMARY**

### **Audit Properties Status**:

| Fix # | Affects Audit Properties? | Status |
|-------|-------------------------|--------|
| Fix #1 | ❌ No (read-only) | ✅ Safe |
| Fix #2 | ❌ No (read-only) | ✅ Safe |
| Fix #3 | ❌ No (read-only) | ✅ Safe |
| Fix #4 | ✅ Yes (UpdatedBy, UpdatedDate) | ✅ Maintained |
| Fix #5 | ✅ Yes (CreatedBy, UpdatedBy) | ✅ Maintained |
| Fix #6 | ❌ No (read-only) | ✅ Safe |
| Fix #7 | ✅ Yes (DeletedBy, IsDeleted, UpdatedBy) | ✅ Maintained |
| Fix #8 | ✅ Yes (UpdatedBy, UpdatedDate) | ✅ Maintained |
| Fix #9 | ❌ No (read-only) | ✅ Safe |
| Fix #10 | ✅ Yes (CreatedBy, CreatedDate) | ✅ Maintained |

### **Soft Delete Verification**:

**Only one location uses soft delete**: `RemovePrivilegeFromPlanAsync` (Line 726-730)

**Current Implementation**:
```csharp
// Soft delete - set audit properties
planPrivilege.IsDeleted = true;
planPrivilege.DeletedBy = tokenModel.UserID;
planPrivilege.DeletedDate = DateTime.UtcNow;
planPrivilege.UpdatedBy = tokenModel.UserID;
planPrivilege.UpdatedDate = DateTime.UtcNow;
```

✅ **Correct**: Soft delete properly implemented with all required audit fields

**Note**: Plan deactivation (Line 1167-1169) is NOT soft delete - it's status change:
```csharp
existingPlan.IsActive = false;  // Status change, not deletion
```

✅ **Correct**: Deactivation != Deletion

---

## 🎯 **IMPLEMENTATION STRATEGY**

### **Order of Implementation** (Safest First):

1. **Batch 1: Pure Read Operations** (No audit property concerns)
   - Fix #1: Name uniqueness check
   - Fix #2: Active subscriptions check (Deactivate)
   - Fix #3: Active subscriptions check (Delete)
   - Fix #6: Privilege existence check (Assign)
   - Fix #9: Plan existence check (Get privileges)
   - Fix #10: Privilege existence check (Create)

2. **Batch 2: Read Operations with Subsequent Updates** (Audit properties maintained)
   - Fix #4: ActivatePlanAsync
   - Fix #5: AssignPrivilegesToPlanAsync
   - Fix #7: RemovePrivilegeFromPlanAsync (soft delete maintained)
   - Fix #8: UpdatePlanPrivilegeAsync

---

## ✅ **PRE-IMPLEMENTATION APPROVAL**

**All fixes verified**: ✅  
**Audit properties maintained**: ✅  
**Soft delete correct**: ✅  
**No functional regressions**: ✅  
**Performance improvements**: ✅  
**Risk level**: 🟢 **LOW**

**Ready to implement**: ✅ **YES**

---

**Verification Complete**: October 21, 2025  
**Reviewer**: AI Analysis  
**Status**: ✅ **APPROVED FOR IMPLEMENTATION**


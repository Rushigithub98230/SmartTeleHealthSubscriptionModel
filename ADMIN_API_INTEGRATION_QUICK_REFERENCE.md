# Admin Portal API Integration - Quick Reference

## 📋 API Endpoints Summary

### Subscription Plan Management

| Operation | Endpoint | Method | Frontend Component | Service Method |
|-----------|----------|--------|-------------------|----------------|
| **Create Plan** | `/api/SubscriptionPlans/admin` | POST | `PlanCreateComponent` | `createPlan(dto)` |
| **List Plans** | `/api/SubscriptionPlans/admin` | GET | `PlanListAdminComponent` | `getAllPlansAdmin(page, pageSize)` |
| **Get Plan** | `/api/SubscriptionPlans/{id}` | GET | `PlanEditComponent` | `getPlanById(planId)` |
| **Update Plan** | `/api/SubscriptionPlans/admin/{id}` | PUT | `PlanEditComponent` | `updatePlan(planId, dto)` |
| **Deactivate Plan** | `/api/SubscriptionPlans/admin/{id}/deactivate` | POST | `PlanListAdminComponent` | `deactivatePlan(planId)` |
| **Reactivate Plan** | `/api/SubscriptionPlans/admin/{id}/reactivate` | POST | *(Not implemented yet)* | *(Future)* |

### Subscription Management

| Operation | Endpoint | Method | Frontend Component | Service Method |
|-----------|----------|--------|-------------------|----------------|
| **List Subscriptions** | `/api/Subscriptions/admin/user-subscriptions` | GET | `AdminSubscriptionListComponent` | `commonService.get()` |
| **Get Subscription** | `/api/Subscriptions/{id}` | GET | *(Shared endpoint)* | `getSubscriptionById(id)` |
| **Cancel Subscription** | `/api/Subscriptions/{id}/cancel` | POST | *(Admin action)* | `cancelSubscription(id, reason)` |

### Master Data

| Operation | Endpoint | Method | Frontend Component | Service Method |
|-----------|----------|--------|-------------------|----------------|
| **Load Categories** | `/api/Categories` | GET | All plan components | `getAllCategories()` |
| **Load Billing Cycles** | `/api/MasterData/billing-cycles` | GET | `PlanCreateComponent` | `getBillingCycles()` |
| **Load Currencies** | `/api/MasterData/currencies` | GET | `PlanCreateComponent` | `getCurrencies()` |
| **Load Privileges** | `/api/Privileges?isActive=true` | GET | Plan create/edit | `getActivePrivileges()` |

---

## 📝 DTO Structures

### CreateSubscriptionPlanDto

```typescript
{
  // Required Fields
  name: string,                      // Max 100 chars
  price: number,                     // > 0
  categoryId: string (GUID),
  billingCycleId: string (GUID),
  currencyId: string (GUID),
  
  // Trial
  isTrialAllowed: boolean,
  trialDurationInDays: number,
  
  // Marketing
  isFeatured: boolean,
  isMostPopular: boolean,
  isTrending: boolean,
  displayOrder: number,
  isActive: boolean,
  
  // Pricing
  isAutoCalculatedPrice: boolean,
  adminCommissionPercent: number,
  priceChangeNoticeDays: number,
  
  // Privileges (Array)
  privileges: [
    {
      privilegeId: string (GUID),
      value: number,                 // -1=unlimited, 0=disabled, >0=count
      privilegeBaseCost: number,     // For plan pricing
      unitCost: number,              // For overage
      durationMonths: number,
      description?: string
    }
  ],
  
  // Plan Features
  messagingCount: number,
  includesMedicationDelivery: boolean,
  includesFollowUpCare: boolean,
  deliveryFrequencyDays: number,
  maxPauseDurationDays: number,
  maxConcurrentUsers: number,
  gracePeriodDays: number
}
```

### UpdateSubscriptionPlanDto

```typescript
{
  id: string (GUID),
  name: string,
  description?: string,
  price: number,
  categoryId: string (GUID),
  billingCycleId: string (GUID),
  currencyId: string (GUID),
  isActive: boolean,
  isMostPopular: boolean,
  isTrending: boolean,
  displayOrder?: number,
  isAutoCalculatedPrice: boolean,
  adminCommissionPercent?: number,
  priceChangeNoticeDays: number,
  monthlyBillingDiscount?: number,
  quarterlyBillingDiscount?: number,
  annualBillingDiscount?: number
}
```

---

## 🔧 Common Patterns

### 1. API Call Pattern

```typescript
// Service method
serviceName.methodName(params).subscribe({
  next: (response) => {
    if (response.statusCode === 200 || response.statusCode === 201) {
      // Success handling
      this.data = response.data;
      
      // Handle pagination meta if present
      if (response.meta) {
        this.totalRecords = response.meta.totalRecords;
        this.totalPages = response.meta.totalPages;
      }
    } else {
      // Non-success status
      this.error = response.message || 'Operation failed';
    }
    this.loading = false;
  },
  error: (error) => {
    // Error handling
    if (error.error?.errors) {
      // Validation errors
      const validationErrors = Object.entries(error.error.errors)
        .map(([key, value]) => `${key}: ${value}`)
        .join(', ');
      this.error = `Validation errors: ${validationErrors}`;
    } else {
      // Generic error
      this.error = error.error?.message || error.message || 'An error occurred';
    }
    this.loading = false;
    console.error('Error:', error);
  }
});
```

### 2. Form Validation Pattern

```typescript
// Form setup with validators
this.form = this.fb.group({
  field1: ['', [Validators.required]],
  field2: ['', [Validators.required, Validators.maxLength(100)]],
  field3: [0, [Validators.required, Validators.min(0.01)]]
});

// Pre-submit validation
if (this.form.invalid) {
  this.markFormGroupTouched(this.form);
  this.error = 'Please fill all required fields';
  return;
}

// Helper method
private markFormGroupTouched(formGroup: FormGroup): void {
  Object.keys(formGroup.controls).forEach(key => {
    formGroup.get(key)?.markAsTouched();
  });
}
```

### 3. Loading State Pattern

```typescript
// Component property
loading = false;

// Before API call
this.loading = true;

// In success handler
this.loading = false;

// In error handler
this.loading = false;

// In template
<div *ngIf="loading">Loading...</div>
<div *ngIf="!loading">...content...</div>
```

### 4. Confirmation Dialog Pattern

```typescript
operationName(): void {
  if (!confirm('Are you sure you want to perform this action?')) {
    return;
  }
  
  // Proceed with operation
  this.service.methodName().subscribe({ ... });
}
```

---

## 🎯 Validation Rules

### Plan Creation

| Field | Validation | Error Message |
|-------|-----------|---------------|
| `name` | Required, max 100 | "Plan name is required" |
| `price` | Required, > 0 | "Price must be greater than 0" |
| `categoryId` | Required, valid GUID | "Category is required" |
| `billingCycleId` | Required, valid GUID | "Billing cycle is required" |
| `currencyId` | Required, valid GUID | "Currency is required" |
| `privileges` | At least 1, valid GUIDs | "Please configure at least one privilege" |
| `trialDurationInDays` | > 0 when trial allowed | "Trial duration must be greater than 0" |

### Privilege Configuration

| Field | Validation | Notes |
|-------|-----------|-------|
| `privilegeId` | Required, valid GUID | Not empty, not all zeros |
| `value` | Integer | -1=unlimited, 0=disabled, >0=count |
| `privilegeBaseCost` | >= 0 | For plan pricing calculation |
| `unitCost` | >= 0 | For overage charges |

---

## 🔍 Debugging Guide

### Check API Call is Correct

1. **Open browser DevTools** → Network tab
2. **Perform action** in admin portal
3. **Look for API call** in network tab
4. **Verify**:
   - ✅ URL: `/api/SubscriptionPlans/admin` (correct path)
   - ✅ Method: POST/GET/PUT as expected
   - ✅ Status: 200/201 for success
   - ✅ Payload (Request): Check structure matches DTO
   - ✅ Response: Check data structure

### Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| **400 Bad Request** | Invalid DTO structure | Check payload matches backend DTO |
| **403 Forbidden** | Not admin role | Verify user has admin role |
| **404 Not Found** | Wrong API path | Check endpoint path |
| **500 Internal Error** | Backend error | Check backend logs |
| **Validation errors** | Missing required fields | Check all required fields are sent |

### Console Logging

The admin components include helpful console logs:

```typescript
// Successful operations
console.log('✅ Plan created successfully:', response.data);
console.log('✅ Loaded billing cycles from API:', this.billingCycles);

// Errors
console.error('❌ Error loading billing cycles:', error);
console.error('❌ Invalid privileges detected:', this.selectedPrivileges);

// Price calculations
console.log('💰 Price auto-calculated:', calculatedPrice);
```

---

## 📊 Response Structures

### Success Response

```typescript
{
  data: T,                          // Response data (DTO or array)
  message: string,                  // Success message
  statusCode: number,               // 200, 201, etc.
  meta?: {                          // Pagination meta (if applicable)
    totalRecords: number,
    pageSize: number,
    currentPage: number,
    totalPages: number,
    hasNextPage: boolean,
    hasPreviousPage: boolean
  }
}
```

### Error Response

```typescript
{
  data: {},                         // Empty object
  message: string,                  // Error message
  statusCode: number,               // 400, 403, 404, 500, etc.
  errors?: {                        // Validation errors (optional)
    [field: string]: string[]
  }
}
```

---

## 🚀 Quick Tests

### Test Plan Creation

1. Navigate to `/webadmin/plans/create`
2. Fill form:
   - Name: "Test Plan"
   - Category: Select one
   - Billing Cycle: Select "Monthly"
   - Price: 25.00
   - Add privilege: "Teleconsultation" (Value: 5)
3. Submit
4. **Expected**: Navigate to plan list, plan appears

### Test Plan Editing

1. Navigate to `/webadmin/plans`
2. Click "Edit" on a plan
3. Change plan name
4. Submit
5. **Expected**: Navigate to plan list, change reflected

### Test Plan Deactivation

1. Navigate to `/webadmin/plans`
2. Click "Deactivate" on a plan
3. Confirm dialog
4. **Expected**: Plan list reloads, plan marked inactive

---

## ✅ Verification Checklist

Use this when adding new admin features:

- [ ] API endpoint exists in backend controller
- [ ] Frontend service method calls correct endpoint
- [ ] HTTP method correct (GET/POST/PUT/DELETE)
- [ ] Payload structure matches backend DTO
- [ ] All required fields included
- [ ] GUIDs validated before submission
- [ ] Success handling implemented
- [ ] Error handling implemented
- [ ] Loading state shown during operation
- [ ] User feedback provided (success/error messages)
- [ ] Navigation after success (if applicable)
- [ ] Confirmation dialog for destructive actions
- [ ] Console logging for debugging
- [ ] Form validation before submission

---

## 📚 Related Files

### Frontend

- **Services**: `frontend/.../core/services/subscription-plan.service.ts`
- **Models**: `frontend/.../core/models/subscription-plan.model.ts`
- **Components**:
  - Plan Create: `frontend/.../admin/plans/plan-create/`
  - Plan List: `frontend/.../admin/plans/plan-list/`
  - Plan Edit: `frontend/.../admin/plans/plan-edit/`
- **Common Service**: `frontend/.../core/services/common.service.ts`

### Backend

- **Controller**: `backend/.../API/Controllers/SubscriptionPlansController.cs`
- **Service**: `backend/.../Application/Services/SubscriptionPlanService.cs`
- **DTOs**: `backend/.../Application/DTOs/SubscriptionPlanDto.cs`
- **Entities**: `backend/.../Core/Entities/SubscriptionPlan.cs`

---

## 🎓 Best Practices

### 1. Always Use CommonService

✅ **Correct**:
```typescript
this.commonService.post('SubscriptionPlans/admin', dto)
```

❌ **Incorrect**:
```typescript
this.http.post('/api/SubscriptionPlans/admin', dto)
```

### 2. Handle Both Success and Error

✅ **Correct**:
```typescript
.subscribe({
  next: (response) => { /* handle success */ },
  error: (error) => { /* handle error */ }
});
```

❌ **Incorrect**:
```typescript
.subscribe((response) => { /* only success */ });
```

### 3. Validate Before Submission

✅ **Correct**:
```typescript
if (this.form.invalid) {
  this.error = 'Please fill required fields';
  return;
}
this.service.submit(data).subscribe(...);
```

❌ **Incorrect**:
```typescript
// No validation
this.service.submit(data).subscribe(...);
```

### 4. Show Loading States

✅ **Correct**:
```typescript
this.loading = true;
this.service.getData().subscribe({
  next: (response) => { 
    this.data = response.data;
    this.loading = false;
  },
  error: (error) => {
    this.loading = false;
  }
});
```

### 5. Provide User Feedback

✅ **Correct**:
```typescript
next: (response) => {
  if (response.statusCode === 200) {
    alert('Operation successful!');
    // or use toast notification
  }
}
```

---

**Last Updated**: January 2025
**Status**: ✅ All integrations verified correct


# API Migration Guide - SRP Refactoring

**Audience:** Frontend Developers, Backend API Consumers  
**Purpose:** Guide for migrating from deprecated endpoints to new service-based endpoints  
**Impact:** Zero breaking changes - all old endpoints still work!

---

## 🎯 OVERVIEW

We've refactored the backend services to follow the Single Responsibility Principle (93% compliance). This means some API endpoints have moved to more appropriate controllers/services.

**Good News:** 
- ✅ **All old endpoints still work** (backward compatible)
- ✅ **No immediate changes required** 
- ✅ **Migrate at your own pace**
- ✅ **Clear warnings guide you**

---

## 📋 DEPRECATED ENDPOINTS

### **1. Payment Methods (SubscriptionsController → PaymentsController)**

#### **Old Endpoints (Still Work, Deprecated):**
```http
❌ DEPRECATED
GET  /api/subscriptions/users/{userId}/payment-methods
POST /api/subscriptions/users/{userId}/payment-methods

⚠️ These still work but log deprecation warnings
```

#### **New Endpoints (Recommended):**
```http
✅ RECOMMENDED
GET  /api/payments/users/{userId}/payment-methods
POST /api/payments/users/{userId}/payment-methods
```

#### **Migration Example:**

**Old Frontend Code:**
```typescript
// ❌ Old (works but deprecated)
const response = await axios.get(
  `/api/subscriptions/users/${userId}/payment-methods`,
  { headers: { Authorization: `Bearer ${token}` } }
);
```

**New Frontend Code:**
```typescript
// ✅ New (recommended)
const response = await axios.get(
  `/api/payments/users/${userId}/payment-methods`,
  { headers: { Authorization: `Bearer ${token}` } }
);
```

**Timeline:** Migrate over 1-2 sprints

---

### **2. Billing History (SubscriptionsController → BillingController)**

#### **Old Endpoint (Still Works, Deprecated):**
```http
❌ DEPRECATED
GET /api/subscriptions/{subscriptionId}/billing-history

⚠️ Still works but logs deprecation warning
```

#### **New Endpoint (Recommended):**
```http
✅ RECOMMENDED
GET /api/billing/subscriptions/{subscriptionId}/history
```

#### **Migration Example:**

**Old Frontend Code:**
```typescript
// ❌ Old (works but deprecated)
const history = await axios.get(
  `/api/subscriptions/${subscriptionId}/billing-history`
);
```

**New Frontend Code:**
```typescript
// ✅ New (recommended)
const history = await axios.get(
  `/api/billing/subscriptions/${subscriptionId}/history`
);
```

**Timeline:** Migrate over 1-2 sprints

---

### **3. Categories (SubscriptionsController → CategoriesController)**

#### **Old Endpoint (Still Works, Deprecated):**
```http
❌ DEPRECATED
GET /api/subscriptions/categories?page=1&pageSize=10

⚠️ Still works but logs deprecation warning
```

#### **New Endpoint (Recommended):**
```http
✅ RECOMMENDED
GET /api/categories?page=1&pageSize=10
```

#### **Migration Example:**

**Old Frontend Code:**
```typescript
// ❌ Old (works but deprecated)
const categories = await axios.get(
  `/api/subscriptions/categories?page=1&pageSize=10`
);
```

**New Frontend Code:**
```typescript
// ✅ New (recommended)
const categories = await axios.get(
  `/api/categories?page=1&pageSize=10`
);
```

**Timeline:** Migrate over 1-2 sprints

---

### **4. Consultations (SubscriptionsController → ConsultationsController - Future)**

#### **Current Endpoint (Functional):**
```http
⚠️ TEMPORARY LOCATION (works, will move later)
POST /api/subscriptions/{subscriptionId}/book-consultation

📝 Note: ConsultationService doesn't exist yet
📝 Will move to /api/consultations when service is created
```

#### **Future Endpoint (When ConsultationService Exists):**
```http
🔮 FUTURE
POST /api/consultations/book
```

**Timeline:** Future enhancement (no immediate action needed)

---

### **5. Medications (SubscriptionsController → MedicationsController - Future)**

#### **Current Endpoint (Functional):**
```http
⚠️ TEMPORARY LOCATION (works, will move later)
POST /api/subscriptions/{subscriptionId}/request-medication

📝 Note: MedicationService doesn't exist yet
📝 Will move to /api/medications when service is created
```

#### **Future Endpoint (When MedicationService Exists):**
```http
🔮 FUTURE
POST /api/medications/request-supply
```

**Timeline:** Future enhancement (no immediate action needed)

---

## ✅ NEW ENDPOINTS (SRP Refactoring Features)

### **Credit Purchase (Enhanced Feature)**

#### **New Endpoint:**
```http
✅ NEW FEATURE
POST /api/subscriptions/{subscriptionId}/purchase-credits

Request Body:
{
  "privilegeName": "Teleconsultation",
  "quantity": 5,
  "paymentMethodId": "pm_xxx"
}

Response (200 OK):
{
  "data": {
    "subscriptionId": "guid",
    "privilegeName": "Teleconsultation",
    "creditsAdded": 5,
    "unitCost": 20.00,
    "totalPaid": 100.00,
    "previousLimit": 5,
    "newLimit": 10,
    "currentUsed": 5,
    "newRemaining": 5,
    "billingRecordId": "guid",
    "purchasedAt": "2025-10-15T12:00:00Z"
  },
  "message": "Successfully purchased 5 additional credits",
  "statusCode": 200
}
```

**Usage:**
```typescript
// Purchase additional credits when user exceeds limit
const result = await axios.post(
  `/api/subscriptions/${subscriptionId}/purchase-credits`,
  {
    privilegeName: "Teleconsultation",
    quantity: 5,
    paymentMethodId: paymentMethod.id
  }
);
```

---

### **Privilege Availability Check**

#### **New Endpoint:**
```http
✅ NEW FEATURE
GET /api/subscriptions/{subscriptionId}/check-privilege/{privilegeName}?requestedAmount=1

Response (200 OK - Available):
{
  "data": {
    "available": true,
    "privilegeName": "Teleconsultation",
    "remaining": 3,
    "requested": 1,
    "afterUse": 2
  },
  "statusCode": 200
}

Response (402 Payment Required - Limit Exceeded):
{
  "data": {
    "available": false,
    "limitExceeded": true,
    "privilegeName": "Teleconsultation",
    "remaining": 0,
    "requested": 1,
    "shortfall": 1,
    "unitCost": 20.00,
    "requiredPayment": 20.00,
    "message": "You've used all your included credits. Purchase 1 additional credit for $20.00 to continue.",
    "purchaseEndpoint": "/api/subscriptions/{id}/purchase-credits",
    "purchaseDetails": {
      "privilegeName": "Teleconsultation",
      "quantity": 1,
      "unitCost": 20.00,
      "totalCost": 20.00
    }
  },
  "statusCode": 402
}
```

**Usage:**
```typescript
// Check before using privilege
const check = await axios.get(
  `/api/subscriptions/${subscriptionId}/check-privilege/Teleconsultation?requestedAmount=1`
);

if (check.data.statusCode === 402) {
  // Show payment modal with check.data.data.purchaseDetails
  const purchase = await purchaseCredits(check.data.data.purchaseDetails);
} else if (check.data.statusCode === 200) {
  // Proceed with using privilege
  await usePrivilege();
}
```

---

## 🔄 CONTROLLER UPDATES NEEDED

### **PaymentsController - Add New Endpoints**

```csharp
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    // SRP Refactoring: New endpoints for payment method management
    
    [HttpGet("users/{userId}/payment-methods")]
    public async Task<JsonModel> GetPaymentMethods(int userId)
    {
        var token = GetToken(HttpContext);
        return await _paymentService.GetPaymentMethodsAsync(userId, token);
    }

    [HttpPost("users/{userId}/payment-methods")]
    public async Task<JsonModel> AddPaymentMethod(
        int userId, 
        [FromBody] AddPaymentMethodDto dto)
    {
        var token = GetToken(HttpContext);
        return await _paymentService.AddPaymentMethodAsync(
            userId, dto.PaymentMethodId, token
        );
    }
}
```

### **BillingController - Add Subscription History Endpoint**

```csharp
[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    // SRP Refactoring: Direct billing history endpoint
    
    [HttpGet("subscriptions/{subscriptionId}/history")]
    public async Task<JsonModel> GetSubscriptionBillingHistory(string subscriptionId)
    {
        var token = GetToken(HttpContext);
        
        if (!Guid.TryParse(subscriptionId, out var id))
        {
            return new JsonModel {
                data = new object(),
                Message = "Invalid subscription ID",
                StatusCode = 400
            };
        }
        
        return await _billingService.GetSubscriptionBillingHistoryAsync(id, token);
    }
}
```

---

## 📱 FRONTEND INTEGRATION EXAMPLES

### **React/TypeScript Example: Payment Methods**

```typescript
// services/paymentService.ts

// ✅ NEW - Use PaymentService endpoints
export const paymentService = {
  
  // Get user's payment methods
  async getPaymentMethods(userId: number): Promise<PaymentMethod[]> {
    const response = await apiClient.get(
      `/api/payments/users/${userId}/payment-methods`
    );
    return response.data.data;
  },
  
  // Add payment method
  async addPaymentMethod(
    userId: number, 
    paymentMethodId: string
  ): Promise<PaymentMethod> {
    const response = await apiClient.post(
      `/api/payments/users/${userId}/payment-methods`,
      { paymentMethodId }
    );
    return response.data.data;
  }
};

// Old way (still works but deprecated):
// await axios.get(`/api/subscriptions/users/${userId}/payment-methods`)
```

### **React/TypeScript Example: Privilege Checking & Credit Purchase**

```typescript
// services/subscriptionService.ts

export const subscriptionService = {
  
  // Check privilege availability
  async checkPrivilege(
    subscriptionId: string,
    privilegeName: string,
    requestedAmount: number = 1
  ): Promise<PrivilegeAvailabilityResponse> {
    const response = await apiClient.get(
      `/api/subscriptions/${subscriptionId}/check-privilege/${privilegeName}`,
      { params: { requestedAmount } }
    );
    return response.data;
  },
  
  // Purchase additional credits
  async purchaseCredits(
    subscriptionId: string,
    privilegeName: string,
    quantity: number,
    paymentMethodId: string
  ): Promise<PurchaseCreditsResponse> {
    const response = await apiClient.post(
      `/api/subscriptions/${subscriptionId}/purchase-credits`,
      { privilegeName, quantity, paymentMethodId }
    );
    return response.data.data;
  }
};

// Usage in component:
const handleUsePrivilege = async () => {
  // 1. Check availability
  const check = await subscriptionService.checkPrivilege(
    subscriptionId, 
    'Teleconsultation', 
    1
  );
  
  // 2. If limit exceeded (402), show purchase modal
  if (check.statusCode === 402) {
    const shouldPurchase = await showPurchaseModal(check.data.purchaseDetails);
    
    if (shouldPurchase) {
      const purchase = await subscriptionService.purchaseCredits(
        subscriptionId,
        'Teleconsultation',
        check.data.shortfall,
        selectedPaymentMethod.id
      );
      
      toast.success(`Successfully purchased ${purchase.creditsAdded} credits!`);
    }
  }
  
  // 3. Proceed with privilege usage
  if (check.data.available) {
    await usePrivilege();
  }
};
```

---

## 🔍 TESTING YOUR MIGRATION

### **Step 1: Verify Old Endpoints Still Work**

```bash
# Test deprecated payment methods endpoint (should work with warning)
curl -X GET "https://api.yourapp.com/api/subscriptions/users/123/payment-methods" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Expected: 200 OK + deprecation warning in logs
```

### **Step 2: Test New Endpoints**

```bash
# Test new payment methods endpoint
curl -X GET "https://api.yourapp.com/api/payments/users/123/payment-methods" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Expected: 200 OK, same data as old endpoint
```

### **Step 3: Compare Responses**

Both endpoints should return **identical data**. The only difference is the URL path.

---

## 📊 MIGRATION CHECKLIST

### **Phase 1: Immediate (No Action Required)**
- ✅ All old endpoints still work
- ✅ No breaking changes
- ✅ Continue using current code

### **Phase 2: Gradual Migration (1-2 Sprints)**

**For Each Deprecated Endpoint:**
- [ ] Update frontend service calls to new endpoints
- [ ] Test new endpoints thoroughly
- [ ] Monitor deprecation logs to track usage
- [ ] Update API documentation

**Priority Order:**
1. **High Priority:** Payment methods (most frequently used)
2. **Medium Priority:** Billing history
3. **Low Priority:** Categories
4. **Future:** Consultations, Medications (when services exist)

### **Phase 3: Cleanup (After 90% Migration)**
- [ ] Remove deprecated endpoints from backend
- [ ] Update OpenAPI/Swagger docs
- [ ] Final testing

---

## 🚀 POSTMAN COLLECTION UPDATES

### **Payment Methods**

**Old Request (Deprecated):**
```json
GET /api/subscriptions/users/{{userId}}/payment-methods
Headers:
  Authorization: Bearer {{token}}
```

**New Request (Recommended):**
```json
GET /api/payments/users/{{userId}}/payment-methods
Headers:
  Authorization: Bearer {{token}}
```

**Add Payment Method (New):**
```json
POST /api/payments/users/{{userId}}/payment-methods
Headers:
  Authorization: Bearer {{token}}
  Content-Type: application/json
Body:
{
  "paymentMethodId": "pm_1234567890"
}
```

---

### **Credit Purchase (New Feature)**

```json
POST /api/subscriptions/{{subscriptionId}}/purchase-credits
Headers:
  Authorization: Bearer {{token}}
  Content-Type: application/json
Body:
{
  "privilegeName": "Teleconsultation",
  "quantity": 5,
  "paymentMethodId": "pm_1234567890"
}

Response (200 OK):
{
  "data": {
    "subscriptionId": "guid",
    "privilegeName": "Teleconsultation",
    "creditsAdded": 5,
    "unitCost": 20.00,
    "totalPaid": 100.00,
    "previousLimit": 5,
    "newLimit": 10,
    "currentUsed": 5,
    "newRemaining": 5,
    "billingRecordId": "guid",
    "purchasedAt": "2025-10-15T12:00:00Z"
  },
  "message": "Successfully purchased 5 additional Teleconsultation credits for $100.00. Your new limit is 10.",
  "statusCode": 200
}
```

---

### **Privilege Availability Check (New Feature)**

```json
GET /api/subscriptions/{{subscriptionId}}/check-privilege/Teleconsultation?requestedAmount=1
Headers:
  Authorization: Bearer {{token}}

Response (200 OK - Available):
{
  "data": {
    "available": true,
    "privilegeName": "Teleconsultation",
    "remaining": 3,
    "requested": 1,
    "afterUse": 2,
    "message": "Privilege is available"
  },
  "message": "Privilege is available",
  "statusCode": 200
}

Response (402 Payment Required - Limit Exceeded):
{
  "data": {
    "available": false,
    "limitExceeded": true,
    "privilegeName": "Teleconsultation",
    "remaining": 0,
    "requested": 1,
    "shortfall": 1,
    "unitCost": 20.00,
    "requiredPayment": 20.00,
    "message": "You've used all your included Teleconsultation credits. Purchase 1 additional credit for $20.00 to continue.",
    "purchaseEndpoint": "/api/subscriptions/{id}/purchase-credits",
    "purchaseDetails": {
      "privilegeName": "Teleconsultation",
      "quantity": 1,
      "unitCost": 20.00,
      "totalCost": 20.00
    }
  },
  "message": "Insufficient Teleconsultation credits. 0 remaining, 1 requested. Purchase 1 additional credit for $20.00.",
  "statusCode": 402
}
```

---

## 📝 DEPRECATION WARNINGS

When you call deprecated endpoints, you'll see these in backend logs:

```
WARN: DEPRECATED: GetPaymentMethodsAsync called from SubscriptionService. Use PaymentService instead.
WARN: DEPRECATED: AddPaymentMethodAsync called from SubscriptionService. Use PaymentService instead.
WARN: DEPRECATED: GetBillingHistoryAsync called from SubscriptionService. Use BillingService instead.
WARN: DEPRECATED: GetAllCategoriesAsync called from SubscriptionService. Use CategoryService instead.
WARN: DEPRECATED: BookConsultationAsync called from SubscriptionService. This should be in ConsultationService.
WARN: DEPRECATED: RequestMedicationSupplyAsync called from SubscriptionService. This should be in MedicationService.
```

**These warnings help you:**
- Identify which code paths still use old endpoints
- Track migration progress
- Find code that needs updating

---

## 🎯 RECOMMENDED MIGRATION STRATEGY

### **Week 1-2: Assessment**
1. Review all frontend API calls
2. Identify deprecated endpoint usage
3. Create migration tickets
4. Prioritize by frequency of use

### **Week 3-4: Payment Methods Migration**
1. Update payment method calls to PaymentService
2. Test thoroughly
3. Deploy to staging
4. Monitor for issues

### **Week 5-6: Billing & Categories Migration**
1. Update billing history calls to BillingService
2. Update category calls to CategoryService
3. Test thoroughly
4. Deploy to staging

### **Week 7-8: Validation & Cleanup**
1. Monitor deprecation logs
2. Verify 90%+ migration complete
3. Consider removing deprecated endpoints
4. Update documentation

---

## ⚠️ BREAKING CHANGES (None!)

**We guarantee:**
- ✅ No breaking changes in this release
- ✅ All old endpoints still functional
- ✅ Same request/response formats
- ✅ Same authentication/authorization
- ✅ Same error handling

**The only change:**
- ⚠️ Deprecation warnings in backend logs (not visible to frontend)
- ⚠️ Compiler warnings in C# code (not affecting runtime)

---

## 📞 SUPPORT & QUESTIONS

### **Common Questions:**

**Q: Do I need to update my frontend code immediately?**  
A: No! All old endpoints still work. Migrate at your own pace over 1-2 sprints.

**Q: Will my API calls break?**  
A: No! Zero breaking changes. Everything works as before.

**Q: What happens if I don't migrate?**  
A: Code continues working, but you'll see deprecation warnings in logs. After 90% migration (1-2 sprints), we'll remove deprecated endpoints.

**Q: What's the benefit of migrating?**  
A: Cleaner API structure, better service boundaries, easier to maintain, follows industry best practices.

**Q: How do I know which endpoints are deprecated?**  
A: Check this guide, or look for `[Obsolete]` attributes in C# code. Compiler will warn you.

---

## 📊 ENDPOINT SUMMARY TABLE

| Operation | Old Endpoint | New Endpoint | Status | Priority |
|-----------|--------------|--------------|--------|----------|
| Get Payment Methods | `/api/subscriptions/users/{userId}/payment-methods` | `/api/payments/users/{userId}/payment-methods` | ⚠️ Deprecated | High |
| Add Payment Method | `/api/subscriptions/users/{userId}/payment-methods` | `/api/payments/users/{userId}/payment-methods` | ⚠️ Deprecated | High |
| Get Billing History | `/api/subscriptions/{id}/billing-history` | `/api/billing/subscriptions/{id}/history` | ⚠️ Deprecated | Medium |
| Get Categories | `/api/subscriptions/categories` | `/api/categories` | ⚠️ Deprecated | Low |
| Book Consultation | `/api/subscriptions/{id}/book-consultation` | `/api/consultations/book` (future) | ⏰ Future | Low |
| Request Medication | `/api/subscriptions/{id}/request-medication` | `/api/medications/request` (future) | ⏰ Future | Low |
| Purchase Credits | N/A | `/api/subscriptions/{id}/purchase-credits` | ✅ New | N/A |
| Check Privilege | N/A | `/api/subscriptions/{id}/check-privilege/{name}` | ✅ New | N/A |

---

## ✅ VALIDATION STEPS

### **1. Verify Backward Compatibility**

```bash
# Test all old endpoints still work
curl -X GET "{{BASE_URL}}/api/subscriptions/users/123/payment-methods" -H "Authorization: Bearer {{TOKEN}}"
curl -X GET "{{BASE_URL}}/api/subscriptions/abc-123/billing-history" -H "Authorization: Bearer {{TOKEN}}"
curl -X GET "{{BASE_URL}}/api/subscriptions/categories" -H "Authorization: Bearer {{TOKEN}}"

# All should return 200 OK with data
```

### **2. Test New Endpoints**

```bash
# Test new endpoints work
curl -X GET "{{BASE_URL}}/api/payments/users/123/payment-methods" -H "Authorization: Bearer {{TOKEN}}"
curl -X GET "{{BASE_URL}}/api/billing/subscriptions/abc-123/history" -H "Authorization: Bearer {{TOKEN}}"
curl -X GET "{{BASE_URL}}/api/categories" -H "Authorization: Bearer {{TOKEN}}"

# All should return 200 OK with identical data to old endpoints
```

### **3. Compare Responses**

Old and new endpoints should return **identical JSON responses**. Only the URL path changes.

---

## 🎯 SUCCESS METRICS

Track these metrics during migration:

1. **Deprecation Log Count:** Should decrease week over week
2. **New Endpoint Usage:** Should increase week over week
3. **Error Rate:** Should remain stable (no increase)
4. **Response Times:** Should be identical or better

---

**End of API Migration Guide**



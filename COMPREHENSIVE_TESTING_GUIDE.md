# 🧪 **COMPREHENSIVE SUBSCRIPTION PLAN TESTING GUIDE**

## 🎯 **TESTING ENDPOINT**
```
POST http://localhost:51269/api/SubscriptionPlans/admin
```

---

## 📋 **TEST PAYLOADS OVERVIEW**

### **1. Basic Payload** - Minimal Features
- **Purpose**: Test basic validation and minimal privilege setup
- **Privileges**: 1 privilege (5 consultations)
- **Price**: $29.99
- **Features**: Basic consultations only

### **2. Intermediate Payload** - Moderate Features  
- **Purpose**: Test moderate complexity with multiple privileges
- **Privileges**: 3 privileges (consultations, messaging, medication)
- **Price**: $59.99 (with discount)
- **Features**: Standard telehealth features

### **3. Premium Payload** - Advanced Features
- **Purpose**: Test complex privilege configurations
- **Privileges**: 6 privileges (unlimited + limited combinations)
- **Price**: $149.99 (with discount)
- **Features**: Premium features with unlimited access

### **4. Enterprise Payload** - Maximum Complexity
- **Purpose**: Test maximum complexity and edge cases
- **Privileges**: 8 privileges (all unlimited)
- **Price**: $499.99 (with discount)
- **Features**: Enterprise-grade features

---

## 🔍 **VALIDATION TESTING SCENARIOS**

### **✅ Scenario 1: Basic Validation (Basic Payload)**
**Expected Result**: ✅ **SUCCESS (201 Created)**

**What to Verify**:
- Plan created successfully
- 1 privilege assigned correctly
- Stripe product and prices created
- Basic features configured

**Response Validation**:
```json
{
  "data": {
    "id": "generated-guid",
    "name": "Basic Health Plan",
    "price": 29.99,
    "isActive": true,
    "stripeProductId": "prod_xxxxx",
    "privileges": [
      {
        "privilegeId": "44444444-4444-4444-4444-444444444444",
        "value": 5,
        "description": "5 consultations per month"
      }
    ]
  },
  "message": "Plan created successfully with privileges",
  "statusCode": 201
}
```

---

### **✅ Scenario 2: Intermediate Complexity (Intermediate Payload)**
**Expected Result**: ✅ **SUCCESS (201 Created)**

**What to Verify**:
- Plan created with discount configuration
- 3 privileges assigned correctly
- Trial period configured (14 days)
- Time-based limits working

**Response Validation**:
```json
{
  "data": {
    "id": "generated-guid",
    "name": "Standard Telehealth Plan",
    "price": 59.99,
    "discountedPrice": 49.99,
    "isTrialAllowed": true,
    "trialDurationInDays": 14,
    "privileges": [
      {
        "privilegeId": "44444444-4444-4444-4444-444444444444",
        "value": 20,
        "dailyLimit": 2,
        "weeklyLimit": 8,
        "monthlyLimit": 20
      }
    ]
  },
  "statusCode": 201
}
```

---

### **✅ Scenario 3: Premium Complexity (Premium Payload)**
**Expected Result**: ✅ **SUCCESS (201 Created)**

**What to Verify**:
- Complex privilege mix (unlimited + limited)
- Multiple time-based limits
- High-value plan creation
- All 6 privileges assigned

**Response Validation**:
```json
{
  "data": {
    "id": "generated-guid",
    "name": "Premium Telehealth Plan",
    "price": 149.99,
    "maxConcurrentUsers": 5,
    "privileges": [
      {
        "privilegeId": "44444444-4444-4444-4444-444444444444",
        "value": -1,
        "dailyLimit": null,
        "weeklyLimit": null,
        "monthlyLimit": null
      }
    ]
  },
  "statusCode": 201
}
```

---

### **✅ Scenario 4: Enterprise Complexity (Enterprise Payload)**
**Expected Result**: ✅ **SUCCESS (201 Created)**

**What to Verify**:
- Maximum complexity handling
- All unlimited privileges
- Enterprise features
- High concurrent users (100)

**Response Validation**:
```json
{
  "data": {
    "id": "generated-guid",
    "name": "Enterprise Healthcare Solution",
    "price": 499.99,
    "maxConcurrentUsers": 100,
    "messagingCount": 10000,
    "privileges": [
      {
        "privilegeId": "44444444-4444-4444-4444-444444444444",
        "value": -1,
        "description": "Unlimited video consultations for all users"
      }
    ]
  },
  "statusCode": 201
}
```

---

## ❌ **NEGATIVE TESTING SCENARIOS**

### **❌ Scenario 5: Invalid Price**
**Payload Modification**:
```json
{
  "name": "Invalid Price Plan",
  "price": 0,  // ❌ Invalid: must be > 0
  "billingCycleId": "11111111-1111-1111-1111-111111111111",
  "currencyId": "22222222-2222-2222-2222-222222222222",
  "categoryId": "33333333-3333-3333-3333-333333333333"
}
```

**Expected Result**: ❌ **FAILURE (400 Bad Request)**
```json
{
  "data": {},
  "message": "Price must be greater than 0",
  "statusCode": 400
}
```

---

### **❌ Scenario 6: Past Expiration Date**
**Payload Modification**:
```json
{
  "name": "Past Date Plan",
  "price": 99.99,
  "expirationDate": "2023-12-31T23:59:59Z",  // ❌ Invalid: past date
  "privileges": [
    {
      "privilegeId": "44444444-4444-4444-4444-444444444444",
      "value": 10,
      "expirationDate": "2023-12-31T23:59:59Z"  // ❌ Invalid: past date
    }
  ]
}
```

**Expected Result**: ❌ **FAILURE (400 Bad Request)**
```json
{
  "data": {},
  "message": "Expiration date cannot be in the past",
  "statusCode": 400
}
```

---

### **❌ Scenario 7: Empty GUID**
**Payload Modification**:
```json
{
  "name": "Empty GUID Plan",
  "price": 99.99,
  "billingCycleId": "00000000-0000-0000-0000-000000000000",  // ❌ Invalid: empty GUID
  "currencyId": "22222222-2222-2222-2222-222222222222",
  "categoryId": "33333333-3333-3333-3333-333333333333"
}
```

**Expected Result**: ❌ **FAILURE (400 Bad Request)**
```json
{
  "data": {},
  "message": "GUID cannot be empty",
  "statusCode": 400
}
```

---

### **❌ Scenario 8: Invalid String Length**
**Payload Modification**:
```json
{
  "name": "This is a very long subscription plan name that exceeds the maximum allowed length of 100 characters and will cause validation to fail because it is too long for the system to handle properly",  // ❌ Invalid: > 100 chars
  "price": 99.99,
  "billingCycleId": "11111111-1111-1111-1111-111111111111",
  "currencyId": "22222222-2222-2222-2222-222222222222",
  "categoryId": "33333333-3333-3333-3333-333333333333"
}
```

**Expected Result**: ❌ **FAILURE (400 Bad Request)**
```json
{
  "data": {},
  "message": "Name cannot exceed 100 characters",
  "statusCode": 400
}
```

---

### **❌ Scenario 9: Invalid Privilege Value**
**Payload Modification**:
```json
{
  "name": "Invalid Privilege Plan",
  "price": 99.99,
  "privileges": [
    {
      "privilegeId": "44444444-4444-4444-4444-444444444444",
      "value": -5,  // ❌ Invalid: must be -1, 0, or positive
      "usagePeriodId": "11111111-1111-1111-1111-111111111111"
    }
  ]
}
```

**Expected Result**: ❌ **FAILURE (400 Bad Request)**
```json
{
  "data": {},
  "message": "Value must be -1 (unlimited), 0 (disabled), or positive number",
  "statusCode": 400
}
```

---

### **❌ Scenario 10: Invalid Time Limits**
**Payload Modification**:
```json
{
  "name": "Invalid Limits Plan",
  "price": 99.99,
  "privileges": [
    {
      "privilegeId": "44444444-4444-4444-4444-444444444444",
      "value": 10,
      "dailyLimit": -1,  // ❌ Invalid: must be ≥ 0
      "weeklyLimit": -5,  // ❌ Invalid: must be ≥ 0
      "monthlyLimit": 10
    }
  ]
}
```

**Expected Result**: ❌ **FAILURE (400 Bad Request)**
```json
{
  "data": {},
  "message": "Daily limit must be 0 or positive",
  "statusCode": 400
}
```

---

## 🔧 **TESTING CHECKLIST**

### **✅ Positive Testing**
- [ ] Basic payload creates successfully
- [ ] Intermediate payload creates successfully  
- [ ] Premium payload creates successfully
- [ ] Enterprise payload creates successfully
- [ ] All privileges are assigned correctly
- [ ] Stripe products and prices are created
- [ ] Time-based limits are configured
- [ ] Discount configurations work
- [ ] Trial periods are set correctly

### **❌ Negative Testing**
- [ ] Invalid price validation works
- [ ] Past date validation works
- [ ] Empty GUID validation works
- [ ] String length validation works
- [ ] Invalid privilege value validation works
- [ ] Invalid time limits validation works
- [ ] Missing required fields validation works
- [ ] Invalid GUID format validation works

---

## 🎯 **WHAT TO VERIFY FOR EACH PAYLOAD**

### **1. Basic Payload Verification**
- ✅ Plan created with basic features
- ✅ 1 privilege assigned (5 consultations)
- ✅ No trial period
- ✅ Basic pricing ($29.99)
- ✅ Stripe integration working

### **2. Intermediate Payload Verification**
- ✅ Plan created with moderate features
- ✅ 3 privileges assigned correctly
- ✅ Trial period configured (14 days)
- ✅ Discount pricing working
- ✅ Time-based limits configured

### **3. Premium Payload Verification**
- ✅ Plan created with advanced features
- ✅ 6 privileges assigned correctly
- ✅ Mix of unlimited and limited privileges
- ✅ High concurrent users (5)
- ✅ Complex time-based limits

### **4. Enterprise Payload Verification**
- ✅ Plan created with enterprise features
- ✅ 8 privileges assigned correctly
- ✅ All unlimited privileges
- ✅ Maximum concurrent users (100)
- ✅ Enterprise-grade configuration

---

## 🚀 **TESTING EXECUTION ORDER**

### **Phase 1: Positive Testing**
1. **Basic Payload** → Should succeed
2. **Intermediate Payload** → Should succeed
3. **Premium Payload** → Should succeed
4. **Enterprise Payload** → Should succeed

### **Phase 2: Negative Testing**
5. **Invalid Price** → Should fail with validation error
6. **Past Date** → Should fail with date validation error
7. **Empty GUID** → Should fail with GUID validation error
8. **String Length** → Should fail with length validation error
9. **Invalid Privilege** → Should fail with privilege validation error
10. **Invalid Limits** → Should fail with limit validation error

---

## 📊 **EXPECTED RESULTS SUMMARY**

| Test Scenario | Expected Status | Validation Tested |
|---------------|----------------|-------------------|
| Basic Payload | ✅ 201 Created | Basic functionality |
| Intermediate Payload | ✅ 201 Created | Moderate complexity |
| Premium Payload | ✅ 201 Created | High complexity |
| Enterprise Payload | ✅ 201 Created | Maximum complexity |
| Invalid Price | ❌ 400 Bad Request | Price validation |
| Past Date | ❌ 400 Bad Request | Date validation |
| Empty GUID | ❌ 400 Bad Request | GUID validation |
| String Length | ❌ 400 Bad Request | Length validation |
| Invalid Privilege | ❌ 400 Bad Request | Privilege validation |
| Invalid Limits | ❌ 400 Bad Request | Limit validation |

---

## ⚠️ **IMPORTANT NOTES**

1. **Replace GUIDs**: All placeholder GUIDs must be replaced with real ones from your database
2. **Admin Token**: Ensure you have a valid admin authorization token
3. **Database Setup**: Ensure all required master data exists (currencies, billing cycles, categories, privileges)
4. **Stripe Configuration**: Ensure Stripe is properly configured for product/price creation
5. **Cleanup**: Consider cleaning up test data after testing

---

**🎯 This comprehensive testing approach will verify all validation rules, edge cases, and functionality of your subscription plan creation endpoint!**

# 🧪 **SUBSCRIPTION PLAN API TESTING GUIDE**

## 🎯 **TESTING ENDPOINT**
```
POST http://localhost:51269/api/SubscriptionPlans/admin
```

## 📋 **VALIDATION RULES COMPLIANCE**

### ✅ **All Validation Rules Met**

1. **Required Fields** ✅
   - `name`: "Premium Telehealth Plan" (max 100 chars)
   - `price`: 99.99 (greater than 0)
   - `billingCycleId`: Valid GUID
   - `currencyId`: Valid GUID
   - `categoryId`: Valid GUID

2. **String Length Validations** ✅
   - `name`: 25 chars (≤ 100)
   - `description`: 150 chars (≤ 500)
   - `shortDescription`: 35 chars (≤ 200)
   - `features`: 120 chars (≤ 1000)
   - `terms`: 120 chars (≤ 500)

3. **Numeric Range Validations** ✅
   - `price`: 99.99 (≥ 0.01)
   - `trialDurationInDays`: 7 (≥ 0)
   - `messagingCount`: 1000 (≥ 0)
   - `deliveryFrequencyDays`: 30 (≥ 1)
   - `maxPauseDurationDays`: 90 (≥ 0)
   - `maxConcurrentUsers`: 1 (≥ 1)
   - `gracePeriodDays`: 7 (≥ 0)

4. **Date Validations** ✅
   - `discountValidUntil`: 2025-12-31 (future date)
   - `effectiveDate`: 2024-01-15 (future date)
   - `expirationDate`: 2025-12-31 (future date)
   - **All privilege dates are future dates** ✅

5. **GUID Validations** ✅
   - All GUIDs are properly formatted
   - No empty GUIDs (00000000-0000-0000-0000-000000000000)

6. **Privilege Validations** ✅
   - `value`: -1 (unlimited), 1000 (limited), 5 (limited) - all valid
   - `dailyLimit`, `weeklyLimit`, `monthlyLimit`: All ≥ 0 or null
   - `unitCost`: All ≥ 0
   - `expirationDate`: All future dates

---

## 🔧 **BEFORE TESTING - REPLACE GUIDs**

### **⚠️ IMPORTANT: You MUST replace the placeholder GUIDs with real ones from your database!**

```json
// Replace these placeholder GUIDs:
"billingCycleId": "11111111-1111-1111-1111-111111111111",  // ← Replace with real Monthly billing cycle GUID
"currencyId": "22222222-2222-2222-2222-222222222222",      // ← Replace with real USD currency GUID
"categoryId": "33333333-3333-3333-3333-333333333333",      // ← Replace with real category GUID
"privilegeId": "44444444-4444-4444-4444-444444444444",     // ← Replace with real privilege GUIDs
```

### **🔍 How to Get Real GUIDs**

**Option 1: Database Query**
```sql
-- Get all required GUIDs
SELECT 'Currency' as Type, Id, Code as Name FROM MasterCurrencies WHERE IsActive = 1 AND Code = 'USD'
UNION ALL
SELECT 'BillingCycle' as Type, Id, Name FROM MasterBillingCycles WHERE IsActive = 1 AND Name = 'Monthly'
UNION ALL
SELECT 'Category' as Type, Id, Name FROM Categories WHERE IsActive = 1
UNION ALL
SELECT 'Privilege' as Type, Id, Name FROM Privileges WHERE IsActive = 1;
```

**Option 2: API Endpoints (if available)**
```bash
# Get currencies
GET http://localhost:51269/api/MasterData/currencies

# Get billing cycles  
GET http://localhost:51269/api/MasterData/billing-cycles

# Get categories
GET http://localhost:51269/api/Categories

# Get privileges
GET http://localhost:51269/api/SubscriptionPlans/admin/privileges
```

---

## 🚀 **TESTING STEPS**

### **Step 1: Prepare the Payload**
1. Copy the payload from `SUBSCRIPTION_PLAN_TEST_PAYLOAD.json`
2. Replace all placeholder GUIDs with real ones from your database
3. Verify all dates are in the future

### **Step 2: Set Up Postman**
1. **Method**: POST
2. **URL**: `http://localhost:51269/api/SubscriptionPlans/admin`
3. **Headers**:
   ```
   Content-Type: application/json
   Authorization: Bearer YOUR_ADMIN_TOKEN
   ```
4. **Body**: Raw JSON (paste the modified payload)

### **Step 3: Execute the Request**
1. Send the request
2. Check the response status (should be 201 Created)
3. Verify the response contains the created subscription plan

---

## 📊 **EXPECTED RESPONSE**

### **Success Response (201 Created)**
```json
{
  "data": {
    "id": "generated-guid",
    "name": "Premium Telehealth Plan",
    "description": "Comprehensive telehealth subscription plan...",
    "price": 99.99,
    "isActive": true,
    "stripeProductId": "prod_xxxxx",
    "stripeMonthlyPriceId": "price_xxxxx",
    "createdDate": "2024-01-15T10:30:00Z",
    "privileges": [
      {
        "privilegeId": "44444444-4444-4444-4444-444444444444",
        "value": -1,
        "description": "Unlimited video consultations per month"
      }
    ]
  },
  "message": "Plan created successfully with privileges",
  "statusCode": 201
}
```

### **Error Response (400 Bad Request)**
```json
{
  "data": {},
  "message": "Validation failed: [specific validation error]",
  "statusCode": 400
}
```

---

## ⚠️ **COMMON VALIDATION ERRORS TO AVOID**

### **❌ Date Errors**
```json
// WRONG - Past date
"expirationDate": "2023-12-31T23:59:59Z"

// CORRECT - Future date
"expirationDate": "2025-12-31T23:59:59Z"
```

### **❌ Price Errors**
```json
// WRONG - Zero or negative price
"price": 0

// CORRECT - Positive price
"price": 99.99
```

### **❌ GUID Errors**
```json
// WRONG - Empty GUID
"billingCycleId": "00000000-0000-0000-0000-000000000000"

// CORRECT - Valid GUID
"billingCycleId": "11111111-1111-1111-1111-111111111111"
```

### **❌ String Length Errors**
```json
// WRONG - Too long name (over 100 chars)
"name": "This is a very long subscription plan name that exceeds the maximum allowed length of 100 characters and will cause validation to fail"

// CORRECT - Within limit
"name": "Premium Telehealth Plan"
```

---

## 🎯 **TESTING SCENARIOS**

### **Scenario 1: Basic Plan Creation**
- Use the provided payload as-is (after replacing GUIDs)
- Should create successfully with all privileges

### **Scenario 2: Plan with Trial**
- Set `isTrialAllowed: true` and `trialDurationInDays: 7`
- Should create successfully with trial configuration

### **Scenario 3: Plan with Discount**
- Set `discountedPrice: 79.99` and `discountValidUntil: "2025-12-31T23:59:59Z"`
- Should create successfully with discount configuration

### **Scenario 4: Plan with Limited Privileges**
- Change privilege `value` from `-1` to `10` (limited)
- Should create successfully with limited privileges

---

## 🔍 **TROUBLESHOOTING**

### **Error: "Subscription plan does not exist"**
- Check if the `billingCycleId`, `currencyId`, or `categoryId` exist in the database
- Verify the GUIDs are correct

### **Error: "Expiration date cannot be in the past"**
- Ensure all dates (`expirationDate`, `effectiveDate`, `discountValidUntil`) are in the future
- Use ISO 8601 format: `YYYY-MM-DDTHH:mm:ssZ`

### **Error: "Access denied - Admin only"**
- Ensure you're using an admin token
- Check the `Authorization` header is properly set

### **Error: "A plan with this name already exists"**
- Change the `name` field to something unique
- Or delete the existing plan first

---

## ✅ **VALIDATION CHECKLIST**

Before sending the request, verify:

- [ ] All placeholder GUIDs replaced with real ones
- [ ] All dates are in the future
- [ ] Price is greater than 0
- [ ] All string lengths are within limits
- [ ] All numeric values are within valid ranges
- [ ] Admin authorization token is set
- [ ] JSON is properly formatted

---

**🎯 This payload is designed to pass all validation rules and create a comprehensive subscription plan with multiple privileges!**

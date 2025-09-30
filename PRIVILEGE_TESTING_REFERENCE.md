# 🔐 **PRIVILEGE TESTING REFERENCE**

## 🎯 **PRIVILEGE TYPES & VALIDATION RULES**

### **📋 Privilege Value Rules**
- **`-1`**: Unlimited usage
- **`0`**: Disabled/No access
- **`> 0`**: Limited usage (specific count)

### **⏰ Time-Based Limits**
- **`dailyLimit`**: Maximum usage per day
- **`weeklyLimit`**: Maximum usage per week  
- **`monthlyLimit`**: Maximum usage per month
- **`null`**: No limit for that time period

### **💰 Unit Cost Rules**
- **`0`**: No additional cost for overage
- **`> 0`**: Cost per unit when used beyond limits

---

## 🧪 **PRIVILEGE TESTING SCENARIOS**

### **✅ Scenario 1: Unlimited Privilege**
```json
{
  "privilegeId": "44444444-4444-4444-4444-444444444444",
  "value": -1,
  "dailyLimit": null,
  "weeklyLimit": null,
  "monthlyLimit": null,
  "unitCost": 0
}
```
**Expected Behavior**: No usage limits, no overage charges

### **✅ Scenario 2: Limited Privilege with Time Limits**
```json
{
  "privilegeId": "55555555-5555-5555-5555-555555555555",
  "value": 100,
  "dailyLimit": 10,
  "weeklyLimit": 50,
  "monthlyLimit": 100,
  "unitCost": 0.25
}
```
**Expected Behavior**: 
- Max 10 per day
- Max 50 per week
- Max 100 per month
- $0.25 per unit over limit

### **✅ Scenario 3: Disabled Privilege**
```json
{
  "privilegeId": "66666666-6666-6666-6666-666666666666",
  "value": 0,
  "dailyLimit": 0,
  "weeklyLimit": 0,
  "monthlyLimit": 0,
  "unitCost": 0
}
```
**Expected Behavior**: No access to this privilege

### **✅ Scenario 4: Mixed Time Limits**
```json
{
  "privilegeId": "77777777-7777-7777-7777-777777777777",
  "value": 50,
  "dailyLimit": null,
  "weeklyLimit": 25,
  "monthlyLimit": 50,
  "unitCost": 1.00
}
```
**Expected Behavior**:
- No daily limit
- Max 25 per week
- Max 50 per month
- $1.00 per unit over limit

---

## 🔍 **VALIDATION TESTING MATRIX**

| Value | Daily Limit | Weekly Limit | Monthly Limit | Unit Cost | Expected Result |
|-------|-------------|--------------|---------------|-----------|-----------------|
| -1 | null | null | null | 0 | ✅ Unlimited |
| -1 | 10 | null | null | 0 | ✅ Unlimited with daily cap |
| 0 | 0 | 0 | 0 | 0 | ✅ Disabled |
| 10 | 5 | 8 | 10 | 0.50 | ✅ Limited with overage |
| 100 | null | 50 | 100 | 0.25 | ✅ No daily limit |
| 50 | 10 | null | 50 | 1.00 | ✅ No weekly limit |
| 25 | 5 | 15 | null | 2.00 | ✅ No monthly limit |

---

## ⚠️ **INVALID COMBINATIONS TO TEST**

### **❌ Invalid Value**
```json
{
  "value": -5  // ❌ Must be -1, 0, or positive
}
```

### **❌ Negative Time Limits**
```json
{
  "dailyLimit": -1,    // ❌ Must be ≥ 0
  "weeklyLimit": -5,   // ❌ Must be ≥ 0
  "monthlyLimit": -10  // ❌ Must be ≥ 0
}
```

### **❌ Negative Unit Cost**
```json
{
  "unitCost": -0.50  // ❌ Must be ≥ 0
}
```

### **❌ Inconsistent Limits**
```json
{
  "value": 10,
  "dailyLimit": 20,    // ❌ Daily limit > monthly limit
  "monthlyLimit": 10
}
```

---

## 🎯 **PRIVILEGE TESTING CHECKLIST**

### **✅ Positive Tests**
- [ ] Unlimited privilege (-1) works
- [ ] Limited privilege (> 0) works
- [ ] Disabled privilege (0) works
- [ ] Time-based limits work correctly
- [ ] Unit costs are applied correctly
- [ ] Mixed limit configurations work
- [ ] Null limits work (no restriction)

### **❌ Negative Tests**
- [ ] Invalid value (-5) fails validation
- [ ] Negative time limits fail validation
- [ ] Negative unit cost fails validation
- [ ] Inconsistent limits fail validation
- [ ] Empty GUID fails validation
- [ ] Past expiration date fails validation

---

## 📊 **EXPECTED RESPONSE PATTERNS**

### **✅ Success Response**
```json
{
  "data": {
    "privileges": [
      {
        "privilegeId": "44444444-4444-4444-4444-444444444444",
        "value": -1,
        "description": "Unlimited video consultations per month",
        "dailyLimit": null,
        "weeklyLimit": null,
        "monthlyLimit": null,
        "unitCost": 0
      }
    ]
  },
  "message": "Plan created successfully with privileges",
  "statusCode": 201
}
```

### **❌ Validation Error Response**
```json
{
  "data": {},
  "message": "Value must be -1 (unlimited), 0 (disabled), or positive number",
  "statusCode": 400
}
```

---

## 🚀 **TESTING EXECUTION ORDER**

1. **Test Basic Privilege** (1 privilege, simple configuration)
2. **Test Multiple Privileges** (3-4 privileges, mixed configurations)
3. **Test Complex Privileges** (6+ privileges, all configurations)
4. **Test Edge Cases** (unlimited, disabled, mixed limits)
5. **Test Validation Errors** (invalid values, negative limits)

---

**🎯 Use this reference to systematically test all privilege configurations and validation scenarios!**

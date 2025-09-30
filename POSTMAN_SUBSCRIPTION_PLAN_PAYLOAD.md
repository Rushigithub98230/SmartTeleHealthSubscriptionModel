# 🚀 **POSTMAN SUBSCRIPTION PLAN CREATION PAYLOAD**

## 📋 **API ENDPOINT DETAILS**

### **Endpoint Information:**
- **Method**: `POST`
- **URL**: `https://your-api-domain.com/api/SubscriptionPlans/admin`
- **Content-Type**: `application/json`
- **Authorization**: Bearer Token (if authentication is enabled)

### **Headers Required:**
```json
{
  "Content-Type": "application/json",
  "Authorization": "Bearer YOUR_JWT_TOKEN_HERE"
}
```

---

## 🎯 **COMPLETE POSTMAN PAYLOAD**

### **Basic Subscription Plan Creation:**
```json
{
  "name": "Premium Telehealth Plan",
  "description": "Comprehensive telehealth subscription plan with unlimited consultations, medication delivery, and priority support",
  "shortDescription": "Premium plan with unlimited consultations",
  "price": 99.99,
  "discountedPrice": 79.99,
  "discountValidUntil": "2024-12-31T23:59:59Z",
  "billingCycleId": "550e8400-e29b-41d4-a716-446655440000",
  "currencyId": "550e8400-e29b-41d4-a716-446655440001",
  "categoryId": "550e8400-e29b-41d4-a716-446655440002",
  "isTrialAllowed": true,
  "trialDurationInDays": 14,
  "isFeatured": true,
  "isMostPopular": true,
  "isTrending": false,
  "displayOrder": 1,
  "messagingCount": 1000,
  "includesMedicationDelivery": true,
  "includesFollowUpCare": true,
  "deliveryFrequencyDays": 30,
  "maxPauseDurationDays": 90,
  "maxConcurrentUsers": 1,
  "gracePeriodDays": 7,
  "isActive": true,
  "features": "Unlimited consultations, 24/7 support, medication delivery, health assessments, priority booking",
  "terms": "This plan includes unlimited consultations, medication delivery, and priority support. Terms and conditions apply.",
  "effectiveDate": "2024-01-01T00:00:00Z",
  "expirationDate": "2025-12-31T23:59:59Z",
  "stripeProductId": "prod_premium_telehealth",
  "stripeMonthlyPriceId": "price_premium_monthly",
  "stripeQuarterlyPriceId": "price_premium_quarterly",
  "stripeAnnualPriceId": "price_premium_annual",
  "privileges": [
    {
      "privilegeId": "550e8400-e29b-41d4-a716-446655440010",
      "value": -1,
      "usagePeriodId": "550e8400-e29b-41d4-a716-446655440000",
      "durationMonths": 1,
      "description": "Unlimited video consultations",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expirationDate": "2025-12-31T23:59:59Z",
      "dailyLimit": null,
      "weeklyLimit": null,
      "monthlyLimit": null,
      "unitCost": 0
    },
    {
      "privilegeId": "550e8400-e29b-41d4-a716-446655440011",
      "value": 100,
      "usagePeriodId": "550e8400-e29b-41d4-a716-446655440000",
      "durationMonths": 1,
      "description": "100 messaging credits per month",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expirationDate": "2025-12-31T23:59:59Z",
      "dailyLimit": 10,
      "weeklyLimit": 50,
      "monthlyLimit": 100,
      "unitCost": 0.50
    },
    {
      "privilegeId": "550e8400-e29b-41d4-a716-446655440012",
      "value": 5,
      "usagePeriodId": "550e8400-e29b-41d4-a716-446655440000",
      "durationMonths": 1,
      "description": "5 medication deliveries per month",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expirationDate": "2025-12-31T23:59:59Z",
      "dailyLimit": 1,
      "weeklyLimit": 2,
      "monthlyLimit": 5,
      "unitCost": 15.00
    }
  ]
}
```

---

## 🎯 **ALTERNATIVE PAYLOAD EXAMPLES**

### **1. Basic Plan (No Privileges):**
```json
{
  "name": "Basic Telehealth Plan",
  "description": "Basic telehealth subscription plan with limited consultations",
  "shortDescription": "Basic plan with 5 consultations",
  "price": 29.99,
  "billingCycleId": "550e8400-e29b-41d4-a716-446655440000",
  "currencyId": "550e8400-e29b-41d4-a716-446655440001",
  "categoryId": "550e8400-e29b-41d4-a716-446655440002",
  "isTrialAllowed": false,
  "trialDurationInDays": 0,
  "isFeatured": false,
  "isMostPopular": false,
  "isTrending": false,
  "displayOrder": 3,
  "messagingCount": 50,
  "includesMedicationDelivery": false,
  "includesFollowUpCare": false,
  "deliveryFrequencyDays": 0,
  "maxPauseDurationDays": 30,
  "maxConcurrentUsers": 1,
  "gracePeriodDays": 3,
  "isActive": true,
  "features": "5 consultations per month, basic support",
  "terms": "Basic plan with limited consultations. Terms and conditions apply.",
  "effectiveDate": "2024-01-01T00:00:00Z",
  "expirationDate": "2025-12-31T23:59:59Z",
  "privileges": []
}
```

### **2. Enterprise Plan (Multiple Privileges):**
```json
{
  "name": "Enterprise Telehealth Plan",
  "description": "Enterprise-grade telehealth subscription plan with unlimited everything",
  "shortDescription": "Enterprise plan with unlimited everything",
  "price": 299.99,
  "billingCycleId": "550e8400-e29b-41d4-a716-446655440000",
  "currencyId": "550e8400-e29b-41d4-a716-446655440001",
  "categoryId": "550e8400-e29b-41d4-a716-446655440002",
  "isTrialAllowed": true,
  "trialDurationInDays": 30,
  "isFeatured": true,
  "isMostPopular": false,
  "isTrending": true,
  "displayOrder": 1,
  "messagingCount": -1,
  "includesMedicationDelivery": true,
  "includesFollowUpCare": true,
  "deliveryFrequencyDays": 7,
  "maxPauseDurationDays": 365,
  "maxConcurrentUsers": 10,
  "gracePeriodDays": 30,
  "isActive": true,
  "features": "Unlimited consultations, unlimited messaging, unlimited medication delivery, priority support, dedicated account manager",
  "terms": "Enterprise plan with unlimited everything. Terms and conditions apply.",
  "effectiveDate": "2024-01-01T00:00:00Z",
  "expirationDate": "2025-12-31T23:59:59Z",
  "stripeProductId": "prod_enterprise_telehealth",
  "stripeMonthlyPriceId": "price_enterprise_monthly",
  "stripeQuarterlyPriceId": "price_enterprise_quarterly",
  "stripeAnnualPriceId": "price_enterprise_annual",
  "privileges": [
    {
      "privilegeId": "550e8400-e29b-41d4-a716-446655440010",
      "value": -1,
      "usagePeriodId": "550e8400-e29b-41d4-a716-446655440000",
      "durationMonths": 1,
      "description": "Unlimited video consultations",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expirationDate": "2025-12-31T23:59:59Z",
      "dailyLimit": null,
      "weeklyLimit": null,
      "monthlyLimit": null,
      "unitCost": 0
    },
    {
      "privilegeId": "550e8400-e29b-41d4-a716-446655440011",
      "value": -1,
      "usagePeriodId": "550e8400-e29b-41d4-a716-446655440000",
      "durationMonths": 1,
      "description": "Unlimited messaging",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expirationDate": "2025-12-31T23:59:59Z",
      "dailyLimit": null,
      "weeklyLimit": null,
      "monthlyLimit": null,
      "unitCost": 0
    },
    {
      "privilegeId": "550e8400-e29b-41d4-a716-446655440012",
      "value": -1,
      "usagePeriodId": "550e8400-e29b-41d4-a716-446655440000",
      "durationMonths": 1,
      "description": "Unlimited medication deliveries",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expirationDate": "2025-12-31T23:59:59Z",
      "dailyLimit": null,
      "weeklyLimit": null,
      "monthlyLimit": null,
      "unitCost": 0
    },
    {
      "privilegeId": "550e8400-e29b-41d4-a716-446655440013",
      "value": 10,
      "usagePeriodId": "550e8400-e29b-41d4-a716-446655440000",
      "durationMonths": 1,
      "description": "10 health assessments per month",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expirationDate": "2025-12-31T23:59:59Z",
      "dailyLimit": 2,
      "weeklyLimit": 5,
      "monthlyLimit": 10,
      "unitCost": 25.00
    }
  ]
}
```

---

## 🔧 **FIELD EXPLANATIONS**

### **Required Fields:**
- `name`: Plan name (max 100 characters)
- `price`: Base price (must be > 0)
- `billingCycleId`: GUID reference to billing cycle (monthly, yearly, etc.)
- `currencyId`: GUID reference to currency (USD, EUR, etc.)
- `categoryId`: GUID reference to category (Mental Health, Physical Health, etc.)

### **Optional Fields:**
- `description`: Detailed plan description (max 500 characters)
- `shortDescription`: Brief plan description (max 200 characters)
- `discountedPrice`: Promotional price
- `discountValidUntil`: When discount expires
- `isTrialAllowed`: Whether trial is available
- `trialDurationInDays`: Trial period length
- `isFeatured`: Whether plan is featured
- `isMostPopular`: Whether plan is most popular
- `isTrending`: Whether plan is trending
- `displayOrder`: Sort order for display
- `messagingCount`: Number of messages included
- `includesMedicationDelivery`: Whether medication delivery is included
- `includesFollowUpCare`: Whether follow-up care is included
- `deliveryFrequencyDays`: How often deliveries occur
- `maxPauseDurationDays`: Maximum pause duration
- `maxConcurrentUsers`: Maximum concurrent users
- `gracePeriodDays`: Grace period for payments
- `isActive`: Whether plan is active
- `features`: Plan features description
- `terms`: Terms and conditions
- `effectiveDate`: When plan becomes effective
- `expirationDate`: When plan expires
- `stripeProductId`: Stripe product ID
- `stripeMonthlyPriceId`: Stripe monthly price ID
- `stripeQuarterlyPriceId`: Stripe quarterly price ID
- `stripeAnnualPriceId`: Stripe annual price ID
- `privileges`: Array of plan privileges

### **Privilege Fields:**
- `privilegeId`: GUID reference to privilege
- `value`: Usage limit (-1 = unlimited, 0 = disabled, >0 = limited)
- `usagePeriodId`: GUID reference to usage period
- `durationMonths`: Duration in months
- `description`: Privilege description
- `effectiveDate`: When privilege becomes effective
- `expirationDate`: When privilege expires
- `dailyLimit`: Daily usage limit
- `weeklyLimit`: Weekly usage limit
- `monthlyLimit`: Monthly usage limit
- `unitCost`: Cost per unit when limit exceeded

---

## 🚨 **IMPORTANT NOTES**

### **GUID Requirements:**
You need to replace the placeholder GUIDs with actual GUIDs from your database:

1. **Billing Cycle GUIDs** (e.g., monthly, yearly):
   - `550e8400-e29b-41d4-a716-446655440000` → Replace with actual billing cycle GUID

2. **Currency GUIDs** (e.g., USD, EUR):
   - `550e8400-e29b-41d4-a716-446655440001` → Replace with actual currency GUID

3. **Category GUIDs** (e.g., Mental Health, Physical Health):
   - `550e8400-e29b-41d4-a716-446655440002` → Replace with actual category GUID

4. **Privilege GUIDs** (e.g., Video Consultation, Messaging):
   - `550e8400-e29b-41d4-a716-446655440010` → Replace with actual privilege GUIDs

### **How to Get Valid GUIDs:**
1. **Check your database** for existing master data
2. **Use the GET endpoints** to retrieve available options:
   - `GET /api/SubscriptionPlans/billing-cycles`
   - `GET /api/SubscriptionPlans/currencies`
   - `GET /api/SubscriptionPlans/categories`
   - `GET /api/Privileges`

### **Validation Rules:**
- `price` must be > 0
- `trialDurationInDays` must be >= 0
- `messagingCount` must be >= 0
- `deliveryFrequencyDays` must be >= 1
- `maxPauseDurationDays` must be >= 0
- `maxConcurrentUsers` must be >= 1
- `gracePeriodDays` must be >= 0
- `privilege.value` must be -1, 0, or positive
- `privilege.dailyLimit` must be >= 0
- `privilege.weeklyLimit` must be >= 0
- `privilege.monthlyLimit` must be >= 0
- `privilege.unitCost` must be >= 0

---

## 🎯 **TESTING STEPS**

### **1. Test Basic Plan Creation:**
```bash
# Use the Basic Plan payload first
# This tests the core functionality without privileges
```

### **2. Test Plan with Privileges:**
```bash
# Use the Premium Plan payload
# This tests the privilege assignment functionality
```

### **3. Test Enterprise Plan:**
```bash
# Use the Enterprise Plan payload
# This tests complex privilege configurations
```

### **4. Expected Responses:**
- **Success (201)**: Plan created successfully
- **Bad Request (400)**: Validation errors
- **Unauthorized (401)**: Authentication required
- **Forbidden (403)**: Admin access required

---

## 🔍 **TROUBLESHOOTING**

### **Common Issues:**
1. **Invalid GUIDs**: Replace placeholder GUIDs with actual database GUIDs
2. **Validation Errors**: Check field requirements and constraints
3. **Authentication**: Ensure proper Bearer token is provided
4. **Authorization**: Ensure user has admin privileges

### **Debug Steps:**
1. **Check API logs** for detailed error messages
2. **Validate JSON** syntax and structure
3. **Verify GUIDs** exist in database
4. **Test with minimal payload** first
5. **Check database constraints** and foreign key relationships

---

## 🚀 **READY TO TEST!**

Copy any of the payload examples above into Postman and test your subscription plan creation API. Make sure to:

1. ✅ Replace placeholder GUIDs with actual database GUIDs
2. ✅ Set proper authorization headers
3. ✅ Use correct API endpoint URL
4. ✅ Validate JSON syntax
5. ✅ Test with different payload variations

**Your subscription plan creation API should now work correctly with the fixed AutoMapper configurations!** 🎉

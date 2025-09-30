# 📊 **DATABASE TABLE REFERENCE FOR SUBSCRIPTION PLAN CREATION**

## 🎯 **REQUIRED TABLES FOR SUBSCRIPTION PLAN PAYLOAD**

Based on the database structure analysis, here are the exact table names and sample data you need for your subscription plan creation:

---

## 📋 **1. CURRENCY IDS - `MasterCurrencies` Table**

**Table Name**: `MasterCurrencies`

**Sample Data** (from seed data):
```sql
-- Get available currencies
SELECT Id, Code, Name, Symbol FROM MasterCurrencies WHERE IsActive = 1;

-- Sample results:
-- Id: [GUID], Code: "USD", Name: "US Dollar", Symbol: "$"
-- Id: [GUID], Code: "EUR", Name: "Euro", Symbol: "€"  
-- Id: [GUID], Code: "GBP", Name: "British Pound", Symbol: "£"
-- Id: [GUID], Code: "INR", Name: "Indian Rupee", Symbol: "₹"
```

**Usage in Payload**:
```json
{
  "currencyId": "REPLACE_WITH_USD_GUID_FROM_DATABASE"
}
```

---

## 📋 **2. BILLING CYCLE IDS - `MasterBillingCycles` Table**

**Table Name**: `MasterBillingCycles`

**Sample Data** (from seed data):
```sql
-- Get available billing cycles
SELECT Id, Name, Description, DurationInDays FROM MasterBillingCycles WHERE IsActive = 1;

-- Sample results:
-- Id: [GUID], Name: "Monthly", Description: "Monthly billing cycle", DurationInDays: 30
-- Id: [GUID], Name: "Quarterly", Description: "Quarterly billing cycle", DurationInDays: 90
-- Id: [GUID], Name: "Annual", Description: "Annual billing cycle", DurationInDays: 365
```

**Usage in Payload**:
```json
{
  "billingCycleId": "REPLACE_WITH_MONTHLY_GUID_FROM_DATABASE"
}
```

---

## 📋 **3. CATEGORY IDS - `Categories` Table**

**Table Name**: `Categories`

**Sample Data** (you need to create these):
```sql
-- Get available categories
SELECT Id, Name, Description FROM Categories WHERE IsActive = 1;

-- Example categories you might have:
-- Id: [GUID], Name: "General Health", Description: "General health consultations"
-- Id: [GUID], Name: "Mental Health", Description: "Mental health and therapy"
-- Id: [GUID], Name: "Dermatology", Description: "Skin and hair consultations"
-- Id: [GUID], Name: "Cardiology", Description: "Heart and cardiovascular health"
```

**Usage in Payload**:
```json
{
  "categoryId": "REPLACE_WITH_CATEGORY_GUID_FROM_DATABASE"
}
```

---

## 📋 **4. PRIVILEGE IDS - `Privileges` Table**

**Table Name**: `Privileges`

**Sample Data** (you need to create these):
```sql
-- Get available privileges
SELECT Id, Name, Description, PrivilegeTypeId FROM Privileges WHERE IsActive = 1;

-- Example privileges you might have:
-- Id: [GUID], Name: "Video Consultations", Description: "Access to video consultations"
-- Id: [GUID], Name: "Messaging", Description: "Unlimited messaging with providers"
-- Id: [GUID], Name: "Medication Delivery", Description: "Home medication delivery"
-- Id: [GUID], Name: "Document Access", Description: "Access to medical documents"
```

**Usage in Payload**:
```json
{
  "privileges": [
    {
      "privilegeId": "REPLACE_WITH_PRIVILEGE_GUID_FROM_DATABASE",
      "value": 10,
      "usagePeriodId": "REPLACE_WITH_BILLING_CYCLE_GUID"
    }
  ]
}
```

---

## 📋 **5. PRIVILEGE TYPE IDS - `MasterPrivilegeTypes` Table**

**Table Name**: `MasterPrivilegeTypes`

**Sample Data** (from seed data):
```sql
-- Get available privilege types
SELECT Id, Name, Description FROM MasterPrivilegeTypes WHERE IsActive = 1;

-- Sample results:
-- Id: [GUID], Name: "Consultation", Description: "Consultation privileges"
-- Id: [GUID], Name: "Medication", Description: "Medication-related privileges"
-- Id: [GUID], Name: "Messaging", Description: "Messaging privileges"
-- Id: [GUID], Name: "Document", Description: "Document access privileges"
```

---

## 🔍 **HOW TO GET THE ACTUAL GUIDs**

### **Method 1: Direct Database Query**
```sql
-- Get all required IDs in one query
SELECT 
    'Currency' as Type, Id, Code as Name FROM MasterCurrencies WHERE IsActive = 1
UNION ALL
SELECT 
    'BillingCycle' as Type, Id, Name FROM MasterBillingCycles WHERE IsActive = 1
UNION ALL
SELECT 
    'Category' as Type, Id, Name FROM Categories WHERE IsActive = 1
UNION ALL
SELECT 
    'Privilege' as Type, Id, Name FROM Privileges WHERE IsActive = 1
UNION ALL
SELECT 
    'PrivilegeType' as Type, Id, Name FROM MasterPrivilegeTypes WHERE IsActive = 1;
```

### **Method 2: Using Entity Framework (if you have access to the application)**
```csharp
// In your application or a test console app
using (var context = new ApplicationDbContext())
{
    var currencies = context.MasterCurrencies.Where(c => c.IsActive).ToList();
    var billingCycles = context.MasterBillingCycles.Where(b => b.IsActive).ToList();
    var categories = context.Categories.Where(c => c.IsActive).ToList();
    var privileges = context.Privileges.Where(p => p.IsActive).ToList();
    
    // Print the IDs
    foreach(var currency in currencies)
        Console.WriteLine($"Currency: {currency.Code} - {currency.Id}");
}
```

### **Method 3: Check if Seed Data is Available**
The seed data shows these default values are created automatically:

**Default Currencies**:
- USD (US Dollar)
- EUR (Euro) 
- GBP (British Pound)
- INR (Indian Rupee)

**Default Billing Cycles**:
- Monthly (30 days)
- Quarterly (90 days)
- Annual (365 days)

**Default Privilege Types**:
- Consultation
- Medication
- Messaging
- Document

---

## 📝 **COMPLETE EXAMPLE PAYLOAD WITH PLACEHOLDERS**

```json
{
  "name": "Premium Health Plan",
  "description": "Comprehensive health plan with unlimited consultations",
  "shortDescription": "Premium plan with full access",
  "price": 99.99,
  "discountedPrice": 79.99,
  "discountValidUntil": "2024-12-31T23:59:59Z",
  "billingCycleId": "REPLACE_WITH_MONTHLY_GUID_FROM_MasterBillingCycles",
  "currencyId": "REPLACE_WITH_USD_GUID_FROM_MasterCurrencies",
  "categoryId": "REPLACE_WITH_CATEGORY_GUID_FROM_Categories",
  "isTrialAllowed": true,
  "trialDurationInDays": 7,
  "isFeatured": true,
  "isMostPopular": false,
  "isTrending": false,
  "displayOrder": 1,
  "messagingCount": 100,
  "includesMedicationDelivery": true,
  "includesFollowUpCare": true,
  "deliveryFrequencyDays": 30,
  "maxPauseDurationDays": 90,
  "maxConcurrentUsers": 1,
  "gracePeriodDays": 7,
  "isActive": true,
  "features": "Unlimited consultations, 24/7 support, medication delivery",
  "terms": "Standard terms and conditions apply",
  "effectiveDate": "2024-01-01T00:00:00Z",
  "expirationDate": "2024-12-31T23:59:59Z",
  "privileges": [
    {
      "privilegeId": "REPLACE_WITH_CONSULTATION_PRIVILEGE_GUID_FROM_Privileges",
      "value": -1,
      "usagePeriodId": "REPLACE_WITH_MONTHLY_GUID_FROM_MasterBillingCycles",
      "durationMonths": 1,
      "description": "Unlimited consultations",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "dailyLimit": null,
      "weeklyLimit": null,
      "monthlyLimit": null,
      "unitCost": 0
    }
  ]
}
```

---

## 🚀 **QUICK START STEPS**

1. **Run the seed data** to populate master tables
2. **Create some categories** in the `Categories` table
3. **Create some privileges** in the `Privileges` table
4. **Query the database** to get the actual GUIDs
5. **Replace the placeholders** in the payload
6. **Test the API** with the complete payload

---

## ⚠️ **IMPORTANT NOTES**

- **All IDs are GUIDs** (not integers)
- **Tables are case-sensitive** in the database
- **IsActive = 1** means the record is active
- **Seed data runs automatically** on first application start
- **You may need to create Categories and Privileges** manually if they don't exist

---

**🎯 Use this reference to get the exact GUIDs from your database and replace the placeholders in your Postman payload!**

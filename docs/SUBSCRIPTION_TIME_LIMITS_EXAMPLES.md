# Subscription Time-Based Privilege Limits - Implementation Guide

## 🚀 Overview

This document demonstrates how to use the new **3-column time-based privilege system** that allows admins to create complex subscription plans with daily, weekly, and monthly usage limits.

## ✨ What's New

### **3 Simple Columns Added:**
- `DailyLimit` - Maximum usage per day
- `WeeklyLimit` - Maximum usage per week  
- `MonthlyLimit` - Maximum usage per month

### **New Usage History Tracking:**
- `PrivilegeUsageHistory` table tracks every privilege usage
- Automatic time-based limit enforcement
- Real-time usage monitoring

## 🎯 How It Works

### **1. Plan Creation**
```csharp
// Admin creates a plan with time-based limits
var plan = new SubscriptionPlanPrivilege
{
    PrivilegeId = consultationPrivilegeId,
    Value = 20,                    // Total: 20 consultations
    DailyLimit = 2,                // Max 2 per day
    WeeklyLimit = 5,               // Max 5 per week
    MonthlyLimit = 15,             // Max 15 per month
    DurationMonths = 6             // Plan valid for 6 months
};
```

### **2. Usage Enforcement**
```csharp
// System automatically checks limits before allowing usage
if (dailyUsage + requestedAmount > dailyLimit) return false;
if (weeklyUsage + requestedAmount > weeklyLimit) return false;
if (monthlyUsage + requestedAmount > monthlyLimit) return false;
```

### **3. Usage Tracking**
```csharp
// Every usage is recorded with timestamp
var usageHistory = new PrivilegeUsageHistory
{
    UsedValue = 1,
    UsedAt = DateTime.UtcNow,
    UsageDate = DateTime.UtcNow.Date,
    UsageWeek = "2024-01",
    UsageMonth = "2024-01"
};
```

## 📋 Complete Example Plans

### **Plan 1: Basic Health Plan - $29.99/month**
```json
{
  "planName": "Basic Health Plan",
  "description": "Essential healthcare services with daily limits",
  "price": 29.99,
  "billingCycle": "Monthly",
  "durationMonths": 1,
  "privileges": [
    {
      "privilegeName": "Teleconsultation",
      "totalValue": 5,
      "dailyLimit": 1,
      "weeklyLimit": 3,
      "monthlyLimit": 5,
      "description": "Max 1 consultation per day, 3 per week, 5 per month"
    },
    {
      "privilegeName": "Health Assessment",
      "totalValue": 2,
      "dailyLimit": 1,
      "weeklyLimit": 1,
      "monthlyLimit": 2,
      "description": "Max 1 assessment per week"
    }
  ]
}
```

### **Plan 2: Premium Health Plan - $79.99/month**
```json
{
  "planName": "Premium Health Plan",
  "description": "Comprehensive healthcare with flexible usage",
  "price": 79.99,
  "billingCycle": "Monthly",
  "durationMonths": 1,
  "privileges": [
    {
      "privilegeName": "Teleconsultation",
      "totalValue": 15,
      "dailyLimit": 2,
      "weeklyLimit": 8,
      "monthlyLimit": 15,
      "description": "Flexible usage with reasonable daily limits"
    },
    {
      "privilegeName": "Medication Delivery",
      "totalValue": 8,
      "dailyLimit": 1,
      "weeklyLimit": 3,
      "monthlyLimit": 8,
      "description": "Medication delivery with weekly limits"
    },
    {
      "privilegeName": "Lab Tests",
      "totalValue": 4,
      "dailyLimit": 1,
      "weeklyLimit": 2,
      "monthlyLimit": 4,
      "description": "Lab tests with weekly restrictions"
    }
  ]
}
```

### **Plan 3: Annual Wellness Plan - $599.99/year**
```json
{
  "planName": "Annual Wellness Plan",
  "description": "Year-round comprehensive wellness coverage",
  "price": 599.99,
  "billingCycle": "Annual",
  "durationMonths": 12,
  "privileges": [
    {
      "privilegeName": "Teleconsultation",
      "totalValue": 100,
      "dailyLimit": 3,
      "weeklyLimit": 10,
      "monthlyLimit": 25,
      "description": "Generous limits for year-round access"
    },
    {
      "privilegeName": "Therapy Sessions",
      "totalValue": 48,
      "dailyLimit": 1,
      "weeklyLimit": 4,
      "monthlyLimit": 12,
      "description": "Regular therapy with weekly maximums"
    },
    {
      "privilegeName": "Health Coaching",
      "totalValue": 24,
      "dailyLimit": 1,
      "weeklyLimit": 2,
      "monthlyLimit": 6,
      "description": "Structured coaching program"
    }
  ]
}
```

## 🔧 API Usage Examples

### **Create a New Plan**
```http
POST /api/admin/subscription/plans
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "planName": "Flexible Health Plan",
  "description": "Health services with smart usage limits",
  "price": 49.99,
  "billingCycle": "Monthly",
  "durationMonths": 1,
  "privileges": [
    {
      "privilegeName": "Teleconsultation",
      "totalValue": 10,
      "dailyLimit": 1,
      "weeklyLimit": 4,
      "monthlyLimit": 10,
      "description": "Smart daily limits prevent abuse"
    }
  ]
}
```

### **Check User's Remaining Privileges**
```http
GET /api/privileges/remaining/{subscriptionId}/Teleconsultation
Authorization: Bearer {user_token}
```

### **Use a Privilege**
```http
POST /api/privileges/use
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "subscriptionId": "guid-here",
  "privilegeName": "Teleconsultation",
  "amount": 1
}
```

## 📊 Usage Monitoring

### **Daily Usage Tracking**
```sql
-- Get daily usage for a specific privilege
SELECT 
    u.UsageDate,
    SUM(u.UsedValue) as DailyUsage,
    pp.DailyLimit
FROM PrivilegeUsageHistories u
JOIN UserSubscriptionPrivilegeUsages uspu ON u.UserSubscriptionPrivilegeUsageId = uspu.Id
JOIN SubscriptionPlanPrivileges pp ON uspu.SubscriptionPlanPrivilegeId = pp.Id
WHERE uspu.SubscriptionId = @subscriptionId 
    AND pp.PrivilegeId = @privilegeId
    AND u.UsageDate = @date
GROUP BY u.UsageDate, pp.DailyLimit
```

### **Weekly Usage Summary**
```sql
-- Get weekly usage summary
SELECT 
    u.UsageWeek,
    SUM(u.UsedValue) as WeeklyUsage,
    pp.WeeklyLimit
FROM PrivilegeUsageHistories u
JOIN UserSubscriptionPrivilegeUsages uspu ON u.UserSubscriptionPrivilegeUsageId = uspu.Id
JOIN SubscriptionPlanPrivileges pp ON uspu.SubscriptionPlanPrivilegeId = pp.Id
WHERE uspu.SubscriptionId = @subscriptionId 
    AND pp.PrivilegeId = @privilegeId
    AND u.UsageWeek = @week
GROUP BY u.UsageWeek, pp.WeeklyLimit
```

## 🎯 Business Rules Examples

### **Rule 1: "5 Consultations in 5 Months - Only 1 per month"**
```csharp
var plan = new SubscriptionPlanPrivilege
{
    Value = 5,                     // Total: 5 consultations
    MonthlyLimit = 1,              // Max 1 per month
    DurationMonths = 5             // Valid for 5 months
};
```

### **Rule 2: "10 Consultations in 2 Months - Max 1 per day"**
```csharp
var plan = new SubscriptionPlanPrivilege
{
    Value = 10,                    // Total: 10 consultations
    DailyLimit = 1,                // Max 1 per day
    DurationMonths = 2             // Valid for 2 months
};
```

### **Rule 3: "15 Consultations in 3 Months - Max 2 per week"**
```csharp
var plan = new SubscriptionPlanPrivilege
{
    Value = 15,                    // Total: 15 consultations
    WeeklyLimit = 2,               // Max 2 per week
    DurationMonths = 3             // Valid for 3 months
};
```

### **Rule 4: "20 Consultations in 6 Months - Max 3 per week"**
```csharp
var plan = new SubscriptionPlanPrivilege
{
    Value = 20,                    // Total: 20 consultations
    WeeklyLimit = 3,               // Max 3 per week
    DurationMonths = 6             // Valid for 6 months
};
```

## 🚨 Error Handling

### **Daily Limit Exceeded**
```json
{
  "success": false,
  "message": "Daily limit exceeded for Teleconsultation. Used: 2, Limit: 1, Requested: 1",
  "errorCode": "DAILY_LIMIT_EXCEEDED"
}
```

### **Weekly Limit Exceeded**
```json
{
  "success": false,
  "message": "Weekly limit exceeded for Teleconsultation. Used: 5, Limit: 3, Requested: 1",
  "errorCode": "WEEKLY_LIMIT_EXCEEDED"
}
```

### **Monthly Limit Exceeded**
```json
{
  "success": false,
  "message": "Monthly limit exceeded for Teleconsultation. Used: 15, Limit: 10, Requested: 1",
  "errorCode": "MONTHLY_LIMIT_EXCEEDED"
}
```

## 🔄 Usage Reset Logic

### **Automatic Resets**
- **Daily Limits**: Reset at midnight UTC
- **Weekly Limits**: Reset on Monday (week start)
- **Monthly Limits**: Reset on 1st of each month

### **Manual Resets**
```csharp
// Admin can manually reset usage for a user
await _privilegeService.ResetUserUsageAsync(userId, privilegeName);
```

## 📈 Benefits of This System

### **1. Simple & Flexible**
- Only 3 columns to manage complex rules
- Easy to understand and maintain
- No complex JSON configurations

### **2. Real-Time Enforcement**
- Immediate limit checking
- No race conditions
- Accurate usage tracking

### **3. Business Friendly**
- Supports any business model
- Easy to modify limits
- Clear audit trail

### **4. Scalable**
- Efficient database queries
- Indexed for performance
- Minimal overhead

## 🎉 Conclusion

This simple 3-column system provides **enterprise-grade subscription management** with **complex time-based rules** while maintaining **ease of use** and **performance**. Admins can create sophisticated subscription plans without complex configurations, and the system automatically enforces all limits in real-time.

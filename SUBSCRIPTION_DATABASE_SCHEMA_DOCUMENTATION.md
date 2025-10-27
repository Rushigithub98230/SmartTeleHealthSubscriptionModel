# 📊 SUBSCRIPTION MANAGEMENT DATABASE SCHEMA DOCUMENTATION

## 🎯 Overview

This document provides a comprehensive overview of the database schema for the **Subscription Management Module** extracted from the SmartTelehealth project. The schema is designed to support a complete subscription-based business model with billing, payments, privilege management, and lifecycle tracking.

## 📋 Database Architecture

### **Core Design Principles**
- **Clean Architecture**: Separation of concerns with clear entity relationships
- **Scalability**: Optimized indexes and efficient query patterns
- **Data Integrity**: Comprehensive foreign key constraints and validation rules
- **Audit Trail**: Complete tracking of all changes and usage patterns
- **Flexibility**: Support for plan versioning and dynamic pricing

---

## 🗂️ Table Categories

### **1. MASTER TABLES (Reference Data)**
| Table | Purpose | Key Fields |
|-------|---------|------------|
| `MasterBillingCycles` | Billing frequency options | Name, DurationInDays |
| `MasterCurrencies` | Supported currencies | Code, Name, Symbol |
| `MasterPrivilegeTypes` | Privilege categories | Name, Description |
| `MasterPaymentStatuses` | Payment status options | Name, Color |

### **2. CORE ENTITIES**
| Table | Purpose | Key Fields |
|-------|---------|------------|
| `Users` | User accounts | Email, StripeCustomerId, UserType |
| `Categories` | Plan categories | Name, ParentCategoryId |
| `Privileges` | Available privileges | Name, PrivilegeTypeId |
| `SubscriptionPlans` | Subscription offerings | Name, Price, VersionNumber, IsLatestVersion |
| `SubscriptionPlanPrivileges` | Plan-privilege mapping | Value, UnitCost, PrivilegeBaseCost |
| `Subscriptions` | User subscriptions | Status, NextBillingDate, StripeSubscriptionId |

### **3. BILLING & PAYMENT TABLES**
| Table | Purpose | Key Fields |
|-------|---------|------------|
| `BillingRecords` | All billing transactions | TotalAmount, Status, StripePaymentIntentId |
| `BillingAdjustments` | Discounts/credits | Type, Amount, IsPercentage |
| `SubscriptionPayments` | Payment details | Amount, Status, StripePaymentIntentId |
| `PaymentRefunds` | Refund tracking | Amount, StripeRefundId |
| `FailedRefunds` | Retry mechanism | RetryCount, LastErrorMessage |

### **4. PRIVILEGE USAGE TRACKING**
| Table | Purpose | Key Fields |
|-------|---------|------------|
| `UserSubscriptionPrivilegeUsages` | Current usage | UsedValue, AllowedValue, UsagePeriodStart |
| `PrivilegeUsageHistories` | Usage audit trail | UsedValue, UsedAt, UsageWeek |

### **5. LIFECYCLE & STATUS TRACKING**
| Table | Purpose | Key Fields |
|-------|---------|------------|
| `SubscriptionStatusHistories` | Status changes | PreviousStatus, NewStatus, ChangedAt |
| `ScheduledPlanMigrations` | Plan versioning | FromPlanId, ToPlanId, ScheduledMigrationDate |

### **6. SYSTEM TABLES**
| Table | Purpose | Key Fields |
|-------|---------|------------|
| `ProcessedWebhookEvents` | Stripe webhook idempotency | StripeEventId, EventType, IsSuccess |

---

## 🔗 Key Relationships

### **Subscription Lifecycle Flow**
```
Users → Subscriptions → SubscriptionPlans → SubscriptionPlanPrivileges → Privileges
  ↓           ↓              ↓
BillingRecords → SubscriptionPayments → PaymentRefunds
  ↓
UserSubscriptionPrivilegeUsages → PrivilegeUsageHistories
```

### **Plan Versioning Flow**
```
SubscriptionPlans (Parent) → SubscriptionPlans (Child Versions)
  ↓
ScheduledPlanMigrations → Subscriptions (Updated)
```

---

## 💡 Key Features

### **1. Plan Versioning System**
- **Parent-Child Relationship**: Plans can have multiple versions
- **Latest Version Flag**: `IsLatestVersion` ensures new subscriptions use current plans
- **Migration Tracking**: `ScheduledPlanMigrations` manages version transitions
- **Price Change Notices**: `PriceChangeNoticeDays` for user notifications

### **2. Privilege-Based Pricing**
- **Dynamic Pricing**: `IsAutoCalculatedPrice` enables automatic price calculation
- **Cost Components**: `PrivilegeBaseCost` + `AdminCommissionPercent/Fixed`
- **Usage Limits**: `Value` field (-1=unlimited, 0=disabled, >0=limited)
- **Overage Charges**: `UnitCost` for usage beyond limits

### **3. Comprehensive Billing System**
- **Multi-Type Billing**: Subscription, Consultation, Overage billing
- **Adjustment Support**: Discounts, credits, refunds, manual payments
- **Stripe Integration**: Full payment intent and invoice tracking
- **Failed Payment Handling**: Retry mechanism with exponential backoff

### **4. Usage Tracking & Analytics**
- **Real-Time Usage**: Current usage vs. allowed limits
- **Historical Data**: Complete audit trail of privilege consumption
- **Period-Based Tracking**: Daily, weekly, monthly usage patterns
- **Overage Detection**: Automatic identification of limit breaches

### **5. Subscription Lifecycle Management**
- **Status Tracking**: Complete history of status changes
- **Trial Support**: Trial subscriptions with automatic conversion
- **Pause/Resume**: Subscription suspension capabilities
- **Cancellation Handling**: Graceful cancellation with reason tracking

---

## 🚀 Performance Optimizations

### **Strategic Indexes**
- **User Lookups**: Email, StripeCustomerId, IsActive
- **Subscription Queries**: UserId, Status, NextBillingDate
- **Billing Operations**: UserId, Status, BillingDate, DueDate
- **Usage Tracking**: SubscriptionId, PrivilegeId, UsagePeriodStart/End
- **Webhook Processing**: StripeEventId, EventType, ReceivedAt

### **Query Optimization Views**
- **`vw_ActiveSubscriptions`**: Quick access to active subscriptions
- **`vw_SubscriptionUsageSummary`**: Usage analytics and reporting
- **`vw_BillingSummary`**: Billing overview and reconciliation

---

## 🔒 Data Integrity & Validation

### **Check Constraints**
- **Status Validation**: Enforced valid status transitions
- **Type Validation**: Proper billing and payment types
- **Decision Validation**: Valid user decisions for plan migrations

### **Foreign Key Relationships**
- **Cascade Deletes**: Proper cleanup of related records
- **Restrict Deletes**: Protection of critical reference data
- **Set Null**: Graceful handling of optional relationships

### **Unique Constraints**
- **Plan-Privilege Mapping**: Prevents duplicate privilege assignments
- **Webhook Idempotency**: Ensures Stripe events are processed once

---

## 📊 Business Logic Support

### **Subscription States**
- `Pending` → `Active` → `Paused` → `Resumed` → `Cancelled`/`Expired`
- `TrialActive` → `Active` (automatic conversion)
- `PaymentFailed` → `Suspended` → `Active` (after payment success)

### **Billing Cycles**
- **Monthly**: 30-day cycles
- **Quarterly**: 90-day cycles  
- **Annual**: 365-day cycles
- **Custom**: Configurable duration support

### **Privilege Management**
- **Unlimited**: `Value = -1`
- **Disabled**: `Value = 0`
- **Limited**: `Value > 0`
- **Overage**: Automatic billing for excess usage

---

## 🔧 Maintenance & Operations

### **Automated Processes**
- **Recurring Billing**: Background service for subscription renewals
- **Usage Reset**: Periodic reset of usage counters
- **Failed Payment Retry**: Automatic retry with exponential backoff
- **Plan Migration**: Scheduled migration to new plan versions

### **Monitoring & Alerts**
- **Failed Payments**: Automatic notification system
- **Usage Thresholds**: Alerts for approaching limits
- **System Health**: Webhook processing monitoring
- **Data Integrity**: Regular consistency checks

---

## 📈 Scalability Considerations

### **Partitioning Strategy**
- **Time-Based**: Partition by billing date for large datasets
- **User-Based**: Partition by user ID for multi-tenant scenarios
- **Status-Based**: Separate active vs. historical data

### **Archiving Strategy**
- **Historical Data**: Archive old billing records and usage history
- **Status History**: Maintain complete audit trail
- **Webhook Events**: Cleanup processed events after retention period

---

## 🛠️ Integration Points

### **Stripe Integration**
- **Customer Management**: `StripeCustomerId` in Users table
- **Subscription Sync**: `StripeSubscriptionId` in Subscriptions table
- **Payment Processing**: `StripePaymentIntentId` in billing tables
- **Webhook Handling**: `ProcessedWebhookEvents` for idempotency

### **External Systems**
- **User Management**: Integration with authentication systems
- **Notification Services**: Email/SMS for billing and usage alerts
- **Analytics Platforms**: Data export for business intelligence
- **Audit Systems**: Compliance and regulatory reporting

---

## 📋 Deployment Checklist

### **Pre-Deployment**
- [ ] Review and customize master data
- [ ] Configure Stripe webhook endpoints
- [ ] Set up monitoring and alerting
- [ ] Test data migration scripts
- [ ] Validate backup and recovery procedures

### **Post-Deployment**
- [ ] Verify all indexes are created
- [ ] Test webhook processing
- [ ] Validate billing calculations
- [ ] Monitor performance metrics
- [ ] Set up automated maintenance jobs

---

## 🔍 Troubleshooting Guide

### **Common Issues**
1. **Webhook Duplicates**: Check `ProcessedWebhookEvents` table
2. **Billing Failures**: Review `FailedRefunds` and retry mechanisms
3. **Usage Discrepancies**: Validate `UserSubscriptionPrivilegeUsages` vs. `PrivilegeUsageHistories`
4. **Plan Migration Issues**: Check `ScheduledPlanMigrations` status
5. **Performance Problems**: Analyze query execution plans and index usage

### **Data Validation Queries**
```sql
-- Check for orphaned records
SELECT COUNT(*) FROM Subscriptions s 
LEFT JOIN Users u ON s.UserId = u.Id 
WHERE u.Id IS NULL;

-- Validate usage consistency
SELECT s.Id, uspu.UsedValue, SUM(ph.UsedValue) as HistoryTotal
FROM Subscriptions s
JOIN UserSubscriptionPrivilegeUsages uspu ON s.Id = uspu.SubscriptionId
JOIN PrivilegeUsageHistories ph ON uspu.Id = ph.UserSubscriptionPrivilegeUsageId
GROUP BY s.Id, uspu.UsedValue
HAVING uspu.UsedValue != SUM(ph.UsedValue);
```

---

This schema provides a robust foundation for a complete subscription management system with enterprise-grade features for billing, payments, usage tracking, and lifecycle management.

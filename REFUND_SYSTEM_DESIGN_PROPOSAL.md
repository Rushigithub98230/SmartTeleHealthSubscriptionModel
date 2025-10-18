# 💡 SUBSCRIPTION REFUND SYSTEM - DESIGN PROPOSAL

> **⚠️ STATUS: CONCEPT/IDEA - NOT YET IMPLEMENTED**
>
> This document outlines the proposed refund system design for the SmartTeleHealth subscription platform.  
> **Implementation Status:** ❌ **NOT IMPLEMENTED**  
> **Last Updated:** October 18, 2025  
> **Author:** Development Team

---

## 📋 TABLE OF CONTENTS

1. [Overview](#overview)
2. [Business Requirements](#business-requirements)
3. [Refund Policy Rules](#refund-policy-rules)
4. [Refund Calculation Formula](#refund-calculation-formula)
5. [Implementation Architecture](#implementation-architecture)
6. [API Design](#api-design)
7. [Database Schema Changes](#database-schema-changes)
8. [Examples & Test Cases](#examples--test-cases)
9. [Implementation Checklist](#implementation-checklist)

---

## 🎯 OVERVIEW

The SmartTeleHealth platform requires a **Usage-Based Refund System** that fairly refunds users based on their actual privilege consumption while accounting for admin commission.

### Core Principles:

- ✅ **NO Grace Period** - No automatic refunds based on time
- ✅ **Usage-Based Only** - Refunds based solely on privilege consumption
- ✅ **50% Usage Threshold** - Refund only if user consumed < 50% of privileges
- ✅ **Billing Cycle Aware** - Calculate based on entire billing cycle (Monthly/Quarterly/Yearly)
- ✅ **Fair Cost Calculation** - Refund = SubscriptionFee - (UsedPrivileges × UnitCost) - ProportionalCommission
- ✅ **Admin Commission Proportional** - Refund proportional admin commission for unused services

---

## 📋 BUSINESS REQUIREMENTS

### Refund Eligibility Criteria:

```
A user is ELIGIBLE for a refund if and only if:

1. Subscription is ACTIVE or CANCELLED (not expired)
2. User has used LESS THAN 50% of total privileges in the billing cycle
3. Payment was SUCCESSFUL (no pending/failed payments)
4. Subscription plan allows refunds (IsRefundable = true)
5. No previous refund has been issued for this billing cycle
```

### Privilege Usage Calculation:

```
For Monthly Plan (10 consultations/month):
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Total in Cycle: 10 consultations
50% Threshold: 5 consultations
Eligible if Used < 5

For Quarterly Plan (10 consultations/month):
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Monthly Limit: 10
Cycle Multiplier: 3 (months)
Total in Cycle: 10 × 3 = 30 consultations
50% Threshold: 15 consultations
Eligible if Used < 15

For Yearly Plan (10 consultations/month):
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Monthly Limit: 10
Cycle Multiplier: 12 (months)
Total in Cycle: 10 × 12 = 120 consultations
50% Threshold: 60 consultations
Eligible if Used < 60
```

---

## 🔐 REFUND POLICY RULES

### Rule 1: Usage Threshold Enforcement

```
STRICT < 50% RULE:

✅ ELIGIBLE Examples:
- 0% usage → Full refund minus $0 used
- 25% usage → Refund 75% of subscription
- 40% usage → Refund 60% of subscription
- 49.99% usage → Still eligible

❌ NOT ELIGIBLE Examples:
- 50% usage → Exactly at threshold = NO REFUND
- 51% usage → Exceeded threshold = NO REFUND
- 75% usage → Significantly exceeded = NO REFUND
- 100% usage → Fully consumed = NO REFUND
```

### Rule 2: Multi-Privilege Calculation

```
When plan has MULTIPLE privileges, calculate AGGREGATE usage:

Example:
Plan has:
  - 30 Teleconsultations (in quarter)
  - 15 Medication Refills (in quarter)
  - 6 Lab Reviews (in quarter)
  
Total Units: 30 + 15 + 6 = 51 units
Threshold: 51 × 50% = 25.5 units

User Used:
  - 12 Teleconsultations
  - 8 Medication Refills
  - 4 Lab Reviews
  
Total Used: 12 + 8 + 4 = 24 units

Usage %: 24 / 51 = 47.06% < 50% ✅ ELIGIBLE

Refund Calculation:
  Teleconsultation unused: (30 - 12) × $20 = $360
  Medication unused: (15 - 8) × $10 = $70
  Lab Review unused: (6 - 4) × $25 = $50
  
  Total Unused Privilege Cost: $480
  
  Admin Commission: $600 (total privilege cost) × 10% = $60
  Proportional Commission Refund: $60 × ($480/$600) = $48
  
  TOTAL REFUND: $480 + $48 = $528
```

### Rule 3: Admin Commission Refund

```
Admin commission is refunded PROPORTIONALLY based on unused privileges:

Formula:
ProportionalCommissionRefund = AdminCommission × (UnusedCost / TotalPrivilegeCost)

Rationale:
- Admin commission covers platform operations
- User should get back commission for services NOT delivered
- Fair to both platform and user
```

---

## 🧮 REFUND CALCULATION FORMULA

### Master Formula:

```
Step 1: Calculate Total Privileges in Billing Cycle
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
For each privilege:
  MonthlyLimit = privilege.MonthlyLimit ?? privilege.Value
  CycleMultiplier = { Monthly: 1, Quarterly: 3, Yearly: 12 }
  LimitInCycle = MonthlyLimit × CycleMultiplier
  
TotalUnitsInCycle = Σ(LimitInCycle) for all privileges


Step 2: Calculate Privilege Usage
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
For each privilege:
  Used = usage.UsedValue (from UserSubscriptionPrivilegeUsage)
  
TotalUnitsUsed = Σ(Used) for all privileges
UsagePercentage = (TotalUnitsUsed / TotalUnitsInCycle) × 100


Step 3: Check Eligibility
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
IsEligible = UsagePercentage < 50%

If NOT eligible → STOP, return NO REFUND


Step 4: Calculate Used Privilege Cost
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
For each privilege:
  UsedCost = privilege.Used × privilege.UnitCost
  
TotalUsedPrivilegeCost = Σ(UsedCost) for all privileges


Step 5: Calculate Unused Privilege Cost
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
For each privilege:
  TotalCost = privilege.LimitInCycle × privilege.UnitCost
  
TotalPrivilegeCost = Σ(TotalCost) for all privileges
UnusedPrivilegeCost = TotalPrivilegeCost - TotalUsedPrivilegeCost


Step 6: Calculate Admin Commission Refund
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
If AdminCommissionPercentage exists:
  AdminCommission = TotalPrivilegeCost × (AdminCommissionPercentage / 100)
Else if AdminCommissionFixed exists:
  AdminCommission = AdminCommissionFixed

UnusedPercentage = UnusedPrivilegeCost / TotalPrivilegeCost
ProportionalAdminCommissionRefund = AdminCommission × UnusedPercentage


Step 7: Calculate Total Refund
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
RefundAmount = UnusedPrivilegeCost + ProportionalAdminCommissionRefund

Ensure non-negative:
RefundAmount = MAX(0, ROUND(RefundAmount, 2))
```

---

## 🏗️ IMPLEMENTATION ARCHITECTURE

### Proposed Architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                  USER REFUND REQUEST                        │
│              POST /api/subscriptions/refunds                │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│          SubscriptionRefundController                       │
│  - Validates request                                        │
│  - Extracts TokenModel                                      │
│  - Calls SubscriptionRefundService                          │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│          SubscriptionRefundService                          │
│                                                             │
│  1. CheckRefundEligibilityAsync()                          │
│     ├─ Get subscription & plan                             │
│     ├─ Calculate cycle multiplier                          │
│     ├─ Get privilege usage                                 │
│     ├─ Calculate usage percentage                          │
│     └─ Check < 50% threshold                               │
│                                                             │
│  2. CalculateRefundWithCommissionAsync()                   │
│     ├─ Calculate used privilege cost                       │
│     ├─ Calculate unused privilege cost                     │
│     ├─ Calculate admin commission                          │
│     ├─ Calculate proportional commission refund            │
│     └─ Return total refund amount                          │
│                                                             │
│  3. ProcessRefundAsync()                                   │
│     ├─ BEGIN TRANSACTION                                   │
│     ├─ Process Stripe refund                               │
│     ├─ Create PaymentRefund record                         │
│     ├─ Update SubscriptionPayment status                   │
│     ├─ Cancel subscription                                 │
│     ├─ Create billing adjustment audit                     │
│     ├─ COMMIT TRANSACTION                                  │
│     └─ Send confirmation email                             │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┴────────────┐
         │                        │
         ▼                        ▼
┌──────────────────┐    ┌──────────────────┐
│  StripeService   │    │  Database Layer  │
│  - Refund API    │    │  - PaymentRefund │
└──────────────────┘    │  - Subscription  │
                        │  - BillingRecord │
                        └──────────────────┘
```

---

## 📊 DETAILED EXAMPLES & TEST CASES

### **Example 1: Monthly Plan - Eligible for Refund**

```
═══════════════════════════════════════════════════════════════
PLAN CONFIGURATION
═══════════════════════════════════════════════════════════════
Plan Name: Basic Monthly
Billing Cycle: Monthly (30 days)
Privileges:
  - Teleconsultation: 10 sessions @ $20/session
  - Medication Refills: 5 refills @ $10/refill
  
Total Privilege Cost:
  (10 × $20) + (5 × $10) = $200 + $50 = $250
  
Admin Commission: 10% = $25
Total Subscription Fee: $275

═══════════════════════════════════════════════════════════════
USER USAGE (After 2 weeks)
═══════════════════════════════════════════════════════════════
Teleconsultation: 3 used (out of 10)
Medication Refills: 2 used (out of 5)

Total Units: 10 + 5 = 15 units
Used Units: 3 + 2 = 5 units
Usage %: 5 / 15 = 33.33% < 50% ✅ ELIGIBLE

═══════════════════════════════════════════════════════════════
REFUND CALCULATION
═══════════════════════════════════════════════════════════════
Used Privilege Costs:
  - Teleconsultation: 3 × $20 = $60
  - Medication: 2 × $10 = $20
  Total Used: $80

Unused Privilege Costs:
  - Teleconsultation: (10-3) × $20 = $140
  - Medication: (5-2) × $10 = $30
  Total Unused: $170

Admin Commission Breakdown:
  Total Admin Commission: $25
  Unused %: $170 / $250 = 68%
  Proportional Commission Refund: $25 × 68% = $17

TOTAL REFUND: $170 + $17 = $187

═══════════════════════════════════════════════════════════════
VERIFICATION
═══════════════════════════════════════════════════════════════
User Paid: $275
User Consumed: $80 (privileges) + $8 (commission) = $88
User Refunded: $187
Balance: $88 + $187 = $275 ✅ CORRECT
```

---

### **Example 2: Quarterly Plan - Eligible for Refund**

```
═══════════════════════════════════════════════════════════════
PLAN CONFIGURATION
═══════════════════════════════════════════════════════════════
Plan Name: Standard Quarterly
Billing Cycle: Quarterly (3 months)
Monthly Privileges:
  - Teleconsultation: 10 sessions/month @ $20/session
  - Medication Refills: 5 refills/month @ $10/refill
  
Quarterly Calculation:
  Teleconsultation: 10 × 3 = 30 sessions @ $20 = $600
  Medication: 5 × 3 = 15 refills @ $10 = $150
  
Total Privilege Cost: $750
Admin Commission: 10% = $75
Total Subscription Fee: $825

═══════════════════════════════════════════════════════════════
USER USAGE (After 6 weeks)
═══════════════════════════════════════════════════════════════
Teleconsultation: 12 used (out of 30)
Medication Refills: 6 used (out of 15)

Total Units: 30 + 15 = 45 units
Used Units: 12 + 6 = 18 units
Usage %: 18 / 45 = 40% < 50% ✅ ELIGIBLE

═══════════════════════════════════════════════════════════════
REFUND CALCULATION
═══════════════════════════════════════════════════════════════
Used Privilege Costs:
  - Teleconsultation: 12 × $20 = $240
  - Medication: 6 × $10 = $60
  Total Used: $300

Unused Privilege Costs:
  - Teleconsultation: (30-12) × $20 = $360
  - Medication: (15-6) × $10 = $90
  Total Unused: $450

Admin Commission Breakdown:
  Total Admin Commission: $75
  Unused %: $450 / $750 = 60%
  Proportional Commission Refund: $75 × 60% = $45

TOTAL REFUND: $450 + $45 = $495

═══════════════════════════════════════════════════════════════
VERIFICATION
═══════════════════════════════════════════════════════════════
User Paid: $825
User Consumed: $300 (privileges) + $30 (commission) = $330
User Refunded: $495
Balance: $330 + $495 = $825 ✅ CORRECT
```

---

### **Example 3: Monthly Plan - NOT Eligible**

```
═══════════════════════════════════════════════════════════════
PLAN CONFIGURATION
═══════════════════════════════════════════════════════════════
Plan Name: Premium Monthly
Billing Cycle: Monthly
Privileges:
  - Teleconsultation: 20 sessions @ $15/session = $300
  
Admin Commission: 15% = $45
Total Subscription Fee: $345

═══════════════════════════════════════════════════════════════
USER USAGE
═══════════════════════════════════════════════════════════════
Teleconsultation: 11 used (out of 20)

Usage %: 11 / 20 = 55% > 50% ❌ NOT ELIGIBLE

═══════════════════════════════════════════════════════════════
REFUND RESULT
═══════════════════════════════════════════════════════════════
Eligibility: ❌ NOT ELIGIBLE
Reason: User consumed 55% of privileges, exceeding 50% threshold
Refund Amount: $0.00
Message: "Not eligible for refund. You have used 55% of your 
         privileges, which exceeds the 50% threshold."
```

---

### **Example 4: Edge Case - Exactly 50% Usage**

```
═══════════════════════════════════════════════════════════════
SCENARIO
═══════════════════════════════════════════════════════════════
Total Privileges: 20 units
Used: 10 units
Usage %: 10 / 20 = 50.00%

═══════════════════════════════════════════════════════════════
DECISION
═══════════════════════════════════════════════════════════════
Condition: UsagePercentage < 50%
Check: 50.00% < 50% → FALSE ❌

Result: NOT ELIGIBLE (must be strictly LESS THAN 50%)
```

---

## 💾 DATABASE SCHEMA CHANGES

### **1. Add Refund Fields to SubscriptionPlan**

```sql
-- Migration: AddRefundPolicyToSubscriptionPlan

ALTER TABLE SubscriptionPlans ADD COLUMN IsRefundable BIT NOT NULL DEFAULT 1;
ALTER TABLE SubscriptionPlans ADD COLUMN RefundUsageThresholdPercentage DECIMAL(5,2) NOT NULL DEFAULT 50.00;
ALTER TABLE SubscriptionPlans ADD COLUMN RefundPolicyNotes NVARCHAR(500) NULL;
ALTER TABLE SubscriptionPlans ADD COLUMN RequireAdminApprovalForRefund BIT NOT NULL DEFAULT 0;
ALTER TABLE SubscriptionPlans ADD COLUMN MinimumDaysBeforeRefundEligible INT NOT NULL DEFAULT 0;

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Maximum privilege usage percentage to qualify for refund (default: 50%)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'SubscriptionPlans',
    @level2type = N'COLUMN', @level2name = N'RefundUsageThresholdPercentage';
```

### **2. Enhance BillingRecord for Audit Trail**

```sql
-- Migration: EnhanceBillingRecordWithRefundDetails

ALTER TABLE BillingRecords ADD COLUMN OriginalAmount DECIMAL(18,2) NULL;
ALTER TABLE BillingRecords ADD COLUMN DiscountAmount DECIMAL(18,2) NULL;
ALTER TABLE BillingRecords ADD COLUMN DiscountType NVARCHAR(50) NULL;
ALTER TABLE BillingRecords ADD COLUMN PromoCodeUsed NVARCHAR(50) NULL;
ALTER TABLE BillingRecords ADD COLUMN DiscountAppliedDate DATETIME2 NULL;
ALTER TABLE BillingRecords ADD COLUMN WasDiscountValid BIT NOT NULL DEFAULT 1;

-- Add index for refund queries
CREATE INDEX IX_BillingRecords_Type_Status ON BillingRecords(Type, Status);
```

### **3. PaymentRefund Table** (Already Exists ✅)

```sql
-- Table: PaymentRefunds (EXISTING)

CREATE TABLE PaymentRefunds (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SubscriptionPaymentId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Reason NVARCHAR(500) NOT NULL,
    StripeRefundId NVARCHAR(100) NULL,
    RefundedAt DATETIME2 NOT NULL,
    ProcessedByUserId INT NULL,
    
    -- Audit fields
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 NOT NULL,
    UpdatedBy INT NULL,
    UpdatedDate DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    FOREIGN KEY (SubscriptionPaymentId) REFERENCES SubscriptionPayments(Id),
    FOREIGN KEY (ProcessedByUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_PaymentRefunds_SubscriptionPaymentId 
    ON PaymentRefunds(SubscriptionPaymentId);
```

---

## 🔌 API DESIGN

### **Endpoint 1: Check Refund Eligibility**

```http
GET /api/subscriptions/refunds/eligibility/{subscriptionId}

Headers:
  Authorization: Bearer {token}

Response 200 OK:
{
  "data": {
    "isEligible": true,
    "eligibilityMessage": "✅ Eligible for refund. You have used 40.00% of your privileges (threshold: 50%).",
    
    // Usage Metrics
    "totalPrivilegesInCycle": 30,
    "privilegesUsed": 12,
    "usagePercentage": 40.00,
    "usageThreshold": 50.00,
    
    // Financial Details
    "totalPrivilegeCost": 600.00,
    "usedPrivilegeCost": 240.00,
    "unusedPrivilegeCost": 360.00,
    "adminCommission": 60.00,
    "adminCommissionType": "Percentage",
    "adminCommissionRate": 10.00,
    "proportionalAdminCommissionRefund": 36.00,
    "totalSubscriptionFee": 660.00,
    "refundAmount": 396.00,
    
    // Timeline
    "subscriptionStartDate": "2025-09-01T00:00:00Z",
    "refundRequestDate": "2025-10-18T06:30:00Z",
    "daysSinceStart": 47,
    
    // Privilege Breakdown
    "privilegeUsageDetails": [
      {
        "privilegeName": "Teleconsultation",
        "limitInCycle": 30,
        "used": 12,
        "unitCost": 20.00,
        "totalCost": 240.00,
        "usagePercentage": 40.00
      }
    ],
    
    // Calculation Explanation
    "refundCalculationExplanation": "..."
  },
  "message": "Eligible for refund",
  "statusCode": 200
}

Response 200 OK (NOT Eligible):
{
  "data": {
    "isEligible": false,
    "eligibilityMessage": "❌ Not eligible for refund. You have used 55.00% of your privileges, which exceeds the 50% threshold.",
    "usagePercentage": 55.00,
    "refundAmount": 0.00,
    ...
  },
  "message": "Not eligible for refund",
  "statusCode": 200
}
```

### **Endpoint 2: Request Refund**

```http
POST /api/subscriptions/refunds

Headers:
  Authorization: Bearer {token}
  Content-Type: application/json

Body:
{
  "subscriptionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reason": "Service did not meet my expectations"
}

Response 200 OK (Success):
{
  "data": {
    "refundId": "9c7b5f21-8a34-4d62-a1fc-5e874b89cde3",
    "subscriptionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "success": true,
    "message": "Refund processed successfully",
    "refundAmount": 396.00,
    "stripeRefundId": "re_1K4p2qLvD9z8xY3m",
    "processedAt": "2025-10-18T06:35:00Z",
    "status": "Processed",
    "eligibilityDetails": { ... }
  },
  "message": "Refund processed successfully",
  "statusCode": 200
}

Response 400 Bad Request (Not Eligible):
{
  "data": {
    "success": false,
    "message": "Not eligible for refund. You have used 55% of your privileges.",
    "status": "Denied",
    "refundAmount": 0.00,
    "eligibilityDetails": { ... }
  },
  "message": "Refund request denied - not eligible",
  "statusCode": 400
}
```

### **Endpoint 3: Get Refund History** (Proposed)

```http
GET /api/subscriptions/{subscriptionId}/refunds

Response 200 OK:
{
  "data": [
    {
      "refundId": "guid",
      "amount": 396.00,
      "reason": "Service did not meet expectations",
      "refundedAt": "2025-10-18T06:35:00Z",
      "stripeRefundId": "re_1K4p2qLvD9z8xY3m",
      "processedBy": "Admin Name"
    }
  ],
  "message": "Refund history retrieved",
  "statusCode": 200
}
```

---

## 📦 PROPOSED FILE STRUCTURE

```
backend/
├── SmartTelehealth.Application/
│   ├── Services/
│   │   └── SubscriptionRefundService.cs ⭐ NEW
│   ├── Interfaces/
│   │   └── ISubscriptionRefundService.cs ⭐ NEW
│   └── DTOs/
│       ├── RequestSubscriptionRefundDto.cs ⭐ NEW
│       ├── RefundEligibilityDto.cs ⭐ NEW
│       ├── SubscriptionRefundResponseDto.cs ⭐ NEW
│       └── PrivilegeUsageBreakdown.cs ⭐ NEW
│
├── SmartTelehealth.Core/
│   ├── Entities/
│   │   ├── SubscriptionPlan.cs (MODIFY - Add refund fields)
│   │   ├── BillingRecord.cs (MODIFY - Add discount tracking)
│   │   └── PaymentRefund.cs ✅ EXISTS
│   └── Interfaces/
│       └── ISubscriptionPaymentRepository.cs (MODIFY - Add refund methods)
│
├── SmartTelehealth.Infrastructure/
│   ├── Repositories/
│   │   └── SubscriptionPaymentRepository.cs (MODIFY - Implement refund methods)
│   └── Migrations/
│       ├── AddRefundPolicyToSubscriptionPlan.cs ⭐ NEW
│       └── EnhanceBillingRecordWithRefundDetails.cs ⭐ NEW
│
└── SmartTelehealth.API/
    └── Controllers/
        └── SubscriptionRefundController.cs ⭐ NEW
```

---

## 🎯 IMPLEMENTATION CHECKLIST

### **Phase 1: Database Schema** (Estimated: 1-2 hours)
- [ ] Create migration for `SubscriptionPlan` refund fields
- [ ] Create migration for `BillingRecord` enhancement
- [ ] Update `SubscriptionPaymentRepository` with refund methods
- [ ] Test migrations locally
- [ ] Verify foreign key constraints

### **Phase 2: Core Business Logic** (Estimated: 1 day)
- [ ] Create `SubscriptionRefundService.cs`
- [ ] Implement `CheckRefundEligibilityAsync()`
- [ ] Implement `CalculateRefundWithCommissionAsync()`
- [ ] Implement `ProcessRefundAsync()`
- [ ] Add helper method for cycle multiplier
- [ ] Add helper method for refund explanation builder
- [ ] Create all DTOs (Request, Response, Eligibility, Breakdown)

### **Phase 3: Stripe Integration** (Estimated: 4-6 hours)
- [ ] Verify `StripeService.ProcessRefundAsync()` exists and works
- [ ] Test Stripe refund API integration
- [ ] Handle Stripe refund failures gracefully
- [ ] Add refund webhook handling (optional)
- [ ] Test partial vs full refunds

### **Phase 4: API Layer** (Estimated: 2-3 hours)
- [ ] Create `SubscriptionRefundController.cs`
- [ ] Add eligibility check endpoint
- [ ] Add refund request endpoint
- [ ] Add refund history endpoint (optional)
- [ ] Implement authorization checks
- [ ] Add input validation
- [ ] Add API documentation/Swagger annotations

### **Phase 5: Notifications** (Estimated: 2-3 hours)
- [ ] Create refund confirmation email template
- [ ] Create refund denial email template
- [ ] Implement `SendRefundConfirmationEmailAsync()`
- [ ] Test email delivery
- [ ] Add SMS notification (optional)

### **Phase 6: Testing** (Estimated: 1 day)
- [ ] Unit tests for refund calculation formulas
- [ ] Unit tests for eligibility logic
- [ ] Unit tests for admin commission calculation
- [ ] Integration tests for Stripe refund
- [ ] Test quarterly plan scenarios
- [ ] Test yearly plan scenarios
- [ ] Test edge cases (exactly 50%, 0% usage, 100% usage)
- [ ] Test multi-privilege plans
- [ ] Test transaction rollback on failure

### **Phase 7: Documentation** (Estimated: 2-3 hours)
- [ ] Update API documentation
- [ ] Create admin guide for refund processing
- [ ] Create user-facing refund policy page
- [ ] Document refund calculation examples
- [ ] Add troubleshooting guide

---

## 📐 REFUND CALCULATION PSEUDOCODE

```
FUNCTION CalculateRefund(subscriptionId):
  
  // Get subscription data
  subscription = GetSubscription(subscriptionId)
  plan = subscription.SubscriptionPlan
  billingCycle = subscription.BillingCycle
  
  // Determine cycle multiplier
  cycleMultiplier = SWITCH billingCycle.Name:
    CASE "monthly": 1
    CASE "quarterly": 3
    CASE "yearly" OR "annual": 12
    DEFAULT: 1
  
  // Initialize counters
  totalUnitsInCycle = 0
  usedUnits = 0
  totalPrivilegeCost = 0
  usedPrivilegeCost = 0
  
  // Calculate for each privilege
  FOR EACH privilege IN plan.PlanPrivileges:
    monthlyLimit = privilege.MonthlyLimit ?? privilege.Value
    
    IF monthlyLimit <= 0 THEN CONTINUE // Skip unlimited/disabled
    
    limitInCycle = CEILING(monthlyLimit × cycleMultiplier)
    used = GetUsage(subscriptionId, privilege.Id)
    unitCost = privilege.UnitCost
    
    totalUnitsInCycle += limitInCycle
    usedUnits += used
    totalPrivilegeCost += (limitInCycle × unitCost)
    usedPrivilegeCost += (used × unitCost)
  
  // Calculate usage percentage
  usagePercentage = (usedUnits / totalUnitsInCycle) × 100
  
  // Check eligibility
  IF usagePercentage >= 50 THEN
    RETURN {
      isEligible: FALSE,
      refundAmount: 0,
      message: "Used " + usagePercentage + "% (threshold: 50%)"
    }
  
  // Calculate unused privilege cost
  unusedPrivilegeCost = totalPrivilegeCost - usedPrivilegeCost
  
  // Calculate admin commission
  IF plan.AdminCommissionPercentage > 0 THEN
    adminCommission = totalPrivilegeCost × (plan.AdminCommissionPercentage / 100)
  ELSE IF plan.AdminCommissionFixed EXISTS THEN
    adminCommission = plan.AdminCommissionFixed
  ELSE
    adminCommission = 0
  
  // Calculate proportional commission refund
  unusedPercentage = unusedPrivilegeCost / totalPrivilegeCost
  proportionalCommissionRefund = adminCommission × unusedPercentage
  
  // Calculate total refund
  refundAmount = unusedPrivilegeCost + proportionalCommissionRefund
  refundAmount = MAX(0, ROUND(refundAmount, 2))
  
  RETURN {
    isEligible: TRUE,
    refundAmount: refundAmount,
    usagePercentage: usagePercentage,
    breakdown: { ... }
  }
```

---

## 🔍 EDGE CASES TO HANDLE

### **Edge Case 1: Mixed Privilege Usage**

```
Scenario:
  - Privilege A: 90% used (9/10)
  - Privilege B: 10% used (1/10)
  - Overall: (9+1)/(10+10) = 50% ❌

Decision: NOT ELIGIBLE (aggregate must be < 50%)
```

### **Edge Case 2: Zero Usage**

```
Scenario: User subscribed but never used any privilege

Usage: 0%
Eligible: YES ✅
Refund: Full subscription fee (100%)
```

### **Edge Case 3: Unlimited Privileges**

```
Scenario: Plan has 1 unlimited privilege (Value = -1)

Solution: Skip unlimited privileges in refund calculation
Only count limited privileges
```

### **Edge Case 4: Purchased Extra Credits**

```
Scenario:
  - Plan limit: 10
  - User purchased +2 extra credits
  - AllowedValue = 12
  - User used: 6

Question: Calculate against 10 or 12?

Decision: Calculate against PLAN LIMIT (10), not AllowedValue
Reason: Extra credits were paid separately, shouldn't affect refund
  
Usage: 6 / 10 = 60% ❌ NOT ELIGIBLE
```

### **Edge Case 5: Admin Commission = 0**

```
Scenario: Plan has no admin commission

Solution:
  - AdminCommission = 0
  - ProportionalCommissionRefund = 0
  - Refund = UnusedPrivilegeCost only
```

### **Edge Case 6: Refund After Privilege Reset**

```
Scenario: User requests refund after monthly cycle renewed

Solution:
  - Only consider CURRENT cycle's usage
  - Previous cycle usage doesn't count
  - Reset happened → usage should be 0 or low
```

---

## 📊 SAMPLE REFUND SCENARIOS TABLE

| Plan Type | Monthly Limit | Cycle | Total Units | Used | Usage % | Threshold | Eligible? | Refund Amount |
|-----------|---------------|-------|-------------|------|---------|-----------|-----------|---------------|
| Basic Monthly | 10 | 1 | 10 | 3 | 30% | 50% | ✅ YES | $140 + $14 = $154 |
| Basic Monthly | 10 | 1 | 10 | 5 | 50% | 50% | ❌ NO | $0 |
| Basic Monthly | 10 | 1 | 10 | 4 | 40% | 50% | ✅ YES | $120 + $12 = $132 |
| Standard Quarterly | 10 | 3 | 30 | 12 | 40% | 50% | ✅ YES | $360 + $36 = $396 |
| Standard Quarterly | 10 | 3 | 30 | 15 | 50% | 50% | ❌ NO | $0 |
| Standard Quarterly | 10 | 3 | 30 | 14 | 46.67% | 50% | ✅ YES | $320 + $32 = $352 |
| Premium Yearly | 10 | 12 | 120 | 50 | 41.67% | 50% | ✅ YES | $1,400 + $140 = $1,540 |
| Premium Yearly | 10 | 12 | 120 | 60 | 50% | 50% | ❌ NO | $0 |

*Assumes $20/unit, 10% admin commission*

---

## 🔐 SECURITY & VALIDATION

### **Validations to Implement:**

```csharp
1. User Ownership Validation
   - User can only refund their own subscriptions
   - Admin can refund any subscription
   - Token validation

2. Subscription Status Validation
   - Cannot refund already-refunded subscription
   - Cannot double-refund same payment
   - Check IsCancelled flag

3. Payment Status Validation
   - Payment must be "Succeeded"
   - Payment amount must be > 0
   - Stripe payment intent must exist

4. Amount Validation
   - Refund amount <= paid amount
   - Refund amount >= 0
   - No negative refunds

5. Idempotency
   - Prevent duplicate refund requests
   - Check if refund already processed
   - Use unique refund IDs
```

---

## 🎨 USER INTERFACE MOCKUP

### **Refund Request Page (Frontend Concept):**

```
╔══════════════════════════════════════════════════════════════╗
║              REQUEST SUBSCRIPTION REFUND                     ║
╚══════════════════════════════════════════════════════════════╝

Subscription: Premium Quarterly Plan
Status: Active
Next Billing: November 1, 2025

┌──────────────────────────────────────────────────────────────┐
│ REFUND ELIGIBILITY CHECK                                     │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│ ✅ You are eligible for a refund!                           │
│                                                              │
│ Privilege Usage Summary:                                     │
│ ┌────────────────────────────────────────────────┐          │
│ │ [████████░░░░░░░░░░░░░░░░░░░░░░] 40.00%       │          │
│ │ Used 12 of 30 consultations (Threshold: 50%)   │          │
│ └────────────────────────────────────────────────┘          │
│                                                              │
│ Financial Breakdown:                                         │
│   Total Subscription Fee:        $660.00                     │
│   Used Services Cost:            $240.00                     │
│   Unused Services Cost:          $360.00                     │
│   Admin Commission Refund:        $36.00                     │
│   ─────────────────────────────────────                     │
│   TOTAL REFUND:                  $396.00                     │
│                                                              │
│ Processing Time: 5-10 business days                         │
└──────────────────────────────────────────────────────────────┘

Reason for Refund:
┌──────────────────────────────────────────────────────────────┐
│ [Service did not meet my expectations                     ]  │
│ [                                                          ]  │
│ [                                                          ]  │
└──────────────────────────────────────────────────────────────┘
(Required, max 500 characters)

[ ] I understand that requesting a refund will cancel my subscription

                    [Cancel]  [Submit Refund Request]
```

---

## ⚖️ BUSINESS RULES SUMMARY

```
┌─────────────────────────────────────────────────────────────┐
│ REFUND POLICY - SMARTTELEHEALTH SUBSCRIPTION PLATFORM      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ 1. ELIGIBILITY REQUIREMENT:                                 │
│    → Must have used LESS THAN 50% of privileges             │
│    → Calculated across ALL privileges in billing cycle      │
│                                                             │
│ 2. CALCULATION METHOD:                                      │
│    → Refund = SubscriptionFee - UsedServices                │
│    → UsedServices = Σ(UsedQty × UnitCost) + PropCommission │
│    → Proportional admin commission refunded for unused      │
│                                                             │
│ 3. BILLING CYCLE AWARENESS:                                 │
│    → Monthly: Calculate against 1x monthly limit            │
│    → Quarterly: Calculate against 3x monthly limit          │
│    → Yearly: Calculate against 12x monthly limit            │
│                                                             │
│ 4. NO GRACE PERIOD:                                         │
│    → No automatic 7-day or 30-day full refund               │
│    → Usage-based calculation from day 1                     │
│                                                             │
│ 5. TRANSACTION SAFETY:                                      │
│    → ACID-compliant refund processing                       │
│    → Rollback on Stripe failure                             │
│    → All-or-nothing refund                                  │
│                                                             │
│ 6. AUDIT TRAIL:                                             │
│    → All refunds logged in PaymentRefund table              │
│    → Billing adjustment created for audit                   │
│    → Email confirmation sent to user                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 REFUND WORKFLOW DIAGRAM

```
USER REFUND REQUEST FLOW:
════════════════════════════════════════════════════════════════

┌─────────┐
│  USER   │ Requests refund via API
└────┬────┘
     │
     ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 1: ELIGIBILITY CHECK                                   │
│                                                             │
│  ✓ Get subscription details                                │
│  ✓ Get billing cycle (Monthly/Quarterly/Yearly)            │
│  ✓ Calculate total privileges in cycle                     │
│    → MonthlyLimit × CycleMultiplier                        │
│  ✓ Get user's privilege usage                              │
│  ✓ Calculate usage percentage                              │
│    → (UsedUnits / TotalUnits) × 100                        │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
              ┌───────────────┐
              │  Usage < 50%? │
              └───┬───────┬───┘
                  │       │
            YES ✅│       │❌ NO
                  │       │
                  │       └──────────────────┐
                  │                          │
                  ▼                          ▼
┌─────────────────────────────────┐  ┌──────────────────┐
│ STEP 2: CALCULATE REFUND AMOUNT │  │  DENY REFUND     │
│                                 │  │                  │
│ A. Used Privilege Cost:         │  │ Return:          │
│    Σ(Used × UnitCost)          │  │  - isEligible:   │
│                                 │  │    false         │
│ B. Unused Privilege Cost:       │  │  - refundAmount: │
│    Σ((Limit-Used) × UnitCost)  │  │    0             │
│                                 │  │  - message:      │
│ C. Admin Commission:            │  │    "Exceeded     │
│    TotalPrivCost × CommRate     │  │    threshold"    │
│                                 │  └──────────────────┘
│ D. Proportional Commission:     │
│    AdminComm × (Unused/Total)   │
│                                 │
│ E. Total Refund:                │
│    UnusedCost + PropComm        │
└─────────────────┬───────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 3: PROCESS REFUND                                      │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ BEGIN TRANSACTION                                   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  1. Process Stripe Refund                                   │
│     → Call Stripe API with payment intent & amount          │
│     → If fails → ROLLBACK transaction                       │
│                                                             │
│  2. Create PaymentRefund Record                             │
│     → Link to SubscriptionPayment                           │
│     → Store amount, reason, Stripe refund ID                │
│                                                             │
│  3. Update SubscriptionPayment Status                       │
│     → Status = "Refunded" or "PartiallyRefunded"           │
│                                                             │
│  4. Cancel Subscription                                     │
│     → IsCancelled = true                                    │
│     → Status = "Cancelled"                                  │
│     → CancellationReason = "Refunded: {reason}"            │
│                                                             │
│  5. Create Billing Adjustment (Audit)                       │
│     → Type = "Refund"                                       │
│     → Amount = refund amount                                │
│     → Link to billing record                                │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ COMMIT TRANSACTION                                  │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  6. Send Confirmation Email                                 │
│     → Include refund amount                                 │
│     → Include processing timeline                           │
│     → Include usage breakdown                               │
└─────────────────────────────────────────────────────────────┘
                      │
                      ▼
              ┌───────────────┐
              │   SUCCESS     │
              │  Refund: $XXX │
              └───────────────┘
```

---

## 🧪 TESTING SCENARIOS

### **Test Case 1: Basic Monthly - Eligible**
```
Input:
  Plan: Basic Monthly ($100, 10 consultations @ $10, 10% commission)
  Used: 4 consultations
  
Expected:
  Eligible: true
  Usage: 40%
  Refund: $60 (unused) + $6 (commission) = $66
  
Assertion:
  ASSERT UsagePercentage == 40.00
  ASSERT IsEligible == true
  ASSERT RefundAmount == 66.00
```

### **Test Case 2: Quarterly - Not Eligible**
```
Input:
  Plan: Standard Quarterly ($660, 30 consultations @ $20, 10% commission)
  Used: 16 consultations
  
Expected:
  Eligible: false
  Usage: 53.33%
  Refund: $0
  
Assertion:
  ASSERT UsagePercentage == 53.33
  ASSERT IsEligible == false
  ASSERT RefundAmount == 0.00
```

### **Test Case 3: Edge - Exactly 50%**
```
Input:
  Plan: Basic Monthly ($100, 10 units)
  Used: 5 units
  
Expected:
  Eligible: false (must be < 50%, not <=)
  Usage: 50%
  Refund: $0
  
Assertion:
  ASSERT UsagePercentage == 50.00
  ASSERT IsEligible == false
  ASSERT RefundAmount == 0.00
```

### **Test Case 4: Zero Usage**
```
Input:
  Plan: Premium Yearly ($1,200, 120 units @ $10)
  Used: 0 units
  
Expected:
  Eligible: true
  Usage: 0%
  Refund: $1,200 (full amount)
  
Assertion:
  ASSERT UsagePercentage == 0.00
  ASSERT IsEligible == true
  ASSERT RefundAmount == 1200.00
```

### **Test Case 5: Multi-Privilege Plan**
```
Input:
  Plan: Multi Quarterly
  - Privilege A: 30 units, 12 used
  - Privilege B: 15 units, 6 used
  Total: 45 units, 18 used
  
Expected:
  Eligible: true
  Usage: 40%
  Refund: Calculated based on both privileges
  
Assertion:
  ASSERT TotalUnitsInCycle == 45
  ASSERT UsedUnits == 18
  ASSERT IsEligible == true
```

---

## 🚀 IMPLEMENTATION TIMELINE

### **Estimated Development Effort:**

| Phase | Tasks | Time | Dependencies |
|-------|-------|------|--------------|
| **Phase 1** | Database Migration | 2 hours | None |
| **Phase 2** | Core Service Logic | 8 hours | Phase 1 |
| **Phase 3** | Stripe Integration | 4 hours | Phase 2 |
| **Phase 4** | API Controllers | 3 hours | Phase 2 |
| **Phase 5** | Email Notifications | 3 hours | Phase 2 |
| **Phase 6** | Unit Testing | 8 hours | Phase 2-4 |
| **Phase 7** | Integration Testing | 4 hours | All |
| **Phase 8** | Documentation | 2 hours | All |
| **TOTAL** | - | **34 hours** (~4-5 days) | - |

---

## 📝 NOTES & CONSIDERATIONS

### **Important Design Decisions:**

1. **Why No Grace Period?**
   - Prevents abuse (sign up, use 1 session, refund)
   - Fair to platform operations
   - Usage-based is more transparent

2. **Why 50% Threshold?**
   - Industry standard for subscription services
   - Balances user fairness with business viability
   - Clear, objective metric (no subjective evaluation)

3. **Why Proportional Admin Commission?**
   - Fair to both parties
   - User shouldn't pay commission for services not delivered
   - Mathematically sound

4. **Why Cancel Subscription on Refund?**
   - Prevents continued access after refund
   - Clearly defined termination point
   - Industry best practice

### **Future Enhancements (Post-MVP):**

```
🔮 FUTURE IDEAS:

1. Partial Refund Without Cancellation
   - User gets prorated refund but keeps subscription
   - Useful for goodwill gestures
   
2. Store Credit Option
   - Instead of cash refund, offer 110% store credit
   - Can be used for future subscriptions
   
3. Refund to Different Payment Method
   - Bank transfer option
   - Crypto refund (future)
   
4. Automated Refund Approvals
   - AI-based fraud detection
   - Auto-approve low-risk refunds < $50
   
5. Refund Analytics Dashboard
   - Track refund rates by plan
   - Identify patterns
   - Improve service quality
```

---

## ⚠️ KNOWN LIMITATIONS (Current Design)

```
1. No Time-Based Refund Window
   - Current design: Pure usage-based
   - Limitation: User could request refund after 11 months if usage < 50%
   - Mitigation: Consider adding maximum days limit in Phase 2

2. No Partial Refund Tiers
   - Current design: Binary (eligible vs not)
   - Limitation: No graduated refund (e.g., 70% at <25% usage, 50% at 25-50%)
   - Mitigation: Can be added as custom policy type

3. No Admin Approval Workflow
   - Current design: Auto-process if eligible
   - Limitation: No manual review for high-value refunds
   - Mitigation: Add RequireAdminApprovalForRefund flag

4. Purchased Extra Credits Not Considered
   - Current design: Only calculates against plan limits
   - Limitation: Extra purchased credits ignored
   - Rationale: Extra credits were separate transactions
```

---

## 📞 STAKEHOLDER SIGN-OFF

**Approval Required From:**
- [ ] Product Owner
- [ ] Finance Team
- [ ] Legal/Compliance Team
- [ ] Customer Support Team
- [ ] Development Team Lead

**Approval Status:** ⏳ **PENDING REVIEW**

---

## 🔗 RELATED DOCUMENTS

- [Privilege Management Complete Guide](./PRIVILEGE_MANAGEMENT_COMPLETE_GUIDE.md)
- [Subscription Billing Walkthrough](./docs/SUBSCRIPTION_BILLING_WALKTHROUGH.md)
- [Stripe Integration Documentation](./docs/STRIPE_INTEGRATION.md) (if exists)

---

## 📧 CONTACT

For questions about this refund system design proposal, contact:
- Development Team: [team@smarttelehealth.com]
- Product Owner: [product@smarttelehealth.com]

---

**END OF DOCUMENT**

*This is a design proposal and implementation plan. No code has been implemented yet.*  
*Implementation requires approval from stakeholders and dedicated development sprint.*


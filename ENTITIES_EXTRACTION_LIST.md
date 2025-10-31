# Complete Entities List for Subscription Management Extraction

## 📋 Complete Entity Extraction List

This document provides the **exact list of all entities** that need to be extracted from the SmartTelehealth codebase to create a standalone subscription management module.

---

## ✅ Required Entities (24 Total)

### 🔴 CRITICAL - Base Entity (1 file)

#### 1. BaseEntity.cs
**Source:** `backend/SmartTelehealth.Core/Entities/BaseEntity.cs`  
**Purpose:** Base class with common audit fields  
**Required By:** ALL entities  
**Priority:** 🔴 **MUST EXTRACT FIRST**

---

### 🔴 CRITICAL - Master Data Entities (5 files from MasterTables.cs)

Extract these 5 classes from `backend/SmartTelehealth.Core/Entities/MasterTables.cs`:

#### 2. MasterBillingCycle
**Purpose:** Billing cycle definitions (Monthly, Quarterly, Annual)  
**Key Properties:** Id, Name, DurationInDays, SortOrder  
**Used By:** SubscriptionPlan, Subscription  

#### 3. MasterCurrency
**Purpose:** Currency definitions (USD, EUR, GBP, etc.)  
**Key Properties:** Id, Code, Name, Symbol, SortOrder  
**Used By:** SubscriptionPlan, BillingRecord, SubscriptionPayment  

#### 4. MasterPrivilegeType
**Purpose:** Privilege type categorization  
**Key Properties:** Id, Name, Description, SortOrder  
**Used By:** Privilege  

#### 5. PaymentStatus
**Purpose:** Payment status lookup table  
**Key Properties:** Id, Name, Description, SortOrder, Color  
**Used By:** Payment tracking  

#### 6. RefundStatus
**Purpose:** Refund status lookup table  
**Key Properties:** Id, Name, Description, SortOrder, Color  
**Used By:** PaymentRefund  

**Note:** Extract ONLY these 5 master tables. Skip others like AppointmentStatus, ConsultationMode, etc.

---

### 🟡 HIGH - Subscription Plan Entities (2 files)

#### 7. SubscriptionPlan.cs
**Source:** `backend/SmartTelehealth.Core/Entities/SubscriptionPlan.cs`  
**Purpose:** Subscription plan template  
**Key Properties:** 
- Id, Name, Description, BasePrice
- PlanType, BillingCycleId, CurrencyId, CategoryId
- IsAutoCalculatedPrice, AdminCommissionPercent
- VersionNumber, IsLatestVersion, ParentPlanId
- StripeProductId, StripePriceId

#### 8. SubscriptionPlanPrivilege.cs
**Source:** `backend/SmartTelehealth.Core/Entities/SubscriptionPlanPrivilege.cs`  
**Purpose:** Junction entity (Plan ↔ Privilege)  
**Key Properties:** 
- SubscriptionPlanId, PrivilegeId
- Value (usage limit), PrivilegeBaseCost, UnitCost

---

### 🟡 HIGH - Subscription Entities (3 files)

#### 9. Subscription.cs
**Source:** `backend/SmartTelehealth.Core/Entities/Subscription.cs`  
**Purpose:** User subscription instance  
**Key Properties:**
- Id, UserId, SubscriptionPlanId, Status
- StartDate, EndDate, NextBillingDate, CurrentPrice
- AutoRenew, IsTrialSubscription, TrialEndDate
- StripeSubscriptionId, StripeCustomerId, StripePriceId
- PendingPlanChangeId, PlanChangeEffectiveDate

#### 10. SubscriptionPayment.cs
**Source:** `backend/SmartTelehealth.Core/Entities/SubscriptionPayment.cs`  
**Purpose:** Subscription-specific payment tracking  
**Key Properties:**
- Id, SubscriptionId, BillingRecordId
- Amount, TaxAmount, NetAmount
- Status (enum), Type (enum), DueDate
- BillingPeriodStart, BillingPeriodEnd
- StripePaymentIntentId, StripeInvoiceId
- AttemptCount, NextRetryAt

#### 11. SubscriptionStatusHistory.cs
**Source:** `backend/SmartTelehealth.Core/Entities/SubscriptionStatusHistory.cs`  
**Purpose:** Subscription status change tracking  
**Key Properties:** Id, SubscriptionId, Status, ChangedAt, Reason

---

### 🟡 HIGH - Privilege Entities (3 files)

#### 12. Privilege.cs
**Source:** `backend/SmartTelehealth.Core/Entities/Privilege.cs`  
**Purpose:** Privilege definitions  
**Key Properties:** Id, Name, Description, PrivilegeTypeId

#### 13. UserSubscriptionPrivilegeUsage.cs
**Source:** `backend/SmartTelehealth.Core/Entities/UserSubscriptionPrivilegeUsage.cs`  
**Purpose:** User privilege usage tracking  
**Key Properties:**
- Id, SubscriptionId, SubscriptionPlanPrivilegeId, PrivilegeId
- UsedValue, AllowedValue
- UsagePeriodStart, UsagePeriodEnd, LastUsedAt, ResetAt

#### 14. PrivilegeUsageHistory.cs
**Source:** `backend/SmartTelehealth.Core/Entities/PrivilegeUsageHistory.cs`  
**Purpose:** Historical privilege usage records  
**Key Properties:** 
- Id, UserSubscriptionPrivilegeUsageId
- UsedValue, UsedAt, UsageDate, UsageWeek, UsageMonth

---

### 🟡 HIGH - Billing Entities (4 files)

#### 15. BillingRecord.cs
**Source:** `backend/SmartTelehealth.Core/Entities/BillingRecord.cs`  
**Purpose:** Master billing records  
**Key Properties:**
- Id, UserId, SubscriptionId, CurrencyId
- Status (enum), Type (enum)
- Amount, TaxAmount, ShippingAmount, TotalAmount
- BillingDate, PaidAt, DueDate
- StripePaymentIntentId, StripeInvoiceId

#### 16. BillingAdjustment.cs
**Source:** `backend/SmartTelehealth.Core/Entities/BillingAdjustment.cs`  
**Purpose:** Billing adjustments/credits  
**Key Properties:** 
- Id, BillingRecordId, Type, Amount
- IsPercentage, Percentage
- AppliedAt, AppliedBy, IsApproved

#### 17. PaymentRefund.cs
**Source:** `backend/SmartTelehealth.Core/Entities/PaymentRefund.cs`  
**Purpose:** Payment refund records  
**Key Properties:**
- Id, SubscriptionPaymentId
- Amount, Reason, StripeRefundId
- RefundedAt, ProcessedByUserId

#### 18. FailedRefund.cs
**Source:** `backend/SmartTelehealth.Core/Entities/FailedRefund.cs`  
**Purpose:** Failed refund tracking  
**Key Properties:**
- Id, SubscriptionPaymentId, Amount, Reason
- StripeRefundId, ErrorMessage, AttemptCount, NextRetryAt

---

### 🟢 MEDIUM - Versioning Entity (1 file)

#### 19. ScheduledPlanMigration.cs
**Source:** `backend/SmartTelehealth.Core/Entities/ScheduledPlanMigration.cs`  
**Purpose:** Plan migration tracking  
**Key Properties:**
- Id, SubscriptionId, FromPlanId, ToPlanId
- NotificationDate, ScheduledMigrationDate
- Status, UserDecision, CompletedDate

---

### 🟢 MEDIUM - Webhook & Sync Entities (3 files)

#### 20. ProcessedWebhookEvent.cs
**Source:** `backend/SmartTelehealth.Core/Entities/ProcessedWebhookEvent.cs`  
**Purpose:** Processed webhook tracking  

#### 21. UnprocessedWebhookEvent.cs
**Source:** `backend/SmartTelehealth.Core/Entities/UnprocessedWebhookEvent.cs`  
**Purpose:** Failed webhook tracking  

#### 22. StripeSyncHistory.cs
**Source:** `backend/SmartTelehealth.Core/Entities/StripeSyncHistory.cs`  
**Purpose:** Stripe sync audit trail  

---

### 🟡 HIGH - Supporting Entities (3 files)

#### 23. Category.cs
**Source:** `backend/SmartTelehealth.Core/Entities/Category.cs`  
**Purpose:** Plan categorization  
**Key Properties:** Id, Name, Description, IsActive  

#### 24. SystemSettings.cs
**Source:** `backend/SmartTelehealth.Core/Entities/SystemSettings.cs`  
**Purpose:** System-wide configuration  
**Key Properties:** Id, Key, Value, Description  

#### 25. User.cs
**Source:** `backend/SmartTelehealth.Core/Entities/User.cs`  
**Purpose:** User entity  
**Key Properties:** Id, Email, FirstName, LastName, UserRoleId  
**Note:** If extracting to standalone system, create minimal stub  

---

## 📋 Entity Extraction Checklist

### Phase 1: Foundation (1 file)
- [ ] BaseEntity.cs

### Phase 2: Master Data (5 files from MasterTables.cs)
- [ ] MasterBillingCycle (extract from MasterTables.cs)
- [ ] MasterCurrency (extract from MasterTables.cs)
- [ ] MasterPrivilegeType (extract from MasterTables.cs)
- [ ] PaymentStatus (extract from MasterTables.cs)
- [ ] RefundStatus (extract from MasterTables.cs)

### Phase 3: Subscription Plan Entities (2 files)
- [ ] SubscriptionPlan.cs
- [ ] SubscriptionPlanPrivilege.cs

### Phase 4: Subscription Entities (3 files)
- [ ] Subscription.cs
- [ ] SubscriptionPayment.cs
- [ ] SubscriptionStatusHistory.cs

### Phase 5: Privilege Entities (3 files)
- [ ] Privilege.cs
- [ ] UserSubscriptionPrivilegeUsage.cs
- [ ] PrivilegeUsageHistory.cs

### Phase 6: Billing Entities (4 files)
- [ ] BillingRecord.cs
- [ ] BillingAdjustment.cs
- [ ] PaymentRefund.cs
- [ ] FailedRefund.cs

### Phase 7: Versioning Entity (1 file)
- [ ] ScheduledPlanMigration.cs

### Phase 8: Webhook Entities (3 files)
- [ ] ProcessedWebhookEvent.cs
- [ ] UnprocessedWebhookEvent.cs
- [ ] StripeSyncHistory.cs

### Phase 9: Supporting Entities (3 files)
- [ ] Category.cs
- [ ] SystemSettings.cs
- [ ] User.cs

---

## 📊 Extraction Summary

### File Count by Category
- **Base Entity:** 1 file
- **Master Data:** 5 files (from 1 source file)
- **Subscription Plan:** 2 files
- **Subscription:** 3 files
- **Privilege:** 3 files
- **Billing:** 4 files
- **Versioning:** 1 file
- **Webhook/Sync:** 3 files
- **Supporting:** 3 files

**TOTAL ENTITIES TO EXTRACT: 25 files**

---

## 🔗 Entity Dependencies

### Dependency Map
```
BaseEntity (inherited by all)

Master Tables (5):
  MasterBillingCycle → used by: SubscriptionPlan, Subscription
  MasterCurrency → used by: SubscriptionPlan, BillingRecord, SubscriptionPayment
  MasterPrivilegeType → used by: Privilege
  PaymentStatus → used by: Payment tracking
  RefundStatus → used by: PaymentRefund

Supporting:
  Category → used by: SubscriptionPlan
  SystemSettings → used by: Pricing calculations
  User → used by: Subscription, BillingRecord (all subscription operations)

Subscription Plan:
  SubscriptionPlan → uses: MasterBillingCycle, MasterCurrency, Category
  SubscriptionPlanPrivilege → uses: SubscriptionPlan, Privilege

Subscription:
  Subscription → uses: User, SubscriptionPlan
  SubscriptionPayment → uses: Subscription, BillingRecord, MasterCurrency
  SubscriptionStatusHistory → uses: Subscription

Privilege:
  Privilege → uses: MasterPrivilegeType
  UserSubscriptionPrivilegeUsage → uses: Subscription, SubscriptionPlanPrivilege, Privilege
  PrivilegeUsageHistory → uses: UserSubscriptionPrivilegeUsage

Billing:
  BillingRecord → uses: User, Subscription, MasterCurrency
  BillingAdjustment → uses: BillingRecord
  PaymentRefund → uses: SubscriptionPayment
  FailedRefund → uses: SubscriptionPayment

Versioning:
  ScheduledPlanMigration → uses: Subscription, SubscriptionPlan (both FromPlan & ToPlan)

Webhook:
  ProcessedWebhookEvent → independent
  UnprocessedWebhookEvent → independent
  StripeSyncHistory → independent
```

---

## ✅ Extraction Priority Order

### Priority 1 (Extract First)
1. ✅ BaseEntity.cs (required by ALL)
2. ✅ User.cs (required by subscriptions)

### Priority 2 (Master Data - Foundation)
3. ✅ MasterBillingCycle
4. ✅ MasterCurrency
5. ✅ MasterPrivilegeType
6. ✅ PaymentStatus
7. ✅ RefundStatus

### Priority 3 (Supporting)
8. ✅ Category
9. ✅ SystemSettings

### Priority 4 (Core Subscription)
10. ✅ SubscriptionPlan
11. ✅ SubscriptionPlanPrivilege
12. ✅ Privilege
13. ✅ Subscription
14. ✅ SubscriptionPayment
15. ✅ SubscriptionStatusHistory

### Priority 5 (Usage & Billing)
16. ✅ UserSubscriptionPrivilegeUsage
17. ✅ PrivilegeUsageHistory
18. ✅ BillingRecord
19. ✅ BillingAdjustment
20. ✅ PaymentRefund
21. ✅ FailedRefund

### Priority 6 (Advanced Features)
22. ✅ ScheduledPlanMigration
23. ✅ ProcessedWebhookEvent
24. ✅ UnprocessedWebhookEvent
25. ✅ StripeSyncHistory

---

## 📁 Source Location Map

### All entities are in:
```
backend/SmartTelehealth.Core/Entities/
```

### Specific files:
- BaseEntity.cs
- SubscriptionPlan.cs
- Subscription.cs
- SubscriptionPlanPrivilege.cs
- SubscriptionPayment.cs
- SubscriptionStatusHistory.cs
- Privilege.cs
- UserSubscriptionPrivilegeUsage.cs
- PrivilegeUsageHistory.cs
- BillingRecord.cs
- BillingAdjustment.cs
- PaymentRefund.cs
- FailedRefund.cs
- ScheduledPlanMigration.cs
- ProcessedWebhookEvent.cs
- UnprocessedWebhookEvent.cs
- StripeSyncHistory.cs
- Category.cs
- SystemSettings.cs
- User.cs

### Special file (contains 5 master tables):
**MasterTables.cs** - Extract these 5 classes:
1. MasterBillingCycle
2. MasterCurrency
3. MasterPrivilegeType
4. PaymentStatus
5. RefundStatus

---

## 🎯 Quick Copy Commands

### Step 1: Copy Individual Entity Files
```bash
# Copy all individual entity files
cp backend/SmartTelehealth.Core/Entities/BaseEntity.cs NewRepo/SmartTelehealth.Core/Entities/
cp backend/SmartTelehealth.Core/Entities/SubscriptionPlan.cs NewRepo/SmartTelehealth.Core/Entities/
cp backend/SmartTelehealth.Core/Entities/Subscription.cs NewRepo/SmartTelehealth.Core/Entities/
# ... (repeat for all 20 individual files)
```

### Step 2: Extract from MasterTables.cs
Create new file: `NewRepo/SmartTelehealth.Core/Entities/MasterTables.cs`

**Extract ONLY these 5 classes:**
1. MasterBillingCycle
2. MasterCurrency
3. MasterPrivilegeType
4. PaymentStatus
5. RefundStatus

**DO NOT extract:**
- AppointmentStatus
- ConsultationMode
- ParticipantRole
- ParticipantStatus
- InvitationStatus
- AppointmentType
- DocumentType
- ReminderType
- ReminderTiming
- EventType

---

## 📝 Notes

### Important Extraction Rules:
1. ✅ Always extract BaseEntity.cs FIRST
2. ✅ Extract User.cs (or create minimal stub)
3. ✅ Extract all 5 master tables from MasterTables.cs
4. ✅ Follow dependency order
5. ✅ Update namespace if extracting to new project

### Files to Skip:
❌ Appointment.cs  
❌ AppointmentInvitation.cs  
❌ AppointmentParticipant.cs  
❌ AppointmentPaymentLog.cs  
❌ AuditLog.cs  
❌ ApplicationLog.cs  
❌ ChatRoom.cs  
❌ ChatSession.cs  
❌ Consultation.cs  
❌ Document.cs  
❌ HealthAssessment.cs  
❌ Message.cs  
❌ MedicationDelivery.cs  
❌ Notification.cs  
❌ Prescription.cs  
❌ Provider.cs  
❌ Question.cs  
❌ QuestionnaireTemplate.cs  
❌ Role.cs  
❌ VideoCall.cs  

**Skip all healthcare-specific entities that are NOT subscription-related**

---

## ✅ Final Entity Count

**Total Entities to Extract: 25 files**

- Individual entity files: 20 files
- Master tables from MasterTables.cs: 5 classes

**All entities are subscription management related only!**

---

**Next Steps:** After extracting entities, proceed with Interfaces, Repositories, Services, Controllers, etc. as documented in COMPLETE_EXTRACTION_GUIDE.md


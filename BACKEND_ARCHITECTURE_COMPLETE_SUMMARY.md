# SmartTeleHealth Backend Architecture - Complete Summary

## 🎯 Executive Summary

The SmartTeleHealth backend is a **comprehensive subscription management system** built with .NET Core that handles the complete lifecycle of healthcare subscription services. It integrates seamlessly with Stripe for payment processing and provides a robust privilege management system for controlling user access to healthcare services.

---

## 🏗️ Architecture Overview

### **Technology Stack**
- **Framework**: ASP.NET Core 6.0+
- **ORM**: Entity Framework Core
- **Database**: SQL Server
- **Payment Gateway**: Stripe API
- **Authentication**: ASP.NET Core Identity
- **Mapping**: AutoMapper
- **Logging**: Microsoft.Extensions.Logging
- **Architecture Pattern**: Clean Architecture (Layered)

### **Core Layers**
1. **API Layer** (`SmartTelehealth.API`) - Controllers, Middleware, Webhooks
2. **Application Layer** (`SmartTelehealth.Application`) - Services, DTOs, Business Logic
3. **Domain Layer** (`SmartTelehealth.Core`) - Entities, Interfaces, Enums
4. **Infrastructure Layer** (`SmartTelehealth.Infrastructure`) - Repositories, External Services, Data Access

---

## 📊 Core Domain Model

### **Primary Entities**

#### **1. User**
- Extends ASP.NET Identity User
- Contains: Id, FirstName, LastName, Email, StripeCustomerId
- **Relationships**: One-to-Many with Subscriptions
- **Role**: Patient, Provider, or Admin

#### **2. SubscriptionPlan**
- Defines subscription plan templates
- Contains: Name, Description, Price, BillingCycle, StripeProductId
- **Key Features**: Trial support, multiple billing cycles, category-based plans
- **Relationships**: One-to-Many with Subscriptions and SubscriptionPlanPrivileges

#### **3. Subscription**
- User's active subscription instance
- Contains: UserId, PlanId, Status, StartDate, NextBillingDate, StripeSubscriptionId
- **Statuses**: Pending, Active, Paused, Cancelled, Expired, PaymentFailed, TrialActive, TrialExpired, Suspended
- **Relationships**: 
  - Many-to-One with User and SubscriptionPlan
  - One-to-Many with BillingRecords, SubscriptionPayments, StatusHistory

#### **4. BillingRecord**
- All billing transactions
- Contains: Amount, TaxAmount, Status, Type, BillingDate, StripeInvoiceId
- **Types**: Subscription, Consultation, Medication, LateFee, Refund, Overage
- **Statuses**: Pending, Paid, Failed, Cancelled, Refunded, Overdue

#### **5. SubscriptionPayment**
- Subscription-specific payments
- Contains: Amount, Status, Type, DueDate, PaidAt, StripePaymentIntentId
- **Types**: Subscription, Trial, Setup, Upgrade, Downgrade, Refund

#### **6. Privilege**
- Available healthcare services
- Contains: Name, Description, PrivilegeType
- **Examples**: "Teleconsultation", "Messaging", "Medication Delivery"

#### **7. SubscriptionPlanPrivilege**
- Junction table linking plans to privileges
- Contains: Value (unlimited=-1, disabled=0, limited>0), DailyLimit, WeeklyLimit, MonthlyLimit, UnitCost
- **Purpose**: Defines what privileges are included in each plan and their limits

#### **8. UserSubscriptionPrivilegeUsage**
- Tracks user's privilege consumption
- Contains: UsedValue, AllowedValue, UsagePeriodStart, UsagePeriodEnd
- **Computed**: RemainingValue, IsExhausted, UsagePercentage

#### **9. SubscriptionStatusHistory**
- Audit trail for status changes
- Contains: FromStatus, ToStatus, ChangedAt, ChangedBy, Reason

---

## 🔄 Service Layer Architecture

### **Subscription Services**

#### **SubscriptionService**
- **Responsibility**: Subscription querying and retrieval
- **Key Methods**:
  - `GetSubscriptionAsync(id, token)` - Retrieve by ID with access control
  - `GetUserSubscriptionsAsync(userId, token)` - Get all user subscriptions
  - `GetUserSubscriptionsWithFilteringAsync(userId, filters, token)` - Advanced filtering
  - `InitializeSubscriptionPrivilegesAsync(subscription, plan, token)` - Set up privilege usage records

#### **SubscriptionLifecycleService**
- **Responsibility**: Complete subscription lifecycle management
- **Key Methods**:
  - `CreateSubscriptionAsync(createDto, token)` - Full subscription creation with Stripe
  - `CancelSubscriptionAsync(id, reason, token)` - Cancel with Stripe sync
  - `PauseSubscriptionAsync(id, token)` - Temporarily pause
  - `ResumeSubscriptionAsync(id, token)` - Resume paused subscription
  - `UpgradeSubscriptionAsync(id, newPlanId, token)` - Upgrade to different plan
  - `RenewSubscriptionAsync(id, token)` - Renew expired subscription
  - `ChangeBillingCycleAsync(id, newCycleId, token)` - Change billing frequency

#### **SubscriptionPlanService**
- **Responsibility**: Plan CRUD and management
- **Key Methods**:
  - `GetPlanByIdAsync(id, token)` - Retrieve plan details
  - `GetSubscriptionPlansWithFilteringAsync(filter, token)` - Filter and search plans
  - `CreatePlanAsync(createDto, token)` - Create plan with Stripe product
  - `UpdatePlanAsync(id, updateDto, token)` - Update plan and sync Stripe
  - `AddPrivilegeToPlanAsync(planId, privilegeDto, token)` - Add privilege to plan
  - `UpdatePlanPrivilegeAsync(id, updateDto, token)` - Modify privilege limits

#### **SubscriptionAutomationService**
- **Responsibility**: Automated background jobs
- **Key Methods**:
  - `ProcessSubscriptionRenewalsAsync()` - Daily billing for renewals
  - `ProcessTrialExpirationsAsync()` - Handle trial end dates
  - `ProcessFailedPaymentsAsync()` - Retry failed payments
  - `SendBillingRemindersAsync()` - Pre-billing notifications
  - `ProcessPrivilegeResetsAsync()` - Reset usage counters

#### **SubscriptionNotificationService**
- **Responsibility**: Email notifications
- **Key Methods**:
  - `SendWelcomeEmailAsync(subscription)` - New subscription welcome
  - `SendCancellationConfirmationAsync(subscription)` - Cancel confirmation
  - `SendBillingReminderAsync(subscription)` - Upcoming billing alert
  - `SendTrialExpirationWarningAsync(subscription)` - Trial ending soon
  - `SendPaymentFailureAlertAsync(subscription)` - Payment failed

### **Billing & Payment Services**

#### **BillingService**
- **Responsibility**: Billing record management
- **Key Methods**:
  - `CreateBillingRecordAsync(createDto, token)` - New billing record
  - `GetBillingRecordAsync(id, token)` - Retrieve by ID
  - `GetUserBillingHistoryAsync(userId, token)` - User's billing history
  - `GetBillingRecordsWithFilteringAsync(filter, token)` - Advanced filtering
  - `CreateBillingAdjustmentAsync(adjustmentDto, token)` - Apply credit/discount

#### **PaymentService**
- **Responsibility**: Payment execution
- **Key Methods**:
  - `ProcessPaymentAsync(billingRecordId, token)` - Execute payment
  - `RetryPaymentAsync(billingRecordId, token)` - Retry failed payment
  - `ProcessRefundAsync(billingRecordId, amount, token)` - Issue refund
  - `ProcessPartialPaymentAsync(billingRecordId, amount, token)` - Partial payment

#### **AutomatedBillingService**
- **Responsibility**: Recurring billing automation
- **Key Methods**:
  - `ProcessRecurringBillingAsync()` - Process scheduled billing
  - `HandleFailedBillingAsync(billingRecord)` - Handle failures

### **Privilege Services**

#### **PrivilegeService**
- **Responsibility**: Privilege usage validation and tracking
- **Key Methods**:
  - `GetRemainingPrivilegeAsync(subscriptionId, privilegeName, token)` - Check remaining
  - `UsePrivilegeAsync(subscriptionId, privilegeName, amount, token)` - Consume privilege
  - `ValidatePrivilegeUsageAsync(subscriptionId, privilegeName, amount, token)` - Validate only
  - `GetPrivilegeUsageHistoryAsync(subscriptionId, privilegeName, token)` - Usage history

**Validation Logic**:
1. Check subscription status (must be Active or TrialActive)
2. Get plan privilege configuration
3. Check if disabled (Value = 0) → Deny
4. Check if unlimited (Value = -1) → Allow
5. Check time-based limits (daily, weekly, monthly)
6. Check quantity-based limits (UsedValue vs AllowedValue)
7. If allowed: Increment usage, create history, check overage

#### **PrivilegeBasedBillingService**
- **Responsibility**: Overage billing
- **Key Methods**:
  - `CalculateOverageChargesAsync(subscriptionId)` - Calculate overage amount
  - `ProcessOverageBillingAsync(subscriptionId, privilege, overageAmount)` - Bill overage

### **Stripe Integration Services**

#### **StripeService**
- **Responsibility**: All Stripe API interactions
- **Key Methods**:
  - `CreateCustomerAsync(email, name, token)` - New Stripe customer
  - `CreateSubscriptionAsync(customerId, priceId, paymentMethodId, token)` - New subscription
  - `CancelSubscriptionAsync(subscriptionId, token)` - Cancel subscription
  - `CreateProductAsync(name, description, token)` - New product
  - `CreatePriceAsync(productId, amount, currency, interval, token)` - New price
  - `CreatePaymentIntentAsync(amount, currency, customerId, token)` - New payment
  - `ConfirmPaymentIntentAsync(paymentIntentId, token)` - Confirm payment

#### **StripeSynchronizationService**
- **Responsibility**: Sync between Stripe and local DB
- **Key Methods**:
  - `SyncSubscriptionsFromStripeAsync()` - Pull subscriptions from Stripe
  - `SyncPaymentsFromStripeAsync()` - Pull payment data

#### **WebhookIdempotencyService**
- **Responsibility**: Prevent duplicate webhook processing
- **Key Methods**:
  - `CheckIdempotencyAsync(eventId, eventType)` - Check if already processed
  - `MarkAsProcessedAsync(eventId, duration)` - Mark event as complete
  - `MarkAsFailedAsync(eventId, error, attempts)` - Mark event as failed

---

## 🔄 Complete Workflows

### **1. Subscription Creation Workflow**

```
1. User selects plan and enters payment method
2. Frontend calls POST /api/subscriptions
3. SubscriptionsController → SubscriptionLifecycleService.CreateSubscriptionAsync()
4. Validate subscription plan exists and is active
5. Check for duplicate active/paused subscriptions
6. Get user details from UserService
7. Ensure Stripe customer exists (create if needed):
   - StripeService.CreateCustomerAsync()
   - Save StripeCustomerId to user
8. Validate payment method with Stripe
9. Get appropriate Stripe price ID based on billing cycle
10. Create Stripe subscription:
    - StripeService.CreateSubscriptionAsync()
    - Returns StripeSubscriptionId
11. Create local subscription entity:
    - Set UserId, PlanId, Status (Active or TrialActive)
    - Set StripeSubscriptionId, StripeCustomerId, StripePriceId
    - Calculate StartDate, NextBillingDate, EndDate
12. Begin database transaction
13. Create subscription in database
14. Create initial status history entry
15. Commit transaction
16. Create initial billing record
17. Initialize privilege usage records:
    - For each plan privilege, create UserSubscriptionPrivilegeUsage
    - Set AllowedValue from plan, UsedValue = 0
18. Send welcome email notification
19. Return subscription DTO to frontend
```

### **2. Payment Processing Workflow**

```
1. Billing record is due (NextBillingDate reached)
2. AutomatedBillingService creates billing record:
   - Type = Subscription
   - Amount = CurrentPrice
   - Status = Pending
3. PaymentService.ProcessPaymentAsync(billingRecordId):
   - Get billing record from database
   - Get user's Stripe customer ID
   - Create Stripe payment intent
   - Confirm payment intent
4. If payment succeeds:
   - Update billing record: Status = Paid, PaidAt = Now
   - Update subscription: LastBillingDate = Now, FailedPaymentAttempts = 0
   - Calculate and set NextBillingDate
   - Send payment confirmation email
5. If payment fails:
   - Update billing record: Status = Failed, FailureReason
   - Update subscription: FailedPaymentAttempts++
   - Schedule retry (1 hour, 1 day, 3 days)
   - Send payment failure alert
   - If 3 failures: Update subscription Status = PaymentFailed
```

### **3. Privilege Usage Workflow**

```
1. User attempts to use a service (e.g., teleconsultation)
2. Service calls PrivilegeService.UsePrivilegeAsync(subscriptionId, "Teleconsultation", 1)
3. Get subscription and validate:
   - Status = Active or TrialActive
   - Not deleted, not paused
4. Get plan privilege configuration:
   - SubscriptionPlanPrivilege for "Teleconsultation"
   - Get Value, DailyLimit, WeeklyLimit, MonthlyLimit, UnitCost
5. Check if privilege is disabled:
   - If Value = 0 → Return false (deny access)
6. Check if privilege is unlimited:
   - If Value = -1 → Allow (skip limit checks)
7. Check time-based limits:
   - Query PrivilegeUsageHistory for daily, weekly, monthly counts
   - If any limit exceeded → Return false (deny access)
8. Get current usage:
   - Query UserSubscriptionPrivilegeUsage
   - Calculate remaining = AllowedValue - UsedValue
9. Check quantity limit:
   - If remaining < requested amount → Return false (deny access)
10. ALL CHECKS PASSED - Allow access:
    - Increment UsedValue
    - Update LastUsedAt
    - Save UserSubscriptionPrivilegeUsage
11. Create usage history record:
    - PrivilegeUsageHistory with timestamp, amount used
12. Check for overage:
    - If UsedValue > AllowedValue:
      - Calculate overage amount
      - Create billing record for overage charge
      - Send overage notification
13. Return true (allow service)
```

### **4. Stripe Webhook Processing Workflow**

```
1. Stripe event occurs (e.g., invoice.payment_succeeded)
2. Stripe sends POST to /api/stripewebhook/webhook
3. StripeWebhookController receives request:
   - Read request body (JSON)
   - Get Stripe-Signature header
4. Verify webhook signature:
   - Use EventUtility.ConstructEvent()
   - If invalid → Return 400 Bad Request
5. Check idempotency:
   - Query ProcessedWebhookEvent by EventId
   - If already processed → Return 200 OK (skip)
   - If failed 3+ times → Return 200 OK (skip)
6. Create processing record:
   - EventId, Status = "Processing", ReceivedAt, Attempts = 1
7. Process event based on type:
   
   For invoice.payment_succeeded:
   - Get BillingRecord by StripeInvoiceId
   - Update Status = Paid, PaidAt = Now
   - Get Subscription by SubscriptionId
   - Update LastBillingDate, FailedPaymentAttempts = 0
   - Send payment confirmation email
   
   For invoice.payment_failed:
   - Get BillingRecord by StripeInvoiceId
   - Update Status = Failed, FailureReason
   - Get Subscription
   - Increment FailedPaymentAttempts
   - If >= 3: Update Status = PaymentFailed
   - Send payment failure alert
   
   For customer.subscription.updated:
   - Get Subscription by StripeSubscriptionId
   - Update Status from Stripe status
   - Update CurrentPrice from Stripe
   - Update NextBillingDate
   - Create status history entry
   
   For customer.subscription.deleted:
   - Get Subscription by StripeSubscriptionId
   - Update Status = Cancelled, CancelledDate = Now
   - Create status history entry
   - Send cancellation notification

8. Mark event as processed:
   - Update ProcessedWebhookEvent: Status = "Processed", ProcessedAt
9. Return 200 OK to Stripe
```

---

## 🔐 Access Control & Security

### **Role-Based Access Control**
- **Admin (RoleID = 332)**: Full access to all resources
- **Provider**: Access to assigned patients and consultations
- **Patient/User**: Access to own subscriptions and billing

### **Token-Based Authentication**
- JWT tokens with user ID and role ID
- Token validation on every API call
- Token model passed to all service methods for audit

### **Access Validation Examples**
```csharp
// User can only access their own subscriptions
if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.UserID != userId)
{
    return new JsonModel { Message = "Access denied", StatusCode = 403 };
}

// Validate subscription access
if (!await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
{
    return new JsonModel { Message = "Access denied", StatusCode = 403 };
}
```

---

## 📝 Audit Trail

### **Audit Fields on All Entities**
- `IsActive` - Soft delete flag
- `IsDeleted` - Deletion flag
- `CreatedBy` - User who created
- `CreatedDate` - Creation timestamp
- `UpdatedBy` - User who last updated
- `UpdatedDate` - Last update timestamp
- `DeletedBy` - User who deleted
- `DeletedDate` - Deletion timestamp

### **Subscription Status History**
- Every status change creates a record
- Tracks: FromStatus, ToStatus, ChangedAt, ChangedBy, Reason

### **Privilege Usage History**
- Every privilege use creates a record
- Tracks: PrivilegeId, UsedAmount, UsedAt, RemainingAfterUse

### **Webhook Event Tracking**
- Every webhook event processed is recorded
- Tracks: EventId, EventType, Status, ProcessedAt, Attempts

---

## 🔄 Background Jobs & Automation

### **Scheduled Jobs**

#### **1. Subscription Renewals** (Daily at 2:00 AM)
- Find subscriptions where `NextBillingDate <= Today`
- Create billing records
- Process payments
- Update billing dates
- Handle failures with retry logic

#### **2. Trial Expirations** (Daily at 3:00 AM)
- Find subscriptions where `Status = TrialActive` and `TrialEndDate <= Today`
- Update status to `TrialExpired`
- Send conversion reminder emails
- Send early warnings (7 days, 3 days, 1 day before)

#### **3. Failed Payment Retries** (Every 6 hours)
- Find billing records where `Status = Failed` and `NextRetryAt <= Now`
- Retry payment processing
- Update attempt count
- Send notifications

#### **4. Billing Reminders** (Daily at 9:00 AM)
- Find subscriptions where `NextBillingDate = Today + 3 days`
- Send billing reminder emails

#### **5. Privilege Resets** (Daily at 12:00 AM)
- Find privilege usages where `UsagePeriodEnd <= Today`
- Reset `UsedValue = 0`
- Update usage period dates

---

## 📊 Database Schema Highlights

### **Key Relationships**
```
User (1) ──────► (N) Subscription
SubscriptionPlan (1) ──────► (N) Subscription
Subscription (1) ──────► (N) BillingRecord
Subscription (1) ──────► (N) SubscriptionPayment
Subscription (1) ──────► (N) SubscriptionStatusHistory
Subscription (1) ──────► (N) UserSubscriptionPrivilegeUsage

SubscriptionPlan (1) ──────► (N) SubscriptionPlanPrivilege
Privilege (1) ──────► (N) SubscriptionPlanPrivilege
SubscriptionPlanPrivilege (1) ──────► (N) UserSubscriptionPrivilegeUsage
```

### **Indexes for Performance**
- `Subscriptions.UserId` - Fast user subscription lookup
- `Subscriptions.StripeSubscriptionId` - Webhook processing
- `Subscriptions.NextBillingDate` - Renewal job queries
- `BillingRecords.UserId` - Billing history
- `BillingRecords.StripeInvoiceId` - Webhook processing
- `ProcessedWebhookEvents.EventId` - Idempotency checks

---

## 🎯 Business Rules Summary

### **Subscription Rules**
1. User can have only ONE active or paused subscription per plan
2. Trial subscriptions automatically convert or expire based on plan settings
3. Status transitions are validated (e.g., can't go from Cancelled to Active without renewal)
4. All subscription operations sync with Stripe

### **Billing Rules**
1. Billing occurs automatically on NextBillingDate
2. Failed payments retry 3 times (1 hour, 1 day, 3 days)
3. After 3 failures, subscription status becomes PaymentFailed
4. All billing is recorded for audit purposes

### **Privilege Rules**
1. Value = -1 means unlimited usage
2. Value = 0 means privilege is disabled
3. Value > 0 means limited usage
4. Time-based limits are checked before quantity limits
5. Overage charges apply when usage exceeds allowed value
6. Usage resets based on usage period (daily, monthly, etc.)

### **Payment Rules**
1. All payments processed through Stripe
2. Payment methods must be validated before use
3. Refunds are processed through Stripe and recorded locally
4. Payment history is maintained indefinitely

---

## 🚀 Integration Points

### **Stripe Integration**
- **API Calls**: Customer, Subscription, Product, Price, Payment management
- **Webhooks**: Real-time sync of subscription and payment events
- **Idempotency**: Ensures webhook events processed exactly once

### **Email Service**
- Welcome emails on subscription creation
- Billing reminders before payment due date
- Payment confirmations and failure alerts
- Trial expiration warnings
- Cancellation confirmations

### **Frontend Integration**
- RESTful API endpoints for all operations
- JSON responses with consistent structure
- Pagination and filtering support
- Error handling with appropriate HTTP status codes

---

## 📈 Key Performance Considerations

### **Database Performance**
- Indexed foreign keys for fast joins
- Indexed StripeSubscriptionId and StripeInvoiceId for webhook processing
- Pagination for large result sets
- Filtered queries at database level

### **Stripe API Performance**
- Retry logic with exponential backoff
- Webhook idempotency to prevent duplicate processing
- Async operations for all API calls

### **Background Job Performance**
- Jobs run during off-peak hours
- Batch processing for large datasets
- Error handling and logging for failed operations

---

## 🔧 Testing Considerations

### **Unit Tests**
- Service layer business logic
- Privilege validation logic
- Status transition validation
- Billing calculations

### **Integration Tests**
- Stripe API integration
- Database operations
- Webhook processing
- End-to-end workflows

### **Test Data**
- Test Stripe accounts and API keys
- Sample subscription plans
- Sample users and subscriptions
- Sample webhook events

---

## 📚 Key Files Reference

### **Controllers**
- `SubscriptionsController.cs` - Subscription API endpoints
- `BillingController.cs` - Billing API endpoints
- `StripeWebhookController.cs` - Webhook processing

### **Services**
- `SubscriptionService.cs` - Subscription queries
- `SubscriptionLifecycleService.cs` - Lifecycle operations
- `SubscriptionPlanService.cs` - Plan management
- `BillingService.cs` - Billing operations
- `PaymentService.cs` - Payment processing
- `PrivilegeService.cs` - Privilege management
- `StripeService.cs` - Stripe integration

### **Entities**
- `Subscription.cs` - Core subscription entity
- `SubscriptionPlan.cs` - Plan template
- `BillingRecord.cs` - Billing transactions
- `Privilege.cs` - Service privileges
- `UserSubscriptionPrivilegeUsage.cs` - Usage tracking

### **Repositories**
- `SubscriptionRepository.cs` - Subscription data access
- `BillingRepository.cs` - Billing data access
- `PrivilegeRepository.cs` - Privilege data access

---

## 🎓 Learning Path for Developers

### **1. Understanding the Domain**
- Read entity definitions to understand data model
- Review status transitions and business rules
- Understand privilege system concepts

### **2. Following a Workflow**
- Start with subscription creation workflow
- Trace code from controller → service → repository
- Observe Stripe integration points

### **3. Exploring Services**
- Read service interfaces to understand contracts
- Review service implementations for business logic
- Understand dependency injection

### **4. Database Schema**
- Review entity relationships
- Understand foreign keys and indexes
- Study audit trail implementation

### **5. Stripe Integration**
- Understand Stripe customer/subscription/payment concepts
- Review webhook event types and handlers
- Study idempotency implementation

---

## ✅ Best Practices Implemented

1. **Separation of Concerns**: Clear layers (API, Application, Domain, Infrastructure)
2. **Dependency Injection**: All services injected via DI container
3. **Repository Pattern**: Data access abstracted through repositories
4. **Service Layer**: Business logic separated from controllers
5. **DTO Pattern**: Data transfer objects for API communication
6. **AutoMapper**: Automatic entity-DTO mapping
7. **Transaction Management**: Database transactions for consistency
8. **Audit Trail**: Complete tracking of changes
9. **Soft Delete**: Preserve data integrity
10. **Logging**: Comprehensive logging for debugging and monitoring
11. **Idempotency**: Webhook events processed exactly once
12. **Retry Logic**: Failed operations retried automatically
13. **Validation**: Input validation at multiple layers
14. **Security**: Role-based access control and authentication

---

## 🔍 Debugging Tips

### **Common Issues**

**1. Webhook Not Processing**
- Check webhook secret configuration
- Verify signature validation
- Check ProcessedWebhookEvent table for idempotency
- Review logs for errors

**2. Payment Failing**
- Check Stripe customer ID
- Verify payment method is valid
- Check Stripe API logs
- Review billing record status

**3. Privilege Not Available**
- Check subscription status (must be Active or TrialActive)
- Verify plan includes privilege
- Check usage limits (daily, weekly, monthly)
- Review UserSubscriptionPrivilegeUsage table

**4. Subscription Not Creating**
- Verify plan is active
- Check for duplicate subscriptions
- Verify Stripe customer creation
- Check database transaction logs

### **Logging Levels**
- **Information**: Normal operations (subscription created, payment processed)
- **Warning**: Potential issues (payment retry, limit exceeded)
- **Error**: Actual errors (Stripe API failure, database error)

---

**End of Complete Summary**



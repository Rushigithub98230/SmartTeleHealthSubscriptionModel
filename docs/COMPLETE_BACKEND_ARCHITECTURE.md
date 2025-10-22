# Complete Backend Architecture Guide

## Table of Contents
1. [System Overview](#system-overview)
2. [Architecture Layers](#architecture-layers)
3. [Core Entities & Relationships](#core-entities--relationships)
4. [Service Layer Organization](#service-layer-organization)
5. [Repository Pattern](#repository-pattern)
6. [Cross-Cutting Concerns](#cross-cutting-concerns)

---

## System Overview

The **SmartTeleHealth Subscription Management System** is a comprehensive healthcare subscription platform built on **.NET 6+** using **Clean Architecture** principles with clear separation between:

- **Core Layer** (Entities, Interfaces, Enums, DTOs)
- **Application Layer** (Business Logic, Services)
- **Infrastructure Layer** (Data Access, External Services)
- **API Layer** (Controllers, Middleware)

### Technology Stack
- **Framework**: ASP.NET Core 6+
- **ORM**: Entity Framework Core
- **Database**: SQL Server
- **Payment Gateway**: Stripe API
- **Architecture Pattern**: Clean Architecture + Repository Pattern
- **DI Container**: Built-in .NET DI

---

## Architecture Layers

### 1. Core Layer (`SmartTelehealth.Core`)

**Purpose**: Domain entities, interfaces, enums, and core DTOs

**Key Components**:
```
SmartTelehealth.Core/
├── Entities/           # Domain entities with business logic
├── Interfaces/         # Repository and service contracts
├── Enums/             # System-wide enumerations
└── DTOs/              # Core data transfer objects
```

**Key Entities**:
- `Subscription` - User subscription records
- `SubscriptionPlan` - Plan definitions with versioning
- `SubscriptionPayment` - Payment tracking
- `BillingRecord` - Billing transaction records
- `Privilege` - Feature/service definitions
- `SubscriptionPlanPrivilege` - Plan-privilege mappings
- `UserSubscriptionPrivilegeUsage` - Usage tracking
- `PrivilegeUsageHistory` - Detailed usage history
- `SubscriptionStatusHistory` - Audit trail
- `PaymentRefund` - Refund tracking

### 2. Application Layer (`SmartTelehealth.Application`)

**Purpose**: Business logic, use cases, and application services

**Key Services**:

#### Subscription Management
- **SubscriptionService** - Core subscription CRUD operations
- **SubscriptionLifecycleService** - Lifecycle operations (create, cancel, pause, resume, renew)
- **SubscriptionAutomationService** - Automated subscription operations
- **SubscriptionNotificationService** - Subscription-related notifications

#### Billing & Payments
- **SubscriptionBillingService** - Consolidated billing operations
- **PaymentService** - Payment processing and retries
- **AutomatedBillingService** - Recurring billing automation
- **InvoiceService** - Invoice generation and management

#### Plan Management
- **SubscriptionPlanService** - Plan CRUD and administration
- **PlanVersioningService** - Plan versioning and migration
- **PlanPricingService** - Healthcare-specific pricing calculations

#### Privilege Management
- **PrivilegeService** - Privilege usage validation and tracking

#### Stripe Integration
- **IStripeService** (Interface) - Stripe operations contract
- **IStripeBillingService** (Interface) - Stripe billing contract

### 3. Infrastructure Layer (`SmartTelehealth.Infrastructure`)

**Purpose**: Data access, external services, and infrastructure concerns

**Key Components**:

#### Repositories (`/Repositories`)
- `SubscriptionRepository` - Subscription data access
- `SubscriptionPlanRepository` - Plan data access with versioning
- `BillingRepository` - Billing record operations
- `SubscriptionPaymentRepository` - Payment records
- `PrivilegeRepository` - Privilege definitions
- `UserSubscriptionPrivilegeUsageRepository` - Usage tracking

#### Services (`/Services`)
- **StripeService** - Stripe API integration
- **StripeBillingService** - Stripe-specific billing logic
- **AutomatedBillingBackgroundService** - Background job for recurring billing
- **PrivilegeResetBackgroundService** - Privilege reset monitoring
- **ScheduledMigrationBackgroundService** - Plan migration automation
- **EmailService**, **NotificationService** - Communication services

#### Data Context (`/Data`)
- **ApplicationDbContext** - EF Core database context

### 4. API Layer (`SmartTelehealth.API`)

**Purpose**: HTTP endpoints, controllers, middleware

**Key Controllers**:
- `SubscriptionsController` - User subscription management endpoints
- `SubscriptionPlansController` - Plan browsing and selection
- `BillingController` - Billing history and operations
- `PaymentController` - Payment processing endpoints
- `StripeWebhookController` - Stripe webhook event handling
- `AdminSubscriptionsController` - Admin subscription management
- `UserSubscriptionController` - User-specific operations

**Middleware**:
- `GlobalExceptionMiddleware` - Centralized error handling
- Authentication/Authorization middleware

---

## Core Entities & Relationships

### Entity Relationship Diagram (Simplified)

```
┌─────────────────┐
│      User       │
└────────┬────────┘
         │ 1
         │
         │ *
┌────────▼─────────┐       ┌──────────────────────┐
│  Subscription    │──────▶│  SubscriptionPlan    │
│                  │ *   1 │                      │
│ - Status         │       │ - Price              │
│ - StartDate      │       │ - VersionNumber      │
│ - NextBillingDate│       │ - IsLatestVersion    │
│ - CurrentPrice   │       │ - ParentPlanId       │
└────────┬─────────┘       └──────────┬───────────┘
         │ 1                          │ 1
         │                            │
         │ *                          │ *
┌────────▼──────────┐        ┌────────▼──────────────────┐
│  BillingRecord    │        │ SubscriptionPlanPrivilege │
│                   │        │                           │
│ - Amount          │        │ - Value (limit)           │
│ - Status          │        │ - UnitCost                │
│ - Type            │        │ - PrivilegeBaseCost       │
└────────┬──────────┘        └────────┬──────────────────┘
         │ 1                          │ *
         │                            │
         │ *                          │ 1
┌────────▼─────────────┐    ┌─────────▼────────┐
│ SubscriptionPayment  │    │    Privilege     │
│                      │    │                  │
│ - Amount             │    │ - Name           │
│ - Status             │    │ - Description    │
│ - StripePaymentIntentId    └──────────────────┘
└──────────────────────┘

┌──────────────────────────────────┐
│ UserSubscriptionPrivilegeUsage   │
│                                  │
│ - SubscriptionId                 │
│ - PrivilegeId                    │
│ - UsedValue                      │
│ - AllowedValue                   │
│ - UsagePeriodStart/End           │
└────────┬─────────────────────────┘
         │ 1
         │
         │ *
┌────────▼──────────────┐
│ PrivilegeUsageHistory │
│                       │
│ - UsedValue           │
│ - UsedAt              │
│ - UsageDate           │
└───────────────────────┘
```

### Key Relationships

1. **User → Subscription** (1:N)
   - One user can have multiple subscriptions

2. **Subscription → SubscriptionPlan** (N:1)
   - Each subscription is based on one plan
   - Plans can have multiple subscriptions

3. **SubscriptionPlan → SubscriptionPlanPrivilege** (1:N)
   - Plans define multiple privileges with limits

4. **Subscription → BillingRecord** (1:N)
   - Subscription generates multiple billing records

5. **BillingRecord → SubscriptionPayment** (1:1 or 1:N)
   - Each billing creates payment record(s)

6. **Subscription → UserSubscriptionPrivilegeUsage** (1:N)
   - Tracks usage per privilege per subscription

7. **UserSubscriptionPrivilegeUsage → PrivilegeUsageHistory** (1:N)
   - Detailed history of each usage event

---

## Service Layer Organization

### Service Responsibility Model

The application follows **Single Responsibility Principle (SRP)** with specialized services:

#### 1. Core Subscription Services

**SubscriptionService**
- **Responsibility**: Basic CRUD, querying, access control
- **Key Operations**:
  - Get subscription by ID
  - Get user subscriptions with filtering
  - Update subscription details
  - Access validation

**SubscriptionLifecycleService**
- **Responsibility**: Lifecycle state transitions
- **Key Operations**:
  - Create subscription (with Stripe integration)
  - Cancel subscription (with reason tracking)
  - Pause/Resume subscription
  - Status transitions with validation
  - Privilege allocation on creation

#### 2. Billing Services

**SubscriptionBillingService** (Consolidated)
- **Responsibility**: All billing operations
- **Key Operations**:
  - Create billing records (subscription, overage, recurring)
  - Calculate plan pricing from privileges
  - Process privilege usage and overage
  - Billing adjustments
  - Billing record management

**PaymentService**
- **Responsibility**: Payment execution and management
- **Key Operations**:
  - Process payment (manual trigger)
  - Record external payment (from webhooks)
  - Retry failed payments
  - Refund processing
  - Payment method management

**AutomatedBillingService**
- **Responsibility**: Automated billing workflows
- **Key Operations**:
  - Process recurring billing (scheduled)
  - Process subscription renewals
  - Handle failed payment retries
  - Calculate proration for plan changes

#### 3. Plan Services

**SubscriptionPlanService**
- **Responsibility**: Plan CRUD and administration
- **Key Operations**:
  - Create/Update/Delete plans
  - Get plans with filtering
  - Manage plan privileges
  - Stripe product/price sync

**PlanVersioningService**
- **Responsibility**: Plan versioning and migration
- **Key Operations**:
  - Create new plan version (preserve existing subscriptions)
  - Schedule plan migrations
  - Migrate subscriptions to new version
  - Handle migration notifications

**PlanPricingService**
- **Responsibility**: Healthcare-specific pricing
- **Key Operations**:
  - Calculate plan price from privileges
  - Calculate admin commission
  - Calculate overage costs

#### 4. Privilege Services

**PrivilegeService**
- **Responsibility**: Privilege usage validation and tracking
- **Key Operations**:
  - Get remaining privileges
  - Use privilege (increment usage)
  - Validate usage limits
  - Create usage history records

---

## Repository Pattern

### Base Repository

All repositories inherit from `RepositoryBase<T>`:

```csharp
public class RepositoryBase<T> where T : BaseEntity
{
    // Standard CRUD operations
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
```

### Specialized Repositories

Each entity has a specialized repository with domain-specific queries:

**SubscriptionRepository**
```csharp
- GetByIdWithDetailsAsync() // Include related entities
- GetByUserIdAsync() // User's subscriptions
- GetByStripeSubscriptionIdAsync() // Stripe sync
- GetSubscriptionsDueForBillingAsync() // Automation
- GetActiveSubscriptionsAsync() // Active only
```

**SubscriptionPlanRepository**
```csharp
- GetByIdWithDetailsAsync() // Include privileges
- GetAllVersionsOfPlanAsync() // Versioning support
- GetActiveSubscriptionsCountAsync() // Migration check
- CreateNewPlanVersionAsync() // Versioning logic
```

---

## Cross-Cutting Concerns

### 1. Transaction Management

**IUnitOfWork Pattern**:
```csharp
await _unitOfWork.BeginTransactionAsync();
try {
    // Multiple operations
    await _unitOfWork.CommitTransactionAsync();
} catch {
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

### 2. Audit Trails

All entities inherit from `BaseEntity`:
```csharp
public abstract class BaseEntity
{
    public DateTime? CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
```

### 3. Error Handling

- Global exception middleware
- Service-level try-catch with logging
- JsonModel standard response format
- Comprehensive logging with ILogger

### 4. Security

- Token-based authentication (TokenModel)
- Role-based access control (RoleId enum)
- Subscription ownership validation
- Admin-only operations enforcement

---

## Next Topics

See related documentation:
- [01_SUBSCRIPTION_LIFECYCLE.md](./01_SUBSCRIPTION_LIFECYCLE.md) - Complete subscription flow
- [02_BILLING_MECHANISM.md](./02_BILLING_MECHANISM.md) - Billing operations
- [03_PAYMENT_PROCESSING.md](./03_PAYMENT_PROCESSING.md) - Payment flows
- [04_STRIPE_INTEGRATION.md](./04_STRIPE_INTEGRATION.md) - Stripe integration details
- [05_PRIVILEGE_MANAGEMENT.md](./05_PRIVILEGE_MANAGEMENT.md) - Privilege system
- [06_PLAN_VERSIONING.md](./06_PLAN_VERSIONING.md) - Plan versioning
- [07_WEBHOOK_PROCESSING.md](./07_WEBHOOK_PROCESSING.md) - Webhook handling
- [08_RENEWAL_RESET.md](./08_RENEWAL_RESET.md) - Renewal and reset logic

---

*Document Version: 1.0*  
*Last Updated: 2025*  
*Architecture Status: Stable*




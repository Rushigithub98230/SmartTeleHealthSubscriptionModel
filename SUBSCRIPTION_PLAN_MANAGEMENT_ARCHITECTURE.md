# 📋 SmartTeleHealth Subscription Plan Management Architecture

## 🎯 Overview
This document provides a comprehensive guide to all components involved in subscription plan management within the SmartTeleHealth system. It covers backend entities, services, controllers, repositories, DTOs, and frontend components.

---

## 🏗️ Backend Architecture

### 📊 Core Entities

#### 1. **SubscriptionPlan** (`SmartTelehealth.Core/Entities/SubscriptionPlan.cs`)
- **Purpose**: Core entity defining subscription plans and their features
- **Key Properties**:
  - `Id` (Guid) - Primary key
  - `Name`, `Description`, `ShortDescription` - Plan identification
  - `Price`, `DiscountedPrice` - Pricing information
  - `BillingCycleId`, `CurrencyId`, `CategoryId` - Foreign keys
  - `IsActive`, `IsFeatured`, `IsMostPopular` - Status flags
  - `TrialDurationInDays`, `IsTrialAllowed` - Trial configuration
  - `StripeProductId`, `StripeMonthlyPriceId` - Stripe integration
  - `MessagingCount`, `IncludesMedicationDelivery` - Plan features

#### 2. **Subscription** (`SmartTelehealth.Core/Entities/Subscription.cs`)
- **Purpose**: Manages user subscriptions to telehealth services
- **Key Properties**:
  - `Id` (Guid) - Primary key
  - `UserId`, `PlanId` - User and plan references
  - `Status` - Subscription lifecycle status
  - `StartDate`, `EndDate`, `NextBillingDate` - Date management
  - `StripeSubscriptionId` - Stripe integration
  - `IsTrialSubscription` - Trial flag

#### 3. **Related Entities**
- `MasterBillingCycle` - Billing frequency definitions
- `MasterCurrency` - Currency management
- `Category` - Plan categorization
- `Privilege` - Service privileges
- `SubscriptionPlanPrivilege` - Plan-privilege relationships

---

### 🔧 Services Layer

#### 1. **SubscriptionPlanService** (`SmartTelehealth.Application/Services/SubscriptionPlanService.cs`)
- **Purpose**: Core business logic for subscription plan management
- **Key Methods**:
  - `GetPlanByIdAsync()` - Retrieve specific plan
  - `GetSubscriptionPlansWithFilteringAsync()` - Advanced filtering
  - `CreatePlanAsync()` - Create new plans
  - `UpdatePlanAsync()` - Update existing plans
  - `DeletePlanAsync()` - Soft delete plans
  - `ActivatePlanAsync()` / `DeactivatePlanAsync()` - Status management

#### 2. **SubscriptionService** (`SmartTelehealth.Application/Services/SubscriptionService.cs`)
- **Purpose**: User subscription management
- **Key Methods**:
  - `CreateSubscriptionAsync()` - Create user subscriptions
  - `UpdateSubscriptionAsync()` - Modify subscriptions
  - `CancelSubscriptionAsync()` - Cancel subscriptions
  - `GetUserSubscriptionsAsync()` - Retrieve user subscriptions

#### 3. **SubscriptionLifecycleService** (`SmartTelehealth.Application/Services/SubscriptionLifecycleService.cs`)
- **Purpose**: Subscription lifecycle management
- **Key Methods**:
  - `UpdateSubscriptionStatusAsync()` - Status transitions
  - `PauseSubscriptionAsync()` - Pause subscriptions
  - `ResumeSubscriptionAsync()` - Resume subscriptions

---

### 🎮 Controllers

#### 1. **SubscriptionPlansController** (`SmartTelehealth.API/Controllers/SubscriptionPlansController.cs`)
- **Route**: `api/subscription-plans`
- **Key Endpoints**:
  - `GET /` - Get active plans (public)
  - `GET /admin` - Get all plans (admin)
  - `POST /admin` - Create plan (admin)
  - `PUT /admin/{id}` - Update plan (admin)
  - `POST /admin/{id}/deactivate` - Deactivate plan (admin)
  - `POST /{id}/activate` - Activate plan (admin)

#### 2. **SubscriptionsController** (`SmartTelehealth.API/Controllers/SubscriptionsController.cs`)
- **Route**: `api/subscriptions`
- **Key Endpoints**:
  - `GET /` - Get user subscriptions
  - `POST /` - Create subscription
  - `PUT /{id}` - Update subscription
  - `POST /{id}/cancel` - Cancel subscription

#### 3. **AdminSubscriptionsController** (`SmartTelehealth.API/Controllers/AdminSubscriptionsController.cs`)
- **Route**: `api/admin/subscriptions`
- **Key Endpoints**:
  - `GET /` - Get all subscriptions (admin)
  - `POST /bulk/status` - Bulk status updates
  - `POST /bulk/cancel` - Bulk cancellations
  - `POST /bulk/notifications` - Bulk notifications

---

### 🗄️ Repository Layer

#### 1. **SubscriptionPlanRepository** (`SmartTelehealth.Infrastructure/Repositories/SubscriptionPlanRepository.cs`)
- **Interface**: `ISubscriptionPlanRepository`
- **Key Methods**:
  - `GetByIdAsync()` - Retrieve by ID
  - `GetActivePlansAsync()` - Get active plans
  - `GetPlansWithFilteringAsync()` - Advanced filtering
  - `CreateAsync()` - Create new plan
  - `UpdateAsync()` - Update existing plan

#### 2. **SubscriptionRepository** (`SmartTelehealth.Infrastructure/Repositories/SubscriptionRepository.cs`)
- **Interface**: `ISubscriptionRepository`
- **Key Methods**:
  - `GetUserSubscriptionsAsync()` - User's subscriptions
  - `GetByStripeIdAsync()` - Stripe integration
  - `CreateAsync()` - Create subscription
  - `UpdateAsync()` - Update subscription

---

### 📋 Data Transfer Objects (DTOs)

#### 1. **Core DTOs**
- `SubscriptionPlanDto` - Plan data transfer
- `CreateSubscriptionPlanDto` - Plan creation
- `UpdateSubscriptionPlanDto` - Plan updates
- `SubscriptionDto` - Subscription data
- `CreateSubscriptionDto` - Subscription creation

#### 2. **Filtering DTOs**
- `SubscriptionPlanFilterDto` - Advanced filtering
- `SubscriptionFilterDto` - Subscription filtering

#### 3. **Specialized DTOs**
- `SubscriptionPlanTimeLimitsDto` - Time-based limits
- `PlanPrivilegeDto` - Privilege configuration
- `PrivilegeTimeLimitDto` - Time-based privilege limits

---

## 🎨 Frontend Architecture

### 🧩 Components

#### 1. **SubscriptionManagementComponent** (`frontend/src/app/admin/subscription-management/subscription-management.ts`)
- **Purpose**: Main admin interface for subscription management
- **Features**:
  - Plan CRUD operations
  - Subscription management
  - Bulk operations
  - Export functionality
  - Advanced filtering

#### 2. **PlanStepperComponent** (`frontend/src/app/admin/subscription-management/plan-stepper.component.ts`)
- **Purpose**: Multi-step plan creation/editing wizard
- **Steps**:
  - Basic Information
  - Pricing Configuration
  - Features & Limits
  - Trial & Marketing
  - Stripe Integration
  - Privilege Management

#### 3. **Dialog Components**
- `PlanDetailsDialogComponent` - Plan details view
- `SubscriptionDetailsDialogComponent` - Subscription details
- `BulkOperationsDialogComponent` - Bulk operations
- `ExportDialogComponent` - Data export
- `ConfirmationDialogComponent` - Confirmations

#### 4. **Homepage Components**
- `PlanCategoryListComponent` - Public plan display
- `QuestionnairePopupComponent` - User onboarding

---

### 🔌 Services

#### 1. **SubscriptionPlanService** (`frontend/src/app/services/subscription-plan.service.ts`)
- **Purpose**: Frontend service for plan management
- **Key Methods**:
  - `createPlan()` - Create new plan
  - `getAllPlans()` - Get all plans
  - `getPlanById()` - Get specific plan
  - `updatePlan()` - Update plan
  - `deletePlan()` - Delete plan
  - `activatePlan()` / `deactivatePlan()` - Status management

#### 2. **SubscriptionService** (`frontend/src/app/services/subscription.service.ts`)
- **Purpose**: Frontend service for subscription management
- **Key Methods**:
  - `getActivePlans()` - Get active plans
  - `createSubscription()` - Create subscription
  - `updateSubscription()` - Update subscription
  - `cancelSubscription()` - Cancel subscription
  - `bulkUpdateStatus()` - Bulk operations

#### 3. **CommonService** (`frontend/src/app/services/common.service.ts`)
- **Purpose**: Shared HTTP service with authentication
- **Key Methods**:
  - `getWithAuth()` - Authenticated GET requests
  - `postWithAuth()` - Authenticated POST requests
  - `putWithAuth()` - Authenticated PUT requests
  - `deleteWithAuth()` - Authenticated DELETE requests

---

### 📱 Models & Interfaces

#### 1. **Core Models** (`frontend/src/app/models/subscription.models.ts`)
- `SubscriptionPlan` - Plan interface
- `Subscription` - Subscription interface
- `CreateSubscriptionPlanDto` - Plan creation
- `UpdateSubscriptionPlanDto` - Plan updates

#### 2. **Service Interfaces**
- `ApiResponse<T>` - Standard API response
- `PaginatedResponse<T>` - Paginated data
- `CheckoutSessionRequest` - Stripe checkout

---

## 🔄 Data Flow

### 1. **Plan Creation Flow**
```
Admin → PlanStepperComponent → SubscriptionPlanService → 
CommonService → SubscriptionPlansController → SubscriptionPlanService → 
SubscriptionPlanRepository → Database
```

### 2. **User Subscription Flow**
```
User → PlanCategoryListComponent → SubscriptionService → 
SubscriptionsController → SubscriptionService → 
SubscriptionRepository → Database + Stripe
```

### 3. **Plan Management Flow**
```
Admin → SubscriptionManagementComponent → SubscriptionPlanService → 
SubscriptionPlansController → SubscriptionPlanService → 
SubscriptionPlanRepository → Database
```

---

## 🛠️ Key Features

### ✅ **Implemented Features**
- ✅ Complete CRUD operations for plans
- ✅ Advanced filtering and pagination
- ✅ Stripe integration for payments
- ✅ Subscription lifecycle management
- ✅ Bulk operations (status, cancel, notify)
- ✅ Export functionality (CSV, Excel, PDF)
- ✅ Privilege management system
- ✅ Trial period support
- ✅ Multi-currency support
- ✅ Category-based organization
- ✅ Real-time status updates
- ✅ Comprehensive error handling

### 🎯 **Business Logic**
- Plan activation/deactivation
- Subscription status transitions
- Billing cycle management
- Overage charge calculation
- Usage tracking and limits
- Payment processing
- Webhook handling
- Notification system

---

## 🔧 Configuration

### **Backend Configuration**
- Database: Entity Framework Core
- Authentication: JWT Bearer tokens
- Payment: Stripe integration
- Logging: Serilog
- Mapping: AutoMapper

### **Frontend Configuration**
- Framework: Angular 17
- UI Library: Angular Material
- State Management: RxJS Observables
- HTTP Client: Angular HttpClient
- Forms: Reactive Forms

---

## 📚 API Endpoints Summary

### **Subscription Plans**
- `GET /api/subscription-plans` - Public active plans
- `GET /api/subscription-plans/admin` - Admin all plans
- `POST /api/subscription-plans/admin` - Create plan
- `PUT /api/subscription-plans/admin/{id}` - Update plan
- `POST /api/subscription-plans/admin/{id}/deactivate` - Deactivate
- `POST /api/subscription-plans/{id}/activate` - Activate

### **Subscriptions**
- `GET /api/subscriptions` - User subscriptions
- `POST /api/subscriptions` - Create subscription
- `PUT /api/subscriptions/{id}` - Update subscription
- `POST /api/subscriptions/{id}/cancel` - Cancel subscription

### **Admin Operations**
- `POST /api/admin/subscriptions/bulk/status` - Bulk status update
- `POST /api/admin/subscriptions/bulk/cancel` - Bulk cancel
- `POST /api/admin/subscriptions/bulk/notifications` - Bulk notify

---

## 🚀 Getting Started

### **Backend Setup**
1. Configure database connection
2. Set up Stripe API keys
3. Run database migrations
4. Start the API server

### **Frontend Setup**
1. Install dependencies: `npm install`
2. Configure environment variables
3. Start development server: `ng serve`
4. Access admin portal at `/admin/subscription-management`

---

## 📝 Notes

- All endpoints require proper authentication
- Admin endpoints require admin role
- Stripe integration requires valid API keys
- Database migrations must be run before first use
- Frontend and backend must be running on configured ports

---

*This architecture supports a complete subscription management system with advanced features for healthcare telehealth services.*

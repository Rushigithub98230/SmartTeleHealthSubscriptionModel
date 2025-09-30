# 🎯 **CONTROLLERS TO EXTRACT FOR SUBSCRIPTION PLAN MANAGEMENT MODULE**

Based on the analysis of the backend controllers, here are the **essential controllers** you need to extract for a complete subscription plan management module:

---

## 🚀 **PRIMARY CONTROLLERS (MUST HAVE)**

### **1. SubscriptionPlansController.cs** ⭐ **CRITICAL**
**Purpose**: Core subscription plan CRUD operations
**Route**: `api/subscriptionplans`
**Key Endpoints**:
- `GET /` - Get all active subscription plans (public)
- `GET /{id}` - Get specific subscription plan
- `POST /admin` - Create new subscription plan (admin only)
- `PUT /admin/{id}` - Update subscription plan (admin only)
- `DELETE /admin/{id}` - Delete subscription plan (admin only)
- `GET /admin` - Get all plans for admin management
- `GET /featured` - Get featured plans
- `GET /trending` - Get trending plans
- `GET /popular` - Get popular plans

**Why Essential**: This is the **core controller** for subscription plan management. Without this, you cannot create, read, update, or delete subscription plans.

---

### **2. SubscriptionsController.cs** ⭐ **CRITICAL**
**Purpose**: User subscription lifecycle management
**Route**: `api/subscriptions`
**Key Endpoints**:
- `GET /` - Get user's subscriptions
- `GET /{id}` - Get specific subscription
- `POST /` - Create new subscription
- `PUT /{id}` - Update subscription
- `DELETE /{id}` - Cancel subscription
- `POST /{id}/pause` - Pause subscription
- `POST /{id}/resume` - Resume subscription
- `POST /{id}/upgrade` - Upgrade subscription
- `POST /{id}/downgrade` - Downgrade subscription

**Why Essential**: This handles the **user subscription lifecycle** - creating, managing, and canceling user subscriptions to plans.

---

### **3. MasterDataController.cs** ⭐ **CRITICAL**
**Purpose**: Master data for subscription plans
**Route**: `api/masterdata`
**Key Endpoints**:
- `GET /billing-cycles` - Get billing cycles (Monthly, Quarterly, Annual)
- `GET /currencies` - Get currencies (USD, EUR, GBP, INR)
- `GET /privilege-types` - Get privilege types
- `GET /payment-statuses` - Get payment statuses
- `GET /refund-statuses` - Get refund statuses

**Why Essential**: This provides the **master data** (billing cycles, currencies, privilege types) that subscription plans depend on.

---

### **4. CategoriesController.cs** ⭐ **CRITICAL**
**Purpose**: Category management for subscription plans
**Route**: `api/categories`
**Key Endpoints**:
- `GET /` - Get all categories
- `GET /{id}` - Get specific category
- `POST /` - Create new category
- `PUT /{id}` - Update category
- `DELETE /{id}` - Delete category

**Why Essential**: Subscription plans are **categorized** (General Health, Mental Health, etc.), so you need category management.

---

## 🔧 **SECONDARY CONTROLLERS (HIGHLY RECOMMENDED)**

### **5. AdminSubscriptionsController.cs** ⭐ **RECOMMENDED**
**Purpose**: Administrative subscription management
**Route**: `api/admin/subscriptions`
**Key Endpoints**:
- `GET /users` - Get all user subscriptions (admin view)
- `GET /plans` - Get all subscription plans (admin view)
- `POST /plans` - Create subscription plan (admin)
- `PUT /plans/{id}` - Update subscription plan (admin)
- `DELETE /plans/{id}` - Delete subscription plan (admin)
- `GET /analytics` - Get subscription analytics
- `POST /bulk-operations` - Bulk operations on subscriptions

**Why Recommended**: Provides **administrative oversight** and bulk operations for subscription management.

---

### **6. SubscriptionAnalyticsController.cs** ⭐ **RECOMMENDED**
**Purpose**: Subscription analytics and reporting
**Route**: `api/subscriptionanalytics`
**Key Endpoints**:
- `GET /overview` - Get subscription overview analytics
- `GET /revenue` - Get revenue analytics
- `GET /churn` - Get churn analysis
- `GET /usage` - Get usage statistics
- `GET /export` - Export analytics data

**Why Recommended**: Provides **business intelligence** and analytics for subscription performance.

---

## 🔗 **SUPPORTING CONTROLLERS (OPTIONAL BUT USEFUL)**

### **7. BillingController.cs** ⭐ **OPTIONAL**
**Purpose**: Billing and payment management
**Route**: `api/billing`
**Key Endpoints**:
- `GET /invoices` - Get billing invoices
- `GET /payments` - Get payment history
- `POST /process` - Process billing

**Why Optional**: If you need **billing and payment** functionality beyond basic subscription management.

---

### **8. PaymentController.cs** ⭐ **OPTIONAL**
**Purpose**: Payment processing
**Route**: `api/payment`
**Key Endpoints**:
- `POST /process` - Process payments
- `POST /refund` - Process refunds
- `GET /history` - Get payment history

**Why Optional**: If you need **payment processing** functionality.

---

## 📋 **CONTROLLER EXTRACTION PRIORITY**

### **🎯 PHASE 1 - CORE FUNCTIONALITY (MUST EXTRACT)**
1. **SubscriptionPlansController.cs** - Core plan management
2. **SubscriptionsController.cs** - User subscription lifecycle
3. **MasterDataController.cs** - Master data dependencies
4. **CategoriesController.cs** - Category management

### **🎯 PHASE 2 - ADMIN FUNCTIONALITY (RECOMMENDED)**
5. **AdminSubscriptionsController.cs** - Administrative management
6. **SubscriptionAnalyticsController.cs** - Analytics and reporting

### **🎯 PHASE 3 - PAYMENT FUNCTIONALITY (OPTIONAL)**
7. **BillingController.cs** - Billing management
8. **PaymentController.cs** - Payment processing

---

## 🔧 **DEPENDENCIES TO EXTRACT WITH CONTROLLERS**

### **Base Controller**
- **BaseController.cs** - Base functionality for all controllers

### **DTOs (Data Transfer Objects)**
- All subscription-related DTOs in `Application/DTOs/`
- Master data DTOs
- Category DTOs

### **Services**
- `ISubscriptionPlanService`
- `ISubscriptionService`
- `ISubscriptionLifecycleService`
- `IMasterDataService`
- `ICategoryService`
- `ISubscriptionAnalyticsService`

### **Entities**
- `SubscriptionPlan`
- `Subscription`
- `MasterBillingCycle`
- `MasterCurrency`
- `Category`
- `Privilege`
- `SubscriptionPlanPrivilege`

---

## 🚀 **QUICK START EXTRACTION**

### **Minimum Viable Module (Phase 1)**
```
Controllers/
├── SubscriptionPlansController.cs
├── SubscriptionsController.cs
├── MasterDataController.cs
└── CategoriesController.cs

Services/
├── ISubscriptionPlanService
├── ISubscriptionService
├── IMasterDataService
└── ICategoryService

DTOs/
├── SubscriptionPlanDto.cs
├── CreateSubscriptionPlanDto.cs
├── UpdateSubscriptionPlanDto.cs
├── SubscriptionDto.cs
├── CreateSubscriptionDto.cs
└── MasterDataDto.cs

Entities/
├── SubscriptionPlan.cs
├── Subscription.cs
├── MasterBillingCycle.cs
├── MasterCurrency.cs
└── Category.cs
```

### **Complete Module (All Phases)**
```
Controllers/
├── SubscriptionPlansController.cs
├── SubscriptionsController.cs
├── MasterDataController.cs
├── CategoriesController.cs
├── AdminSubscriptionsController.cs
├── SubscriptionAnalyticsController.cs
├── BillingController.cs
└── PaymentController.cs

Services/
├── ISubscriptionPlanService
├── ISubscriptionService
├── ISubscriptionLifecycleService
├── IMasterDataService
├── ICategoryService
├── ISubscriptionAnalyticsService
├── IBillingService
└── IPaymentService

DTOs/
├── All subscription-related DTOs
├── All master data DTOs
├── All category DTOs
├── All analytics DTOs
└── All billing DTOs

Entities/
├── All subscription-related entities
├── All master data entities
├── All category entities
└── All billing entities
```

---

## ⚠️ **IMPORTANT NOTES**

1. **Start with Phase 1** - Get core functionality working first
2. **Extract BaseController.cs** - All controllers depend on it
3. **Include DTOs and Services** - Controllers are useless without them
4. **Test each phase** - Ensure functionality works before adding more
5. **Consider your needs** - Only extract what you actually need

---

**🎯 RECOMMENDATION: Start with Phase 1 (4 controllers) to get basic subscription plan management working, then add Phase 2 and 3 as needed!**

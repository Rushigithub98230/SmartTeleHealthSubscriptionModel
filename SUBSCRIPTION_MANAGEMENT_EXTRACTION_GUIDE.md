# 🎯 **Subscription Management Extraction Guide**

## **Complete Developer Playbook for Migrating Subscription Management Functionality**

This comprehensive guide provides step-by-step instructions for extracting the complete subscription management functionality from the SmartTelehealth backend codebase and migrating it to another healthcare project.

---

## 📋 **Table of Contents**

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Core Components Inventory](#core-components-inventory)
4. [Step-by-Step Extraction Process](#step-by-step-extraction-process)
5. [Database Migration](#database-migration)
6. [Configuration & Dependencies](#configuration--dependencies)
7. [Testing Strategy](#testing-strategy)
8. [Post-Migration Checklist](#post-migration-checklist)
9. [Troubleshooting](#troubleshooting)

---

## 🎯 **Overview**

The subscription management system includes:
- **Subscription Plan Management** - Create, update, and manage subscription plans
- **Billing and Payments** - Complete payment processing with Stripe integration
- **Subscription Lifecycle Management** - Create, pause, resume, cancel, renew subscriptions
- **User Subscription Management** - User-specific subscription handling
- **Privileges Management** - Feature access control and usage tracking
- **Categories Management** - Service category organization
- **Advanced Filtering & Pagination** - Database-level filtering, pagination, and sorting
- **Analytics & Reporting** - Comprehensive subscription analytics and reporting

---

## ⚙️ **Prerequisites**

- .NET 6+ project structure
- Entity Framework Core
- AutoMapper
- Stripe.NET SDK
- SQL Server or compatible database
- Basic understanding of Clean Architecture patterns

---

## 📦 **Core Components Inventory**

### **1. Entities (Core Layer)**
```
📁 SmartTelehealth.Core/Entities/
├── Subscription.cs                    # Main subscription entity
├── SubscriptionPlan.cs               # Subscription plan definitions
├── SubscriptionPlanPrivilege.cs      # Plan-privilege relationships
├── SubscriptionPayment.cs            # Payment records
├── SubscriptionStatusHistory.cs      # Status change tracking
├── Privilege.cs                      # Feature privileges
├── UserSubscriptionPrivilegeUsage.cs # Usage tracking
├── PrivilegeUsageHistory.cs          # Usage history
├── BillingRecord.cs                  # Billing records
├── BillingAdjustment.cs              # Billing adjustments
├── Category.cs                       # Service categories
├── CategoryFeeRange.cs               # Category pricing
├── PaymentRefund.cs                  # Refund records
├── ProcessedWebhookEvent.cs          # Webhook idempotency tracking
├── MasterBillingCycle.cs             # Billing cycle master data
├── MasterCurrency.cs                 # Currency master data
├── MasterPrivilegeType.cs            # Privilege type master data
├── PaymentStatus.cs                  # Payment status master data
├── RefundStatus.cs                   # Refund status master data
└── User.cs                           # User entity (required for subscriptions)
```

### **2. Interfaces (Core Layer)**
```
📁 SmartTelehealth.Core/Interfaces/
├── ISubscriptionRepository.cs
├── ISubscriptionPlanRepository.cs
├── ISubscriptionPlanPrivilegeRepository.cs
├── ISubscriptionPaymentRepository.cs
├── ISubscriptionStatusHistoryRepository.cs
├── IUserSubscriptionPrivilegeUsageRepository.cs
├── IPrivilegeUsageHistoryRepository.cs
├── IBillingRepository.cs
├── IBillingAdjustmentRepository.cs
├── ICategoryRepository.cs
├── IProcessedWebhookEventRepository.cs
├── IStripeService.cs
├── IWebhookIdempotencyService.cs
├── IStripeSynchronizationService.cs
└── IStripeBillingService.cs
```

### **3. DTOs (Application Layer)**
```
📁 SmartTelehealth.Application/DTOs/
├── SubscriptionDto.cs
├── CreateSubscriptionDto.cs
├── UpdateSubscriptionDto.cs
├── SubscriptionPlanDto.cs
├── CreateSubscriptionPlanDto.cs
├── SubscriptionPlanTimeLimitsDto.cs
├── SubscriptionDashboardDto.cs
├── SubscriptionStatusHistoryDto.cs
├── SubscriptionPaymentDto.cs
├── PaymentRefundDto.cs
├── PrivilegeDto.cs
├── UserPrivilegeUsageDto.cs
├── PrivilegeUsageDto.cs
├── BillingDto.cs
├── BillingRecordDto.cs
├── CreateBillingRecordDto.cs
├── UpdateBillingRecordDto.cs
├── CreateBillingAdjustmentDto.cs
├── BillingCycleProcessResultDto.cs
├── CategoryDto.cs
├── AnalyticsDtos.cs
├── SubscriptionFilterDto.cs          # Advanced subscription filtering
├── SubscriptionPlanFilterDto.cs      # Advanced plan filtering
├── BillingFilterDto.cs               # Advanced billing filtering
├── BillingDtos.cs                    # Additional billing DTOs
├── WebhookProcessingStats.cs         # Webhook statistics
├── PaymentRequestDto.cs              # Payment processing
├── RefundRequestDto.cs               # Refund processing
└── JsonModel.cs                      # Standard API response wrapper
```

### **4. Services (Application Layer)**
```
📁 SmartTelehealth.Application/Services/
├── SubscriptionService.cs                    # Core subscription logic
├── SubscriptionPlanService.cs                # Subscription plan management
├── SubscriptionLifecycleService.cs          # Lifecycle management
├── SubscriptionAnalyticsService.cs          # Analytics and reporting
├── SubscriptionNotificationService.cs       # User notifications
├── SubscriptionAutomationService.cs         # Automation logic
├── AutomatedBillingService.cs               # Billing automation
├── BillingService.cs                        # Billing operations
├── PrivilegeService.cs                      # Privilege management
├── CategoryService.cs                       # Category management
├── WebhookIdempotencyService.cs             # Webhook idempotency
├── StripeSynchronizationService.cs          # Stripe data sync
└── StripeBillingService.cs                  # Stripe billing operations
```

### **5. Controllers (API Layer) - UPDATED STRUCTURE**

#### **5.1. Core Subscription Controllers**
```
📁 SmartTelehealth.API/Controllers/
├── SubscriptionsController.cs               # Main subscription endpoints (api/subscriptions)
│   ├── GET /api/subscriptions              # Get all subscriptions with filtering
│   ├── GET /api/subscriptions/{id}         # Get subscription by ID
│   ├── GET /api/subscriptions/active       # Get active subscriptions
│   ├── POST /api/subscriptions             # Create subscription
│   ├── PUT /api/subscriptions/{id}         # Update subscription
│   ├── DELETE /api/subscriptions/{id}      # Cancel subscription
│   └── POST /api/subscriptions/{id}/pause  # Pause subscription
│
├── UserSubscriptionsController.cs           # User-specific endpoints (api/user/usersubscriptions)
│   ├── GET /api/user/usersubscriptions/subscriptions  # Get user's subscriptions
│   ├── GET /api/user/usersubscriptions/privilege-usage # Get privilege usage
│   └── POST /api/user/usersubscriptions/purchase      # Purchase subscription
│
└── SubscriptionManagementController.cs      # Web admin management (webadmin/subscription-management)
    ├── GET /webadmin/subscription-management/subscriptions  # Admin subscription management
    ├── GET /webadmin/subscription-management/categories     # Admin category management
    ├── GET /webadmin/subscription-management/analytics      # Admin analytics
    └── POST /webadmin/subscription-management/bulk-operations # Bulk operations
```

#### **5.2. Subscription Plan Controllers**
```
├── SubscriptionPlansController.cs           # Plan management (api/subscriptionplans)
│   ├── GET /api/subscriptionplans          # Get all plans (public)
│   ├── GET /api/subscriptionplans/{id}     # Get plan by ID
│   ├── GET /api/subscriptionplans/active   # Get active plans
│   ├── POST /api/subscriptionplans         # Create plan (admin)
│   ├── PUT /api/subscriptionplans/{id}     # Update plan (admin)
│   └── DELETE /api/subscriptionplans/{id}  # Delete plan (admin)
│
└── SubscriptionPlanPrivilegesController.cs  # Plan-privilege management (api/subscriptionplanprivileges)
    ├── GET /api/subscriptionplanprivileges/privileges        # Get all privileges
    ├── GET /api/subscriptionplanprivileges/privileges/{id}   # Get privilege by ID
    ├── POST /api/subscriptionplanprivileges/privileges      # Create privilege
    ├── PUT /api/subscriptionplanprivileges/privileges/{id}  # Update privilege
    ├── DELETE /api/subscriptionplanprivileges/privileges/{id} # Delete privilege
    ├── GET /api/subscriptionplanprivileges/users/{userId}   # Get user privileges
    └── PUT /api/subscriptionplanprivileges/time-based-limits # Update time-based limits
```

#### **5.3. Admin & Analytics Controllers**
```
├── AdminSubscriptionController.cs           # Admin operations (api/admin/adminsubscription)
│   ├── GET /api/admin/adminsubscription    # Get all subscriptions (admin)
│   ├── GET /api/admin/adminsubscription/analytics # Get analytics
│   ├── GET /api/admin/adminsubscription/analytics/revenue # Get revenue analytics
│   ├── GET /api/admin/adminsubscription/analytics/churn # Get churn analytics
│   └── POST /api/admin/adminsubscription/bulk/* # Bulk operations
│
├── SubscriptionAnalyticsController.cs       # Analytics endpoints (api/subscriptionanalytics)
│   ├── GET /api/subscriptionanalytics      # Get subscription analytics
│   ├── GET /api/subscriptionanalytics/revenue # Get revenue analytics
│   ├── GET /api/subscriptionanalytics/churn # Get churn analytics
│   ├── GET /api/subscriptionanalytics/usage/{subscriptionId} # Get usage analytics
│   └── GET /api/subscriptionanalytics/export # Export analytics data
│
└── SubscriptionAutomationController.cs      # Automation endpoints (api/subscriptionautomation)
    ├── POST /api/subscriptionautomation/billing/trigger # Trigger billing
    ├── POST /api/subscriptionautomation/renew/{subscriptionId} # Renew subscription
    ├── POST /api/subscriptionautomation/change-plan/{subscriptionId} # Change plan
    ├── POST /api/subscriptionautomation/state-transition/{subscriptionId} # State transition
    ├── GET /api/subscriptionautomation/status # Get automation status
    └── GET /api/subscriptionautomation/logs # Get automation logs
```

#### **5.4. Billing & Payment Controllers**
```
├── BillingController.cs                     # Billing operations (api/billing)
│   ├── GET /api/billing                    # Get all billing records
│   ├── GET /api/billing/{id}               # Get billing record by ID
│   ├── GET /api/billing/user/{userId}      # Get user billing history
│   ├── GET /api/billing/subscription/{subscriptionId} # Get subscription billing
│   ├── POST /api/billing                   # Create billing record
│   ├── POST /api/billing/{id}/process-payment # Process payment
│   ├── POST /api/billing/{id}/process-refund # Process refund
│   └── GET /api/billing/analytics          # Get billing analytics
│
└── CategoriesController.cs                  # Category management (api/categories)
    ├── GET /api/categories                 # Get all categories
    ├── GET /api/categories/{id}            # Get category by ID
    ├── GET /api/categories/active          # Get active categories
    ├── GET /api/categories/paged           # Get paginated categories
    ├── POST /api/categories                # Create category
    ├── PUT /api/categories/{id}            # Update category
    └── DELETE /api/categories/{id}         # Delete category
```

#### **5.5. Stripe Integration Controllers**
```
├── StripeController.cs                      # Stripe operations (api/stripe)
│   ├── POST /api/stripe/create-customer    # Create Stripe customer
│   ├── POST /api/stripe/create-payment-method # Create payment method
│   ├── POST /api/stripe/create-subscription # Create Stripe subscription
│   └── GET /api/stripe/customer/{customerId} # Get customer details
│
├── StripeWebhookController.cs               # Stripe webhook ingestion (api/stripewebhook)
│   └── POST /api/stripewebhook             # Process Stripe webhooks
│
├── AdminStripeSyncController.cs             # Stripe synchronization (api/admin/adminstripesync)
│   ├── POST /api/admin/adminstripesync/sync-customers # Sync customers
│   ├── POST /api/admin/adminstripesync/sync-subscriptions # Sync subscriptions
│   └── GET /api/admin/adminstripesync/sync-status # Get sync status
│
└── StripeTestController.cs                  # Stripe testing (api/stripetest)
    ├── POST /api/stripetest/create-test-customer # Create test customer
    └── POST /api/stripetest/create-test-subscription # Create test subscription
```

### **6. Repositories (Infrastructure Layer)**
```
📁 SmartTelehealth.Infrastructure/Repositories/
├── SubscriptionRepository.cs                # Subscription data access
├── SubscriptionPlanRepository.cs            # Subscription plan data access
├── SubscriptionPlanPrivilegeRepository.cs   # Plan-privilege data access
├── SubscriptionPaymentRepository.cs         # Payment data access
├── SubscriptionStatusHistoryRepository.cs   # Status history data access
├── UserSubscriptionPrivilegeUsageRepository.cs # Usage tracking data access
├── PrivilegeUsageHistoryRepository.cs       # Usage history data access
├── BillingRepository.cs                     # Billing data access
├── BillingAdjustmentRepository.cs           # Billing adjustment data access
├── CategoryRepository.cs                    # Category data access
├── ProcessedWebhookEventRepository.cs       # Webhook idempotency data access
├── UserRepository.cs                        # User data access
└── RepositoryBase.cs                        # Base repository implementation
```

### **7. Infrastructure Services**
```
📁 SmartTelehealth.Infrastructure/Services/
├── StripeService.cs                      # Stripe API integration
├── WebhookIdempotencyService.cs          # Webhook processing
├── StripeSynchronizationService.cs       # Stripe data sync
├── StripeBillingService.cs               # Stripe billing operations
├── NotificationService.cs                # Email notifications
├── PdfService.cs                         # PDF generation for invoices
└── AutomatedBillingBackgroundService.cs  # Background billing processing
```

---

## 🔄 **Step-by-Step Extraction Process**

### **Phase 1: Project Setup**

#### **Step 1.1: Create New Project Structure**
```bash
# Create new solution structure
mkdir YourHealthcareProject
cd YourHealthcareProject

# Create projects
dotnet new sln -n YourHealthcareProject
dotnet new classlib -n YourHealthcareProject.Core
dotnet new classlib -n YourHealthcareProject.Application
dotnet new classlib -n YourHealthcareProject.Infrastructure
dotnet new webapi -n YourHealthcareProject.API
dotnet new xunit -n YourHealthcareProject.Tests

# Add projects to solution
dotnet sln add YourHealthcareProject.Core
dotnet sln add YourHealthcareProject.Application
dotnet sln add YourHealthcareProject.Infrastructure
dotnet sln add YourHealthcareProject.API
dotnet sln add YourHealthcareProject.Tests
```

#### **Step 1.2: Install Required NuGet Packages**
```xml
<!-- Core Project -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.0" />

<!-- Application Project -->
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="6.0.0" />

<!-- Infrastructure Project -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.0" />
<PackageReference Include="Stripe.net" Version="43.0.0" />

<!-- API Project -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.0.0" />
```

### **Phase 2: Core Layer Migration**

#### **Step 2.1: Copy Core Entities**
1. Copy all subscription-related entities from `SmartTelehealth.Core/Entities/`
2. Update namespaces to match your project
3. Ensure all entity relationships are preserved
4. Copy `TokenModel.cs` and `AuditType.cs` enums

#### **Step 2.2: Copy Core Interfaces**
1. Copy all subscription-related interfaces from `SmartTelehealth.Core/Interfaces/`
2. Update namespaces
3. Ensure interface contracts are complete

#### **Step 2.3: Update Entity Relationships**
```csharp
// Example: Update namespace in Subscription.cs
namespace YourHealthcareProject.Core.Entities
{
    public class Subscription : BaseEntity
    {
        // ... existing properties
    }
}
```

### **Phase 3: Application Layer Migration**

#### **Step 3.1: Copy DTOs**
1. Copy all subscription-related DTOs
2. Update namespaces
3. Ensure all properties are mapped correctly

#### **Step 3.2: Copy Service Interfaces**
1. Copy all service interfaces from `SmartTelehealth.Application/Interfaces/`
2. Update namespaces
3. Verify interface completeness

#### **Step 3.3: Copy Services**
1. Copy all subscription-related services
2. Update namespaces and dependencies
3. Ensure all business logic is preserved

#### **Step 3.4: Update Mapping Profiles**
```csharp
// Copy and update MappingProfile.cs
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Subscription mappings
        CreateMap<CreateSubscriptionDto, Subscription>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            // ... other mappings
    }
}
```

### **Phase 4: Infrastructure Layer Migration**

#### **Step 4.1: Copy Repositories**
1. Copy all subscription-related repositories
2. Update namespaces and dependencies
3. Ensure all data access methods are included

#### **Step 4.2: Copy Stripe Service**
1. Copy `StripeService.cs` from Infrastructure/Services
2. Update configuration references
3. Ensure all Stripe operations are included

#### **Step 4.3: Update DbContext**
```csharp
// Add subscription-related DbSets to your DbContext
public class YourDbContext : DbContext
{
    // Subscription-related DbSets
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<SubscriptionPlanPrivilege> SubscriptionPlanPrivileges { get; set; }
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
    public DbSet<SubscriptionStatusHistory> SubscriptionStatusHistories { get; set; }
    public DbSet<Privilege> Privileges { get; set; }
    public DbSet<UserSubscriptionPrivilegeUsage> UserSubscriptionPrivilegeUsages { get; set; }
    public DbSet<PrivilegeUsageHistory> PrivilegeUsageHistories { get; set; }
    public DbSet<BillingRecord> BillingRecords { get; set; }
    public DbSet<BillingAdjustment> BillingAdjustments { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryFeeRange> CategoryFeeRanges { get; set; }
    public DbSet<ProviderFee> ProviderFees { get; set; }
    public DbSet<PaymentRefund> PaymentRefunds { get; set; }
    
    // Master tables
    public DbSet<MasterBillingCycle> MasterBillingCycles { get; set; }
    public DbSet<MasterCurrency> MasterCurrencies { get; set; }
    public DbSet<MasterPrivilegeType> MasterPrivilegeTypes { get; set; }
}
```

#### **Step 4.4: Configure Entity Relationships**
```csharp
// Add to OnModelCreating method
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    // Copy all ConfigureSubscription* methods from original DbContext
    ConfigureSubscription(builder);
    ConfigureSubscriptionPlan(builder);
    ConfigureSubscriptionPayment(builder);
    ConfigureSubscriptionStatusHistory(builder);
    ConfigureBillingRecord(builder);
    ConfigureCategory(builder);
    ConfigurePrivilegeUsageHistory(builder);
    ConfigureMasterTables(builder);
}
```

### **Phase 5: API Layer Migration**

#### **Step 5.1: Copy Controllers**
1. Copy all subscription-related controllers
2. Update namespaces and dependencies
3. Ensure all endpoints are included

#### **Step 5.2: Update Base Controller**
```csharp
// Copy BaseController.cs and update namespace
public abstract class BaseController : Controller
{
    [NonAction]
    public TokenModel GetToken(HttpContext httpContext)
    {
        // ... existing implementation
    }
}
```

#### **Step 5.3: Update Startup Configuration**
```csharp
// In Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add subscription-related services
    services.AddScoped<ISubscriptionService, SubscriptionService>();
    services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
    services.AddScoped<ISubscriptionLifecycleService, SubscriptionLifecycleService>();
    services.AddScoped<ISubscriptionAnalyticsService, SubscriptionAnalyticsService>();
    services.AddScoped<ISubscriptionNotificationService, SubscriptionNotificationService>();
    services.AddScoped<ISubscriptionAutomationService, SubscriptionAutomationService>();
    services.AddScoped<IAutomatedBillingService, AutomatedBillingService>();
    services.AddScoped<IBillingService, BillingService>();
    services.AddScoped<IPrivilegeService, PrivilegeService>();
    services.AddScoped<ICategoryService, CategoryService>();
    
    // Add repositories
    services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
    services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
    services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
    services.AddScoped<ISubscriptionPaymentRepository, SubscriptionPaymentRepository>();
    services.AddScoped<ISubscriptionStatusHistoryRepository, SubscriptionStatusHistoryRepository>();
    services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();
    services.AddScoped<IPrivilegeUsageHistoryRepository, PrivilegeUsageHistoryRepository>();
    services.AddScoped<IBillingRepository, BillingRepository>();
    services.AddScoped<IBillingAdjustmentRepository, BillingAdjustmentRepository>();
    services.AddScoped<ICategoryRepository, CategoryRepository>();
    
    // Add Stripe service
    services.AddScoped<IStripeService, StripeService>();
    
    // Add AutoMapper
    services.AddAutoMapper(typeof(MappingProfile));
    
    // Add background services
    services.AddHostedService<AutomatedBillingBackgroundService>();

    // Add MVC filters/utilities
    services.AddControllers(options =>
    {
        options.Filters.Add<JsonModelActionFilter>(); // Ensures JsonModel wrapping
    });
}
```

---

## 🗄️ **Database Migration**

### **Step 6.1: Create Migration Scripts**
```bash
# Create initial migration
dotnet ef migrations add InitialSubscriptionManagement --project YourHealthcareProject.Infrastructure --startup-project YourHealthcareProject.API

# Update database
dotnet ef database update --project YourHealthcareProject.Infrastructure --startup-project YourHealthcareProject.API
```

### **Step 6.2: Seed Master Data**
```sql
-- Copy and run seed-privileges.sql
-- Update with your project's table names and data
INSERT INTO MasterBillingCycles (Id, Name, Description, IsActive, SortOrder, CreatedDate, UpdatedDate)
VALUES 
    (NEWID(), 'Monthly', 'Monthly billing cycle', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Quarterly', 'Quarterly billing cycle', 1, 2, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Annual', 'Annual billing cycle', 1, 3, GETUTCDATE(), GETUTCDATE());

INSERT INTO MasterCurrencies (Id, Code, Name, Symbol, IsActive, SortOrder, CreatedDate, UpdatedDate)
VALUES 
    (NEWID(), 'USD', 'US Dollar', '$', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'EUR', 'Euro', '€', 1, 2, GETUTCDATE(), GETUTCDATE());

INSERT INTO MasterPrivilegeTypes (Id, Name, Description, IsActive, SortOrder, CreatedDate, UpdatedDate)
VALUES 
    (NEWID(), 'Consultation', 'Medical consultation privilege', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Messaging', 'Messaging privilege', 1, 2, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Video Call', 'Video call privilege', 1, 3, GETUTCDATE(), GETUTCDATE());
```

### **Step 6.3: Seed Categories (required by SubscriptionPlan.CategoryId)**
- Option A (recommended): run the provided DataUpdater tool to ensure baseline categories and update existing plans.
```powershell
# From backend folder
./RunDataUpdater.ps1
```
- Option B: seed minimal categories manually if your project doesn't have them yet.
```sql
INSERT INTO Categories (Id, Name, Description, BasePrice, ConsultationFee, OneTimeConsultationFee, IsActive, RequiresHealthAssessment, AllowsMedicationDelivery, AllowsFollowUpMessaging, CreatedDate)
VALUES
    (NEWID(), 'Primary Care', 'General health consultations', 100.00, 100.00, 150.00, 1, 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'Mental Health', 'Therapy and counseling', 150.00, 150.00, 200.00, 1, 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'Dermatology', 'Skin consultations', 120.00, 120.00, 180.00, 1, 0, 1, 1, GETUTCDATE());
```

### **Step 6.4: Seed Privileges Data**
Execute `backend/seed-privileges.sql` after inserting MasterPrivilegeTypes to populate commonly used privileges.

### **Step 6.5: Seed Master Data for Subscription Management**
```sql
-- Insert Master Billing Cycles
INSERT INTO MasterBillingCycles (Id, Name, Description, DurationInDays, IsActive, SortOrder, CreatedDate, UpdatedDate)
VALUES 
    (NEWID(), 'Monthly', 'Monthly billing cycle', 30, 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Quarterly', 'Quarterly billing cycle', 90, 1, 2, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Annual', 'Annual billing cycle', 365, 1, 3, GETUTCDATE(), GETUTCDATE());

-- Insert Master Currencies
INSERT INTO MasterCurrencies (Id, Code, Name, Symbol, IsActive, SortOrder, CreatedDate, UpdatedDate)
VALUES 
    (NEWID(), 'USD', 'US Dollar', '$', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'EUR', 'Euro', '€', 1, 2, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'GBP', 'British Pound', '£', 1, 3, GETUTCDATE(), GETUTCDATE());

-- Insert Master Privilege Types
INSERT INTO MasterPrivilegeTypes (Id, Name, Description, IsActive, SortOrder, CreatedDate, UpdatedDate)
VALUES 
    (NEWID(), 'Consultation', 'Medical consultation privilege', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Messaging', 'Messaging privilege', 1, 2, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Video Call', 'Video call privilege', 1, 3, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Medication', 'Medication delivery privilege', 1, 4, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Document', 'Document access privilege', 1, 5, GETUTCDATE(), GETUTCDATE());

-- Insert Payment Statuses
INSERT INTO PaymentStatuses (Id, Name, Description, IsActive, SortOrder, Color, CreatedDate, UpdatedDate)
VALUES 
    (NEWID(), 'Pending', 'Payment is pending', 1, 1, '#FFA500', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Paid', 'Payment completed successfully', 1, 2, '#008000', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Failed', 'Payment failed', 1, 3, '#FF0000', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Cancelled', 'Payment was cancelled', 1, 4, '#808080', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Refunded', 'Payment was refunded', 1, 5, '#FFC0CB', GETUTCDATE(), GETUTCDATE());

-- Insert Refund Statuses
INSERT INTO RefundStatuses (Id, Name, Description, IsActive, SortOrder, Color, CreatedDate, UpdatedDate)
VALUES 
    (NEWID(), 'Pending', 'Refund is pending', 1, 1, '#FFA500', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Processed', 'Refund processed successfully', 1, 2, '#008000', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Failed', 'Refund failed', 1, 3, '#FF0000', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Cancelled', 'Refund was cancelled', 1, 4, '#808080', GETUTCDATE(), GETUTCDATE());
```

---

## ⚙️ **Configuration & Dependencies**

### **Step 7.1: Update appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YourConnectionString"
  },
  "Stripe": {
    "PublishableKey": "your_stripe_publishable_key",
    "SecretKey": "your_stripe_secret_key",
    "WebhookSecret": "your_stripe_webhook_secret"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### **Step 7.3: Webhook Processing Configuration**
```csharp
// Add webhook processing services
services.AddScoped<IWebhookIdempotencyService, WebhookIdempotencyService>();
services.AddScoped<IProcessedWebhookEventRepository, ProcessedWebhookEventRepository>();
services.AddScoped<IStripeSynchronizationService, StripeSynchronizationService>();
services.AddScoped<IStripeBillingService, StripeBillingService>();

// Add webhook processing background service
services.AddHostedService<AutomatedBillingBackgroundService>();
```

### **Step 7.4: Webhook Endpoint Configuration**
- Ensure your API exposes the webhook endpoint (controller included above) and your Stripe dashboard points to it, e.g.: `/api/StripeWebhook`.
- Set `Stripe:WebhookSecret` in environment configuration for signature verification.
- Configure webhook retry and idempotency settings:

```json
{
  "StripeSettings": {
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5,
    "WebhookIdempotencyEnabled": true,
    "WebhookProcessingTimeoutMinutes": 10
  }
}
```

### **Step 7.5: Add Required Middleware**
```csharp
// In Program.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Add required middleware
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<InputValidationMiddleware>();
    app.UseMiddleware<RateLimitingMiddleware>();
    
    // ... other middleware
}
```

---

## 🧪 **Testing Strategy**

### **Step 8.1: Copy Test Files**
1. Copy `MockStripeService.cs` from `SmartTelehealth.API.Tests/Mocks/`
2. Copy integration tests from `SmartTelehealth.API.Tests/IntegrationTests/`
3. Update namespaces and dependencies

### **Step 8.2: Create Test Database**
```csharp
// In test setup
services.AddDbContext<YourDbContext>(options =>
{
    options.UseInMemoryDatabase("TestDb");
});
```

### **Step 8.3: Run Tests**
```bash
dotnet test YourHealthcareProject.Tests
```

### **Step 8.4: End‑to‑End Subscription Lifecycle Validation**
- Create plan → attach Stripe product/prices → create subscription → verify `StripeSubscriptionId` saved.
- Pause/resume/cancel → verify status history and next billing date logic.
- Charge → verify `SubscriptionPayment` and `BillingRecord` updated.
- Trigger Stripe webhook events (invoice.paid, invoice.payment_failed) → verify webhook processing updates local state idempotently.

### **Step 8.5: Webhook Processing Validation**
- Test webhook idempotency by sending duplicate events
- Verify webhook retry logic for failed processing
- Test webhook signature validation
- Validate webhook event processing statistics
- Test webhook processing timeout handling

### **Step 8.6: Advanced Filtering Validation**
- Test subscription filtering with complex criteria
- Validate plan filtering with multiple parameters
- Test billing record filtering and pagination
- Verify sorting and search functionality

---

## ✅ **Post-Migration Checklist**

### **Step 9.1: Verify Core Functionality**
- [ ] Subscription creation works
- [ ] Subscription plan management works
- [ ] Billing and payment processing works
- [ ] Privilege management works
- [ ] Category management works
- [ ] Stripe integration works
- [ ] Webhook processing works
- [ ] Webhook idempotency works
- [ ] Stripe synchronization works
- [ ] Background services are running

### **Step 9.2: Verify API Endpoints**
- [ ] All subscription endpoints respond correctly
- [ ] All billing endpoints respond correctly
- [ ] All plan management endpoints respond correctly
- [ ] All webhook endpoints respond correctly
- [ ] Advanced filtering endpoints work correctly
- [ ] Authentication and authorization work
- [ ] Error handling works properly
- [ ] Validation works correctly

### **Step 9.3: Verify Database**
- [ ] All tables are created correctly
- [ ] Relationships are working
- [ ] Master data is seeded
- [ ] Webhook processing table is created
- [ ] All foreign key relationships work
- [ ] Migrations run successfully

### **Step 9.4: Verify External Integrations**
- [ ] Stripe webhooks are working
- [ ] Stripe webhook idempotency is working
- [ ] Stripe synchronization is working
- [ ] Payment processing works
- [ ] Refund processing works
- [ ] Email notifications work (if implemented)
- [ ] PDF invoice generation works

---

## 🔧 **Troubleshooting**

### **Common Issues**

#### **Issue 1: Missing Dependencies**
```bash
# Solution: Install missing packages
dotnet add package PackageName
```

#### **Issue 2: Namespace Conflicts**
```csharp
// Solution: Update all namespaces consistently
using YourHealthcareProject.Core.Entities;
using YourHealthcareProject.Application.DTOs;
```

#### **Issue 3: Database Migration Errors**
```bash
# Solution: Reset and recreate migrations
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialSubscriptionManagement
dotnet ef database update
```

#### **Issue 4: Stripe Configuration Issues**
```csharp
// Solution: Verify Stripe configuration
var stripeConfig = configuration.GetSection("Stripe");
if (string.IsNullOrEmpty(stripeConfig["SecretKey"]))
{
    throw new InvalidOperationException("Stripe configuration is missing");
}
```

#### **Issue 5: Webhook Processing Issues**
```csharp
// Solution: Check webhook processing configuration
var webhookConfig = configuration.GetSection("StripeSettings");
if (!webhookConfig.GetValue<bool>("WebhookIdempotencyEnabled"))
{
    _logger.LogWarning("Webhook idempotency is disabled - duplicate events may be processed");
}
```

#### **Issue 6: Filtering and Pagination Issues**
```csharp
// Solution: Verify filter DTOs are properly configured
if (filter.Page < 1) filter.Page = 1;
if (filter.PageSize < 1 || filter.PageSize > 200) filter.PageSize = 50;
```

#### **Issue 7: Master Data Missing**
```sql
-- Solution: Check if master data is seeded
SELECT COUNT(*) FROM MasterBillingCycles;
SELECT COUNT(*) FROM MasterCurrencies;
SELECT COUNT(*) FROM MasterPrivilegeTypes;
```

---

## 📚 **Additional Resources**

### **Key Files to Reference**
- `SUBSCRIPTION_MANAGEMENT_DOCUMENTATION.md` - Original documentation
- `seed-privileges.sql` - Database seed scripts
- `UpdateSubscriptionPlans.sql` - Plan update scripts

### **Supporting Utilities/Helpers to Include**
- `SmartTelehealth.API/Controllers/BaseController.cs` – token extraction (`TokenModel` propagation)
- `SmartTelehealth.Application/DTOs/JsonModel.cs` – standard API response wrapper
- `SmartTelehealth.API/Filters/JsonModelActionFilter.cs` – enforces `JsonModel` response format
- `SmartTelehealth.API/GlobalExceptionMiddleware.cs` – uniform error responses
- `SmartTelehealth.API/Middleware/InputValidationMiddleware.cs` – request validation
- `SmartTelehealth.API/Middleware/RateLimitingMiddleware.cs` – request throttling
- `SmartTelehealth.Core/DTOs/TokenModel.cs` – user authentication context
- `SmartTelehealth.Core/DTOs/WebhookProcessingStats.cs` – webhook statistics
- `SmartTelehealth.Application/DTOs/SubscriptionFilterDto.cs` – advanced filtering
- `SmartTelehealth.Application/DTOs/SubscriptionPlanFilterDto.cs` – plan filtering
- `SmartTelehealth.Application/DTOs/BillingFilterDto.cs` – billing filtering

### **Important Notes**
1. **Preserve Audit Fields**: Ensure all entities maintain audit fields (CreatedBy, UpdatedBy, etc.)
2. **Maintain Relationships**: Keep all entity relationships intact
3. **Test Thoroughly**: Run comprehensive tests before going live
4. **Monitor Logs**: Check logs for any issues during migration
5. **Backup Data**: Always backup existing data before migration
6. **Webhook Security**: Ensure webhook signature validation is properly configured
7. **Idempotency**: Test webhook idempotency to prevent duplicate processing
8. **Master Data**: Ensure all master data is properly seeded before testing
9. **Filtering**: Test advanced filtering capabilities thoroughly
10. **Stripe Sync**: Verify Stripe synchronization is working correctly

---

## 🎯 **Success Criteria**

The migration is successful when:
- ✅ All subscription functionality works in the new project
- ✅ Database schema is correctly migrated
- ✅ All API endpoints respond correctly
- ✅ Stripe integration works properly
- ✅ Webhook processing works correctly
- ✅ Webhook idempotency is functioning
- ✅ Stripe synchronization is working
- ✅ Advanced filtering works correctly
- ✅ Master data is properly seeded
- ✅ Background services are running
- ✅ All tests pass
- ✅ No critical errors in logs

---

**📝 Note**: This guide covers the complete subscription management functionality. For additional features like chat, video calls, or other non-subscription features, refer to their respective extraction guides.

**🔄 Version**: 3.0  
**📅 Last Updated**: [Current Date]  
**👨‍💻 Maintained By**: Development Team

---

## 🔄 **Webhook Processing Deep Dive**

### **Webhook Processing Architecture**
The subscription management system includes comprehensive webhook processing for Stripe integration:

#### **1. Webhook Idempotency**
- `ProcessedWebhookEvent` entity tracks processed webhooks
- Prevents duplicate processing of Stripe events
- Supports retry logic for failed webhooks
- Maintains processing statistics

#### **2. Webhook Event Types Supported**
- `customer.subscription.created`
- `customer.subscription.updated`
- `customer.subscription.deleted`
- `invoice.payment_succeeded`
- `invoice.payment_failed`
- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `customer.created`
- `customer.updated`
- `customer.deleted`

#### **3. Webhook Processing Flow**
1. **Receive Webhook** → Validate signature
2. **Check Idempotency** → Prevent duplicate processing
3. **Process Event** → Update local database
4. **Update Statistics** → Track processing metrics
5. **Handle Errors** → Retry logic and error logging

#### **4. Webhook Configuration**
```json
{
  "StripeSettings": {
    "WebhookSecret": "whsec_...",
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5,
    "WebhookIdempotencyEnabled": true,
    "WebhookProcessingTimeoutMinutes": 10
  }
}
```

#### **5. Webhook Testing**
- Use Stripe CLI for local webhook testing
- Test webhook signature validation
- Verify idempotency with duplicate events
- Test error handling and retry logic
- Validate webhook processing statistics

---

## 🎯 **Complete Feature Matrix**

### **Core Subscription Management**
- ✅ Subscription lifecycle management
- ✅ Subscription plan management
- ✅ Billing and payment processing
- ✅ Privilege management and usage tracking
- ✅ Category management
- ✅ Stripe integration
- ✅ Webhook processing with idempotency
- ✅ Advanced filtering and pagination
- ✅ Analytics and reporting
- ✅ Background processing
- ✅ Email notifications
- ✅ PDF invoice generation
- ✅ Refund processing
- ✅ Master data management
- ✅ Audit logging
- ✅ Error handling and retry logic

### **Controller Consolidation Summary**
- **SubscriptionsController**: Main subscription endpoints (api/subscriptions)
- **UserSubscriptionsController**: User-specific endpoints (api/user/usersubscriptions)
- **SubscriptionManagementController**: Web admin management (webadmin/subscription-management)
- **SubscriptionPlansController**: Plan management (api/subscriptionplans)
- **SubscriptionPlanPrivilegesController**: Plan-privilege management (api/subscriptionplanprivileges)
- **AdminSubscriptionController**: Admin operations (api/admin/adminsubscription)
- **SubscriptionAnalyticsController**: Analytics endpoints (api/subscriptionanalytics)
- **SubscriptionAutomationController**: Automation endpoints (api/subscriptionautomation)
- **BillingController**: Billing operations (api/billing)
- **CategoriesController**: Category management (api/categories)
- **StripeController**: Stripe operations (api/stripe)
- **StripeWebhookController**: Stripe webhook ingestion (api/stripewebhook)
- **AdminStripeSyncController**: Stripe synchronization (api/admin/adminstripesync)
- **StripeTestController**: Stripe testing (api/stripetest)

### **Advanced Filtering & Pagination Features**
- ✅ Database-level filtering for all GET endpoints
- ✅ Comprehensive pagination with metadata
- ✅ Advanced sorting capabilities
- ✅ Search functionality across all entities
- ✅ Date range filtering
- ✅ Status-based filtering
- ✅ Category-based filtering
- ✅ User-based filtering
- ✅ Performance optimized queries
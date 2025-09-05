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
- **Questionnaire Management** - Health assessment questionnaires

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
└── MasterTables.cs                   # Reference data (billing cycles, currencies, etc.)
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
└── IStripeService.cs
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
└── AnalyticsDtos.cs
```

### **4. Services (Application Layer)**
```
📁 SmartTelehealth.Application/Services/
├── SubscriptionService.cs                    # Core subscription logic
├── SubscriptionLifecycleService.cs          # Lifecycle management
├── SubscriptionAnalyticsService.cs          # Analytics and reporting
├── SubscriptionNotificationService.cs       # User notifications
├── SubscriptionAutomationService.cs         # Automation logic
├── AutomatedBillingService.cs               # Billing automation
├── BillingService.cs                        # Billing operations
├── PrivilegeService.cs                      # Privilege management
├── CategoryService.cs                       # Category management
└── BackgroundServices/
    └── SubscriptionBackgroundService.cs     # Background processing
```

### **5. Controllers (API Layer)**
```
📁 SmartTelehealth.API/Controllers/
├── SubscriptionsController.cs               # Main subscription endpoints
├── SubscriptionManagementController.cs      # Admin management
├── UserSubscriptionsController.cs           # User-specific endpoints
├── SubscriptionPlansController.cs           # Plan management
├── SubscriptionPlanPrivilegesController.cs  # Plan-privilege management
├── StripeController.cs                      # Stripe operations (payments, products, prices)
├── StripeWebhookController.cs               # Stripe webhook ingestion
├── SubscriptionAnalyticsController.cs       # Analytics endpoints
├── SubscriptionAutomationController.cs      # Automation endpoints
├── AdminSubscriptionController.cs           # Admin operations
├── PrivilegesController.cs                  # Privilege management
├── BillingController.cs                     # Billing operations
└── ProviderPrivilegesController.cs          # Provider privilege management
```

### **6. Repositories (Infrastructure Layer)**
```
📁 SmartTelehealth.Infrastructure/Repositories/
├── SubscriptionRepository.cs
├── SubscriptionPlanRepository.cs
├── SubscriptionPlanPrivilegeRepository.cs
├── SubscriptionPaymentRepository.cs
├── SubscriptionStatusHistoryRepository.cs
├── UserSubscriptionPrivilegeUsageRepository.cs
├── PrivilegeUsageHistoryRepository.cs
├── BillingRepository.cs
├── BillingAdjustmentRepository.cs
├── CategoryRepository.cs
└── StripeService.cs
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
    services.AddScoped<ISubscriptionLifecycleService, SubscriptionLifecycleService>();
    services.AddScoped<ISubscriptionAnalyticsService, SubscriptionAnalyticsService>();
    services.AddScoped<ISubscriptionNotificationService, SubscriptionNotificationService>();
    services.AddScoped<ISubscriptionAutomationService, SubscriptionAutomationService>();
    services.AddScoped<IAutomatedBillingService, AutomatedBillingService>();
    services.AddScoped<IBillingService, BillingService>();
    services.AddScoped<IPrivilegeService, PrivilegeService>();
    services.AddScoped<ICategoryService, CategoryService>();
    services.AddScoped<IProviderFeeService, ProviderFeeService>();
    
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
    services.AddScoped<IProviderFeeRepository, ProviderFeeRepository>();
    
    // Add Stripe service
    services.AddScoped<IStripeService, StripeService>();
    
    // Add AutoMapper
    services.AddAutoMapper(typeof(MappingProfile));
    
    // Add background services
    services.AddHostedService<SubscriptionBackgroundService>();

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
- Option B: seed minimal categories manually if your project doesn’t have them yet.
```sql
INSERT INTO Categories (Id, Name, Description, BasePrice, ConsultationFee, OneTimeConsultationFee, IsActive, RequiresHealthAssessment, AllowsMedicationDelivery, AllowsFollowUpMessaging, CreatedDate)
VALUES
    (NEWID(), 'Primary Care', 'General health consultations', 100.00, 100.00, 150.00, 1, 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'Mental Health', 'Therapy and counseling', 150.00, 150.00, 200.00, 1, 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'Dermatology', 'Skin consultations', 120.00, 120.00, 180.00, 1, 0, 1, 1, GETUTCDATE());
```

### **Step 6.4: Seed Privileges Data**
Execute `backend/seed-privileges.sql` after inserting MasterPrivilegeTypes to populate commonly used privileges.

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

### **Step 7.3: Webhook Endpoint**
- Ensure your API exposes the webhook endpoint (controller included above) and your Stripe dashboard points to it, e.g.: `/api/StripeWebhook`.
- Set `Stripe:WebhookSecret` in environment configuration for signature verification.

### **Step 7.2: Add Required Middleware**
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

---

## ✅ **Post-Migration Checklist**

### **Step 9.1: Verify Core Functionality**
- [ ] Subscription creation works
- [ ] Subscription plan management works
- [ ] Billing and payment processing works
- [ ] Privilege management works
- [ ] Category management works
- [ ] Stripe integration works
- [ ] Background services are running

### **Step 9.2: Verify API Endpoints**
- [ ] All subscription endpoints respond correctly
- [ ] Authentication and authorization work
- [ ] Error handling works properly
- [ ] Validation works correctly

### **Step 9.3: Verify Database**
- [ ] All tables are created correctly
- [ ] Relationships are working
- [ ] Master data is seeded
- [ ] Migrations run successfully

### **Step 9.4: Verify External Integrations**
- [ ] Stripe webhooks are working
- [ ] Payment processing works
- [ ] Email notifications work (if implemented)

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

### **Important Notes**
1. **Preserve Audit Fields**: Ensure all entities maintain audit fields (CreatedBy, UpdatedBy, etc.)
2. **Maintain Relationships**: Keep all entity relationships intact
3. **Test Thoroughly**: Run comprehensive tests before going live
4. **Monitor Logs**: Check logs for any issues during migration
5. **Backup Data**: Always backup existing data before migration

---

## 🎯 **Success Criteria**

The migration is successful when:
- ✅ All subscription functionality works in the new project
- ✅ Database schema is correctly migrated
- ✅ All API endpoints respond correctly
- ✅ Stripe integration works properly
- ✅ Background services are running
- ✅ All tests pass
- ✅ No critical errors in logs

---

**📝 Note**: This guide covers the complete subscription management functionality. For additional features like chat, video calls, or other non-subscription features, refer to their respective extraction guides.

**🔄 Version**: 1.0  
**📅 Last Updated**: [Current Date]  
**👨‍💻 Maintained By**: Development Team

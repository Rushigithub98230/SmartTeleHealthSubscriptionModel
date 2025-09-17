# 🎯 **Subscription Plan Management Extraction Guide**

## **Focused Guide for Extracting Only Subscription Plan Creation and Management**

This guide provides a **minimal, focused extraction** for **only** the subscription plan creation and management functionality from the SmartTelehealth backend codebase.

---

## 📋 **What This Guide Covers**

✅ **Subscription Plan CRUD Operations**
- Create subscription plans
- Read/List subscription plans (with filtering, pagination, sorting)
- Update subscription plans
- Delete subscription plans
- Activate/Deactivate plans

✅ **Plan Management Features**
- Plan privilege management
- Category-based plan organization
- Advanced filtering and search
- Stripe integration for plan pricing

❌ **What This Guide Does NOT Cover**
- User subscription management
- Billing and payment processing
- Subscription lifecycle management
- User privilege usage tracking
- Analytics and reporting
- Webhook processing

---

## 🎯 **Core Components to Extract**

### **1. Entities (Core Layer) - 4 Entities Only**

```
📁 YourProject.Core/Entities/
├── SubscriptionPlan.cs                    # Main plan entity
├── SubscriptionPlanPrivilege.cs          # Plan-privilege relationships
├── Category.cs                           # Plan categories
└── Privilege.cs                          # Available privileges
```

### **2. DTOs (Application Layer) - 6 DTOs Only**

```
📁 YourProject.Application/DTOs/
├── SubscriptionPlanDto.cs                # Plan data transfer
├── CreateSubscriptionPlanDto.cs          # Plan creation
├── UpdateSubscriptionPlanDto.cs          # Plan updates
├── SubscriptionPlanFilterDto.cs          # Plan filtering
├── CategoryDto.cs                        # Category data
└── JsonModel.cs                          # Standard API response
```

### **3. Interfaces (Core Layer) - 4 Interfaces Only**

```
📁 YourProject.Core/Interfaces/
├── ISubscriptionPlanRepository.cs        # Plan data access
├── ICategoryRepository.cs                # Category data access
├── IPrivilegeRepository.cs               # Privilege data access
└── ISubscriptionPlanPrivilegeRepository.cs # Plan-privilege data access
```

### **4. Services (Application Layer) - 2 Services Only**

```
📁 YourProject.Application/Services/
├── SubscriptionPlanService.cs            # Plan business logic
└── CategoryService.cs                    # Category management
```

### **5. Repositories (Infrastructure Layer) - 4 Repositories Only**

```
📁 YourProject.Infrastructure/Repositories/
├── SubscriptionPlanRepository.cs         # Plan data access
├── CategoryRepository.cs                 # Category data access
├── PrivilegeRepository.cs                # Privilege data access
└── SubscriptionPlanPrivilegeRepository.cs # Plan-privilege data access
```

### **6. Controllers (API Layer) - 2 Controllers Only**

```
📁 YourProject.API/Controllers/
├── SubscriptionPlansController.cs        # Plan management endpoints
└── CategoriesController.cs               # Category management endpoints
```

---

## 🔄 **Step-by-Step Extraction Process**

### **Phase 1: Project Setup**

#### **Step 1.1: Create New Project Structure**
```bash
# Create new solution structure
mkdir YourProject
cd YourProject

# Create projects
dotnet new sln -n YourProject
dotnet new classlib -n YourProject.Core
dotnet new classlib -n YourProject.Application
dotnet new classlib -n YourProject.Infrastructure
dotnet new webapi -n YourProject.API

# Add projects to solution
dotnet sln add YourProject.Core
dotnet sln add YourProject.Application
dotnet sln add YourProject.Infrastructure
dotnet sln add YourProject.API
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

<!-- API Project -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.0.0" />
```

### **Phase 2: Core Layer Migration**

#### **Step 2.1: Copy Core Entities**
Copy these 4 entities from `SmartTelehealth.Core/Entities/`:
1. `SubscriptionPlan.cs`
2. `SubscriptionPlanPrivilege.cs`
3. `Category.cs`
4. `Privilege.cs`

#### **Step 2.2: Copy Core Interfaces**
Copy these 4 interfaces from `SmartTelehealth.Core/Interfaces/`:
1. `ISubscriptionPlanRepository.cs`
2. `ICategoryRepository.cs`
3. `IPrivilegeRepository.cs`
4. `ISubscriptionPlanPrivilegeRepository.cs`

#### **Step 2.3: Copy Supporting Files**
Copy these supporting files:
- `TokenModel.cs` (from Core/DTOs)
- `BaseEntity.cs` (from Core/Entities)

### **Phase 3: Application Layer Migration**

#### **Step 3.1: Copy DTOs**
Copy these 6 DTOs from `SmartTelehealth.Application/DTOs/`:
1. `SubscriptionPlanDto.cs`
2. `CreateSubscriptionPlanDto.cs`
3. `UpdateSubscriptionPlanDto.cs`
4. `SubscriptionPlanFilterDto.cs`
5. `CategoryDto.cs`
6. `JsonModel.cs`

#### **Step 3.2: Copy Service Interfaces**
Copy these 2 service interfaces from `SmartTelehealth.Application/Interfaces/`:
1. `ISubscriptionPlanService.cs`
2. `ICategoryService.cs`

#### **Step 3.3: Copy Services**
Copy these 2 services from `SmartTelehealth.Application/Services/`:
1. `SubscriptionPlanService.cs`
2. `CategoryService.cs`

#### **Step 3.4: Update Mapping Profile**
```csharp
// Create MappingProfile.cs
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Subscription Plan mappings
        CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<SubscriptionPlan, SubscriptionPlanDto>();
        CreateMap<Category, CategoryDto>();
        CreateMap<Privilege, PrivilegeDto>();
    }
}
```

### **Phase 4: Infrastructure Layer Migration**

#### **Step 4.1: Copy Repositories**
Copy these 4 repositories from `SmartTelehealth.Infrastructure/Repositories/`:
1. `SubscriptionPlanRepository.cs`
2. `CategoryRepository.cs`
3. `PrivilegeRepository.cs`
4. `SubscriptionPlanPrivilegeRepository.cs`

#### **Step 4.2: Copy Base Repository**
Copy `RepositoryBase.cs` from `SmartTelehealth.Infrastructure/Repositories/`

#### **Step 4.3: Update DbContext**
```csharp
// Add to your DbContext
public class YourDbContext : DbContext
{
    // Subscription Plan Management DbSets
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<SubscriptionPlanPrivilege> SubscriptionPlanPrivileges { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Privilege> Privileges { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Copy entity configurations from original DbContext
        ConfigureSubscriptionPlan(builder);
        ConfigureSubscriptionPlanPrivilege(builder);
        ConfigureCategory(builder);
        ConfigurePrivilege(builder);
    }

    private void ConfigureSubscriptionPlan(ModelBuilder builder)
    {
        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ShortDescription).HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TrialPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.BillingCycle).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.StripeProductId).HasMaxLength(255);
            entity.Property(e => e.StripePriceId).HasMaxLength(255);
            
            // Relationships
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureSubscriptionPlanPrivilege(ModelBuilder builder)
    {
        builder.Entity<SubscriptionPlanPrivilege>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Relationships
            entity.HasOne(e => e.SubscriptionPlan)
                  .WithMany()
                  .HasForeignKey(e => e.SubscriptionPlanId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Privilege)
                  .WithMany()
                  .HasForeignKey(e => e.PrivilegeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureCategory(ModelBuilder builder)
    {
        builder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ConsultationFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OneTimeConsultationFee).HasColumnType("decimal(18,2)");
        });
    }

    private void ConfigurePrivilege(ModelBuilder builder)
    {
        builder.Entity<Privilege>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50);
        });
    }
}
```

### **Phase 5: API Layer Migration**

#### **Step 5.1: Copy Controllers**
Copy these 2 controllers from `SmartTelehealth.API/Controllers/`:
1. `SubscriptionPlansController.cs`
2. `CategoriesController.cs`

#### **Step 5.2: Copy Base Controller**
Copy `BaseController.cs` from `SmartTelehealth.API/Controllers/`

#### **Step 5.3: Update Startup Configuration**
```csharp
// In Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add Entity Framework
    services.AddDbContext<YourDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Add subscription plan services
    services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
    services.AddScoped<ICategoryService, CategoryService>();
    
    // Add repositories
    services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
    services.AddScoped<ICategoryRepository, CategoryRepository>();
    services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
    services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
    
    // Add AutoMapper
    services.AddAutoMapper(typeof(MappingProfile));
    
    // Add MVC
    services.AddControllers();
    services.AddSwaggerGen();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseAuthorization();
    app.UseEndpoints(endpoints => endpoints.MapControllers());
}
```

---

## 🗄️ **Database Migration**

### **Step 6.1: Create Migration Scripts**
```bash
# Create initial migration
dotnet ef migrations add InitialSubscriptionPlanManagement --project YourProject.Infrastructure --startup-project YourProject.API

# Update database
dotnet ef database update --project YourProject.Infrastructure --startup-project YourProject.API
```

### **Step 6.2: Seed Master Data**
```sql
-- Insert Categories
INSERT INTO Categories (Id, Name, Description, BasePrice, ConsultationFee, OneTimeConsultationFee, IsActive, RequiresHealthAssessment, AllowsMedicationDelivery, AllowsFollowUpMessaging, CreatedDate)
VALUES
    (NEWID(), 'Primary Care', 'General health consultations', 100.00, 100.00, 150.00, 1, 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'Mental Health', 'Therapy and counseling', 150.00, 150.00, 200.00, 1, 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'Dermatology', 'Skin consultations', 120.00, 120.00, 180.00, 1, 0, 1, 1, GETUTCDATE());

-- Insert Privileges
INSERT INTO Privileges (Id, Name, Description, Type, IsActive, CreatedDate)
VALUES
    (NEWID(), 'Video Consultation', 'Video call consultations', 'Consultation', 1, GETUTCDATE()),
    (NEWID(), 'Messaging', 'Text messaging with providers', 'Communication', 1, GETUTCDATE()),
    (NEWID(), 'Medication Delivery', 'Prescription medication delivery', 'Medication', 1, GETUTCDATE()),
    (NEWID(), 'Document Access', 'Access to medical documents', 'Document', 1, GETUTCDATE());
```

---

## 🧪 **Testing the Extraction**

### **Step 7.1: Test API Endpoints**
```bash
# Test plan creation
curl -X POST "https://localhost:5001/api/subscriptionplans/admin" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Basic Plan",
    "description": "Basic healthcare plan",
    "price": 29.99,
    "currency": "USD",
    "billingCycle": "Monthly",
    "categoryId": "your-category-id"
  }'

# Test plan listing
curl -X GET "https://localhost:5001/api/subscriptionplans/active?page=1&pageSize=10"

# Test plan filtering
curl -X POST "https://localhost:5001/api/subscriptionplans/filter" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "searchTerm": "basic",
    "isActive": true
  }'
```

### **Step 7.2: Verify Database**
```sql
-- Check if tables are created
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('SubscriptionPlans', 'Categories', 'Privileges', 'SubscriptionPlanPrivileges');

-- Check if data is seeded
SELECT COUNT(*) FROM Categories;
SELECT COUNT(*) FROM Privileges;
```

---

## ✅ **Post-Extraction Checklist**

### **Step 8.1: Verify Core Functionality**
- [ ] Subscription plan creation works
- [ ] Subscription plan listing works
- [ ] Subscription plan updates work
- [ ] Subscription plan deletion works
- [ ] Plan activation/deactivation works
- [ ] Category management works
- [ ] Privilege management works
- [ ] Advanced filtering works
- [ ] Pagination works
- [ ] Sorting works

### **Step 8.2: Verify API Endpoints**
- [ ] `GET /api/subscriptionplans/active` - Public plan listing
- [ ] `POST /api/subscriptionplans/filter` - Advanced filtering
- [ ] `GET /api/subscriptionplans/admin` - Admin plan listing
- [ ] `POST /api/subscriptionplans/admin` - Create plan
- [ ] `PUT /api/subscriptionplans/admin/{id}` - Update plan
- [ ] `DELETE /api/subscriptionplans/admin/{id}` - Delete plan
- [ ] `POST /api/subscriptionplans/admin/{id}/activate` - Activate plan
- [ ] `POST /api/subscriptionplans/admin/{id}/deactivate` - Deactivate plan
- [ ] `GET /api/categories` - Category listing
- [ ] `POST /api/categories` - Create category

### **Step 8.3: Verify Database**
- [ ] All tables are created correctly
- [ ] Relationships are working
- [ ] Master data is seeded
- [ ] Migrations run successfully

---

## 🎯 **API Endpoints Summary**

### **Subscription Plans Controller**
```
GET    /api/subscriptionplans/active              # Get active plans (public)
POST   /api/subscriptionplans/filter              # Advanced filtering (public)
GET    /api/subscriptionplans/admin               # Get all plans (admin)
GET    /api/subscriptionplans/admin/{id}          # Get plan by ID (admin)
POST   /api/subscriptionplans/admin               # Create plan (admin)
PUT    /api/subscriptionplans/admin/{id}          # Update plan (admin)
DELETE /api/subscriptionplans/admin/{id}          # Delete plan (admin)
POST   /api/subscriptionplans/admin/{id}/activate # Activate plan (admin)
POST   /api/subscriptionplans/admin/{id}/deactivate # Deactivate plan (admin)
```

### **Categories Controller**
```
GET    /api/categories              # Get all categories
GET    /api/categories/{id}         # Get category by ID
GET    /api/categories/active       # Get active categories
GET    /api/categories/paged        # Get paginated categories
POST   /api/categories              # Create category
PUT    /api/categories/{id}         # Update category
DELETE /api/categories/{id}         # Delete category
```

---

## 📊 **File Count Summary**

| Layer | Files | Description |
|-------|-------|-------------|
| **Entities** | 4 | Core business entities |
| **DTOs** | 6 | Data transfer objects |
| **Interfaces** | 4 | Repository interfaces |
| **Services** | 2 | Business logic services |
| **Repositories** | 4 | Data access repositories |
| **Controllers** | 2 | API endpoints |
| **Total** | **22** | **Minimal extraction** |

---

## 🚀 **Benefits of This Focused Extraction**

✅ **Minimal Dependencies** - Only 22 files needed
✅ **No Complex Integrations** - No Stripe, billing, or webhook complexity
✅ **Simple Database** - Only 4 tables required
✅ **Easy to Test** - Straightforward CRUD operations
✅ **Quick Setup** - Can be implemented in hours, not days
✅ **Clean Architecture** - Follows SOLID principles
✅ **Scalable** - Can be extended later if needed

---

## ⚠️ **Important Notes**

1. **No Stripe Integration** - This extraction does not include Stripe payment processing
2. **No User Subscriptions** - This only manages plans, not user subscriptions
3. **No Billing** - No payment or billing functionality included
4. **No Analytics** - No reporting or analytics features
5. **Minimal Dependencies** - Only includes what's absolutely necessary

---

**📝 Note**: This focused extraction provides a clean, minimal subscription plan management system that can be easily integrated into any healthcare project. It includes all the essential CRUD operations and advanced filtering capabilities without the complexity of the full subscription management system.

**🔄 Version**: 1.0  
**📅 Last Updated**: [Current Date]  
**👨‍💻 Maintained By**: Development Team

# 🎯 **SUBSCRIPTION PLAN EXTRACTION CONFIGURATION**

## 📋 **COMPLETE MAPPING CONFIGURATIONS & DEPENDENCY INJECTION**

This document provides all the AutoMapper configurations and dependency injection registrations needed to extract and configure the subscription plan management system in another project.

---

## 🗺️ **AUTOMAPPER CONFIGURATIONS**

### **1. Core Mapping Profile**

```csharp
using AutoMapper;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Mapping;

public class SubscriptionPlanMappingProfile : Profile
{
    public SubscriptionPlanMappingProfile()
    {
        // Subscription Plan Mappings
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.ShortDescription))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src => src.DiscountedPrice))
            .ForMember(dest => dest.DiscountValidUntil, opt => opt.MapFrom(src => src.DiscountValidUntil))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.BillingCycleId))
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.CurrencyId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => src.IsFeatured))
            .ForMember(dest => dest.IsTrialAllowed, opt => opt.MapFrom(src => src.IsTrialAllowed))
            .ForMember(dest => dest.TrialDurationInDays, opt => opt.MapFrom(src => src.TrialDurationInDays))
            .ForMember(dest => dest.IsMostPopular, opt => opt.MapFrom(src => src.IsMostPopular))
            .ForMember(dest => dest.IsTrending, opt => opt.MapFrom(src => src.IsTrending))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.StripeProductId, opt => opt.MapFrom(src => src.StripeProductId))
            .ForMember(dest => dest.StripeMonthlyPriceId, opt => opt.MapFrom(src => src.StripeMonthlyPriceId))
            .ForMember(dest => dest.StripeQuarterlyPriceId, opt => opt.MapFrom(src => src.StripeQuarterlyPriceId))
            .ForMember(dest => dest.StripeAnnualPriceId, opt => opt.MapFrom(src => src.StripeAnnualPriceId))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Terms, opt => opt.MapFrom(src => src.Terms))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.EffectivePrice, opt => opt.MapFrom(src => src.EffectivePrice))
            .ForMember(dest => dest.HasActiveDiscount, opt => opt.MapFrom(src => src.HasActiveDiscount))
            .ForMember(dest => dest.IsCurrentlyAvailable, opt => opt.MapFrom(src => src.IsCurrentlyAvailable))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));

        // Create Subscription Plan Mapping
        CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.ShortDescription))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src => src.DiscountedPrice))
            .ForMember(dest => dest.DiscountValidUntil, opt => opt.MapFrom(src => src.DiscountValidUntil))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.BillingCycleId))
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.CurrencyId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => src.IsFeatured))
            .ForMember(dest => dest.IsTrialAllowed, opt => opt.MapFrom(src => src.IsTrialAllowed))
            .ForMember(dest => dest.TrialDurationInDays, opt => opt.MapFrom(src => src.TrialDurationInDays))
            .ForMember(dest => dest.IsMostPopular, opt => opt.MapFrom(src => src.IsMostPopular))
            .ForMember(dest => dest.IsTrending, opt => opt.MapFrom(src => src.IsTrending))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.StripeProductId, opt => opt.MapFrom(src => src.StripeProductId))
            .ForMember(dest => dest.StripeMonthlyPriceId, opt => opt.MapFrom(src => src.StripeMonthlyPriceId))
            .ForMember(dest => dest.StripeQuarterlyPriceId, opt => opt.MapFrom(src => src.StripeQuarterlyPriceId))
            .ForMember(dest => dest.StripeAnnualPriceId, opt => opt.MapFrom(src => src.StripeAnnualPriceId))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Terms, opt => opt.MapFrom(src => src.Terms))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Update Subscription Plan Mapping
        CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.ShortDescription))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src => src.DiscountedPrice))
            .ForMember(dest => dest.DiscountValidUntil, opt => opt.MapFrom(src => src.DiscountValidUntil))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.BillingCycleId))
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.CurrencyId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => src.IsFeatured))
            .ForMember(dest => dest.IsTrialAllowed, opt => opt.MapFrom(src => src.IsTrialAllowed))
            .ForMember(dest => dest.TrialDurationInDays, opt => opt.MapFrom(src => src.TrialDurationInDays))
            .ForMember(dest => dest.IsMostPopular, opt => opt.MapFrom(src => src.IsMostPopular))
            .ForMember(dest => dest.IsTrending, opt => opt.MapFrom(src => src.IsTrending))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Terms, opt => opt.MapFrom(src => src.Terms))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Category Mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.ConsultationDescription, opt => opt.MapFrom(src => src.ConsultationDescription))
            .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => src.BasePrice))
            .ForMember(dest => dest.ConsultationFee, opt => opt.MapFrom(src => src.ConsultationFee))
            .ForMember(dest => dest.ConsultationDurationMinutes, opt => opt.MapFrom(src => src.ConsultationDurationMinutes))
            .ForMember(dest => dest.RequiresHealthAssessment, opt => opt.MapFrom(src => src.RequiresHealthAssessment))
            .ForMember(dest => dest.AllowsMedicationDelivery, opt => opt.MapFrom(src => src.AllowsMedicationDelivery))
            .ForMember(dest => dest.AllowsFollowUpMessaging, opt => opt.MapFrom(src => src.AllowsFollowUpMessaging))
            .ForMember(dest => dest.AllowsOneTimeConsultation, opt => opt.MapFrom(src => src.AllowsOneTimeConsultation))
            .ForMember(dest => dest.OneTimeConsultationFee, opt => opt.MapFrom(src => src.OneTimeConsultationFee))
            .ForMember(dest => dest.OneTimeConsultationDurationMinutes, opt => opt.MapFrom(src => src.OneTimeConsultationDurationMinutes))
            .ForMember(dest => dest.IsMostPopular, opt => opt.MapFrom(src => src.IsMostPopular))
            .ForMember(dest => dest.IsTrending, opt => opt.MapFrom(src => src.IsTrending))
            .ForMember(dest => dest.SubscriptionPlans, opt => opt.MapFrom(src => src.SubscriptionPlans));

        // Privilege Mappings
        CreateMap<Privilege, PrivilegeDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));

        // Subscription Plan Privilege Mappings
        CreateMap<SubscriptionPlanPrivilege, PlanPrivilegeDto>()
            .ForMember(dest => dest.PrivilegeId, opt => opt.MapFrom(src => src.PrivilegeId))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.UsagePeriodId, opt => opt.MapFrom(src => src.UsagePeriodId))
            .ForMember(dest => dest.DurationMonths, opt => opt.MapFrom(src => src.DurationMonths))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.DailyLimit, opt => opt.MapFrom(src => src.DailyLimit))
            .ForMember(dest => dest.WeeklyLimit, opt => opt.MapFrom(src => src.WeeklyLimit))
            .ForMember(dest => dest.MonthlyLimit, opt => opt.MapFrom(src => src.MonthlyLimit))
            .ForMember(dest => dest.UnitCost, opt => opt.MapFrom(src => src.UnitCost));

        // Master Data Mappings
        CreateMap<MasterBillingCycle, MasterDataDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<MasterCurrency, MasterDataDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        // Subscription Mappings (for validation)
        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.PlanId, opt => opt.MapFrom(src => src.SubscriptionPlanId.ToString()))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.SubscriptionPlan.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));
    }
}
```

---

## 🔧 **DEPENDENCY INJECTION CONFIGURATIONS**

### **1. Application Layer Dependency Injection**

```csharp
using Microsoft.Extensions.DependencyInjection;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;

namespace SmartTelehealth.Application;

public static class SubscriptionPlanDependencyInjection
{
    public static IServiceCollection AddSubscriptionPlanServices(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(SubscriptionPlanDependencyInjection).Assembly);
        
        // Register Core Services
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IPrivilegeService, PrivilegeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        
        // Register Subscription Plan Service with all dependencies
        services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>(provider =>
            new SubscriptionPlanService(
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionPlanRepository>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionPlanPrivilegeRepository>(),
                provider.GetRequiredService<ICategoryService>(),
                provider.GetRequiredService<AutoMapper.IMapper>(),
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SubscriptionPlanService>>(),
                provider.GetRequiredService<IStripeService>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IPrivilegeRepository>(),
                provider.GetRequiredService<INotificationService>(),
                provider.GetRequiredService<IUserService>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionRepository>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IUnitOfWork>()
            )
        );
        
        return services;
    }
}
```

### **2. Infrastructure Layer Dependency Injection**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.Infrastructure.Services;

namespace SmartTelehealth.Infrastructure;

public static class SubscriptionPlanInfrastructureDependencyInjection
{
    public static IServiceCollection AddSubscriptionPlanInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database Configuration
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Register Core Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        
        // Register Subscription Plan Related Repositories
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>(); // For validation
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        
        // Register Master Data Repositories
        services.AddScoped<IMasterBillingCycleRepository, MasterBillingCycleRepository>();
        services.AddScoped<IMasterCurrencyRepository, MasterCurrencyRepository>();
        
        // Register Services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICommunicationService, TwilioService>();
        services.AddScoped<IStripeService, StripeService>();
        
        // Register JWT Service (if needed for authentication)
        services.AddScoped<IJwtService, JwtService>();
        
        return services;
    }
}
```

### **3. API Layer Configuration**

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmartTelehealth.Application;
using SmartTelehealth.Infrastructure;

namespace SmartTelehealth.API;

public static class SubscriptionPlanApiConfiguration
{
    public static IServiceCollection AddSubscriptionPlanApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Infrastructure
        services.AddSubscriptionPlanInfrastructure(configuration);
        
        // Add Application Services
        services.AddSubscriptionPlanServices();
        
        // Add Controllers
        services.AddControllers();
        
        // Add API Documentation (optional)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        return services;
    }
    
    public static WebApplication ConfigureSubscriptionPlanApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
        return app;
    }
}
```

---

## 📦 **REQUIRED PACKAGES**

### **1. NuGet Packages**

```xml
<!-- Core Packages -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />

<!-- Logging -->
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />

<!-- Stripe Integration -->
<PackageReference Include="Stripe.net" Version="44.0.0" />

<!-- Communication Services -->
<PackageReference Include="Twilio" Version="6.0.0" />
<PackageReference Include="SendGrid" Version="9.28.0" />

<!-- JWT Authentication -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />

<!-- API Documentation -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

---

## ⚙️ **CONFIGURATION SETTINGS**

### **1. appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SubscriptionPlanDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "StripeSettings": {
    "SecretKey": "sk_test_your_stripe_secret_key",
    "PublishableKey": "pk_test_your_stripe_publishable_key",
    "WebhookSecret": "whsec_your_webhook_secret"
  },
  "TwilioSettings": {
    "AccountSid": "your_twilio_account_sid",
    "AuthToken": "your_twilio_auth_token",
    "PhoneNumber": "your_twilio_phone_number"
  },
  "SendGridSettings": {
    "ApiKey": "your_sendgrid_api_key",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Your App Name"
  },
  "JwtSettings": {
    "SecretKey": "your_jwt_secret_key_at_least_32_characters_long",
    "Issuer": "YourAppName",
    "Audience": "YourAppUsers",
    "ExpirationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 🚀 **USAGE IN PROGRAM.CS**

```csharp
using SmartTelehealth.API;

var builder = WebApplication.CreateBuilder(args);

// Add Subscription Plan Services
builder.Services.AddSubscriptionPlanApi(builder.Configuration);

var app = builder.Build();

// Configure Subscription Plan API
app.ConfigureSubscriptionPlanApi();

app.Run();
```

---

## 📋 **EXTRACTION CHECKLIST**

### **✅ Files to Extract:**

#### **Core Entities:**
- [ ] `SubscriptionPlan.cs`
- [ ] `MasterBillingCycle.cs`
- [ ] `MasterCurrency.cs`
- [ ] `Category.cs`
- [ ] `Privilege.cs`
- [ ] `SubscriptionPlanPrivilege.cs`
- [ ] `Subscription.cs` (for validation)
- [ ] `BaseEntity.cs`

#### **Interfaces:**
- [ ] `ISubscriptionPlanRepository.cs`
- [ ] `ISubscriptionPlanPrivilegeRepository.cs`
- [ ] `ISubscriptionRepository.cs`
- [ ] `ICategoryRepository.cs`
- [ ] `IPrivilegeRepository.cs`
- [ ] `IUserRepository.cs`
- [ ] `INotificationRepository.cs`
- [ ] `IMasterBillingCycleRepository.cs`
- [ ] `IMasterCurrencyRepository.cs`
- [ ] `IUnitOfWork.cs`
- [ ] `ISubscriptionPlanService.cs`
- [ ] `ICategoryService.cs`
- [ ] `IPrivilegeService.cs`
- [ ] `INotificationService.cs`
- [ ] `IUserService.cs`
- [ ] `IStripeService.cs`
- [ ] `IMasterDataService.cs`

#### **Repositories:**
- [ ] `SubscriptionPlanRepository.cs`
- [ ] `SubscriptionPlanPrivilegeRepository.cs`
- [ ] `SubscriptionRepository.cs`
- [ ] `CategoryRepository.cs`
- [ ] `PrivilegeRepository.cs`
- [ ] `UserRepository.cs`
- [ ] `NotificationRepository.cs`
- [ ] `MasterBillingCycleRepository.cs`
- [ ] `MasterCurrencyRepository.cs`
- [ ] `UnitOfWork.cs`
- [ ] `GenericRepository.cs`

#### **Services:**
- [ ] `SubscriptionPlanService.cs`
- [ ] `CategoryService.cs`
- [ ] `PrivilegeService.cs`
- [ ] `NotificationService.cs`
- [ ] `UserService.cs`
- [ ] `StripeService.cs`
- [ ] `MasterDataService.cs`

#### **DTOs:**
- [ ] `SubscriptionPlanDto.cs`
- [ ] `CreateSubscriptionPlanDto.cs`
- [ ] `UpdateSubscriptionPlanDto.cs`
- [ ] `SubscriptionPlanFilterDto.cs`
- [ ] `PlanPrivilegeDto.cs`
- [ ] `CategoryDto.cs`
- [ ] `PrivilegeDto.cs`
- [ ] `MasterDataDto.cs`
- [ ] `SubscriptionDto.cs` (for validation)

#### **Controllers:**
- [ ] `SubscriptionPlansController.cs`

#### **Database Context:**
- [ ] `ApplicationDbContext.cs`

#### **Configuration Files:**
- [ ] `MappingProfile.cs` (or create new `SubscriptionPlanMappingProfile.cs`)
- [ ] `DependencyInjection.cs` (or create new ones as shown above)

---

## 🎯 **FINAL NOTES**

1. **Database Migration**: Run `dotnet ef migrations add InitialSubscriptionPlanMigration` after setting up the context.

2. **Stripe Configuration**: Ensure Stripe keys are properly configured in appsettings.json.

3. **Authentication**: Configure JWT authentication if needed for admin endpoints.

4. **Logging**: Configure Serilog or other logging providers as needed.

5. **Testing**: Create unit tests for services and integration tests for controllers.

This configuration provides a complete, production-ready subscription plan management system that can be extracted and used in any .NET project! 🚀

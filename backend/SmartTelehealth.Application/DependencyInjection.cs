using Microsoft.Extensions.DependencyInjection;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;

namespace SmartTelehealth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        
        // Register Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProviderService, ProviderService>();
        services.AddScoped<IPrivilegeService, PrivilegeService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>(provider =>
            new SubscriptionService(
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionRepository>(),
                provider.GetRequiredService<AutoMapper.IMapper>(),
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SubscriptionService>>(),
                provider.GetRequiredService<IStripeService>(),
                provider.GetRequiredService<IPrivilegeService>(),
                provider.GetRequiredService<INotificationService>(),
                provider.GetRequiredService<IUserService>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionPlanPrivilegeRepository>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IUserSubscriptionPrivilegeUsageRepository>(),
                provider.GetRequiredService<IBillingService>(),
                provider.GetRequiredService<ISubscriptionNotificationService>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IPrivilegeRepository>(),
                provider.GetRequiredService<ICategoryService>()
            )
        );
        services.AddScoped<IConsultationService, ConsultationService>();
        services.AddScoped<IHealthAssessmentService, HealthAssessmentService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IHomeMedService, HomeMedService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        
        // Register Analytics Service
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        
        // Register Chat Services
        services.AddScoped<IChatStorageService, ChatStorageService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<ChatService>();
        services.AddScoped<ChatRoomService>();
        
        // Register Video Call Services
        services.AddScoped<IVideoCallService, VideoCallService>();
        
        // Register Questionnaire Service
        services.AddScoped<IQuestionnaireService, QuestionnaireService>();
        
        // Register Automated Billing and Lifecycle Services
        services.AddScoped<IAutomatedBillingService, AutomatedBillingService>();
        services.AddScoped<ISubscriptionLifecycleService, SubscriptionLifecycleService>();
        services.AddScoped<ISubscriptionAutomationService, SubscriptionAutomationService>();
        services.AddHostedService<Services.BackgroundServices.SubscriptionBackgroundService>();
        
        // Register Provider Payout Services
        services.AddScoped<IProviderPayoutService, ProviderPayoutService>();
        services.AddScoped<IPayoutPeriodService, PayoutPeriodService>();
        
        // Register New Services
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ISubscriptionAnalyticsService, SubscriptionAnalyticsService>();
        services.AddScoped<ISubscriptionNotificationService, SubscriptionNotificationService>();
        
        return services;
    }
} 
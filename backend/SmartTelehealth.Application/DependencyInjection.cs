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
        // Register Payment Service
        services.AddScoped<IPaymentService, PaymentService>(provider =>
            new PaymentService(
                provider.GetRequiredService<IStripeBillingService>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IBillingRepository>(),
                provider.GetRequiredService<IStripeService>(),
                provider.GetRequiredService<AutoMapper.IMapper>(),
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PaymentService>>()
            )
        );

        // Register Billing Service (updated to use PaymentService)
        services.AddScoped<IBillingService, BillingService>(provider =>
            new BillingService(
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IBillingRepository>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionRepository>(),
                provider.GetRequiredService<IPaymentService>(), // Changed from IStripeBillingService to IPaymentService
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IUserRepository>(),
                provider.GetRequiredService<INotificationService>(),
                provider.GetRequiredService<AutoMapper.IMapper>(),
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<BillingService>>()
            )
        );
        
        // Register Privilege-Based Billing Service
        services.AddScoped<IPrivilegeBasedBillingService, PrivilegeBasedBillingService>();
        services.AddScoped<IHomeMedService, HomeMedService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        
        // Register Analytics Service
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        
        // Register Webhook Idempotency Service
        services.AddScoped<IWebhookIdempotencyService, WebhookIdempotencyService>();
        
        // Register Chat Services
        services.AddScoped<IChatStorageService, ChatStorageService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IChatRoomService, ChatRoomService>();
        
        // Register Video Call Services
        services.AddScoped<IVideoCallService, VideoCallService>();
        
        // Register Questionnaire Service
        services.AddScoped<IQuestionnaireService, QuestionnaireService>();
        
        // Register Automated Billing and Lifecycle Services
        services.AddScoped<IAutomatedBillingService, AutomatedBillingService>();
        services.AddScoped<ISubscriptionLifecycleService, SubscriptionLifecycleService>(provider =>
            new SubscriptionLifecycleService(
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionRepository>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionStatusHistoryRepository>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionPlanRepository>(),
                provider.GetRequiredService<AutoMapper.IMapper>(),
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SubscriptionLifecycleService>>(),
                provider.GetRequiredService<IStripeService>(),
                provider.GetRequiredService<IPrivilegeService>(),
                provider.GetRequiredService<INotificationService>(),
                provider.GetRequiredService<IUserService>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.ISubscriptionPlanPrivilegeRepository>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IUserSubscriptionPrivilegeUsageRepository>(),
                provider.GetRequiredService<IBillingService>(),
                provider.GetRequiredService<ISubscriptionNotificationService>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IPrivilegeRepository>(),
                provider.GetRequiredService<SmartTelehealth.Core.Interfaces.IUnitOfWork>()
            )
        );
        services.AddScoped<ISubscriptionAutomationService, SubscriptionAutomationService>();
        
        // Register Provider Payout Services
        services.AddScoped<IProviderPayoutService, ProviderPayoutService>();
        services.AddScoped<IPayoutPeriodService, PayoutPeriodService>();
        
        // Register New Services
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ISubscriptionAnalyticsService, SubscriptionAnalyticsService>();
        services.AddScoped<ISubscriptionNotificationService, SubscriptionNotificationService>();
        
        // Register Subscription Plan Service
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
        
        // Register Stripe Synchronization Service
        services.AddScoped<IStripeSynchronizationService, StripeSynchronizationService>();
        
        return services;
    }
} 
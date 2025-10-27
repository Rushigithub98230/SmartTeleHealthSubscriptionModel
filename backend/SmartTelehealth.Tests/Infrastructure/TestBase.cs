using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.Infrastructure.Services;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Application.Utilities;
using Moq;
using Microsoft.Extensions.Configuration;

namespace SmartTelehealth.Tests.Infrastructure;

/// <summary>
/// Base class for all integration tests that provides real service implementations
/// with mocked third-party services and real SQL Server database.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ApplicationDbContext DbContext;
    protected readonly IHost Host;
    protected readonly Mock<ILogger> MockLogger;
    protected readonly TestDataBuilder TestData;
    protected readonly TestDatabaseSetup DatabaseSetup;
    protected readonly MasterData MasterData;

    protected TestBase()
    {
        // Create test configuration
        var configuration = CreateTestConfiguration();
        
        // Create host with test services
        Host = CreateTestHost(configuration);
        ServiceProvider = Host.Services;
        
        // Get real database context
        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Create mock logger
        MockLogger = new Mock<ILogger>();
        
        // Setup test database with migrations and seed data
        DatabaseSetup = new TestDatabaseSetup(ServiceProvider.GetRequiredService<ILogger<TestDatabaseSetup>>());
        
        // Initialize test database
        InitializeTestDatabase();
        
        // Get master data for testing
        MasterData = DatabaseSetup.GetMasterDataAsync().Result;
        
        // Create test data builder
        TestData = new TestDataBuilder(DbContext, MasterData);
    }

    /// <summary>
    /// Creates test configuration with real SQL Server database and test settings
    /// </summary>
    private IConfiguration CreateTestConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "ConnectionStrings:DefaultConnection", "Server=(localdb)\\MSSQLLocalDB;Database=SmartTelehealth_Test;Trusted_Connection=true;MultipleActiveResultSets=true" },
            { "Stripe:SecretKey", "sk_test_mock_key" },
            { "Stripe:PublishableKey", "pk_test_mock_key" },
            { "Stripe:WebhookSecret", "whsec_mock_secret" },
            { "Jwt:SecretKey", "test_secret_key_that_is_long_enough_for_jwt" },
            { "Jwt:Issuer", "SmartTelehealth_Test" },
            { "Jwt:Audience", "SmartTelehealth_Test_Audience" },
            { "Jwt:ExpiryMinutes", "60" },
            { "Logging:LogLevel:Default", "Information" },
            { "Logging:LogLevel:Microsoft", "Warning" },
            { "Logging:LogLevel:System", "Warning" }
        });
        
        return configurationBuilder.Build();
    }

    /// <summary>
    /// Creates test host with real services and mocked third-party services
    /// </summary>
    private IHost CreateTestHost(IConfiguration configuration)
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Add configuration
                services.AddSingleton(configuration);
                
                // Add real database context
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
                
                // Add real repositories
                services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
                services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
                services.AddScoped<IBillingRepository, BillingRepository>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
                services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
                services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();
                services.AddScoped<ISubscriptionStatusHistoryRepository, SubscriptionStatusHistoryRepository>();
                services.AddScoped<ISubscriptionPaymentRepository, SubscriptionPaymentRepository>();
                services.AddScoped<IBillingAdjustmentRepository, BillingAdjustmentRepository>();
                services.AddScoped<IPaymentRefundRepository, PaymentRefundRepository>();
                services.AddScoped<IProcessedWebhookEventRepository, ProcessedWebhookEventRepository>();
                services.AddScoped<IMasterBillingCycleRepository, MasterBillingCycleRepository>();
                services.AddScoped<IMasterCurrencyRepository, MasterCurrencyRepository>();
                services.AddScoped<IMasterPrivilegeTypeRepository, MasterPrivilegeTypeRepository>();
                services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();
                services.AddScoped<ICategoryRepository, CategoryRepository>();
                services.AddScoped<IProviderOnboardingRepository, ProviderOnboardingRepository>();
                services.AddScoped<IProviderFeeRepository, ProviderFeeRepository>();
                services.AddScoped<IVideoCallRepository, VideoCallRepository>();
                services.AddScoped<IMessageRepository, MessageRepository>();
                services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
                services.AddScoped<IChatRoomParticipantRepository, ChatRoomParticipantRepository>();
                services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
                services.AddScoped<IMessageReactionRepository, MessageReactionRepository>();
                services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
                services.AddScoped<IStripeSyncHistoryRepository, StripeSyncHistoryRepository>();
                services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
                
                // Add real services
                services.AddScoped<ISubscriptionService, SubscriptionService>();
                services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
                services.AddScoped<ISubscriptionBillingService, SubscriptionBillingService>();
                services.AddScoped<ISubscriptionLifecycleService, SubscriptionLifecycleService>();
                services.AddScoped<IAutomatedBillingService, AutomatedBillingService>();
                services.AddScoped<IPaymentService, PaymentService>();
                services.AddScoped<IPlanPricingService, PlanPricingService>();
                services.AddScoped<IPlanVersioningService, PlanVersioningService>();
                services.AddScoped<ISubscriptionAutomationService, SubscriptionAutomationService>();
                services.AddScoped<INotificationService, NotificationService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IPrivilegeService, PrivilegeService>();
                services.AddScoped<ICategoryService, CategoryService>();
                services.AddScoped<IProviderOnboardingService, ProviderOnboardingService>();
                services.AddScoped<IProviderFeeService, ProviderFeeService>();
                services.AddScoped<IVideoCallService, VideoCallService>();
                services.AddScoped<IMessageService, MessageService>();
                services.AddScoped<IChatRoomService, ChatRoomService>();
                services.AddScoped<IPrescriptionService, PrescriptionService>();
                services.AddScoped<IChatSessionService, ChatSessionService>();
                services.AddScoped<IStripeSynchronizationService, StripeSynchronizationService>();
                services.AddScoped<IWebhookService, WebhookService>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                
                // Add utility services
                services.AddScoped<CurrencyService>();
                
                // Add mocked third-party services
                services.AddScoped<IStripeService>(provider => CreateMockStripeService());
                services.AddScoped<IEmailService>(provider => CreateMockEmailService());
                services.AddScoped<ISmsService>(provider => CreateMockSmsService());
                services.AddScoped<IPushNotificationService>(provider => CreateMockPushNotificationService());
                
                // Add logging
                services.AddLogging(builder => builder.AddConsole().AddDebug());
            });

        return hostBuilder.Build();
    }

    /// <summary>
    /// Creates mock Stripe service for testing
    /// </summary>
    private IStripeService CreateMockStripeService()
    {
        var mockStripeService = new Mock<IStripeService>();
        
        // Mock successful payment processing
        mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(new PaymentResultDto
            {
                Status = "succeeded",
                TransactionId = Guid.NewGuid().ToString(),
                Amount = 100.00m,
                Currency = "usd",
                ErrorMessage = null
            });
        
        // Mock customer creation
        mockStripeService.Setup(x => x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync("cus_test_customer_id");
        
        // Mock subscription creation
        mockStripeService.Setup(x => x.CreateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync("sub_test_subscription_id");
        
        // Mock price creation
        mockStripeService.Setup(x => x.CreatePriceAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TokenModel>()))
            .ReturnsAsync("price_test_price_id");
        
        // Mock product creation
        mockStripeService.Setup(x => x.CreateProductAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync("prod_test_product_id");
        
        return mockStripeService.Object;
    }

    /// <summary>
    /// Creates mock email service for testing
    /// </summary>
    private IEmailService CreateMockEmailService()
    {
        var mockEmailService = new Mock<IEmailService>();
        
        // Mock successful email sending
        mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(new JsonModel { StatusCode = 200, Message = "Email sent successfully" });
        
        return mockEmailService.Object;
    }

    /// <summary>
    /// Creates mock SMS service for testing
    /// </summary>
    private ISmsService CreateMockSmsService()
    {
        var mockSmsService = new Mock<ISmsService>();
        
        // Mock successful SMS sending
        mockSmsService.Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(new JsonModel { StatusCode = 200, Message = "SMS sent successfully" });
        
        return mockSmsService.Object;
    }

    /// <summary>
    /// Creates mock push notification service for testing
    /// </summary>
    private IPushNotificationService CreateMockPushNotificationService()
    {
        var mockPushService = new Mock<IPushNotificationService>();
        
        // Mock successful push notification sending
        mockPushService.Setup(x => x.SendPushNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(new JsonModel { StatusCode = 200, Message = "Push notification sent successfully" });
        
        return mockPushService.Object;
    }

    /// <summary>
    /// Initializes test database with clean state
    /// </summary>
    private void InitializeTestDatabase()
    {
        // Setup database with migrations and seed data
        DatabaseSetup.SetupAsync().Wait();
    }


    /// <summary>
    /// Gets a service from the DI container
    /// </summary>
    protected T GetService<T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a mock service for verification
    /// </summary>
    protected Mock<T> GetMockService<T>() where T : class
    {
        var service = ServiceProvider.GetRequiredService<T>();
        return Mock.Get(service);
    }

    /// <summary>
    /// Creates a test token model
    /// </summary>
    protected TokenModel CreateTestToken(Guid? userId = null, int? roleId = null)
    {
        return new TokenModel
        {
            UserID = userId ?? Guid.NewGuid(),
            RoleID = roleId ?? 1, // Default to admin role
            Email = "test@example.com",
            FullName = "Test User"
        };
    }

    /// <summary>
    /// Saves changes to database and returns the count
    /// </summary>
    protected async Task<int> SaveChangesAsync()
    {
        return await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public virtual void Dispose()
    {
        DbContext?.Dispose();
        Host?.Dispose();
    }
}


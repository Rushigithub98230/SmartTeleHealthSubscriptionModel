using Microsoft.EntityFrameworkCore;
using Moq;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;
using SmartTelehealth.Tests.Integration.Billing;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Infrastructure.Services;

namespace SmartTelehealth.Tests.Integration.Subscription;

/// <summary>
/// Comprehensive test base with ALL subscription services properly initialized
/// Uses REAL service implementations for core business logic
/// Mocks only external integrations (Stripe, Notifications)
/// </summary>
public abstract class ComprehensiveTestBase : IDisposable
{
    protected readonly ApplicationDbContext _context;
    protected readonly TestDataBuilder _testDataBuilder;
    protected readonly TokenModel _adminToken;
    protected readonly TokenModel _userToken;

    // Mocks (for external/infrastructure concerns only)
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;
    protected readonly Mock<IStripeService> _stripeServiceMock;
    protected readonly Mock<INotificationService> _notificationServiceMock;
    protected readonly Mock<IUserService> _userServiceMock;
    protected readonly Mock<ISubscriptionNotificationService> _subscriptionNotificationServiceMock;
    protected readonly Mock<ICategoryService> _categoryServiceMock;
    protected readonly Mock<IPlanPricingService> _planPricingServiceMock;

    // Repositories (ALL real implementations)
    protected readonly ISubscriptionRepository _subscriptionRepository;
    protected readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    protected readonly IPrivilegeRepository _privilegeRepository;
    protected readonly IUserSubscriptionPrivilegeUsageRepository _privilegeUsageRepository;
    protected readonly IBillingRepository _billingRepository;
    protected readonly ISubscriptionPaymentRepository _subscriptionPaymentRepository;
    protected readonly IUserRepository _userRepository;
    protected readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepository;
    protected readonly IPrivilegeUsageHistoryRepository _usageHistoryRepository;
    protected readonly ISubscriptionStatusHistoryRepository _statusHistoryRepository;

    // Services (ALL real implementations - this is what we're testing!)
    protected readonly IStripeBillingService _stripeBillingService;
    protected readonly IPaymentService _paymentService;
    protected readonly IPrivilegeService _privilegeService;
    protected readonly ISubscriptionBillingService _subscriptionBillingService;
    protected readonly IAutomatedBillingService _automatedBillingService;
    protected readonly ISubscriptionLifecycleService _subscriptionLifecycleService;
    protected readonly ISubscriptionService _subscriptionService;

    protected readonly IMapper _mapper;

    public ComprehensiveTestBase()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        // Setup tokens
        _adminToken = new TokenModel { UserID = 1, RoleID = 1, Role = "Admin", Email = "admin@test.com" };
        _userToken = new TokenModel { UserID = 2, RoleID = 2, Role = "User", Email = "user@test.com" };

        // Setup mocks
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _stripeServiceMock = new Mock<IStripeService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _userServiceMock = new Mock<IUserService>();
        _subscriptionNotificationServiceMock = new Mock<ISubscriptionNotificationService>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _planPricingServiceMock = new Mock<IPlanPricingService>();

        SetupMocks();

        // Setup mapper (with proper mapping profiles if needed)
        var mapperConfig = new MapperConfiguration(cfg => {
            // Add mapping profiles here if needed
        });
        _mapper = mapperConfig.CreateMapper();

        // Initialize ALL repositories
        _subscriptionRepository = new SubscriptionRepository(_context);
        _subscriptionPlanRepository = new SubscriptionPlanRepository(_context);
        _privilegeRepository = new PrivilegeRepository(_context);
        _privilegeUsageRepository = new UserSubscriptionPrivilegeUsageRepository(_context);
        _billingRepository = new BillingRepository(_context);
        _subscriptionPaymentRepository = new SubscriptionPaymentRepository(_context);
        _userRepository = new UserRepository(_context);
        _planPrivilegeRepository = new SubscriptionPlanPrivilegeRepository(_context);
        _usageHistoryRepository = new PrivilegeUsageHistoryRepository(_context);
        _statusHistoryRepository = new SubscriptionStatusHistoryRepository(_context);

        // Initialize services in dependency order
        InitializeServices();

        // Initialize test data builder
        _testDataBuilder = new TestDataBuilder(_context, _adminToken.UserID);

        SeedBasicData();
    }

    private void SetupMocks()
    {
        // Setup UnitOfWork mock
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Setup Stripe mocks (external service)
        _stripeServiceMock.Setup(x => x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync((string email, string name, TokenModel token) => $"cus_test_{Guid.NewGuid().ToString().Substring(0, 8)}");

        _stripeServiceMock.Setup(x => x.CreateSubscriptionAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync((string customerId, string priceId, string paymentMethodId, TokenModel token) =>
                $"sub_test_{Guid.NewGuid().ToString().Substring(0, 8)}");

        _stripeServiceMock.Setup(x => x.CancelSubscriptionAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(true);

        _stripeServiceMock.Setup(x => x.UpdateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(true);

        // Setup Notification service mock
        _notificationServiceMock.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(new JsonModel { StatusCode = 200, Message = "Notification sent" });

        // Setup UserService mock
        _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<TokenModel>()))
            .ReturnsAsync((int userId, TokenModel token) => new JsonModel
            {
                StatusCode = 200,
                data = new { Id = userId, Email = $"user{userId}@test.com", FirstName = "Test", LastName = $"User{userId}" }
            });

        // Setup CategoryService mock
        _categoryServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(new JsonModel { StatusCode = 200, data = new { Id = Guid.NewGuid(), Name = "Test Category" } });

        // Setup PlanPricingService mock
        _planPricingServiceMock.Setup(x => x.CalculatePlanPriceAsync(It.IsAny<Guid>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(new JsonModel { StatusCode = 200, data = new { Price = 99.99m } });
    }

    private void InitializeServices()
    {
        // 1. StripeBillingService (low-level Stripe operations)
        _stripeBillingService = new StripeBillingService(
            _billingRepository,
            _subscriptionRepository,
            _stripeServiceMock.Object,
            _notificationServiceMock.Object,
            _userRepository,
            _unitOfWorkMock.Object,
            new Mock<ILogger<StripeBillingService>>().Object
        );

        // 2. PrivilegeService (REAL - core business logic)
        _privilegeService = new PrivilegeService(
            _privilegeRepository,
            _planPrivilegeRepository,
            _privilegeUsageRepository,
            _usageHistoryRepository,
            _subscriptionRepository,
            new Mock<ILogger<PrivilegeService>>().Object
        );

        // 3. PaymentService (REAL - core business logic)
        _paymentService = new PaymentService(
            _stripeBillingService,
            _billingRepository,
            _stripeServiceMock.Object,
            _mapper,
            new Mock<ILogger<PaymentService>>().Object,
            _subscriptionPaymentRepository,
            _subscriptionRepository,
            _unitOfWorkMock.Object
        );

        // 4. SubscriptionBillingService (REAL - core business logic)
        _subscriptionBillingService = new SubscriptionBillingService(
            _unitOfWorkMock.Object,
            _billingRepository,
            _subscriptionRepository,
            _subscriptionPlanRepository,
            _privilegeUsageRepository,
            _privilegeRepository,
            _userRepository,
            _paymentService,
            _stripeServiceMock.Object,
            _notificationServiceMock.Object,
            _planPricingServiceMock.Object,
            _mapper,
            new Mock<ILogger<SubscriptionBillingService>>().Object
        );

        // 5. AutomatedBillingService (REAL - core business logic)
        _automatedBillingService = new AutomatedBillingService(
            _subscriptionRepository,
            _subscriptionPlanRepository,
            _subscriptionBillingService,
            _stripeServiceMock.Object,
            _usageHistoryRepository,
            _privilegeUsageRepository,
            _unitOfWorkMock.Object,
            new Mock<ILogger<AutomatedBillingService>>().Object,
            _notificationServiceMock.Object,
            _userRepository,
            _billingRepository,
            _subscriptionPaymentRepository
        );

        // 6. SubscriptionLifecycleService (REAL - core business logic)
        _subscriptionLifecycleService = new SubscriptionLifecycleService(
            _subscriptionRepository,
            _statusHistoryRepository,
            _subscriptionPlanRepository,
            _mapper,
            new Mock<ILogger<SubscriptionLifecycleService>>().Object,
            _stripeServiceMock.Object,
            _privilegeService,
            _notificationServiceMock.Object,
            _userServiceMock.Object,
            _planPrivilegeRepository,
            _privilegeUsageRepository,
            _subscriptionBillingService,
            _subscriptionNotificationServiceMock.Object,
            _privilegeRepository,
            _unitOfWorkMock.Object
        );

        // 7. SubscriptionService (REAL - core business logic)
        _subscriptionService = new SubscriptionService(
            _subscriptionRepository,
            _mapper,
            new Mock<ILogger<SubscriptionService>>().Object,
            _stripeServiceMock.Object,
            _privilegeService,
            _notificationServiceMock.Object,
            _userServiceMock.Object,
            _planPrivilegeRepository,
            _privilegeUsageRepository,
            _subscriptionBillingService,
            _subscriptionNotificationServiceMock.Object,
            _privilegeRepository,
            _categoryServiceMock.Object,
            _unitOfWorkMock.Object,
            _paymentService
        );
    }

    protected void SeedBasicData()
    {
        // Seed essential master data
        if (!_context.MasterCurrencies.Any())
        {
            _context.MasterCurrencies.Add(new MasterCurrency
            {
                Id = Guid.NewGuid(),
                Name = "USD",
                Code = "USD",
                Symbol = "$",
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });
        }

        if (!_context.Users.Any())
        {
            _context.Users.AddRange(
                new User
                {
                    Id = 1,
                    Email = "admin@test.com",
                    FirstName = "Admin",
                    LastName = "User",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = 1
                },
                new User
                {
                    Id = 2,
                    Email = "user@test.com",
                    FirstName = "Test",
                    LastName = "User",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = 1
                }
            );
        }

        _context.SaveChanges();
    }

    #region Helper Methods for Service-Level Testing

    /// <summary>
    /// Helper to verify payment processed successfully through PaymentService
    /// </summary>
    protected async Task<bool> VerifyPaymentProcessedAsync(Guid billingRecordId)
    {
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        return billingRecord != null && billingRecord.Status == BillingRecord.BillingStatus.Paid;
    }

    /// <summary>
    /// Helper to verify privileges were reset through service logic
    /// </summary>
    protected async Task<bool> VerifyPrivilegesResetAsync(Guid subscriptionId)
    {
        var usages = await _privilegeUsageRepository.GetBySubscriptionIdAsync(subscriptionId);
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);

        if (subscription == null) return false;

        foreach (var usage in usages)
        {
            // Skip unlimited privileges
            if (usage.AllowedValue == -1) continue;

            // Check reset conditions
            if (usage.UsedValue != 0) return false;
            if (usage.UsagePeriodStart != (subscription.LastBillingDate ?? subscription.StartDate)) return false;
            if (usage.UsagePeriodEnd != subscription.NextBillingDate) return false;
            if (!usage.ResetAt.HasValue) return false;
        }

        return true;
    }

    /// <summary>
    /// Helper to calculate expected next billing date
    /// </summary>
    protected DateTime CalculateExpectedNextBillingDate(DateTime baseDate, string billingCycleName, int durationInDays)
    {
        return billingCycleName.ToLower() switch
        {
            "monthly" => baseDate.AddMonths(1),
            "quarterly" => baseDate.AddMonths(3),
            "yearly" or "annual" => baseDate.AddYears(1),
            "weekly" => baseDate.AddDays(7),
            "daily" => baseDate.AddDays(1),
            _ => baseDate.AddDays(durationInDays)
        };
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}


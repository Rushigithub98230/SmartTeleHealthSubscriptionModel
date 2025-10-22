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
/// Clean test base for subscription integration testing with properly configured services
/// </summary>
public abstract class CleanTestBase : IDisposable
{
    protected readonly ApplicationDbContext _context;
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;
    protected readonly Mock<IStripeService> _stripeServiceMock;
    protected readonly IMapper _mapper;
    protected readonly TestDataBuilder _testDataBuilder;
    protected readonly TokenModel _adminToken;
    protected readonly TokenModel _userToken;

    // Repositories
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

    // Services
    protected readonly IPaymentService _paymentService;
    protected readonly IPrivilegeService _privilegeService;

    public CleanTestBase()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        // Setup mocks
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _stripeServiceMock = new Mock<IStripeService>();

        // Setup UnitOfWork
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Setup mapper
        var mapperConfig = new MapperConfiguration(cfg => { });
        _mapper = mapperConfig.CreateMapper();

        // Initialize repositories
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

        // Setup Stripe mocks
        SetupDefaultStripeMocks();

        // Initialize services
        var stripeBillingService = new StripeBillingService(
            _billingRepository,
            _subscriptionRepository,
            _stripeServiceMock.Object,
            new Mock<INotificationService>().Object,
            _userRepository,
            _unitOfWorkMock.Object,
            new Mock<ILogger<StripeBillingService>>().Object
        );

        _paymentService = new PaymentService(
            stripeBillingService,
            _billingRepository,
            _stripeServiceMock.Object,
            _mapper,
            new Mock<ILogger<PaymentService>>().Object,
            _subscriptionPaymentRepository,
            _subscriptionRepository,
            _unitOfWorkMock.Object
        );

        _privilegeService = new PrivilegeService(
            _privilegeRepository,
            _planPrivilegeRepository,
            _privilegeUsageRepository,
            _usageHistoryRepository,
            _subscriptionRepository,
            new Mock<ILogger<PrivilegeService>>().Object
        );

        // Setup tokens
        _adminToken = new TokenModel { UserID = 1, RoleID = 1, Role = "Admin", Email = "admin@test.com" };
        _userToken = new TokenModel { UserID = 2, RoleID = 2, Role = "User", Email = "user@test.com" };

        // Initialize test data builder
        _testDataBuilder = new TestDataBuilder(_context, _adminToken.UserID);

        SeedBasicData();
    }

    protected virtual void SetupDefaultStripeMocks()
    {
        // Mock successful Stripe operations by default
        _stripeServiceMock.Setup(x => x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync((string email, string name, TokenModel token) => $"cus_test_{Guid.NewGuid().ToString().Substring(0, 8)}");

        _stripeServiceMock.Setup(x => x.CreateSubscriptionAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TokenModel>()))
            .ReturnsAsync((string customerId, string priceId, string paymentMethodId, TokenModel token) =>
                $"sub_test_{Guid.NewGuid().ToString().Substring(0, 8)}");

        _stripeServiceMock.Setup(x => x.CancelSubscriptionAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(true);
    }

    protected void SeedBasicData()
    {
        // Seed basic master data if not exists
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
            _context.Users.Add(new User
            {
                Id = 1,
                Email = "admin@test.com",
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1
            });
            
            _context.Users.Add(new User
            {
                Id = 2,
                Email = "user@test.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1
            });
        }

        _context.SaveChanges();
    }

    #region Helper Methods

    /// <summary>
    /// Creates a subscription with payment processing
    /// </summary>
    protected async Task<Core.Entities.Subscription> CreateSubscriptionWithPaymentAsync(
        User user,
        SubscriptionPlan plan,
        MasterBillingCycle billingCycle,
        bool processPayment = true)
    {
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, billingCycle);

        if (processPayment)
        {
            // Create billing record
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SubscriptionId = subscription.Id,
                Amount = plan.Price,
                TotalAmount = plan.Price,
                TaxAmount = 0,
                ShippingAmount = 0,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                BillingDate = DateTime.UtcNow,
                DueDate = subscription.NextBillingDate,
                CurrencyId = plan.CurrencyId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = user.Id
            };
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();

            // Process payment
            await _paymentService.ProcessPaymentAsync(billingRecord.Id, _userToken);
        }

        return subscription;
    }

    /// <summary>
    /// Advances subscription to next billing date
    /// </summary>
    protected async Task AdvanceTimeToBillingDateAsync(Core.Entities.Subscription subscription)
    {
        var billingCycle = await _context.MasterBillingCycles.FindAsync(subscription.BillingCycleId);
        if (billingCycle == null) return;

        subscription.LastBillingDate = subscription.NextBillingDate;
        subscription.NextBillingDate = CalculateNextBillingDate(subscription.NextBillingDate, billingCycle);
        subscription.UpdatedDate = DateTime.UtcNow;
        subscription.UpdatedBy = _adminToken.UserID;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }

    private DateTime CalculateNextBillingDate(DateTime baseDate, MasterBillingCycle billingCycle)
    {
        return billingCycle.Name.ToLower() switch
        {
            "monthly" => baseDate.AddMonths(1),
            "quarterly" => baseDate.AddMonths(3),
            "yearly" or "annual" => baseDate.AddYears(1),
            "weekly" => baseDate.AddDays(7),
            "daily" => baseDate.AddDays(1),
            _ => baseDate.AddDays(billingCycle.DurationInDays)
        };
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}


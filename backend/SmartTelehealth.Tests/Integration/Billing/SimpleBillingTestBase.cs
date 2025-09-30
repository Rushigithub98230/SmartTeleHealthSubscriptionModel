using Microsoft.EntityFrameworkCore;
using Moq;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.Infrastructure.Services;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Enums;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Services;
using AutoMapper;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Tests.Integration.Billing;

/// <summary>
/// Simple base class for billing integration tests without WebApplicationFactory
/// </summary>
public abstract class SimpleBillingTestBase : IDisposable
{
    protected readonly ApplicationDbContext _context;
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;
    protected readonly Mock<IStripeService> _stripeServiceMock;
    protected readonly Mock<ILogger<PrivilegeBasedBillingService>> _privilegeBasedBillingServiceLoggerMock;
    protected readonly Mock<ILogger<AutomatedBillingService>> _automatedBillingServiceLoggerMock;
    protected readonly Mock<ILogger<BillingService>> _billingServiceLoggerMock;
    protected readonly Mock<ILogger<StripeBillingService>> _stripeBillingServiceLoggerMock;

    protected readonly ISubscriptionRepository _subscriptionRepository;
    protected readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    protected readonly IPrivilegeRepository _privilegeRepository;
    protected readonly IUserSubscriptionPrivilegeUsageRepository _privilegeUsageRepository;
    protected readonly IBillingRepository _billingRepository;

    protected readonly IPrivilegeBasedBillingService _privilegeBasedBillingService;
    protected readonly IAutomatedBillingService _automatedBillingService;
    protected readonly IBillingService _billingService;
    protected readonly IStripeBillingService _stripeBillingService;
    protected readonly IMapper _mapper;

    protected TokenModel _adminToken;
    protected TokenModel _userToken;

    public SimpleBillingTestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated(); // Ensure the in-memory database is created

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _stripeServiceMock = new Mock<IStripeService>();
        _privilegeBasedBillingServiceLoggerMock = new Mock<ILogger<PrivilegeBasedBillingService>>();
        _automatedBillingServiceLoggerMock = new Mock<ILogger<AutomatedBillingService>>();
        _billingServiceLoggerMock = new Mock<ILogger<BillingService>>();
        _stripeBillingServiceLoggerMock = new Mock<ILogger<StripeBillingService>>();

        // Repositories
        _subscriptionRepository = new SubscriptionRepository(_context);
        _subscriptionPlanRepository = new SubscriptionPlanRepository(_context);
        _privilegeRepository = new PrivilegeRepository(_context);
        _privilegeUsageRepository = new UserSubscriptionPrivilegeUsageRepository(_context);
        _billingRepository = new BillingRepository(_context);

        // Create a simple mock mapper
        var mapperConfig = new MapperConfiguration(cfg => { });
        _mapper = mapperConfig.CreateMapper();

        // Create StripeBillingService first
        _stripeBillingService = new StripeBillingService(
            _billingRepository,
            _subscriptionRepository,
            _stripeServiceMock.Object,
            new Mock<INotificationService>().Object,
            new Mock<IUserRepository>().Object,
            _unitOfWorkMock.Object,
            _stripeBillingServiceLoggerMock.Object
        );

        // Create and setup PaymentService mock
        var paymentServiceMock = new Mock<IPaymentService>();
        paymentServiceMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<Guid>(), It.IsAny<TokenModel>()))
            .Returns(async (Guid billingRecordId, TokenModel tokenModel) =>
            {
                // Actually update the billing record status in the database
                var billingRecord = await _context.BillingRecords.FindAsync(billingRecordId);
                if (billingRecord != null)
                {
                    billingRecord.Status = BillingRecord.BillingStatus.Paid;
                    billingRecord.UpdatedBy = tokenModel.UserID;
                    billingRecord.UpdatedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                
                return new JsonModel 
                { 
                    data = new { Success = true, Message = "Payment processed successfully" },
                    Message = "Payment processed successfully",
                    StatusCode = 200
                };
            });
        paymentServiceMock.Setup(x => x.ProcessRefundAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<TokenModel>()))
            .Returns(async (Guid billingRecordId, decimal amount, TokenModel tokenModel) =>
            {
                // Actually update the billing record status in the database
                var billingRecord = await _context.BillingRecords.FindAsync(billingRecordId);
                if (billingRecord != null)
                {
                    billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                    billingRecord.UpdatedBy = tokenModel.UserID;
                    billingRecord.UpdatedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                
                return new JsonModel 
                { 
                    data = new { Success = true, Message = "Refund processed successfully" },
                    Message = "Refund processed successfully",
                    StatusCode = 200
                };
            });

        // Create BillingService
        _billingService = new BillingService(
            _billingRepository,
            _subscriptionRepository,
            paymentServiceMock.Object,
            new Mock<IUserRepository>().Object,
            new Mock<INotificationService>().Object,
            _mapper,
            _billingServiceLoggerMock.Object
        );

        // Create PrivilegeBasedBillingService
        _privilegeBasedBillingService = new PrivilegeBasedBillingService(
            _unitOfWorkMock.Object,
            _billingRepository,
            _subscriptionRepository,
            _subscriptionPlanRepository,
            _privilegeUsageRepository,
            _privilegeRepository,
            _stripeServiceMock.Object,
            _mapper,
            _privilegeBasedBillingServiceLoggerMock.Object
        );

        // Create AutomatedBillingService
        _automatedBillingService = new AutomatedBillingService(
            _subscriptionRepository,
            _subscriptionPlanRepository,
            _billingService,
            _stripeServiceMock.Object,
            new Mock<IPrivilegeUsageHistoryRepository>().Object,
            _privilegeUsageRepository,
            _unitOfWorkMock.Object,
            _automatedBillingServiceLoggerMock.Object,
            new Mock<INotificationService>().Object,
            new Mock<IUserRepository>().Object,
            _billingRepository
        );

        // Setup UnitOfWork mocks
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        // Setup TokenModels
        _adminToken = new TokenModel { UserID = 1, RoleID = 1, Role = "Admin", Email = "admin@example.com" };
        _userToken = new TokenModel { UserID = 2, RoleID = 2, Role = "User", Email = "user@example.com" };

        SeedDatabase();
    }

    protected void SeedDatabase()
    {
        // Clear existing data to ensure idempotency for each test
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Seed Master Data
        var masterCurrency = new MasterCurrency { Id = Guid.NewGuid(), Name = "USD", Code = "USD", Symbol = "$", IsActive = true, CreatedBy = 1, CreatedDate = DateTime.UtcNow };
        var masterBillingCycleMonthly = new MasterBillingCycle { Id = Guid.NewGuid(), Name = "Monthly", DurationInDays = 30, IsActive = true, CreatedBy = 1, CreatedDate = DateTime.UtcNow };
        var masterBillingCycleYearly = new MasterBillingCycle { Id = Guid.NewGuid(), Name = "Yearly", DurationInDays = 365, IsActive = true, CreatedBy = 1, CreatedDate = DateTime.UtcNow };
        var masterPrivilegeType = new MasterPrivilegeType { Id = Guid.NewGuid(), Name = "Consultation", Description = "Consultation privilege", IsActive = true, CreatedBy = 1, CreatedDate = DateTime.UtcNow };

        _context.MasterCurrencies.Add(masterCurrency);
        _context.MasterBillingCycles.Add(masterBillingCycleMonthly);
        _context.MasterBillingCycles.Add(masterBillingCycleYearly);
        _context.MasterPrivilegeTypes.Add(masterPrivilegeType);

        // Seed Users
        var adminUser = new User { Id = 1, Email = "admin@example.com", UserName = "admin", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 };
        var regularUser = new User { Id = 2, Email = "user@example.com", UserName = "user", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 };
        _context.Users.AddRange(adminUser, regularUser);

        // Seed Privileges
        var consultationPrivilege = new Privilege { Id = Guid.NewGuid(), Name = "Consultation", Description = "Allows booking consultations", PrivilegeTypeId = masterPrivilegeType.Id, IsActive = true, CreatedBy = 1, CreatedDate = DateTime.UtcNow };
        var medicationPrivilege = new Privilege { Id = Guid.NewGuid(), Name = "Medication", Description = "Allows ordering medication", PrivilegeTypeId = masterPrivilegeType.Id, IsActive = true, CreatedBy = 1, CreatedDate = DateTime.UtcNow };
        _context.Privileges.AddRange(consultationPrivilege, medicationPrivilege);

        // Seed Subscription Plans
        var basicPlan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Basic Plan",
            Description = "Basic health plan with limited consultations",
            Price = 100.00m,
            BillingCycleId = masterBillingCycleMonthly.Id,
            CurrencyId = masterCurrency.Id,
            IsActive = true,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            PlanType = PlanType.Standard
        };
        _context.SubscriptionPlans.Add(basicPlan);

        // Seed Subscription Plan Privileges
        _context.SubscriptionPlanPrivileges.Add(new SubscriptionPlanPrivilege
        {
            Id = Guid.NewGuid(),
            SubscriptionPlanId = basicPlan.Id,
            PrivilegeId = consultationPrivilege.Id,
            DailyLimit = 5,
            UnitCost = 10.00m,
            IsActive = true,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        });
        _context.SubscriptionPlanPrivileges.Add(new SubscriptionPlanPrivilege
        {
            Id = Guid.NewGuid(),
            SubscriptionPlanId = basicPlan.Id,
            PrivilegeId = medicationPrivilege.Id,
            DailyLimit = 2,
            UnitCost = 20.00m,
            IsActive = true,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

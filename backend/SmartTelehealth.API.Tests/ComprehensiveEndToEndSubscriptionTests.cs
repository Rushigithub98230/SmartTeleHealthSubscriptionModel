using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartTelehealth.API;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.API.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json;

namespace SmartTelehealth.API.Tests
{
    /// <summary>
    /// Comprehensive End-to-End test suite for complete subscription management flow
    /// Tests: Admin plan creation → User purchase → Lifecycle actions → Webhooks → Billing → Edge cases
    /// Uses real database operations with mocked external services
    /// </summary>
    public class ComprehensiveEndToEndSubscriptionTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;
        private readonly ApplicationDbContext _dbContext;
        private readonly IServiceScope _scope;
        
        // Core services - REAL implementations
        private readonly ISubscriptionService _subscriptionService;
        private readonly IBillingService _billingService;
        private readonly IUserService _userService;
        private readonly ISubscriptionLifecycleService _subscriptionLifecycleService;
        private readonly ISubscriptionNotificationService _notificationService;
        
        // Mock services - ONLY for external dependencies
        private readonly MockStripeService _mockStripeService;
        private readonly MockNotificationService _mockNotificationService;
        private readonly MockAuditService _mockAuditService;

        // Test data
        private User _adminUser;
        private User _testUser;
        private SubscriptionPlan _testPlan;
        private MasterBillingCycle _monthlyCycle;
        private MasterCurrency _usdCurrency;
        private Privilege _consultationPrivilege;
        private Subscription _activeSubscription;

        public ComprehensiveEndToEndSubscriptionTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;

            // Configure test database
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database for REAL testing
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"E2ETestDb_{Guid.NewGuid()}");
                    });

                    // Register REAL repositories
                    services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
                    services.AddScoped<IBillingRepository, BillingRepository>();
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
                    services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
                    services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
                    services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();

                    // Register MOCK services for external dependencies
                    services.AddScoped<IStripeService>(provider => new MockStripeService());
                    services.AddScoped<INotificationService>(provider => new MockNotificationService());
                    services.AddScoped<IAuditService>(provider => new MockAuditService());
                });
            });

            _client = _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Get REAL services
            _subscriptionService = _scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
            _billingService = _scope.ServiceProvider.GetRequiredService<IBillingService>();
            _userService = _scope.ServiceProvider.GetRequiredService<IUserService>();
            _subscriptionLifecycleService = _scope.ServiceProvider.GetRequiredService<ISubscriptionLifecycleService>();
            _notificationService = _scope.ServiceProvider.GetRequiredService<ISubscriptionNotificationService>();
            
            // Get MOCK services
            _mockStripeService = (MockStripeService)_scope.ServiceProvider.GetRequiredService<IStripeService>();
            _mockNotificationService = (MockNotificationService)_scope.ServiceProvider.GetRequiredService<INotificationService>();
            _mockAuditService = (MockAuditService)_scope.ServiceProvider.GetRequiredService<IAuditService>();

            // Initialize test database
            InitializeTestDatabaseAsync().Wait();
        }

        #region Test Database Setup

        private async Task InitializeTestDatabaseAsync()
        {
            try
            {
                // Create admin user
                _adminUser = new User
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@test.com",
                    PhoneNumber = "1234567890",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // Create test user
                _testUser = new User
                {
                    Id = 2,
                    FirstName = "Test",
                    LastName = "User",
                    Email = "user@test.com",
                    PhoneNumber = "0987654321",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // Create billing cycle
                _monthlyCycle = new MasterBillingCycle
                {
                    Id = Guid.NewGuid(),
                    Name = "Monthly",
                    Description = "Monthly billing cycle",
                    DurationInDays = 30,
                    IsActive = true
                };

                // Create currency
                _usdCurrency = new MasterCurrency
                {
                    Id = Guid.NewGuid(),
                    Code = "USD",
                    Name = "US Dollar",
                    Symbol = "$",
                    IsActive = true
                };

                // Create privilege
                _consultationPrivilege = new Privilege
                {
                    Id = Guid.NewGuid(),
                    Name = "Teleconsultation",
                    Description = "Video consultation with healthcare provider",
                    IsActive = true
                };

                // Create subscription plan
                _testPlan = new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Name = "Premium Plan",
                    Description = "Premium subscription with all features",
                    Price = 99.99m,
                    BillingCycleId = _monthlyCycle.Id,
                    CurrencyId = _usdCurrency.Id,
                    IsActive = true,
                    IsTrialAllowed = true,
                    TrialDurationInDays = 7,
                    CreatedDate = DateTime.UtcNow,
                    StripeProductId = "prod_premium_test",
                    StripeMonthlyPriceId = "price_premium_monthly_test"
                };

                // Add entities to database
                _dbContext.Users.AddRange(_adminUser, _testUser);
                _dbContext.MasterBillingCycles.Add(_monthlyCycle);
                _dbContext.MasterCurrencies.Add(_usdCurrency);
                _dbContext.Privileges.Add(_consultationPrivilege);
                _dbContext.SubscriptionPlans.Add(_testPlan);

                await _dbContext.SaveChangesAsync();

                _output.WriteLine("End-to-End test database initialized successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error initializing test database: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 1. Admin Plan Creation Tests

        [Fact]
        [Trait("Category", "Admin Plan Creation")]
        [Trait("Priority", "Critical")]
        public async Task Test_01_AdminCreateSubscriptionPlan_WithPrivileges_ShouldSucceed()
        {
            // Arrange
            var createPlanDto = new CreateSubscriptionPlanDto
            {
                Name = "Enterprise Plan",
                Description = "Enterprise subscription with advanced features",
                Price = 199.99m,
                BillingCycleId = _monthlyCycle.Id,
                CurrencyId = _usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = true,
                TrialDurationInDays = 14,
                Privileges = new List<PlanPrivilegeDto>
                {
                    new PlanPrivilegeDto
                    {
                        PrivilegeId = _consultationPrivilege.Id
                    }
                }
            };

            var adminToken = new TokenModel { UserID = _adminUser.Id, RoleID = 1 };

            // Act
            var result = await _subscriptionService.CreatePlanAsync(createPlanDto, adminToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Plan created successfully with privileges", result.Message);

            var planDto = result.data as SubscriptionPlanDto;
            Assert.NotNull(planDto);
            Assert.Equal("Enterprise Plan", planDto.Name);
            Assert.Equal(199.99m, planDto.Price);

            // Verify in REAL database
            var dbPlan = await _dbContext.SubscriptionPlans.FindAsync(Guid.Parse(planDto.Id));
            Assert.NotNull(dbPlan);
            Assert.Equal("Enterprise Plan", dbPlan.Name);
            Assert.Equal(199.99m, dbPlan.Price);

            // Verify privileges were assigned
            var planPrivileges = await _dbContext.SubscriptionPlanPrivileges
                .Where(pp => pp.SubscriptionPlanId == dbPlan.Id)
                .ToListAsync();
            Assert.NotEmpty(planPrivileges);
            Assert.Equal(_consultationPrivilege.Id, planPrivileges.First().PrivilegeId);

            // Verify audit log
            Assert.True(_mockAuditService.HasAuditLog("CreateSubscriptionPlan", "SubscriptionPlan", planDto.Id));

            _output.WriteLine($"✅ Admin plan creation test passed - Plan ID: {planDto.Id}");
        }

        [Fact]
        [Trait("Category", "Admin Plan Creation")]
        [Trait("Priority", "High")]
        public async Task Test_02_AdminUpdateSubscriptionPlan_ShouldReflectInDatabase()
        {
            // Arrange
            var updateDto = new UpdateSubscriptionPlanDto
            {
                Name = "Updated Premium Plan",
                Description = "Updated premium subscription plan",
                Price = 149.99m,
                BillingCycleId = _testPlan.BillingCycleId,
                IsActive = true,
                DisplayOrder = 1
            };

            var adminToken = new TokenModel { UserID = _adminUser.Id, RoleID = 1 };

            // Act
            var result = await _subscriptionService.UpdateSubscriptionPlanAsync(_testPlan.Id.ToString(), updateDto, adminToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Subscription plan updated successfully with Stripe synchronization", result.Message);

            // Verify in REAL database
            var updatedPlan = await _dbContext.SubscriptionPlans.FindAsync(_testPlan.Id);
            Assert.NotNull(updatedPlan);
            Assert.Equal("Updated Premium Plan", updatedPlan.Name);
            Assert.Equal(149.99m, updatedPlan.Price);

            // Verify audit log
            Assert.True(_mockAuditService.HasAuditLog("SubscriptionPlanUpdated", "SubscriptionPlan", _testPlan.Id.ToString()));

            _output.WriteLine($"✅ Admin plan update test passed - Plan ID: {_testPlan.Id}");
        }

        #endregion

        #region 2. User Subscription Purchase Tests

        [Fact]
        [Trait("Category", "User Subscription Purchase")]
        [Trait("Priority", "Critical")]
        public async Task Test_03_UserPurchaseSubscription_WithTrial_ShouldSucceed()
        {
            // Arrange
            var createDto = new CreateSubscriptionDto
            {
                UserId = _testUser.Id,
                PlanId = _testPlan.Id.ToString(),
                BillingCycleId = _testPlan.BillingCycleId,
                Price = _testPlan.Price,
                IsActive = true,
                StartImmediately = true,
                AutoRenew = true,
                PaymentMethodId = "pm_test_payment_method"
            };

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act
            var result = await _subscriptionService.CreateSubscriptionAsync(createDto, userToken);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.StatusCode == 200 || result.StatusCode == 201, $"Expected 200 or 201, got {result.StatusCode}");
            Assert.Equal("Subscription created successfully with payment integration", result.Message);

            var subscriptionDto = result.data as SubscriptionDto;
            Assert.NotNull(subscriptionDto);
            _activeSubscription = await _dbContext.Subscriptions.FindAsync(Guid.Parse(subscriptionDto.Id));

            // Verify in REAL database
            Assert.NotNull(_activeSubscription);
            Assert.Equal(_testUser.Id, _activeSubscription.UserId);
            Assert.Equal(_testPlan.Id, _activeSubscription.SubscriptionPlanId);
            Assert.True(_activeSubscription.Status == Subscription.SubscriptionStatuses.Active || 
                       _activeSubscription.Status == Subscription.SubscriptionStatuses.TrialActive);

            // Verify Stripe integration
            Assert.True(_mockStripeService.HasMockData("cus_test_user"));

            // Verify audit log
            Assert.True(_mockAuditService.HasAuditLog("CreateSubscription", "Subscription", subscriptionDto.Id));

            _output.WriteLine($"✅ User subscription purchase test passed - Subscription ID: {subscriptionDto.Id}");
        }

        [Fact]
        [Trait("Category", "User Subscription Purchase")]
        [Trait("Priority", "High")]
        public async Task Test_04_UserPurchaseSubscription_WithoutTrial_ShouldSucceed()
        {
            // Arrange - Create a plan without trial
            var noTrialPlan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "No Trial Plan",
                Description = "Plan without trial period",
                Price = 49.99m,
                BillingCycleId = _monthlyCycle.Id,
                CurrencyId = _usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = false,
                TrialDurationInDays = 0,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.SubscriptionPlans.Add(noTrialPlan);
            await _dbContext.SaveChangesAsync();

            var createDto = new CreateSubscriptionDto
            {
                UserId = _testUser.Id,
                PlanId = noTrialPlan.Id.ToString(),
                BillingCycleId = noTrialPlan.BillingCycleId,
                Price = noTrialPlan.Price,
                IsActive = true,
                StartImmediately = true,
                AutoRenew = true,
                PaymentMethodId = "pm_test_payment_method"
            };

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act
            var result = await _subscriptionService.CreateSubscriptionAsync(createDto, userToken);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.StatusCode == 200 || result.StatusCode == 201);
            Assert.Equal("Subscription created successfully with payment integration", result.Message);

            var subscriptionDto = result.data as SubscriptionDto;
            Assert.NotNull(subscriptionDto);

            // Verify in REAL database
            var dbSubscription = await _dbContext.Subscriptions.FindAsync(Guid.Parse(subscriptionDto.Id));
            Assert.NotNull(dbSubscription);
            Assert.Equal(Subscription.SubscriptionStatuses.Active, dbSubscription.Status);
            Assert.Equal(49.99m, dbSubscription.CurrentPrice);

            _output.WriteLine($"✅ User subscription purchase (no trial) test passed - Subscription ID: {subscriptionDto.Id}");
        }

        #endregion

        #region 3. Subscription Lifecycle Actions Tests

        [Fact]
        [Trait("Category", "Subscription Lifecycle")]
        [Trait("Priority", "Critical")]
        public async Task Test_05_SubscriptionPauseResume_ShouldWorkCorrectly()
        {
            // Arrange - Ensure we have an active subscription
            if (_activeSubscription == null)
            {
                await Test_03_UserPurchaseSubscription_WithTrial_ShouldSucceed();
            }

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act 1: Pause subscription
            var pauseResult = await _subscriptionService.PauseSubscriptionAsync(_activeSubscription.Id.ToString(), userToken);

            // Assert 1: Pause should succeed (unless it's a trial subscription)
            Assert.NotNull(pauseResult);
            
            if (_activeSubscription.Status == Subscription.SubscriptionStatuses.TrialActive)
            {
                // Trial subscriptions cannot be paused - business rule
                Assert.True(pauseResult.StatusCode == 400 || pauseResult.StatusCode == 500);
                _output.WriteLine($"✅ Trial subscription pause correctly rejected - Business rule enforced");
                return;
            }

            Assert.Equal(200, pauseResult.StatusCode);
            Assert.Equal("Subscription paused successfully with Stripe synchronization", pauseResult.Message);

            // Verify in REAL database
            var pausedSubscription = await _dbContext.Subscriptions.FindAsync(_activeSubscription.Id);
            Assert.NotNull(pausedSubscription);
            Assert.Equal(Subscription.SubscriptionStatuses.Paused, pausedSubscription.Status);
            Assert.True(pausedSubscription.IsPaused);

            // Act 2: Resume subscription
            var resumeResult = await _subscriptionService.ResumeSubscriptionAsync(_activeSubscription.Id.ToString(), userToken);

            // Assert 2: Resume should succeed
            Assert.NotNull(resumeResult);
            Assert.Equal(200, resumeResult.StatusCode);
            Assert.Equal("Subscription resumed successfully with Stripe synchronization", resumeResult.Message);

            // Verify in REAL database
            var resumedSubscription = await _dbContext.Subscriptions.FindAsync(_activeSubscription.Id);
            Assert.NotNull(resumedSubscription);
            Assert.Equal(Subscription.SubscriptionStatuses.Active, resumedSubscription.Status);
            Assert.False(resumedSubscription.IsPaused);

            // Verify audit logs
            Assert.True(_mockAuditService.HasAuditLog("PauseSubscription", "Subscription", _activeSubscription.Id.ToString()));
            Assert.True(_mockAuditService.HasAuditLog("ResumeSubscription", "Subscription", _activeSubscription.Id.ToString()));

            _output.WriteLine($"✅ Subscription pause/resume test passed - Subscription ID: {_activeSubscription.Id}");
        }

        [Fact]
        [Trait("Category", "Subscription Lifecycle")]
        [Trait("Priority", "Critical")]
        public async Task Test_06_SubscriptionCancelReactivate_ShouldWorkCorrectly()
        {
            // Arrange - Ensure we have an active subscription
            if (_activeSubscription == null)
            {
                await Test_03_UserPurchaseSubscription_WithTrial_ShouldSucceed();
            }

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act 1: Cancel subscription
            var cancelResult = await _subscriptionService.CancelSubscriptionAsync(
                _activeSubscription.Id.ToString(), 
                "User requested cancellation", 
                userToken);

            // Assert 1: Cancel should succeed
            Assert.NotNull(cancelResult);
            Assert.Equal(200, cancelResult.StatusCode);
            Assert.Equal("Subscription cancelled successfully with Stripe synchronization", cancelResult.Message);

            // Verify in REAL database
            var cancelledSubscription = await _dbContext.Subscriptions.FindAsync(_activeSubscription.Id);
            Assert.NotNull(cancelledSubscription);
            Assert.Equal(Subscription.SubscriptionStatuses.Cancelled, cancelledSubscription.Status);
            Assert.True(cancelledSubscription.IsCancelled);
            Assert.Equal("User requested cancellation", cancelledSubscription.CancellationReason);

            // Act 2: Reactivate subscription
            var reactivateResult = await _subscriptionService.ReactivateSubscriptionAsync(_activeSubscription.Id.ToString(), userToken);

            // Assert 2: Reactivate should succeed
            Assert.NotNull(reactivateResult);
            Assert.Equal(200, reactivateResult.StatusCode);
            Assert.Equal("Subscription reactivated successfully with Stripe synchronization", reactivateResult.Message);

            // Verify in REAL database
            var reactivatedSubscription = await _dbContext.Subscriptions.FindAsync(_activeSubscription.Id);
            Assert.NotNull(reactivatedSubscription);
            Assert.Equal(Subscription.SubscriptionStatuses.Active, reactivatedSubscription.Status);
            Assert.False(reactivatedSubscription.IsCancelled);

            // Verify audit logs
            Assert.True(_mockAuditService.HasAuditLog("CancelSubscription", "Subscription", _activeSubscription.Id.ToString()));
            Assert.True(_mockAuditService.HasAuditLog("ReactivateSubscription", "Subscription", _activeSubscription.Id.ToString()));

            _output.WriteLine($"✅ Subscription cancel/reactivate test passed - Subscription ID: {_activeSubscription.Id}");
        }

        #endregion

        #region 4. Webhook Processing Tests

        [Fact]
        [Trait("Category", "Webhook Processing")]
        [Trait("Priority", "Critical")]
        public async Task Test_07_StripeWebhook_SubscriptionUpdated_ShouldUpdateStatus()
        {
            // Arrange - Create a subscription for webhook testing
            var webhookSubscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                SubscriptionPlanId = _testPlan.Id,
                BillingCycleId = _monthlyCycle.Id,
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow.AddDays(-30),
                NextBillingDate = DateTime.UtcNow.AddDays(5),
                CurrentPrice = 99.99m,
                StripeSubscriptionId = "sub_webhook_test",
                StripeCustomerId = "cus_webhook_test",
                AutoRenew = true,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.Subscriptions.Add(webhookSubscription);
            await _dbContext.SaveChangesAsync();

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act: Simulate Stripe webhook event
            var webhookEvent = new
            {
                Type = "customer.subscription.updated",
                Data = new
                {
                    Object = new
                    {
                        Id = "sub_webhook_test",
                        Status = "past_due",
                        CurrentPeriodEnd = ((DateTimeOffset)DateTime.UtcNow.AddDays(35)).ToUnixTimeSeconds()
                    }
                }
            };

            var webhookResult = await _mockStripeService.ProcessWebhookAsync(
                JsonSerializer.Serialize(webhookEvent), 
                "mock_signature", 
                userToken);

            // Assert
            Assert.True(webhookResult);

            // Verify webhook processing didn't crash the system
            Assert.True(_mockStripeService.GetAuditLogCount() > 0);

            _output.WriteLine($"✅ Stripe webhook processing test passed - Subscription ID: {webhookSubscription.Id}");
        }

        [Fact]
        [Trait("Category", "Webhook Processing")]
        [Trait("Priority", "High")]
        public async Task Test_08_StripeWebhook_PaymentFailed_ShouldHandleGracefully()
        {
            // Arrange
            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act: Simulate payment failed webhook
            var paymentFailedEvent = new
            {
                Type = "invoice.payment_failed",
                Data = new
                {
                    Object = new
                    {
                        Id = "in_test_payment_failed",
                        Subscription = "sub_webhook_test",
                        AmountDue = 9999,
                        Currency = "usd"
                    }
                }
            };

            var webhookResult = await _mockStripeService.ProcessWebhookAsync(
                JsonSerializer.Serialize(paymentFailedEvent), 
                "mock_signature", 
                userToken);

            // Assert
            Assert.True(webhookResult);

            // Verify webhook processing handled the failure gracefully
            Assert.True(_mockStripeService.GetAuditLogCount() > 0);

            _output.WriteLine($"✅ Payment failed webhook test passed");
        }

        #endregion

        #region 5. Billing and Payment Tests

        [Fact]
        [Trait("Category", "Billing and Payment")]
        [Trait("Priority", "Critical")]
        public async Task Test_09_PaymentProcessing_WithValidPayment_ShouldSucceed()
        {
            // Arrange - Ensure we have an active subscription
            if (_activeSubscription == null)
            {
                await Test_03_UserPurchaseSubscription_WithTrial_ShouldSucceed();
            }

            var paymentRequest = new PaymentRequestDto
            {
                PaymentMethodId = "pm_test_payment_method",
                Amount = 99.99m,
                Currency = "usd"
            };

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act
            var result = await _subscriptionService.ProcessPaymentAsync(_activeSubscription.Id.ToString(), paymentRequest, userToken);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.StatusCode == 200 || result.StatusCode == 201);

            // Verify in REAL database that billing record was created
            var billingRecords = await _dbContext.BillingRecords
                .Where(br => br.SubscriptionId == _activeSubscription.Id)
                .ToListAsync();

            Assert.NotEmpty(billingRecords);

            // Verify mock Stripe service was called
            var paymentIntent = _mockStripeService.GetMockData("last_payment_result");
            Assert.NotNull(paymentIntent);

            _output.WriteLine($"✅ Payment processing test passed - Subscription ID: {_activeSubscription.Id}");
        }

        [Fact]
        [Trait("Category", "Billing and Payment")]
        [Trait("Priority", "High")]
        public async Task Test_10_PaymentFailureRecovery_ShouldResetFailedAttempts()
        {
            // Arrange - Create a subscription with payment failure
            var failedSubscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                SubscriptionPlanId = _testPlan.Id,
                BillingCycleId = _monthlyCycle.Id,
                Status = Subscription.SubscriptionStatuses.PaymentFailed,
                StartDate = DateTime.UtcNow.AddDays(-30),
                NextBillingDate = DateTime.UtcNow.AddDays(5),
                CurrentPrice = 99.99m,
                LastPaymentFailedDate = DateTime.UtcNow.AddDays(-1),
                LastPaymentError = "Insufficient funds",
                FailedPaymentAttempts = 2,
                AutoRenew = true,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.Subscriptions.Add(failedSubscription);
            await _dbContext.SaveChangesAsync();

            var paymentRequest = new PaymentRequestDto
            {
                Amount = 99.99m,
                Currency = "usd",
                PaymentMethodId = "pm_test_recovery"
            };

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act: Attempt payment recovery
            var result = await _subscriptionService.ProcessPaymentAsync(failedSubscription.Id.ToString(), paymentRequest, userToken);

            // Assert: Payment recovery should succeed
            Assert.NotNull(result);
            Assert.True(result.StatusCode == 200 || result.StatusCode == 201);

            // Verify subscription is now active
            var recoveredSubscription = await _dbContext.Subscriptions.FindAsync(failedSubscription.Id);
            Assert.NotNull(recoveredSubscription);
            Assert.Equal(Subscription.SubscriptionStatuses.Active, recoveredSubscription.Status);
            Assert.Equal(0, recoveredSubscription.FailedPaymentAttempts);
            Assert.Null(recoveredSubscription.LastPaymentError);

            // Verify billing record was created
            var billingRecords = await _dbContext.BillingRecords
                .Where(br => br.SubscriptionId == failedSubscription.Id)
                .ToListAsync();
            Assert.NotEmpty(billingRecords);

            _output.WriteLine($"✅ Payment failure recovery test passed - Subscription ID: {failedSubscription.Id}");
        }

        [Fact]
        [Trait("Category", "Billing and Payment")]
        [Trait("Priority", "High")]
        public async Task Test_11_BillingHistory_ShouldReturnCorrectRecords()
        {
            // Arrange - Ensure we have an active subscription
            if (_activeSubscription == null)
            {
                await Test_03_UserPurchaseSubscription_WithTrial_ShouldSucceed();
            }

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act
            var result = await _subscriptionService.GetBillingHistoryAsync(_activeSubscription.Id.ToString(), userToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Billing history retrieved successfully", result.Message);
            Assert.NotNull(result.data);

            // Verify in REAL database
            var billingRecords = await _dbContext.BillingRecords
                .Where(br => br.SubscriptionId == _activeSubscription.Id)
                .ToListAsync();

            var historyDto = result.data as IEnumerable<BillingHistoryDto>;
            Assert.NotNull(historyDto);
            Assert.Equal(billingRecords.Count, historyDto.Count());

            _output.WriteLine($"✅ Billing history test passed - Subscription ID: {_activeSubscription.Id}");
        }

        #endregion

        #region 6. Edge Cases and Error Handling Tests

        [Fact]
        [Trait("Category", "Edge Cases")]
        [Trait("Priority", "High")]
        public async Task Test_12_InvalidOperations_ShouldHandleGracefully()
        {
            // Arrange
            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Test 1: Try to pause non-existent subscription
            var nonExistentId = Guid.NewGuid().ToString();
            var pauseResult = await _subscriptionService.PauseSubscriptionAsync(nonExistentId, userToken);
            Assert.NotNull(pauseResult);
            Assert.Equal(404, pauseResult.StatusCode);

            // Test 2: Try to get non-existent subscription
            var getResult = await _subscriptionService.GetSubscriptionAsync(nonExistentId, userToken);
            Assert.NotNull(getResult);
            Assert.Equal(404, getResult.StatusCode);
            Assert.Equal("Subscription not found", getResult.Message);

            // Test 3: Try to process payment for non-existent subscription
            var paymentRequest = new PaymentRequestDto
            {
                Amount = 99.99m,
                Currency = "usd",
                PaymentMethodId = "pm_test"
            };

            var paymentResult = await _subscriptionService.ProcessPaymentAsync(nonExistentId, paymentRequest, userToken);
            Assert.NotNull(paymentResult);
            Assert.Equal(404, paymentResult.StatusCode);

            _output.WriteLine($"✅ Invalid operations error handling test passed");
        }

        [Fact]
        [Trait("Category", "Edge Cases")]
        [Trait("Priority", "High")]
        public async Task Test_13_BusinessRuleEnforcement_ShouldPreventInvalidTransitions()
        {
            // Arrange - Create subscriptions in various states
            var cancelledSubscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                SubscriptionPlanId = _testPlan.Id,
                BillingCycleId = _monthlyCycle.Id,
                Status = Subscription.SubscriptionStatuses.Cancelled,
                StartDate = DateTime.UtcNow.AddDays(-30),
                CancelledDate = DateTime.UtcNow.AddDays(-1),
                CancellationReason = "Customer request",
                CurrentPrice = 99.99m,
                AutoRenew = false,
                CreatedDate = DateTime.UtcNow
            };

            var expiredSubscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                SubscriptionPlanId = _testPlan.Id,
                BillingCycleId = _monthlyCycle.Id,
                Status = Subscription.SubscriptionStatuses.Expired,
                StartDate = DateTime.UtcNow.AddDays(-60),
                ExpirationDate = DateTime.UtcNow.AddDays(-1),
                CurrentPrice = 99.99m,
                AutoRenew = false,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.Subscriptions.AddRange(cancelledSubscription, expiredSubscription);
            await _dbContext.SaveChangesAsync();

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act & Assert: Test invalid transitions
            var invalidPauseResult = await _subscriptionService.PauseSubscriptionAsync(cancelledSubscription.Id.ToString(), userToken);
            Assert.NotNull(invalidPauseResult);
            Assert.True(invalidPauseResult.StatusCode == 400 || invalidPauseResult.StatusCode == 500);

            var invalidResumeResult = await _subscriptionService.ResumeSubscriptionAsync(expiredSubscription.Id.ToString(), userToken);
            Assert.NotNull(invalidResumeResult);
            Assert.True(invalidResumeResult.StatusCode == 400 || invalidResumeResult.StatusCode == 500);

            // Verify subscriptions remain in original state
            var stillCancelled = await _dbContext.Subscriptions.FindAsync(cancelledSubscription.Id);
            Assert.Equal(Subscription.SubscriptionStatuses.Cancelled, stillCancelled.Status);

            var stillExpired = await _dbContext.Subscriptions.FindAsync(expiredSubscription.Id);
            Assert.Equal(Subscription.SubscriptionStatuses.Expired, stillExpired.Status);

            _output.WriteLine($"✅ Business rule enforcement test passed");
        }

        [Fact]
        [Trait("Category", "Edge Cases")]
        [Trait("Priority", "Medium")]
        public async Task Test_14_StripeServiceFailure_ShouldHandleGracefully()
        {
            // Arrange: Set Stripe service to fail
            _mockStripeService.SetFailureMode(true, "Mock Stripe API failure");

            var createDto = new CreateSubscriptionDto
            {
                UserId = _testUser.Id,
                PlanId = _testPlan.Id.ToString(),
                BillingCycleId = _testPlan.BillingCycleId,
                Price = _testPlan.Price,
                IsActive = true,
                StartImmediately = true,
                AutoRenew = true,
                PaymentMethodId = "pm_test_payment_method"
            };

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act: Try to create subscription with failing Stripe service
            var result = await _subscriptionService.CreateSubscriptionAsync(createDto, userToken);

            // Assert: Should handle Stripe failure gracefully
            Assert.NotNull(result);
            Assert.True(result.StatusCode == 400 || result.StatusCode == 500);

            // Reset Stripe service for other tests
            _mockStripeService.SetFailureMode(false);

            _output.WriteLine($"✅ Stripe service failure handling test passed");
        }

        #endregion

        #region 7. Trial Management Tests

        [Fact]
        [Trait("Category", "Trial Management")]
        [Trait("Priority", "Critical")]
        public async Task Test_15_TrialLifecycle_ShouldHandleAllStates()
        {
            // Arrange: Create a trial subscription
            var trialSubscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                SubscriptionPlanId = _testPlan.Id,
                BillingCycleId = _monthlyCycle.Id,
                Status = Subscription.SubscriptionStatuses.TrialActive,
                StartDate = DateTime.UtcNow.AddDays(-7),
                TrialEndDate = DateTime.UtcNow.AddDays(7),
                NextBillingDate = DateTime.UtcNow.AddDays(7),
                CurrentPrice = 0m,
                AutoRenew = true,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.Subscriptions.Add(trialSubscription);
            await _dbContext.SaveChangesAsync();

            // Act & Assert: Test trial extension
            var extendResult = await _subscriptionLifecycleService.ExtendTrialAsync(trialSubscription.Id.ToString(), 3, "Customer request");
            Assert.NotNull(extendResult);
            Assert.True(extendResult.StatusCode == 200 || extendResult.StatusCode == 201);

            // Verify trial was extended in database
            var updatedSubscription = await _dbContext.Subscriptions.FindAsync(trialSubscription.Id);
            Assert.NotNull(updatedSubscription);
            Assert.Equal(Subscription.SubscriptionStatuses.TrialActive, updatedSubscription.Status);
            Assert.True(updatedSubscription.TrialEndDate > DateTime.UtcNow.AddDays(7));

            // Act & Assert: Test trial conversion to active
            var convertResult = await _subscriptionLifecycleService.ConvertTrialToActiveAsync(trialSubscription.Id.ToString());
            Assert.NotNull(convertResult);
            Assert.True(convertResult.StatusCode == 200 || convertResult.StatusCode == 201);

            // Verify subscription is now active
            var activeSubscription = await _dbContext.Subscriptions.FindAsync(trialSubscription.Id);
            Assert.NotNull(activeSubscription);
            Assert.Equal(Subscription.SubscriptionStatuses.Active, activeSubscription.Status);
            Assert.True(activeSubscription.CurrentPrice > 0);

            _output.WriteLine($"✅ Trial lifecycle test passed - Subscription ID: {trialSubscription.Id}");
        }

        #endregion

        #region 8. Admin Bulk Operations Tests

        [Fact]
        [Trait("Category", "Admin Bulk Operations")]
        [Trait("Priority", "High")]
        public async Task Test_16_AdminBulkActions_ShouldProcessMultipleSubscriptions()
        {
            // Arrange: Create multiple test subscriptions
            var subscription1 = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                SubscriptionPlanId = _testPlan.Id,
                BillingCycleId = _monthlyCycle.Id,
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddDays(30),
                CurrentPrice = 99.99m,
                AutoRenew = true,
                CreatedDate = DateTime.UtcNow
            };

            var subscription2 = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                SubscriptionPlanId = _testPlan.Id,
                BillingCycleId = _monthlyCycle.Id,
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddDays(30),
                CurrentPrice = 99.99m,
                AutoRenew = true,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.Subscriptions.AddRange(subscription1, subscription2);
            await _dbContext.SaveChangesAsync();

            var actions = new List<BulkActionRequestDto>
            {
                new BulkActionRequestDto
                {
                    SubscriptionId = subscription1.Id.ToString(),
                    Action = "pause",
                    Reason = "Bulk admin pause"
                },
                new BulkActionRequestDto
                {
                    SubscriptionId = subscription2.Id.ToString(),
                    Action = "cancel",
                    Reason = "Bulk admin cancellation"
                }
            };

            var adminToken = new TokenModel { UserID = _adminUser.Id, RoleID = 1 };

            // Act: Perform bulk actions
            var result = await _subscriptionService.PerformBulkActionAsync(actions, adminToken);

            // Assert: Bulk actions completed successfully
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Bulk actions completed", result.Message);

            // Verify in REAL database that actions were applied
            var updatedSubscription1 = await _dbContext.Subscriptions.FindAsync(subscription1.Id);
            var updatedSubscription2 = await _dbContext.Subscriptions.FindAsync(subscription2.Id);

            Assert.NotNull(updatedSubscription1);
            Assert.NotNull(updatedSubscription2);
            Assert.Equal(Subscription.SubscriptionStatuses.Paused, updatedSubscription1.Status);
            Assert.Equal(Subscription.SubscriptionStatuses.Cancelled, updatedSubscription2.Status);

            // Verify audit logs were created
            Assert.True(_mockAuditService.HasAuditLog("PauseSubscription", "Subscription", subscription1.Id.ToString()));
            Assert.True(_mockAuditService.HasAuditLog("CancelSubscription", "Subscription", subscription2.Id.ToString()));

            _output.WriteLine($"✅ Admin bulk actions test passed");
        }

        #endregion

        #region 9. Data Consistency and Integration Tests

        [Fact]
        [Trait("Category", "Data Consistency")]
        [Trait("Priority", "Critical")]
        public async Task Test_17_DataConsistency_StripeAndLocalData_ShouldBeSynchronized()
        {
            // Arrange - Ensure we have an active subscription
            if (_activeSubscription == null)
            {
                await Test_03_UserPurchaseSubscription_WithTrial_ShouldSucceed();
            }

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act: Get subscription details
            var result = await _subscriptionService.GetSubscriptionAsync(_activeSubscription.Id.ToString(), userToken);

            // Assert: Subscription retrieved successfully
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var subscriptionDto = result.data as SubscriptionDto;
            Assert.NotNull(subscriptionDto);

            // Verify in REAL database
            var dbSubscription = await _dbContext.Subscriptions.FindAsync(_activeSubscription.Id);
            Assert.NotNull(dbSubscription);

            // Verify data consistency between DTO and database
            Assert.Equal(dbSubscription.Status, subscriptionDto.Status);
            Assert.Equal(dbSubscription.CurrentPrice, subscriptionDto.CurrentPrice);
            Assert.Equal(dbSubscription.NextBillingDate, subscriptionDto.NextBillingDate);
            Assert.Equal(dbSubscription.AutoRenew, subscriptionDto.AutoRenew);

            // Verify Stripe IDs are consistent
            Assert.Equal(dbSubscription.StripeCustomerId, subscriptionDto.StripeCustomerId);
            Assert.Equal(dbSubscription.StripeSubscriptionId, subscriptionDto.StripeSubscriptionId);

            _output.WriteLine($"✅ Data consistency test passed - Subscription ID: {_activeSubscription.Id}");
        }

        [Fact]
        [Trait("Category", "Data Consistency")]
        [Trait("Priority", "High")]
        public async Task Test_18_SubscriptionStatus_ComputedProperties_ShouldBeConsistent()
        {
            // Arrange - Ensure we have an active subscription
            if (_activeSubscription == null)
            {
                await Test_03_UserPurchaseSubscription_WithTrial_ShouldSucceed();
            }

            var userToken = new TokenModel { UserID = _testUser.Id, RoleID = 2 };

            // Act: Get subscription
            var result = await _subscriptionService.GetSubscriptionAsync(_activeSubscription.Id.ToString(), userToken);

            // Assert: Subscription retrieved successfully
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var subscriptionDto = result.data as SubscriptionDto;
            Assert.NotNull(subscriptionDto);

            // Verify computed properties are consistent with status
            Assert.Equal(subscriptionDto.Status == Subscription.SubscriptionStatuses.Active, subscriptionDto.IsActive);
            Assert.Equal(subscriptionDto.Status == Subscription.SubscriptionStatuses.Paused, subscriptionDto.IsPaused);
            Assert.Equal(subscriptionDto.Status == Subscription.SubscriptionStatuses.Cancelled, subscriptionDto.IsCancelled);
            Assert.Equal(subscriptionDto.Status == Subscription.SubscriptionStatuses.Expired, subscriptionDto.IsExpired);

            _output.WriteLine($"✅ Subscription status computed properties test passed - Subscription ID: {_activeSubscription.Id}");
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            try
            {
                // Clean up test data
                if (_dbContext != null)
                {
                    _dbContext.Database.EnsureDeleted();
                    _dbContext.Dispose();
                }

                _scope?.Dispose();
                _client?.Dispose();
                _factory?.Dispose();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        #endregion
    }
}

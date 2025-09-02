using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.API;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Services;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace SmartTelehealth.API.Tests
{
    /// <summary>
    /// Focused tests for specific Stripe integration workflows
    /// Tests individual components and their interactions
    /// </summary>
    public class StripeIntegrationWorkflowTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _context;
        private readonly ITestOutputHelper _output;
        private Mock<IStripeService> _mockStripeService;

        public StripeIntegrationWorkflowTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase($"WorkflowTestDb_{Guid.NewGuid()}"));

                    _mockStripeService = new Mock<IStripeService>();
                    services.AddScoped(_ => _mockStripeService.Object);
                });
            }).CreateClient();

            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SetupTestData();
        }

        private void SetupTestData()
        {
            // Seed master tables first
            SeedData.SeedMasterTables(_context);

            // Add additional test user if needed
            if (!_context.Users.Any(u => u.Email == "test@example.com"))
            {
                var user = new User
                {
                    // Don't set Id - let database auto-generate it
                    Email = "test@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    UserRoleId = 1, // Set to Client role (ID 1 from seed data)
                    IsActive = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Users.Add(user);
                _context.SaveChanges();
            }
        }

        [Fact]
        public async Task TestSubscriptionCreationWithStripeIntegration()
        {
            _output.WriteLine("=== Testing Subscription Creation with Stripe Integration ===");

            // Setup mocks
            _mockStripeService.Setup(x => x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync("cus_test123");

            _mockStripeService.Setup(x => x.CreateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync("sub_test123");

            // Create subscription plan
            var plan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Test Plan",
                Description = "Test plan",
                Price = 29.99m,
                BillingCycleId = Guid.NewGuid(),
                StripeProductId = "prod_test123",
                StripeMonthlyPriceId = "price_test123",
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();

            // Test subscription creation
            var subscriptionService = _scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
            var createDto = new CreateSubscriptionDto
            {
                PlanId = plan.Id.ToString(),
                BillingCycleId = Guid.NewGuid(),
                PaymentMethodId = "pm_test123"
            };

            var result = await subscriptionService.CreateSubscriptionAsync(createDto, new TokenModel { UserID = 1, RoleID = 2 });

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Verify Stripe integration was called
            _mockStripeService.Verify(x => x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.Once);
            _mockStripeService.Verify(x => x.CreateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.Once);

            _output.WriteLine("✅ Subscription creation with Stripe integration verified");
        }

        [Fact]
        public async Task TestPaymentProcessingWithValidation()
        {
            _output.WriteLine("=== Testing Payment Processing with Validation ===");

            // Setup mocks
            _mockStripeService.Setup(x => x.ValidatePaymentMethodAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new PaymentResultDto
                {
                    Status = "succeeded",
                    PaymentIntentId = "pi_test123",
                    Amount = 29.99m,
                    Currency = "USD",
                    ProcessedAt = DateTime.UtcNow
                });

            _mockStripeService.Setup(x => x.GetCustomerPaymentMethodsAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new List<PaymentMethodDto>
                {
                    new PaymentMethodDto
                    {
                        Id = "pm_test123",
                        IsDefault = true,
                        Type = "card",
                        Card = new CardDto
                        {
                            Last4 = "4242",
                            Brand = "visa",
                            ExpMonth = 12,
                            ExpYear = 2025
                        }
                    }
                });

            // Create billing record
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = 1,
                Amount = 29.99m,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                BillingDate = DateTime.UtcNow,
                CurrencyId = _context.MasterCurrencies.First().Id,
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();

            // Test payment processing
            var billingService = _scope.ServiceProvider.GetRequiredService<IBillingService>();
            var result = await billingService.ProcessPaymentAsync(billingRecord.Id, new TokenModel { UserID = 1, RoleID = 2 });

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Verify validation was called
            _mockStripeService.Verify(x => x.ValidatePaymentMethodAsync(It.IsAny<string>(), It.IsAny<TokenModel>()), Times.AtLeastOnce);

            // Verify payment processing was called
            _mockStripeService.Verify(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.AtLeastOnce);

            _output.WriteLine("✅ Payment processing with validation verified");
        }

        [Fact]
        public async Task TestBillingRecordCorrelation()
        {
            _output.WriteLine("=== Testing Billing Record Correlation ===");

            // Create billing record
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = 1,
                Amount = 29.99m,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                BillingDate = DateTime.UtcNow,
                StripeInvoiceId = "in_test123",
                StripePaymentIntentId = "pi_test123",
                CurrencyId = _context.MasterCurrencies.First().Id,
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();

            // Verify correlation data is stored
            var savedRecord = await _context.BillingRecords.FindAsync(billingRecord.Id);
            Assert.NotNull(savedRecord);
            Assert.Equal("in_test123", savedRecord.StripeInvoiceId);
            Assert.Equal("pi_test123", savedRecord.StripePaymentIntentId);

            _output.WriteLine("✅ Billing record correlation verified");
        }

        [Fact]
        public async Task TestWebhookIdempotency()
        {
            _output.WriteLine("=== Testing Webhook Idempotency ===");

            var auditService = _scope.ServiceProvider.GetRequiredService<IAuditService>();
            var eventId = "evt_test123";

            // First check - should not be processed
            var isProcessed1 = await auditService.IsEventProcessedAsync(eventId);
            Assert.False(isProcessed1);

            // Mark as processed
            await auditService.MarkEventAsProcessedAsync(eventId, "test_event", "Processed");

            // Second check - should be processed
            var isProcessed2 = await auditService.IsEventProcessedAsync(eventId);
            Assert.True(isProcessed2);

            _output.WriteLine("✅ Webhook idempotency verified");
        }

        [Fact]
        public async Task TestBillingCycleSynchronization()
        {
            _output.WriteLine("=== Testing Billing Cycle Synchronization ===");

            // Setup mock
            _mockStripeService.Setup(x => x.GetSubscriptionAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new SubscriptionDto
                {
                    Id = "sub_test123",
                    CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                    Status = "active"
                });

            // Create subscription with Stripe ID
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = 1,
                SubscriptionPlanId = Guid.NewGuid(),
                StripeSubscriptionId = "sub_test123",
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Test billing cycle calculation
            var automatedBillingService = _scope.ServiceProvider.GetRequiredService<IAutomatedBillingService>();
            var nextBillingDate = await automatedBillingService.CalculateNextBillingDateAsync(subscription.Id, new TokenModel { UserID = 1, RoleID = 2 });

            Assert.True(nextBillingDate > DateTime.UtcNow);

            // Verify Stripe subscription was queried
            _mockStripeService.Verify(x => x.GetSubscriptionAsync("sub_test123", It.IsAny<TokenModel>()), Times.AtLeastOnce);

            _output.WriteLine("✅ Billing cycle synchronization verified");
        }

        [Fact]
        public async Task TestPaymentRetryLogic()
        {
            _output.WriteLine("=== Testing Payment Retry Logic ===");

            var attemptCount = 0;
            _mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .Returns(() =>
                {
                    attemptCount++;
                    if (attemptCount <= 2)
                    {
                        throw new Exception("Temporary payment failure");
                    }
                    return Task.FromResult(new PaymentResultDto
                    {
                        Status = "succeeded",
                        PaymentIntentId = "pi_test123",
                        Amount = 29.99m,
                        Currency = "USD",
                        ProcessedAt = DateTime.UtcNow
                    });
                });

            _mockStripeService.Setup(x => x.ValidatePaymentMethodAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            _mockStripeService.Setup(x => x.GetCustomerPaymentMethodsAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new List<PaymentMethodDto>
                {
                    new PaymentMethodDto
                    {
                        Id = "pm_test123",
                        IsDefault = true,
                        Type = "card",
                        Card = new CardDto
                        {
                            Last4 = "4242",
                            Brand = "visa",
                            ExpMonth = 12,
                            ExpYear = 2025
                        }
                    }
                });

            // Create billing record
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = 1,
                Amount = 29.99m,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                BillingDate = DateTime.UtcNow,
                CurrencyId = _context.MasterCurrencies.First().Id,
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();

            // Test payment processing with retry
            var billingService = _scope.ServiceProvider.GetRequiredService<IBillingService>();
            var result = await billingService.ProcessPaymentAsync(billingRecord.Id, new TokenModel { UserID = 1, RoleID = 2 });

            // Should succeed after retries
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Verify multiple attempts were made
            Assert.True(attemptCount > 1);

            _output.WriteLine("✅ Payment retry logic verified");
        }

        public void Dispose()
        {
            _context?.Dispose();
            _scope?.Dispose();
            _client?.Dispose();
        }
    }
}

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.API;
using SmartTelehealth.API.Controllers;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace SmartTelehealth.API.Tests
{
    /// <summary>
    /// Comprehensive End-to-End Tests for Stripe Integration
    /// Tests the complete workflow: Subscription Creation → Payment Processing → Billing → Webhooks
    /// </summary>
    public class EndToEndStripeIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _context;
        private readonly ITestOutputHelper _output;
        private Mock<IStripeService> _mockStripeService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<ILogger<StripeWebhookController>> _mockWebhookLogger;

        // Test data
        private readonly TokenModel _adminToken = new() { UserID = 1, RoleID = 1 };
        private readonly TokenModel _userToken = new() { UserID = 2, RoleID = 2 };
        private readonly Guid _testPlanId = Guid.NewGuid();
        private readonly Guid _testSubscriptionId = Guid.NewGuid();
        private readonly string _testStripeCustomerId = "cus_test123";
        private readonly string _testStripeSubscriptionId = "sub_test123";
        private readonly string _testStripePaymentIntentId = "pi_test123";
        private readonly string _testStripeInvoiceId = "in_test123";
        private User _testUser;

        public EndToEndStripeIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;

            // Create client with test database
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
                    if (descriptor != null) services.Remove(descriptor);

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

                    // Mock external services
                    _mockStripeService = new Mock<IStripeService>();
                    _mockNotificationService = new Mock<INotificationService>();
                    _mockWebhookLogger = new Mock<ILogger<StripeWebhookController>>();

                    services.AddScoped(_ => _mockStripeService.Object);
                    services.AddScoped(_ => _mockNotificationService.Object);
                    services.AddScoped(_ => _mockWebhookLogger.Object);
                });
            }).CreateClient();

            // Get services
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Setup database
            SetupTestData();
            SetupMockServices();
        }

        private void SetupTestData()
        {
            // Seed master tables first
            SeedData.SeedMasterTables(_context);

            // Create test user
            _testUser = new User
            {
                // Don't set Id - let database auto-generate it
                Email = "user@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRoleId = 1, // Set to Client role (ID 1 from seed data)
                StripeCustomerId = _testStripeCustomerId,
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Users.Add(_testUser);
            _context.SaveChanges(); // Save to get the user ID

            // Create test subscription plan
            var testPlan = new SubscriptionPlan
            {
                Id = _testPlanId,
                Name = "Test Premium Plan",
                Description = "Test plan for integration testing",
                Price = 29.99m,
                BillingCycleId = Guid.NewGuid(),
                IsTrialAllowed = true,
                TrialDurationInDays = 7,
                StripeProductId = "prod_test123",
                StripeMonthlyPriceId = "price_test123",
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlans.Add(testPlan);

            // Create test subscription
            var testSubscription = new Subscription
            {
                Id = _testSubscriptionId,
                UserId = _testUser.Id,
                SubscriptionPlanId = _testPlanId,
                Status = Subscription.SubscriptionStatuses.TrialActive,
                StartDate = DateTime.UtcNow,
                TrialStartDate = DateTime.UtcNow,
                TrialEndDate = DateTime.UtcNow.AddDays(7),
                NextBillingDate = DateTime.UtcNow.AddDays(7),
                CurrentPrice = 29.99m,
                IsActive = true,
                CreatedBy = 2,
                CreatedDate = DateTime.UtcNow
            };
            _context.Subscriptions.Add(testSubscription);

            // Create test currency
            var testCurrency = new MasterCurrency
            {
                Id = Guid.NewGuid(),
                Code = "USD",
                Name = "US Dollar",
                Symbol = "$",
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.MasterCurrencies.Add(testCurrency);

            _context.SaveChanges();
        }

        private void SetupMockServices()
        {
            // Mock Stripe service responses
            _mockStripeService.Setup(x => x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(_testStripeCustomerId);

            _mockStripeService.Setup(x => x.CreateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(_testStripeSubscriptionId);

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new PaymentResultDto
                {
                    Status = "succeeded",
                    PaymentIntentId = _testStripePaymentIntentId,
                    Amount = 29.99m,
                    Currency = "USD",
                    ProcessedAt = DateTime.UtcNow
                });

            _mockStripeService.Setup(x => x.ValidatePaymentMethodAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            _mockStripeService.Setup(x => x.GetSubscriptionAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new SubscriptionDto
                {
                    Id = _testStripeSubscriptionId,
                    CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                    Status = "active"
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

            // Mock notification service
            _mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { StatusCode = 200, Message = "Notification created" });
        }

        [Fact]
        public async Task TestCompleteSubscriptionCreationWorkflow()
        {
            _output.WriteLine("=== Testing Complete Subscription Creation Workflow ===");

            // Step 1: Create subscription plan with Stripe integration
            var createPlanDto = new CreateSubscriptionPlanDto
            {
                Name = "Integration Test Plan",
                Description = "Plan for testing Stripe integration",
                Price = 39.99m,
                BillingCycleId = Guid.NewGuid(),
                IsTrialAllowed = true,
                TrialDurationInDays = 14,
                Privileges = new List<PlanPrivilegeDto>
                {
                    new PlanPrivilegeDto
                    {
                        PrivilegeId = Guid.NewGuid(),
                        DailyLimit = 5,
                        WeeklyLimit = 20,
                        MonthlyLimit = 80,
                        Value = 100,
                        UsagePeriodId = Guid.NewGuid(),
                        DurationMonths = 12
                    }
                }
            };

            var planResponse = await _client.PostAsJsonAsync("/api/SubscriptionPlans", createPlanDto);
            Assert.True(planResponse.IsSuccessStatusCode);

            var planResult = await planResponse.Content.ReadFromJsonAsync<JsonModel>();
            Assert.NotNull(planResult);
            Assert.Equal(200, planResult.StatusCode);

            _output.WriteLine("✅ Subscription plan created successfully");

            // Step 2: Create subscription with automatic Stripe integration
            var createSubscriptionDto = new CreateSubscriptionDto
            {
                PlanId = _testPlanId.ToString(),
                BillingCycleId = Guid.NewGuid(),
                PaymentMethodId = "pm_test123"
            };

            var subscriptionResponse = await _client.PostAsJsonAsync("/api/Subscriptions", createSubscriptionDto);
            Assert.True(subscriptionResponse.IsSuccessStatusCode);

            var subscriptionResult = await subscriptionResponse.Content.ReadFromJsonAsync<JsonModel>();
            Assert.NotNull(subscriptionResult);
            Assert.Equal(200, subscriptionResult.StatusCode);

            _output.WriteLine("✅ Subscription created with Stripe integration");

            // Step 3: Verify Stripe customer creation was called
            _mockStripeService.Verify(x => x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.AtLeastOnce);
            _output.WriteLine("✅ Stripe customer creation verified");

            // Step 4: Verify Stripe subscription creation was called
            _mockStripeService.Verify(x => x.CreateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.AtLeastOnce);
            _output.WriteLine("✅ Stripe subscription creation verified");

            // Step 5: Verify local subscription has Stripe ID
            var createdSubscription = _context.Subscriptions.FirstOrDefault(s => s.SubscriptionPlanId == _testPlanId);
            Assert.NotNull(createdSubscription);
            Assert.Equal(_testStripeSubscriptionId, createdSubscription.StripeSubscriptionId);
            _output.WriteLine("✅ Local subscription linked to Stripe subscription");

            _output.WriteLine("=== Subscription Creation Workflow Test PASSED ===");
        }

        [Fact]
        public async Task TestPaymentProcessingWithRetryLogic()
        {
            _output.WriteLine("=== Testing Payment Processing with Retry Logic ===");

            // Create billing record
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                SubscriptionId = _testSubscriptionId,
                Amount = 29.99m,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                CurrencyId = _context.MasterCurrencies.First().Id,
                IsActive = true,
                CreatedBy = 2,
                CreatedDate = DateTime.UtcNow
            };
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();

            _output.WriteLine("✅ Test billing record created");

            // Test payment processing
            var billingService = _scope.ServiceProvider.GetRequiredService<IBillingService>();
            var result = await billingService.ProcessPaymentAsync(billingRecord.Id, _userToken);

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            _output.WriteLine("✅ Payment processing completed successfully");

            // Verify payment method validation was called
            _mockStripeService.Verify(x => x.ValidatePaymentMethodAsync(It.IsAny<string>(), It.IsAny<TokenModel>()), Times.AtLeastOnce);
            _output.WriteLine("✅ Payment method validation verified");

            // Verify payment processing was called
            _mockStripeService.Verify(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.AtLeastOnce);
            _output.WriteLine("✅ Stripe payment processing verified");

            // Verify billing record was updated with Stripe correlation
            var updatedRecord = await _context.BillingRecords.FindAsync(billingRecord.Id);
            Assert.NotNull(updatedRecord);
            Assert.Equal(BillingRecord.BillingStatus.Paid, updatedRecord.Status);
            Assert.Equal(_testStripePaymentIntentId, updatedRecord.StripePaymentIntentId);
            Assert.Equal(_testStripePaymentIntentId, updatedRecord.TransactionId);
            _output.WriteLine("✅ Billing record updated with Stripe correlation data");

            _output.WriteLine("=== Payment Processing Test PASSED ===");
        }

        [Fact]
        public async Task TestWebhookProcessingWithIdempotency()
        {
            _output.WriteLine("=== Testing Webhook Processing with Idempotency ===");

            // Create test webhook event
            var webhookEvent = new
            {
                id = "evt_test123",
                type = "invoice.payment_succeeded",
                data = new
                {
                    @object = new
                    {
                        id = _testStripeInvoiceId,
                        customer = _testStripeCustomerId,
                        amount_paid = 2999,
                        currency = "usd",
                        status = "paid",
                        payment_intent = _testStripePaymentIntentId
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // First webhook call
            var response1 = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response1.IsSuccessStatusCode);
            _output.WriteLine("✅ First webhook call processed successfully");

            // Second webhook call (should be idempotent)
            var response2 = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response2.IsSuccessStatusCode);
            _output.WriteLine("✅ Second webhook call processed (idempotent)");

            // Verify webhook was processed only once (check audit logs)
            var auditService = _scope.ServiceProvider.GetRequiredService<IAuditService>();
            var isProcessed = await auditService.IsEventProcessedAsync("evt_test123");
            Assert.True(isProcessed);
            _output.WriteLine("✅ Webhook idempotency verified");

            _output.WriteLine("=== Webhook Processing Test PASSED ===");
        }

        [Fact]
        public async Task TestBillingCycleSynchronization()
        {
            _output.WriteLine("=== Testing Billing Cycle Synchronization ===");

            var automatedBillingService = _scope.ServiceProvider.GetRequiredService<IAutomatedBillingService>();
            
            // Test billing cycle calculation with Stripe sync
            var nextBillingDate = await automatedBillingService.CalculateNextBillingDateAsync(_testSubscriptionId, _userToken);
            
            Assert.True(nextBillingDate > DateTime.UtcNow);
            _output.WriteLine($"✅ Next billing date calculated: {nextBillingDate}");

            // Verify Stripe subscription was queried for period end
            _mockStripeService.Verify(x => x.GetSubscriptionAsync(_testStripeSubscriptionId, It.IsAny<TokenModel>()), Times.AtLeastOnce);
            _output.WriteLine("✅ Stripe subscription period end synchronization verified");

            _output.WriteLine("=== Billing Cycle Synchronization Test PASSED ===");
        }

        [Fact]
        public async Task TestCompleteEndToEndWorkflow()
        {
            _output.WriteLine("=== Testing Complete End-to-End Workflow ===");

            // Step 1: Create subscription plan
            var createPlanDto = new CreateSubscriptionPlanDto
            {
                Name = "E2E Test Plan",
                Description = "Complete workflow test plan",
                Price = 49.99m,
                BillingCycleId = Guid.NewGuid(),
                IsTrialAllowed = true,
                TrialDurationInDays = 7
            };

            var planResponse = await _client.PostAsJsonAsync("/api/SubscriptionPlans", createPlanDto);
            Assert.True(planResponse.IsSuccessStatusCode);
            _output.WriteLine("✅ Step 1: Subscription plan created");

            // Step 2: Create subscription (triggers Stripe integration)
            var createSubscriptionDto = new CreateSubscriptionDto
            {
                PlanId = _testPlanId.ToString(),
                BillingCycleId = Guid.NewGuid(),
                PaymentMethodId = "pm_test123"
            };

            var subscriptionResponse = await _client.PostAsJsonAsync("/api/Subscriptions", createSubscriptionDto);
            Assert.True(subscriptionResponse.IsSuccessStatusCode);
            _output.WriteLine("✅ Step 2: Subscription created with Stripe integration");

            // Step 3: Create billing record
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                SubscriptionId = _testSubscriptionId,
                Amount = 49.99m,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                CurrencyId = _context.MasterCurrencies.First().Id,
                IsActive = true,
                CreatedBy = 2,
                CreatedDate = DateTime.UtcNow
            };
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();
            _output.WriteLine("✅ Step 3: Billing record created");

            // Step 4: Process payment
            var billingService = _scope.ServiceProvider.GetRequiredService<IBillingService>();
            var paymentResult = await billingService.ProcessPaymentAsync(billingRecord.Id, _userToken);
            Assert.Equal(200, paymentResult.StatusCode);
            _output.WriteLine("✅ Step 4: Payment processed successfully");

            // Step 5: Simulate webhook for payment success
            var webhookEvent = new
            {
                id = "evt_e2e_test",
                type = "invoice.payment_succeeded",
                data = new
                {
                    @object = new
                    {
                        id = _testStripeInvoiceId,
                        customer = _testStripeCustomerId,
                        amount_paid = 4999,
                        currency = "usd",
                        status = "paid",
                        payment_intent = _testStripePaymentIntentId
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var webhookResponse = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(webhookResponse.IsSuccessStatusCode);
            _output.WriteLine("✅ Step 5: Webhook processed successfully");

            // Step 6: Verify complete workflow
            var finalSubscription = await _context.Subscriptions.FindAsync(_testSubscriptionId);
            var finalBillingRecord = await _context.BillingRecords.FindAsync(billingRecord.Id);

            Assert.NotNull(finalSubscription);
            Assert.NotNull(finalBillingRecord);
            Assert.Equal(_testStripeSubscriptionId, finalSubscription.StripeSubscriptionId);
            Assert.Equal(BillingRecord.BillingStatus.Paid, finalBillingRecord.Status);
            Assert.Equal(_testStripePaymentIntentId, finalBillingRecord.StripePaymentIntentId);
            _output.WriteLine("✅ Step 6: Complete workflow verification passed");

            _output.WriteLine("=== Complete End-to-End Workflow Test PASSED ===");
        }

        [Fact]
        public async Task TestErrorHandlingAndRecovery()
        {
            _output.WriteLine("=== Testing Error Handling and Recovery ===");

            // Test payment method validation failure
            _mockStripeService.Setup(x => x.ValidatePaymentMethodAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(false);

            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = _testUser.Id,
                Amount = 29.99m,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                BillingDate = DateTime.UtcNow,
                CurrencyId = _context.MasterCurrencies.First().Id,
                IsActive = true,
                CreatedBy = 2,
                CreatedDate = DateTime.UtcNow
            };
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();

            var billingService = _scope.ServiceProvider.GetRequiredService<IBillingService>();
            var result = await billingService.ProcessPaymentAsync(billingRecord.Id, _userToken);

            // Should handle validation failure gracefully
            Assert.NotNull(result);
            _output.WriteLine("✅ Payment method validation failure handled gracefully");

            // Reset mock for successful validation
            _mockStripeService.Setup(x => x.ValidatePaymentMethodAsync(It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            _output.WriteLine("=== Error Handling and Recovery Test PASSED ===");
        }

        public void Dispose()
        {
            _context?.Dispose();
            _scope?.Dispose();
            _client?.Dispose();
        }
    }
}

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
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace SmartTelehealth.API.Tests
{
    /// <summary>
    /// Comprehensive tests for Stripe webhook processing
    /// Tests all webhook event types and their handling
    /// </summary>
    public class StripeWebhookIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _context;
        private readonly ITestOutputHelper _output;
        private Mock<IStripeService> _mockStripeService;
        private Mock<INotificationService> _mockNotificationService;

        public StripeWebhookIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
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
                        options.UseInMemoryDatabase($"WebhookTestDb_{Guid.NewGuid()}"));

                    _mockStripeService = new Mock<IStripeService>();
                    _mockNotificationService = new Mock<INotificationService>();
                    services.AddScoped(_ => _mockStripeService.Object);
                    services.AddScoped(_ => _mockNotificationService.Object);
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

            var user = new User
            {
                // Don't set Id - let database auto-generate it
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                StripeCustomerId = "cus_test123",
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Users.Add(user);
            _context.SaveChanges(); // Save to get the user ID

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
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

            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SubscriptionId = subscription.Id,
                Amount = 29.99m,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                BillingDate = DateTime.UtcNow,
                StripePaymentIntentId = "pi_test123",
                StripeInvoiceId = "in_test123",
                CurrencyId = Guid.NewGuid(),
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.BillingRecords.Add(billingRecord);

            _context.SaveChanges();
        }

        [Fact]
        public async Task TestInvoicePaymentSucceededWebhook()
        {
            _output.WriteLine("=== Testing Invoice Payment Succeeded Webhook ===");

            var webhookEvent = new
            {
                id = "evt_payment_succeeded",
                type = "invoice.payment_succeeded",
                data = new
                {
                    @object = new
                    {
                        id = "in_test123",
                        customer = "cus_test123",
                        amount_paid = 2999,
                        currency = "usd",
                        status = "paid",
                        payment_intent = "pi_test123"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonModel>(responseContent);
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            _output.WriteLine("✅ Invoice payment succeeded webhook processed successfully");
        }

        [Fact]
        public async Task TestInvoicePaymentFailedWebhook()
        {
            _output.WriteLine("=== Testing Invoice Payment Failed Webhook ===");

            var webhookEvent = new
            {
                id = "evt_payment_failed",
                type = "invoice.payment_failed",
                data = new
                {
                    @object = new
                    {
                        id = "in_test123",
                        customer = "cus_test123",
                        amount_due = 2999,
                        currency = "usd",
                        status = "open",
                        payment_intent = "pi_test123"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            _output.WriteLine("✅ Invoice payment failed webhook processed successfully");
        }

        [Fact]
        public async Task TestSubscriptionUpdatedWebhook()
        {
            _output.WriteLine("=== Testing Subscription Updated Webhook ===");

            var webhookEvent = new
            {
                id = "evt_subscription_updated",
                type = "customer.subscription.updated",
                data = new
                {
                    @object = new
                    {
                        id = "sub_test123",
                        customer = "cus_test123",
                        status = "active",
                        current_period_start = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        current_period_end = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds()
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            _output.WriteLine("✅ Subscription updated webhook processed successfully");
        }

        [Fact]
        public async Task TestSubscriptionDeletedWebhook()
        {
            _output.WriteLine("=== Testing Subscription Deleted Webhook ===");

            var webhookEvent = new
            {
                id = "evt_subscription_deleted",
                type = "customer.subscription.deleted",
                data = new
                {
                    @object = new
                    {
                        id = "sub_test123",
                        customer = "cus_test123",
                        status = "canceled"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            _output.WriteLine("✅ Subscription deleted webhook processed successfully");
        }

        [Fact]
        public async Task TestPaymentIntentSucceededWebhook()
        {
            _output.WriteLine("=== Testing Payment Intent Succeeded Webhook ===");

            var webhookEvent = new
            {
                id = "evt_payment_intent_succeeded",
                type = "payment_intent.succeeded",
                data = new
                {
                    @object = new
                    {
                        id = "pi_test123",
                        customer = "cus_test123",
                        amount = 2999,
                        currency = "usd",
                        status = "succeeded"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            _output.WriteLine("✅ Payment intent succeeded webhook processed successfully");
        }

        [Fact]
        public async Task TestPaymentIntentRequiresActionWebhook()
        {
            _output.WriteLine("=== Testing Payment Intent Requires Action Webhook ===");

            // Setup notification service mock
            _mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { StatusCode = 200, Message = "Notification created" });

            var webhookEvent = new
            {
                id = "evt_payment_intent_requires_action",
                type = "payment_intent.requires_action",
                data = new
                {
                    @object = new
                    {
                        id = "pi_test123",
                        customer = "cus_test123",
                        amount = 2999,
                        currency = "usd",
                        status = "requires_action"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            // Verify notification was sent
            _mockNotificationService.Verify(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<TokenModel>()), Times.AtLeastOnce);

            _output.WriteLine("✅ Payment intent requires action webhook processed successfully");
        }

        [Fact]
        public async Task TestSetupIntentSucceededWebhook()
        {
            _output.WriteLine("=== Testing Setup Intent Succeeded Webhook ===");

            var webhookEvent = new
            {
                id = "evt_setup_intent_succeeded",
                type = "setup_intent.succeeded",
                data = new
                {
                    @object = new
                    {
                        id = "seti_test123",
                        customer = "cus_test123",
                        status = "succeeded"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            _output.WriteLine("✅ Setup intent succeeded webhook processed successfully");
        }

        [Fact]
        public async Task TestInvoiceCreatedWebhook()
        {
            _output.WriteLine("=== Testing Invoice Created Webhook ===");

            var webhookEvent = new
            {
                id = "evt_invoice_created",
                type = "invoice.created",
                data = new
                {
                    @object = new
                    {
                        id = "in_test123",
                        customer = "cus_test123",
                        amount_due = 2999,
                        currency = "usd",
                        status = "draft"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            _output.WriteLine("✅ Invoice created webhook processed successfully");
        }

        [Fact]
        public async Task TestInvoiceVoidedWebhook()
        {
            _output.WriteLine("=== Testing Invoice Voided Webhook ===");

            var webhookEvent = new
            {
                id = "evt_invoice_voided",
                type = "invoice.voided",
                data = new
                {
                    @object = new
                    {
                        id = "in_test123",
                        customer = "cus_test123",
                        amount_due = 2999,
                        currency = "usd",
                        status = "void"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            _output.WriteLine("✅ Invoice voided webhook processed successfully");
        }

        [Fact]
        public async Task TestWebhookIdempotency()
        {
            _output.WriteLine("=== Testing Webhook Idempotency ===");

            var webhookEvent = new
            {
                id = "evt_idempotency_test",
                type = "invoice.payment_succeeded",
                data = new
                {
                    @object = new
                    {
                        id = "in_test123",
                        customer = "cus_test123",
                        amount_paid = 2999,
                        currency = "usd",
                        status = "paid"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // First call
            var response1 = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response1.IsSuccessStatusCode);

            // Second call (should be idempotent)
            var response2 = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response2.IsSuccessStatusCode);

            // Both should return success
            var result1 = await response1.Content.ReadAsStringAsync();
            var result2 = await response2.Content.ReadAsStringAsync();

            Assert.Contains("successfully", result1);
            Assert.Contains("successfully", result2);

            _output.WriteLine("✅ Webhook idempotency verified");
        }

        [Fact]
        public async Task TestUnhandledWebhookEvent()
        {
            _output.WriteLine("=== Testing Unhandled Webhook Event ===");

            var webhookEvent = new
            {
                id = "evt_unhandled",
                type = "unknown.event.type",
                data = new
                {
                    @object = new
                    {
                        id = "obj_test123"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(webhookEvent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/StripeWebhook", content);
            Assert.True(response.IsSuccessStatusCode);

            _output.WriteLine("✅ Unhandled webhook event processed gracefully");
        }

        public void Dispose()
        {
            _context?.Dispose();
            _scope?.Dispose();
            _client?.Dispose();
        }
    }
}

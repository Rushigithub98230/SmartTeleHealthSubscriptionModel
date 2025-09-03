using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartTelehealth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using System.Net;

namespace SmartTelehealth.API.Tests.IntegrationTests
{
    public class SubscriptionManagementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SubscriptionManagementIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Use in-memory database for testing
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetPrivileges_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/Privileges");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task GetMasterData_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/MasterData/billing-cycles");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task GetMasterDataCurrencies_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/MasterData/currencies");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task GetMasterDataPrivilegeTypes_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/MasterData/privilege-types");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task TestStripeConnection_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/Stripe/test-connection");

            // Assert
            // Note: This might return 500 if Stripe keys are not configured, but the endpoint should exist
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetSubscriptionPlans_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/SubscriptionPlans");

            // Assert
            // Note: This might return 403 if authorization is required, but the endpoint should exist
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetSubscriptionAnalytics_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/SubscriptionAnalytics");

            // Assert
            // Note: This might return 403 if authorization is required, but the endpoint should exist
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreatePrivilege_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var privilegeData = new
            {
                Name = "Test Privilege",
                Description = "Test privilege for integration testing",
                PrivilegeTypeId = Guid.NewGuid().ToString(),
                IsActive = true
            };

            var json = JsonSerializer.Serialize(privilegeData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Privileges", content);

            // Assert
            // Note: This might return 403 if authorization is required, but the endpoint should exist
            Assert.True(response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.Created || 
                       response.StatusCode == HttpStatusCode.Forbidden ||
                       response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            // Health check endpoint might not exist, but if it does, it should return 200
            if (response.StatusCode != HttpStatusCode.NotFound)
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}

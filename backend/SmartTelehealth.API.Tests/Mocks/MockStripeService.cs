using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Tests.Mocks
{
    // Simple DTOs for testing
    public class ProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PriceDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
    }

    public class CardDto
    {
        public string Brand { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
    }

    public class RecurringDto
    {
        public string Interval { get; set; } = string.Empty;
        public int IntervalCount { get; set; }
    }

    public class MockStripeService : IStripeService
    {
        private readonly Dictionary<string, object> _mockData;
        private readonly List<AuditLogEntry> _auditLogs;
        private bool _shouldFail;
        private string? _failureReason;

        public MockStripeService(bool shouldFail = false, string? failureReason = null)
        {
            _mockData = new Dictionary<string, object>();
            _auditLogs = new List<AuditLogEntry>();
            _shouldFail = shouldFail;
            _failureReason = failureReason;
            InitializeMockData();
        }

        private void InitializeMockData()
        {
            // Mock basic test data
            _mockData["cus_test_user"] = new CustomerDto { Id = "cus_test_user", Email = "test@example.com", Name = "Test User" };
            _mockData["pm_test_payment_method"] = new PaymentMethodDto { Id = "pm_test_payment_method", Type = "card" };
            _mockData["prod_basic_test"] = new ProductDto { Id = "prod_basic_test", Name = "Basic Plan" };
            _mockData["price_basic_monthly_test"] = new PriceDto { Id = "price_basic_monthly_test", ProductId = "prod_basic_test" };
        }

        public async Task<string> CreateCustomerAsync(string email, string name, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            var customerId = $"cus_test_{Guid.NewGuid().ToString("N")[..8]}";
            _mockData[customerId] = new CustomerDto { Id = customerId, Email = email, Name = name };
            return customerId;
        }

        public async Task<CustomerDto> GetCustomerAsync(string customerId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.TryGetValue(customerId, out var customer) ? customer as CustomerDto : throw new Exception("Not found");
        }

        public async Task<IEnumerable<CustomerDto>> ListCustomersAsync(TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.Values.Where(v => v is CustomerDto).Cast<CustomerDto>();
        }

        public async Task<string> CreatePaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            var newId = $"pm_test_{Guid.NewGuid().ToString("N")[..8]}";
            _mockData[newId] = new PaymentMethodDto { Id = newId, Type = "card" };
            return newId;
        }

        public async Task<bool> UpdatePaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(paymentMethodId);
        }

        public async Task<string> AddPaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
        {
            return await CreatePaymentMethodAsync(customerId, paymentMethodId, tokenModel);
        }

        public async Task<bool> SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(paymentMethodId);
        }

        public async Task<bool> RemovePaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.Remove(paymentMethodId);
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.Values.Where(v => v is PaymentMethodDto).Cast<PaymentMethodDto>();
        }

        public async Task<bool> ValidatePaymentMethodAsync(string paymentMethodId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(paymentMethodId);
        }

        public async Task<PaymentMethodValidationDto> ValidatePaymentMethodDetailedAsync(string paymentMethodId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return new PaymentMethodValidationDto
            {
                IsValid = _mockData.ContainsKey(paymentMethodId),
                ValidationMessage = "Mock validation"
            };
        }

        public async Task<string> CreateProductAsync(string name, string description, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            var productId = $"prod_test_{Guid.NewGuid().ToString("N")[..8]}";
            _mockData[productId] = new ProductDto { Id = productId, Name = name, Description = description };
            return productId;
        }

        public async Task<bool> UpdateProductAsync(string productId, string name, string description, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(productId);
        }

        public async Task<bool> DeleteProductAsync(string productId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.Remove(productId);
        }

        public async Task<string> CreatePriceAsync(string productId, decimal amount, string currency, string interval, int intervalCount, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            var priceId = $"price_test_{Guid.NewGuid().ToString("N")[..8]}";
            _mockData[priceId] = new PriceDto { Id = priceId, ProductId = productId };
            return priceId;
        }

        public async Task<bool> UpdatePriceAsync(string priceId, decimal amount, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(priceId);
        }

        public async Task<string> UpdatePriceWithNewPriceAsync(string oldPriceId, string productId, decimal newAmount, string currency, string interval, int intervalCount, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            var newPriceId = $"price_test_{Guid.NewGuid().ToString("N")[..8]}";
            _mockData[newPriceId] = new PriceDto { Id = newPriceId, ProductId = productId };
            return newPriceId;
        }

        public async Task<bool> DeactivatePriceAsync(string priceId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(priceId);
        }

        public async Task<string> CreateSubscriptionAsync(string customerId, string priceId, string paymentMethodId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            var subscriptionId = $"sub_test_{Guid.NewGuid().ToString("N")[..8]}";
            _mockData[subscriptionId] = new SubscriptionDto { Id = Guid.NewGuid().ToString(), StripeCustomerId = customerId };
            return subscriptionId;
        }

        public async Task<bool> CancelSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(subscriptionId);
        }

        public async Task<SubscriptionDto> GetSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.TryGetValue(subscriptionId, out var sub) ? sub as SubscriptionDto : throw new Exception("Not found");
        }

        public async Task<bool> UpdateSubscriptionAsync(string subscriptionId, string priceId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(subscriptionId);
        }

        public async Task<bool> PauseSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(subscriptionId);
        }

        public async Task<bool> ResumeSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return _mockData.ContainsKey(subscriptionId);
        }

        public async Task<PaymentResultDto> ProcessPaymentAsync(string paymentMethodId, decimal amount, string currency, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            
            // Check if we have a predefined payment result
            if (_mockData.TryGetValue("payment_result", out var predefinedResult))
            {
                return predefinedResult as PaymentResultDto;
            }
            
            // Fallback to generating a new payment result
            var paymentIntentId = $"pi_test_{Guid.NewGuid().ToString("N")[..8]}";
            var paymentResult = new PaymentResultDto
            {
                PaymentIntentId = paymentIntentId,
                Status = "succeeded",
                Amount = amount,
                Currency = currency
            };
            
            // Store the payment result with both the payment intent ID and a generic key
            _mockData[paymentIntentId] = paymentResult;
            _mockData["last_payment_result"] = paymentResult;
            
            return paymentResult;
        }

        public async Task<bool> ProcessRefundAsync(string paymentIntentId, decimal amount, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return true;
        }

        public async Task<string> CreateCheckoutSessionAsync(string priceId, string successUrl, string cancelUrl, TokenModel tokenModel)
        {
            if (_shouldFail) throw new Exception(_failureReason ?? "Mock failure");
            return $"cs_test_{Guid.NewGuid().ToString("N")[..8]}";
        }

        public async Task<bool> ProcessWebhookAsync(string json, string signature, TokenModel tokenModel)
        {
            if (_shouldFail) return false; // Return false instead of throwing exception
            
            // Simulate audit logging for webhook processing
            _auditLogs.Add(new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Action = "WebhookProcessed",
                EntityType = "Webhook",
                EntityId = "webhook_test",
                Details = "Webhook processed successfully",
                UserId = tokenModel?.UserID ?? 0,
                Timestamp = DateTime.UtcNow
            });
            
            return true;
        }

        // Helper methods for testing
        public bool HasMockData(string key) => _mockData.ContainsKey(key);
        public object GetMockData(string key) => _mockData.TryGetValue(key, out var value) ? value : null;
        public void SetMockData(string key, object value) => _mockData[key] = value;
        public int GetAuditLogCount() => _auditLogs.Count;
        public bool HasAnyPaymentData() => _mockData.Any(kvp => kvp.Key.StartsWith("pi_test_"));
        public void SetFailureMode(bool shouldFail, string? failureReason = null)
        {
            _shouldFail = shouldFail;
            _failureReason = failureReason;
        }
    }
}

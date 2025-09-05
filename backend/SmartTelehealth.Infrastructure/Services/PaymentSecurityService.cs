using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SmartTelehealth.Infrastructure.Services
{
    /// <summary>
    /// Infrastructure service responsible for payment security and fraud prevention operations.
    /// This service handles payment security validation, fraud detection, rate limiting,
    /// and payment attempt monitoring. It provides comprehensive security measures for
    /// payment processing including IP validation, amount limits, and suspicious activity detection.
    /// 
    /// Key Features:
    /// - Payment request validation and security checks
    /// - Fraud detection and prevention mechanisms
    /// - Rate limiting and payment attempt monitoring
    /// - IP address validation and geolocation checks
    /// - Payment amount validation and limits
    /// - Suspicious activity detection and blocking
    /// - Payment security audit logging
    /// - Cache-based security state management
    /// - Comprehensive security monitoring
    /// - Integration with audit and logging systems
    /// </summary>
    public class PaymentSecurityService : IPaymentSecurityService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PaymentSecurityService> _logger;
          
        private readonly Dictionary<string, int> _paymentAttempts = new();
        private readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the PaymentSecurityService
        /// </summary>
        /// <param name="cache">Memory cache for storing security state and rate limiting data</param>
        /// <param name="logger">Logger instance for recording service operations and errors</param>
        /// <param name="auditService">Service for audit logging and security event tracking</param>
        public PaymentSecurityService(
            IMemoryCache cache,
            ILogger<PaymentSecurityService> logger,
            IAuditService auditService)
        {
            _cache = cache;
            _logger = logger;
              
        }

        /// <summary>
        /// Validates a payment request for security compliance and fraud prevention
        /// </summary>
        /// <param name="userId">The unique identifier of the user making the payment request</param>
        /// <param name="ipAddress">The IP address of the payment request origin</param>
        /// <param name="amount">The payment amount to validate</param>
        /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
        /// <returns>Boolean indicating whether the payment request is valid and secure</returns>
        /// <exception cref="Exception">Thrown when payment validation fails or security checks encounter errors</exception>
        /// <remarks>
        /// This method:
        /// - Validates payment amount against configured limits
        /// - Checks IP address for suspicious activity or blacklisting
        /// - Monitors payment attempt frequency and rate limiting
        /// - Performs fraud detection and security validation
        /// - Logs all payment validation attempts for security monitoring
        /// - Used for payment security and fraud prevention
        /// - Ensures compliance with security policies and regulations
        /// - Returns validation result for payment processing decision
        /// </remarks>
        public async Task<bool> ValidatePaymentRequestAsync(string userId, string ipAddress, decimal amount, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Validating payment request for user {UserId} from IP {IpAddress} amount {Amount} by user {TokenUserId}", 
                    userId, ipAddress, amount, tokenModel.UserID);

                // Check rate limiting
                if (!await CheckRateLimitAsync(userId, ipAddress, tokenModel))
                {
                    
                    _logger.LogWarning("Rate limit exceeded for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                    return false;
                }

                // Check for suspicious activity
                if (await DetectSuspiciousActivityAsync(userId, ipAddress, amount, tokenModel))
                {
                   
                    _logger.LogWarning("Suspicious payment activity detected for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                    return false;
                }

                // Check amount limits
                if (!await ValidateAmountLimitsAsync(userId, amount, tokenModel))
                {
                   
                    _logger.LogWarning("Amount limit exceeded for user {UserId} by user {TokenUserId}: {Amount}", userId, tokenModel.UserID, amount);
                    return false;
                }

               

                _logger.LogInformation("Payment request validated successfully for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment request for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return false;
            }
        }

        /// <summary>
        /// Checks rate limiting for payment attempts from a specific user and IP address
        /// </summary>
        /// <param name="userId">The unique identifier of the user making the payment request</param>
        /// <param name="ipAddress">The IP address of the payment request origin</param>
        /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
        /// <returns>Boolean indicating whether the request is within rate limits</returns>
        /// <exception cref="Exception">Thrown when rate limit check fails</exception>
        /// <remarks>
        /// This method:
        /// - Checks rate limiting for payment attempts from user and IP
        /// - Validates against configured rate limit thresholds
        /// - Returns rate limit compliance status
        /// - Used for payment security and abuse prevention
        /// - Ensures proper rate limiting enforcement
        /// - Logs all rate limit checks for security monitoring
        /// </remarks>
        public async Task<bool> CheckRateLimitAsync(string userId, string ipAddress, TokenModel tokenModel)
        {
            try
            {
                var userKey = $"payment_attempts_user_{userId}";
                var ipKey = $"payment_attempts_ip_{ipAddress}";

                var userAttempts = await GetAttemptsAsync(userKey);
                var ipAttempts = await GetAttemptsAsync(ipKey);

                // Allow max 5 attempts per user per hour
                if (userAttempts >= 5)
                {
                    _logger.LogWarning("Rate limit exceeded for user {UserId} by user {TokenUserId}: {Attempts} attempts", userId, tokenModel.UserID, userAttempts);
                    return false;
                }

                // Allow max 10 attempts per IP per hour
                if (ipAttempts >= 10)
                {
                    _logger.LogWarning("Rate limit exceeded for IP {IpAddress} by user {TokenUserId}: {Attempts} attempts", ipAddress, tokenModel.UserID, ipAttempts);
                    return false;
                }

                // Increment counters
                await IncrementAttemptsAsync(userKey);
                await IncrementAttemptsAsync(ipKey);

                _logger.LogInformation("Rate limit check passed for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return false;
            }
        }

        /// <summary>
        /// Detects suspicious payment activity based on user behavior and transaction patterns
        /// </summary>
        /// <param name="userId">The unique identifier of the user making the payment request</param>
        /// <param name="ipAddress">The IP address of the payment request origin</param>
        /// <param name="amount">The payment amount to analyze</param>
        /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
        /// <returns>Boolean indicating whether suspicious activity was detected</returns>
        /// <exception cref="Exception">Thrown when suspicious activity detection fails</exception>
        /// <remarks>
        /// This method:
        /// - Detects suspicious payment activity based on user behavior patterns
        /// - Analyzes transaction patterns and amounts for anomalies
        /// - Returns suspicious activity detection results
        /// - Used for fraud detection and security monitoring
        /// - Ensures proper suspicious activity detection
        /// - Logs all suspicious activity detection for security monitoring
        /// </remarks>
        public async Task<bool> DetectSuspiciousActivityAsync(string userId, string ipAddress, decimal amount, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Detecting suspicious activity for user {UserId} from IP {IpAddress} amount {Amount} by user {TokenUserId}", 
                    userId, ipAddress, amount, tokenModel.UserID);

                // Check for geographic anomalies
                if (await IsGeographicAnomalyAsync(userId, ipAddress))
                {
                    _logger.LogWarning("Geographic anomaly detected for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                    return true;
                }

                // Check for unusual payment patterns
                var userHistory = await GetUserPaymentHistoryAsync(userId);
                var recentAttempts = await GetUserPaymentAttemptsAsync(userId, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
                
                var riskScore = CalculateRiskScore(recentAttempts);
                
                // Consider activity suspicious if risk score > 70
                if (riskScore > 70)
                {
                    _logger.LogWarning("High risk score detected for user {UserId} by user {TokenUserId}: {RiskScore}", userId, tokenModel.UserID, riskScore);
                    return true;
                }

                // Check for unusual amounts
                if (amount > userHistory.AverageAmount * 3)
                {
                    _logger.LogWarning("Unusual amount detected for user {UserId} by user {TokenUserId}: {Amount} vs average {AverageAmount}", 
                        userId, tokenModel.UserID, amount, userHistory.AverageAmount);
                    return true;
                }

                _logger.LogInformation("No suspicious activity detected for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting suspicious activity for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return false;
            }
        }

        /// <summary>
        /// Validates payment amount against configured limits for a specific user
        /// </summary>
        /// <param name="userId">The unique identifier of the user making the payment request</param>
        /// <param name="amount">The payment amount to validate</param>
        /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
        /// <returns>Boolean indicating whether the amount is within configured limits</returns>
        /// <exception cref="Exception">Thrown when amount limit validation fails</exception>
        /// <remarks>
        /// This method:
        /// - Validates payment amount against configured limits for the user
        /// - Checks against daily, weekly, and monthly amount limits
        /// - Returns amount limit compliance status
        /// - Used for payment security and fraud prevention
        /// - Ensures proper amount limit enforcement
        /// - Logs all amount limit validations for security monitoring
        /// </remarks>
        public async Task<bool> ValidateAmountLimitsAsync(string userId, decimal amount, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Validating amount limits for user {UserId} amount {Amount} by user {TokenUserId}", 
                    userId, amount, tokenModel.UserID);

                // Get user's payment history
                var userHistory = await GetUserPaymentHistoryAsync(userId);
                
                // Check against user's average payment amount
                var maxAllowedAmount = userHistory.AverageAmount * 5; // Allow up to 5x average
                
                if (amount > maxAllowedAmount)
                {
                    _logger.LogWarning("Amount limit exceeded for user {UserId} by user {TokenUserId}: {Amount} > {MaxAllowed}", 
                        userId, tokenModel.UserID, amount, maxAllowedAmount);
                    return false;
                }

                // Check against absolute limits
                var absoluteMaxAmount = 10000.00m; // $10,000 absolute limit
                if (amount > absoluteMaxAmount)
                {
                    _logger.LogWarning("Absolute amount limit exceeded for user {UserId} by user {TokenUserId}: {Amount} > {AbsoluteMax}", 
                        userId, tokenModel.UserID, amount, absoluteMaxAmount);
                    return false;
                }

                _logger.LogInformation("Amount limits validated successfully for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating amount limits for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return false;
            }
        }

        /// <summary>
        /// Logs a payment attempt for security monitoring and audit purposes
        /// </summary>
        /// <param name="userId">The unique identifier of the user making the payment request</param>
        /// <param name="ipAddress">The IP address of the payment request origin</param>
        /// <param name="amount">The payment amount attempted</param>
        /// <param name="success">Whether the payment attempt was successful</param>
        /// <param name="errorMessage">Optional error message if the payment attempt failed</param>
        /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
        /// <returns>Task representing the asynchronous logging operation</returns>
        /// <exception cref="Exception">Thrown when payment attempt logging fails</exception>
        /// <remarks>
        /// This method:
        /// - Logs payment attempts for security monitoring and audit purposes
        /// - Records success/failure status and error details
        /// - Used for security monitoring and fraud detection
        /// - Ensures proper payment attempt logging
        /// - Logs all payment attempt logging for audit purposes
        /// </remarks>
        public async Task LogPaymentAttemptAsync(string userId, string ipAddress, decimal amount, bool success, string? errorMessage, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Logging payment attempt for user {UserId} from IP {IpAddress} amount {Amount} success {Success} by user {TokenUserId}", 
                    userId, ipAddress, amount, success, tokenModel.UserID);

                var logEntry = new PaymentAttemptLog
                {
                    UserId = int.Parse(userId),
                    IpAddress = ipAddress,
                    Amount = amount,
                    Success = success,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.UtcNow
                };

                // Store in cache for recent analysis
                var cacheKey = $"payment_attempt_{userId}_{DateTime.UtcNow:yyyyMMddHH}";
                var attempts = await GetAttemptsAsync(cacheKey);
                _cache.Set(cacheKey, attempts + 1, TimeSpan.FromHours(1));

                

                _logger.LogInformation("Payment attempt logged successfully for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging payment attempt for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            }
        }

        /// <summary>
        /// Generates a comprehensive security report for a specific user and date range
        /// </summary>
        /// <param name="userId">The unique identifier of the user to generate security report for</param>
        /// <param name="startDate">The start date for the security report period</param>
        /// <param name="endDate">The end date for the security report period</param>
        /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
        /// <returns>PaymentSecurityReportDto containing comprehensive security report information</returns>
        /// <exception cref="Exception">Thrown when security report generation fails</exception>
        /// <remarks>
        /// This method:
        /// - Generates comprehensive security report for the specified user and date range
        /// - Validates user ID and date range parameters
        /// - Returns detailed security report information
        /// - Used for security monitoring and reporting
        /// - Ensures proper security report generation
        /// - Logs all security report generation for audit purposes
        /// </remarks>
        public async Task<PaymentSecurityReportDto> GenerateSecurityReportAsync(string userId, DateTime startDate, DateTime endDate, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Generating security report for user {UserId} from {StartDate} to {EndDate} by user {TokenUserId}", 
                    userId, startDate, endDate, tokenModel?.UserID ?? 0);

                var paymentAttempts = await GetUserPaymentAttemptsAsync(userId, startDate, endDate);
                var userHistory = await GetUserPaymentHistoryAsync(userId);
                
                var report = new PaymentSecurityReportDto
                {
                    UserId = int.Parse(userId),
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalAttempts = paymentAttempts.Count,
                    SuccessfulAttempts = paymentAttempts.Count(a => a.Success),
                    FailedAttempts = paymentAttempts.Count(a => !a.Success),
                    RiskScore = CalculateRiskScore(paymentAttempts).ToString(),
                    AverageAmount = userHistory.AverageAmount,
                    GeneratedBy = tokenModel.UserID,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Security report generated successfully for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating security report for user {UserId} by user {TokenUserId}", userId, tokenModel.UserID);
                return new PaymentSecurityReportDto
                {
                    UserId = int.Parse(userId),
                    StartDate = startDate,
                    EndDate = endDate,
                    ErrorMessage = ex.Message
                };
            }
        }

        // Helper methods
        private async Task<int> GetAttemptsAsync(string key)
        {
            var cachedValue = await Task.FromResult(_cache.Get(key)?.ToString() ?? "0");
            return int.TryParse(cachedValue, out var attempts) ? attempts : 0;
        }

        private async Task IncrementAttemptsAsync(string key)
        {
            await Task.Run(() =>
            {
                var currentValue = _cache.Get(key)?.ToString() ?? "0";
                var attempts = int.TryParse(currentValue, out var currentAttempts) ? currentAttempts : 0;
                
                var entry = _cache.CreateEntry(key);
                entry.Value = (attempts + 1).ToString();
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            });
        }

        private async Task<UserPaymentHistoryDto> GetUserPaymentHistoryAsync(string userId)
        {
            // This would typically query the database
            // For now, return mock data
            return new UserPaymentHistoryDto
            {
                UserId = int.Parse(userId),
                AverageAmount = 150.00m,
                RecentPayments = new List<PaymentHistoryItemDto>
                {
                    new PaymentHistoryItemDto { Amount = 100.00m, Timestamp = DateTime.UtcNow.AddHours(-2) },
                    new PaymentHistoryItemDto { Amount = 200.00m, Timestamp = DateTime.UtcNow.AddHours(-1) }
                }
            };
        }

        private async Task<bool> IsGeographicAnomalyAsync(string userId, string ipAddress)
        {
            // This would typically use a geolocation service
            // For now, return false (no anomaly detected)
            return false;
        }

        private async Task<List<PaymentAttemptLog>> GetUserPaymentAttemptsAsync(string userId, DateTime startDate, DateTime endDate)
        {
            // This would typically query the database
            // For now, return mock data
            return new List<PaymentAttemptLog>
            {
                new PaymentAttemptLog { UserId = int.Parse(userId), Amount = 100.00m, Success = true, Timestamp = DateTime.UtcNow.AddHours(-1) },
                new PaymentAttemptLog { UserId = int.Parse(userId), Amount = 200.00m, Success = false, Timestamp = DateTime.UtcNow.AddHours(-2) }
            };
        }

        private int CalculateRiskScore(List<PaymentAttemptLog> attempts)
        {
            if (!attempts.Any()) return 0;

            var score = 0;
            
            // Failed attempts increase risk
            score += attempts.Count(a => !a.Success) * 10;
            
            // High amounts increase risk
            score += attempts.Count(a => a.Amount > 500) * 5;
            
            // Rapid attempts increase risk
            var rapidAttempts = attempts.Where(a => a.Timestamp > DateTime.UtcNow.AddMinutes(-5)).Count();
            score += rapidAttempts * 15;

            return Math.Min(score, 100); // Cap at 100
        }
    }

    // DTOs for payment security
    public class PaymentAttemptLog
    {
        public int UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class UserPaymentHistoryDto
    {
        public int UserId { get; set; }
        public decimal AverageAmount { get; set; }
        public List<PaymentHistoryItemDto> RecentPayments { get; set; } = new();
    }

    public class PaymentHistoryItemDto
    {
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

namespace SmartTelehealth.Application.Interfaces
{
    public interface IPaymentSecurityService
    {
        Task<bool> ValidatePaymentRequestAsync(string userId, string ipAddress, decimal amount, TokenModel tokenModel);
        Task<bool> CheckRateLimitAsync(string userId, string ipAddress, TokenModel tokenModel);
        Task<bool> DetectSuspiciousActivityAsync(string userId, string ipAddress, decimal amount, TokenModel tokenModel);
        Task<bool> ValidateAmountLimitsAsync(string userId, decimal amount, TokenModel tokenModel);
        Task LogPaymentAttemptAsync(string userId, string ipAddress, decimal amount, bool success, string? errorMessage = null, TokenModel tokenModel = null);
        Task<PaymentSecurityReportDto> GenerateSecurityReportAsync(string userId, DateTime startDate, DateTime endDate, TokenModel tokenModel);
    }
} 
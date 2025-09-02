using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartTelehealth.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;
        private readonly string _encryptionKey;

        public AuditService(
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditService> logger)
        {
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _encryptionKey = Environment.GetEnvironmentVariable("AUDIT_ENCRYPTION_KEY") ?? "default-encryption-key-change-in-production";
        }

        public async Task<JsonModel> GetAuditLogByIdAsync(Guid id, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Getting audit log {Id} by user {TokenUserId}", id, tokenModel?.UserID ?? 0);
                
                var auditLog = await _auditLogRepository.GetByIdAsync(id);
                if (auditLog == null)
                    return new JsonModel { data = new object(), Message = "Audit log not found", StatusCode = 404 };
                
                var dto = _mapper.Map<AuditLogDto>(auditLog);
                
                _logger.LogInformation("Audit log {Id} retrieved successfully by user {TokenUserId}", id, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dto, Message = "Audit log retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit log {Id} by user {TokenUserId}", id, tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving the audit log", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetUserAuditLogsAsync(int userId, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Getting audit logs for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
                
                var auditLogs = await _auditLogRepository.GetByUserIdAsync(userId);
                var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);
                
                _logger.LogInformation("Retrieved {Count} audit logs for user {UserId} by user {TokenUserId}", dtos.Count, userId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dtos, Message = "User audit logs retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving user audit logs", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> SearchAuditLogsAsync(AuditLogSearchDto searchDto, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Searching audit logs by user {TokenUserId}", tokenModel?.UserID ?? 0);
                
                var auditLogs = await _auditLogRepository.GetWithFiltersAsync(
                    searchDto.Action, 
                    searchDto.UserId, 
                    searchDto.StartDate, 
                    searchDto.EndDate, 
                    searchDto.Page, 
                    searchDto.PageSize);
                var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);
                
                _logger.LogInformation("Search completed with {Count} results by user {TokenUserId}", dtos.Count, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dtos, Message = "Audit log search completed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching audit logs by user {TokenUserId}", tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while searching audit logs", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetRecentAuditLogsAsync(int count, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Getting {Count} recent audit logs by user {TokenUserId}", count, tokenModel?.UserID ?? 0);
                
                var auditLogs = await _auditLogRepository.GetRecentAsync(count);
                var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);
                
                _logger.LogInformation("Retrieved {Count} recent audit logs by user {TokenUserId}", dtos.Count, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dtos, Message = "Recent audit logs retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent audit logs by user {TokenUserId}", tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving recent audit logs", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetAuditLogsAsync(string? action, int? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Getting audit logs with filters by user {TokenUserId}", tokenModel?.UserID ?? 0);
                
                var auditLogs = await _auditLogRepository.GetWithFiltersAsync(action, userId, startDate, endDate, page, pageSize);
                var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);
                
                _logger.LogInformation("Retrieved {Count} audit logs with filters by user {TokenUserId}", dtos.Count, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dtos, Message = "Audit logs retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs with filters by user {TokenUserId}", tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving audit logs", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetUserAuditLogCountAsync(int userId, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Getting audit log count for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
                
                var count = await _auditLogRepository.GetCountByUserIdAsync(userId);
                
                _logger.LogInformation("Retrieved audit log count {Count} for user {UserId} by user {TokenUserId}", count, userId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = count, Message = "User audit log count retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit log count for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving user audit log count", StatusCode = 500 };
            }
        }

        public async Task LogUserActionAsync(int userId, string action, string entityType, string? entityId, string? description, TokenModel tokenModel)
        {
            try
            {
                var createDto = new CreateAuditLogDto
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId ?? "",
                    UserId = userId,
                    Description = description ?? ""
                };

                await CreateAuditLogAsync(createDto, tokenModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user action: {Action} by user {UserId}", action, userId);
            }
        }

        public async Task<JsonModel> CreateAuditLogAsync(CreateAuditLogDto createDto, TokenModel tokenModel)
        {
            try
            {
                _logger.LogInformation("Creating audit log by user {TokenUserId}", tokenModel?.UserID ?? 0);
                
                var auditLog = _mapper.Map<AuditLog>(createDto);
                auditLog.Timestamp = DateTime.UtcNow;

                // Add HTTP context information
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    auditLog.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                    auditLog.UserAgent = httpContext.Request.Headers["User-Agent"].ToString();
                }

                // Encrypt sensitive data for payment events
                if (auditLog.EntityType == "Payment")
                {
                    auditLog.Description = EncryptSensitiveData(auditLog.Description);
                    if (!string.IsNullOrEmpty(auditLog.EntityId))
                    {
                        auditLog.EntityId = EncryptSensitiveData(auditLog.EntityId);
                    }
                }

                var createdLog = await _auditLogRepository.CreateAsync(auditLog);
                
                // Log to file with sanitized data
                _logger.LogInformation("AUDIT: {Action} by {UserId} on {EntityType} - {SanitizedDescription}",
                    auditLog.Action, auditLog.UserId, auditLog.EntityType, SanitizeDescription(auditLog.Description));

                var dto = _mapper.Map<AuditLogDto>(createdLog);
                
                _logger.LogInformation("Audit log created successfully by user {TokenUserId}", tokenModel?.UserID ?? 0);
                return new JsonModel { data = dto, Message = "Audit log created successfully", StatusCode = 201 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log by user {TokenUserId}: {Action} by {UserId}", tokenModel?.UserID ?? 0, createDto.Action, createDto.UserId);
                return new JsonModel { data = new object(), Message = "An error occurred while creating the audit log", StatusCode = 500 };
            }
        }











        public async Task LogDataChangeAsync(int userId, string entityType, string entityId, string? oldValues, string? newValues, TokenModel tokenModel)
        {
            _logger.LogInformation("Logging data change by user {TokenUserId}: {EntityType} {EntityId}", 
                tokenModel?.UserID ?? 0, entityType, entityId);
            
            var createDto = new CreateAuditLogDto
            {
                Action = "DataChange",
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                Description = $"Data changed for {entityType} {entityId}",
                OldValues = oldValues,
                NewValues = newValues
            };

            await CreateAuditLogAsync(createDto, tokenModel);
        }

        public async Task LogSecurityEventAsync(int userId, string action, string? description, string? ipAddress, TokenModel tokenModel)
        {
            _logger.LogInformation("Logging security event by user {TokenUserId}: {Action} for user {UserId}", 
                tokenModel?.UserID ?? 0, action, userId);
            
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "Security",
                UserId = userId,
                Description = description ?? "",
                IpAddress = ipAddress ?? ""
            };

            await CreateAuditLogAsync(createDto, tokenModel);
        }

        public async Task LogPaymentEventAsync(int userId, string action, string? entityId, string? status, string? errorMessage, TokenModel tokenModel)
        {
            _logger.LogInformation("Logging payment event by user {TokenUserId}: {Action} for user {UserId}", 
                tokenModel?.UserID ?? 0, action, userId);
            
            // Sanitize payment data for logging
            var sanitizedEntityId = SanitizePaymentData(entityId);
            var sanitizedErrorMessage = SanitizePaymentData(errorMessage);
            
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "Payment",
                EntityId = sanitizedEntityId ?? "",
                UserId = userId,
                Description = $"Payment {action} for user {userId}",
                Status = status,
                ErrorMessage = sanitizedErrorMessage
            };

            await CreateAuditLogAsync(createDto, tokenModel);
        }

        public async Task LogSubscriptionEventAsync(int userId, string action, string? subscriptionId, string? status, TokenModel tokenModel)
        {
            _logger.LogInformation("Logging subscription event by user {TokenUserId}: {Action} for user {UserId}", 
                tokenModel?.UserID ?? 0, action, userId);
            
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "Subscription",
                EntityId = subscriptionId ?? "",
                UserId = userId,
                Description = $"Subscription {action} for user {userId}"
            };

            await CreateAuditLogAsync(createDto, tokenModel);
        }

        public async Task LogConsultationEventAsync(int userId, string action, string? consultationId, string? status, TokenModel tokenModel)
        {
            _logger.LogInformation("Logging consultation event by user {TokenUserId}: {Action} for user {UserId}", 
                tokenModel?.UserID ?? 0, action, userId);
            
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "Consultation",
                EntityId = consultationId ?? "",
                UserId = userId,
                Description = $"Consultation {action} for user {userId}"
            };

            await CreateAuditLogAsync(createDto, tokenModel);
        }

        public async Task LogActionAsync(string entity, string action, string entityId, string description, TokenModel tokenModel)
        {
            _logger.LogInformation("Logging action by user {TokenUserId}: {Action} on {Entity} {EntityId}", 
                tokenModel?.UserID ?? 0, action, entity, entityId);
            
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = entity,
                EntityId = entityId,
                UserId = GetCurrentUserId(), // fallback to empty if not available
                Description = description
            };
            await CreateAuditLogAsync(createDto, tokenModel);
        }

        // Helper to get userId from context (if available)
        private int GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return int.TryParse(userId, out var parsedUserId) ? parsedUserId : 0;
        }

        // PCI-Compliant Payment Data Sanitization
        private string? SanitizePaymentData(string? data)
        {
            if (string.IsNullOrEmpty(data)) return data;

            // Remove or mask sensitive payment information
            var sanitized = data
                .Replace(Regex.Replace(data, @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", "****-****-****-****"), "") // Credit card numbers
                .Replace(Regex.Replace(data, @"\b\d{3}-\d{2}-\d{4}\b", "***-**-****"), "") // SSN
                .Replace(Regex.Replace(data, @"\b\d{3}\d{2}\d{4}\b", "******"), ""); // SSN without dashes

            return sanitized;
        }

        // Encrypt sensitive data for storage
        private string EncryptSensitiveData(string data)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];

                using var encryptor = aes.CreateEncryptor();
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting sensitive data");
                return "[ENCRYPTION_ERROR]";
            }
        }

        // Decrypt sensitive data for retrieval
        private string DecryptSensitiveData(string encryptedData)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];

                using var decryptor = aes.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(encryptedData);
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting sensitive data");
                return "[DECRYPTION_ERROR]";
            }
        }

        // Sanitize description for file logging
        private string SanitizeDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return description;

            // Remove sensitive patterns from log output
            return Regex.Replace(description, 
                @"(?:payment|card|account|routing|swift|iban|bic)\s*[:=]\s*\S+", 
                "[REDACTED]", 
                RegexOptions.IgnoreCase);
        }

        // CRITICAL FIX: Webhook idempotency methods
        public async Task<bool> IsEventProcessedAsync(string eventId)
        {
            try
            {
                // Check if this event has already been processed by looking for existing audit log
                var existingLog = await _auditLogRepository.GetByEntityIdAsync(eventId);
                return existingLog != null && existingLog.Action == "Processed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if event {EventId} was processed", eventId);
                return false; // If we can't check, assume not processed to be safe
            }
        }

        public async Task MarkEventAsProcessedAsync(string eventId, string eventType, string status)
        {
            try
            {
                var createDto = new CreateAuditLogDto
                {
                    Action = status, // "Processed", "Failed", etc.
                    EntityType = "Webhook",
                    EntityId = eventId,
                    UserId = 0, // System user
                    Description = $"Webhook event {eventType} {status.ToLower()}"
                };

                await CreateAuditLogAsync(createDto, new TokenModel { UserID = 0, RoleID = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking event {EventId} as {Status}", eventId, status);
            }
        }
    }
} 
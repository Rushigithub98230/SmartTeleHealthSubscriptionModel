using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IAuditService
    {
        Task<JsonModel> GetAuditLogByIdAsync(Guid id, TokenModel tokenModel);
        Task<JsonModel> CreateAuditLogAsync(CreateAuditLogDto createDto, TokenModel tokenModel);
        Task<JsonModel> GetUserAuditLogsAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> SearchAuditLogsAsync(AuditLogSearchDto searchDto, TokenModel tokenModel);
        Task<JsonModel> GetRecentAuditLogsAsync(int count, TokenModel tokenModel);
        Task<JsonModel> GetAuditLogsAsync(string? action, int? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize, TokenModel tokenModel);
        Task<JsonModel> GetUserAuditLogCountAsync(int userId, TokenModel tokenModel);
        
        // Business-specific audit methods
        Task LogUserActionAsync(int userId, string action, string entityType, string? entityId, string? description, TokenModel tokenModel);
        Task LogDataChangeAsync(int userId, string entityType, string entityId, string? oldValues, string? newValues, TokenModel tokenModel);
        Task LogSecurityEventAsync(int userId, string action, string? description, string? ipAddress, TokenModel tokenModel);
        Task LogPaymentEventAsync(int userId, string action, string? entityId, string? status, string? errorMessage, TokenModel tokenModel);
        Task LogSubscriptionEventAsync(int userId, string action, string? subscriptionId, string? status, TokenModel tokenModel);
        Task LogConsultationEventAsync(int userId, string action, string? consultationId, string? status, TokenModel tokenModel);
        Task LogActionAsync(string entity, string action, string entityId, string description, TokenModel tokenModel);
        
        // Webhook idempotency methods
        Task<bool> IsEventProcessedAsync(string eventId);
        Task MarkEventAsProcessedAsync(string eventId, string eventType, string status);
    }
} 
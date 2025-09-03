using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            ILogger<AuditService> logger)
        {
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<JsonModel> GetAuditLogByIdAsync(int id, TokenModel tokenModel)
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

        public async Task<JsonModel> GetDatabaseAuditTrailAsync(string tableName, string? entityId = null, TokenModel tokenModel = null)
        {
            try
            {
                _logger.LogInformation("Getting database audit trail for table {TableName} by user {TokenUserId}", tableName, tokenModel?.UserID ?? 0);
                
                var auditLogs = await _auditLogRepository.GetDatabaseAuditTrailAsync(tableName, entityId);
                var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);
                
                _logger.LogInformation("Retrieved {Count} database audit records for table {TableName} by user {TokenUserId}", dtos.Count, tableName, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dtos, Message = "Database audit trail retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database audit trail for table {TableName} by user {TokenUserId}", tableName, tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving database audit trail", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetUserDatabaseAuditTrailAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, TokenModel tokenModel = null)
        {
            try
            {
                _logger.LogInformation("Getting user database audit trail for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
                
                var auditLogs = await _auditLogRepository.GetUserDatabaseAuditTrailAsync(userId, fromDate, toDate);
                var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);
                
                _logger.LogInformation("Retrieved {Count} user database audit records for user {UserId} by user {TokenUserId}", dtos.Count, userId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dtos, Message = "User database audit trail retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user database audit trail for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving user database audit trail", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetEntityChangeHistoryAsync(string tableName, string entityId, TokenModel tokenModel = null)
        {
            try
            {
                _logger.LogInformation("Getting entity change history for {TableName} ID {EntityId} by user {TokenUserId}", tableName, entityId, tokenModel?.UserID ?? 0);
                
                var auditLogs = await _auditLogRepository.GetEntityChangeHistoryAsync(tableName, entityId);
                var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);
                
                _logger.LogInformation("Retrieved {Count} change history records for {TableName} ID {EntityId} by user {TokenUserId}", dtos.Count, tableName, entityId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dtos, Message = "Entity change history retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity change history for {TableName} ID {EntityId} by user {TokenUserId}", tableName, entityId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving entity change history", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, TokenModel tokenModel = null)
        {
            try
            {
                _logger.LogInformation("Getting audit statistics by user {TokenUserId}", tokenModel?.UserID ?? 0);
                
                var statistics = await _auditLogRepository.GetAuditStatisticsAsync(fromDate, toDate);
                
                _logger.LogInformation("Retrieved audit statistics by user {TokenUserId}", tokenModel?.UserID ?? 0);
                return new JsonModel { data = statistics, Message = "Audit statistics retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit statistics by user {TokenUserId}", tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving audit statistics", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetRecentDatabaseChangesAsync(int count = 50, TokenModel tokenModel = null)
        {
            try
            {
                _logger.LogInformation("Getting {Count} recent database changes by user {TokenUserId}", count, tokenModel?.UserID ?? 0);
                
                var auditLogs = await _auditLogRepository.GetRecentDatabaseChangesAsync(count);
                var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);
                
                _logger.LogInformation("Retrieved {Count} recent database changes by user {TokenUserId}", dtos.Count, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dtos, Message = "Recent database changes retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent database changes by user {TokenUserId}", tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving recent database changes", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, TokenModel tokenModel = null)
        {
            try
            {
                _logger.LogInformation("Getting audit logs by date range by user {TokenUserId}", tokenModel?.UserID ?? 0);
                
                var auditLogs = await _auditLogRepository.GetByDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);
                
                _logger.LogInformation("Retrieved {Count} audit logs by date range by user {TokenUserId}", dtos.Count, tokenModel?.UserID ?? 0);
                return new JsonModel { data = dtos, Message = "Audit logs by date range retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs by date range by user {TokenUserId}", tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "An error occurred while retrieving audit logs by date range", StatusCode = 500 };
            }
        }
    }
}
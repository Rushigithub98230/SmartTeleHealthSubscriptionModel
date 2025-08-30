using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Tests.Mocks
{
    public class MockAuditService : IAuditService
    {
        private readonly List<AuditLogEntry> _auditLogs;
        private bool _shouldFail;
        private string? _failureReason;

        public MockAuditService(bool shouldFail = false, string failureReason = null)
        {
            _auditLogs = new List<AuditLogEntry>();
            _shouldFail = shouldFail;
            _failureReason = failureReason;
        }

        public async Task<JsonModel> GetAuditLogByIdAsync(Guid id, TokenModel tokenModel)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock audit log retrieval failed",
                    data = null
                };
            }

            var auditLog = _auditLogs.FirstOrDefault(log => log.Id == id);
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock audit log retrieved successfully",
                data = auditLog
            };
        }

        public async Task<JsonModel> CreateAuditLogAsync(CreateAuditLogDto createDto, TokenModel tokenModel)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock audit log creation failed",
                    data = null
                };
            }

            var auditLog = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Action = createDto.Action,
                EntityType = createDto.EntityType,
                EntityId = createDto.EntityId,
                Details = createDto.Description,
                UserId = createDto.UserId,
                Timestamp = DateTime.UtcNow
            };

            _auditLogs.Add(auditLog);

            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock audit log created successfully",
                data = auditLog
            };
        }

        public async Task<JsonModel> GetUserAuditLogsAsync(int userId, TokenModel tokenModel)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock user audit log retrieval failed",
                    data = null
                };
            }

            var userLogs = _auditLogs.Where(log => log.UserId == userId).ToList();
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock user audit logs retrieved successfully",
                data = userLogs
            };
        }

        public async Task<JsonModel> SearchAuditLogsAsync(AuditLogSearchDto searchDto, TokenModel tokenModel)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock audit log search failed",
                    data = null
                };
            }

            var filteredLogs = _auditLogs
                .Where(log => (string.IsNullOrEmpty(searchDto.Action) || log.Action == searchDto.Action) &&
                             (string.IsNullOrEmpty(searchDto.EntityType) || log.EntityType == searchDto.EntityType))
                .ToList();

            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock audit log search completed successfully",
                data = filteredLogs
            };
        }

        public async Task<JsonModel> GetRecentAuditLogsAsync(int count, TokenModel tokenModel)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock recent audit log retrieval failed",
                    data = null
                };
            }

            var recentLogs = _auditLogs.OrderByDescending(log => log.Timestamp).Take(count).ToList();
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock recent audit logs retrieved successfully",
                data = recentLogs
            };
        }

        public async Task<JsonModel> GetAuditLogsAsync(string? action, int? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize, TokenModel tokenModel)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock audit log retrieval failed",
                    data = null
                };
            }

            var filteredLogs = _auditLogs
                .Where(log => (string.IsNullOrEmpty(action) || log.Action == action) &&
                             (!userId.HasValue || log.UserId == userId) &&
                             (!startDate.HasValue || log.Timestamp >= startDate) &&
                             (!endDate.HasValue || log.Timestamp <= endDate))
                .OrderByDescending(log => log.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock audit logs retrieved successfully",
                data = filteredLogs
            };
        }

        public async Task<JsonModel> GetUserAuditLogCountAsync(int userId, TokenModel tokenModel)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock user audit log count retrieval failed",
                    data = null
                };
            }

            var count = _auditLogs.Count(log => log.UserId == userId);
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock user audit log count retrieved successfully",
                data = count
            };
        }

        public async Task LogUserActionAsync(int userId, string action, string entityType, string? entityId, string? description, TokenModel tokenModel)
        {
            var auditLog = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = entityType,
                EntityId = entityId ?? "",
                Details = description ?? "",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            _auditLogs.Add(auditLog);
        }

        public async Task LogDataChangeAsync(int userId, string entityType, string entityId, string? oldValues, string? newValues, TokenModel tokenModel)
        {
            var auditLog = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Action = "DataChange",
                EntityType = entityType,
                EntityId = entityId,
                Details = $"Old: {oldValues ?? "N/A"}, New: {newValues ?? "N/A"}",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            _auditLogs.Add(auditLog);
        }

        public async Task LogSecurityEventAsync(int userId, string action, string? description, string? ipAddress, TokenModel tokenModel)
        {
            var auditLog = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = "Security",
                EntityId = "",
                Details = $"{description ?? ""} IP: {ipAddress ?? "N/A"}",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            _auditLogs.Add(auditLog);
        }

        public async Task LogPaymentEventAsync(int userId, string action, string? entityId, string? status, string? errorMessage, TokenModel tokenModel)
        {
            var auditLog = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = "Payment",
                EntityId = entityId ?? "",
                Details = $"Status: {status ?? "N/A"}, Error: {errorMessage ?? "N/A"}",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            _auditLogs.Add(auditLog);
        }

        public async Task LogSubscriptionEventAsync(int userId, string action, string? subscriptionId, string? status, TokenModel tokenModel)
        {
            var auditLog = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = "Subscription",
                EntityId = subscriptionId ?? "",
                Details = $"Status: {status ?? "N/A"}",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            _auditLogs.Add(auditLog);
        }

        public async Task LogConsultationEventAsync(int userId, string action, string? consultationId, string? status, TokenModel tokenModel)
        {
            var auditLog = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = "Consultation",
                EntityId = consultationId ?? "",
                Details = $"Status: {status ?? "N/A"}",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            _auditLogs.Add(auditLog);
        }

        public async Task LogActionAsync(string entity, string action, string entityId, string description, TokenModel tokenModel)
        {
            var auditLog = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = entity,
                EntityId = entityId,
                Details = description,
                UserId = 0, // System action
                Timestamp = DateTime.UtcNow
            };

            _auditLogs.Add(auditLog);
        }

        // Helper methods for testing
        public List<AuditLogEntry> GetAuditLogs() => new List<AuditLogEntry>(_auditLogs);
        public void ClearAuditLogs() => _auditLogs.Clear();
        public void SetFailureMode(bool shouldFail, string failureReason = null) { _shouldFail = shouldFail; _failureReason = failureReason; }
        public bool HasAuditLog(string action, string entityType, string entityId) => _auditLogs.Any(log => log.Action == action && log.EntityType == entityType && log.EntityId == entityId);
        public int GetAuditLogCount() => _auditLogs.Count;
    }

    public class AuditLogEntry
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Details { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

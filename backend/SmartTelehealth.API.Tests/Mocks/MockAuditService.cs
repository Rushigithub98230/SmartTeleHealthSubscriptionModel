using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Tests.Mocks
{
    public class MockAuditService : IAuditService
    {
        private readonly List<AuditLogDto> _auditLogs;
        private bool _shouldFail;
        private string? _failureReason;

        public MockAuditService(bool shouldFail = false, string? failureReason = null)
        {
            _auditLogs = new List<AuditLogDto>();
            _shouldFail = shouldFail;
            _failureReason = failureReason;
        }

        public async Task<JsonModel> GetAuditLogByIdAsync(int id, TokenModel tokenModel)
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

        public async Task<JsonModel> GetDatabaseAuditTrailAsync(string tableName, string? entityId = null, TokenModel? tokenModel = null)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock database audit trail retrieval failed",
                    data = null
                };
            }

            var filteredLogs = _auditLogs.Where(log => log.TableName == tableName).ToList();
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock database audit trail retrieved successfully",
                data = filteredLogs
            };
        }

        public async Task<JsonModel> GetUserDatabaseAuditTrailAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, TokenModel? tokenModel = null)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock user database audit trail retrieval failed",
                    data = null
                };
            }

            var filteredLogs = _auditLogs.Where(log => log.UserId == userId).ToList();
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock user database audit trail retrieved successfully",
                data = filteredLogs
            };
        }

        public async Task<JsonModel> GetEntityChangeHistoryAsync(string tableName, string entityId, TokenModel? tokenModel = null)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock entity change history retrieval failed",
                    data = null
                };
            }

            var filteredLogs = _auditLogs.Where(log => log.TableName == tableName && log.PrimaryKey == entityId).ToList();
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock entity change history retrieved successfully",
                data = filteredLogs
            };
        }

        public async Task<JsonModel> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, TokenModel? tokenModel = null)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock audit statistics retrieval failed",
                    data = null
                };
            }

            var stats = new
            {
                TotalLogs = _auditLogs.Count,
                UniqueUsers = _auditLogs.Select(log => log.UserId).Distinct().Count(),
                UniqueTables = _auditLogs.Select(log => log.TableName).Distinct().Count()
            };

            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock audit statistics retrieved successfully",
                data = stats
            };
        }

        public async Task<JsonModel> GetRecentDatabaseChangesAsync(int count = 50, TokenModel? tokenModel = null)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock recent database changes retrieval failed",
                    data = null
                };
            }

            var recentLogs = _auditLogs.OrderByDescending(log => log.DateTime).Take(count).ToList();
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock recent database changes retrieved successfully",
                data = recentLogs
            };
        }

        public async Task<JsonModel> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, TokenModel? tokenModel = null)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock audit logs by date range retrieval failed",
                    data = null
                };
            }

            var filteredLogs = _auditLogs.Where(log => log.DateTime >= startDate && log.DateTime <= endDate).ToList();
            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock audit logs by date range retrieved successfully",
                data = filteredLogs
            };
        }

        // Helper methods for testing
        public void AddAuditLog(AuditLogDto auditLog)
        {
            _auditLogs.Add(auditLog);
        }

        public void ClearAuditLogs()
        {
            _auditLogs.Clear();
        }

        public void SetFailureMode(bool shouldFail, string? reason = null)
        {
            _shouldFail = shouldFail;
            _failureReason = reason;
        }
    }
}
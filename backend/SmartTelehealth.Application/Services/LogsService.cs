using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for managing application logs and audit logs.
/// </summary>
public class LogsService : ILogsService
{
    private readonly IApplicationLogRepository _applicationLogRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<LogsService> _logger;

    public LogsService(
        IApplicationLogRepository applicationLogRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper,
        ILogger<LogsService> logger)
    {
        _applicationLogRepository = applicationLogRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<JsonModel> GetApplicationLogsAsync(ApplicationLogFilterDto filter, TokenModel token)
    {
        try
        {
            _logger.LogInformation("Getting application logs with filter by user {UserId}", token?.UserID ?? 0);

            var (logs, totalCount) = await _applicationLogRepository.GetLogsAsync(
                filter.StartDate,
                filter.EndDate,
                filter.LogLevel,
                filter.Source,
                filter.UserId,
                filter.SearchText,
                filter.Page,
                filter.PageSize);

            var dtos = _mapper.Map<List<ApplicationLogDto>>(logs);

            var result = new
            {
                logs = dtos,
                totalCount,
                page = filter.Page,
                pageSize = filter.PageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };

            return new JsonModel
            {
                data = result,
                Message = "Application logs retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application logs by user {UserId}", token?.UserID ?? 0);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve application logs",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAuditLogsAsync(AuditLogFilterDto filter, TokenModel token)
    {
        try
        {
            _logger.LogInformation("Getting audit logs with filter by user {UserId}", token?.UserID ?? 0);

            var auditLogs = await _auditLogRepository.GetByDateRangeAsync(
                filter.StartDate ?? DateTime.MinValue,
                filter.EndDate ?? DateTime.MaxValue);

            var dtos = _mapper.Map<List<AuditLogDto>>(auditLogs);

            var result = new
            {
                logs = dtos,
                totalCount = dtos.Count,
                page = filter.Page,
                pageSize = filter.PageSize,
                totalPages = (int)Math.Ceiling(dtos.Count / (double)filter.PageSize)
            };

            return new JsonModel
            {
                data = result,
                Message = "Audit logs retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs by user {UserId}", token?.UserID ?? 0);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve audit logs",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetCombinedLogsAsync(CombinedLogFilterDto filter, TokenModel token)
    {
        try
        {
            _logger.LogInformation("Getting combined logs with filter by user {UserId}", token?.UserID ?? 0);

            if (filter.LogType == "application")
            {
                var appFilter = new ApplicationLogFilterDto
                {
                    StartDate = filter.StartDate,
                    EndDate = filter.EndDate,
                    LogLevel = filter.LogLevel,
                    Source = filter.Source,
                    UserId = filter.UserId,
                    SearchText = filter.SearchText,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
                return await GetApplicationLogsAsync(appFilter, token);
            }
            else
            {
                var auditFilter = new AuditLogFilterDto
                {
                    StartDate = filter.StartDate,
                    EndDate = filter.EndDate,
                    Type = filter.Type,
                    TableName = filter.TableName,
                    EntityId = filter.EntityId,
                    UserId = filter.UserId,
                    SearchText = filter.SearchText,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
                return await GetAuditLogsAsync(auditFilter, token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting combined logs by user {UserId}", token?.UserID ?? 0);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve combined logs",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetLogByIdAsync(long id, string logType, TokenModel token)
    {
        try
        {
            _logger.LogInformation("Getting log {Id} of type {LogType} by user {UserId}", id, logType, token?.UserID ?? 0);

            if (logType == "application")
            {
                var log = await _applicationLogRepository.GetLogByIdAsync(id);
                if (log == null)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Application log not found",
                        StatusCode = 404
                    };
                }

                var dto = _mapper.Map<ApplicationLogDto>(log);
                return new JsonModel
                {
                    data = dto,
                    Message = "Application log retrieved successfully",
                    StatusCode = 200
                };
            }
            else if (logType == "audit")
            {
                var log = await _auditLogRepository.GetByIdAsync((int)id);
                if (log == null)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Audit log not found",
                        StatusCode = 404
                    };
                }

                var dto = _mapper.Map<AuditLogDto>(log);
                return new JsonModel
                {
                    data = dto,
                    Message = "Audit log retrieved successfully",
                    StatusCode = 200
                };
            }
            else
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invalid log type",
                    StatusCode = 400
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting log {Id} of type {LogType} by user {UserId}", id, logType, token?.UserID ?? 0);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve log",
                StatusCode = 500
            };
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class AuditController : BaseController
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<JsonModel> GetAllAuditLogs(
        [FromQuery] string? action = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var tokenModel = GetToken(HttpContext);
        int? parsedUserId = null;
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int parsedId))
        {
            parsedUserId = parsedId;
        }
        var response = await _auditService.GetAuditLogsAsync(action, parsedUserId, startDate, endDate, page, pageSize, tokenModel);
        return response;
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetAuditLog(Guid id)
    {
        var tokenModel = GetToken(HttpContext);
        var response = await _auditService.GetAuditLogByIdAsync(id, tokenModel);
        return response;
    }
} 
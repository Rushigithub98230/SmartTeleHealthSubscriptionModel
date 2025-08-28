using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OneTimeConsultationController : BaseController
{
    private readonly IConsultationService _consultationService;

    public OneTimeConsultationController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpGet("my-consultations")]
    public async Task<JsonModel> GetMyOneTimeConsultations()
    {
        var userId = GetCurrentUserId();
        return await _consultationService.GetUserOneTimeConsultationsAsync(userId, GetToken(HttpContext));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
} 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ConsultationsController : BaseController
{
    private readonly IConsultationService _consultationService;

    public ConsultationsController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpGet]
    public async Task<JsonModel> GetUserConsultations()
    {
        var userId = GetCurrentUserId();
        return await _consultationService.GetUserConsultationsAsync(userId, GetToken(HttpContext));
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetConsultation(Guid id)
    {
        return await _consultationService.GetConsultationByIdAsync(id, GetToken(HttpContext));
    }

    [HttpPost]
    public async Task<JsonModel> CreateConsultation(CreateConsultationDto createDto)
    {
        return await _consultationService.CreateConsultationAsync(createDto, GetToken(HttpContext));
    }

    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateConsultation(Guid id, UpdateConsultationDto updateDto)
    {
        return await _consultationService.UpdateConsultationAsync(id, updateDto, GetToken(HttpContext));
    }

    [HttpPost("{id}/cancel")]
    public async Task<JsonModel> CancelConsultation(Guid id, [FromBody] string reason)
    {
        return await _consultationService.CancelConsultationAsync(id, reason, GetToken(HttpContext));
    }

    [HttpPost("{id}/start")]
    public async Task<JsonModel> StartConsultation(Guid id)
    {
        return await _consultationService.StartConsultationAsync(id, GetToken(HttpContext));
    }

    [HttpPost("{id}/complete")]
    public async Task<JsonModel> CompleteConsultation(Guid id, [FromBody] string notes)
    {
        return await _consultationService.CompleteConsultationAsync(id, notes, GetToken(HttpContext));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
} 
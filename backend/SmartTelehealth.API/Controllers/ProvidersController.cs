using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProvidersController : BaseController
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    public async Task<JsonModel> GetAllProviders()
    {
        return await _providerService.GetAllProvidersAsync(GetToken(HttpContext));
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetProvider(int id)
    {
        return await _providerService.GetProviderByIdAsync(id, GetToken(HttpContext));
    }

    [HttpPost]
    public async Task<JsonModel> CreateProvider([FromBody] CreateProviderDto createProviderDto)
    {
        return await _providerService.CreateProviderAsync(createProviderDto, GetToken(HttpContext));
    }

    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateProvider(int id, [FromBody] UpdateProviderDto updateProviderDto)
    {
        if (id != updateProviderDto.Id)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        return await _providerService.UpdateProviderAsync(id, updateProviderDto, GetToken(HttpContext));
    }

    [HttpDelete("{id}")]
    public async Task<JsonModel> DeleteProvider(int id)
    {
        return await _providerService.DeleteProviderAsync(id, GetToken(HttpContext));
    }
} 
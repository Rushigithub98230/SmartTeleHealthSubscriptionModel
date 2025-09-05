using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for Infermedica AI-powered medical diagnosis and doctor recommendation integration.
/// This controller provides comprehensive functionality for medical text parsing, AI-powered diagnosis,
/// specialist recommendations, and provider matching based on medical conditions and symptoms.
/// It integrates with Infermedica's medical AI services for enhanced healthcare decision support.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class InfermedicaController : BaseController
{
    private readonly IInfermedicaService _infermedicaService;
    private readonly IProviderRepository _providerRepository;

    /// <summary>
    /// Initializes a new instance of the InfermedicaController with required services.
    /// </summary>
    /// <param name="infermedicaService">Service for Infermedica AI integration</param>
    /// <param name="providerRepository">Repository for provider data access</param>
    public InfermedicaController(IInfermedicaService infermedicaService, IProviderRepository providerRepository)
    {
        _infermedicaService = infermedicaService;
        _providerRepository = providerRepository;
    }

    /// <summary>
    /// Parses medical text using Infermedica AI to extract medical entities and symptoms.
    /// This endpoint processes unstructured medical text and extracts structured medical information
    /// including symptoms, conditions, and medical entities for further analysis and diagnosis.
    /// </summary>
    /// <param name="text">The medical text to parse</param>
    /// <returns>JsonModel containing parsed medical entities and extracted information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Parses medical text using Infermedica AI
    /// - Extracts medical entities, symptoms, and conditions
    /// - Returns structured medical information
    /// - Access restricted to authenticated users
    /// - Used for medical text analysis and entity extraction
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on parsing operations
    /// - Maintains medical text processing audit trails
    /// </remarks>
    [HttpPost("parse")]
    public async Task<JsonModel> Parse([FromBody] string text)
    {
        return await _infermedicaService.ParseAsync(text);
    }

    /// <summary>
    /// Performs AI-powered medical diagnosis using Infermedica services.
    /// This endpoint analyzes medical symptoms and conditions to provide diagnostic suggestions
    /// and medical recommendations based on AI-powered medical analysis.
    /// </summary>
    /// <param name="request">DTO containing diagnosis request with symptoms and medical information</param>
    /// <returns>JsonModel containing diagnostic results and medical recommendations</returns>
    /// <remarks>
    /// This endpoint:
    /// - Performs AI-powered medical diagnosis
    /// - Analyzes symptoms and medical conditions
    /// - Provides diagnostic suggestions and recommendations
    /// - Access restricted to authenticated users
    /// - Used for medical diagnosis and decision support
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed diagnostic feedback
    /// - Maintains medical diagnosis audit trails
    /// </remarks>
    [HttpPost("diagnose")]
    public async Task<JsonModel> Diagnose([FromBody] InfermedicaDiagnosisRequestDto request)
    {
        return await _infermedicaService.DiagnoseAsync(request);
    }

    /// <summary>
    /// Recommends healthcare providers based on AI-powered medical analysis.
    /// This endpoint analyzes medical conditions and symptoms to suggest appropriate specialists
    /// and matches them with available healthcare providers in the system.
    /// </summary>
    /// <param name="request">DTO containing diagnosis request for specialist recommendation</param>
    /// <returns>JsonModel containing recommended healthcare providers and specialist information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Recommends healthcare providers based on medical analysis
    /// - Matches medical conditions with appropriate specialists
    /// - Returns provider information and contact details
    /// - Access restricted to authenticated users
    /// - Used for provider recommendation and medical matching
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed provider recommendations
    /// - Maintains provider recommendation audit trails
    /// </remarks>
    [HttpPost("recommend-doctors")]
    public async Task<JsonModel> RecommendDoctors([FromBody] InfermedicaDiagnosisRequestDto request)
    {
        var specialistResult = await _infermedicaService.SuggestSpecialistAsync(request);
        if (specialistResult.StatusCode != 200)
            return specialistResult;
        
        // Extract specialty from the result and get providers
        var specialty = specialistResult.data?.ToString() ?? "";
        if (string.IsNullOrEmpty(specialty))
            return new JsonModel { data = new List<object>(), Message = "No specialists found", StatusCode = 200 };
        
        var providers = await _providerRepository.GetProvidersBySpecialtyAsync(specialty);
        var providerData = providers.Select(p => new { p.Id, p.FullName, p.Specialty, p.Email, p.PhoneNumber });
        
        return new JsonModel { data = providerData, Message = "Doctors recommended successfully", StatusCode = 200 };
    }
} 
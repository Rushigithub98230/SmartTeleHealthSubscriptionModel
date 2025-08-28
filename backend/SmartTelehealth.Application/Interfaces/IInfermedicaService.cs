using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IInfermedicaService
{
    Task<JsonModel> ParseAsync(string text);
    Task<JsonModel> DiagnoseAsync(InfermedicaDiagnosisRequestDto request);
    Task<JsonModel> SuggestSpecialistAsync(InfermedicaDiagnosisRequestDto request);
} 
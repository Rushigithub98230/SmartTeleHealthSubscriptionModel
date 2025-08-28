using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Infrastructure.Services;

public class InfermedicaService : IInfermedicaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InfermedicaService> _logger;
    private readonly string _appId;
    private readonly string _appKey;
    private readonly string _baseUrl;

    public InfermedicaService(IConfiguration config, ILogger<InfermedicaService> logger)
    {
        _logger = logger;
        _appId = config["Infermedica:AppId"] ?? throw new InvalidOperationException("Infermedica AppId missing");
        _appKey = config["Infermedica:AppKey"] ?? throw new InvalidOperationException("Infermedica AppKey missing");
        _baseUrl = config["Infermedica:BaseUrl"] ?? "https://api.infermedica.com/v3";
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("App-Id", _appId);
        _httpClient.DefaultRequestHeaders.Add("App-Key", _appKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<JsonModel> ParseAsync(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Text input is required",
                    StatusCode = 400
                };
            }

            var payload = JsonSerializer.Serialize(new { text });
            var response = await _httpClient.PostAsync(_baseUrl + "/parse", new StringContent(payload, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<InfermedicaParseResponseDto>(json) ?? new();

            return new JsonModel
            {
                data = result,
                Message = "Text parsed successfully",
                StatusCode = 200
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during text parsing");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to connect to Infermedica service",
                StatusCode = 503
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error during text parsing");
            return new JsonModel
            {
                data = new object(),
                Message = "Invalid response from Infermedica service",
                StatusCode = 500
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during text parsing");
            return new JsonModel
            {
                data = new object(),
                Message = "An unexpected error occurred during text parsing",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DiagnoseAsync(InfermedicaDiagnosisRequestDto request)
    {
        try
        {
            if (request == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Diagnosis request is required",
                    StatusCode = 400
                };
            }

            var payload = JsonSerializer.Serialize(request);
            var response = await _httpClient.PostAsync(_baseUrl + "/diagnosis", new StringContent(payload, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<InfermedicaDiagnosisResponseDto>(json) ?? new();

            return new JsonModel
            {
                data = result,
                Message = "Diagnosis completed successfully",
                StatusCode = 200
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during diagnosis");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to connect to Infermedica service",
                StatusCode = 503
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error during diagnosis");
            return new JsonModel
            {
                data = new object(),
                Message = "Invalid response from Infermedica service",
                StatusCode = 500
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during diagnosis");
            return new JsonModel
            {
                data = new object(),
                Message = "An unexpected error occurred during diagnosis",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SuggestSpecialistAsync(InfermedicaDiagnosisRequestDto request)
    {
        try
        {
            if (request == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Diagnosis request is required",
                    StatusCode = 400
                };
            }

            var payload = JsonSerializer.Serialize(request);
            var response = await _httpClient.PostAsync(_baseUrl + "/suggest-specialist", new StringContent(payload, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<InfermedicaSuggestSpecialistResponseDto>(json) ?? new();

            if (result.Specialties == null || !result.Specialties.Any())
            {
                return new JsonModel
                {
                    data = new List<object>(),
                    Message = "No specialists found",
                    StatusCode = 200
                };
            }

            return new JsonModel
            {
                data = result.Specialties.FirstOrDefault()?.Name ?? "",
                Message = "Specialist suggestion completed successfully",
                StatusCode = 200
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during specialist suggestion");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to connect to Infermedica service",
                StatusCode = 503
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error during specialist suggestion");
            return new JsonModel
            {
                data = new object(),
                Message = "Invalid response from Infermedica service",
                StatusCode = 500
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during specialist suggestion");
            return new JsonModel
            {
                data = new object(),
                Message = "An unexpected error occurred during specialist suggestion",
                StatusCode = 500
            };
        }
    }
} 
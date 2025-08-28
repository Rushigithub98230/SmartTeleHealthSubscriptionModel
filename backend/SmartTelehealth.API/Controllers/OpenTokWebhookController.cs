using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Text.Json;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenTokWebhookController : BaseController
{
    private readonly IOpenTokService _openTokService;

    public OpenTokWebhookController(IOpenTokService openTokService)
    {
        _openTokService = openTokService;
    }

    [HttpPost("webhook")]
    public async Task<JsonModel> HandleWebhook()
    {
        // Read the webhook payload
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        // Parse the webhook data
        var webhookData = JsonSerializer.Deserialize<Dictionary<string, object>>(payload);
        
        if (webhookData == null)
        {
            return new JsonModel { data = new object(), Message = "Invalid webhook payload", StatusCode = 400 };
        }

        // Extract webhook information
        var webhook = new OpenTokWebhookDto
        {
            EventType = webhookData.ContainsKey("eventType") ? webhookData["eventType"].ToString() : string.Empty,
            SessionId = webhookData.ContainsKey("sessionId") ? webhookData["sessionId"].ToString() : string.Empty,
            ConnectionId = webhookData.ContainsKey("connectionId") ? webhookData["connectionId"]?.ToString() : null,
            StreamId = webhookData.ContainsKey("streamId") ? webhookData["streamId"]?.ToString() : null,
            RecordingId = webhookData.ContainsKey("recordingId") ? webhookData["recordingId"]?.ToString() : null,
            BroadcastId = webhookData.ContainsKey("broadcastId") ? webhookData["broadcastId"]?.ToString() : null,
            Timestamp = DateTime.UtcNow,
            Data = webhookData
        };

        // Process the webhook
        var result = await _openTokService.HandleWebhookAsync(webhook, GetToken(HttpContext));

        if (result.StatusCode == 200)
        {
            return new JsonModel { data = new object(), Message = "Webhook processed successfully", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = result.Message, StatusCode = 500 };
        }
    }

    [HttpGet("health")]
    public async Task<JsonModel> HealthCheck()
    {
        var result = await _openTokService.IsServiceHealthyAsync(GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            return new JsonModel { data = new object(), Message = "OpenTok service is operational", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = "OpenTok service is not operational", StatusCode = 503 };
        }
    }

    [HttpPost("session/{sessionId}/token")]
    public async Task<JsonModel> GenerateToken(string sessionId, [FromBody] TokenRequest request)
    {
        if (request.UserId <= 0 || string.IsNullOrEmpty(request.UserName))
        {
            return new JsonModel { data = new object(), Message = "UserId and UserName are required", StatusCode = 400 };
        }

        var result = await _openTokService.GenerateTokenAsync(sessionId, request.UserId.ToString(), request.UserName, request.Role, GetToken(HttpContext));

        if (result.StatusCode == 200)
        {
            return new JsonModel { data = new { token = result.data }, Message = "Token generated successfully", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 };
        }
    }

    [HttpPost("session")]
    public async Task<JsonModel> CreateSession([FromBody] CreateSessionRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionName))
        {
            return new JsonModel { data = new object(), Message = "SessionName is required", StatusCode = 400 };
        }

        var result = await _openTokService.CreateSessionAsync(request.SessionName, request.IsArchived, GetToken(HttpContext));

        if (result.StatusCode == 200)
        {
            return new JsonModel { data = result.data, Message = "Session created successfully", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 };
        }
    }

    [HttpPost("session/{sessionId}/recording")]
    public async Task<JsonModel> StartRecording(string sessionId, [FromBody] StartRecordingRequest request)
    {
        var options = new OpenTokRecordingOptions
        {
            Name = request.Name ?? $"Recording_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            HasAudio = request.HasAudio,
            HasVideo = request.HasVideo,
            OutputMode = request.OutputMode,
            Resolution = request.Resolution,
            Layout = request.Layout,
            MaxDuration = request.MaxDuration,
            Storage = request.Storage
        };

        var result = await _openTokService.StartRecordingAsync(sessionId, options, GetToken(HttpContext));

        if (result.StatusCode == 200)
        {
            return new JsonModel { data = result.data, Message = "Recording started successfully", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 };
        }
    }

    [HttpDelete("recording/{recordingId}")]
    public async Task<JsonModel> StopRecording(string recordingId)
    {
        var result = await _openTokService.StopRecordingAsync(recordingId, GetToken(HttpContext));

        if (result.StatusCode == 200)
        {
            return new JsonModel { data = new object(), Message = "Recording stopped successfully", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 };
        }
    }

    [HttpGet("recording/{recordingId}/url")]
    public async Task<JsonModel> GetRecordingUrl(string recordingId)
    {
        var result = await _openTokService.GetRecordingUrlAsync(recordingId, GetToken(HttpContext));

        if (result.StatusCode == 200)
        {
            return new JsonModel { data = new { url = result.data }, Message = "Recording URL retrieved successfully", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 };
        }
    }
}

public class TokenRequest
{
            public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public OpenTokRole Role { get; set; } = OpenTokRole.Publisher;
}

public class CreateSessionRequest
{
    public string SessionName { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = true;
}

public class StartRecordingRequest
{
    public string? Name { get; set; }
    public bool HasAudio { get; set; } = true;
    public bool HasVideo { get; set; } = true;
    public OpenTokRecordingOutputMode OutputMode { get; set; } = OpenTokRecordingOutputMode.Composed;
    public string? Resolution { get; set; } = "1280x720";
    public string? Layout { get; set; }
    public int? MaxDuration { get; set; }
    public string? Storage { get; set; } = "cloud";
} 
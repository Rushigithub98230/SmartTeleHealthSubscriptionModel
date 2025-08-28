using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace SmartTelehealth.API.Middleware;

public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;

    public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Validate and sanitize request
            if (!await ValidateRequest(context))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid request data");
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in input validation middleware");
            throw;
        }
    }

    private async Task<bool> ValidateRequest(HttpContext context)
    {
        // Skip validation for certain endpoints
        if (ShouldSkipValidation(context.Request.Path))
        {
            return true;
        }

        // Validate Content-Type for POST/PUT requests
        if (context.Request.Method == "POST" || context.Request.Method == "PUT")
        {
            var contentType = context.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) || !contentType.Contains("application/json"))
            {
                _logger.LogWarning("Invalid Content-Type: {ContentType}", contentType);
                return false;
            }
        }

        // Validate request size
        if (context.Request.ContentLength > 10 * 1024 * 1024) // 10MB limit
        {
            _logger.LogWarning("Request too large: {Size} bytes", context.Request.ContentLength);
            return false;
        }

        // Validate headers
        if (!ValidateHeaders(context.Request.Headers))
        {
            return false;
        }

        return true;
    }

    private bool ShouldSkipValidation(PathString path)
    {
        var skipPaths = new[]
        {
            "/api/health",
            "/api/webhook",
            "/swagger",
            "/favicon.ico"
        };

        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath));
    }

    private bool ValidateHeaders(IHeaderDictionary headers)
    {
        // Check for suspicious headers
        var suspiciousHeaders = new[]
        {
            "X-Forwarded-For",
            "X-Real-IP",
            "X-Forwarded-Proto"
        };

        foreach (var header in suspiciousHeaders)
        {
            if (headers.ContainsKey(header))
            {
                var value = headers[header].ToString();
                if (!IsValidIpAddress(value))
                {
                    _logger.LogWarning("Suspicious header value: {Header} = {Value}", header, value);
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsValidIpAddress(string ip)
    {
        if (string.IsNullOrEmpty(ip))
            return false;

        // Basic IP validation
        var ipPattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        return Regex.IsMatch(ip, ipPattern);
    }
}

public static class InputValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseInputValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<InputValidationMiddleware>();
    }
} 
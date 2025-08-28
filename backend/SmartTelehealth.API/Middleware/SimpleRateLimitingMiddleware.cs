using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace SmartTelehealth.API.Middleware;

public class SimpleRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SimpleRateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore = new();

    public SimpleRateLimitingMiddleware(RequestDelegate next, ILogger<SimpleRateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var endpoint = context.Request.Path.Value;

        if (IsRateLimited(clientId, endpoint))
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", clientId, endpoint);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Use IP address as primary identifier
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // For authenticated users, include user ID
        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"{ip}:{userId}";
        }

        return ip;
    }

    private bool IsRateLimited(string clientId, string endpoint)
    {
        var now = DateTime.UtcNow;
        var key = $"{clientId}:{endpoint}";

        // Clean up old entries
        CleanupOldEntries();

        var rateLimit = GetRateLimitForEndpoint(endpoint);
        
        if (_rateLimitStore.TryGetValue(key, out var info))
        {
            if (now - info.FirstRequest < rateLimit.Window)
            {
                if (info.RequestCount >= rateLimit.MaxRequests)
                {
                    return true;
                }
                info.RequestCount++;
            }
            else
            {
                // Reset window
                info.FirstRequest = now;
                info.RequestCount = 1;
            }
        }
        else
        {
            _rateLimitStore.TryAdd(key, new RateLimitInfo
            {
                FirstRequest = now,
                RequestCount = 1
            });
        }

        return false;
    }

    private (int MaxRequests, TimeSpan Window) GetRateLimitForEndpoint(string endpoint)
    {
        // Define rate limits for different endpoints
        if (endpoint?.Contains("/auth/") == true)
        {
            return (5, TimeSpan.FromMinutes(1)); // 5 requests per minute for auth
        }
        
        if (endpoint?.Contains("/webhook") == true)
        {
            return (50, TimeSpan.FromMinutes(1)); // 50 requests per minute for webhooks
        }

        return (100, TimeSpan.FromMinutes(1)); // Default: 100 requests per minute
    }

    private void CleanupOldEntries()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-5);
        var keysToRemove = _rateLimitStore
            .Where(kvp => kvp.Value.FirstRequest < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _rateLimitStore.TryRemove(key, out _);
        }
    }

    private class RateLimitInfo
    {
        public DateTime FirstRequest { get; set; }
        public int RequestCount { get; set; }
    }
}

public static class SimpleRateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseSimpleRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SimpleRateLimitingMiddleware>();
    }
} 
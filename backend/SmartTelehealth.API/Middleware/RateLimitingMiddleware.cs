using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace SmartTelehealth.API.Middleware;

public static class RateLimitingMiddleware
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Rate limiter for authentication endpoints
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Rate limiter for API endpoints
            options.AddPolicy("api", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 1000,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Rate limiter for webhook endpoints
            options.AddPolicy("webhook", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 50,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
            };
        });

        return services;
    }
} 
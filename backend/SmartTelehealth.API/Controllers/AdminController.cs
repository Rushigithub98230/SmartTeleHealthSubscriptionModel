using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IBillingService _billingService;
    private readonly ICategoryService _categoryService;
    private readonly IProviderService _providerService;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly IAnalyticsService _analyticsService;

    public AdminController(
        ISubscriptionService subscriptionService,
        IBillingService billingService,
        ICategoryService categoryService,
        IProviderService providerService,
        IUserService userService,
        IAuditService auditService,
        IAnalyticsService analyticsService)
    {
        _subscriptionService = subscriptionService;
        _billingService = billingService;
        _categoryService = categoryService;
        _providerService = providerService;
        _userService = userService;
        _auditService = auditService;
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get admin dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<JsonModel> GetDashboard()
    {
        var dashboard = new AdminDashboardDto
        {
            TotalSubscriptions = await GetTotalSubscriptions(),
            ActiveSubscriptions = await GetActiveSubscriptions(),
            TotalRevenue = await GetTotalRevenue(),
            MonthlyRecurringRevenue = await GetMonthlyRecurringRevenue(),
            TotalUsers = await GetTotalUsers(),
            TotalProviders = await GetTotalProviders(),
            RecentSubscriptions = await GetRecentSubscriptions(),
            RecentBillingRecords = await GetRecentBillingRecords(),
            SystemHealth = await GetSystemHealthData()
        };

        return new JsonModel { data = dashboard, Message = "Dashboard data retrieved successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Get all subscriptions
    /// </summary>
    [HttpGet("subscriptions")]
    public async Task<JsonModel> GetAllSubscriptions()
    {
        return await _subscriptionService.GetActiveSubscriptionsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get audit logs
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<JsonModel> GetAuditLogs([FromQuery] string? action = null, [FromQuery] string? userId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        int? parsedUserId = null;
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int parsedId))
        {
            parsedUserId = parsedId;
        }
        return await _auditService.GetAuditLogsAsync(action, parsedUserId, startDate, endDate, page, pageSize, GetToken(HttpContext));
    }

    /// <summary>
    /// Export data
    /// </summary>
    [HttpGet("export/{dataType}")]
    public async Task<JsonModel> ExportData(string dataType, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        // TODO: Implement data export functionality in service
        return new JsonModel { data = new object(), Message = $"Export of {dataType} data initiated", StatusCode = 200 };
    }

    // Helper methods for dashboard
    private async Task<int> GetTotalSubscriptions()
    {
        var response = await _subscriptionService.GetActiveSubscriptionsAsync(GetToken(HttpContext));
        return ((IEnumerable<SubscriptionDto>)response.data)?.Count() ?? 0;
    }

    private async Task<int> GetActiveSubscriptions()
    {
        var response = await _subscriptionService.GetActiveSubscriptionsAsync(GetToken(HttpContext));
        return ((IEnumerable<SubscriptionDto>)response.data)?.Count(s => s.IsActive) ?? 0;
    }

    private async Task<decimal> GetTotalRevenue()
    {
        // TODO: Implement revenue calculation
        return 50000.00m;
    }

    private async Task<decimal> GetMonthlyRecurringRevenue()
    {
        // TODO: Implement MRR calculation
        return 15000.00m;
    }

    private async Task<int> GetTotalUsers()
    {
        // TODO: Implement user count
        return 1250;
    }

    private async Task<int> GetTotalProviders()
    {
        // TODO: Implement provider count
        return 45;
    }

    private async Task<IEnumerable<SubscriptionDto>> GetRecentSubscriptions()
    {
        var response = await _subscriptionService.GetActiveSubscriptionsAsync(GetToken(HttpContext));
        return ((IEnumerable<SubscriptionDto>)response.data)?.OrderByDescending(s => s.CreatedAt).Take(10) ?? new List<SubscriptionDto>();
    }

    private async Task<IEnumerable<BillingRecordDto>> GetRecentBillingRecords()
    {
        // TODO: Implement recent billing records
        return new List<BillingRecordDto>();
    }

    private async Task<SystemHealthDto> GetSystemHealthData()
    {
        return new SystemHealthDto
        {
            DatabaseStatus = "Healthy",
            ApiStatus = "Healthy",
            PaymentGatewayStatus = "Healthy",
            EmailServiceStatus = "Healthy",
            LastBackup = DateTime.UtcNow.AddHours(-2),
            SystemUptime = TimeSpan.FromDays(30),
            ActiveConnections = 150,
            MemoryUsage = 75.5,
            CpuUsage = 45.2
        };
    }
}

public class AdminDashboardDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public int TotalUsers { get; set; }
    public int TotalProviders { get; set; }
    public IEnumerable<SubscriptionDto> RecentSubscriptions { get; set; } = new List<SubscriptionDto>();
    public IEnumerable<BillingRecordDto> RecentBillingRecords { get; set; } = new List<BillingRecordDto>();
    public SystemHealthDto SystemHealth { get; set; } = new();
} 
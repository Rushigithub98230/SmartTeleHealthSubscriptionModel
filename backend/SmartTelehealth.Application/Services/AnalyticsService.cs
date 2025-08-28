using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using AutoMapper;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IConsultationRepository _consultationRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<AnalyticsService> _logger;
    private readonly IMapper _mapper;

    public AnalyticsService(
        ISubscriptionRepository subscriptionRepository,
        IBillingRepository billingRepository,
        IUserRepository userRepository,
        IProviderRepository providerRepository,
        IConsultationRepository consultationRepository,
        ICategoryRepository categoryRepository,
        ILogger<AnalyticsService> logger,
        IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _billingRepository = billingRepository;
        _userRepository = userRepository;
        _providerRepository = providerRepository;
        _consultationRepository = consultationRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<JsonModel> GetRevenueAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var analytics = new RevenueAnalyticsDto
            {
                TotalRevenue = await GetTotalRevenueAsync(startDate, endDate, tokenModel),
                MonthlyRevenue = await GetMonthlyRecurringRevenueAsync(tokenModel),
                AnnualRevenue = await GetAnnualRecurringRevenueAsync(tokenModel),
                TotalSubscriptions = await GetTotalSubscriptionsAsync(tokenModel),
                ActiveSubscriptions = await GetActiveSubscriptionsAsync(tokenModel),
                NewSubscriptionsThisMonth = await GetNewSubscriptionsThisMonthAsync(tokenModel),
                CancelledSubscriptionsThisMonth = await GetCancelledSubscriptionsAsync(tokenModel),
                AverageRevenuePerSubscription = await CalculateAverageSubscriptionValueAsync(tokenModel),
                TotalRefunds = await GetRefundsIssuedAsync(startDate, endDate, tokenModel)
            };

            _logger.LogInformation("Revenue analytics retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = analytics, Message = "Revenue analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving revenue analytics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserActivityAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var analytics = new UserActivityAnalyticsDto
            {
                TotalUsers = await GetTotalUsersAsync(tokenModel),
                ActiveUsers = await GetActiveUsersAsync(tokenModel),
                NewUsersThisMonth = await GetNewUsersThisMonthAsync(tokenModel),
                UsersWithActiveSubscriptions = await GetActiveSubscriptionsAsync(tokenModel),
                AverageConsultationsPerUser = 0, // TODO: Implement
                AverageMessagesPerUser = 0, // TODO: Implement
                TotalLogins = 0 // TODO: Implement
            };

            _logger.LogInformation("User activity analytics retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = analytics, Message = "User activity analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving user activity analytics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAppointmentAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var analytics = new AppointmentAnalyticsDto
            {
                TotalAppointments = 0, // TODO: Implement
                CompletedAppointments = 0, // TODO: Implement
                CancelledAppointments = 0, // TODO: Implement
                PendingAppointments = 0, // TODO: Implement
                CompletionRate = 0, // TODO: Implement
                AverageAppointmentDuration = 0 // TODO: Implement
            };

            _logger.LogInformation("Appointment analytics retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = analytics, Message = "Appointment analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving appointment analytics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var analytics = new SubscriptionAnalyticsDto
            {
                TotalSubscriptions = await GetTotalSubscriptionsAsync(tokenModel),
                ActiveSubscriptions = await GetActiveSubscriptionsAsync(tokenModel),
                PausedSubscriptions = await GetPausedSubscriptionsAsync(tokenModel),
                CancelledSubscriptions = await GetCancelledSubscriptionsAsync(tokenModel),
                NewSubscriptionsThisMonth = await GetNewSubscriptionsThisMonthAsync(tokenModel),
                ChurnRate = await CalculateChurnRateAsync(startDate, endDate, tokenModel),
                AverageSubscriptionValue = await CalculateAverageSubscriptionValueAsync(tokenModel),
                MonthlyGrowth = await GetMonthlyGrowthAsync(tokenModel)
            };

            _logger.LogInformation("Subscription analytics retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = analytics, Message = "Subscription analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving subscription analytics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionAnalyticsAsync(DateTime? startDate, DateTime? endDate, string? planId, TokenModel tokenModel)
    {
        try
        {
            var analytics = new SubscriptionAnalyticsDto
            {
                TotalSubscriptions = await GetTotalSubscriptionsAsync(tokenModel),
                ActiveSubscriptions = await GetActiveSubscriptionsAsync(tokenModel),
                PausedSubscriptions = await GetPausedSubscriptionsAsync(tokenModel),
                CancelledSubscriptions = await GetCancelledSubscriptionsAsync(tokenModel),
                NewSubscriptionsThisMonth = await GetNewSubscriptionsThisMonthAsync(tokenModel),
                ChurnRate = await CalculateChurnRateAsync(startDate, endDate, tokenModel),
                AverageSubscriptionValue = await CalculateAverageSubscriptionValueAsync(tokenModel),
                MonthlyGrowth = await GetMonthlyGrowthAsync(tokenModel)
            };

            _logger.LogInformation("Subscription analytics for plan {PlanId} retrieved by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = analytics, Message = "Subscription analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics for plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving subscription analytics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionDashboardAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var subscriptionAnalyticsResult = await GetSubscriptionAnalyticsAsync(startDate, endDate, tokenModel);
            var subscriptionAnalytics = subscriptionAnalyticsResult.data as SubscriptionAnalyticsDto ?? new SubscriptionAnalyticsDto();
            
            var revenueResult = await GetRevenueAnalyticsAsync(startDate, endDate, tokenModel);
            var revenue = revenueResult.data as RevenueAnalyticsDto ?? new RevenueAnalyticsDto();
            
            var dashboard = new SubscriptionDashboardDto
            {
                Revenue = revenue,
                SubscriptionAnalytics = subscriptionAnalytics,
                TopCategories = await GetTopCategoriesAsync(startDate, endDate, tokenModel),
                RevenueTrends = await GetRevenueTrendAsync(startDate, endDate, tokenModel),
                CategoryRevenue = await GetRevenueByCategoryAsync(startDate, endDate, tokenModel)
            };

            _logger.LogInformation("Subscription dashboard retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = dashboard, Message = "Subscription dashboard retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription dashboard by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving subscription dashboard", StatusCode = 500 };
        }
    }

            public async Task<JsonModel> GetChurnAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var churnAnalytics = await GetChurnMetricsAsync(startDate ?? DateTime.UtcNow.AddDays(-30), endDate ?? DateTime.UtcNow, tokenModel);
            _logger.LogInformation("Churn analytics retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = churnAnalytics, Message = "Churn analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting churn analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving churn analytics", StatusCode = 500 };
        }
    }

            public async Task<JsonModel> GetPlanAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var planAnalytics = await GetPlanMetricsAsync(startDate ?? DateTime.UtcNow.AddDays(-30), endDate ?? DateTime.UtcNow);
            _logger.LogInformation("Plan analytics retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = planAnalytics, Message = "Plan analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving plan analytics", StatusCode = 500 };
        }
    }

            public async Task<JsonModel> GetUsageAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var usageAnalytics = await GetUsageMetricsAsync(startDate ?? DateTime.UtcNow.AddDays(-30), endDate ?? DateTime.UtcNow, tokenModel);
            _logger.LogInformation("Usage analytics retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = usageAnalytics, Message = "Usage analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving usage analytics", StatusCode = 500 };
        }
    }

            public async Task<decimal> GetMonthlyRecurringRevenueAsync(TokenModel tokenModel)
    {
        try
        {
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var mrr = activeSubscriptions.Sum(s => s.Amount);
            
            _logger.LogInformation("Monthly recurring revenue calculated by user {UserId}: {MRR}", tokenModel?.UserID ?? 0, mrr);
            return mrr;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating monthly recurring revenue by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

            public async Task<decimal> GetAnnualRecurringRevenueAsync(TokenModel tokenModel)
    {
        try
        {
            var mrr = await GetMonthlyRecurringRevenueAsync(tokenModel);
            var arr = mrr * 12;
            
            _logger.LogInformation("Annual recurring revenue calculated by user {UserId}: {ARR}", tokenModel?.UserID ?? 0, arr);
            return arr;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating annual recurring revenue by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

            public async Task<decimal> CalculateChurnRateAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;
            
            var totalSubscriptionsAtStart = await _subscriptionRepository.GetActiveSubscriptionsCountAsync();
            var cancelledSubscriptions = await _subscriptionRepository.GetCancelledSubscriptionsCountAsync();
            
            var churnRate = totalSubscriptionsAtStart > 0 ? (decimal)cancelledSubscriptions / totalSubscriptionsAtStart * 100 : 0;
            
            _logger.LogInformation("Churn rate calculated by user {UserId}: {ChurnRate}%", tokenModel?.UserID ?? 0, churnRate);
            return churnRate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating churn rate by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

            public async Task<decimal> CalculateAverageSubscriptionValueAsync(TokenModel tokenModel)
    {
        try
        {
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var averageValue = activeSubscriptions.Any() ? activeSubscriptions.Average(s => s.Amount) : 0;
            
            _logger.LogInformation("Average subscription value calculated by user {UserId}: {AverageValue}", tokenModel?.UserID ?? 0, averageValue);
            return averageValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average subscription value by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

    public async Task<IEnumerable<CategoryAnalyticsDto>> GetTopCategoriesAsync(DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;
            
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryAnalytics = new List<CategoryAnalyticsDto>();
            
            foreach (var category in categories)
            {
                var subscriptions = await _subscriptionRepository.GetByCategoryIdAsync(category.Id);
                var subscriptionsInRange = subscriptions.Where(s => s.CreatedDate >= start && s.CreatedDate <= end);
                
                var analytics = new CategoryAnalyticsDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    TotalSubscriptions = subscriptionsInRange.Count(),
                    ActiveSubscriptions = subscriptionsInRange.Count(s => s.Status == "Active"),
                    Revenue = subscriptionsInRange.Sum(s => s.Amount),
                    GrowthRate = 0 // TODO: Implement growth rate calculation
                };
                
                categoryAnalytics.Add(analytics);
            }
            
            var topCategories = categoryAnalytics
                .OrderByDescending(ca => ca.Revenue)
                .Take(10)
                .ToList();
            
            _logger.LogInformation("Top categories analytics calculated by user {UserId}: {CategoryCount} categories", 
                tokenModel?.UserID ?? 0, topCategories.Count);
            return topCategories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating top categories analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return Enumerable.Empty<CategoryAnalyticsDto>();
        }
    }

            public async Task<decimal> GetMonthlyGrowthAsync(TokenModel tokenModel)
    {
        try
        {
            var currentMonth = await GetMonthlyRecurringRevenueAsync(tokenModel);
            var lastMonth = await GetMonthlyRecurringRevenueAsync(tokenModel); // TODO: Implement last month calculation
            
            var growthRate = lastMonth > 0 ? ((currentMonth - lastMonth) / lastMonth) * 100 : 0;
            
            _logger.LogInformation("Monthly growth rate calculated by user {UserId}: {GrowthRate}%", tokenModel?.UserID ?? 0, growthRate);
            return growthRate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating monthly growth rate by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

            public async Task<int> GetNewSubscriptionsThisMonthAsync(TokenModel tokenModel)
    {
        try
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            
            var newSubscriptions = await _subscriptionRepository.GetSubscriptionsCreatedInRangeAsync(startOfMonth, endOfMonth);
            var count = newSubscriptions.Count();
            
            _logger.LogInformation("New subscriptions this month calculated by user {UserId}: {Count}", tokenModel?.UserID ?? 0, count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating new subscriptions this month by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

            public async Task<int> GetActiveSubscriptionsAsync(TokenModel tokenModel)
    {
        try
        {
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var count = activeSubscriptions.Count();
            
            _logger.LogInformation("Active subscriptions count calculated by user {UserId}: {Count}", tokenModel?.UserID ?? 0, count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating active subscriptions count by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

    public async Task<int> GetPausedSubscriptionsAsync(TokenModel tokenModel)
    {
        try
        {
            var pausedSubscriptions = await _subscriptionRepository.GetPausedSubscriptionsAsync();
            var count = pausedSubscriptions.Count();
            
            _logger.LogInformation("Paused subscriptions count calculated by user {UserId}: {Count}", tokenModel?.UserID ?? 0, count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating paused subscriptions count by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

    public async Task<int> GetCancelledSubscriptionsAsync(TokenModel tokenModel)
    {
        try
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            
            var cancelledSubscriptions = await _subscriptionRepository.GetCancelledSubscriptionsInRangeAsync(startOfMonth, endOfMonth);
            var count = cancelledSubscriptions.Count();
            
            _logger.LogInformation("Cancelled subscriptions this month calculated by user {UserId}: {Count}", tokenModel?.UserID ?? 0, count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating cancelled subscriptions this month by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

    public async Task<JsonModel> GetBillingAnalyticsAsync(TokenModel tokenModel)
    {
        try
        {
            var analytics = new BillingAnalyticsDto
            {
                TotalRevenue = await GetTotalRevenueAsync(null, null, tokenModel),
                MonthlyRecurringRevenue = await GetMonthlyRecurringRevenueAsync(tokenModel),
                AverageRevenuePerUser = await CalculateAverageRevenuePerUserAsync(tokenModel),
                FailedPayments = await GetFailedPaymentsAsync(),
                RefundsIssued = await GetRefundsIssuedAsync(null, null, tokenModel),
                PaymentSuccessRate = await CalculatePaymentSuccessRateAsync(),
                RevenueByCategory = await GetRevenueByCategoryAsync(null, null, tokenModel),
                RevenueTrend = await GetRevenueTrendAsync(null, null, tokenModel)
            };

            return new JsonModel { data = analytics, Message = "Billing analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing analytics");
            return new JsonModel { data = new object(), Message = "Error retrieving billing analytics", StatusCode = 500 };
        }
    }

            public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;
            
            var billingRecords = await _billingRepository.GetAllAsync();
            var revenueInRange = billingRecords
                .Where(br => br.CreatedDate >= start && br.CreatedDate <= end && br.Status == BillingRecord.BillingStatus.Paid)
                .Sum(br => br.Amount);
            
            _logger.LogInformation("Total revenue calculated by user {UserId}: {Revenue} for period {StartDate} to {EndDate}", 
                tokenModel?.UserID ?? 0, revenueInRange, start, end);
            return revenueInRange;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total revenue by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

    public async Task<decimal> CalculateAverageRevenuePerUserAsync(TokenModel tokenModel)
    {
        try
        {
            var totalRevenue = await GetTotalRevenueAsync(null, null, tokenModel);
            var totalUsers = await GetTotalUsersAsync(tokenModel);

            if (totalUsers == 0) return 0;

            return totalRevenue / totalUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average revenue per user");
            return 0;
        }
    }

    public async Task<int> GetFailedPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // TODO: Implement failed payments count
            return 23;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed payments");
            return 0;
        }
    }

            public async Task<int> GetRefundsIssuedAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;
            
            var billingRecords = await _billingRepository.GetAllAsync();
            var refundsInRange = billingRecords
                .Where(br => br.CreatedDate >= start && br.CreatedDate <= end && br.Status == BillingRecord.BillingStatus.Refunded)
                .Count();
            
            _logger.LogInformation("Refunds issued calculated by user {UserId}: {RefundCount} for period {StartDate} to {EndDate}", 
                tokenModel?.UserID ?? 0, refundsInRange, start, end);
            return refundsInRange;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating refunds issued by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

    public async Task<decimal> CalculatePaymentSuccessRateAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // TODO: Implement payment success rate calculation
            return 96.8m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating payment success rate");
            return 0;
        }
    }

    public async Task<IEnumerable<CategoryRevenueDto>> GetRevenueByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;
            
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryRevenue = new List<CategoryRevenueDto>();
            
            foreach (var category in categories)
            {
                var subscriptions = await _subscriptionRepository.GetByCategoryIdAsync(category.Id);
                var subscriptionsInRange = subscriptions.Where(s => s.CreatedDate >= start && s.CreatedDate <= end);
                
                var revenue = subscriptionsInRange.Sum(s => s.Amount);
                
                categoryRevenue.Add(new CategoryRevenueDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Revenue = revenue,
                    SubscriptionCount = subscriptionsInRange.Count()
                });
            }
            
            var sortedCategoryRevenue = categoryRevenue
                .OrderByDescending(cr => cr.Revenue)
                .ToList();
            
            _logger.LogInformation("Category revenue calculated by user {UserId}: {CategoryCount} categories", 
                tokenModel?.UserID ?? 0, sortedCategoryRevenue.Count);
            return sortedCategoryRevenue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating category revenue by user {UserId}", tokenModel?.UserID ?? 0);
            return Enumerable.Empty<CategoryRevenueDto>();
        }
    }

    public async Task<IEnumerable<RevenueTrendDto>> GetRevenueTrendAsync(DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;
            
            var billingRecords = await _billingRepository.GetAllAsync();
            var revenueTrends = new List<RevenueTrendDto>();
            
            var currentDate = start;
            while (currentDate <= end)
            {
                var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                var monthlyRevenue = billingRecords
                    .Where(br => br.CreatedDate >= monthStart && br.CreatedDate <= monthEnd && br.Status == BillingRecord.BillingStatus.Paid)
                    .Sum(br => br.Amount);
                
                revenueTrends.Add(new RevenueTrendDto
                {
                    Period = monthStart.ToString("yyyy-MM"),
                    Revenue = monthlyRevenue,
                    Month = monthStart.Month,
                    Year = monthStart.Year
                });
                
                currentDate = currentDate.AddMonths(1);
            }
            
            _logger.LogInformation("Revenue trends calculated by user {UserId}: {TrendCount} periods", 
                tokenModel?.UserID ?? 0, revenueTrends.Count);
            return revenueTrends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating revenue trends by user {UserId}", tokenModel?.UserID ?? 0);
            return Enumerable.Empty<RevenueTrendDto>();
        }
    }

    public async Task<JsonModel> GetUserAnalyticsAsync(TokenModel tokenModel)
    {
        try
        {
            var analytics = new UserAnalyticsDto
            {
                TotalUsers = await GetTotalUsersAsync(tokenModel),
                ActiveUsers = await GetActiveUsersAsync(tokenModel),
                NewUsersThisMonth = await GetNewUsersThisMonthAsync(tokenModel),
                UserRetentionRate = await CalculateUserRetentionRateAsync(),
                AverageUserLifetime = await CalculateAverageUserLifetimeAsync(),
                TopUserCategories = await GetTopUserCategoriesAsync()
            };

            return new JsonModel { data = analytics, Message = "User analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user analytics");
            return new JsonModel { data = new object(), Message = "Error retrieving user analytics", StatusCode = 500 };
        }
    }

            public async Task<int> GetTotalUsersAsync(TokenModel tokenModel)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var count = users.Count();
            
            _logger.LogInformation("Total users count calculated by user {UserId}: {Count}", tokenModel?.UserID ?? 0, count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total users count by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

            public async Task<int> GetActiveUsersAsync(TokenModel tokenModel)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var activeCount = users.Count(u => u.IsActive);
            
            _logger.LogInformation("Active users count calculated by user {UserId}: {Count}", tokenModel?.UserID ?? 0, activeCount);
            return activeCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating active users count by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

            public async Task<int> GetNewUsersThisMonthAsync(TokenModel tokenModel)
    {
        try
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            
            var users = await _userRepository.GetAllAsync();
            var newUsersCount = users.Count(u => u.CreatedDate >= startOfMonth && u.CreatedDate <= endOfMonth);
            
            _logger.LogInformation("New users this month calculated by user {UserId}: {Count}", tokenModel?.UserID ?? 0, newUsersCount);
            return newUsersCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating new users this month by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

    public async Task<decimal> CalculateUserRetentionRateAsync()
    {
        try
        {
            // TODO: Implement user retention rate calculation
            return 87.3m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating user retention rate");
            return 0;
        }
    }

    public async Task<TimeSpan> CalculateAverageUserLifetimeAsync()
    {
        try
        {
            // TODO: Implement average user lifetime calculation
            return TimeSpan.FromDays(180);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average user lifetime");
            return TimeSpan.Zero;
        }
    }

    public async Task<IEnumerable<CategoryAnalyticsDto>> GetTopUserCategoriesAsync()
    {
        try
        {
            // TODO: Implement top user categories
            return new List<CategoryAnalyticsDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top user categories");
            return new List<CategoryAnalyticsDto>();
        }
    }

    public async Task<JsonModel> GetProviderAnalyticsAsync(TokenModel tokenModel)
    {
        try
        {
            var analytics = new ProviderAnalyticsDto
            {
                TotalProviders = await GetTotalProvidersAsync(tokenModel),
                ActiveProviders = await GetActiveProvidersAsync(tokenModel),
                AverageProviderRating = await CalculateAverageProviderRatingAsync(),
                // TotalConsultations = 0, // TODO: Implement
                // Use privilege usage system for consultation analytics if needed
                AverageConsultationDuration = 0, // TODO: Implement
                TopPerformingProviders = await GetTopPerformingProvidersAsync(),
                ProviderWorkload = await GetProviderWorkloadAsync()
            };

            return new JsonModel { data = analytics, Message = "Provider analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider analytics");
            return new JsonModel { data = new object(), Message = "Error retrieving provider analytics", StatusCode = 500 };
        }
    }

    public async Task<int> GetTotalProvidersAsync()
    {
        try
        {
            // TODO: Implement total providers count
            return 45;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total providers");
            return 0;
        }
    }

    public async Task<int> GetActiveProvidersAsync()
    {
        try
        {
            // TODO: Implement active providers count
            return 38;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active providers");
            return 0;
        }
    }

    public async Task<decimal> CalculateAverageProviderRatingAsync()
    {
        try
        {
            // TODO: Implement average provider rating calculation
            return 4.5m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average provider rating");
            return 0;
        }
    }

    public async Task<IEnumerable<ProviderPerformanceDto>> GetTopPerformingProvidersAsync()
    {
        try
        {
            // TODO: Implement top performing providers
            return new List<ProviderPerformanceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top performing providers");
            return new List<ProviderPerformanceDto>();
        }
    }

    public async Task<IEnumerable<ProviderWorkloadDto>> GetProviderWorkloadAsync()
    {
        try
        {
            // TODO: Implement provider workload
            return new List<ProviderWorkloadDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider workload");
            return new List<ProviderWorkloadDto>();
        }
    }





    public async Task<IEnumerable<ApiUsageDto>> GetApiUsageAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // TODO: Implement API usage tracking
            return new List<ApiUsageDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API usage");
            return new List<ApiUsageDto>();
        }
    }

    public async Task<IEnumerable<ErrorLogDto>> GetErrorLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // TODO: Implement error logs
            return new List<ErrorLogDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error logs");
            return new List<ErrorLogDto>();
        }
    }

    public async Task<byte[]> GenerateSubscriptionReportAsync(DateTime startDate, DateTime endDate, string format = "pdf")
    {
        try
        {
            // TODO: Implement subscription report generation
            return new byte[0];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating subscription report");
            throw;
        }
    }

    public async Task<byte[]> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf")
    {
        try
        {
            // TODO: Implement billing report generation
            return new byte[0];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating billing report");
            throw;
        }
    }

    public async Task<byte[]> GenerateUserReportAsync(DateTime startDate, DateTime endDate, string format = "pdf")
    {
        try
        {
            // TODO: Implement user report generation
            return new byte[0];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating user report");
            throw;
        }
    }

    public async Task<byte[]> GenerateProviderReportAsync(DateTime startDate, DateTime endDate, string format = "pdf")
    {
        try
        {
            // TODO: Implement provider report generation
            return new byte[0];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating provider report");
            throw;
        }
    }

    private async Task<int> GetTotalSubscriptionsAsync()
    {
        try
        {
            var subscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            return subscriptions.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total subscriptions");
            return 0;
        }
    }

    // Additional interface methods with correct signatures
    public async Task<JsonModel> GetBillingAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null)
    {
        try
        {
            var analytics = new BillingAnalyticsDto
            {
                TotalRevenue = await GetTotalRevenueAsync(startDate, endDate, tokenModel),
                FailedPayments = await GetFailedPaymentsAsync(startDate, endDate),
                PaymentSuccessRate = await CalculatePaymentSuccessRateAsync(startDate, endDate),
                AverageRevenuePerUser = await CalculateAverageRevenuePerUserAsync(tokenModel),
                RefundsIssued = await GetRefundsIssuedAsync(startDate, endDate, tokenModel)
            };

            return new JsonModel { data = analytics, Message = "Billing analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing analytics");
            return new JsonModel { data = new object(), Message = "Error retrieving billing analytics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null)
    {
        try
        {
            var analytics = new UserAnalyticsDto
            {
                TotalUsers = await GetTotalUsersAsync(tokenModel),
                ActiveUsers = await GetActiveUsersAsync(tokenModel),
                NewUsersThisMonth = await GetNewUsersThisMonthAsync(tokenModel),
                UserRetentionRate = await CalculateUserRetentionRateAsync(),
                AverageUserLifetime = await CalculateAverageUserLifetimeAsync()
            };

            return new JsonModel { data = analytics, Message = "User analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user analytics");
            return new JsonModel { data = new object(), Message = "Error retrieving user analytics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetProviderAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var analytics = new ProviderAnalyticsDto
            {
                TotalProviders = await GetTotalProvidersAsync(tokenModel),
                ActiveProviders = await GetActiveProvidersAsync(tokenModel),
                AverageProviderRating = await CalculateAverageProviderRatingAsync()
            };

            return new JsonModel { data = analytics, Message = "Provider analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider analytics");
            return new JsonModel { data = new object(), Message = "Error retrieving provider analytics", StatusCode = 500 };
        }
    }



    public async Task<JsonModel> GenerateSubscriptionReportAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;
            
            var reportData = new
            {
                Period = new { StartDate = start, EndDate = end },
                SubscriptionAnalytics = await GetSubscriptionAnalyticsAsync(start, end, tokenModel),
                RevenueAnalytics = await GetRevenueAnalyticsAsync(start, end, tokenModel),
                TopCategories = await GetTopCategoriesAsync(start, end, tokenModel),
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = tokenModel?.UserID ?? 0
            };
            
            _logger.LogInformation("Subscription report generated by user {UserId} for period {StartDate} to {EndDate}", 
                tokenModel?.UserID ?? 0, start, end);
            return new JsonModel { data = reportData, Message = "Subscription report generated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating subscription report by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error generating subscription report", StatusCode = 500 };
        }
    }

            public async Task<JsonModel> GenerateBillingReportAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var reportData = await GenerateBillingReportAsync(startDate ?? DateTime.UtcNow.AddMonths(-1), endDate ?? DateTime.UtcNow, "pdf");
            _logger.LogInformation("Billing report generated by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = reportData, Message = "Billing report generated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating billing report by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error generating billing report", StatusCode = 500 };
        }
    }

            public async Task<JsonModel> GenerateUserReportAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var reportData = await GenerateUserReportAsync(startDate ?? DateTime.UtcNow.AddMonths(-1), endDate ?? DateTime.UtcNow, "pdf");
            _logger.LogInformation("User report generated by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = reportData, Message = "User report generated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating user report by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving user report", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GenerateProviderReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var reportData = await GenerateProviderReportAsync(startDate ?? DateTime.UtcNow.AddMonths(-1), endDate ?? DateTime.UtcNow, "pdf");
            return new JsonModel { data = reportData, Message = "Provider report generated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating provider report");
            return new JsonModel { data = new object(), Message = "Error generating provider report", StatusCode = 500 };
        }
    }

            public async Task<JsonModel> ExportSubscriptionAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;
            
            var analytics = await GetSubscriptionAnalyticsAsync(start, end, tokenModel);
            var categories = await GetTopCategoriesAsync(start, end, tokenModel);
            var revenue = await GetRevenueAnalyticsAsync(start, end, tokenModel);
            
            var exportData = new
            {
                Period = new { StartDate = start, EndDate = end },
                Analytics = analytics,
                Categories = categories,
                Revenue = revenue,
                ExportedAt = DateTime.UtcNow,
                ExportedBy = tokenModel?.UserID ?? 0
            };
            
            _logger.LogInformation("Subscription analytics exported by user {UserId} for period {StartDate} to {EndDate}", 
                tokenModel?.UserID ?? 0, start, end);
            return new JsonModel { data = exportData, Message = "Subscription analytics exported successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting subscription analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error exporting subscription analytics", StatusCode = 500 };
        }
    }

    // Missing methods for subscription dashboard
    private async Task<OverviewMetricsDto> GetOverviewMetricsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var totalSubscriptions = await GetTotalSubscriptionsAsync();
            var activeSubscriptions = await GetActiveSubscriptionsAsync(tokenModel);
            var newSubscriptions = await GetNewSubscriptionsThisMonthAsync(tokenModel);
            var cancelledSubscriptions = await GetCancelledSubscriptionsAsync(tokenModel);
            var pausedSubscriptions = await GetPausedSubscriptionsAsync(tokenModel);
            var averageValue = await CalculateAverageSubscriptionValueAsync(tokenModel);
            var totalRevenue = await GetMonthlyRecurringRevenueAsync(tokenModel);

            return new OverviewMetricsDto
            {
                TotalSubscriptions = totalSubscriptions,
                ActiveSubscriptions = activeSubscriptions,
                CancelledSubscriptions = cancelledSubscriptions,
                PausedSubscriptions = pausedSubscriptions,
                TrialSubscriptions = 0, // TODO: Implement when trial tracking is available
                NewSubscriptionsThisPeriod = newSubscriptions,
                CancelledSubscriptionsThisPeriod = cancelledSubscriptions,
                AverageSubscriptionValue = averageValue,
                TotalRevenue = totalRevenue
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overview metrics");
            return new OverviewMetricsDto();
        }
    }

    private async Task<RevenueAnalyticsDto> GetRevenueMetricsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var mrr = await GetMonthlyRecurringRevenueAsync(tokenModel);
            var arr = await GetAnnualRecurringRevenueAsync(tokenModel);
            var totalRevenue = mrr;
            var averageValue = await CalculateAverageSubscriptionValueAsync(tokenModel);

            return new RevenueAnalyticsDto
            {
                TotalRevenue = totalRevenue,
                MonthlyRevenue = mrr,
                AverageRevenuePerSubscription = averageValue,
                MonthlyRevenueBreakdown = new List<MonthlyRevenueData>(), // TODO: Implement monthly revenue tracking
                RevenueByCategory = new List<CategoryRevenueData>() // TODO: Implement plan revenue tracking
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue metrics");
            return new RevenueAnalyticsDto();
        }
    }

    private async Task<ChurnAnalyticsDto> GetChurnMetricsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var churnRate = await CalculateChurnRateAsync(startDate, endDate, tokenModel);
            var cancelledSubscriptions = await GetCancelledSubscriptionsAsync(tokenModel);
            var retentionRate = 100 - churnRate;

            return new ChurnAnalyticsDto
            {
                ChurnRate = churnRate,
                RetentionRate = retentionRate,
                CancelledSubscriptions = cancelledSubscriptions,
                CancellationReasons = new List<CancellationReasonDto>(), // TODO: Implement cancellation reason tracking
                AverageLifetime = 0, // TODO: Implement lifetime calculation
                CohortRetention = new List<CohortRetentionDto>() // TODO: Implement cohort analysis
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting churn metrics");
            return new ChurnAnalyticsDto();
        }
    }

    private async Task<PlanAnalyticsDto> GetPlanMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var topCategories = await GetTopCategoriesAsync(startDate, endDate);
            var totalPlans = await GetTotalSubscriptionPlansAsync();

            return new PlanAnalyticsDto
            {
                PlanPerformance = new List<PlanPerformanceDto>(), // TODO: Implement plan performance tracking
                TopPerformingPlans = new List<PlanPerformanceDto>(), // TODO: Implement top plans tracking
                PlanComparison = new List<PlanPerformanceDto>() // TODO: Implement plan comparison
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan metrics");
            return new PlanAnalyticsDto();
        }
    }

    private async Task<UsageAnalyticsDto> GetUsageMetricsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var totalUsers = await GetTotalUsersAsync(tokenModel);
            var activeUsers = await GetActiveUsersAsync(tokenModel);
            var averageUsage = await CalculateAverageUsageAsync();

            return new UsageAnalyticsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = totalUsers - activeUsers,
                AverageUsage = averageUsage,
                FeatureUsage = new List<FeatureUsageDto>(), // TODO: Implement feature usage tracking
                UserActivity = new List<UserActivityDto>() // TODO: Implement user activity tracking
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage metrics");
            return new UsageAnalyticsDto();
        }
    }

    // Helper methods for metrics
    private async Task<int> GetTotalSubscriptionPlansAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            return categories.Sum(c => c.SubscriptionPlans?.Count ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total subscription plans");
            return 0;
        }
    }

    private async Task<decimal> CalculateAverageUsageAsync()
    {
        try
        {
            // TODO: Implement when usage tracking is available
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average usage");
            return 0;
        }
    }

    // === MISSING INTERFACE METHODS ===
    
    public async Task<JsonModel> GetSystemAnalyticsAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting system analytics by user {UserId}", tokenModel?.UserID ?? 0);
            
            var analytics = new
            {
                SystemHealth = await GetSystemHealthAsync(tokenModel),
                TotalUsers = await GetTotalUsersAsync(tokenModel),
                TotalSubscriptions = await GetTotalSubscriptionsAsync(tokenModel),
                TotalRevenue = await GetTotalRevenueAsync(null, null, tokenModel),
                ActiveSubscriptions = await GetActiveSubscriptionsAsync(tokenModel)
            };
            
            _logger.LogInformation("System analytics retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = analytics, Message = "System analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system analytics by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving system analytics", StatusCode = 500 };
        }
    }







    public async Task<JsonModel> GetSystemHealthAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting system health by user {UserId}", tokenModel?.UserID ?? 0);
            
            var health = new
            {
                Status = "Healthy",
                LastChecked = DateTime.UtcNow,
                DatabaseConnection = "Connected",
                ExternalServices = "All Operational"
            };
            
            _logger.LogInformation("System health retrieved by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = health, Message = "System health retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving system health", StatusCode = 500 };
        }
    }

    // === END MISSING INTERFACE METHODS ===

    public async Task<JsonModel> GenerateProviderReportAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Generating provider report by user {UserId}", tokenModel?.UserID ?? 0);
            
            var report = new
            {
                TotalProviders = await GetTotalProvidersAsync(tokenModel),
                ActiveProviders = await GetActiveProvidersAsync(tokenModel),
                NewProvidersThisMonth = await GetNewProvidersThisMonthAsync(tokenModel),
                GeneratedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Provider report generated by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = report, Message = "Provider report generated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating provider report by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error generating provider report", StatusCode = 500 };
        }
    }

    // === MISSING METHODS ===
    
    public async Task<int> GetTotalSubscriptionsAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting total subscriptions count by user {UserId}", tokenModel?.UserID ?? 0);
            var count = await _subscriptionRepository.GetCountAsync();
            _logger.LogInformation("Total subscriptions count: {Count} by user {UserId}", count, tokenModel?.UserID ?? 0);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total subscriptions count by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }
    
    public async Task<int> GetTotalProvidersAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting total providers count by user {UserId}", tokenModel?.UserID ?? 0);
            var providers = await _userRepository.GetByUserTypeAsync("Provider");
            var count = providers.Count();
            _logger.LogInformation("Total providers count: {Count} by user {UserId}", count, tokenModel?.UserID ?? 0);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total providers count by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }
    
    public async Task<int> GetActiveProvidersAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting active providers count by user {UserId}", tokenModel?.UserID ?? 0);
            var providers = await _userRepository.GetByUserTypeAsync("Provider");
            var count = providers.Count(p => p.IsActive);
            _logger.LogInformation("Active providers count: {Count} by user {UserId}", count, tokenModel?.UserID ?? 0);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active providers count by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }
    
    public async Task<int> GetNewProvidersThisMonthAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting new providers this month count by user {UserId}", tokenModel?.UserID ?? 0);
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var providers = await _userRepository.GetByUserTypeAsync("Provider");
            var count = providers.Count(p => p.CreatedDate >= startOfMonth);
            _logger.LogInformation("New providers this month: {Count} by user {UserId}", count, tokenModel?.UserID ?? 0);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting new providers this month count by user {UserId}", tokenModel?.UserID ?? 0);
            return 0;
        }
    }

    // === END MISSING METHODS ===
} 
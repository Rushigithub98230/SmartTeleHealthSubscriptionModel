using Microsoft.EntityFrameworkCore;
using Moq;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Enums;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.Infrastructure.Services;
using System.Diagnostics;
using Xunit;

namespace SmartTelehealth.Tests.Integration.Billing;

/// <summary>
/// Performance tests for the billing system to ensure it can handle production loads
/// </summary>
public class PerformanceBillingTests : SimpleBillingTestBase
{
    private readonly TestDataBuilder _dataBuilder;

    public PerformanceBillingTests()
    {
        _dataBuilder = new TestDataBuilder(_context);
        
        // Setup Stripe service mocks with realistic delays
        _stripeServiceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .Returns(async () =>
            {
                await Task.Delay(100); // Simulate network delay
                return new PaymentResultDto { Status = "succeeded", PaymentIntentId = "pi_test_123" };
            });
    }

    [Fact]
    public async Task PerformanceTest_PlanPriceCalculation_WithLargeNumberOfPrivileges()
    {
        // Arrange - Create a plan with many privileges
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly
        var usdCurrency = testEnv.MasterData.Currencies[0]; // USD

        // Create a plan with 50 privileges
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Performance Test Plan",
            Description = "Plan with many privileges for performance testing",
            Price = 0, // Will be calculated
            BillingCycleId = monthlyCycle.Id,
            CurrencyId = usdCurrency.Id,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _adminToken.UserID
        };
        _context.SubscriptionPlans.Add(plan);

        // Create 50 privileges
        var privileges = new List<Privilege>();
        for (int i = 0; i < 50; i++)
        {
            var privilege = new Privilege
            {
                Id = Guid.NewGuid(),
                Name = $"Test Privilege {i}",
                Description = $"Test privilege {i} for performance testing",
                PrivilegeTypeId = testEnv.MasterData.PrivilegeTypes[0].Id,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminToken.UserID
            };
            privileges.Add(privilege);
        }
        _context.Privileges.AddRange(privileges);

        // Create plan privileges
        var planPrivileges = new List<SubscriptionPlanPrivilege>();
        for (int i = 0; i < 50; i++)
        {
            var planPrivilege = new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = plan.Id,
                PrivilegeId = privileges[i].Id,
                DailyLimit = (i % 10) + 1, // Vary the limits
                UnitCost = (i % 20) + 5.00m, // Vary the costs
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminToken.UserID
            };
            planPrivileges.Add(planPrivilege);
        }
        _context.SubscriptionPlanPrivileges.AddRange(planPrivileges);
        await _context.SaveChangesAsync();

        // Act - Measure performance
        var stopwatch = Stopwatch.StartNew();
        var calculateDto = new CalculatePlanPriceDto
        {
            PlanId = plan.Id,
            AdminCommissionPercentage = 15
        };
        var result = await _privilegeBasedBillingService.CalculatePlanBasePriceAsync(calculateDto, _adminToken);
        stopwatch.Stop();

        // Assert
        Assert.True(result.StatusCode == 200);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Plan price calculation took {stopwatch.ElapsedMilliseconds}ms, should be under 1000ms");
    }

    [Fact]
    public async Task PerformanceTest_ConcurrentPrivilegeUsage_WithManyUsers()
    {
        // Arrange - Create many users and subscriptions
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(100); // 100 users
        var basicPlan = testEnv.Plans[0]; // Basic Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly
        var basicConsultation = testEnv.Privileges[0]; // Basic Consultation

        // Create subscriptions for all users
        var subscriptions = new List<Subscription>();
        foreach (var user in users)
        {
            var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
            await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);
            subscriptions.Add(subscription);
        }

        // Act - Process usage for all users concurrently
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task<JsonModel>>();

        foreach (var user in users)
        {
            var usageDto = new ProcessPrivilegeUsageDto
            {
                UserId = user.Id,
                PrivilegeId = basicConsultation.Id,
                UsageCount = 1
            };
            tasks.Add(_privilegeBasedBillingService.ProcessPrivilegeUsageAsync(usageDto, _userToken));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.All(results, result => Assert.True(result.StatusCode == 200));
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Concurrent usage processing took {stopwatch.ElapsedMilliseconds}ms, should be under 5000ms");
    }

    [Fact]
    public async Task PerformanceTest_BulkBillingRecordCreation()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(50); // 50 users

        // Act - Create billing records for all users
        var stopwatch = Stopwatch.StartNew();
        var billingRecords = new List<BillingRecord>();

        foreach (var user in users)
        {
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Amount = 100.00m,
                TotalAmount = 100.00m,
                TaxAmount = 0,
                ShippingAmount = 0,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                Description = $"Performance test billing record for user {user.Id}",
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminToken.UserID
            };
            billingRecords.Add(billingRecord);
        }

        _context.BillingRecords.AddRange(billingRecords);
        await _context.SaveChangesAsync();
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"Bulk billing record creation took {stopwatch.ElapsedMilliseconds}ms, should be under 2000ms");
        Assert.Equal(50, await _context.BillingRecords.CountAsync());
    }

    [Fact]
    public async Task PerformanceTest_UsageSummaryGeneration_WithLargeDataset()
    {
        // Arrange - Create user with extensive usage history
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var premiumPlan = testEnv.Plans[1]; // Premium Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly

        var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, premiumPlan, monthlyCycle);
        await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Create extensive usage history
        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        foreach (var usage in usages)
        {
            usage.UsedValue = 1000; // High usage
            usage.UpdatedDate = DateTime.UtcNow;
            usage.UpdatedBy = _adminToken.UserID;
        }
        await _context.SaveChangesAsync();

        // Act - Measure performance
        var stopwatch = Stopwatch.StartNew();
        var result = await _privilegeBasedBillingService.GetPrivilegeUsageSummaryAsync(user.Id, _userToken);
        stopwatch.Stop();

        // Assert
        Assert.True(result.StatusCode == 200);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Usage summary generation took {stopwatch.ElapsedMilliseconds}ms, should be under 500ms");
    }

    [Fact]
    public async Task PerformanceTest_ConcurrentPaymentProcessing()
    {
        // Arrange - Create billing records for payment processing
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(20); // 20 users

        var billingRecords = new List<BillingRecord>();
        foreach (var user in users)
        {
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Amount = 50.00m,
                TotalAmount = 50.00m,
                TaxAmount = 0,
                ShippingAmount = 0,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                Description = $"Payment test billing record for user {user.Id}",
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminToken.UserID
            };
            billingRecords.Add(billingRecord);
        }
        _context.BillingRecords.AddRange(billingRecords);
        await _context.SaveChangesAsync();

        // Act - Process payments concurrently
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task<JsonModel>>();

        foreach (var billingRecord in billingRecords)
        {
            tasks.Add(_billingService.ProcessPaymentAsync(billingRecord.Id, _userToken));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.All(results, result => Assert.True(result.StatusCode == 200));
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, $"Concurrent payment processing took {stopwatch.ElapsedMilliseconds}ms, should be under 10000ms");
    }

    [Fact]
    public async Task PerformanceTest_SubscriptionRenewal_WithManySubscriptions()
    {
        // Arrange - Create many subscriptions for renewal
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(50); // 50 users
        var basicPlan = testEnv.Plans[0]; // Basic Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly

        var subscriptions = new List<Subscription>();
        foreach (var user in users)
        {
            var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
            await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);
            
            // Set subscription as overdue for renewal
            subscription.NextBillingDate = DateTime.UtcNow.AddDays(-1);
            subscription.UpdatedDate = DateTime.UtcNow;
            subscription.UpdatedBy = _adminToken.UserID;
            subscriptions.Add(subscription);
        }
        await _context.SaveChangesAsync();

        // Act - Process renewals sequentially (as they would be in a batch job)
        var stopwatch = Stopwatch.StartNew();
        var results = new List<JsonModel>();

        foreach (var subscription in subscriptions)
        {
            var result = await _privilegeBasedBillingService.ProcessSubscriptionRenewalAsync(subscription.Id, _userToken);
            results.Add(result);
        }
        stopwatch.Stop();

        // Assert
        Assert.All(results, result => Assert.True(result.StatusCode == 200));
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Subscription renewal processing took {stopwatch.ElapsedMilliseconds}ms, should be under 5000ms");
    }

    [Fact]
    public async Task PerformanceTest_MemoryUsage_WithLargeDataset()
    {
        // Arrange - Create large dataset
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(1000); // 1000 users
        var basicPlan = testEnv.Plans[0]; // Basic Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly

        // Create subscriptions and usage tracking for all users
        var subscriptions = new List<Subscription>();
        foreach (var user in users)
        {
            var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
            await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);
            subscriptions.Add(subscription);
        }

        // Act - Perform operations that load large datasets
        var stopwatch = Stopwatch.StartNew();
        
        // Load all subscriptions
        var allSubscriptions = await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .ToListAsync();

        // Load all privilege usages
        var allUsages = await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.Subscription)
            .Include(u => u.Privilege)
            .Include(u => u.SubscriptionPlanPrivilege)
            .ToListAsync();

        stopwatch.Stop();

        // Assert
        Assert.Equal(1000, allSubscriptions.Count);
        Assert.True(allUsages.Count > 0);
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Large dataset loading took {stopwatch.ElapsedMilliseconds}ms, should be under 3000ms");
    }

    [Fact]
    public async Task PerformanceTest_TransactionHandling_WithRollback()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var basicPlan = testEnv.Plans[0]; // Basic Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly

        var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
        await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Act - Test transaction rollback performance
        var stopwatch = Stopwatch.StartNew();
        
        // Simulate a transaction that will fail
        await _unitOfWorkMock.Object.BeginTransactionAsync();
        try
        {
            // Perform some operations
            var usageDto = new ProcessPrivilegeUsageDto
            {
                UserId = user.Id,
                PrivilegeId = testEnv.Privileges[0].Id,
                UsageCount = 1
            };
            await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(usageDto, _userToken);
            
            // Simulate failure
            throw new Exception("Simulated failure");
        }
        catch
        {
            await _unitOfWorkMock.Object.RollbackTransactionAsync();
        }
        
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Transaction rollback took {stopwatch.ElapsedMilliseconds}ms, should be under 1000ms");
    }

}

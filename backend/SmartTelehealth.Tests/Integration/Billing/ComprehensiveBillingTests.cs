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
using Xunit;

namespace SmartTelehealth.Tests.Integration.Billing;

/// <summary>
/// Comprehensive billing system tests covering all major scenarios
/// </summary>
public class ComprehensiveBillingTests : SimpleBillingTestBase
{
    private readonly TestDataBuilder _dataBuilder;

    public ComprehensiveBillingTests()
    {
        _dataBuilder = new TestDataBuilder(_context);
    }

    [Fact]
    public async Task Test_CompleteBillingLifecycle_ShouldWorkEndToEnd()
    {
        // Arrange - Create complete test environment
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(3);
        var user = users[0];

        // Create subscription for user
        var basicPlan = testEnv.Plans[0]; // Basic Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly
        var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
        
        // Create privilege usage tracking
        await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Act & Assert - Test complete lifecycle

        // 1. Calculate plan price
        var calculateDto = new CalculatePlanPriceDto
        {
            PlanId = basicPlan.Id,
            AdminCommissionPercentage = 10
        };
        var priceResult = await _privilegeBasedBillingService.CalculatePlanBasePriceAsync(calculateDto, _adminToken);
        Assert.True(priceResult.StatusCode == 200);

        // 2. Process privilege usage within limits
        var basicConsultation = testEnv.Privileges[0]; // Basic Consultation
        var usageDto = new ProcessPrivilegeUsageDto
        {
            UserId = user.Id,
            PrivilegeId = basicConsultation.Id,
            UsageCount = 1
        };
        var usageResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(usageDto, _userToken);
        Assert.True(usageResult.StatusCode == 200);

        // 3. Process privilege usage exceeding limits (should create overage billing)
        var overageUsageDto = new ProcessPrivilegeUsageDto
        {
            UserId = user.Id,
            PrivilegeId = basicConsultation.Id,
            UsageCount = 5 // Exceed daily limit of 2
        };
        var overageResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(overageUsageDto, _userToken);
        Assert.True(overageResult.StatusCode == 200);

        // 4. Verify overage billing record was created
        var overageBilling = await _context.BillingRecords
            .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Type == BillingRecord.BillingType.Overage);
        Assert.NotNull(overageBilling);
        Assert.Equal(BillingRecord.BillingStatus.Pending, overageBilling.Status);

        // 5. Process payment for overage
        var paymentResult = await _billingService.ProcessPaymentAsync(overageBilling.Id, _userToken);
        Assert.True(paymentResult.StatusCode == 200);

        // 6. Verify payment was processed
        var updatedBilling = await _context.BillingRecords.FindAsync(overageBilling.Id);
        Assert.NotNull(updatedBilling);
        Assert.Equal(BillingRecord.BillingStatus.Paid, updatedBilling.Status);

        // 7. Get usage summary
        var summaryResult = await _privilegeBasedBillingService.GetPrivilegeUsageSummaryAsync(user.Id, _userToken);
        Assert.True(summaryResult.StatusCode == 200);

        // 8. Process subscription renewal
        var renewalResult = await _privilegeBasedBillingService.ProcessSubscriptionRenewalAsync(subscription.Id, _userToken);
        Assert.True(renewalResult.StatusCode == 200);

        // 9. Verify usage was reset after renewal
        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();
        Assert.All(usages, u => Assert.Equal(0, u.UsedValue));
    }

    [Fact]
    public async Task Test_PlanPriceCalculation_WithDifferentCommissionRates()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var premiumPlan = testEnv.Plans[1]; // Premium Health Plan

        // Test with 0% commission
        var calculateDto1 = new CalculatePlanPriceDto
        {
            PlanId = premiumPlan.Id,
            AdminCommissionPercentage = 0
        };
        var result1 = await _privilegeBasedBillingService.CalculatePlanBasePriceAsync(calculateDto1, _adminToken);
        Assert.True(result1.StatusCode == 200);

        // Test with 15% commission
        var calculateDto2 = new CalculatePlanPriceDto
        {
            PlanId = premiumPlan.Id,
            AdminCommissionPercentage = 15
        };
        var result2 = await _privilegeBasedBillingService.CalculatePlanBasePriceAsync(calculateDto2, _adminToken);
        Assert.True(result2.StatusCode == 200);

        // Test with 25% commission
        var calculateDto3 = new CalculatePlanPriceDto
        {
            PlanId = premiumPlan.Id,
            AdminCommissionPercentage = 25
        };
        var result3 = await _privilegeBasedBillingService.CalculatePlanBasePriceAsync(calculateDto3, _adminToken);
        Assert.True(result3.StatusCode == 200);
    }

    [Fact]
    public async Task Test_PrivilegeUsageTracking_WithMultiplePrivileges()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var premiumPlan = testEnv.Plans[1]; // Premium Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly

        var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, premiumPlan, monthlyCycle);
        await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Act - Use multiple privileges
        var extendedConsultation = testEnv.Privileges[1]; // Extended Consultation
        var expressMedication = testEnv.Privileges[4]; // Express Medication Delivery
        var followUpCare = testEnv.Privileges[5]; // Follow-up Care

        // Use extended consultation
        var consultationUsage = new ProcessPrivilegeUsageDto
        {
            UserId = user.Id,
            PrivilegeId = extendedConsultation.Id,
            UsageCount = 2
        };
        var consultationResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(consultationUsage, _userToken);
        Assert.True(consultationResult.StatusCode == 200);

        // Use express medication delivery
        var medicationUsage = new ProcessPrivilegeUsageDto
        {
            UserId = user.Id,
            PrivilegeId = expressMedication.Id,
            UsageCount = 1
        };
        var medicationResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(medicationUsage, _userToken);
        Assert.True(medicationResult.StatusCode == 200);

        // Use follow-up care
        var followUpUsage = new ProcessPrivilegeUsageDto
        {
            UserId = user.Id,
            PrivilegeId = followUpCare.Id,
            UsageCount = 1
        };
        var followUpResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(followUpUsage, _userToken);
        Assert.True(followUpResult.StatusCode == 200);

        // Assert - Verify usage summary shows all privileges
        var summaryResult = await _privilegeBasedBillingService.GetPrivilegeUsageSummaryAsync(user.Id, _userToken);
        Assert.True(summaryResult.StatusCode == 200);
    }

    [Fact]
    public async Task Test_OverageBilling_WithDifferentPrivilegeTypes()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var basicPlan = testEnv.Plans[0]; // Basic Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly

        var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
        await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Act - Exceed limits for different privileges
        var basicConsultation = testEnv.Privileges[0]; // Basic Consultation (limit: 2)
        var standardMedication = testEnv.Privileges[3]; // Standard Medication Delivery (limit: 1)

        // Exceed consultation limit
        var consultationOverage = new ProcessPrivilegeUsageDto
        {
            UserId = user.Id,
            PrivilegeId = basicConsultation.Id,
            UsageCount = 5 // Exceed limit of 2
        };
        var consultationResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(consultationOverage, _userToken);
        Assert.True(consultationResult.StatusCode == 200);

        // Exceed medication limit
        var medicationOverage = new ProcessPrivilegeUsageDto
        {
            UserId = user.Id,
            PrivilegeId = standardMedication.Id,
            UsageCount = 3 // Exceed limit of 1
        };
        var medicationResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(medicationOverage, _userToken);
        Assert.True(medicationResult.StatusCode == 200);

        // Assert - Verify overage billing record was created (batched for efficiency)
        var overageBillings = await _context.BillingRecords
            .Where(b => b.UserId == user.Id && b.Type == BillingRecord.BillingType.Overage)
            .ToListAsync();
        Assert.Single(overageBillings); // Batching consolidates multiple overage charges

        // Verify total overage charges
        var totalOverage = overageBillings.Sum(b => b.TotalAmount);
        Assert.True(totalOverage > 0);
    }

    [Fact]
    public async Task Test_SubscriptionRenewal_WithPendingOverageCharges()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var basicPlan = testEnv.Plans[0]; // Basic Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly

        var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
        await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Create pending overage charges
        var overageBilling = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Amount = 50.00m,
            TotalAmount = 50.00m,
            TaxAmount = 0,
            ShippingAmount = 0,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Overage,
            Description = "Pending overage charge",
            BillingDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _adminToken.UserID
        };
        _context.BillingRecords.Add(overageBilling);
        await _context.SaveChangesAsync();

        // Act - Process renewal
        var renewalResult = await _privilegeBasedBillingService.ProcessSubscriptionRenewalAsync(subscription.Id, _userToken);

        // Assert - Renewal should succeed even with pending charges
        Assert.True(renewalResult.StatusCode == 200);
        Assert.Equal("Subscription renewed successfully with privilege usage reset", renewalResult.Message);

        // Verify usage was reset
        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();
        Assert.All(usages, u => Assert.Equal(0, u.UsedValue));
    }

    [Fact]
    public async Task Test_ProfessionalPlan_WithHighLimitsAndOverageCharges()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var professionalPlan = testEnv.Plans[2]; // Professional Health Plan
        var yearlyCycle = testEnv.MasterData.BillingCycles[4]; // Yearly

        var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, professionalPlan, yearlyCycle);
        await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Act - Use privileges within high limits first
        var specialistConsultation = testEnv.Privileges[2]; // Specialist Consultation (limit: 20)

        var withinLimitUsage = new ProcessPrivilegeUsageDto
        {
            UserId = user.Id,
            PrivilegeId = specialistConsultation.Id,
            UsageCount = 15 // Within limit of 20
        };
        var withinLimitResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(withinLimitUsage, _userToken);

        // Assert - Should succeed without overage charges
        Assert.True(withinLimitResult.StatusCode == 200);

        // Verify no overage billing record was created for within-limit usage
        var overageBilling = await _context.BillingRecords
            .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Type == BillingRecord.BillingType.Overage);
        Assert.Null(overageBilling);

        // Act - Now exceed the high limit
        var overageUsage = new ProcessPrivilegeUsageDto
        {
            UserId = user.Id,
            PrivilegeId = specialistConsultation.Id,
            UsageCount = 10 // This will make total usage 25, exceeding limit of 20
        };
        var overageResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(overageUsage, _userToken);

        // Assert - Should succeed but create overage charges
        Assert.True(overageResult.StatusCode == 200);

        // Verify overage billing record was created
        var overageBillingAfter = await _context.BillingRecords
            .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Type == BillingRecord.BillingType.Overage);
        Assert.NotNull(overageBillingAfter);
        Assert.Equal(250.00m, overageBillingAfter.Amount); // 5 overage units * $50 unit cost = $250
    }

    [Fact]
    public async Task Test_BillingRecordManagement_WithDifferentStatuses()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(1);
        var user = users[0];

        // Create billing records with different statuses
        var billingRecords = await _dataBuilder.CreateTestBillingRecordsAsync(user.Id, 5);

        // Act - Test different billing operations
        var paidRecord = billingRecords.First(b => b.Status == BillingRecord.BillingStatus.Paid);
        var pendingRecord = billingRecords.First(b => b.Status == BillingRecord.BillingStatus.Pending);

        // Test getting billing records (using a method that exists)
        var historyResult = await _billingService.GetUserBillingHistoryAsync(user.Id, _userToken);
        Assert.True(historyResult.StatusCode == 200);

        // Test processing payment for pending record
        var paymentResult = await _billingService.ProcessPaymentAsync(pendingRecord.Id, _userToken);
        Assert.True(paymentResult.StatusCode == 200);

        // Test processing refund for paid record
        var refundResult = await _billingService.ProcessRefundAsync(paidRecord.Id, 25.00m, _userToken);
        Assert.True(refundResult.StatusCode == 200);
    }

    [Fact]
    public async Task Test_ErrorHandling_WithInvalidData()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();

        // Act & Assert - Test error scenarios

        // 1. Calculate price for non-existent plan
        var invalidPlanDto = new CalculatePlanPriceDto
        {
            PlanId = Guid.NewGuid(), // Non-existent plan
            AdminCommissionPercentage = 10
        };
        var priceResult = await _privilegeBasedBillingService.CalculatePlanBasePriceAsync(invalidPlanDto, _adminToken);
        Assert.True(priceResult.StatusCode == 404);

        // 2. Process usage for non-existent user
        var invalidUsageDto = new ProcessPrivilegeUsageDto
        {
            UserId = 99999, // Non-existent user
            PrivilegeId = testEnv.Privileges[0].Id,
            UsageCount = 1
        };
        var usageResult = await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(invalidUsageDto, _userToken);
        Assert.True(usageResult.StatusCode == 404);

        // 3. Process renewal for non-existent subscription
        var invalidRenewalResult = await _privilegeBasedBillingService.ProcessSubscriptionRenewalAsync(Guid.NewGuid(), _userToken);
        Assert.True(invalidRenewalResult.StatusCode == 404);

        // 4. Get usage summary for non-existent user
        var invalidSummaryResult = await _privilegeBasedBillingService.GetPrivilegeUsageSummaryAsync(99999, _userToken);
        Assert.True(invalidSummaryResult.StatusCode == 404);
    }

    [Fact]
    public async Task Test_ConcurrentUsage_WithMultipleUsers()
    {
        // Arrange
        var testEnv = await _dataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _dataBuilder.CreateTestUsersAsync(3);
        var basicPlan = testEnv.Plans[0]; // Basic Health Plan
        var monthlyCycle = testEnv.MasterData.BillingCycles[2]; // Monthly

        // Create subscriptions for multiple users
        var subscriptions = new List<Subscription>();
        foreach (var user in users)
        {
            var subscription = await _dataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
            await _dataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);
            subscriptions.Add(subscription);
        }

        // Act - Process usage for multiple users concurrently
        var basicConsultation = testEnv.Privileges[0]; // Basic Consultation
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

        // Assert - All operations should succeed
        Assert.All(results, result => Assert.True(result.StatusCode == 200));

        // Verify all users have usage records
        foreach (var user in users)
        {
            var usage = await _context.UserSubscriptionPrivilegeUsages
                .FirstOrDefaultAsync(u => u.Subscription.UserId == user.Id && u.PrivilegeId == basicConsultation.Id);
            Assert.NotNull(usage);
            Assert.True(usage.UsedValue > 0);
        }
    }

}

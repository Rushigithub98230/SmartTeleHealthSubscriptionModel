using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

public interface IAutomatedBillingService
{
    // Core billing methods
    Task ProcessRecurringBillingAsync(TokenModel tokenModel);
    Task ProcessSubscriptionRenewalAsync(TokenModel tokenModel);
    Task ProcessFailedPaymentRetryAsync(TokenModel tokenModel);
    Task ProcessPlanChangeAsync(Guid subscriptionId, Guid newPlanId, TokenModel tokenModel);
    Task ProcessManualBillingAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<PaymentResultDto> ProcessPaymentAsync(Guid subscriptionId, decimal amount, TokenModel tokenModel);
    Task<bool> ValidateBillingCycleAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<DateTime> CalculateNextBillingDateAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<decimal> CalculateProratedAmountAsync(Guid subscriptionId, DateTime effectiveDate, TokenModel tokenModel);
    
    // Billing cycle management methods
    Task ProcessBillingCycleAsync(Guid billingCycleId, TokenModel tokenModel);
    Task ProcessBillingForDateAsync(DateTime billingDate, TokenModel tokenModel);
    Task ProcessAllFailedPaymentRetriesAsync(TokenModel tokenModel);
    Task ProcessAllSubscriptionRenewalsAsync(TokenModel tokenModel);
    Task<BillingStatistics> GetBillingStatisticsAsync(DateTime startDate, DateTime endDate, TokenModel tokenModel);
    
    // Overage processing methods
    Task<decimal> CalculateOverageChargeAsync(Subscription subscription);
    Task<Guid?> CreateOverageBillingRecordAsync(Subscription subscription, decimal overageAmount, TokenModel tokenModel);
    Task<bool> ProcessOverageChargesAsync(Subscription subscription, TokenModel tokenModel);
    Task<int> GetActualUsageForPrivilegeAsync(Guid subscriptionId, Guid privilegeId);
}

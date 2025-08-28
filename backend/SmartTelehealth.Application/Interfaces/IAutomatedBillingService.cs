using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IAutomatedBillingService
{
    Task ProcessRecurringBillingAsync(TokenModel tokenModel);
    Task ProcessSubscriptionRenewalAsync(TokenModel tokenModel);
    Task ProcessFailedPaymentRetryAsync(TokenModel tokenModel);
    Task ProcessPlanChangeAsync(Guid subscriptionId, Guid newPlanId, TokenModel tokenModel);
    Task ProcessManualBillingAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<PaymentResultDto> ProcessPaymentAsync(Guid subscriptionId, decimal amount, TokenModel tokenModel);
    Task<bool> ValidateBillingCycleAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<DateTime> CalculateNextBillingDateAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<decimal> CalculateProratedAmountAsync(Guid subscriptionId, DateTime effectiveDate, TokenModel tokenModel);
}

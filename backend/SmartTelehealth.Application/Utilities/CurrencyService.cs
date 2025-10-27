using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Utilities;

/// <summary>
/// Centralized currency service to ensure consistent currency handling across the application.
/// SINGLE SOURCE OF TRUTH for all currency operations.
/// </summary>
public class CurrencyService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(ISubscriptionRepository subscriptionRepository, ILogger<CurrencyService> logger)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the currency code for a given currency ID with consistent fallback logic.
    /// </summary>
    /// <param name="currencyId">Currency ID to resolve</param>
    /// <returns>Currency code in lowercase format for Stripe compatibility</returns>
    public async Task<string> GetCurrencyCodeAsync(Guid currencyId)
    {
        try
        {
            var currency = await _subscriptionRepository.GetCurrencyByIdAsync(currencyId);
            var currencyCode = currency?.Code?.ToLower() ?? "usd";
            
            _logger.LogDebug("Resolved currency {CurrencyId} to code {CurrencyCode}", currencyId, currencyCode);
            return currencyCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving currency {CurrencyId}, falling back to USD", currencyId);
            return "usd";
        }
    }

    /// <summary>
    /// Gets the currency code for a subscription plan.
    /// </summary>
    /// <param name="plan">Subscription plan to get currency for</param>
    /// <returns>Currency code in lowercase format</returns>
    public async Task<string> GetCurrencyCodeForPlanAsync(SubscriptionPlan plan)
    {
        if (plan?.CurrencyId == null)
        {
            _logger.LogWarning("Plan {PlanId} has no currency ID, using USD fallback", plan?.Id);
            return "usd";
        }

        return await GetCurrencyCodeAsync(plan.CurrencyId);
    }

    /// <summary>
    /// Validates that a currency code is supported by the system.
    /// </summary>
    /// <param name="currencyCode">Currency code to validate</param>
    /// <returns>True if supported, false otherwise</returns>
    public static bool IsSupportedCurrency(string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return false;

        var normalized = currencyCode.Trim().ToLower();
        var supportedCurrencies = new[]
        {
            "usd", "eur", "gbp", "cad", "aud", "jpy", "chf", "sek", "nok", "dkk",
            "pln", "czk", "huf", "ron", "bgn", "hrk", "rsd", "mkd", "all", "bam"
        };

        return supportedCurrencies.Contains(normalized);
    }

    /// <summary>
    /// Gets the number of decimal places for a currency (for proper rounding).
    /// </summary>
    /// <param name="currencyCode">Currency code</param>
    /// <returns>Number of decimal places</returns>
    public static int GetDecimalPlaces(string currencyCode)
    {
        var normalized = currencyCode?.Trim().ToLower() ?? "usd";
        
        // Currencies with no decimal places
        var noDecimalCurrencies = new[] { "jpy", "krw", "vnd", "idr" };
        
        return noDecimalCurrencies.Contains(normalized) ? 0 : 2;
    }

    /// <summary>
    /// Rounds an amount according to currency-specific rules.
    /// </summary>
    /// <param name="amount">Amount to round</param>
    /// <param name="currencyCode">Currency code</param>
    /// <returns>Rounded amount</returns>
    public static decimal RoundAmount(decimal amount, string currencyCode)
    {
        var decimalPlaces = GetDecimalPlaces(currencyCode);
        return Math.Round(amount, decimalPlaces, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Formats an amount according to currency-specific rules.
    /// </summary>
    /// <param name="amount">Amount to format</param>
    /// <param name="currencyCode">Currency code</param>
    /// <returns>Formatted amount string</returns>
    public static string FormatAmount(decimal amount, string currencyCode)
    {
        var roundedAmount = RoundAmount(amount, currencyCode);
        var decimalPlaces = GetDecimalPlaces(currencyCode);
        
        return decimalPlaces == 0 
            ? roundedAmount.ToString("F0") 
            : roundedAmount.ToString($"F{decimalPlaces}");
    }
}


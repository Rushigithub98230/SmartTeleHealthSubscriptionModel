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
        try
        {
            if (plan?.CurrencyId == null)
            {
                _logger.LogWarning("Plan has no currency ID, falling back to USD");
                return "usd";
            }

            return await GetCurrencyCodeAsync(plan.CurrencyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting currency code for plan {PlanId}, falling back to USD", plan?.Id);
            return "usd";
        }
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
        var supportedCurrencies = new[] { "usd", "eur", "gbp", "cad", "aud", "jpy", "chf", "sek", "nok", "dkk", "inr", "brl", "mxn", "cny" };
        
        return supportedCurrencies.Contains(normalized);
    }

    /// <summary>
    /// Gets the default currency code for the system.
    /// </summary>
    /// <returns>Default currency code</returns>
    public static string GetDefaultCurrency()
    {
        return "usd";
    }

    /// <summary>
    /// Converts an amount from one currency to another using exchange rates.
    /// Note: This is a simplified implementation. In production, you would integrate with a real exchange rate API.
    /// </summary>
    /// <param name="amount">Amount to convert</param>
    /// <param name="fromCurrency">Source currency code</param>
    /// <param name="toCurrency">Target currency code</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>Converted amount</returns>
    public static decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency, ILogger? logger = null)
    {
        try
        {
            if (amount <= 0)
            {
                logger?.LogDebug("Non-positive amount provided for currency conversion");
                return 0;
            }

            var normalizedFrom = fromCurrency?.Trim().ToLower() ?? "usd";
            var normalizedTo = toCurrency?.Trim().ToLower() ?? "usd";

            // If same currency, return original amount
            if (normalizedFrom == normalizedTo)
            {
                logger?.LogDebug("Same currency conversion, returning original amount");
                return amount;
            }

            // Validate currencies are supported
            if (!IsSupportedCurrency(normalizedFrom) || !IsSupportedCurrency(normalizedTo))
            {
                logger?.LogWarning("Unsupported currency conversion from {From} to {To}, returning original amount", normalizedFrom, normalizedTo);
                return amount;
            }

            // Simplified exchange rates (in production, use real-time rates)
            var exchangeRates = new Dictionary<string, decimal>
            {
                { "usd", 1.0m },
                { "eur", 0.85m },
                { "gbp", 0.73m },
                { "cad", 1.25m },
                { "aud", 1.35m },
                { "jpy", 110.0m },
                { "chf", 0.92m },
                { "sek", 8.5m },
                { "nok", 8.8m },
                { "dkk", 6.3m },
                { "inr", 75.0m },
                { "brl", 5.2m },
                { "mxn", 20.0m },
                { "cny", 6.4m }
            };

            var fromRate = exchangeRates.GetValueOrDefault(normalizedFrom, 1.0m);
            var toRate = exchangeRates.GetValueOrDefault(normalizedTo, 1.0m);

            // Convert to USD first, then to target currency
            var usdAmount = amount / fromRate;
            var convertedAmount = usdAmount * toRate;

            logger?.LogInformation("Currency conversion: {Amount} {From} = {Converted} {To} (via USD)", 
                amount, normalizedFrom.ToUpper(), convertedAmount, normalizedTo.ToUpper());

            return Math.Round(convertedAmount, 2); // Round to 2 decimal places
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error converting currency from {From} to {To}, returning original amount", fromCurrency, toCurrency);
            return amount; // Safe fallback
        }
    }

    /// <summary>
    /// Formats a currency amount with proper symbol and decimal places.
    /// </summary>
    /// <param name="amount">Amount to format</param>
    /// <param name="currencyCode">Currency code</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>Formatted currency string</returns>
    public static string FormatCurrency(decimal amount, string currencyCode, ILogger? logger = null)
    {
        try
        {
            var normalized = currencyCode?.Trim().ToLower() ?? "usd";
            
            var currencySymbols = new Dictionary<string, string>
            {
                { "usd", "$" },
                { "eur", "€" },
                { "gbp", "£" },
                { "cad", "C$" },
                { "aud", "A$" },
                { "jpy", "¥" },
                { "chf", "CHF" },
                { "sek", "kr" },
                { "nok", "kr" },
                { "dkk", "kr" },
                { "inr", "₹" },
                { "brl", "R$" },
                { "mxn", "$" },
                { "cny", "¥" }
            };

            var symbol = currencySymbols.GetValueOrDefault(normalized, "$");
            var formattedAmount = amount.ToString("F2");
            
            // For JPY, don't show decimal places
            if (normalized == "jpy")
            {
                formattedAmount = amount.ToString("F0");
            }

            return $"{symbol}{formattedAmount}";
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error formatting currency {Amount} for code {Code}", amount, currencyCode);
            return $"${amount:F2}"; // Safe fallback to USD format
        }
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


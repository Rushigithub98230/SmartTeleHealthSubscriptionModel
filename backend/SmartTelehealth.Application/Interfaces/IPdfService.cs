using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IPdfService
{
    /// <summary>
    /// Generates an invoice PDF for a billing record
    /// </summary>
    /// <param name="billingRecord">The billing record to generate invoice for</param>
    /// <param name="user">The user information</param>
    /// <param name="subscription">Optional subscription information</param>
    /// <returns>PDF as byte array</returns>
    Task<byte[]> GenerateInvoicePdfAsync(BillingRecordDto billingRecord, UserDto user, SubscriptionDto? subscription = null);
    
    /// <summary>
    /// Generates a subscription summary PDF
    /// </summary>
    /// <param name="subscription">The subscription to generate summary for</param>
    /// <param name="user">The user information</param>
    /// <returns>PDF as byte array</returns>
    Task<byte[]> GenerateSubscriptionSummaryPdfAsync(SubscriptionDto subscription, UserDto user);
    
    /// <summary>
    /// Generates a billing history PDF
    /// </summary>
    /// <param name="billingRecords">List of billing records</param>
    /// <param name="user">The user information</param>
    /// <returns>PDF as byte array</returns>
    Task<byte[]> GenerateBillingHistoryPdfAsync(IEnumerable<BillingRecordDto> billingRecords, UserDto user);
} 
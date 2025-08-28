using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IInvoiceService
{
    /// <summary>
    /// Generate an invoice for a billing record
    /// </summary>
    Task<JsonModel> GenerateInvoiceAsync(string billingRecordId, TokenModel tokenModel);

    /// <summary>
    /// Get invoice details by invoice number
    /// </summary>
    Task<JsonModel> GetInvoiceAsync(string invoiceNumber, TokenModel tokenModel);

    /// <summary>
    /// Get all invoices for a user with pagination
    /// </summary>
    Task<JsonModel> GetUserInvoicesAsync(int userId, int page = 1, int pageSize = 20, TokenModel tokenModel = null);

    /// <summary>
    /// Download invoice in specified format (PDF/CSV)
    /// </summary>
    Task<JsonModel> DownloadInvoiceAsync(string invoiceNumber, string format, TokenModel tokenModel);

    /// <summary>
    /// Send invoice to specified email address
    /// </summary>
    Task<JsonModel> SendInvoiceAsync(string invoiceNumber, string email, TokenModel tokenModel);
}

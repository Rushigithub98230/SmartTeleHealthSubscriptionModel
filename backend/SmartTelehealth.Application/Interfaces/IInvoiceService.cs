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
    
    // ===== PHASE 4: INVOICE MANAGEMENT ENHANCEMENTS =====
    
    /// <summary>
    /// Get all invoices with filtering and pagination (Admin only)
    /// Phase 4: Admin dashboard invoice management
    /// </summary>
    Task<JsonModel> GetAllInvoicesAsync(int page, int pageSize, string? status, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    
    /// <summary>
    /// Regenerate an invoice if billing details changed
    /// Phase 4: Invoice correction and updates
    /// </summary>
    Task<JsonModel> RegenerateInvoiceAsync(string invoiceNumber, TokenModel tokenModel);
    
    /// <summary>
    /// Get invoice statistics for admin dashboard
    /// Phase 4: Dashboard analytics
    /// </summary>
    Task<JsonModel> GetInvoiceStatsAsync(TokenModel tokenModel);
    
    /// <summary>
    /// Bulk send multiple invoices
    /// Phase 4: Batch operations for invoice delivery
    /// </summary>
    Task<JsonModel> BulkSendInvoicesAsync(BulkSendInvoicesRequestDto request, TokenModel tokenModel);
}

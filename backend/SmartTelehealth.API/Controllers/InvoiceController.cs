using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive invoice management and processing.
/// This controller provides extensive functionality for generating, managing, and processing
/// invoices including invoice creation, retrieval, download, and delivery. It handles the
/// complete invoice lifecycle from generation to delivery and payment tracking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class InvoiceController : BaseController
{
    private readonly IInvoiceService _invoiceService;

    /// <summary>
    /// Initializes a new instance of the InvoiceController with the required invoice service.
    /// </summary>
    /// <param name="invoiceService">Service for handling invoice-related business logic</param>
    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    /// <summary>
    /// Generates an invoice for a specific billing record.
    /// This endpoint creates a new invoice based on billing record information including
    /// invoice details, line items, totals, and invoice metadata for billing and payment processing.
    /// </summary>
    /// <param name="billingRecordId">The unique identifier of the billing record</param>
    /// <returns>JsonModel containing the generated invoice information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Generates invoice from billing record information
    /// - Creates invoice with line items, totals, and metadata
    /// - Sets up invoice for payment processing and delivery
    /// - Access restricted to authenticated users
    /// - Used for invoice generation and billing management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on invoice generation
    /// - Maintains invoice audit trails and generation history
    /// </remarks>
    [HttpPost("generate/{billingRecordId}")]
    public async Task<JsonModel> GenerateInvoice(string billingRecordId)
    {
        return await _invoiceService.GenerateInvoiceAsync(billingRecordId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves detailed information about a specific invoice by its invoice number.
    /// This endpoint provides comprehensive invoice details including invoice content,
    /// line items, totals, payment status, and invoice metadata for authorized users.
    /// </summary>
    /// <param name="invoiceNumber">The unique invoice number</param>
    /// <returns>JsonModel containing the invoice details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed invoice information by invoice number
    /// - Includes invoice content, line items, and payment status
    /// - Shows invoice metadata and billing information
    /// - Access restricted to invoice owners and authorized users
    /// - Used for invoice details and management
    /// - Includes comprehensive invoice information and metadata
    /// - Provides secure access to invoice information
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("{invoiceNumber}")]
    public async Task<JsonModel> GetInvoice(string invoiceNumber)
    {
        return await _invoiceService.GetInvoiceAsync(invoiceNumber, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all invoices for a specific user with pagination support.
    /// This endpoint provides a paginated list of invoices associated with a user,
    /// including invoice details, status, and payment information for invoice management.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20)</param>
    /// <returns>JsonModel containing paginated user invoices</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns paginated invoices for the specified user
    /// - Includes invoice details, status, and payment information
    /// - Provides pagination for large invoice datasets
    /// - Access restricted to invoice owners and authorized users
    /// - Used for user invoice history and management
    /// - Includes comprehensive invoice information and metadata
    /// - Provides secure access to user invoice data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserInvoices(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _invoiceService.GetUserInvoicesAsync(userId, page, pageSize, GetToken(HttpContext));
    }

    /// <summary>
    /// Downloads an invoice in the specified format (PDF/CSV).
    /// This endpoint generates and downloads an invoice file in the requested format
    /// including PDF for printing and CSV for data processing and analysis.
    /// </summary>
    /// <param name="invoiceNumber">The unique invoice number</param>
    /// <param name="format">Download format (pdf, csv) (default: pdf)</param>
    /// <returns>JsonModel containing the download information or file data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Downloads invoice in specified format (PDF/CSV)
    /// - Generates formatted invoice file for download
    /// - Provides secure file download with proper headers
    /// - Access restricted to invoice owners and authorized users
    /// - Used for invoice download and file generation
    /// - Includes comprehensive validation and error handling
    /// - Provides secure file download functionality
    /// - Maintains download audit trails and access history
    /// </remarks>
    [HttpGet("{invoiceNumber}/download")]
    public async Task<JsonModel> DownloadInvoice(string invoiceNumber, [FromQuery] string format = "pdf")
    {
        return await _invoiceService.DownloadInvoiceAsync(invoiceNumber, format, GetToken(HttpContext));
    }

    /// <summary>
    /// Sends an invoice to a specified email address.
    /// This endpoint delivers an invoice to the recipient's email address including
    /// invoice attachment, delivery confirmation, and email tracking for invoice delivery.
    /// </summary>
    /// <param name="invoiceNumber">The unique invoice number</param>
    /// <param name="request">DTO containing email delivery details</param>
    /// <returns>JsonModel containing the delivery result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Sends invoice to specified email address
    /// - Includes invoice attachment and delivery confirmation
    /// - Tracks email delivery and recipient confirmation
    /// - Access restricted to invoice owners and authorized users
    /// - Used for invoice delivery and email management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on delivery operations
    /// - Maintains delivery audit trails and email history
    /// </remarks>
    [HttpPost("{invoiceNumber}/send")]
    public async Task<JsonModel> SendInvoice(string invoiceNumber, [FromBody] SendInvoiceRequest request)
    {
        return await _invoiceService.SendInvoiceAsync(invoiceNumber, request.Email, GetToken(HttpContext));
    }
}

public class SendInvoiceRequest
{
    public string Email { get; set; } = string.Empty;
}

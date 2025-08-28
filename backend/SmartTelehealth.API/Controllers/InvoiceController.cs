using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoiceController : BaseController
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    /// <summary>
    /// Generate an invoice for a billing record
    /// </summary>
    [HttpPost("generate/{billingRecordId}")]
    public async Task<JsonModel> GenerateInvoice(string billingRecordId)
    {
        return await _invoiceService.GenerateInvoiceAsync(billingRecordId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get invoice details by invoice number
    /// </summary>
    [HttpGet("{invoiceNumber}")]
    public async Task<JsonModel> GetInvoice(string invoiceNumber)
    {
        return await _invoiceService.GetInvoiceAsync(invoiceNumber, GetToken(HttpContext));
    }

    /// <summary>
    /// Get all invoices for a user with pagination
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserInvoices(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _invoiceService.GetUserInvoicesAsync(userId, page, pageSize, GetToken(HttpContext));
    }

    /// <summary>
    /// Download invoice in specified format (PDF/CSV)
    /// </summary>
    [HttpGet("{invoiceNumber}/download")]
    public async Task<JsonModel> DownloadInvoice(string invoiceNumber, [FromQuery] string format = "pdf")
    {
        return await _invoiceService.DownloadInvoiceAsync(invoiceNumber, format, GetToken(HttpContext));
    }

    /// <summary>
    /// Send invoice to specified email address
    /// </summary>
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

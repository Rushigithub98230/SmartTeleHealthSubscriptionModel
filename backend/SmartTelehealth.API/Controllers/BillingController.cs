using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : BaseController
{
    private readonly IBillingService _billingService;
    private readonly IPdfService _pdfService;
    private readonly IUserService _userService;
    private readonly ISubscriptionService _subscriptionService;

    public BillingController(
        IBillingService billingService, 
        IPdfService pdfService,
        IUserService userService,
        ISubscriptionService subscriptionService)
    {
        _billingService = billingService;
        _pdfService = pdfService;
        _userService = userService;
        _subscriptionService = subscriptionService;
    }



    [HttpGet]
    [HttpGet("records")]
    [AllowAnonymous]
    public async Task<JsonModel> GetAllBillingRecords(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string[]? status = null,
        [FromQuery] string[]? type = null,
        [FromQuery] string[]? userId = null,
        [FromQuery] string[]? subscriptionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? format = null,
        [FromQuery] bool? includeFailed = null)
    {
        // If format is specified, return export data
        if (!string.IsNullOrEmpty(format) && (format.ToLower() == "csv" || format.ToLower() == "excel"))
        {
            return await _billingService.ExportBillingRecordsAsync(GetToken(HttpContext), page, pageSize, searchTerm, status, type, userId, subscriptionId, startDate, endDate, sortBy, sortOrder, format);
        }
        
        // If includeFailed is true, add failed status to the status array
        if (includeFailed == true && status != null)
        {
            var statusList = status.ToList();
            if (!statusList.Contains("Failed"))
            {
                statusList.Add("Failed");
                status = statusList.ToArray();
            }
        }
        
        return await _billingService.GetAllBillingRecordsAsync(page, pageSize, searchTerm, status, type, userId, subscriptionId, startDate, endDate, sortBy, sortOrder, GetToken(HttpContext));
    }

    /// <summary>
    /// Download invoice PDF for a billing record
    /// </summary>
    [HttpGet("{id}/invoice-pdf")]
    public async Task<JsonModel> DownloadInvoicePdf(Guid id)
    {
        return await _billingService.GenerateInvoicePdfAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get billing record by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetBillingRecord(Guid id)
    {
        return await _billingService.GetBillingRecordAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get user billing history
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserBillingHistory(int userId)
    {
        return await _billingService.GetUserBillingHistoryAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription billing history
    /// </summary>
    [HttpGet("subscription/{subscriptionId}")]
    public async Task<JsonModel> GetSubscriptionBillingHistory(Guid subscriptionId)
    {
        return await _billingService.GetSubscriptionBillingHistoryAsync(subscriptionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Create a new billing record
    /// </summary>
    [HttpPost]
    public async Task<JsonModel> CreateBillingRecord([FromBody] CreateBillingRecordDto createDto)
    {
        return await _billingService.CreateBillingRecordAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Process payment for a billing record
    /// </summary>
    [HttpPost("{id}/process-payment")]
    public async Task<JsonModel> ProcessPayment(Guid id)
    {
        return await _billingService.ProcessPaymentAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Process refund for a billing record
    /// </summary>
    [HttpPost("{id}/process-refund")]
    public async Task<JsonModel> ProcessRefund(Guid id, [FromBody] RefundRequestDto refundRequest)
    {
        return await _billingService.ProcessRefundAsync(id, refundRequest.Amount, refundRequest.Reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Calculate total amount including tax and shipping
    /// </summary>
    [HttpPost("calculate-total")]
    public async Task<JsonModel> CalculateTotal([FromBody] BillingCalculationRequestDto request)
    {
        return await _billingService.CalculateTotalAmountAsync(request.BaseAmount, 0, 0, GetToken(HttpContext));
    }

    /// <summary>
    /// Calculate tax amount for a given base amount and state
    /// </summary>
    [HttpPost("calculate-tax")]
    public async Task<JsonModel> CalculateTax([FromBody] TaxCalculationRequestDto request)
    {
        return await _billingService.CalculateTaxAmountAsync(request.BaseAmount, request.State, GetToken(HttpContext));
    }

    /// <summary>
    /// Calculate shipping amount
    /// </summary>
    [HttpPost("calculate-shipping")]
    public async Task<JsonModel> CalculateShipping([FromBody] ShippingCalculationRequestDto request)
    {
        return await _billingService.CalculateShippingAmountAsync(request.DeliveryAddress, request.IsExpress, GetToken(HttpContext));
    }

    /// <summary>
    /// Check if payment is overdue
    /// </summary>
    [HttpGet("{id}/overdue-status")]
    public async Task<JsonModel> CheckOverdueStatus(Guid id)
    {
        return await _billingService.IsPaymentOverdueAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Calculate due date based on billing date and grace period
    /// </summary>
    [HttpPost("calculate-due-date")]
    public async Task<JsonModel> CalculateDueDate([FromBody] DueDateCalculationRequestDto request)
    {
        return await _billingService.CalculateDueDateAsync(request.BillingDate, request.GracePeriodDays, GetToken(HttpContext));
    }

    /// <summary>
    /// Get billing analytics
    /// </summary>
    [HttpGet("analytics")]
    public async Task<JsonModel> GetBillingAnalytics()
    {
        return await _billingService.GetBillingAnalyticsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Create recurring billing
    /// </summary>
    [HttpPost("recurring")]
    public async Task<JsonModel> CreateRecurringBilling([FromBody] CreateRecurringBillingDto createDto)
    {
        return await _billingService.CreateRecurringBillingAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Process recurring payment
    /// </summary>
    [HttpPost("recurring/{subscriptionId}/process")]
    public async Task<JsonModel> ProcessRecurringPayment(Guid subscriptionId)
    {
        return await _billingService.ProcessRecurringPaymentAsync(subscriptionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Cancel recurring billing
    /// </summary>
    [HttpPost("recurring/{subscriptionId}/cancel")]
    public async Task<JsonModel> CancelRecurringBilling(Guid subscriptionId)
    {
        return await _billingService.CancelRecurringBillingAsync(subscriptionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Create upfront payment
    /// </summary>
    [HttpPost("upfront")]
    public async Task<JsonModel> CreateUpfrontPayment([FromBody] CreateUpfrontPaymentDto createDto)
    {
        return await _billingService.CreateUpfrontPaymentAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Process bundle payment
    /// </summary>
    [HttpPost("bundle")]
    public async Task<JsonModel> ProcessBundlePayment([FromBody] CreateBundlePaymentDto createDto)
    {
        return await _billingService.ProcessBundlePaymentAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Apply billing adjustment
    /// </summary>
    [HttpPost("{id}/adjustments")]
    public async Task<JsonModel> ApplyBillingAdjustment(Guid id, [FromBody] CreateBillingAdjustmentDto adjustmentDto)
    {
        return await _billingService.ApplyBillingAdjustmentAsync(id, adjustmentDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get billing adjustments
    /// </summary>
    [HttpGet("{id}/adjustments")]
    public async Task<JsonModel> GetBillingAdjustments(Guid id)
    {
        return await _billingService.GetBillingAdjustmentsAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retry failed payment
    /// </summary>
    [HttpPost("{id}/retry-failed")]
    public async Task<JsonModel> RetryFailedPayment(Guid id)
    {
        return await _billingService.RetryFailedPaymentAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retry payment
    /// </summary>
    [HttpPost("{id}/retry")]
    public async Task<JsonModel> RetryPayment(Guid id)
    {
        return await _billingService.RetryPaymentAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Process partial payment
    /// </summary>
    [HttpPost("{id}/partial-payment")]
    public async Task<JsonModel> ProcessPartialPayment(Guid id, [FromBody] PartialPaymentRequestDto request)
    {
        return await _billingService.ProcessPartialPaymentAsync(id, request.Amount, GetToken(HttpContext));
    }

    /// <summary>
    /// Create invoice
    /// </summary>
    [HttpPost("invoice")]
    public async Task<JsonModel> CreateInvoice([FromBody] CreateInvoiceDto createDto)
    {
        return await _billingService.CreateInvoiceAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate billing report
    /// </summary>
    [HttpGet("report")]
    public async Task<JsonModel> GenerateBillingReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "pdf")
    {
        return await _billingService.GenerateBillingReportAsync(startDate, endDate, format, GetToken(HttpContext));
    }

    /// <summary>
    /// Get billing summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<JsonModel> GetBillingSummary([FromQuery] int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _billingService.GetBillingSummaryAsync(userId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get payment schedule
    /// </summary>
    [HttpGet("schedule/{subscriptionId}")]
    public async Task<JsonModel> GetPaymentSchedule(Guid subscriptionId)
    {
        return await _billingService.GetPaymentScheduleAsync(subscriptionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Update payment method
    /// </summary>
    [HttpPut("{id}/payment-method")]
    public async Task<JsonModel> UpdatePaymentMethod(Guid id, [FromBody] UpdatePaymentMethodRequestDto request)
    {
        return await _billingService.UpdatePaymentMethodAsync(id, request.PaymentMethodId, GetToken(HttpContext));
    }

    /// <summary>
    /// Create billing cycle
    /// </summary>
    [HttpPost("cycle")]
    public async Task<JsonModel> CreateBillingCycle([FromBody] CreateBillingCycleDto createDto)
    {
        return await _billingService.CreateBillingCycleAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Process billing cycle
    /// </summary>
    [HttpPost("cycle/{id}/process")]
    public async Task<JsonModel> ProcessBillingCycle(Guid id)
    {
        return await _billingService.ProcessBillingCycleAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get billing cycle records
    /// </summary>
    [HttpGet("cycle/{id}/records")]
    public async Task<JsonModel> GetBillingCycleRecords(Guid id)
    {
        return await _billingService.GetBillingCycleRecordsAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get all pending payments (Admin only)
    /// </summary>
    [HttpGet("pending")]
    public async Task<JsonModel> GetPendingPayments()
    {
        return await _billingService.GetPendingPaymentsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get all overdue billing records (Admin only)
    /// </summary>
    [HttpGet("overdue")]
    public async Task<JsonModel> GetOverdueBillingRecords()
    {
        return await _billingService.GetOverdueBillingRecordsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get revenue summary for admin reporting (accrual and cash)
    /// </summary>
    [HttpGet("revenue-summary")]
    public async Task<JsonModel> GetRevenueSummary([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? planId = null)
    {
        return await _billingService.GetRevenueSummaryAsync(from, to, planId, GetToken(HttpContext));
    }

    /// <summary>
    /// Export revenue/financial data for admin (CSV/Excel)
    /// </summary>
    [HttpGet("export-revenue")]
    public async Task<JsonModel> ExportRevenue([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? planId = null, [FromQuery] string format = "csv")
    {
        return await _billingService.ExportRevenueAsync(from, to, planId, format, GetToken(HttpContext));
    }

    /// <summary>
    /// Get payment history
    /// </summary>
    [HttpGet("payment-history")]
    public async Task<JsonModel> GetPaymentHistory([FromQuery] int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _billingService.GetPaymentHistoryAsync(userId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get payment analytics
    /// </summary>
    [HttpGet("payment-analytics")]
    public async Task<JsonModel> GetPaymentAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _billingService.GetPaymentAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get user payment analytics
    /// </summary>
    [HttpGet("payment-analytics/{userId}")]
    public async Task<JsonModel> GetUserPaymentAnalytics(int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _billingService.GetPaymentAnalyticsAsync(userId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate invoice for a billing record
    /// </summary>
    [HttpPost("{id}/generate-invoice")]
    public async Task<JsonModel> GenerateInvoice(Guid id)
    {
        return await _billingService.GenerateInvoiceAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get invoice by invoice number
    /// </summary>
    [HttpGet("invoice/{invoiceNumber}")]
    public async Task<JsonModel> GetInvoice(string invoiceNumber)
    {
        return await _billingService.GetInvoiceAsync(invoiceNumber, GetToken(HttpContext));
    }

    /// <summary>
    /// Update invoice status
    /// </summary>
    [HttpPut("invoice/{invoiceNumber}/status")]
    public async Task<JsonModel> UpdateInvoiceStatus(string invoiceNumber, [FromBody] UpdateInvoiceStatusRequestDto request)
    {
        return await _billingService.UpdateInvoiceStatusAsync(invoiceNumber, request.Status, GetToken(HttpContext));
    }
}

// DTOs for the controller
public class RefundRequestDto
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class PartialPaymentRequestDto
{
    public decimal Amount { get; set; }
}

public class TaxCalculationRequestDto
{
    public decimal BaseAmount { get; set; }
    public string State { get; set; } = string.Empty;
}

public class ShippingCalculationRequestDto
{
    public string DeliveryAddress { get; set; } = string.Empty;
    public bool IsExpress { get; set; }
}

public class DueDateCalculationRequestDto
{
    public DateTime BillingDate { get; set; }
    public int GracePeriodDays { get; set; }
}

public class UpdatePaymentMethodRequestDto
{
    public string PaymentMethodId { get; set; } = string.Empty;
}

public class BillingCalculationRequestDto
{
    public decimal BaseAmount { get; set; }
    public string State { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public bool IsExpress { get; set; }
}

public class BillingCalculationDto
{
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OverdueStatusDto
{
    public Guid BillingRecordId { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? DueDate { get; set; }
    public int DaysOverdue { get; set; }
}

public class UpdateInvoiceStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
} 
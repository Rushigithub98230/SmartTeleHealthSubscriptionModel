using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for managing billing records, invoices, and payment processing.
/// This controller provides comprehensive billing functionality including creating billing records,
/// processing payments, handling refunds, generating invoices, and managing billing cycles.
/// It integrates with Stripe for payment processing and provides detailed billing analytics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class BillingController : BaseController
{
    private readonly IBillingService _billingService;
    private readonly IPdfService _pdfService;
    private readonly IUserService _userService;
    private readonly ISubscriptionService _subscriptionService;

    /// <summary>
    /// Initializes a new instance of the BillingController with required services.
    /// </summary>
    /// <param name="billingService">Service for handling billing-related business logic</param>
    /// <param name="pdfService">Service for generating PDF documents and invoices</param>
    /// <param name="userService">Service for user management operations</param>
    /// <param name="subscriptionService">Service for subscription management operations</param>
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



    /// <summary>
    /// Retrieves all billing records with comprehensive filtering and pagination options.
    /// This endpoint provides access to billing records with advanced filtering capabilities
    /// including status, type, user, subscription, date range, and export functionality.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 10)</param>
    /// <param name="searchTerm">Search term to filter records by ID, user, or subscription</param>
    /// <param name="status">Array of billing statuses to filter by (Pending, Paid, Failed, etc.)</param>
    /// <param name="type">Array of billing types to filter by (Subscription, Consultation, etc.)</param>
    /// <param name="userId">Array of user IDs to filter by</param>
    /// <param name="subscriptionId">Array of subscription IDs to filter by</param>
    /// <param name="startDate">Start date for date range filtering</param>
    /// <param name="endDate">End date for date range filtering</param>
    /// <param name="sortBy">Field to sort by (createdDate, amount, status)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="format">Export format (csv, excel) - returns export data instead of paginated results</param>
    /// <param name="includeFailed">Include failed billing records in the results</param>
    /// <returns>JsonModel containing paginated billing records or export data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Supports comprehensive filtering by multiple criteria
    /// - Provides pagination for large datasets
    /// - Supports data export in CSV or Excel format
    /// - Includes failed records when requested
    /// - Access restricted to administrators only
    /// - Used for billing management and financial reporting
    /// - Returns detailed billing information with audit trails
    /// </remarks>
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
    /// Generates and downloads an invoice PDF for a specific billing record.
    /// This endpoint creates a professional PDF invoice containing billing details,
    /// payment information, and company branding for the specified billing record.
    /// </summary>
    /// <param name="id">The unique identifier of the billing record</param>
    /// <returns>JsonModel containing the PDF file data or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Generates a professional PDF invoice
    /// - Includes billing details, payment information, and company branding
    /// - Access restricted to billing record owner or administrators
    /// - Used for invoice generation and download functionality
    /// - Returns PDF file data for direct download
    /// - Includes tax calculations and payment terms
    /// </remarks>
    [HttpGet("{id}/invoice-pdf")]
    public async Task<JsonModel> DownloadInvoicePdf(Guid id)
    {
        return await _billingService.GenerateInvoicePdfAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves detailed information about a specific billing record.
    /// This endpoint returns comprehensive billing record details including
    /// payment status, amounts, dates, and associated subscription information.
    /// </summary>
    /// <param name="id">The unique identifier of the billing record to retrieve</param>
    /// <returns>JsonModel containing the billing record details or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed billing record information
    /// - Includes payment status, amounts, and transaction details
    /// - Shows associated subscription and user information
    /// - Access restricted to billing record owner or administrators
    /// - Used for billing record details and payment tracking
    /// - Provides complete audit trail of billing activities
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetBillingRecord(Guid id)
    {
        return await _billingService.GetBillingRecordAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves the complete billing history for a specific user.
    /// This endpoint returns all billing records associated with the specified user,
    /// providing a comprehensive view of their payment history and billing activities.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>JsonModel containing the user's billing history or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all billing records for the specified user
    /// - Includes payment history, amounts, and status information
    /// - Shows subscription-related billing and standalone charges
    /// - Access restricted to the user themselves or administrators
    /// - Used for user billing history and payment tracking
    /// - Provides chronological view of all billing activities
    /// </remarks>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserBillingHistory(int userId)
    {
        return await _billingService.GetUserBillingHistoryAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves the billing history for a specific subscription.
    /// This endpoint returns all billing records associated with the specified subscription,
    /// providing a detailed view of subscription-related billing and payment activities.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription</param>
    /// <returns>JsonModel containing the subscription's billing history or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all billing records for the specified subscription
    /// - Includes recurring billing, upgrades, and adjustments
    /// - Shows payment history and billing cycle information
    /// - Access restricted to subscription owner or administrators
    /// - Used for subscription billing tracking and management
    /// - Provides detailed view of subscription-related financial activities
    /// </remarks>
    [HttpGet("subscription/{subscriptionId}")]
    public async Task<JsonModel> GetSubscriptionBillingHistory(Guid subscriptionId)
    {
        return await _billingService.GetSubscriptionBillingHistoryAsync(subscriptionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new billing record in the system.
    /// This endpoint allows creation of billing records for various types of charges
    /// including subscription fees, consultation fees, and other service charges.
    /// </summary>
    /// <param name="createDto">DTO containing the billing record creation details</param>
    /// <returns>JsonModel containing the creation result and new billing record information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new billing record with specified details
    /// - Supports various billing types (subscription, consultation, etc.)
    /// - Sets up payment processing and due dates
    /// - Access restricted to administrators or authorized users
    /// - Used for manual billing creation and charge management
    /// - Includes validation of billing details and business rules
    /// - Sets up audit trails and administrative tracking
    /// </remarks>
    [HttpPost]
    public async Task<JsonModel> CreateBillingRecord([FromBody] CreateBillingRecordDto createDto)
    {
        return await _billingService.CreateBillingRecordAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Processes payment for a specific billing record.
    /// This endpoint handles payment processing through Stripe, updating billing status,
    /// and managing payment confirmations and notifications.
    /// </summary>
    /// <param name="id">The unique identifier of the billing record to process payment for</param>
    /// <returns>JsonModel containing the payment processing result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Processes payment through Stripe payment gateway
    /// - Updates billing record status based on payment result
    /// - Handles payment failures and retry logic
    /// - Sends payment confirmation notifications
    /// - Access restricted to billing record owner or administrators
    /// - Used for manual payment processing and payment retries
    /// - Includes comprehensive payment validation and security checks
    /// </remarks>
    [HttpPost("{id}/process-payment")]
    public async Task<JsonModel> ProcessPayment(Guid id)
    {
        return await _billingService.ProcessPaymentAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Processes a refund for a specific billing record.
    /// This endpoint handles refund processing through Stripe, updating billing status,
    /// and managing refund confirmations and notifications.
    /// </summary>
    /// <param name="id">The unique identifier of the billing record to process refund for</param>
    /// <param name="refundRequest">DTO containing refund amount and reason</param>
    /// <returns>JsonModel containing the refund processing result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Processes refund through Stripe payment gateway
    /// - Updates billing record status and refund information
    /// - Handles partial and full refunds
    /// - Records refund reason for audit purposes
    /// - Access restricted to administrators only
    /// - Used for refund processing and customer service
    /// - Includes refund validation and business rule checks
    /// </remarks>
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
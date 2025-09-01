using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/payments")]
//[Authorize]
public class PaymentController : BaseController
{
    private readonly IStripeService _stripeService;
    private readonly IBillingService _billingService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IAuditService _auditService;
    private readonly IPaymentSecurityService _paymentSecurityService;

    public PaymentController(
        IStripeService stripeService,
        IBillingService billingService,
        ISubscriptionService subscriptionService,
        IAuditService auditService,
        IPaymentSecurityService paymentSecurityService)
    {
        _stripeService = stripeService;
        _billingService = billingService;
        _subscriptionService = subscriptionService;
        _auditService = auditService;
        _paymentSecurityService = paymentSecurityService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<JsonModel> GetAllPayments()
    {
        var result = await GetPaymentHistory();
        return new JsonModel { data = result.data, Message = "All payments retrieved successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Get all payment methods for the current user
    /// </summary>
    [HttpGet("payment-methods")]
    public async Task<JsonModel> GetPaymentMethods()
    {
        var userId = GetCurrentUserId();
        var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString(), GetToken(HttpContext));
        return new JsonModel { data = paymentMethods, Message = "Payment methods retrieved successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Add a new payment method for the current user
    /// </summary>
    [HttpPost("payment-methods")]
    public async Task<JsonModel> AddPaymentMethod([FromBody] AddPaymentMethodDto request)
    {
        var userId = GetCurrentUserId();
        
        // Validate payment method
        var validationResult = await _stripeService.ValidatePaymentMethodAsync(request.PaymentMethodId, GetToken(HttpContext));
        if (!validationResult)
        {
            return new JsonModel { data = new object(), Message = "Invalid payment method", StatusCode = 400 };
        }

        // Add payment method to customer
        var paymentMethodId = await _stripeService.AddPaymentMethodAsync(userId.ToString(), request.PaymentMethodId, GetToken(HttpContext));
        
        // Get the payment method details
        var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString(), GetToken(HttpContext));
        var paymentMethod = paymentMethods.FirstOrDefault(pm => pm.Id == paymentMethodId);
        
        if (paymentMethod == null)
        {
            return new JsonModel { data = new object(), Message = "Failed to retrieve payment method details", StatusCode = 400 };
        }
        
        // Log the action
        await _auditService.LogUserActionAsync(userId, "AddPaymentMethod", "PaymentMethod", paymentMethod.Id, "Payment method added successfully", GetToken(HttpContext));
        
        return new JsonModel { data = paymentMethod, Message = "Payment method added successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Set a payment method as default for the current user
    /// </summary>
    [HttpPut("payment-methods/{paymentMethodId}/default")]
    public async Task<JsonModel> SetDefaultPaymentMethod(string paymentMethodId)
    {
        var userId = GetCurrentUserId();
        var result = await _stripeService.SetDefaultPaymentMethodAsync(userId.ToString(), paymentMethodId, GetToken(HttpContext));
        
        if (result)
        {
            await _auditService.LogUserActionAsync(userId, "SetDefaultPaymentMethod", "PaymentMethod", paymentMethodId, "Default payment method updated", GetToken(HttpContext));
            return new JsonModel { data = true, Message = "Default payment method updated", StatusCode = 200 };
        }
        
        return new JsonModel { data = new object(), Message = "Failed to set default payment method", StatusCode = 400 };
    }

    /// <summary>
    /// Remove a payment method for the current user
    /// </summary>
    [HttpDelete("payment-methods/{paymentMethodId}")]
    public async Task<JsonModel> RemovePaymentMethod(string paymentMethodId)
    {
        var userId = GetCurrentUserId();
        var result = await _stripeService.RemovePaymentMethodAsync(userId.ToString(), paymentMethodId, GetToken(HttpContext));
        
        if (result)
        {
            await _auditService.LogUserActionAsync(userId, "RemovePaymentMethod", "PaymentMethod", paymentMethodId, "Payment method removed", GetToken(HttpContext));
            return new JsonModel { data = true, Message = "Payment method removed", StatusCode = 200 };
        }
        
        return new JsonModel { data = new object(), Message = "Failed to remove payment method", StatusCode = 400 };
    }

    /// <summary>
    /// Process a payment for a billing record
    /// </summary>
    [HttpPost("process-payment")]
    public async Task<JsonModel> ProcessPayment([FromBody] ProcessPaymentRequestDto request)
    {
        var userId = GetCurrentUserId();
        var ipAddress = GetClientIpAddress();
        
        // Validate billing record exists and belongs to user
        var billingRecord = await _billingService.GetBillingRecordAsync(request.BillingRecordId, GetToken(HttpContext));
        if (billingRecord.StatusCode != 200 || billingRecord.data == null)
        {
            return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 400 };
        }

        if (((BillingRecordDto)billingRecord.data).UserId != userId)
        {
            await _auditService.LogSecurityEventAsync(userId, "PaymentAccessDenied", 
                $"User {userId} attempted to access billing record {request.BillingRecordId} belonging to {((BillingRecordDto)billingRecord.data).UserId}", ipAddress, GetToken(HttpContext));
            return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
        }

        // Security validation
        if (!await _paymentSecurityService.ValidatePaymentRequestAsync(userId.ToString(), ipAddress, ((BillingRecordDto)billingRecord.data).Amount, GetToken(HttpContext)))
        {
            return new JsonModel { data = new object(), Message = "Payment request validation failed", StatusCode = 400 };
        }

        // Process payment
        var result = await _billingService.ProcessPaymentAsync(request.BillingRecordId, GetToken(HttpContext));
        
        // Log payment attempt
        await _paymentSecurityService.LogPaymentAttemptAsync(
            userId.ToString(), 
            ipAddress, 
            ((BillingRecordDto)billingRecord.data).Amount, 
            result.StatusCode == 200, 
            result.StatusCode == 200 ? null : result.Message,
            GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            await _auditService.LogPaymentEventAsync(userId, "PaymentProcessed", request.BillingRecordId.ToString(), "Success", null, GetToken(HttpContext));
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Retry a failed payment
    /// </summary>
    [HttpPost("retry-payment/{billingRecordId}")]
    public async Task<JsonModel> RetryPayment(Guid billingRecordId)
    {
        var userId = GetCurrentUserId();
        
        // Validate billing record
        var billingRecord = await _billingService.GetBillingRecordAsync(billingRecordId, GetToken(HttpContext));
        if (billingRecord.StatusCode != 200 || billingRecord.data == null)
        {
            return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 400 };
        }

        if (((BillingRecordDto)billingRecord.data).UserId != userId)
        {
            return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
        }

        // Retry payment with exponential backoff
        var result = await _billingService.RetryPaymentAsync(billingRecordId, GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            await _auditService.LogPaymentEventAsync(userId, "PaymentRetried", billingRecordId.ToString(), "Success", null, GetToken(HttpContext));
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Process a refund for a billing record
    /// </summary>
    [HttpPost("refund/{billingRecordId}")]
    public async Task<JsonModel> ProcessRefund(Guid billingRecordId, [FromBody] RefundRequestDto request)
    {
        var userId = GetCurrentUserId();
        
        // Validate billing record
        var billingRecord = await _billingService.GetBillingRecordAsync(billingRecordId, GetToken(HttpContext));
        if (billingRecord.StatusCode != 200 || billingRecord.data == null)
        {
            return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 400 };
        }

        if (((BillingRecordDto)billingRecord.data).UserId != userId)
        {
            return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
        }

        // Process refund
        var result = await _billingService.ProcessRefundAsync(billingRecordId, request.Amount, request.Reason, GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            await _auditService.LogPaymentEventAsync(userId, "RefundProcessed", billingRecordId.ToString(), "Success", request.Reason, GetToken(HttpContext));
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Get payment history for the current user
    /// </summary>
    [HttpGet("history")]
    public async Task<JsonModel> GetPaymentHistory([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();
        var history = await _billingService.GetPaymentHistoryAsync(userId, startDate, endDate, GetToken(HttpContext));
        return history;
    }

    /// <summary>
    /// Validate a payment method
    /// </summary>
    [HttpPost("validate-payment-method")]
    public async Task<JsonModel> ValidatePaymentMethod([FromBody] ValidatePaymentMethodDto request)
    {
        var validationResult = await _stripeService.ValidatePaymentMethodDetailedAsync(request.PaymentMethodId, GetToken(HttpContext));
        return new JsonModel { data = validationResult, Message = "Payment method validation completed", StatusCode = 200 };
    }

    /// <summary>
    /// Get payment analytics for the current user
    /// </summary>
    [HttpGet("analytics")]
    public async Task<JsonModel> GetPaymentAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = GetCurrentUserId();
        var analytics = await _billingService.GetPaymentAnalyticsAsync(userId, startDate, endDate, GetToken(HttpContext));
        return analytics;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }

    private string GetClientIpAddress()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }
        
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        return remoteIp ?? "unknown";
    }
}

 
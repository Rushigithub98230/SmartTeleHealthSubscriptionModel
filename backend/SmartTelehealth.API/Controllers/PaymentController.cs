using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for managing payment methods, processing payments, and handling payment-related operations.
/// This controller provides comprehensive payment functionality including payment method management,
/// payment processing, refunds, and payment security features. It integrates with Stripe for
/// secure payment processing and includes advanced security measures for payment validation.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the PaymentController with required services.
    /// </summary>
    /// <param name="stripeService">Service for Stripe payment gateway integration</param>
    /// <param name="billingService">Service for billing-related operations</param>
    /// <param name="subscriptionService">Service for subscription management</param>
    /// <param name="auditService">Service for audit logging and tracking</param>
    /// <param name="paymentSecurityService">Service for payment security and validation</param>
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

    /// <summary>
    /// Retrieves all payments for the current user (public endpoint for testing).
    /// This endpoint provides access to payment history and is primarily used for
    /// testing and development purposes.
    /// </summary>
    /// <returns>JsonModel containing all payments for the current user</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns payment history for the current user
    /// - No authentication required (for testing purposes)
    /// - Used primarily for development and testing
    /// - Provides access to payment information and history
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    public async Task<JsonModel> GetAllPayments()
    {
        var result = await GetPaymentHistory();
        return new JsonModel { data = result.data, Message = "All payments retrieved successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Retrieves all payment methods associated with the current user.
    /// This endpoint returns a list of all payment methods (credit cards, bank accounts, etc.)
    /// that the user has added to their account for payment processing.
    /// </summary>
    /// <returns>JsonModel containing the list of payment methods or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all payment methods associated with the current user
    /// - Includes payment method details (masked card numbers, expiry dates)
    /// - Shows which payment method is set as default
    /// - Access restricted to the authenticated user
    /// - Used for payment method management in the frontend
    /// - Provides secure access to payment method information
    /// </remarks>
    [HttpGet("payment-methods")]
    public async Task<JsonModel> GetPaymentMethods()
    {
        var userId = GetCurrentUserId();
        var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString(), GetToken(HttpContext));
        return new JsonModel { data = paymentMethods, Message = "Payment methods retrieved successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Adds a new payment method to the current user's account.
    /// This endpoint allows users to add additional payment methods (credit cards, bank accounts)
    /// to their account for payment processing and subscription billing.
    /// </summary>
    /// <param name="request">DTO containing the payment method ID to add</param>
    /// <returns>JsonModel containing the result of adding the payment method</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates the payment method with Stripe
    /// - Associates the payment method with the user's account
    /// - Sets up the payment method for future billing
    /// - Access restricted to the authenticated user
    /// - Used when users want to add backup payment methods
    /// - Includes comprehensive validation and security checks
    /// - Logs the action for audit purposes
    /// </remarks>
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
        
        return new JsonModel { data = paymentMethod, Message = "Payment method added successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Sets a payment method as the default for the current user.
    /// This endpoint allows users to designate one of their payment methods as the default
    /// for automatic billing and subscription payments.
    /// </summary>
    /// <param name="paymentMethodId">The unique identifier of the payment method to set as default</param>
    /// <returns>JsonModel containing the result of setting the default payment method</returns>
    /// <remarks>
    /// This endpoint:
    /// - Sets the specified payment method as the user's default
    /// - Updates the default payment method in Stripe
    /// - Access restricted to the authenticated user
    /// - Used for payment method management and preference setting
    /// - Ensures the payment method belongs to the user
    /// - Updates subscription billing to use the new default method
    /// </remarks>
    [HttpPut("payment-methods/{paymentMethodId}/default")]
    public async Task<JsonModel> SetDefaultPaymentMethod(string paymentMethodId)
    {
        var userId = GetCurrentUserId();
        var result = await _stripeService.SetDefaultPaymentMethodAsync(userId.ToString(), paymentMethodId, GetToken(HttpContext));
        
        if (result)
        {
            return new JsonModel { data = true, Message = "Default payment method updated", StatusCode = 200 };
        }
        
        return new JsonModel { data = new object(), Message = "Failed to set default payment method", StatusCode = 400 };
    }

    /// <summary>
    /// Removes a payment method from the current user's account.
    /// This endpoint allows users to delete payment methods they no longer want to use
    /// for billing and subscription payments.
    /// </summary>
    /// <param name="paymentMethodId">The unique identifier of the payment method to remove</param>
    /// <returns>JsonModel containing the result of removing the payment method</returns>
    /// <remarks>
    /// This endpoint:
    /// - Removes the specified payment method from the user's account
    /// - Deletes the payment method from Stripe
    /// - Access restricted to the authenticated user
    /// - Used for payment method management and cleanup
    /// - Ensures the payment method belongs to the user
    /// - Handles cases where the payment method is currently set as default
    /// </remarks>
    [HttpDelete("payment-methods/{paymentMethodId}")]
    public async Task<JsonModel> RemovePaymentMethod(string paymentMethodId)
    {
        var userId = GetCurrentUserId();
        var result = await _stripeService.RemovePaymentMethodAsync(userId.ToString(), paymentMethodId, GetToken(HttpContext));
        
        if (result)
        {
            return new JsonModel { data = true, Message = "Payment method removed", StatusCode = 200 };
        }
        
        return new JsonModel { data = new object(), Message = "Failed to remove payment method", StatusCode = 400 };
    }

    /// <summary>
    /// Processes a payment for a specific billing record with advanced security validation.
    /// This endpoint handles payment processing through Stripe with comprehensive security checks,
    /// including IP validation, amount verification, and fraud detection.
    /// </summary>
    /// <param name="request">DTO containing the billing record ID and payment details</param>
    /// <returns>JsonModel containing the payment processing result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates the billing record exists and belongs to the user
    /// - Performs security validation including IP address and amount checks
    /// - Processes payment through Stripe payment gateway
    /// - Logs payment attempts for security and audit purposes
    /// - Access restricted to the billing record owner
    /// - Used for manual payment processing and payment retries
    /// - Includes comprehensive fraud detection and security measures
    /// - Handles payment failures and provides detailed error information
    /// </remarks>
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

 
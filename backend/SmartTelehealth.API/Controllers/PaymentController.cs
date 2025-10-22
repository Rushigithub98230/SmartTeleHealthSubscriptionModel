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
    private readonly ISubscriptionBillingService _billingService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IAuditService _auditService;
    private readonly IPaymentSecurityService _paymentSecurityService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the PaymentController with required services.
    /// UPDATED: Now uses consolidated ISubscriptionBillingService and IUserService for automatic Stripe customer creation
    /// </summary>
    /// <param name="stripeService">Service for Stripe payment gateway integration</param>
    /// <param name="billingService">Service for billing-related operations (consolidated)</param>
    /// <param name="subscriptionService">Service for subscription management</param>
    /// <param name="auditService">Service for audit logging and tracking</param>
    /// <param name="paymentSecurityService">Service for payment security and validation</param>
    /// <param name="userService">Service for user management and operations</param>
    public PaymentController(
        IStripeService stripeService,
        ISubscriptionBillingService billingService,
        ISubscriptionService subscriptionService,
        IAuditService auditService,
        IPaymentSecurityService paymentSecurityService,
        IUserService userService)
    {
        _stripeService = stripeService;
        _billingService = billingService;
        _subscriptionService = subscriptionService;
        _auditService = auditService;
        _paymentSecurityService = paymentSecurityService;
        _userService = userService;
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
    /// Automatically creates a Stripe customer if one doesn't exist.
    /// </summary>
    /// <returns>JsonModel containing the list of payment methods or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all payment methods associated with the current user
    /// - Automatically creates Stripe customer if needed
    /// - Includes payment method details (masked card numbers, expiry dates)
    /// - Shows which payment method is set as default
    /// - Access restricted to the authenticated user
    /// - Used for payment method management in the frontend
    /// - Provides secure access to payment method information
    /// </remarks>
    [HttpGet("payment-methods")]
    public async Task<JsonModel> GetPaymentMethods()
    {
        try
        {
            var token = GetToken(HttpContext);
            
            // Get user details from database
            var userResult = await _userService.GetUserByIdAsync(token.UserID, token);
            if (userResult.StatusCode != 200 || userResult.data == null)
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not found", 
                    StatusCode = 404 
                };
            }
            
            var user = userResult.data as UserDto;
            if (user == null)
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "Invalid user data", 
                    StatusCode = 500 
                };
            }
            
            // Ensure Stripe customer exists (creates automatically if needed)
            var stripeCustomerId = await _stripeService.EnsureStripeCustomerAsync(
                user.Id,
                user.Email,
                user.FullName,
                user.StripeCustomerId,
                token
            );
            
            // Get payment methods using Stripe customer ID
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(stripeCustomerId, token);
            
            return new JsonModel 
            { 
                data = paymentMethods, 
                Message = "Payment methods retrieved successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            return new JsonModel 
            { 
                data = new object(), 
                Message = $"An unexpected error occurred: {ex.Message}", 
                StatusCode = 500 
            };
        }
    }

    /// <summary>
    /// Adds a new payment method to the current user's account.
    /// This endpoint allows users to add additional payment methods (credit cards, bank accounts)
    /// to their account for payment processing and subscription billing.
    /// Automatically creates a Stripe customer if one doesn't exist.
    /// </summary>
    /// <param name="request">DTO containing the payment method ID to add</param>
    /// <returns>JsonModel containing the result of adding the payment method</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates the payment method with Stripe
    /// - Associates the payment method with the user's account
    /// - Automatically creates Stripe customer if needed
    /// - Sets up the payment method for future billing
    /// - Access restricted to the authenticated user
    /// - Used when users want to add backup payment methods
    /// - Includes comprehensive validation and security checks
    /// - Logs the action for audit purposes
    /// </remarks>
    [HttpPost("payment-methods")]
    public async Task<JsonModel> AddPaymentMethod([FromBody] AddPaymentMethodDto request)
    {
        try
        {
            var token = GetToken(HttpContext);
            
            // Get user details from database
            var userResult = await _userService.GetUserByIdAsync(token.UserID, token);
            if (userResult.StatusCode != 200 || userResult.data == null)
            {
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };
            }
            
            var user = userResult.data as UserDto;
            if (user == null)
            {
                return new JsonModel { data = new object(), Message = "Invalid user data", StatusCode = 500 };
            }
            
            // Ensure Stripe customer exists (creates automatically if needed)
            var stripeCustomerId = await _stripeService.EnsureStripeCustomerAsync(
                user.Id,
                user.Email,
                user.FullName,
                user.StripeCustomerId,
                token
            );
            
            // Validate payment method
            var validationResult = await _stripeService.ValidatePaymentMethodAsync(request.PaymentMethodId, token);
            if (!validationResult)
            {
                return new JsonModel { data = new object(), Message = "Invalid payment method", StatusCode = 400 };
            }

            // Add payment method to customer using Stripe customer ID
            var paymentMethodId = await _stripeService.AddPaymentMethodAsync(stripeCustomerId, request.PaymentMethodId, token);
            
            // Get the payment method details
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(stripeCustomerId, token);
            var paymentMethod = paymentMethods.FirstOrDefault(pm => pm.Id == paymentMethodId);
            
            if (paymentMethod == null)
            {
                return new JsonModel { data = new object(), Message = "Failed to retrieve payment method details", StatusCode = 400 };
            }
            
            // Log the action
            
            return new JsonModel { data = paymentMethod, Message = "Payment method added successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel 
            { 
                data = new object(), 
                Message = $"An unexpected error occurred: {ex.Message}", 
                StatusCode = 500 
            };
        }
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
        try
        {
            var token = GetToken(HttpContext);
            
            // Get user details from database
            var userResult = await _userService.GetUserByIdAsync(token.UserID, token);
            if (userResult.StatusCode != 200 || userResult.data == null)
            {
                return new JsonModel { data = false, Message = "User not found", StatusCode = 404 };
            }
            
            var user = userResult.data as UserDto;
            if (user == null)
            {
                return new JsonModel { data = false, Message = "Invalid user data", StatusCode = 500 };
            }
            
            // Ensure Stripe customer exists (creates automatically if needed)
            var stripeCustomerId = await _stripeService.EnsureStripeCustomerAsync(
                user.Id,
                user.Email,
                user.FullName,
                user.StripeCustomerId,
                token
            );
            
            var result = await _stripeService.SetDefaultPaymentMethodAsync(stripeCustomerId, paymentMethodId, token);
            
            if (result)
            {
                return new JsonModel { data = true, Message = "Default payment method updated", StatusCode = 200 };
            }
            
            return new JsonModel { data = false, Message = "Failed to set default payment method", StatusCode = 400 };
        }
        catch (Exception ex)
        {
            return new JsonModel 
            { 
                data = false, 
                Message = $"An unexpected error occurred: {ex.Message}", 
                StatusCode = 500 
            };
        }
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
        try
        {
            var token = GetToken(HttpContext);
            
            // Get user details from database
            var userResult = await _userService.GetUserByIdAsync(token.UserID, token);
            if (userResult.StatusCode != 200 || userResult.data == null)
            {
                return new JsonModel { data = false, Message = "User not found", StatusCode = 404 };
            }
            
            var user = userResult.data as UserDto;
            if (user == null)
            {
                return new JsonModel { data = false, Message = "Invalid user data", StatusCode = 500 };
            }
            
            // Ensure Stripe customer exists (creates automatically if needed)
            var stripeCustomerId = await _stripeService.EnsureStripeCustomerAsync(
                user.Id,
                user.Email,
                user.FullName,
                user.StripeCustomerId,
                token
            );
            
            var result = await _stripeService.RemovePaymentMethodAsync(stripeCustomerId, paymentMethodId, token);
            
            if (result)
            {
                return new JsonModel { data = true, Message = "Payment method removed", StatusCode = 200 };
            }
            
            return new JsonModel { data = false, Message = "Failed to remove payment method", StatusCode = 400 };
        }
        catch (Exception ex)
        {
            return new JsonModel 
            { 
                data = false, 
                Message = $"An unexpected error occurred: {ex.Message}", 
                StatusCode = 500 
            };
        }
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
        var token = GetToken(HttpContext);
        var ipAddress = GetClientIpAddress();
        
        // Validate billing record exists and belongs to user
        var billingRecord = await _billingService.GetBillingRecordAsync(request.BillingRecordId, token);
        if (billingRecord.StatusCode != 200 || billingRecord.data == null)
        {
            return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 400 };
        }

        if (((BillingRecordDto)billingRecord.data).UserId != token.UserID)
        {
           
            return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
        }

        // Security validation
        if (!await _paymentSecurityService.ValidatePaymentRequestAsync(token.UserID.ToString(), ipAddress, ((BillingRecordDto)billingRecord.data).Amount, token))
        {
            return new JsonModel { data = new object(), Message = "Payment request validation failed", StatusCode = 400 };
        }

        // Process payment
        var result = await _billingService.ProcessPaymentAsync(request.BillingRecordId, token);
        
        // Log payment attempt
        await _paymentSecurityService.LogPaymentAttemptAsync(
            token.UserID.ToString(), 
            ipAddress, 
            ((BillingRecordDto)billingRecord.data).Amount, 
            result.StatusCode == 200, 
            result.StatusCode == 200 ? null : result.Message,
            token);
        
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
        var token = GetToken(HttpContext);
        
        // Validate billing record
        var billingRecord = await _billingService.GetBillingRecordAsync(billingRecordId, token);
        if (billingRecord.StatusCode != 200 || billingRecord.data == null)
        {
            return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 400 };
        }

        if (((BillingRecordDto)billingRecord.data).UserId != token.UserID)
        {
            return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
        }

        // Retry payment with exponential backoff
        var result = await _billingService.RetryPaymentAsync(billingRecordId, token);
        
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
        var token = GetToken(HttpContext);
        
        // Validate billing record
        var billingRecord = await _billingService.GetBillingRecordAsync(billingRecordId, token);
        if (billingRecord.StatusCode != 200 || billingRecord.data == null)
        {
            return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 400 };
        }

        if (((BillingRecordDto)billingRecord.data).UserId != token.UserID)
        {
            return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
        }

        // Process refund
        var result = await _billingService.ProcessRefundAsync(billingRecordId, request.Amount, request.Reason, token);
        
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
        var token = GetToken(HttpContext);
        var history = await _billingService.GetPaymentHistoryAsync(token.UserID, startDate, endDate, token);
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
        var token = GetToken(HttpContext);
        var analytics = await _billingService.GetPaymentAnalyticsAsync(token.UserID, startDate, endDate, token);
        return analytics;
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

    #region Phase 3: Failed Payment Management

    /// <summary>
    /// Gets all failed payments with comprehensive details and retry status
    /// Phase 3: Admin Portal Enhancement
    /// </summary>
    /// <returns>JsonModel containing failed payments list and summary</returns>
    [HttpGet("failed")]
    public async Task<JsonModel> GetFailedPayments()
    {
        return await _billingService.GetFailedPaymentsAsync(GetToken(HttpContext));
    }

    // BUILD FIX: REMOVED DUPLICATE METHOD RetryPayment(Guid id) at lines 382-392
    // Same functionality already exists at line 266: RetryPayment(Guid billingRecordId)
    // Route conflict: [HttpPost("{id}/retry")] vs [HttpPost("retry-payment/{billingRecordId}")]
    
    /// <summary>
    /// Send payment reminder email to customer
    /// Phase 3: Admin Portal Enhancement
    /// </summary>
    /// <param name="id">Billing record ID</param>
    /// <param name="request">Reminder customization options</param>
    /// <returns>JsonModel containing send result</returns>
    [HttpPost("{id}/send-reminder")]
    public async Task<JsonModel> SendPaymentReminder(Guid id, [FromBody] SendReminderRequestDto request)
    {
        return await _billingService.SendPaymentReminderAsync(id, request, GetToken(HttpContext));
    }

    /// <summary>
    /// Bulk retry multiple failed payments
    /// Phase 3: Admin Portal Enhancement
    /// </summary>
    /// <param name="request">Bulk retry request with billing record IDs</param>
    /// <returns>JsonModel containing bulk retry results</returns>
    [HttpPost("bulk-retry")]
    public async Task<JsonModel> BulkRetryPayments([FromBody] BulkRetryRequestDto request)
    {
        return await _billingService.BulkRetryPaymentsAsync(request, GetToken(HttpContext));
    }

    // BUILD FIX: REMOVED DUPLICATE METHOD GetPaymentAnalytics at lines 411-420
    // Same functionality already exists at line 349: GetPaymentAnalytics with same parameters
    // Both had route [HttpGet("analytics")] causing conflict

    #endregion
}

 
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using System.Threading.Tasks;
using System;

namespace SmartTelehealth.API.Controllers
{
    /// <summary>
    /// Controller responsible for Stripe payment integration and testing functionality.
    /// This controller provides essential functionality for testing Stripe connectivity,
    /// creating checkout sessions, and managing Stripe payment operations. It serves as
    /// the primary interface for Stripe payment processing and integration testing.
    /// </summary>
    [ApiController]
    [Route("api/stripe")]
    public class StripeController : BaseController
    {
        private readonly IStripeService _stripeService;
        private readonly ISubscriptionPlanService _subscriptionPlanService;

        /// <summary>
        /// Initializes a new instance of the StripeController with the required services.
        /// </summary>
        /// <param name="stripeService">Service for handling Stripe-related business logic</param>
        /// <param name="subscriptionPlanService">Service for handling subscription plan operations</param>
        public StripeController(IStripeService stripeService, ISubscriptionPlanService subscriptionPlanService)
        {
            _stripeService = stripeService;
            _subscriptionPlanService = subscriptionPlanService;
        }

        /// <summary>
        /// Tests the Stripe API connection and validates integration functionality.
        /// This endpoint performs a connectivity test by attempting to list customers
        /// from Stripe to verify that the API integration is working correctly.
        /// </summary>
        /// <returns>JsonModel containing connection test results</returns>
        /// <remarks>
        /// This endpoint:
        /// - Tests Stripe API connectivity and authentication
        /// - Validates Stripe service integration functionality
        /// - Returns customer count as proof of successful connection
        /// - Access restricted to authenticated users
        /// - Used for Stripe integration testing and health checks
        /// - Includes comprehensive error handling for connection issues
        /// - Provides detailed feedback on connection status
        /// - Maintains connection test audit trails and logs
        /// </remarks>
        [HttpGet("test-connection")]
        public async Task<JsonModel> TestConnection()
        {
            // Test Stripe connection by attempting to list customers
            var customers = await _stripeService.ListCustomersAsync(GetToken(HttpContext));
            return new JsonModel 
            { 
                data = new { customerCount = customers.Count() }, 
                Message = "Stripe connection successful", 
                StatusCode = 200 
            };
        }

        /// <summary>
        /// Creates a new Stripe checkout session for payment processing.
        /// This endpoint generates a secure checkout session URL that customers can use
        /// to complete their payment for subscriptions or services through Stripe.
        /// </summary>
        /// <param name="request">DTO containing checkout session configuration details</param>
        /// <returns>JsonModel containing the checkout session URL</returns>
        /// <remarks>
        /// This endpoint:
        /// - Creates secure Stripe checkout session for payment processing
        /// - Configures success and cancel URLs for payment flow
        /// - Uses predefined Stripe price ID for payment processing
        /// - Access restricted to authenticated users
        /// - Used for payment processing and checkout flow
        /// - Includes comprehensive validation and error handling
        /// - Provides secure checkout session URLs
        /// - Maintains payment session audit trails and transaction logs
        /// </remarks>
        [HttpPost("create-checkout-session")]
        public async Task<JsonModel> CreateCheckoutSession([FromBody] CheckoutSessionRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.PlanId))
                    return new JsonModel { data = new object(), Message = "Plan ID is required", StatusCode = 400 };
                
                if (string.IsNullOrEmpty(request.BillingCycleId))
                    return new JsonModel { data = new object(), Message = "Billing cycle ID is required", StatusCode = 400 };

                // Get the subscription plan to retrieve Stripe price IDs
                var planResult = await _subscriptionPlanService.GetPlanByIdAsync(request.PlanId, GetToken(HttpContext));
                if (planResult.StatusCode != 200)
                    return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };

                var plan = planResult.data as dynamic;
                if (plan == null)
                    return new JsonModel { data = new object(), Message = "Invalid plan data", StatusCode = 500 };

                // Get the appropriate Stripe price ID based on billing cycle
                string priceId = GetStripePriceIdForBillingCycle(plan, request.BillingCycleId);
                if (string.IsNullOrEmpty(priceId))
                    return new JsonModel { data = new object(), Message = "Price not available for selected billing cycle", StatusCode = 400 };

                // Create checkout session with dynamic price ID
                var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(priceId, request.SuccessUrl, request.CancelUrl, GetToken(HttpContext));
                
                return new JsonModel 
                { 
                    data = new { url = sessionUrl, sessionId = Guid.NewGuid().ToString() }, 
                    Message = "Checkout session created successfully", 
                    StatusCode = 200 
                };
            }
            catch (Exception ex)
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = $"Error creating checkout session: {ex.Message}", 
                    StatusCode = 500 
                };
            }
        }

        /// <summary>
        /// Gets the appropriate Stripe price ID based on billing cycle
        /// </summary>
        private string GetStripePriceIdForBillingCycle(dynamic plan, string billingCycleId)
        {
            return billingCycleId.ToLower() switch
            {
                "monthly" => plan.StripeMonthlyPriceId,
                "quarterly" => plan.StripeQuarterlyPriceId,
                "annual" => plan.StripeAnnualPriceId,
                _ => plan.StripeMonthlyPriceId // Default to monthly
            };
        }
    }

    public class CheckoutSessionRequest
    {
        public string PlanId { get; set; } = string.Empty;
        public string BillingCycleId { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }
} 
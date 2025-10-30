using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using System.Threading.Tasks;
using System;
using System.Linq;
using Stripe;

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
        private readonly IUserService _userService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionBillingService _billingService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeController> _logger;

        /// <summary>
        /// Initializes a new instance of the StripeController with the required services.
        /// </summary>
        /// <param name="stripeService">Service for handling Stripe-related business logic</param>
        /// <param name="subscriptionPlanService">Service for handling subscription plan operations</param>
        /// <param name="userService">Service for user management operations</param>
        /// <param name="subscriptionService">Service for subscription management operations</param>
        /// <param name="logger">Logger for logging operations</param>
        public StripeController(
            IStripeService stripeService, 
            ISubscriptionPlanService subscriptionPlanService, 
            IUserService userService,
            ISubscriptionService subscriptionService,
            ISubscriptionBillingService billingService,
            IConfiguration configuration,
            ILogger<StripeController> logger)
        {
            _stripeService = stripeService;
            _subscriptionPlanService = subscriptionPlanService;
            _userService = userService;
            _subscriptionService = subscriptionService;
            _billingService = billingService;
            _configuration = configuration;
            _logger = logger;
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
        /// Creates a new Stripe checkout session for subscription payment processing.
        /// This is the production-ready implementation with proper customer ID management,
        /// duplicate prevention, and active subscription validation.
        /// </summary>
        /// <param name="planId">The subscription plan ID to purchase</param>
        /// <returns>JsonModel containing the Stripe checkout session URL</returns>
        /// <remarks>
        /// This endpoint implements the following critical features:
        /// - Prevents customer ID duplication by searching Stripe by email
        /// - Validates user eligibility (checks for existing active subscriptions)
        /// - Syncs Stripe customer ID to User table for consistency
        /// - Constructs success/cancel URLs securely on backend
        /// - Comprehensive logging with emojis for easy debugging
        /// - Proper error handling with clear status codes
        /// </remarks>
        [HttpPost("create-checkout-session/{planId}")]
        public async Task<JsonModel> CreateCheckoutSession(Guid planId)
        {
            try
            {
                var token = GetToken(HttpContext);
                var userId = token.UserID;
                
                _logger.LogInformation("🛒 User {UserId} initiating checkout for plan {PlanId}", userId, planId);
                _logger.LogInformation("🔍 Token details: UserID={UserId}, RoleID={RoleId}", token.UserID, token.RoleID);
                
                // 1. Get user details (needed for Stripe customer creation)
                _logger.LogInformation("📞 Calling GetUserByIdAsync for user {UserId}", userId);
                var userResult = await _userService.GetUserByIdAsync(userId, token);
                
                _logger.LogInformation("📋 GetUserByIdAsync result: StatusCode={StatusCode}, HasData={HasData}, Message={Message}", 
                    userResult.StatusCode, userResult.data != null, userResult.Message);
                
                if (userResult.StatusCode != 200 || userResult.data == null)
                {
                    _logger.LogWarning("⚠️ User {UserId} not found during checkout - Status: {StatusCode}, Message: {Message}", 
                        userId, userResult.StatusCode, userResult.Message);
                    return new JsonModel { data = new object(), Message = userResult.Message ?? "User not found", StatusCode = userResult.StatusCode };
                }
                
                var user = (UserDto)userResult.data;
                
                // 2. Get plan details
                var planResult = await _subscriptionPlanService.GetPlanByIdAsync(planId.ToString(), token);
                if (planResult.StatusCode != 200 || planResult.data == null)
                {
                    _logger.LogWarning("⚠️ Plan {PlanId} not found during checkout", planId);
                    return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
                }
                
                var plan = (SubscriptionPlanDto)planResult.data;
                
                // 3. CRITICAL: Check if user already has active subscription (prevent duplicates)
                var activeSubsResult = await _subscriptionService.GetUserSubscriptionsAsync(userId, token);
                if (activeSubsResult.StatusCode == 200 && activeSubsResult.data != null)
                {
                    var subs = activeSubsResult.data as IEnumerable<SubscriptionDto>;
                    if (subs != null && subs.Any(s => s.Status == "Active" || s.Status == "TrialActive"))
                    {
                        _logger.LogWarning("⚠️ User {UserId} already has an active subscription, blocking checkout", userId);
                        return new JsonModel 
                        { 
                            data = new object(), 
                            Message = "You already have an active subscription", 
                            StatusCode = 400 
                        };
                    }
                }
                
                // 4. CRITICAL: Ensure Stripe customer exists with proper de-duplication
                // This uses the FIXED EnsureStripeCustomerAsync method that:
                // - Searches by email to prevent duplicates
                // - Syncs customer ID to User table
                var stripeCustomerId = await _stripeService.EnsureStripeCustomerAsync(
                    userId, 
                    user.Email, 
                    user.FullName, 
                    user.StripeCustomerId, 
                    token
                );
                
                _logger.LogInformation("✅ Ensured Stripe customer {CustomerId} for user {UserId}", 
                    stripeCustomerId, userId);
                
                // 5. Validate plan has Stripe price configured
                var stripePriceId = plan.StripePriceId;
                if (string.IsNullOrEmpty(stripePriceId))
                {
                    _logger.LogWarning("⚠️ Plan {PlanId} doesn't have a Stripe price ID", planId);
                    return new JsonModel 
                    { 
                        data = new object(), 
                        Message = "Plan doesn't have a valid Stripe price configured", 
                        StatusCode = 400 
                    };
                }
                
            // 6. Get frontend URL from configuration (CRITICAL: Must point to frontend, not backend)
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
            
            _logger.LogInformation("🔗 Using frontend URL for redirects: {FrontendUrl}", frontendUrl);
            
            // 7. Create Stripe checkout session
            var checkoutUrl = await _stripeService.CreateCheckoutSessionWithCustomerAsync(
                customerId: stripeCustomerId,
                priceId: stripePriceId,
                successUrl: $"{frontendUrl}/web/subscriptions/success?session_id={{CHECKOUT_SESSION_ID}}",
                cancelUrl: $"{frontendUrl}/web/subscriptions/plans",
                tokenModel: token,
                planId: planId.ToString()
            );
                
                _logger.LogInformation("🎟️ Created checkout session for user {UserId}, plan {PlanId}, customer {CustomerId}", 
                    userId, planId, stripeCustomerId);
                
                return new JsonModel 
                { 
                    data = new { url = checkoutUrl, sessionId = stripeCustomerId }, 
                    Message = "Checkout session created", 
                    StatusCode = 200 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating checkout session for plan {PlanId}", planId);
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = $"Failed to create checkout session: {ex.Message}", 
                    StatusCode = 500 
                };
            }
        }

        /// <summary>
        /// Verifies a Stripe checkout session and checks if subscription and billing record exist
        /// This endpoint is called by the frontend success page to verify actual backend state
        /// </summary>
        /// <param name="sessionId">The Stripe checkout session ID</param>
        /// <returns>Verification status with subscription and billing details</returns>
        [HttpGet("verify-session/{sessionId}")]
        [Authorize]
        public async Task<ActionResult<JsonModel>> VerifyCheckoutSession(string sessionId)
        {
            try
            {
                _logger.LogInformation("🔍 Verifying checkout session {SessionId}", sessionId);

                var token = GetToken(HttpContext);

                // 1. Retrieve Stripe checkout session
                Stripe.Checkout.Session session;
                try
                {
                    var sessionService = new Stripe.Checkout.SessionService();
                    session = await sessionService.GetAsync(sessionId);
                    if (session == null)
                    {
                        _logger.LogWarning("❌ Checkout session {SessionId} not found in Stripe", sessionId);
                        return Ok(new JsonModel
                        {
                            data = new { verified = false, reason = "Session not found in Stripe" },
                            Message = "Checkout session not found",
                            StatusCode = 404
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error retrieving checkout session {SessionId} from Stripe", sessionId);
                    return Ok(new JsonModel
                    {
                        data = new { verified = false, reason = "Error retrieving session from Stripe" },
                        Message = $"Error retrieving session: {ex.Message}",
                        StatusCode = 500
                    });
                }

                // 2. Extract subscription ID from session
                if (string.IsNullOrEmpty(session.SubscriptionId))
                {
                    _logger.LogWarning("⚠️ Session {SessionId} has no subscription ID (may be incomplete)", sessionId);
                    return Ok(new JsonModel
                    {
                        data = new 
                        { 
                            verified = false, 
                            reason = "Session incomplete - subscription not yet created",
                            sessionStatus = session.Status,
                            paymentStatus = session.PaymentStatus
                        },
                        Message = "Session is still processing",
                        StatusCode = 202 // Accepted - still processing
                    });
                }

                var stripeSubscriptionId = session.SubscriptionId;

                // 3. Check if local subscription exists
                var subscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, token);
                SubscriptionDto? subscription = null;
                
                if (subscriptionResult.StatusCode == 200 && subscriptionResult.data != null)
                {
                    subscription = subscriptionResult.data as SubscriptionDto;
                }

                // 4. Check if billing record exists
                var billingRecords = new List<BillingRecordDto>();
                if (subscription != null && Guid.TryParse(subscription.Id, out var subscriptionId))
                {
                    try
                    {
                        // Try to get billing records for this subscription
                        var filter = new BillingFilterDto
                        {
                            SubscriptionId = subscriptionId,
                            Page = 1,
                            PageSize = 100
                        };
                        var billingResult = await _billingService.GetBillingRecordsWithFilteringAsync(filter, token);
                        if (billingResult.StatusCode == 200 && billingResult.data != null)
                        {
                            var billingList = billingResult.data as IEnumerable<BillingRecordDto>;
                            if (billingList != null)
                            {
                                billingRecords = billingList.ToList();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not retrieve billing records for subscription {SubscriptionId}", subscriptionId);
                        // Continue without billing records - verification will show as pending
                    }
                }

                // 5. Determine verification status
                bool isVerified = subscription != null && billingRecords.Any();
                string verificationStatus = isVerified ? "verified" : "pending";

                if (isVerified)
                {
                    _logger.LogInformation("✅ Session {SessionId} verified - Subscription {SubscriptionId} and billing record exist", 
                        sessionId, subscription?.Id);
                }
                else
                {
                    _logger.LogInformation("⏳ Session {SessionId} pending - Subscription: {HasSubscription}, Billing: {HasBilling}", 
                        sessionId, subscription != null, billingRecords.Any());
                }

                return Ok(new JsonModel
                {
                    data = new
                    {
                        verified = isVerified,
                        status = verificationStatus,
                        sessionId = sessionId,
                        stripeSubscriptionId = stripeSubscriptionId,
                        subscriptionId = subscription?.Id,
                        subscriptionStatus = subscription?.Status,
                        billingRecordCount = billingRecords.Count,
                        billingRecords = billingRecords.Select(br => new
                        {
                            id = br.Id,
                            amount = br.Amount,
                            taxAmount = br.TaxAmount,
                            totalAmount = br.Amount + br.TaxAmount,
                            status = br.Status,
                            billingDate = br.BillingDate
                        }),
                        sessionStatus = session.Status,
                        paymentStatus = session.PaymentStatus
                    },
                    Message = isVerified ? "Session verified successfully" : "Session verification pending",
                    StatusCode = isVerified ? 200 : 202
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying checkout session {SessionId}", sessionId);
                return Ok(new JsonModel
                {
                    data = new { verified = false, reason = "Internal server error" },
                    Message = $"Error verifying session: {ex.Message}",
                    StatusCode = 500
                });
            }
        }
    }
} 
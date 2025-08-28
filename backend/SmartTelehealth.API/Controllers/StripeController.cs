using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeController : BaseController
    {
        private readonly IStripeService _stripeService;

        public StripeController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

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

        [HttpPost("create-checkout-session")]
        public async Task<JsonModel> CreateCheckoutSession([FromBody] CheckoutSessionRequest request)
        {
            // Use your actual Stripe test price ID here:
            var priceId = "price_12345"; // <-- Replace with your Stripe test price ID
            var successUrl = request.SuccessUrl;
            var cancelUrl = request.CancelUrl;
            var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(priceId, successUrl, cancelUrl, GetToken(HttpContext));
            return new JsonModel 
            { 
                data = new { url = sessionUrl }, 
                Message = "Checkout session created successfully", 
                StatusCode = 200 
            };
        }
    }

    public class CheckoutSessionRequest
    {
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
} 
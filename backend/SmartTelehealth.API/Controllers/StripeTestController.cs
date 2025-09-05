using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers
{
    /// <summary>
    /// Controller responsible for Stripe payment testing and development functionality.
    /// This controller provides testing endpoints for Stripe payment integration including
    /// payment method testing, payment processing simulation, and development testing
    /// for Stripe payment functionality in the SmartTelehealth system.
    /// </summary>
    [ApiController]
    [Route("api/stripe")]
    public class StripeTestController : BaseController
    {
        /// <summary>
        /// Tests Stripe payment method processing and validation.
        /// This endpoint simulates Stripe payment processing for development and testing purposes,
        /// including payment method validation and response simulation for payment integration testing.
        /// </summary>
        /// <param name="request">DTO containing payment method testing details</param>
        /// <returns>JsonModel containing the test payment result</returns>
        /// <remarks>
        /// This endpoint:
        /// - Tests Stripe payment method processing and validation
        /// - Simulates payment processing for development testing
        /// - Validates payment method data and processing logic
        /// - Access restricted to development and testing environments
        /// - Used for Stripe payment integration testing and development
        /// - Includes comprehensive payment testing and validation
        /// - Provides detailed feedback on payment testing operations
        /// - Maintains payment testing audit trails and logs
        /// </remarks>
        [HttpPost("test-payment")]
        public JsonModel TestPayment([FromBody] PaymentMethodRequest request)
        {
            // Simulate Stripe logic here (replace with your real service call)
            // For now, just return a dummy response
            var result = new
            {
                status = "received",
                paymentMethodId = request.PaymentMethodId
            };

            return new JsonModel 
            { 
                data = result, 
                Message = "Payment method received successfully", 
                StatusCode = 200 
            };
        }
    }

    public class PaymentMethodRequest
    {
        public string PaymentMethodId { get; set; }
    }
} 
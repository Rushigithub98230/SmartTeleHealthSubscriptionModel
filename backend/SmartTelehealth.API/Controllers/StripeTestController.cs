using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeTestController : BaseController
    {
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
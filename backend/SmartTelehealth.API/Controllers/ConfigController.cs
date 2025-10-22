using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.API.Controllers
{
    /// <summary>
    /// Configuration Controller
    /// Provides public configuration values needed by the frontend
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get Stripe publishable key for frontend initialization
        /// This is safe to expose publicly as it's the publishable key, not the secret key
        /// </summary>
        /// <returns>Stripe configuration for frontend</returns>
        [HttpGet("stripe-public")]
        public IActionResult GetStripePublicConfig()
        {
            try
            {
                var publishableKey = _configuration["StripeSettings:PublishableKey"];

                if (string.IsNullOrEmpty(publishableKey))
                {
                    return StatusCode(500, new JsonModel
                    {
                        
                        StatusCode = 500,
                        Message = "Stripe publishable key not configured on server"
                    });
                }

                return Ok(new JsonModel
                {
                    
                    StatusCode = 200,
                    Message = "Stripe configuration retrieved successfully",
                    data = new
                    {
                        publishableKey = publishableKey,
                        // Add other public Stripe config if needed
                        currency = "usd",
                        locale = "en"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new JsonModel
                {
                    
                    StatusCode = 500,
                    Message = $"Error retrieving Stripe configuration: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get frontend configuration
        /// Returns all public configuration values needed by the frontend
        /// </summary>
        /// <returns>Frontend configuration object</returns>
        [HttpGet("frontend")]
        public IActionResult GetFrontendConfig()
        {
            try
            {
                return Ok(new JsonModel
                {
                    
                    StatusCode = 200,
                    Message = "Frontend configuration retrieved successfully",
                    data = new
                    {
                        stripe = new
                        {
                            publishableKey = _configuration["StripeSettings:PublishableKey"],
                            currency = "usd",
                            locale = "en"
                        },
                        features = new
                        {
                            chatEnabled = true,
                            videoEnabled = true,
                            appointmentsEnabled = true,
                            subscriptionsEnabled = true
                        },
                        limits = new
                        {
                            maxFileUploadSizeMB = 10,
                            maxMessageLength = 1000
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new JsonModel
                {
                   
                    StatusCode = 500,
                    Message = $"Error retrieving frontend configuration: {ex.Message}"
                });
            }
        }
    }
}



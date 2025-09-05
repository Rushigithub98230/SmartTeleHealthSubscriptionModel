using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Core.DTOs;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers
{
    /// <summary>
    /// Base controller class that provides common functionality for all API controllers.
    /// This abstract class serves as the foundation for all controllers in the SmartTelehealth API,
    /// providing shared methods and configurations for authentication, authorization, and response handling.
    /// </summary>
    //[Authorize]
    [Produces("application/json")]
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Extracts user authentication information from the HTTP context and creates a TokenModel.
        /// This method is used by all controllers to get the current user's ID and role for authorization
        /// and audit purposes throughout the application.
        /// </summary>
        /// <param name="httpContext">The HTTP context containing user claims and authentication information</param>
        /// <returns>TokenModel containing the user ID and role ID extracted from JWT claims</returns>
        /// <remarks>
        /// This method:
        /// - Extracts the user ID from the NameIdentifier claim (standard JWT claim)
        /// - Extracts the role ID from a custom "RoleId" claim
        /// - Handles parsing errors gracefully by defaulting to 0 for invalid values
        /// - Returns a TokenModel that can be used throughout the application for user identification
        /// 
        /// The TokenModel is used for:
        /// - Authorization checks (ensuring users can only access their own data)
        /// - Audit logging (tracking who performed what actions)
        /// - Service layer operations (passing user context to business logic)
        /// </remarks>
        [NonAction]
        public TokenModel GetToken(HttpContext httpContext)
        {
            var userIDClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleIDClaim = httpContext.User.FindFirst("RoleId")?.Value; // Use custom RoleId claim instead of ClaimTypes.Role

            int userID = 0;
            int roleID = 0;

            if (!string.IsNullOrEmpty(userIDClaim) && int.TryParse(userIDClaim, out int parsedUserID))
            {
                userID = parsedUserID;
            }

            if (!string.IsNullOrEmpty(roleIDClaim) && int.TryParse(roleIDClaim, out int parsedRoleID))
            {
                roleID = parsedRoleID;
            }

            return new TokenModel
            {
                UserID = userID,
                RoleID = roleID
            };
        }
    }
}

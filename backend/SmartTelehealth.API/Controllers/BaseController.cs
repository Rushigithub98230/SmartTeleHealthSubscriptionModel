using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Core.DTOs;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    public abstract class BaseController : Controller
    {
        [NonAction]
        public TokenModel GetToken(HttpContext httpContext)
        {
            var userIDClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleIDClaim = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

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

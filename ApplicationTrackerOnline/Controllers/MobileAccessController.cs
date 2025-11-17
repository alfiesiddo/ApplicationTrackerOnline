using Microsoft.AspNetCore.Authentication.JwtBearer; // <-- Add this using statement
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationTrackerOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileAccessController : ControllerBase
    {
        [HttpGet("test")]
        [Authorize(AuthenticationSchemes = "JwtScheme")]
        public IActionResult Test()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            return Ok(new { message = "Hello from protected endpoint", userId });
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationTrackerOnline.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [Authorize(AuthenticationSchemes = "JwtScheme")]
        [HttpGet("hello")]
        public IActionResult Hello()
        {
            var userId = User?.FindFirst("sub")?.Value;
            return Ok(new { message = "Hello from protected endpoint!", userId });
        }
    }
}
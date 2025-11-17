using ApplicationTrackerOnline.Models;
using ApplicationTrackerOnline.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationTrackerOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthController> _logger;
        private readonly JwtService _jwt;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtService jwt, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                       ?? await _userManager.FindByNameAsync(dto.Email);

            if (user == null)
                return Unauthorized(new { error = "Invalid email or password." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { error = "Invalid email or password." });

            try
            {
                var token = _jwt.GenerateToken(user);
                return Ok(new { access_token = token, token_type = "Bearer", expires_in = 3600 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token generation failed for user {UserId}", user.Id);
                return StatusCode(500, new { error = "Token generation failed", details = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { error = "Email and password are required." });

            // Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return Conflict(new { error = "Email is already registered." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            try
            {
                var token = _jwt.GenerateToken(user);

                return Ok(new
                {
                    message = "User registered successfully.",
                    access_token = token,
                    token_type = "Bearer",
                    expires_in = 3600
                });
            }
            catch (Exception ex)
            {
                // Log the full exception (stack trace, inner exception, everything)
                _logger.LogError(ex, "Token generation failed during registration for user {Email}", dto.Email);

                // Safe return for clients
                return StatusCode(500, new { error = "Token generation failed", details = ex.Message });
            }
        }
    }


        public record RegisterDto(string Email, string Password);
    public record LoginDto(string Email, string Password);
}
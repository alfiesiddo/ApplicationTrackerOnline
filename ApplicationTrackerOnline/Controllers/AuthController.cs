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
        private readonly JwtService _jwt;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtService jwt)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt;
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

            var token = _jwt.GenerateToken(user);

            return Ok(new { access_token = token, token_type = "Bearer", expires_in = 3600 });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { error = "Email and password are required." });

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

            //auto-login and return JWT
            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                message = "User registered successfully.",
                access_token = token,
                token_type = "Bearer"
            });
        }
    }

    public record RegisterDto(string Email, string Password);
    public record LoginDto(string Email, string Password);
}
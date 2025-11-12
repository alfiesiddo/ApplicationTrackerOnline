using ApplicationTrackerOnline.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApplicationTrackerOnline.Services
{
    public class JwtService
    {
        private readonly JwtOptions _options;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IOptions<JwtOptions> options, ILogger<JwtService> logger)
        {
            _options = options.Value;
            _logger = logger;

            if (string.IsNullOrEmpty(_options.Key))
            {
                _logger.LogError("JWT secret key is missing from configuration (Jwt:Key).");
                throw new InvalidOperationException("JWT secret key not configured. Set Jwt:Key in configuration.");
            }
        }

        public string GenerateToken(ApplicationUser user)
        {
            try
            {
                _logger.LogInformation("Generating JWT for user {Email}", user?.Email ?? "UNKNOWN");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user?.Id ?? Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user?.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: _options.Issuer ?? "applicationTrackerAPI",
                    audience: _options.Audience ?? "applicationtrackerClients",
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_options.ExpireMinutes > 0 ? _options.ExpireMinutes : 60),
                    signingCredentials: creds
                );

                _logger.LogInformation("JWT successfully generated for {Email}", user?.Email ?? "UNKNOWN");
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT generation failed for {Email}", user?.Email ?? "UNKNOWN");
                throw;
            }
        }
    }
}

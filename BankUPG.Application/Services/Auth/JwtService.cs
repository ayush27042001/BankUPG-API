using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BankUPG.SharedKernal.Models;

namespace BankUPG.Application.Services.Auth
{
    public class JwtService
    {
        private readonly AppSettings _appSettings;

        public JwtService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public string GenerateToken(string email, string firstName, string lastName, int userId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("FirstName", firstName),
                new Claim("LastName", lastName),
                new Claim(ClaimTypes.Email, email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Jwt.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _appSettings.Jwt.Issuer,
                audience: _appSettings.Jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_appSettings.Jwt.Secret);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _appSettings.Jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _appSettings.Jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public DateTime GetRefreshTokenExpiration()
        {
            return DateTime.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays);
        }
    }
}

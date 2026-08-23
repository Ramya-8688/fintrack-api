using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FinTrack.Infrastructure.Authentication
{
    /// <summary>
    /// Service for generating JWT tokens for authentication.
    /// </summary>
    public interface IJwtTokenProvider
    {
        /// <summary>
        /// Generates a JWT token for a user.
        /// </summary>
        /// <param name="userId">User ID to include in token.</param>
        /// <param name="email">User email to include in token.</param>
        /// <returns>JWT token string.</returns>
        string GenerateToken(string userId, string email);
    }

    /// <summary>
    /// Implementation of IJwtTokenProvider.
    /// </summary>
    public class JwtTokenProvider : IJwtTokenProvider
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        /// <summary>
        /// Initializes the JWT token provider.
        /// </summary>
        public JwtTokenProvider(string secretKey, string issuer, string audience, int expirationMinutes)
        {
            if (secretKey.Length < 32)
                throw new ArgumentException("Secret key must be at least 32 characters long", nameof(secretKey));

            _secretKey = secretKey;
            _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
            _audience = audience ?? throw new ArgumentNullException(nameof(audience));
            _expirationMinutes = expirationMinutes;
        }

        /// <summary>
        /// Generates a JWT token with user claims.
        /// </summary>
        public string GenerateToken(string userId, string email)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

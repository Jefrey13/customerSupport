using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CustomerService.API.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly byte[] _key;
        private readonly IUserRoleRepository _userRoleRepository;

        public TokenService(
            IOptions<JwtSettings> jwtOptions,
            IUserRoleRepository userRoleRepository)
        {
            _jwtSettings = jwtOptions.Value;
            _key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            _userRoleRepository = userRoleRepository;
        }

        public string GenerateAccessToken(User user)
        {
            var roles = (user.UserRoles != null && user.UserRoles.Any())
                ? user.UserRoles.Select(ur => ur.Role.RoleName)
                : _userRoleRepository
                    .GetRolesByUserIdAsync(user.UserId)
                    .Result
                    .Select(ur => ur.Role.RoleName);

                var claims = new List<Claim>
                {
                    // Standard subject claim
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    // Include NameIdentifier so SignalR and other code can pick it up
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                     new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            // Role claims
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(securityToken);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false,
                RoleClaimType = ClaimTypes.Role
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, validationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
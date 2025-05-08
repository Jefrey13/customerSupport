using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CustomerService.API.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly byte[] _key;
        private readonly IUserRoleRepository _userRoleRepository;

        public TokenService(
            IOptions<JwtSettings> jwtOptions,
            IUserRoleRepository userRoleRepository
        )
        {
            _jwtSettings = jwtOptions.Value;
            _key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            _userRoleRepository = userRoleRepository;
        }

        public string GenerateAccessToken(User user)
        {
            // 1) Obtener la lista de roles del usuario
            var roles = (user.UserRoleUsers != null && user.UserRoleUsers.Any())
                ? user.UserRoleUsers.Select(ur => ur.Role.RoleName)
                : _userRoleRepository
                    .GetRolesByUserIdAsync(user.UserId)
                    .Result
                    .Select(ur => ur.Role.RoleName);

            // 2) Construir los claims básicos + roles
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            // 3) Crear descriptor y generar token
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256
            );
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var rnd = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(rnd);
            return Convert.ToBase64String(rnd);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false,
                // Importante: para que se reconozcan los ClaimTypes.Role
                RoleClaimType = ClaimTypes.Role
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, validationParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
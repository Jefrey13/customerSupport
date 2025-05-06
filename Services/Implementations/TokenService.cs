using CustomerService.API.Models;
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
        private readonly JwtSettings _j;
        private readonly byte[] _key;
        public TokenService(IOptions<JwtSettings> options)
        {
            _j = options.Value;
            _key = Encoding.UTF8.GetBytes(_j.Key);
        }

        public string GenerateAccessToken(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
            var desc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_j.DurationInMinutes),
                Issuer = _j.Issuer,
                Audience = _j.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256)
            };
            var token = handler.CreateToken(desc);
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
            var handler = new JwtSecurityTokenHandler();
            var opts = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _j.Issuer,
                ValidAudience = _j.Audience,
                ValidateLifetime = false
            };
            var principal = handler.ValidateToken(token, opts, out var secTok);
            if (secTok is not JwtSecurityToken jwt || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }
    }
}

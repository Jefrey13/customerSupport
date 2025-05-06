using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.Extensions.Options;

namespace CustomerService.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IContactRepository _contacts;
        private readonly IAuthTokenRepository _tokens;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _jwt;
        private readonly IEmailService _email;
        private readonly IUnitOfWork _uow;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUserRepository users,
            IContactRepository contacts,
            IAuthTokenRepository tokens,
            IPasswordHasher hasher,
            ITokenService jwt,
            IEmailService email,
            IUnitOfWork uow,
            IOptions<JwtSettings> jwtOptions)
        {
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _contacts = contacts ?? throw new ArgumentNullException(nameof(contacts));
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _jwtSettings = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            if (await _users.ExistsAsync(u => u.Email == request.Email, ct))
                throw new ArgumentException("Email already in use");

            var hashedPwd = _hasher.Hash(request.Password);
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = Encoding.UTF8.GetBytes(hashedPwd),
                IsActive = false
            };
            await _users.AddAsync(user, ct);

            var contact = new Contact
            {
                CompanyName = request.CompanyName,
                ContactName = request.ContactName,
                Email = request.Email,
                Phone = request.Phone,
                Country = request.Country
            };
            await _contacts.AddAsync(contact, ct);

            await _uow.SaveChangesAsync(ct);

            User? userModel = await _users.GetByEmailAsync(contact.Email);
            var accessToken = _jwt.GenerateAccessToken(userModel!);
            var refreshToken = _jwt.GenerateRefreshToken();

            var authEntity = new AuthToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                TokenType = TokenType.Refresh.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                JwtId = Guid.NewGuid().ToString()
            };
            //await _tokens.AddAsync(authEntity, ct);
           // await _uow.SaveChangesAsync(ct);

            await _email.SendAsync(
                user.Email,
                "Verify your email",
                $"Click to verify: /api/auth/verify-email?token={authEntity.Token}"
            );

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = authEntity.ExpiresAt,
                UserId = user.UserId,
                ContactId = contact.ContactId
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, ct)
                     ?? throw new KeyNotFoundException("Invalid credentials");

            var storedHash = Encoding.UTF8.GetString(user.PasswordHash);

            if (!_hasher.Verify(storedHash, request.Password))
                throw new ArgumentException("Invalid credentials");
            if (!user.IsActive)
                throw new InvalidOperationException("Account not verified");

            user.LastLoginAt = DateTime.UtcNow;
            _users.Update(user);
            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();
            var authEntity = new AuthToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                TokenType = TokenType.Refresh.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                JwtId = Guid.NewGuid().ToString()
            };
            await _tokens.AddAsync(authEntity, ct);
            await _uow.SaveChangesAsync(ct);

            await _email.SendAsync(
                user.Email,
                    "You are login successfully",
                    $"<b>Wellcome back again,</b> {user.FullName}" +
                    $"<br/>" +
                    $"<img src=\"https://i.ibb.co/wrJQkw6p/welcomeback.webp\" alt=\"undraw-proud-self-j8xv\" height=\"300px\" width=\"300px\" border=\"0\">" +
                    $"<br/>" +
                    $"If want was not you, contact us \"<a href=\"https://ibb.co/3yrSZKKs\">Clik here</a>"
            );

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = authEntity.ExpiresAt,
                UserId = user.UserId
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
        {
            var existing = await _tokens.GetByTokenAsync(request.RefreshToken, ct)
                         ?? throw new KeyNotFoundException("Invalid token");

            if (existing.TokenType != TokenType.Refresh.ToString()
             || existing.ExpiresAt <= DateTime.UtcNow
             || existing.Revoked)
                throw new ArgumentException("Invalid or expired token");

            existing.Revoked = true;
            _tokens.Update(existing);

            var user = await _users.GetByIdAsync(existing.UserId, ct)
                     ?? throw new KeyNotFoundException("User not found");

            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();
            var newEntity = new AuthToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                TokenType = TokenType.Refresh.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                JwtId = Guid.NewGuid().ToString(),
            };
            await _tokens.AddAsync(newEntity, ct);
            await _uow.SaveChangesAsync(ct);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = newEntity.ExpiresAt,
                UserId = user.UserId
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, ct)
                     ?? throw new KeyNotFoundException("Email not found");

            var resetToken = _jwt.GenerateRefreshToken();
            var entity = new AuthToken
            {
                UserId = user.UserId,
                Token = resetToken,
                TokenType = TokenType.PasswordReset.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                JwtId = Guid.NewGuid().ToString()
            };
            await _tokens.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            await _email.SendAsync(
                user.Email,
                "Reset your password",
                $"Reset: /api/auth/reset-password?token={resetToken}"
            );
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        {
            var entity = await _tokens.GetByTokenAsync(request.Token, ct)
                       ?? throw new KeyNotFoundException("Invalid token");

            if (entity.TokenType != TokenType.PasswordReset.ToString()
             || entity.Revoked
             || entity.ExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Invalid or expired token");

            var user = await _users.GetByIdAsync(entity.UserId, ct)
                     ?? throw new KeyNotFoundException("User not found");

            var newHash = _hasher.Hash(request.NewPassword);
            user.PasswordHash = Encoding.UTF8.GetBytes(newHash);
            user.SecurityStamp = Guid.NewGuid();
            entity.Revoked = true;

            _users.Update(user);
            _tokens.Update(entity);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task VerifyEmailAsync(string token, CancellationToken ct = default)
        {
            var entity = await _tokens.GetByTokenAsync(token, ct)
                       ?? throw new KeyNotFoundException("Invalid token");

            if (entity.TokenType != TokenType.Verification.ToString()
             || entity.Revoked
             || entity.ExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Invalid or expired token");

            var user = await _users.GetByIdAsync(entity.UserId, ct)
                     ?? throw new KeyNotFoundException("User not found");

            user.IsActive = true;
            entity.Revoked = true;

            _users.Update(user);
            _tokens.Update(entity);
            await _uow.SaveChangesAsync(ct);
        }
    }
}

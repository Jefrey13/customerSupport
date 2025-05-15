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
        private readonly IUserRoleRepository _usersRoles;
        private readonly IRoleRepository _roles;
        private readonly IAuthTokenRepository _tokens;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _jwt;
        private readonly IEmailService _email;
        private readonly IUnitOfWork _uow;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUserRepository users,
            IUserRoleRepository usersRoles,
            IRoleRepository roles,
            IAuthTokenRepository tokens,
            IPasswordHasher hasher,
            ITokenService jwt,
            IEmailService email,
            IUnitOfWork uow,
            IOptions<JwtSettings> jwtOptions)
        {
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _usersRoles = usersRoles ?? throw new ArgumentNullException(nameof(usersRoles));
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _jwtSettings = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        public async Task RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            if (await _users.ExistsAsync(u => u.Email == request.Email, ct))
                throw new ArgumentException("Email already in use.");

            // 1) Create the user tied to a company
            var hashedPwd = _hasher.Hash(request.Password);
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = Encoding.UTF8.GetBytes(hashedPwd),
                IsActive = false,
                CompanyId = request.CompanyId,
                Phone = request.Phone,
                ImageUrl = request.ImageUrl,
                Identifier = request.Identifier,
                CreatedAt = DateTime.UtcNow
            };

            await _users.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            // 2) Assign the Customer role
            var customerRole = await _roles
                .GetByNameAsync("Customer", ct)
                ?? throw new InvalidOperationException("Customer role not found.");

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = customerRole.RoleId,
                AssignedAt = DateTime.UtcNow
            };

            await _usersRoles.AddAsync(userRole, ct);
            await _uow.SaveChangesAsync(ct);

            // 3) Generate and store verification token
            var verifyToken = _jwt.GenerateRefreshToken();
            var authEntity = new AuthToken
            {
                UserId = user.UserId,
                Token = verifyToken,
                TokenType = TokenType.Verification.ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };
            await _tokens.AddAsync(authEntity, ct);
            await _uow.SaveChangesAsync(ct);

            // 4) Send verification email
            var link = $"http://localhost:5173/verify-account?token={Uri.EscapeDataString(verifyToken)}";
            var html = $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
              <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
                <h2>¡Bienvenido, {user.FullName}!</h2>
              </div>
              <div style=""padding:24px;color:#333;"">
                <p>Gracias por registrarte en <strong>Customer Support Chat</strong>.</p>
                <p>Para activar tu cuenta, haz clic en el botón:</p>
                <p style=""text-align:center;margin:24px 0;"">
                  <a href=""{link}"" style=""background:#356ace;color:#fff;padding:12px 24px;border-radius:4px;text-decoration:none;"">Verificar mi cuenta</a>
                </p>
                <p style=""font-size:12px;color:#777;"">Si no solicitaste este correo, ignóralo.</p>
              </div>
            </div>";
            await _email.SendAsync(user.Email, "Activa tu cuenta", html);
        }


        public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, ct)
                       ?? throw new KeyNotFoundException("Invalid credentials.");

            var storedHash = Encoding.UTF8.GetString(user.PasswordHash);
            if (!_hasher.Verify(storedHash, request.Password))
                throw new ArgumentException("Invalid credentials.");
            if (!user.IsActive)
                throw new InvalidOperationException("Account not verified.");

            user.UpdatedAt = DateTime.UtcNow;
            _users.Update(user);
            await _uow.SaveChangesAsync(ct);

            // Generate tokens
            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            var authEntity = new AuthToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                TokenType = TokenType.Refresh.ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                // Revoked y Used por defecto ya salen como 'false'
                // RowVersion lo gestiona SQL Server
            };

            await _tokens.AddAsync(authEntity, ct);
            await _uow.SaveChangesAsync(ct);

            var html = $@"
                <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
                  <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
                    <h2>Hola {user.FullName}, bienvenido de vuelta!</h2>
                  </div>
                  <div style=""padding:24px;color:#333;"">
                    <p>Has iniciado sesión correctamente en <strong>Customer Support Dashboard</strong>.</p>
                    <p>Si no fuiste tú, por favor contacta con nuestro equipo de soporte.</p>
                  </div>
                </div>";
            await _email.SendAsync(user.Email, "Inicio de sesión exitoso", html);

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

            // Revoke the old refresh token
            existing.Revoked = true;
            _tokens.Update(existing);
            await _uow.SaveChangesAsync(ct);

            // Load the user
            var user = await _users.GetByIdAsync(existing.UserId, ct)
                     ?? throw new KeyNotFoundException("User not found");

            // Generate new tokens
            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            var newEntity = new AuthToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                TokenType = TokenType.Refresh.ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes)
                // Revoked and Used default to false
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
                     ?? throw new KeyNotFoundException("Email not found.");

            // Generate reset token
            var resetToken = _jwt.GenerateRefreshToken();
            var now = DateTime.UtcNow;

            // Store reset token
            var entity = new AuthToken
            {
                UserId = user.UserId,
                Token = resetToken,
                TokenType = TokenType.PasswordReset.ToString(),
                CreatedAt = now,
                ExpiresAt = now.AddHours(1)
                // Revoked and Used default to false
            };
            await _tokens.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            // Send reset email
            var link = $"http://localhost:5173/reset-password?token={Uri.EscapeDataString(resetToken)}";
            var html = $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
              <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
                <h2>Reset Your Password</h2>
              </div>
              <div style=""padding:24px;color:#333;"">
                <p>Hello {user.FullName},</p>
                <p>Click the button below to reset your password:</p>
                <p style=""text-align:center;margin:20px 0;"">
                  <a href=""{link}"" style=""background:#356ace;color:#fff;padding:12px 24px;border-radius:4px;text-decoration:none;"" target=""_blank"">
                    Reset My Password
                  </a>
                </p>
                <p style=""font-size:12px;color:#777;"">If you didn't request this, please ignore this email.</p>
              </div>
            </div>";
            await _email.SendAsync(user.Email, "Password Reset Instructions", html);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        {
            var entity = await _tokens.GetByTokenAsync(request.Token, ct);

            //if (entity.TokenType != TokenType.Verification.ToString()
            // || entity.ExpiresAt <= DateTime.UtcNow)
            //    throw new ArgumentException("Invalid or expired token.");

            var user = await _users.GetByIdAsync(entity.UserId, ct)
                     ?? throw new KeyNotFoundException("User not found.");

            // Update user password
            var newHash = _hasher.Hash(request.NewPassword);
            user.PasswordHash = Encoding.UTF8.GetBytes(newHash);
            user.UpdatedAt = DateTime.UtcNow;
            _users.Update(user);

            // Revoke the reset token
            entity.Revoked = true;
            _tokens.Update(entity);

            await _uow.SaveChangesAsync(ct);

            // Notify user
            var html = $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
              <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
                <h2>Password Reset Successfully</h2>
              </div>
              <div style=""padding:24px;color:#333;"">
                <p>Hello {user.FullName},</p>
                <p>Your password has been updated successfully.</p>
                <p style=""font-size:12px;color:#777;"">If you did not perform this action, please contact support immediately.</p>
              </div>
            </div>";
            await _email.SendAsync(user.Email, "Your password has been reset", html);
        }
        public async Task VerifyEmailAsync(string token, CancellationToken ct = default)
        {
            var entity = await _tokens.GetByTokenAsync(token, ct)
                       ?? throw new KeyNotFoundException("Invalid token.");

            if (entity.TokenType != TokenType.Verification.ToString()
             || entity.ExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Invalid or expired token.");

            var user = await _users.GetByIdAsync(entity.UserId, ct)
                     ?? throw new KeyNotFoundException("User not found.");

            // Activate user
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            _users.Update(user);

            // Revoke verification token
            entity.Revoked = true;
            _tokens.Update(entity);

            await _uow.SaveChangesAsync(ct);

            // Send confirmation email
            var html = $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
              <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
                <h2>Account Verified!</h2>
              </div>
              <div style=""padding:24px;color:#333;"">
                <p>Your account has been successfully verified.</p>
                <p>You can now log in and start using the platform.</p>
              </div>
            </div>";
            await _email.SendAsync(user.Email, "Account Verified", html);
        }

    }
}

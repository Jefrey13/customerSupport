using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Implementations;
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
        private readonly IContactRepository _contacts;
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
            IContactRepository contacts,
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
            _contacts = contacts ?? throw new ArgumentNullException(nameof(contacts));
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
                throw new ArgumentException("Email already in use");

            var hashedPwd = _hasher.Hash(request.Password);
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = Encoding.UTF8.GetBytes(hashedPwd),
                IsActive = false,
                CreatedBy = Guid.Parse("D34D3DE7-3A3F-40DC-ABF9-77CD9431EEA3"),
                UpdatedBy = null
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

            var clientRole = await _roles
                .GetByNameAsync("db_client", ct)
                ?? throw new InvalidOperationException("Default role not found");

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = clientRole.RoleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = Guid.Parse("D34D3DE7-3A3F-40DC-ABF9-77CD9431EEA3")
            };
            await _usersRoles.AddAsync(userRole, ct);

            await _uow.SaveChangesAsync(ct);

            var verifyToken = _jwt.GenerateRefreshToken();
            var authEntity = new AuthToken
            {
                UserId = user.UserId,
                Token = verifyToken,
                TokenType = TokenType.Verification.ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                JwtId = Guid.NewGuid().ToString(),
                CreatedBy = Guid.Parse("D34D3DE7-3A3F-40DC-ABF9-77CD9431EEA3")
            };
            await _tokens.AddAsync(authEntity, ct);

            await _uow.SaveChangesAsync(ct);

            var link = $"http://localhost:5173/verify-account?token={Uri.EscapeDataString(verifyToken)}";
            var html = $@"
                <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
                  <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
                    <h2 style=""margin:0;font-size:20px;color:#fff;"">¡Bienvenido, {user.FullName}!</h2>
                  </div>
                  <div style=""padding:24px;color:#333;line-height:1.5;"">
                    <p>Gracias por registrarte en <strong>Customer Support Chat</strong>.</p>
                    <p>Para activar tu cuenta, haz clic en el botón:</p>
                    <p style=""text-align:center;margin:24px 0;"">
                      <a href=""{link}"" style=""background:#356ace;color:#fff;text-decoration:none;padding:12px 24px;border-radius:4px;display:inline-block;"" target=""_blank"">
                        Verificar mi cuenta
                      </a>
                    </p>
                    <p style=""font-size:12px;color:#777;"">
                      Si lo prefieres, copia este enlace en tu navegador:<br/>
                      <a href=""{link}"" style=""color:#356ace;"">{link}</a>
                    </p>
                    <p style=""font-size:12px;color:#777;"">
                      Si no solicitaste este correo, ignóralo.
                    </p>
                  </div>
                  <div style=""background:#f4f4f7;padding:12px;text-align:center;font-size:12px;color:#999;"">
                    © {DateTime.UtcNow.Year} Customer Support Dashboard
                  </div>
                </div>";

            await _email.SendAsync(user.Email, "Activa tu cuenta", html);
        }


        public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, ct)
                       ?? throw new KeyNotFoundException("Credenciales inválidas");

            var storedHash = Encoding.UTF8.GetString(user.PasswordHash);
            if (!_hasher.Verify(storedHash, request.Password))
                throw new ArgumentException("Credenciales inválidas");
            if (!user.IsActive)
                throw new InvalidOperationException("Cuenta no verificada");

            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.UserId;
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
                JwtId = Guid.NewGuid().ToString(),
                CreatedBy = user.UserId,
                CreatedAt = DateTime.UtcNow
            };
            await _tokens.AddAsync(authEntity, ct);
            await _uow.SaveChangesAsync(ct);

            var html = $@"
        <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
          <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
            <h2 style=""margin:0;font-size:20px;color:#fff;"">Hola {user.FullName}, bienvenido de vuelta!</h2>
          </div>
          <div style=""padding:24px;color:#333;line-height:1.5;"">
            <p>Has iniciado sesión correctamente en <strong>Customer Support Dashboard</strong>.</p>
            <p>Si no has sido tú, por favor contacta inmediatamente con nuestro equipo de soporte.</p>
          </div>
          <div style=""background:#f4f4f7;padding:12px;text-align:center;font-size:12px;color:#999;"">
            © {DateTime.UtcNow.Year} Customer Support Dashboard
          </div>
        </div>";

            await _email.SendAsync(user.Email, "Inicio de sesión exitoso", html);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = authEntity.ExpiresAt,
                UserId = user.UserId,
                ContactId = Guid.NewGuid()
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
            var now = DateTime.UtcNow;
            var system = user.UserId; // o el admin si prefieres

            var entity = new AuthToken
            {
                UserId = user.UserId,
                Token = resetToken,
                TokenType = TokenType.PasswordReset.ToString(),
                ExpiresAt = now.AddHours(1),
                JwtId = Guid.NewGuid().ToString(),
                CreatedAt = now,
                CreatedBy = system,
                Revoked = false,
                Used = false
            };
            await _tokens.AddAsync(entity, ct);

            // igual para Contact si lo estuvieras creando aquí...

            await _uow.SaveChangesAsync(ct);

            var link = $"http://localhost:5173/reset-password?token={Uri.EscapeDataString(resetToken)}";
            var html = $@"
      <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
        <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
          <h2 style=""margin:0;font-size:20px;color:#fff;"">Reset Your Password</h2>
        </div>
        <div style=""padding:24px;color:#333;line-height:1.5;"">
          <p>Hello {user.FullName},</p>
          <p>Click the button below to reset your password:</p>
          <p style=""text-align:center;margin:20px 0;"">
            <a href=""{link}"" style=""background:#356ace;color:#fff;text-decoration:none;padding:12px 24px;border-radius:4px;display:inline-block;"" target=""_blank"">
              Reset My Password
            </a>
          </p>
          <p style=""font-size:12px;color:#777;"">
            If you didn't request this, please ignore this email.
          </p>
        </div>
        <div style=""background:#f4f4f7;padding:12px;text-align:center;font-size:12px;color:#999;"">
          © {now.Year} Customer Support Dashboard
        </div>
      </div>";

            await _email.SendAsync(user.Email, "Password Reset Instructions", html);
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
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.UserId;

            entity.Revoked = true;

            _users.Update(user);
            _tokens.Update(entity);
            await _uow.SaveChangesAsync(ct);

            var html = $@"
      <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
        <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
          <h2 style=""margin:0;font-size:20px;color:#fff;"">Password Reset Successfully</h2>
        </div>
        <div style=""padding:24px;color:#333;line-height:1.5;"">
          <p>Hello {user.FullName},</p>
          <p>Your password has been updated successfully.</p>
          <p style=""font-size:12px;color:#777;"">
            If you did not perform this action, please contact support immediately.
          </p>
        </div>
        <div style=""background:#f4f4f7;padding:12px;text-align:center;font-size:12px;color:#999;"">
          © {DateTime.UtcNow.Year} Customer Support Dashboard
        </div>
      </div>";
            await _email.SendAsync(user.Email, "Your password has been reset", html);
        }



        public async Task VerifyEmailAsync(string token, CancellationToken ct = default)
        {
            var entity = await _tokens.GetByTokenAsync(token, ct)
                       ?? throw new KeyNotFoundException("Token inválido");

            if (entity.TokenType != TokenType.Verification.ToString()
             || entity.Revoked
             || entity.ExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Token inválido o expirado");

            var user = await _users.GetByIdAsync(entity.UserId, ct)
                     ?? throw new KeyNotFoundException("Usuario no encontrado");

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.UserId;
            entity.Revoked = true;
            entity.CreatedBy = user.UserId;

            _users.Update(user);
            _tokens.Update(entity);
            await _uow.SaveChangesAsync(ct);

            var html = $@"
              <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
                <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
                  <h2 style=""margin:0;font-size:20px;color:#fff;"">¡Cuenta verificada!</h2>
                </div>
                <div style=""padding:24px;color:#333;line-height:1.5;"">
                  <p>Tu cuenta ha sido activada exitosamente.</p>
                  <p>Ahora puedes iniciar sesión y empezar a usar la plataforma.</p>
                </div>
                <div style=""background:#f4f4f7;padding:12px;text-align:center;font-size:12px;color:#999;"">
                  © {DateTime.UtcNow.Year} Customer Support Chat
                </div>
              </div>";

            await _email.SendAsync(user.Email, "Cuenta verificada", html);
        }

    }
}

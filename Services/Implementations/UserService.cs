using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CustomerService.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly IPresenceService _presence;
        private readonly IEmailService _email;
        private readonly JwtSettings _jwtSettings;
        private readonly ITokenService _jwt;
        public UserService(IUnitOfWork uow, IPasswordHasher hasher, IPresenceService presence,
            IEmailService email, IOptions<JwtSettings> jwtOptions, ITokenService jwt)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _presence = presence ?? throw new ArgumentNullException(nameof(presence));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
            _jwtSettings = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        public async Task<PagedResponse<UserDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default)
        {
            var query = _uow.Users.GetAll();
            var paged = await PagedList<User>.CreateAsync(query, @params.PageNumber, @params.PageSize, cancellation);
            var users = paged.ToList();
            var userIds = users.Select(u => u.UserId).ToList();

            var convoCounts = await _uow.Conversations
                .GetAll()
                .Where(c => c.ClientUserId.HasValue && userIds.Contains(c.ClientUserId.Value))
                .GroupBy(c => c.ClientUserId.Value)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellation);

            var lastOnlines = await _presence.GetLastOnlineAsync(userIds, cancellation);

            var userRoles = await _uow.UserRoles
                .GetAll()
                .Where(ur => userIds.Contains(ur.UserId))
                .Include(ur => ur.Role)
                .ToListAsync(cancellation);

            var rolesByUser = userRoles
                .GroupBy(ur => ur.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ur => new RoleDto { RoleId = ur.RoleId, RoleName = ur.Role.RoleName }).ToList()
                );

            var dtos = users.Select(u =>
            {
                DateTime? last = null;
                if (lastOnlines.TryGetValue(u.UserId, out var lo)) last = lo;
                var count = convoCounts.ContainsKey(u.UserId) ? convoCounts[u.UserId] : 0;
                return new UserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName!,
                    Email = u.Email!,
                    IsActive = u.IsActive,
                    CompanyId = u.CompanyId,
                    Phone = u.Phone,
                    Identifier = u.Identifier,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    ImageUrl = u.ImageUrl,
                    LastOnline = last,
                    IsOnline = last.HasValue && (DateTime.UtcNow - last.Value).TotalMinutes < 5,
                    ClientType = count switch { 0 => "Nuevo", > 0 and <= 5 => "Frecuente", _ => "VIP" },
                    Roles = rolesByUser.ContainsKey(u.UserId)
                                ? rolesByUser[u.UserId]
                                : new List<RoleDto>()
                };
            }).ToList();

            return new PagedResponse<UserDto>(dtos, paged.MetaData);
        }

        public async Task<UserDto> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            if (id <= 0) throw new ArgumentException("Invalid user ID.", nameof(id));
            var u = await _uow.Users.GetByIdAsync(id, cancellation)
                     ?? throw new KeyNotFoundException("User not found.");

            var lastDict = await _presence.GetLastOnlineAsync(new[] { u.UserId }, cancellation);
            var last = lastDict.ContainsKey(u.UserId) ? lastDict[u.UserId] : (DateTime?)null;
            var convoCount = await _uow.Conversations
                .GetAll()
                .CountAsync(c => c.ClientUserId == u.UserId, cancellation);

            var roles = await _uow.UserRoles.GetRolesByUserIdAsync(u.UserId, cancellation);
            var rolesDto = roles.Select(ur => new RoleDto { RoleId = ur.RoleId, RoleName = ur.Role.RoleName });

            return new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName!,
                Email = u.Email!,
                IsActive = u.IsActive,
                CompanyId = u.CompanyId,
                Phone = u.Phone,
                Identifier = u.Identifier,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                ImageUrl = u.ImageUrl,
                LastOnline = last,
                IsOnline = last.HasValue && (DateTime.UtcNow - last.Value).TotalMinutes < 5,
                ClientType = convoCount switch { 0 => "Nuevo", > 0 and <= 5 => "Frecuente", _ => "VIP" },
                Roles = rolesDto
            };
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.", nameof(request.Email));
            if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Password is required.", nameof(request.Password));
            if (await _uow.Users.ExistsAsync(u => u.Email == request.Email, cancellation)) throw new InvalidOperationException("Email already in use.");

            var hash = _hasher.Hash(request.Password);
            var entity = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = Encoding.UTF8.GetBytes(hash),
                IsActive = true,
                CompanyId = request.CompanyId,
                Phone = request.Phone,
                Identifier = request.Identifier,
                CreatedAt = DateTime.UtcNow,
                ImageUrl = request.ImageUrl
            };

            await _uow.Users.AddAsync(entity, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            if (request.RoleIds.Any())
            {
                foreach (var rid in request.RoleIds.Distinct())
                {
                    if (!await _uow.Roles.ExistsAsync(r => r.RoleId == rid, cancellation))
                        throw new KeyNotFoundException($"RoleId {rid} no existe.");
                    var ur = new UserRole { UserId = entity.UserId, RoleId = rid, AssignedAt = DateTime.UtcNow };
                    await _uow.UserRoles.AddAsync(ur, cancellation);
                }
                await _uow.SaveChangesAsync(cancellation);
            }

            var createdRoles = request.RoleIds.Any()
                ? (await _uow.Roles.GetAll().Where(r => request.RoleIds.Contains(r.RoleId)).ToListAsync(cancellation))
                    .Select(r => new RoleDto { RoleId = r.RoleId, RoleName = r.RoleName })
                : Enumerable.Empty<RoleDto>();

            var verifyToken = _jwt.GenerateRefreshToken();

            // 4) Send verification email
            var link = $"http://localhost:5173/verify-account?token={Uri.EscapeDataString(verifyToken)}";
            var html = $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
              <div style=""background:#356ace;padding:16px;text-align:center;color:#fff;"">
                <h2>¡Bienvenido, {entity.FullName}!</h2>
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
            await _email.SendAsync(entity.Email, "Activa tu cuenta", html);

            return new UserDto
            {
                UserId = entity.UserId,
                FullName = entity.FullName!,
                Email = entity.Email!,
                IsActive = entity.IsActive,
                CompanyId = entity.CompanyId,
                Phone = entity.Phone,
                Identifier = entity.Identifier,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                ImageUrl = entity.ImageUrl,
                LastOnline = null,
                IsOnline = false,
                ClientType = "Nuevo",
                Roles = createdRoles
            };
        }

        public async Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellation = default)
        {
            if (request.UserId <= 0) throw new ArgumentException("Invalid user ID.", nameof(request.UserId));
            var entity = await _uow.Users.GetByIdAsync(request.UserId, cancellation)
                         ?? throw new KeyNotFoundException("User not found.");

            entity.FullName = request.FullName;
            entity.IsActive = request.IsActive;
            entity.CompanyId = request.CompanyId;
            entity.Phone = request.Phone;
            entity.Identifier = request.Identifier;
            entity.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                var hash = _hasher.Hash(request.NewPassword);
                entity.PasswordHash = Encoding.UTF8.GetBytes(hash);
            }

            var actuales = (await _uow.UserRoles.GetRolesByUserIdAsync(entity.UserId, cancellation))
                           .Select(ur => ur.RoleId).ToList();
            var porQuitar = actuales.Except(request.RoleIds).ToList();
            var porAgregar = request.RoleIds.Except(actuales).Distinct().ToList();

            foreach (var rid in porQuitar)
            {
                var ur = await _uow.UserRoles.GetAll()
                    .FirstAsync(x => x.UserId == entity.UserId && x.RoleId == rid, cancellation);
                _uow.UserRoles.Remove(ur);
            }

            foreach (var rid in porAgregar)
            {
                var ur = new UserRole { UserId = entity.UserId, RoleId = rid, AssignedAt = DateTime.UtcNow };
                await _uow.UserRoles.AddAsync(ur, cancellation);
            }

            _uow.Users.Update(entity);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellation = default)
        {
            if (id <= 0) throw new ArgumentException("Invalid user ID.", nameof(id));
            var entity = await _uow.Users.GetByIdAsync(id, cancellation)
                         ?? throw new KeyNotFoundException("User not found.");
            _uow.Users.Remove(entity);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task<IEnumerable<AgentDto>> GetByRoleAsync(string roleName, CancellationToken cancellation = default)
        {
            return await _uow.UserRoles
                .GetAll()
                .Where(ur => ur.Role.RoleName == roleName)
                .Select(ur => new AgentDto
                {
                    UserId = ur.User.UserId,
                    FullName = ur.User.FullName!,
                    Email = ur.User.Email!
                })
                .Distinct()
                .ToListAsync(cancellation);
        }
    }
}
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

namespace CustomerService.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly IPresenceService _presence;

        public UserService(IUnitOfWork uow, IPasswordHasher hasher, IPresenceService presence)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _presence = presence ?? throw new ArgumentNullException(nameof(presence));
        }

        public async Task<PagedResponse<UserDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default)
        {
            var query = _uow.Users.GetAll();
            var paged = await PagedList<User>.CreateAsync(query, @params.PageNumber, @params.PageSize, cancellation);
            var users = paged.ToList();
            var userIds = users.Select(u => u.UserId).ToList();

            // 1) Batch: conteos de conversaciones
            var convoCounts = await _uow.Conversations
                .GetAll()
                .Where(c => userIds.Contains(c.ClientUserId ?? 0))
                .GroupBy(c => c.ClientUserId)
                .Select(g => new { UserId = g.Key!.Value, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellation);

            // 2) Batch: últimos onlines
            var lastOnlines = await _presence.GetLastOnlineAsync(userIds, cancellation);

            var dtos = users.Select(u =>
            {
                lastOnlines.TryGetValue(u.UserId, out var lastOnline);
                convoCounts.TryGetValue(u.UserId, out var convoCount);

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
                    LastOnline = lastOnline,
                    IsOnline = lastOnline.HasValue
                                 && (DateTime.UtcNow - lastOnline.Value).TotalMinutes < 5,
                    ClientType = convoCount switch
                    {
                        0 => "Nuevo",
                        > 0 and <= 5 => "Frecuente",
                        _ => "VIP"
                    }
                };
            }).ToList();

            return new PagedResponse<UserDto>(dtos, paged.MetaData);
        }

        public async Task<UserDto> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            if (id <= 0) throw new ArgumentException("Invalid user ID.", nameof(id));
            var u = await _uow.Users.GetByIdAsync(id, cancellation)
                  ?? throw new KeyNotFoundException("User not found.");

            var lastOnline = await _presence.GetLastOnlineAsync(u.UserId, cancellation);
            var convoCount = await _uow.Conversations
                .GetAll()
                .CountAsync(c => c.ClientUserId == u.UserId, cancellation);

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
                LastOnline = lastOnline,
                IsOnline = lastOnline.HasValue
                             && (DateTime.UtcNow - lastOnline.Value).TotalMinutes < 5,
                ClientType = convoCount switch
                {
                    0 => "Nuevo",
                    > 0 and <= 5 => "Frecuente",
                    _ => "VIP"
                }
            };
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.", nameof(request.Email));
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required.", nameof(request.Password));

            if (await _uow.Users.ExistsAsync(u => u.Email == request.Email, cancellation))
                throw new InvalidOperationException("Email already in use.");

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
                ClientType = "Nuevo"
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
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

namespace CustomerService.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;

        public UserService(IUnitOfWork uow, IPasswordHasher hasher)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public async Task<PagedResponse<UserDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default)
        {
            var query = _uow.Users.GetAll();
            var paged = await PagedList<User>.CreateAsync(query, @params.PageNumber, @params.PageSize, cancellation);

            var dtos = paged.Select(u => new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                IsActive = u.IsActive,
                CompanyId = u.CompanyId,
                Phone = u.Phone,
                Identifier = u.Identifier,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            return new PagedResponse<UserDto>(dtos, paged.MetaData);
        }

        public async Task<UserDto> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            if (id <= 0) throw new ArgumentException("Invalid user ID.", nameof(id));

            var u = await _uow.Users.GetByIdAsync(id, cancellation)
                  ?? throw new KeyNotFoundException("User not found.");

            return new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                IsActive = u.IsActive,
                CompanyId = u.CompanyId,
                Phone = u.Phone,
                Identifier = u.Identifier,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
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
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Users.AddAsync(entity, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            return new UserDto
            {
                UserId = entity.UserId,
                FullName = entity.FullName,
                Email = entity.Email,
                IsActive = entity.IsActive,
                CompanyId = entity.CompanyId,
                Phone = entity.Phone,
                Identifier = entity.Identifier,
                CreatedAt = entity.CreatedAt
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
    }
}
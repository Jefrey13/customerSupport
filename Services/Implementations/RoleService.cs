using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Mapster;

namespace CustomerService.API.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roles;
        private readonly IUnitOfWork _uow;

        public RoleService(
            IRoleRepository roles,
            IUnitOfWork uow)
        {
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<PagedResponse<RoleDto>> GetAllAsync(
            PaginationParams @params,
            CancellationToken cancellation = default)
        {
            // Preparamos la consulta
            var query = _uow.Roles.GetAll().OrderBy(r => r.RoleName);

            // Creamos la página
            var paged = await PagedList<AppRole>.CreateAsync(
                query,
                @params.PageNumber,
                @params.PageSize,
                cancellation);

            // Mapeamos a DTOs
            var dtos = paged
                .Select(r => r.Adapt<RoleDto>())
                .ToList();

            // Envolvemos en PagedResponse
            return new PagedResponse<RoleDto>(
                dtos,
                paged.MetaData
            );
        }

        public async Task<RoleDto> GetByIdAsync(
            int id,
            CancellationToken cancellation = default)
        {
            var entity = await _uow.Roles.GetByIdAsync(id, cancellation)
                         ?? throw new KeyNotFoundException($"Role {id} not found.");
            return entity.Adapt<RoleDto>();
        }

        public async Task<RoleDto> CreateAsync(
            CreateRoleRequest request,
            CancellationToken cancellation = default)
        {
            if (await _roles.ExistsAsync(r => r.RoleName == request.RoleName, cancellation))
                throw new ArgumentException("Role name already exists.");

            var entity = request.Adapt<AppRole>();
            await _uow.Roles.AddAsync(entity, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            return entity.Adapt<RoleDto>();
        }

        public async Task UpdateAsync(
            UpdateRoleRequest request,
            CancellationToken cancellation = default)
        {
            var entity = await _roles.GetByIdAsync(request.RoleId, cancellation)
                         ?? throw new KeyNotFoundException($"Role {request.RoleId} not found.");

            entity.RoleName = request.RoleName;
            entity.Description = request.Description;

            _uow.Roles.Update(entity);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task DeleteAsync(
            int id,
            CancellationToken cancellation = default)
        {
            var entity = await _uow.Roles.GetByIdAsync(id, cancellation)
                         ?? throw new KeyNotFoundException($"Role {id} not found.");

            _roles.Remove(entity);
            await _uow.SaveChangesAsync(cancellation);
        }
    }
}
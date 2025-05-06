using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface IRoleService
    {
        Task<PagedResponse<RoleDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default);
        Task<RoleDto> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellation = default);
        Task UpdateAsync(UpdateRoleRequest request, CancellationToken cancellation = default);
        Task DeleteAsync(int id, CancellationToken cancellation = default);
    }
}

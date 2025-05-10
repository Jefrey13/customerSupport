using CustomerService.API.Dtos.ResponseDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface IUserRoleService
    {
        Task<IEnumerable<RoleDto>> GetRolesByUserAsync(int userId, CancellationToken cancellation = default);
        Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellation = default);
        Task RemoveRoleAsync(int userId, int roleId, CancellationToken cancellation = default);
    }
}

using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;
namespace CustomerService.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponse<UserDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default);
        Task<UserDto> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellation = default);
        Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellation = default);
        Task DeleteAsync(int id, CancellationToken cancellation = default);
        Task<IEnumerable<AgentDto>> GetByRoleAsync(string roleName, CancellationToken cancellation = default);
    }
}
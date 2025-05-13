using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Models;

namespace CustomerService.API.Services.Interfaces
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuRequestDtos>> GetMenuByRoles(string roleName, CancellationToken cancellation = default);
    }
}

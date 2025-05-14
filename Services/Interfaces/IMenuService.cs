using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;

namespace CustomerService.API.Services.Interfaces
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuResponseDto>> GetByRolesAsync(
           IEnumerable<string> roleNames,
           CancellationToken cancellation = default
       );
    }
}

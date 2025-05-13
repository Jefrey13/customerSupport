using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IMenuRepository: IGenericRepository<Menu>
    {
        List<Menu> GetMenuByRoles(string roleName, CancellationToken cancellation = default);
    }
}
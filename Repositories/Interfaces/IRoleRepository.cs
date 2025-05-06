using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IRoleRepository : IGenericRepository<AppRole>
    {
        Task<AppRole?> GetByNameAsync(string roleName, CancellationToken cancellation = default);
    }
}

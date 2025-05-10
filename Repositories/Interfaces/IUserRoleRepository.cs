using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IUserRoleRepository : IGenericRepository<UserRole>
    {
        Task<IEnumerable<UserRole>> GetRolesByUserIdAsync(int userId, CancellationToken cancellation = default);
    }
}

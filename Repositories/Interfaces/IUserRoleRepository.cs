using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IUserRoleRepository : IGenericRepository<UserRole>
    {
        Task<IEnumerable<UserRole>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellation = default);
    }
}

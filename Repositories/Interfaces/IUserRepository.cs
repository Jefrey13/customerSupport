using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellation = default);
    }
}

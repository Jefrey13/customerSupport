using CustomerService.API.Data.context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(CustomerSupportDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException(nameof(email));
            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == email, cancellation);
        }
    }
}

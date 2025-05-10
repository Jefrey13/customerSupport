using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(CustomerSupportContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede ser vacío.", nameof(email));

            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == email, cancellation);
        }
    }
}
using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(CustomerSupportContext context) : base(context) { }

        public async Task<IEnumerable<UserRole>> GetRolesByUserIdAsync(
            int userId,
            CancellationToken cancellation = default)
        {
            if (userId <= 0)
                throw new ArgumentException("El UserId debe ser mayor que cero.", nameof(userId));

            return await _dbSet
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .ToListAsync(cancellation);
        }
    }
}
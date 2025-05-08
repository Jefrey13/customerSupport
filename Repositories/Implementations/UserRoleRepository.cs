using CustomerService.API.Data.context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class UserRoleRepository
        : GenericRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(CustomerSupportContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<UserRole>> GetRolesByUserIdAsync(
            Guid userId,
            CancellationToken cancellation = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)      // carga la tabla AppRoles
                .ToListAsync(cancellation);
        }
    }
}

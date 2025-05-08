using CustomerService.API.Data.context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class RoleRepository : GenericRepository<AppRole>, IRoleRepository
    {
        public RoleRepository(CustomerSupportContext context) : base(context) { }

        public async Task<AppRole?> GetByNameAsync(string roleName, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException(nameof(roleName));
            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(r => r.RoleName == roleName, cancellation);
        }
    }
}

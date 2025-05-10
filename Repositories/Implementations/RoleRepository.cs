using CustomerService.API.Data.Context;
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
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("El nombre de rol no puede ser vacío.", nameof(roleName));

            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(r => r.RoleName == roleName, cancellation);
        }
    }
}
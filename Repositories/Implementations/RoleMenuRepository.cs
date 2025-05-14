using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Repositories.Implementations
{
    public class RoleMenuRepository : GenericRepository<RoleMenu>, IRoleMenuRepository
    {
        public RoleMenuRepository(CustomerSupportContext context)
            : base(context)
        {
        }

        public async Task<List<RoleMenu>> GetByRoleNamesAsync(
            IEnumerable<string> roleNames,
            CancellationToken cancellation = default
        )
        {
            if (roleNames == null)
                throw new ArgumentNullException(nameof(roleNames));

            return await _dbSet
                .AsNoTracking()
                .Include(rm => rm.Role)
                .Include(rm => rm.Menu)
                .Where(rm => roleNames.Contains(rm.Role.RoleName!))
                .ToListAsync(cancellation);
        }
    }
}

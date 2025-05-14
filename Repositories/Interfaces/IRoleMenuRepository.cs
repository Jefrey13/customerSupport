using CustomerService.API.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IRoleMenuRepository : IGenericRepository<RoleMenu>
    {
        Task<List<RoleMenu>> GetByRoleNamesAsync(
            IEnumerable<string> roleNames,
            CancellationToken cancellation = default
        );
    }
}

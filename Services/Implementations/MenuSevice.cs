using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Services.Implementations
{
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _uow;
        public MenuService(IUnitOfWork uow)
            => _uow = uow ?? throw new ArgumentNullException(nameof(uow));

        public async Task<IEnumerable<MenuResponseDto>> GetByRolesAsync(
            IEnumerable<string> roleNames,
            CancellationToken cancellation = default
        )
        {
            if (roleNames == null || !roleNames.Any())
                throw new ArgumentException("Debe especificar al menos un rol", nameof(roleNames));

            var entries = await _uow.RoleMenus
                .GetAll()
                .Include(rm => rm.Menu)
                .Include(rm => rm.Role)
                .Where(rm => roleNames.Contains(rm.Role.RoleName))
                .ToListAsync(cancellation);

            var menus = entries
                .Select(rm => rm.Menu)
                .DistinctBy(m => m.MenuId)
                .OrderBy(m => m.Index)
                .Select(m => new MenuResponseDto
                {
                    MenuId = m.MenuId,
                    Name = m.Name,
                    Description = m.Description,
                    Url = m.Url,
                    Index = m.Index,
                    Icon = m.Icon
                });

            return menus;
        }
    }
}
//using CustomerService.API.Data.Context;
//using CustomerService.API.Models;
//using CustomerService.API.Repositories.Interfaces;
//using Microsoft.EntityFrameworkCore;

//namespace CustomerService.API.Repositories.Implementations
//{
//    public class MenuRepository : GenericRepository<Menu>, IMenuRepository
//    {
//        public MenuRepository(CustomerSupportContext context) : base(context) { }

//        public List<Menu> GetMenuByRoles(string roleName, CancellationToken cancellation = default)
//        {
//            var data = _context.Menus.Where(m => m.RoleMenus.Where(me => me.Role.RoleName == roleName));

//            return data.ToList();
//        }
//    }
//}
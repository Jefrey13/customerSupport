//using CustomerService.API.Dtos.RequestDtos;
//using CustomerService.API.Models;
//using CustomerService.API.Repositories.Interfaces;
//using CustomerService.API.Services.Interfaces;

//namespace CustomerService.API.Services.Implementations
//{
//    public class MenuSevice : IMenuService
//    {
//        private readonly IMenuRepository _menuRepository;
//        public MenuSevice(IMenuRepository menuRepository)
//        {
//            _menuRepository = menuRepository;
//        }
//        public Task<IEnumerable<MenuRequestDtos>> GetMenuByRoles(string roleName, CancellationToken cancellation = default)
//        {
//            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException("Especificar el rol del usuario");

//            Menu menus = new Menu();

//            return new MenuRequestDtos
//            {
//                Name = 
//            }
//        }
//    }
//}

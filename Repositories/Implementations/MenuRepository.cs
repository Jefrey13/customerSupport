// CustomerService.API/Repositories/Implementations/MenuRepository.cs
using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;

namespace CustomerService.API.Repositories.Implementations
{
    public class MenuRepository : GenericRepository<Menu>, IMenuRepository
    {
        public MenuRepository(CustomerSupportContext context)
            : base(context)
        {
        }

    }
}
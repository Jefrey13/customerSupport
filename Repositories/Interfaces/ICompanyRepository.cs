using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface ICompanyRepository : IGenericRepository<Company>
    {
        Task<Company?> GetByNameAsync(string name, CancellationToken cancellation = default);
    }
}

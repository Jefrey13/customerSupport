using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IContactRepository : IGenericRepository<Contact>
    {
        Task<IEnumerable<Contact>> SearchByCompanyAsync(string company, CancellationToken cancellation = default);
    }
}

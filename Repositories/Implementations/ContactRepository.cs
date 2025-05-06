using CustomerService.API.Data.context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class ContactRepository : GenericRepository<Contact>, IContactRepository
    {
        public ContactRepository(CustomerSupportDbContext context) : base(context) { }

        public async Task<IEnumerable<Contact>> SearchByCompanyAsync(string company, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(company)) return Array.Empty<Contact>();
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.CompanyName.Contains(company))
                .ToListAsync(cancellation);
        }
    }
}

using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        public CompanyRepository(CustomerSupportContext context) : base(context) { }

        public async Task<Company?> GetByNameAsync(string name, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The company name cannot be empty.", nameof(name));

            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Name == name, cancellation);
        }
    }
}

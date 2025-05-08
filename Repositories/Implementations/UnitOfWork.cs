using CustomerService.API.Data.context;
using CustomerService.API.Repositories.Interfaces;

namespace CustomerService.API.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CustomerSupportContext _context;
        public UnitOfWork(CustomerSupportContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellation = default) =>
            _context.SaveChangesAsync(cancellation);
    }
}

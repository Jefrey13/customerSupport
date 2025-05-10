using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class AttachmentRepository : GenericRepository<Attachment>, IAttachmentRepository
    {
        public AttachmentRepository(CustomerSupportContext context) : base(context) { }

        public async Task<IEnumerable<Attachment>> GetByMessageAsync(int messageId, CancellationToken cancellation = default)
        {
            if (messageId <= 0)
                throw new ArgumentException("El messageId debe ser mayor que cero.", nameof(messageId));

            return await _dbSet
                .AsNoTracking()
                .Where(a => a.MessageId == messageId)
                .ToListAsync(cancellation);
        }
    }
}
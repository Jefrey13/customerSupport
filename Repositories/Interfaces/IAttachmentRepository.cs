using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IAttachmentRepository : IGenericRepository<Attachment>
    {
        Task<IEnumerable<Attachment>> GetByMessageAsync(int messageId, CancellationToken cancellation = default);
    }
}

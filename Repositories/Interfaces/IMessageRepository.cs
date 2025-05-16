using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default);
        Task<Message?> GetByExternalIdAsync(string externalId, CancellationToken cancellation = default);
    }
}
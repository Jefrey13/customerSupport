using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task<IEnumerable<Conversation>> GetByAgentAsync(int agentId, CancellationToken cancellation = default);
        Task<IEnumerable<Conversation>> GetPendingAsync(CancellationToken cancellation = default);
    }
}

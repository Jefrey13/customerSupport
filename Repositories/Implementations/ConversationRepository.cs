using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(CustomerSupportContext context) : base(context) { }

        public async Task<IEnumerable<Conversation>> GetByAgentAsync(int agentId, CancellationToken cancellation = default)
        {
            if (agentId <= 0)
                throw new ArgumentException("El agentId debe ser mayor que cero.", nameof(agentId));

            return await _dbSet
                .AsNoTracking()
                .Where(c => c.AssignedAgent == agentId)
                .Include(c => c.Messages)
                .ToListAsync(cancellation);
        }

        public async Task<IEnumerable<Conversation>> GetPendingAsync(CancellationToken cancellation = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.Status == "PendingHuman" || c.Status == "Bot")
                .ToListAsync(cancellation);
        }
    }
}
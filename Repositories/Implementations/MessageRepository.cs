using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(CustomerSupportContext context) : base(context) { }

        public async Task<IEnumerable<Message>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0)
                throw new ArgumentException("El conversationId debe ser mayor que cero.", nameof(conversationId));

            return await _dbSet
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.Attachments)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellation);
        }

        public async Task<Message?> GetByExternalIdAsync(string externalId, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("El externalId no puede estar vacío.", nameof(externalId));

            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ExternalId == externalId, cancellation);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;

namespace CustomerService.API.Services.Implementations
{
    /// <summary>
    /// Encapsula la lógica de iniciar conversaciones, listarlas,
    /// asignar agentes, obtener detalle y cerrarlas.
    /// </summary>
    public class ConversationService : IConversationService
    {
        private readonly IUnitOfWork _uow;

        public ConversationService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<ConversationDto> StartAsync(StartConversationRequest request, CancellationToken cancellation = default)
        {
            var conv = new Conversation
            {
                CompanyId = request.CompanyId,
                ClientUserId = request.ClientUserId,
                Status = "Bot",
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Conversations.AddAsync(conv, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            return new ConversationDto
            {
                ConversationId = conv.ConversationId,
                CompanyId = conv.CompanyId,
                ClientUserId = conv.ClientUserId,
                Status = conv.Status,
                CreatedAt = conv.CreatedAt
            };
        }

        public async Task<IEnumerable<ConversationDto>> GetPendingAsync(CancellationToken cancellation = default)
        {
            var list = await _uow.Conversations.GetPendingAsync(cancellation);
            return list.Select(c => new ConversationDto
            {
                ConversationId = c.ConversationId,
                CompanyId = c.CompanyId,
                ClientUserId = c.ClientUserId,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                AssignedAgent = c.AssignedAgent
            });
        }

        public async Task AssignAgentAsync(int conversationId, int agentUserId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));

            var conv = await _uow.Conversations.GetByIdAsync(conversationId, cancellation)
                       ?? throw new KeyNotFoundException("Conversation not found.");

            conv.AssignedAgent = agentUserId;
            conv.AssignedAt = DateTime.UtcNow;
            conv.Status = "Human";

            _uow.Conversations.Update(conv);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task<ConversationDto?> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            if (id <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(id));

            var c = await _uow.Conversations.GetByIdAsync(id, cancellation);
            if (c == null) return null;

            return new ConversationDto
            {
                ConversationId = c.ConversationId,
                CompanyId = c.CompanyId,
                ClientUserId = c.ClientUserId,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                AssignedAgent = c.AssignedAgent,
                AssignedAt = c.AssignedAt
            };
        }

        public async Task CloseAsync(int conversationId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));

            var c = await _uow.Conversations.GetByIdAsync(conversationId, cancellation)
                  ?? throw new KeyNotFoundException("Conversation not found.");

            c.Status = "Closed";
            _uow.Conversations.Update(c);
            await _uow.SaveChangesAsync(cancellation);
        }
    }
}
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
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Services.Implementations
{
    public class ConversationService : IConversationService
    {
        private readonly IUnitOfWork _uow;

        public ConversationService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<IEnumerable<ConversationDto>> GetAllAsync(CancellationToken cancellation = default)
        {
            var conversations = await _uow.Conversations
                .GetAll()
                .Include(c => c.Messages)
                .Include(c => c.ClientUser)
                .ToListAsync(cancellation);

            return conversations.Select(ToDto);
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

            return ToDto(conv);
        }

        public async Task<IEnumerable<ConversationDto>> GetPendingAsync(CancellationToken cancellation = default)
        {
            var conversations = await _uow.Conversations
                .GetAll()
                .Where(c => c.Status == "PendingHuman" || c.Status == "Bot")
                .Include(c => c.Messages)
                .Include(c => c.ClientUser)
                .ToListAsync(cancellation);

            return conversations.Select(ToDto);
        }

        public async Task AssignAgentAsync(int conversationId, int agentUserId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0)
                throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));

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
            if (id <= 0)
                throw new ArgumentException("Invalid conversation ID.", nameof(id));

            var c = await _uow.Conversations
                .GetAll()
                .Include(c => c.Messages)
                .Include(c => c.ClientUser)
                .SingleOrDefaultAsync(c => c.ConversationId == id, cancellation);

            return c is null ? null : ToDto(c);
        }

        public async Task CloseAsync(int conversationId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0)
                throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));

            var c = await _uow.Conversations.GetByIdAsync(conversationId, cancellation)
                  ?? throw new KeyNotFoundException("Conversation not found.");

            c.Status = "Closed";
            _uow.Conversations.Update(c);
            await _uow.SaveChangesAsync(cancellation);
        }

        private static ConversationDto ToDto(Conversation c) =>
            new ConversationDto
            {
                ConversationId = c.ConversationId,
                CompanyId = c.CompanyId,
                ClientUserId = c.ClientUserId,
                ClientUserName = c.ClientUser?.FullName,
                AssignedAgent = c.AssignedAgent,
                AssignedAgentName = c.AssignedAgentNavigation?.FullName,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                AssignedAt = c.AssignedAt,
                ContactName = c.ClientUser?.FullName,
                ContactNumber = c.ClientUser?.Phone,
                TotalMensajes = c.Messages.Count,
                UltimaActividad = c.Messages.Any() ? c.Messages.Max(m => m.CreatedAt) : c.CreatedAt,
                Duracion = DateTime.UtcNow - c.CreatedAt
            };
    }
}
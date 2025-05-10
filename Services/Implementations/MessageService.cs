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
    /// Maneja el envío y la recuperación de mensajes por conversación,
    /// incluyendo el orden cronológico y carga de adjuntos.
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;

        public MessageService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageRequest request, CancellationToken cancellation = default)
        {
            if (request.ConversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(request.ConversationId));
            if (string.IsNullOrWhiteSpace(request.MessageType)) throw new ArgumentException("MessageType is required.", nameof(request.MessageType));

            var msg = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                Content = request.Content,
                MessageType = request.MessageType,
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Messages.AddAsync(msg, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            return new MessageDto
            {
                MessageId = msg.MessageId,
                ConversationId = msg.ConversationId,
                SenderId = msg.SenderId,
                Content = msg.Content,
                MessageType = msg.MessageType,
                CreatedAt = msg.CreatedAt
            };
        }

        public async Task<IEnumerable<MessageDto>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));

            var list = await _uow.Messages.GetByConversationAsync(conversationId, cancellation);
            return list.Select(m => new MessageDto
            {
                MessageId = m.MessageId,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                Content = m.Content,
                MessageType = m.MessageType,
                CreatedAt = m.CreatedAt,
                Attachments = m.Attachments?.Select(a => new AttachmentDto
                {
                    AttachmentId = a.AttachmentId,
                    MessageId = a.MessageId,
                    MediaId = a.MediaId,
                    FileName = a.FileName,
                    MediaUrl = a.MediaUrl,
                }).ToList() ?? new List<AttachmentDto>()
            });
        }
    }
}
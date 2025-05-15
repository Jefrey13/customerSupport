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
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;

        public MessageService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageRequest request, CancellationToken cancellation = default)
        {
            if (request.ConversationId <= 0)
                throw new ArgumentException("ConversationId must be greater than zero.", nameof(request.ConversationId));

            if (string.IsNullOrWhiteSpace(request.MessageType))
                throw new ArgumentException("MessageType is required.", nameof(request.MessageType));

            var msg = new Models.Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                Content = request.Content,
                MessageType = request.MessageType,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Messages.AddAsync(msg, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            // Si viene un archivo en request.File, debe haberse manejado antes de invocar este servicio:
            //   subida a storage, creación de Attachment y guardado con _uow.Attachments

            return new MessageDto
            {
                MessageId = msg.MessageId,
                ConversationId = msg.ConversationId,
                SenderId = msg.SenderId,
                Content = msg.Content,
                MessageType = msg.MessageType,
                CreatedAt = msg.CreatedAt,
                Attachments = msg.Attachments.Select(a => new AttachmentDto
                {
                    AttachmentId = a.AttachmentId,
                    MessageId = a.MessageId,
                    MediaId = a.MediaId,
                    FileName = a.FileName,
                    MimeType = a.MimeType,
                    MediaUrl = a.MediaUrl
                }).ToList()
            };
        }

        public async Task<IEnumerable<MessageDto>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0)
                throw new ArgumentException("ConversationId must be greater than zero.", nameof(conversationId));

            var messages = await _uow.Messages.GetByConversationAsync(conversationId, cancellation);

            return messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    ConversationId = m.ConversationId,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    MessageType = m.MessageType,
                    CreatedAt = m.CreatedAt,
                    Attachments = m.Attachments.Select(a => new AttachmentDto
                    {
                        AttachmentId = a.AttachmentId,
                        MessageId = a.MessageId,
                        MediaId = a.MediaId,
                        FileName = a.FileName,
                        MimeType = a.MimeType,
                        MediaUrl = a.MediaUrl
                    }).ToList()
                });
        }
    }
}
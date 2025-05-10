using System;
using System.Collections.Generic;
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
    /// Gestiona la subida y recuperación de adjuntos (mediaId, URL, nombre).
    /// </summary>
    public class AttachmentService : IAttachmentService
    {
        private readonly IUnitOfWork _uow;

        public AttachmentService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<AttachmentDto> UploadAsync(UploadAttachmentRequest request, CancellationToken cancellation = default)
        {
            if (request.MessageId <= 0) throw new ArgumentException("Invalid message ID.", nameof(request.MessageId));
            if (string.IsNullOrWhiteSpace(request.MediaId)) throw new ArgumentException("MediaId is required.", nameof(request.MediaId));

            var a = new Attachment
            {
                MessageId = request.MessageId,
                MediaId = request.MediaId,
                FileName = request.FileName,
                MediaUrl = request.MediaUrl,
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Attachments.AddAsync(a, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            return new AttachmentDto
            {
                AttachmentId = a.AttachmentId,
                MessageId = a.MessageId,
                MediaId = a.MediaId,
                FileName = a.FileName,
                MediaUrl = a.MediaUrl,
            };
        }

        public async Task<IEnumerable<AttachmentDto>> GetByMessageAsync(int messageId, CancellationToken cancellation = default)
        {
            if (messageId <= 0) throw new ArgumentException("Invalid message ID.", nameof(messageId));

            var list = await _uow.Attachments.GetByMessageAsync(messageId, cancellation);
            var dtos = new List<AttachmentDto>();
            foreach (var a in list)
            {
                dtos.Add(new AttachmentDto
                {
                    AttachmentId = a.AttachmentId,
                    MessageId = a.MessageId,
                    MediaId = a.MediaId,
                    FileName = a.FileName,
                    MediaUrl = a.MediaUrl,
                });
            }
            return dtos;
        }
    }
}
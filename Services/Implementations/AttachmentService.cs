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
    public class AttachmentService : IAttachmentService
    {
        private readonly IUnitOfWork _uow;

        public AttachmentService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<AttachmentDto> UploadAsync(UploadAttachmentRequest request, CancellationToken cancellation = default)
        {
            if (request.MessageId <= 0)
                throw new ArgumentException("MessageId must be greater than zero.", nameof(request.MessageId));

            if (string.IsNullOrWhiteSpace(request.MediaId))
                throw new ArgumentException("MediaId is required.", nameof(request.MediaId));

            var attachment = new Attachment
            {
                MessageId = request.MessageId,
                MediaId = request.MediaId,
                FileName = request.FileName,
                MimeType = request.MimeType,
                MediaUrl = request.MediaUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Attachments.AddAsync(attachment, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            return new AttachmentDto
            {
                AttachmentId = attachment.AttachmentId,
                MessageId = attachment.MessageId,
                MediaId = attachment.MediaId,
                FileName = attachment.FileName,
                MimeType = attachment.MimeType,
                MediaUrl = attachment.MediaUrl
            };
        }

        public async Task<IEnumerable<AttachmentDto>> GetByMessageAsync(int messageId, CancellationToken cancellation = default)
        {
            if (messageId <= 0)
                throw new ArgumentException("MessageId must be greater than zero.", nameof(messageId));

            var attachments = await _uow.Attachments.GetByMessageAsync(messageId, cancellation);

            var result = new List<AttachmentDto>();
            foreach (var a in attachments)
            {
                result.Add(new AttachmentDto
                {
                    AttachmentId = a.AttachmentId,
                    MessageId = a.MessageId,
                    MediaId = a.MediaId,
                    FileName = a.FileName,
                    MimeType = a.MimeType,
                    MediaUrl = a.MediaUrl
                });
            }
            return result;
        }
    }
}
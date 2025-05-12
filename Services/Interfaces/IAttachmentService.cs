using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface IAttachmentService
    {
        /// <summary>
        /// Sube un nuevo adjunto y lo asocia a un mensaje existente.
        /// </summary>
        Task<AttachmentDto> UploadAsync(UploadAttachmentRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// Obtiene todos los adjuntos asociados a un mensaje dado.
        /// </summary>
        Task<IEnumerable<AttachmentDto>> GetByMessageAsync(int messageId, CancellationToken cancellation = default);
    }
}

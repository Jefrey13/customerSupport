using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Dtos.RequestDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<AttachmentDto> UploadAsync(UploadAttachmentRequest request, CancellationToken cancellation = default);
        Task<IEnumerable<AttachmentDto>> GetByMessageAsync(int messageId, CancellationToken cancellation = default);
    }
}

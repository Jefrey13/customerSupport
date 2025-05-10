using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto> SendMessageAsync(SendMessageRequest request, CancellationToken cancellation = default);
        Task<IEnumerable<MessageDto>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default);
    }
}

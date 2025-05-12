using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Dtos.RequestDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface IConversationService
    {
        Task<IEnumerable<ConversationDto>> GetAllAsync(CancellationToken cancellation = default);
        Task<ConversationDto> StartAsync(StartConversationRequest request, CancellationToken cancellation = default);
        Task<IEnumerable<ConversationDto>> GetPendingAsync(CancellationToken cancellation = default);
        Task AssignAgentAsync(int conversationId, int agentUserId, CancellationToken cancellation = default);
        Task<ConversationDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task CloseAsync(int conversationId, CancellationToken cancellation = default);
    }
}

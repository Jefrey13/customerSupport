using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto> SendMessageAsync(SendMessageRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// Obtiene todos los mensajes de una conversación, ordenados cronológicamente, incluyendo adjuntos.
        /// </summary>
        Task<IEnumerable<MessageDto>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default);
    }
}

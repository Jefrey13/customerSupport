using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace CustomerService.API.Utils
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// El cliente llama a este método al iniciar la UI de chat
        /// para suscribirse al grupo de la conversación.
        /// </summary>
        public Task JoinConversation(string conversationId)
            => Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

        /// <summary>
        /// Opcional: Permitir que el cliente envíe mensajes
        /// directos al grupo sin pasar por la API REST.
        /// </summary>
        public Task SendMessageToConversation(string conversationId, object message)
            => Clients.Group(conversationId).SendAsync("ReceiveMessage", message);
    }
}

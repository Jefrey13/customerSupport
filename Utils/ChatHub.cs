using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CustomerService.API.Utils
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// Permite que un cliente se suscriba al grupo de una conversación
        /// para recibir los mensajes en tiempo real.
        /// </summary>
        /// <param name="conversationId">ID de la conversación</param>
        public Task JoinConversation(string conversationId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

        /// <summary>
        /// Permite que un cliente abandone el grupo de una conversación
        /// al cerrar la UI de chat o cambiar de conversación.
        /// </summary>
        /// <param name="conversationId">ID de la conversación</param>
        public Task LeaveConversation(string conversationId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);

        /// <summary>
        /// Método opcional para que un cliente envíe un mensaje directo al grupo
        /// sin pasar por la API REST. Dispara el evento "ReceiveMessage".
        /// </summary>
        /// <param name="conversationId">ID de la conversación</param>
        /// <param name="message">Carga útil del mensaje</param>
        public Task SendMessageToConversation(string conversationId, object message) =>
            Clients.Group(conversationId).SendAsync("ReceiveMessage", message);
    }
}
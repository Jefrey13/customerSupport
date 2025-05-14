using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CustomerService.API.Utils
{
    public class ChatHub : Hub
    {
        public Task JoinConversation(string conversationId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

        public Task LeaveConversation(string conversationId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);

        public Task SendMessageToConversation(string conversationId, object message) =>
            Clients.Group(conversationId).SendAsync("ReceiveMessage", message);

        public Task BroadcastConversationCreated(object conversationDto) =>
            Clients.All.SendAsync("ConversationCreated", conversationDto);
    }
}
using CustomerService.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CustomerService.API.Hubs
{
    public class NotificationsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // claim NameIdentifier es el UserId
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userId, out var uid))
            {
                // Añadimos esta conexión al grupo del usuario
                await Groups.AddToGroupAsync(Context.ConnectionId, uid.ToString());
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userId, out var uid))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, uid.ToString());
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

}

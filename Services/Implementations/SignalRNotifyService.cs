using CustomerService.API.Hubs;
using CustomerService.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CustomerService.API.Services.Implementations
{
    public class SignalRNotifyService : ISignalRNotifyService
    {
        private readonly IHubContext<NotificationsHub> _hub;

        public SignalRNotifyService(IHubContext<NotificationsHub> hub)
        {
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        public Task NotifyUserAsync(int userId, string method, object payload)
        {
            // Usamos el group con el nombre del userId para enviar sólo a sus conexiones
            return _hub.Clients.Group(userId.ToString())
                       .SendAsync(method, payload);
        }
    }
}

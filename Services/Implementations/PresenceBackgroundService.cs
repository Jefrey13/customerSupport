using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomerService.API.Services.Implementations
{
    public class PresenceBackgroundService : BackgroundService
    {
        private readonly ILogger<PresenceBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public PresenceBackgroundService(ILogger<PresenceBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PresenceBackgroundService iniciado.");
            while (!stoppingToken.IsCancellationRequested)
            {
                // - Persistir _lastOnline en base de datos
                // - Eliminar entradas muy antiguas
                // - Emitir eventos de desconexión por tiempo de inactividad

                await Task.Delay(_checkInterval, stoppingToken);
            }
            _logger.LogInformation("PresenceBackgroundService detenido.");
        }
    }
}
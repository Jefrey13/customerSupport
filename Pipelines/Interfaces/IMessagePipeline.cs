using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Pipelines.Interfaces
{
    /// <summary>
    /// Orquesta el flujo: DB, IA, WhatsApp y SignalR.
    /// </summary>
    public interface IMessagePipeline
    {
        Task ProcessIncomingAsync(string fromPhone, string externalId,
                          string? text, string? mediaId,
                          string? mimeType, string? caption,
                          CancellationToken ct = default);
    }
}
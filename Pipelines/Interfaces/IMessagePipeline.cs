using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;

namespace CustomerService.API.Pipelines.Interfaces
{
    /// <summary>
    /// Orquesta el flujo: DB, IA, WhatsApp y SignalR.
    /// </summary>
    public interface IMessagePipeline
    {
        //Task ProcessIncomingAsync(string fromPhone, string externalId,
        //                  string? text, string? mediaId,
        //                  string? mimeType, string? caption,
        //                  CancellationToken ct = default);
        Task ProcessIncomingAsync(ChangeValue value, CancellationToken ct = default);

    }
}
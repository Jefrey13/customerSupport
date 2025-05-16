using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Services.Interfaces
{
    /// <summary>
    /// Envía mensajes de texto y media a través de la WhatsApp Cloud API.
    /// </summary>
    public interface IWhatsAppService
    {
        // Envío de texto “suavo” (sin persistencia ni SignalR)
        Task SendTextAsync(string toPhone, string text);

        // Envío de texto en contexto de conversación
        Task SendTextAsync(
            int conversationId,
            int senderId,
            string text,
            CancellationToken cancellation = default);

        // Subida de bytes a media_id
        Task<string> UploadMediaAsync(byte[] data, string mimeType);

        // Envío de media a un número determinado (requiere mediaId)
        Task SendMediaAsync(
            string toPhone,
            string mediaId,
            string mimeType,
            string? caption = null);

        // **Nuevo**: Envío de media en contexto de conversación,
        // guarda mensaje y attachment en BD, sube bytes y notifica SignalR.
        Task SendMediaAsync(
            int conversationId,
            int senderId,
            byte[] file,
            string filename,
            string mimeType,
            string? caption = null,
            CancellationToken cancellation = default);
    }
}
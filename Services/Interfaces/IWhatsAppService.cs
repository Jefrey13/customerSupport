namespace CustomerService.API.Services.Interfaces
{
    /// <summary>
    /// Envía mensajes de texto y media a través de la WhatsApp Cloud API.
    /// </summary>
    public interface IWhatsAppService
    {
        Task SendTextAsync(string toPhone, string text);
        Task<string> UploadMediaAsync(byte[] data, string mimeType);
        Task SendMediaAsync(string toPhone, string mediaId, string mimeType, string? caption = null);
    }
}

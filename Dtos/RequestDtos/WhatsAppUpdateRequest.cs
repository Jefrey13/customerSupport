namespace CustomerService.API.Dtos.RequestDtos
{
    /// <summary>
    /// Representa la carga que envía WhatsApp Cloud API en cada webhook.
    /// </summary>
    public class WhatsAppUpdateRequest
    {
        public List<Entry> Entry { get; set; } = new();
    }
}
namespace CustomerService.API.Dtos.RequestDtos
{
    /// <summary>
    /// Datos para que un agente envíe un mensaje (texto o fichero).
    /// </summary>
    public class SendMessageRequest
    {
        public Guid SenderId { get; set; }
        public string? Content { get; set; }
        public string MessageType { get; set; } = "Text";
        public string? Caption { get; set; }
        public IFormFile? File { get; set; }
    }
}

namespace CustomerService.API.Dtos.RequestDtos
{
    /// <summary>
    /// Asigna un agente y actualiza el estado de la conversación.
    /// </summary>
    public class UpdateConversationRequest
    {
        public Guid AssignedAgent { get; set; }
        public string Status { get; set; } = null!;
    }
}

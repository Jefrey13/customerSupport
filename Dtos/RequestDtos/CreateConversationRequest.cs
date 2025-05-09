namespace CustomerService.API.Dtos.RequestDtos
{
    /// <summary>
    /// Datos para que un agente abra manualmente una nueva conversación.
    /// </summary>
    public class CreateConversationRequest
    {
        /// <summary>
        /// El ID del contacto con el que se va a iniciar la conversación.
        /// </summary>
        public Guid ContactId { get; set; }

        /// <summary>
        /// (Opcional) El GUID del agente al que se asignará esta conversación.
        /// </summary>
        public Guid? AssignedAgent { get; set; }
    }
}

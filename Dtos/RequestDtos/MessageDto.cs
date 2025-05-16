using System;
using System.Collections.Generic;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string? Content { get; set; }
        public string MessageType { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";     // <-- Estado actual (sent, delivered, read…)
        public DateTime? DeliveredAt { get; set; }          // <-- Opcional, cuándo fue entregado
        public DateTime? ReadAt { get; set; }          // <-- Opcional, cuándo fue leído
        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}

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
        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}
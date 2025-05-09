namespace CustomerService.API.Dtos.ResponseDtos
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string? Content { get; set; }
        public string? Caption { get; set; }
        public string MessageType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}

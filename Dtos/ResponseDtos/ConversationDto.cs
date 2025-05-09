namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public Guid ContactId { get; set; }
        public Guid? AssignedAgent { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}

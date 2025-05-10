namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public int? CompanyId { get; set; }
        public int? ClientUserId { get; set; }
        public int? AssignedAgent { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
    }
}
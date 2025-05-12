namespace CustomerService.API.Dtos.RequestDtos
{
    public class AgentDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}

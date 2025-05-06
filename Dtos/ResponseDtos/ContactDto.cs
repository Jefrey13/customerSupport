namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ContactDto
    {
        public Guid ContactId { get; init; }
        public string CompanyName { get; init; } = "";
        public string ContactName { get; init; } = "";
        public string Email { get; init; } = "";
        public string? Phone { get; init; }
        public string? Country { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}

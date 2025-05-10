namespace CustomerService.API.Dtos.ResponseDtos
{
    public class CompanyDto
    {
        public int CompanyId { get; init; }
        public string Name { get; init; } = "";
        public DateTime CreatedAt { get; init; }
    }
}
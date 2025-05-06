namespace CustomerService.API.Dtos.ResponseDtos
{
    public class RoleDto
    {
        public int RoleId { get; init; }
        public string RoleName { get; init; } = "";
        public string? Description { get; init; }
    }
}

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class UserDto
    {
        public Guid UserId { get; init; }
        public string FullName { get; init; } = "";
        public string Email { get; init; } = "";
        public bool IsActive { get; init; }
        public IEnumerable<int>? RoleIds { get; init; }
        public Guid ContactId { get; init; }
    }
}

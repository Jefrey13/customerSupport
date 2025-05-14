namespace CustomerService.API.Dtos.ResponseDtos
{
    public class UserDto
    {
        public int UserId { get; init; }
        public string FullName { get; init; } = "";
        public string Email { get; init; } = "";
        public bool IsActive { get; init; }
        public int? CompanyId { get; init; }
        public string? Phone { get; init; }
        public string? Identifier { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        public string? ImageUrl { get; init; }
        public string ClientType { get; init; } = "Nuevo";
        public DateTime? LastOnline { get; init; }
        public bool IsOnline { get; init; }

        public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }
}
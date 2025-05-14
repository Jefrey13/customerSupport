using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class CreateUserRequest
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(3), MaxLength(100)]
        public string Password { get; set; } = "";

        public int CompanyId { get; set; } = 1;

        public string? Phone { get; set; }

        public string? Identifier { get; set; }

        public List<int> RoleIds { get; set; } = new() { 1 };

        public string? ImageUrl { get; set; }
    }
}
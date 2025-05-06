using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class CreateUserRequest
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(8), MaxLength(100)]
        public string Password { get; set; } = "";

        public IEnumerable<int>? RoleIds { get; set; }
    }
}

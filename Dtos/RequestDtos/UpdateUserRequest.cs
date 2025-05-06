using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class UpdateUserRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [EmailAddress]
        public string? Email { get; set; }

        public IEnumerable<int>? RoleIds { get; set; }
    }
}

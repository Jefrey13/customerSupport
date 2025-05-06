using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class RegisterRequest
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(8), MaxLength(100)]
        public string Password { get; set; } = "";

        // Contact data
        [Required, StringLength(150)]
        public string CompanyName { get; set; } = "";

        [Required, StringLength(100)]
        public string ContactName { get; set; } = "";

        [Phone]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }
    }
}

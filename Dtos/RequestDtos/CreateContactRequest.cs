using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class CreateContactRequest
    {
        [Required, StringLength(150)]
        public string CompanyName { get; set; } = "";

        [Required, StringLength(100)]
        public string ContactName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Phone]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }
    }
}

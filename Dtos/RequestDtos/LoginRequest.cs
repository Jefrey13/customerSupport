using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class LoginRequest
    {
        [Required]  
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }

}

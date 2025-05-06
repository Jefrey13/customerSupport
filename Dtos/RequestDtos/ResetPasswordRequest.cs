using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = "";

        [Required, MinLength(8), MaxLength(100)]
        public string NewPassword { get; set; } = "";
    }
}

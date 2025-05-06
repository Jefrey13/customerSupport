using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; } = "";
    }
}

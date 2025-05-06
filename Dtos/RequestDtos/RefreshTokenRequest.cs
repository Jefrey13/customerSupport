using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = "";
    }
}

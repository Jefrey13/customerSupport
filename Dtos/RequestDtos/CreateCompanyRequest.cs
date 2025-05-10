using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class CreateCompanyRequest
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = "";
    }
}

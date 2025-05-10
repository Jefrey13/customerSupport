using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class UpdateCompanyRequest
    {
        [Required]
        public int CompanyId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = "";
    }
}

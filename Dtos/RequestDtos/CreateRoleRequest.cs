using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class CreateRoleRequest
    {
        [Required, StringLength(50)]
        public string RoleName { get; set; } = "";

        [StringLength(200)]
        public string? Description { get; set; }
    }
}

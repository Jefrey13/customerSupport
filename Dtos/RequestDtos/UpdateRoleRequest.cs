using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class UpdateRoleRequest
    {
        [Required]
        public int RoleId { get; set; }

        [Required, StringLength(50)]
        public string RoleName { get; set; } = "";

        [StringLength(200)]
        public string? Description { get; set; }
    }
}

namespace CustomerService.API.Models
{
    public class RoleMenu
    {
        public int MenuId { get; set; }
        public int RoleId { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public virtual AppRole Role { get; set; } = null!;

        public virtual Menu Menu { get; set; } = null!;
    }
}
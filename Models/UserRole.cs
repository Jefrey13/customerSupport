using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Models;

[PrimaryKey("UserId", "RoleId")]
[Table("UserRoles", Schema = "auth")]
public partial class UserRole
{
    [Key]
    public Guid UserId { get; set; }

    [Key]
    public int RoleId { get; set; }

    public DateTime AssignedAt { get; set; }

    public Guid AssignedBy { get; set; }

    [ForeignKey("AssignedBy")]
    [InverseProperty("UserRoleAssignedByNavigations")]
    public virtual User AssignedByNavigation { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("UserRoles")]
    public virtual AppRole Role { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserRoleUsers")]
    public virtual User User { get; set; } = null!;
}

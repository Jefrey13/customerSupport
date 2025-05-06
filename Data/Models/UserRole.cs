using System;
using System.Collections.Generic;

namespace CustomerService.API.Data.Models;

public partial class UserRole
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime AssignedAt { get; set; }

    public Guid AssignedBy { get; set; }

    public virtual User AssignedByNavigation { get; set; } = null!;

    public virtual AppRole Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

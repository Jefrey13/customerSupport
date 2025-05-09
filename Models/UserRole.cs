﻿using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class UserRole
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual AppRole Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

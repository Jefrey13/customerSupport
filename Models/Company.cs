using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

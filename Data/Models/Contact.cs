using System;
using System.Collections.Generic;

namespace CustomerService.API.Data.Models;

public partial class Contact
{
    public Guid ContactId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Country { get; set; }

    public DateTime CreatedAt { get; set; }
}

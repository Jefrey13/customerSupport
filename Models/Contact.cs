using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Models;

[Table("Contacts", Schema = "crm")]
public partial class Contact
{
    [Key]
    public Guid ContactId { get; set; }

    [StringLength(150)]
    public string CompanyName { get; set; } = null!;

    [StringLength(100)]
    public string ContactName { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    public DateTime CreatedAt { get; set; }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Models;

[Table("Users", Schema = "auth")]
[Index("Email", Name = "UQ__Users__A9D10534BABBE0AD", IsUnique = true)]
public partial class User
{
    [Key]
    public Guid UserId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [MaxLength(256)]
    public byte[] PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; }

    public Guid SecurityStamp { get; set; }

    public Guid ConcurrencyStamp { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<AppRole> AppRoleCreatedByNavigations { get; set; } = new List<AppRole>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<AppRole> AppRoleUpdatedByNavigations { get; set; } = new List<AppRole>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<AuthToken> AuthTokenCreatedByNavigations { get; set; } = new List<AuthToken>();

    [InverseProperty("User")]
    public virtual ICollection<AuthToken> AuthTokenUsers { get; set; } = new List<AuthToken>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("InverseCreatedByNavigation")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; } = new List<User>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("InverseUpdatedByNavigation")]
    public virtual User? UpdatedByNavigation { get; set; }

    [InverseProperty("AssignedByNavigation")]
    public virtual ICollection<UserRole> UserRoleAssignedByNavigations { get; set; } = new List<UserRole>();

    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoleUsers { get; set; } = new List<UserRole>();
}

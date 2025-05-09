using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

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

    public virtual ICollection<AppRole> AppRoleCreatedByNavigations { get; set; } = new List<AppRole>();

    public virtual ICollection<AppRole> AppRoleUpdatedByNavigations { get; set; } = new List<AppRole>();

    public virtual ICollection<AuthToken> AuthTokenCreatedByNavigations { get; set; } = new List<AuthToken>();

    public virtual ICollection<AuthToken> AuthTokenUsers { get; set; } = new List<AuthToken>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ICollection<UserRole> UserRoleAssignedByNavigations { get; set; } = new List<UserRole>();

    public virtual ICollection<UserRole> UserRoleUsers { get; set; } = new List<UserRole>();
}

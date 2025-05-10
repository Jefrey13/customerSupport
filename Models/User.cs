using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public byte[]? PasswordHash { get; set; }

    public bool IsActive { get; set; }

    public Guid SecurityStamp { get; set; }

    public Guid ConcurrencyStamp { get; set; }

    public int? CompanyId { get; set; }

    public string? Phone { get; set; }

    public string? Identifier { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<AuthToken> AuthTokens { get; set; } = new List<AuthToken>();

    public virtual Company? Company { get; set; }

    public virtual ICollection<Conversation> ConversationAssignedAgentNavigations { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> ConversationAssignedByNavigations { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> ConversationClientUsers { get; set; } = new List<Conversation>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

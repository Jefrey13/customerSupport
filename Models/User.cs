using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerService.API.Models
{
    public partial class User
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public byte[]? PasswordHash { get; set; }
        public bool DataRequested { get; set; } = false;
        public bool IsActive { get; set; }
        public Guid SecurityStamp { get; set; }
        public Guid ConcurrencyStamp { get; set; }
        public int? CompanyId { get; set; }
        public string? Phone { get; set; }
        public string? Identifier { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public virtual ICollection<AuthToken> AuthTokens { get; set; } = new List<AuthToken>();
        public virtual Company? Company { get; set; }
        public virtual ICollection<Conversation> ConversationAssignedAgentNavigations { get; set; } = new List<Conversation>();
        public virtual ICollection<Conversation> ConversationAssignedByNavigations { get; set; } = new List<Conversation>();
        public virtual ICollection<Conversation> ConversationClientUsers { get; set; } = new List<Conversation>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        [NotMapped]
        public string ClientType { get; set; } = "Nuevo";

        [NotMapped]
        public DateTime? LastOnline { get; set; }

        [NotMapped]
        public bool IsOnline
            => LastOnline.HasValue
               && (DateTime.UtcNow - LastOnline.Value).TotalMinutes < 5;
    }
}

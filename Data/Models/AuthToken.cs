using System;
using System.Collections.Generic;

namespace CustomerService.API.Data.Models;

public partial class AuthToken
{
    public Guid TokenId { get; set; }

    public Guid UserId { get; set; }

    public string TokenType { get; set; } = null!;

    public string? JwtId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public bool Used { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public string? IpAddress { get; set; }

    public string? DeviceInfo { get; set; }

    public Guid CreatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<AuthToken> InverseReplacedByToken { get; set; } = new List<AuthToken>();

    public virtual AuthToken? ReplacedByToken { get; set; }

    public virtual User User { get; set; } = null!;
}

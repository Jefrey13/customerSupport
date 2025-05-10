using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class AuthToken
{
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string TokenType { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public bool Used { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

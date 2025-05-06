using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Models;

[Table("AuthTokens", Schema = "auth")]
[Index("UserId", "TokenType", Name = "IX_AuthTokens_User_Type")]
public partial class AuthToken
{
    [Key]
    public Guid TokenId { get; set; }

    public Guid UserId { get; set; }

    [StringLength(50)]
    public string TokenType { get; set; } = null!;

    [StringLength(100)]
    public string? JwtId { get; set; }

    [StringLength(500)]
    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public bool Used { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    [StringLength(200)]
    public string? DeviceInfo { get; set; }

    public Guid CreatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    [InverseProperty("AuthTokenCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [InverseProperty("ReplacedByToken")]
    public virtual ICollection<AuthToken> InverseReplacedByToken { get; set; } = new List<AuthToken>();

    [ForeignKey("ReplacedByTokenId")]
    [InverseProperty("InverseReplacedByToken")]
    public virtual AuthToken? ReplacedByToken { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AuthTokenUsers")]
    public virtual User User { get; set; } = null!;
}

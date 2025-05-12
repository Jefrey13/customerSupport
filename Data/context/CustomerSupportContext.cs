using System;
using System.Collections.Generic;
using CustomerService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Data.Context;

public partial class CustomerSupportContext : DbContext
{
    public CustomerSupportContext()
    {
    }

    public CustomerSupportContext(DbContextOptions<CustomerSupportContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppRole> AppRoles { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AuthToken> AuthTokens { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-N56GM63T;Database=CustomerSupportDB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.RoleId);

            entity.ToTable("AppRoles", "auth");

            entity.HasIndex(e => e.RoleName, "UQ_AppRoles_RoleName").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK__Attachme__442C64BE0401CF2F");

            entity.ToTable("Attachments", "chat");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FileName).HasMaxLength(200);
            entity.Property(e => e.MediaId).HasMaxLength(100);
            entity.Property(e => e.MediaUrl).HasMaxLength(500);

            entity.HasOne(d => d.Message).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.MessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attachments_Messages");
        });

        modelBuilder.Entity<AuthToken>(entity =>
        {
            entity.HasKey(e => e.TokenId);

            entity.ToTable("AuthTokens", "auth");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.TokenType).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.AuthTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuthTokens_Users");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies", "crm");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__C050D877771F928B");

            entity.ToTable("Conversations", "chat");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Bot");

            entity.HasOne(d => d.AssignedAgentNavigation).WithMany(p => p.ConversationAssignedAgentNavigations)
                .HasForeignKey(d => d.AssignedAgent)
                .HasConstraintName("FK_Conversations_Agent");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.ConversationAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK_Conversations_AssignedBy");

            entity.HasOne(d => d.ClientUser).WithMany(p => p.ConversationClientUsers)
                .HasForeignKey(d => d.ClientUserId)
                .HasConstraintName("FK_Conversations_Client");

            entity.HasOne(d => d.Company).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_Conversations_Companies");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C0C9CCF00998F");

            entity.ToTable("Messages", "chat");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MessageType).HasMaxLength(20);

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Conversations");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Sender");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity
                .ToTable("Users", "auth")
                .ToTable(tb => tb.IsTemporal(ttb =>
                {
                    ttb.UseHistoryTable("UsersHistory", "auth");
                    ttb
                        .HasPeriodStart("ValidFrom")
                        .HasColumnName("ValidFrom");
                    ttb
                        .HasPeriodEnd("ValidTo")
                        .HasColumnName("ValidTo");
                }));

            entity.Property(e => e.ConcurrencyStamp).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Identifier).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SecurityStamp).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Company).WithMany(p => p.Users)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_Users_Companies");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.ToTable("UserRoles", "auth");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
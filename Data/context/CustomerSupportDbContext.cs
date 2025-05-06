using System;
using System.Collections.Generic;
using CustomerService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Data.context;

public partial class CustomerSupportDbContext : DbContext
{
    public CustomerSupportDbContext()
    {
    }

    public CustomerSupportDbContext(DbContextOptions<CustomerSupportDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppRole> AppRoles { get; set; }

    public virtual DbSet<AuthToken> AuthTokens { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-N56GM63T;Database=CustomerSupportDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AppRoleCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppRoles_CreatedBy_User");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AppRoleUpdatedByNavigations).HasConstraintName("FK_AppRoles_UpdatedBy_User");
        });

        modelBuilder.Entity<AuthToken>(entity =>
        {
            entity.HasIndex(e => e.ExpiresAt, "IX_AuthTokens_Active").HasFilter("([Revoked]=(0) AND [Used]=(0))");

            entity.Property(e => e.TokenId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AuthTokenCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuthTokens_CreatedBy_User");

            entity.HasOne(d => d.ReplacedByToken).WithMany(p => p.InverseReplacedByToken).HasConstraintName("FK_AuthTokens_ReplacedBy_Token");

            entity.HasOne(d => d.User).WithMany(p => p.AuthTokenUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuthTokens_Users");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.Property(e => e.ContactId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("UsersHistory", "auth");
                        ttb
                            .HasPeriodStart("ValidFrom")
                            .HasColumnName("ValidFrom");
                        ttb
                            .HasPeriodEnd("ValidTo")
                            .HasColumnName("ValidTo");
                    }));

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ConcurrencyStamp).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SecurityStamp).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_CreatedBy_User");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InverseUpdatedByNavigation).HasConstraintName("FK_Users_UpdatedBy_User");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.UserRoleAssignedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_AssignedBy_User");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoleUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

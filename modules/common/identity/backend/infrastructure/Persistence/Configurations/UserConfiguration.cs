using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Identity.Domain.Entities;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.Id).HasName("users_pkey");

        entity.ToTable("users", "security", t => { t.HasCheckConstraint("security_users_role_ck", "role IN ('admin', 'lecturer', 'student')"); t.HasCheckConstraint("security_users_status_ck", "status IN ('active', 'disabled')"); t.HasCheckConstraint("security_users_failed_login_ck", "failed_login_count >= 0"); });

        entity.HasIndex(e => e.NormalizedEmail, "users_normalized_email_key").IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Email).HasColumnName("email");
        entity.Property(e => e.FailedLoginCount).HasDefaultValue(0).HasColumnName("failed_login_count");
        entity.Property(e => e.FullName).HasColumnName("full_name");
        entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
        entity.Property(e => e.LockedUntil).HasColumnName("locked_until");
        entity.Property(e => e.MustChangePassword)
            .HasDefaultValue(true)
            .HasColumnName("must_change_password");
        entity.Property(e => e.NormalizedEmail).HasColumnName("normalized_email");
        entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
        entity.Property(e => e.Role).HasColumnName("role");
        entity.Property(e => e.Status)
            .HasDefaultValueSql("'active'::text")
            .HasColumnName("status");
        entity.Property(e => e.TokenVersion)
            .HasDefaultValue(1)
            .HasColumnName("token_version");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
    }
}

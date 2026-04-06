using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasMaxLength(254)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500)
            .HasColumnType("text");

        builder.Property(u => u.Bio)
            .HasMaxLength(500)
            .HasColumnType("text");

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasIndex(u => u.Username)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasMany(u => u.Identities)
            .WithOne()
            .HasForeignKey(a => a.UserId);

        builder.ToTable("users", "users", t =>
        {
            t.HasCheckConstraint("CK_users_email_not_empty", "trim(email) <> ''");
            t.HasCheckConstraint("CK_users_username_not_empty", "trim(username) <> ''");
        });
    }
}

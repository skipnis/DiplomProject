using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Admin.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class AdminUserConfig : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Username)
            .HasMaxLength(50)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.PasswordHash)
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(a => a.Username)
            .IsUnique();
    }
}

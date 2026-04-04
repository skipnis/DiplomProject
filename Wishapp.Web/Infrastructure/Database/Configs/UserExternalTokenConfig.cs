using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class UserExternalTokenConfig : IEntityTypeConfiguration<UserExternalToken>
{
    public void Configure(EntityTypeBuilder<UserExternalToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Provider)
            .HasMaxLength(50)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(t => t.Scope)
            .HasMaxLength(100)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(t => t.RefreshToken)
            .HasMaxLength(2048)
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(t => new { t.UserId, t.Provider, t.Scope }).IsUnique();
    }
}

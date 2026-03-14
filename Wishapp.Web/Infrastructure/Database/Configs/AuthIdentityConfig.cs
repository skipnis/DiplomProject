using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class AuthIdentityConfig : IEntityTypeConfiguration<AuthIdentity>
{
    public void Configure(EntityTypeBuilder<AuthIdentity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.HasIndex(a => new { a.Provider, a.ProviderKey }).IsUnique();
        
        builder.HasIndex(a => a.UserId);

        builder.Property(a => a.Provider)
            .HasConversion<string>();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.Identities)
            .WithOne()
            .HasForeignKey(a => a.UserId);
    }
}
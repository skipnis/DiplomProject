using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class BlacklistItemConfig : IEntityTypeConfiguration<BlacklistItem>
{
    public void Configure(EntityTypeBuilder<BlacklistItem> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.UserId)
            .IsRequired();

        builder.Property(item => item.Title)
            .HasMaxLength(100)
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(item => item.UserId);

        builder.ToTable("blacklist_items", "users", t =>
        {
            t.HasCheckConstraint("CK_blacklist_items_title_not_empty", "trim(title) <> ''");
        });
    }
}

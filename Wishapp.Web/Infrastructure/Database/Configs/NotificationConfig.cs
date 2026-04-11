using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .HasConversion<int>();

        builder.Property(n => n.Status)
            .HasConversion<int>();

        builder.Property(n => n.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
        builder.HasIndex(n => new { n.Status, n.CreatedAt });

        builder.ToTable("notifications", "notifications");
    }
}

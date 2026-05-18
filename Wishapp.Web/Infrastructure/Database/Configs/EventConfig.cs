using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Events.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class EventConfig : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000)
            .HasColumnType("text");

        builder.HasIndex(e => e.OwnerId);

        builder.HasIndex(e => new { e.OwnerId, e.Date });

        builder.ToTable("events", "events", t => t.HasCheckConstraint("CK_events_title_not_empty", "trim(title) <> ''"));
    }
}

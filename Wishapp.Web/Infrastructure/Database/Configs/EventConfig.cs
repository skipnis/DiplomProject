using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Events.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class EventConfig : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.OwnerId);
        
        builder.HasIndex(e => new { e.OwnerId, e.Date });
    }
}

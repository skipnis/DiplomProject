using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class GiftProposalConfig : IEntityTypeConfiguration<GiftProposal>
{
    public void Configure(EntityTypeBuilder<GiftProposal> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CustomTitle).HasMaxLength(200);
        builder.Property(p => p.CustomDescription).HasMaxLength(2000);
        builder.Property(p => p.HintMessage).HasMaxLength(500);
        builder.Property(p => p.SenderAlias).HasMaxLength(100);
        builder.Property(p => p.RecipientComment).HasMaxLength(500);
        builder.Property(p => p.CustomImagePath).HasMaxLength(500);

        builder.HasIndex(p => new { p.RecipientId, p.IsViewedByRecipient, p.CreatedAt })
            .IsDescending(false, false, true);
        builder.HasIndex(p => new { p.SenderId, p.CreatedAt });

        builder.ToTable("gift_proposals", "proposals", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("ck_gift_proposals_status", "status IN (0, 1, 2)");
            tableBuilder.HasCheckConstraint("ck_gift_proposals_source_type", "source_type IN (1, 2, 3)");
        });
    }
}

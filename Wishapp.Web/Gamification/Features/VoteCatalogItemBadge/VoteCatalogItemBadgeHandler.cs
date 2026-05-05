using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Gamification.Features.VoteCatalogItemBadge;

public sealed class VoteCatalogItemBadgeHandler(ApplicationDbContext db, ICatalogApi catalogApi)
    : ICommandHandler<VoteCatalogItemBadgeCommand>
{
    public async Task<Result> HandleAsync(VoteCatalogItemBadgeCommand command, CancellationToken ct = default)
    {
        var exists = await catalogApi.ItemExistsAsync(command.CatalogItemId, ct);
        if (!exists)
            return Error.NotFound("Catalog.NotFound", "Catalog item not found.");

        var badgeExists = await db.CatalogBadgeDefinitions
            .AnyAsync(b => b.Id == command.BadgeType && b.IsActive, ct);
        if (!badgeExists)
            return Error.NotFound("Catalog.BadgeNotFound", "Badge definition not found.");

        var alreadyVoted = await db.CatalogItemBadgeVotes
            .AnyAsync(v => v.CatalogItemId == command.CatalogItemId
                           && v.UserId == command.UserId
                           && v.BadgeType == command.BadgeType, ct);
        if (alreadyVoted)
            return Result.Success();

        db.CatalogItemBadgeVotes.Add(
            CatalogItemBadgeVote.Create(command.CatalogItemId, command.UserId, command.BadgeType));
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

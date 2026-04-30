using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Gamification.Features.UnvoteCatalogItemBadge;

public sealed class UnvoteCatalogItemBadgeHandler(ApplicationDbContext db)
    : ICommandHandler<UnvoteCatalogItemBadgeCommand>
{
    public async Task<Result> HandleAsync(UnvoteCatalogItemBadgeCommand command, CancellationToken ct = default)
    {
        var vote = await db.CatalogItemBadgeVotes
            .FirstOrDefaultAsync(v => v.CatalogItemId == command.CatalogItemId
                                      && v.UserId == command.UserId
                                      && v.BadgeType == command.BadgeType, ct);
        if (vote is not null)
        {
            db.CatalogItemBadgeVotes.Remove(vote);
            await db.SaveChangesAsync(ct);
        }
        return Result.Success();
    }
}

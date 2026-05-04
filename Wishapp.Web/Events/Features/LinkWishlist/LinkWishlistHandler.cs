using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Events.Features.LinkWishlist;

public sealed class LinkWishlistHandler(ApplicationDbContext db, IWishlistsApi wishlistsApi)
    : ICommandHandler<LinkWishlistCommand>
{
    public async Task<Result> HandleAsync(
        LinkWishlistCommand command,
        CancellationToken ct = default)
    {
        var @event = await db.Events
            .FirstOrDefaultAsync(e => e.Id == command.EventId, ct);

        if (@event is null)
            return Error.NotFound("Events.NotFound", "Event not found");

        if (@event.OwnerId != command.UserId)
            return Error.Forbidden("Events.Forbidden", "Access denied");

        if (command.WishlistId.HasValue)
        {
            var canLink = await wishlistsApi.CanLinkWishlistAsync(command.UserId, command.WishlistId.Value, ct);
            if (canLink.IsFailure)
                return canLink.Error;

            @event.LinkWishlist(command.WishlistId.Value);
        }
        else
        {
            @event.UnlinkWishlist();
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

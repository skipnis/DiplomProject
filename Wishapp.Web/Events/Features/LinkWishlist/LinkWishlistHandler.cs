using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Events.Features.LinkWishlist;

public sealed class LinkWishlistHandler(ApplicationDbContext db)
    : ICommandHandler<LinkWishlistCommand>
{
    public async Task<Result> HandleAsync(
        LinkWishlistCommand command,
        CancellationToken ct = default)
    {
        var @event = await db.Events
            .FirstOrDefaultAsync(e => e.Id == command.EventId, ct);

        if (@event is null)
        {
            return Error.NotFound("Events.NotFound", "Event not found");
        }

        if (@event.OwnerId != command.UserId)
        {
            return Error.Forbidden("Events.Forbidden", "Access denied");
        }

        @event.LinkedWishlistId = command.WishlistId;
        
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

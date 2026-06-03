using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Dtos;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Events.Features.GetEventByWishlist;

public sealed class GetEventByWishlistHandler(ApplicationDbContext db)
    : IQueryHandler<GetEventByWishlistQuery, EventDto>
{
    public async Task<Result<EventDto>> HandleAsync(
        GetEventByWishlistQuery query,
        CancellationToken ct = default)
    {
        var linkedEvent = await db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.LinkedWishlistId == query.WishlistId && e.OwnerId == query.UserId,
                ct);

        if (linkedEvent is null)
            return Error.NotFound("Events.NotFound", "No event linked to this wishlist");

        return new EventDto(
            linkedEvent.Id,
            linkedEvent.Title,
            linkedEvent.Description,
            linkedEvent.Date,
            linkedEvent.LinkedWishlistId,
            linkedEvent.CreatedAt);
    }
}

using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Occasions.Update;

public sealed class UpdateOccasionHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<UpdateOccasionCommand>
{
    public async Task<Result> HandleAsync(
        UpdateOccasionCommand command,
        CancellationToken ct = default)
    {
        var occasion = await db.CatalogOccasions
            .FirstOrDefaultAsync(o => o.Id == command.Id, ct);

        if (occasion is null)
        {
            return Error.NotFound("Catalog.OccasionNotFound", "Occasion not found");
        }

        var keyTaken = await db.CatalogOccasions
            .AnyAsync(o => o.Key == command.Key && o.Id != command.Id, ct);

        if (keyTaken)
        {
            return Error.Conflict("Catalog.OccasionExists", "Occasion with this key already exists");
        }

        occasion.Update(command.Key, command.Label, command.Order);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:occasions", token: ct);

        return Result.Success();
    }
}

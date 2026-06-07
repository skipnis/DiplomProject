using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Occasions.Create;

public sealed class CreateOccasionHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<CreateOccasionCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateOccasionCommand command,
        CancellationToken ct = default)
    {
        var exists = await db.CatalogOccasions
            .AnyAsync(o => o.Key == command.Key, ct);

        if (exists)
        {
            return Error.Conflict("Catalog.OccasionExists", "Occasion with this key already exists");
        }

        var nextOrder = await db.CatalogOccasions.AnyAsync(ct)
            ? await db.CatalogOccasions.MaxAsync(o => o.Order, ct) + 1
            : 1;

        var occasion = CatalogOccasion.Create(command.Key, command.Label, nextOrder);

        db.CatalogOccasions.Add(occasion);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:occasions", token: ct);

        return occasion.Id;
    }
}

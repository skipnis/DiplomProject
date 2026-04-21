using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Occasions.Delete;

public sealed class DeleteOccasionHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<DeleteOccasionCommand>
{
    public async Task<Result> HandleAsync(
        DeleteOccasionCommand command,
        CancellationToken ct = default)
    {
        var occasion = await db.CatalogOccasions
            .FirstOrDefaultAsync(o => o.Id == command.Id, ct);

        if (occasion is null)
        {
            return Error.NotFound("Catalog.OccasionNotFound", "Occasion not found");
        }

        db.CatalogOccasions.Remove(occasion);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:occasions", token: ct);

        return Result.Success();
    }
}

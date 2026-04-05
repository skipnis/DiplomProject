using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Features.RateCatalogItem;

public sealed class RateCatalogItemHandler(ApplicationDbContext db)
    : ICommandHandler<RateCatalogItemCommand>
{
    private static readonly Error NotFound =
        Error.NotFound("Catalog.NotFound", "Catalog item not found.");

    public async Task<Result> HandleAsync(RateCatalogItemCommand command, CancellationToken ct = default)
    {
        var exists = await db.CatalogItems
            .AnyAsync(i => i.Id == command.CatalogItemId && i.IsPublished, ct);

        if (!exists)
            return NotFound;

        var existing = await db.CatalogItemRatings
            .FirstOrDefaultAsync(r => r.UserId == command.UserId && r.CatalogItemId == command.CatalogItemId, ct);

        if (existing is not null)
        {
            existing.Update(command.Value);
        }
        else
        {
            db.CatalogItemRatings.Add(CatalogItemRating.Create(command.UserId, command.CatalogItemId, command.Value));
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

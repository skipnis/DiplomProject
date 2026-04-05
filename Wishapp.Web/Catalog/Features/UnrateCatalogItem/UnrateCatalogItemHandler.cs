using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Features.UnrateCatalogItem;

public sealed class UnrateCatalogItemHandler(ApplicationDbContext db)
    : ICommandHandler<UnrateCatalogItemCommand>
{
    public async Task<Result> HandleAsync(UnrateCatalogItemCommand command, CancellationToken ct = default)
    {
        var rating = await db.CatalogItemRatings
            .FirstOrDefaultAsync(r => r.UserId == command.UserId && r.CatalogItemId == command.CatalogItemId, ct);

        if (rating is not null)
        {
            db.CatalogItemRatings.Remove(rating);
            await db.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}

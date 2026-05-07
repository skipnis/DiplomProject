using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Items.Create;

public sealed class CreateCatalogItemHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<CreateCatalogItemCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateCatalogItemCommand command,
        CancellationToken ct = default)
    {
        var categoryExists = await db.CatalogCategories
            .AnyAsync(c => c.Id == command.CategoryId, ct);

        if (!categoryExists)
            return Error.NotFound("Catalog.CategoryNotFound", "Category not found");

        if (command.OccasionIds.Count > 0)
        {
            var validOccasionCount = await db.CatalogOccasions
                .CountAsync(o => command.OccasionIds.Contains(o.Id), ct);
            if (validOccasionCount != command.OccasionIds.Count)
                return Error.NotFound("Catalog.OccasionNotFound", "One or more occasions not found");
        }

        var item = CatalogItem.Create(
            command.Name,
            command.Description,
            command.Price,
            command.Currency,
            command.ImagePath,
            command.Url,
            command.CategoryId);

        item.SetOccasions(command.OccasionIds);
        db.CatalogItems.Add(item);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:price-range", token: ct);

        return item.Id;
    }
}

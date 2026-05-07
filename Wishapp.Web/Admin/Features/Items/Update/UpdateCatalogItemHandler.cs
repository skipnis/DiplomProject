using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Items.Update;

public sealed class UpdateCatalogItemHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<UpdateCatalogItemCommand>
{
    public async Task<Result> HandleAsync(
        UpdateCatalogItemCommand command,
        CancellationToken ct = default)
    {
        var item = await db.CatalogItems
            .Include(i => i.Occasions)
            .FirstOrDefaultAsync(i => i.Id == command.Id, ct);

        if (item is null)
            return Error.NotFound("Catalog.ItemNotFound", "Catalog item not found");

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

        item.Update(
            command.Name,
            command.Description,
            command.Price,
            command.Currency,
            command.ImagePath,
            command.Url,
            command.CategoryId,
            command.IsPublished);

        item.SetOccasions(command.OccasionIds);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:price-range", token: ct);

        return Result.Success();
    }
}

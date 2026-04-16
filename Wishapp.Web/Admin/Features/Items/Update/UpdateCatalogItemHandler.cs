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
            .FirstOrDefaultAsync(i => i.Id == command.Id, ct);

        if (item is null)
        {
            return Error.NotFound("Catalog.ItemNotFound", "Catalog item not found");
        }

        var categoryExists = await db.CatalogCategories
            .AnyAsync(c => c.Id == command.CategoryId, ct);

        if (!categoryExists)
        {
            return Error.NotFound("Catalog.CategoryNotFound", "Category not found");
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

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:price-range", token: ct);

        return Result.Success();
    }
}
